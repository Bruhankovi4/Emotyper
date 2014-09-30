using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
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

        public delegate void DictionaryFired(object sender, EmoEventDictionary e);
        public event DictionaryFired OnDataArrived;

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
                    Console.WriteLine(_userId);
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
                    Thread.Sleep(300);
                }

            }
            catch (ThreadInterruptedException exception)
            {
                Console.WriteLine(exception.Message);
            }
        }
    }
}