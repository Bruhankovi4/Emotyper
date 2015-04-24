using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using Configuration;
using CsvReader;
using Emotiv;


namespace EmotyperDataUtility
{
    public class EmoEventRows : EventArgs
    {
        public List<List<double>> Rows { get; set; }
    }

    public class EmoEventDictionary : EventArgs
    {
        public Dictionary<EdkDll.EE_DataChannel_t, double[]> Dictionary { get; set; }
    }

    public class EmotypeEventSource
    {
        private readonly EmoEngine _engine; // Access to the EDK is viaa the EmoEngine 
        private int _userId=-1;
        private Thread _thread;
        public bool fireDictionary = true;

        public delegate void RowsFired(object sender, EmoEventRows e);
        public event RowsFired OnRowsArrived;

        public delegate void EmulationFinished(object sender);
        public event EmulationFinished OnEmulationFinish;

        public delegate void DictionaryFired(object sender, EmoEventDictionary e);
        public event DictionaryFired OnDataArrived;
        public string emulationDirectory;


        protected void OnEmulationFinished()
        {
            EmulationFinished handler = OnEmulationFinish;
            if (handler != null) 
                handler(this);
        }
        protected void OnRowsRead(List<List<double>> reading)
        {
            RowsFired handler = OnRowsArrived;
            if (handler != null) 
                handler(this, new EmoEventRows {Rows=reading});
        }

        protected void OnDataRead(Dictionary<EdkDll.EE_DataChannel_t, double[]> _data)
        {
            DictionaryFired handler = OnDataArrived;
            if (handler != null)
                handler(this, new EmoEventDictionary { Dictionary = _data });
        }
        public EmotypeEventSource()
        {
            _engine = EmoEngine.Instance;
            _engine.UserAdded += OnUserAdded;

            // connect to Emoengine.            
            _engine.Connect();

        }

        void OnUserAdded(object sender, EmoEngineEventArgs e)
        {
            Console.WriteLine("User Added Event has occured");
            // record the user 
            _userId = (int)e.userId;
            // enable data aquisition for this user.
            _engine.DataAcquisitionEnable((uint)_userId, true);
            // ask for up to 1 second of buffered data
            _engine.EE_DataSetBufferSizeInSec(1);
        }

        public void Start()
        {
            if(_thread != null)
                return;
            _thread = new Thread(StartListening);
            _thread.Start();
        }

        public void StartEmulation(string filepath)
        {
            if (_thread != null)
                return;
            emulationDirectory = filepath;
            _thread = new Thread(StartEmulating);
            _thread.Start();
        }
        public void Stop()
        {
            if (_thread == null) 
                return;

            _thread.Interrupt();
            _thread = null;
        }

        private void StartListening()
        {
            try
            {
                while (_thread != null)
                {
                    // Handle any waiting events
                    _engine.ProcessEvents();
                   // Console.WriteLine(_userId);
                    // If the user has not yet connected, do not proceed
                    if ((int)_userId == -1)
                    {
                        continue;
                    }

                    Dictionary<EdkDll.EE_DataChannel_t, double[]> data = _engine.GetData((uint)_userId);
                    if (data == null)
                    {
                        continue;
                    }
                    if (fireDictionary)
                    {
                        OnDataArrived(this, new EmoEventDictionary { Dictionary = data });
                    }
                    else
                    {
                        int bufferSize = data[EdkDll.EE_DataChannel_t.TIMESTAMP].Length;
                        List<List<double>> rows = new List<List<double>>();
                        for (int i = 0; i < bufferSize; i++)
                        {
                            List<double> row = new List<double>(data.Keys.Select(channel => data[channel][i]));
                            rows.Add(row);
                        }
                        OnRowsArrived(this, new EmoEventRows { Rows = rows });
                    }
                    Thread.Sleep(100);
                }

            }
            catch (ThreadInterruptedException exception)
            {
                Console.WriteLine(exception.Message);
            }
        }
        private void StartEmulating()
        {
            
            try
            {
                    Dictionary<EdkDll.EE_DataChannel_t, List<double>> data = new Dictionary<EdkDll.EE_DataChannel_t, List<double>>();
                for (EdkDll.EE_DataChannel_t sensor = EdkDll.EE_DataChannel_t.AF3;
                     sensor <= EdkDll.EE_DataChannel_t.AF4;
                     sensor++)
                {
                    data.Add(sensor,new List<double>());
                }
                foreach (String file in Directory.GetFiles(emulationDirectory))
                {
                    for (EdkDll.EE_DataChannel_t sensor = EdkDll.EE_DataChannel_t.AF3;
                         sensor <= EdkDll.EE_DataChannel_t.AF4;
                          sensor++)
                    {
                        DataTable table = CSVReader.ReadCSVFile(file.Replace("\\", "//"), true);
                        List<double> serie = new List<double>();
                        for (int j = 0; j < table.Rows.Count; j++)
                        {
                            DataRow row = table.Rows[j];
                            double val;
                            Double.TryParse(row[(int)sensor].ToString(), out val);
                            serie.Add(val);
                        }
                        data[sensor].AddRange(serie);
                    }
                    
                }
                while (_thread != null)
                {
                           Dictionary<EdkDll.EE_DataChannel_t, double[]> fireData = new Dictionary<EdkDll.EE_DataChannel_t, double[]>();
                           for (EdkDll.EE_DataChannel_t sensor = EdkDll.EE_DataChannel_t.AF3;
                         sensor <= EdkDll.EE_DataChannel_t.AF4;
                          sensor++)
                           {
                               if (data[sensor].Count < 40)
                               {
                                   OnEmulationFinished();
                                   Thread.Sleep(100);
                                   Stop();
                                   break;
                               }
                               fireData.Add(sensor, data[sensor].GetRange(0, Config.EmulatedSamplesCount).ToArray());
                               data[sensor].RemoveRange(0, Config.EmulatedSamplesCount);
                           }
                                    
                    if (fireDictionary)
                    {
                        OnDataArrived(this, new EmoEventDictionary { Dictionary = fireData });
                    }

                    Thread.Sleep(Config.EmulationPeriod);
                }

            }
            catch (ThreadInterruptedException exception)
            {
                Console.WriteLine(exception.Message);
            }
        }
       
    }
}