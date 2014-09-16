using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Emotiv;

namespace EmotyperDataUtility
{
    public class EmoEventArgs : EventArgs
    {
        public List<List<double>> Rows { get; set; }
    }

    public class EmotypeEventSource
    {
        private readonly EmoEngine _engine; // Access to the EDK is viaa the EmoEngine 
        private int _userId;
        private long _isRunning = 1;
        private Thread _thread;

        public delegate void EmoFired(object sender, EmoEventArgs e);

        public event EmoFired OnDataArrived;

        protected virtual void OnDataRead(List<List<double>> reading)
        {
            EmoFired handler = OnDataArrived;
            if (handler != null) 
                handler(this, new EmoEventArgs {Rows=reading});
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
            if(_thread != null)
                _thread.Join();
        }

        private void StartListening()
        {
            while (_isRunning != 0)
            {
                // Handle any waiting events
                _engine.ProcessEvents();
                // If the user has not yet connected, do not proceed
                if ((int)_userId == -1)
                    return;

                Dictionary<EdkDll.EE_DataChannel_t, double[]> data = _engine.GetData((uint)_userId);
                if (data == null)
                    return;

                int bufferSize = data[EdkDll.EE_DataChannel_t.TIMESTAMP].Length;
                List<List<double>> rows = new List<List<double>>();
                for (int i = 0; i < bufferSize; i++)
                {
                    List<double> row = new List<double>(data.Keys.Select(channel => data[channel][i]));
                    rows.Add(row);
                }
                OnDataArrived(this, new EmoEventArgs {Rows = rows});
            }
        }
    }

    public class FileStorageWriter
    {

        private string _filestorageRoot;
        private EmoEngine _engine; // Access to the EDK is viaa the EmoEngine 
        int userID = -1; // userID is used to uniquely identify a user's headset
        string _filename = "outfile.csv"; // output filename
        private Thread _writingThread;
        private TextWriter _file;
        private Dictionary<String, List<double>> _rawData;
            
        public FileStorageWriter(string filestorageRootFolder = "C://")
        {
            // create the engine
            Init(filestorageRootFolder);
            InitDatastorage();
        }

        private void InitDatastorage()
        {
            _rawData = new Dictionary<string, List<double>>(){
                {"COUNTER", new List<double>()},
                {"INTERPOLATED", new List<double>()},
                {"RAW_CQ", new List<double>()},
                {"AF3", new List<double>()},
                {"F7", new List<double>()},
                {"F3", new List<double>()},
                {"FC5", new List<double>()},
                {"T7", new List<double>()},
                {"P7", new List<double>()},
                {"O1", new List<double>()},
                {"O2", new List<double>()},
                {"P8", new List<double>()},
                {"T8", new List<double>()},
                {"FC6", new List<double>()},
                {"F4", new List<double>()},
                {"F8", new List<double>()},
                {"AF4", new List<double>()},
                {"GYROX", new List<double>()},
                {"GYROY", new List<double>()},
                {"TIMESTAMP", new List<double>()},
                {"ES_TIMESTAMP", new List<double>()},
                {"FUNC_ID", new List<double>()},
                {"FUNC_VALUE", new List<double>()},
                {"MARKER", new List<double>()},
                {"SYNC_SIGNAL", new List<double>()}
            };
        }

        private void Init(string filestorageRootFolder)
        {
            _engine = EmoEngine.Instance;
            _engine.UserAdded += OnUserAdded;

            // connect to Emoengine.            
            _engine.Connect();

            // create a header for our output file            
            _filestorageRoot = filestorageRootFolder;
            _writingThread = new Thread(StartRecording);
        }

        void OnUserAdded(object sender, EmoEngineEventArgs e)
        {
            Console.WriteLine("User Added Event has occured");

            // record the user 
            userID = (int)e.userId;

            // enable data aquisition for this user.
            _engine.DataAcquisitionEnable((uint)userID, true);

            // ask for up to 1 second of buffered data
            _engine.EE_DataSetBufferSizeInSec(1);
        }
      
        void Run()
        {
            // Handle any waiting events
            _engine.ProcessEvents();
            // If the user has not yet connected, do not proceed
            if ((int)userID == -1)
                return;

            Dictionary<EdkDll.EE_DataChannel_t, double[]> data = _engine.GetData((uint)userID);
            if (data == null)
            {
                return;
            }
            int _bufferSize = data[EdkDll.EE_DataChannel_t.TIMESTAMP].Length;
            Log("Writing {0} lines of data ", _bufferSize);
            for (int i = 0; i < _bufferSize; i++)           
                foreach (EdkDll.EE_DataChannel_t channel in data.Keys)                
                    _rawData[channel.ToString()].Add(data[channel][i]);                           
        }

        private void Log(String format, params Object [] parameters)
        {
            Console.WriteLine(format, parameters);
        }

        public void StartWritingToFile(string _folderName, string _fileName)
        {
            _filename = _filestorageRoot + "\\" + _folderName + "\\";
            if (!Directory.Exists(_filename))
                Directory.CreateDirectory(_filename);
            _filename += _fileName + ".csv";
            InitDatastorage();
            _writingThread = new Thread(StartRecording);
            _writingThread.Start();
        }

        private void StartRecording()
        {            
            while (true)
            {
                Run();             
            }
        }

        public void StopWriting()
        {
            _writingThread.Abort();
            _file = new StreamWriter(_filename, true);
            string header = String.Join(";", _rawData.Keys.ToArray());
            _file.WriteLine(header);
            int size = _rawData["TIMESTAMP"].Count;   //can be any of the sensors
            try
            {
                for (int i = 0; i < size; i++)
                {
                    // now write the data
                    foreach (String channel in _rawData.Keys)
                    {
                        _file.Write(_rawData[channel][i] + ";");
                    }
                    _file.WriteLine("");
                }
            }
            finally
            {
                _file.Close();
            }
        }
    }
}
