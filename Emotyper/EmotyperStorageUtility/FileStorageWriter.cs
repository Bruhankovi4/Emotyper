using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Emotiv;

namespace EmotyperStorageUtility
{
    public class FileStorageWriter
    {
        private string filestorageRoot;
        EmoEngine engine; // Access to the EDK is viaa the EmoEngine 
        int userID = -1; // userID is used to uniquely identify a user's headset
        string filename = "outfile.csv"; // output filename
        private Thread writingThread;
        private TextWriter file;
        private Dictionary<String, List<double>> rawData = new Dictionary<string, List<double>>();
            
        public FileStorageWriter(string filestorageRootFolder = "C://")
        {
            // create the engine
            init(filestorageRootFolder);
            initDatastorage();
        }

        private void initDatastorage()
        {
            rawData = new Dictionary<string, List<double>>(){
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

        private void init(string filestorageRootFolder)
        {
            engine = EmoEngine.Instance;
            engine.UserAdded += new EmoEngine.UserAddedEventHandler(engine_UserAdded_Event);

            // connect to Emoengine.            
            engine.Connect();

            // create a header for our output file            
            filestorageRoot = filestorageRootFolder;
            writingThread = new Thread(startRecording);
        }

        void engine_UserAdded_Event(object sender, EmoEngineEventArgs e)
        {
            Console.WriteLine("User Added Event has occured");

            // record the user 
            userID = (int)e.userId;

            // enable data aquisition for this user.
            engine.DataAcquisitionEnable((uint)userID, true);

            // ask for up to 1 second of buffered data
            //engine.EE_DataSetBufferSizeInSec(0.3f);

        }
      
        void Run()
        {
            // Handle any waiting events
            engine.ProcessEvents();
            // If the user has not yet connected, do not proceed
            if ((int)userID == -1)
                return;
            Dictionary<EdkDll.EE_DataChannel_t, double[]> data = engine.GetData((uint)userID);
            if (data == null)
            {
                return;
            }
            int _bufferSize = data[EdkDll.EE_DataChannel_t.TIMESTAMP].Length;
            Console.WriteLine("Writing " + _bufferSize.ToString() + " lines of data ");
            for (int i = 0; i < _bufferSize; i++)           
                foreach (EdkDll.EE_DataChannel_t channel in data.Keys)                
                    rawData[channel.ToString()].Add(data[channel][i]);                           
        }

        public void StartWritingToFile(string _folderName, string _fileName)
        {
            filename = filestorageRoot + "\\" + _folderName + "\\";
            if (!Directory.Exists(filename))
                Directory.CreateDirectory(filename);
            filename += _fileName + ".csv";
            initDatastorage();
            writingThread = new Thread(startRecording);
            writingThread.Start();
        }

        private void startRecording()
        {            
            while (true)
            {
                Run();             
            }
        }

        public void StopWriting()
        {
            writingThread.Abort();
            file = new StreamWriter(filename, true);
            string header = "COUNTER;INTERPOLATED;RAW_CQ;AF3;F7;F3; FC5; T7; P7; O1; O2;P8;T8; FC6; F4;F8; AF4;GYROX; GYROY; TIMESTAMP; ES_TIMESTAMP;FUNC_ID; FUNC_VALUE; MARKER; SYNC_SIGNAL;";            
            file.WriteLine(header);
            int size = rawData["TIMESTAMP"].Count;   //can be any of the sensors
            try
            {
                for (int i = 0; i < size; i++)
                {
                    // now write the data
                    foreach (String channel in rawData.Keys)
                    {
                        file.Write(rawData[channel][i] + ";");
                    }
                    file.WriteLine("");
                }
                file.Close();
            }
            catch (Exception)
            {

                file.Close();
            }
           
            
        }
    }
}
