using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Calculations;
using Emotiv;
using EmotyperDataUtility;
using SOM;
using SOM_Visualization;
using ZedGraphSample;

namespace Classifier
{
    class Program
    {

        private static string gridFolder = @"D:\GitRepos\Emotyper\Emotyper\SOMInstances";
        private static string coordsFolder = @"D:\GitRepos\Emotyper\Emotyper\SOMCoordinates";
        private static string classifierFolder = @"D:\GitRepos\Emotyper\Emotyper\SOMClassifiers\Classifier.cls";
        private static Dictionary<EdkDll.EE_DataChannel_t, SOM.Neuron[,]> maps = new Dictionary<EdkDll.EE_DataChannel_t, Neuron[,]>();
        private static Dictionary<EdkDll.EE_DataChannel_t, Grid> grids = new Dictionary<EdkDll.EE_DataChannel_t, Grid>();
        private static Dictionary<EdkDll.EE_DataChannel_t, LinkedList<double>> windows = new Dictionary<EdkDll.EE_DataChannel_t, LinkedList<double>>();
        private static int dimensions = 128;
        private static Neuron[,] outputs;
        private static bool load = false;
        private static bool useRows = true;

        void OnDataArrived(object sender, EmoEventDictionary e)
        {

            Dictionary<EdkDll.EE_DataChannel_t, double[]> data = e.Dictionary;
            if (data == null)
                return;
            int _bufferSize = data[EdkDll.EE_DataChannel_t.TIMESTAMP].Length;
            if (_bufferSize < 128)
                return;
            int g1 = 0;
            int g2 = 0;
            int g3 = 0;
            int g4 = 0;
            List<EdkDll.EE_DataChannel_t> exclude = new List<EdkDll.EE_DataChannel_t>();
           // exclude.Add(EdkDll.EE_DataChannel_t.O1);
           // exclude.Add(EdkDll.EE_DataChannel_t.O2);
           // exclude.Add(EdkDll.EE_DataChannel_t.P7);
           // exclude.Add(EdkDll.EE_DataChannel_t.P8);
           // exclude.Add(EdkDll.EE_DataChannel_t.T7);
           // exclude.Add(EdkDll.EE_DataChannel_t.T8);
           // exclude.Add(EdkDll.EE_DataChannel_t.FC5);
           // exclude.Add(EdkDll.EE_DataChannel_t.FC6);
           //// exclude.Add(EdkDll.EE_DataChannel_t.AF4);
           // //exclude.Add(EdkDll.EE_DataChannel_t.AF3);
           // //exclude.Add(EdkDll.EE_DataChannel_t.F4);
           // exclude.Add(EdkDll.EE_DataChannel_t.F8);
           // //exclude.Add(EdkDll.EE_DataChannel_t.F3);
           // exclude.Add(EdkDll.EE_DataChannel_t.F7);
            // for (int i = _bufferSize-128; i < _bufferSize; i++)
            for (EdkDll.EE_DataChannel_t sensor = EdkDll.EE_DataChannel_t.AF3;
                  sensor <= EdkDll.EE_DataChannel_t.AF4;
                  sensor++)
            {

                       if (exclude.Contains(sensor))
                           continue;
                double res = Classify(data[sensor], sensor);
                Console.WriteLine(sensor+" "+ getLetter(res)+"| ");
                if (res == 0)
                    g1++;
                else if (res == 2)
                    g2++;
                else if (res == 3)
                    g3++;
                else if (res == 4)
                    g4++;
            }
            Console.WriteLine("========================================");
            Console.WriteLine();
            int result = Math.Max(Math.Max(g1, g2), Math.Max(g3, g4));
            string reslt = getLetter(result);
           // Console.WriteLine("Classification result: " + reslt);
            Console.WriteLine();
        }

        private string getLetter(double digit)
        {
            string reslt = "";
            if (1 == digit)
                reslt = "A";
            else if (2 == digit)
                reslt = "B";
            else if (3 == digit)
                reslt = "C";
            else if (4== digit)
                reslt = "neutral";
            return reslt;
        }

        private static WinnerDistance getRangeWinner(int length, int start, int end, SOM.Neuron[,] outputs, double[] pattern)
        {
            SOM.Neuron winner = null;
            double min = double.MaxValue;
            for (int i = start; i < end; i++)
                for (int j = 0; j < length; j++)
                {
                    // double d = Distance(pattern, outputs[i, j].Weights);
                    double d = Calculator.DistancePearson(pattern, outputs[i, j].Weights);
                    if (d < min)
                    {
                        min = d;
                        winner = outputs[i, j];
                    }
                }
            return new WinnerDistance(min, winner);
        }
        private static SOM.Neuron WinnerParallel(double[] pattern)
        {
            SOM.Neuron winner = null;
            double min = double.MaxValue;
            int length = 100;
            int quater = length / 4;

            var task1 = Task.Factory.StartNew(() => getRangeWinner(length, 0, quater, outputs, pattern));
            var task2 = Task.Factory.StartNew(() => getRangeWinner(length, quater, quater * 2, outputs, pattern));
            var task3 = Task.Factory.StartNew(() => getRangeWinner(length, quater * 2, quater * 3, outputs, pattern));
            var task4 = Task.Factory.StartNew(() => getRangeWinner(length, quater * 3, length, outputs, pattern));
            Task.WaitAll(task1, task2, task3, task4);
            WinnerDistance res1 = task1.Result;
            WinnerDistance res2 = task2.Result;
            WinnerDistance res3 = task3.Result;
            WinnerDistance res4 = task4.Result;
            double curmin = Math.Min(Math.Min(res1.distance, res2.distance), Math.Min(res3.distance, res4.distance));
            if (curmin < min)
                if (res1.distance == curmin)
                    winner = res1.winner;
                else if (res2.distance == curmin)
                    winner = res1.winner;
                else if (res3.distance == curmin)
                    winner = res3.winner;
                else
                    winner = res4.winner;

            return winner;
        }
        static double Classify(double[] series, EdkDll.EE_DataChannel_t sensor)
        {
            outputs = maps[sensor];
            SOM.Neuron n = WinnerParallel(series);
            return grids[sensor].values[n.X, n.Y];
        }
        private static void buildGrids()
        {
            for (EdkDll.EE_DataChannel_t sensor = EdkDll.EE_DataChannel_t.AF3;
                 sensor <= EdkDll.EE_DataChannel_t.AF4;
                 sensor++)
            {
                string SensorName = Enum.GetName(typeof(EdkDll.EE_DataChannel_t), sensor);
                string coordsPath = Path.Combine(coordsFolder, String.Format("{0}Coordinates.csv", SensorName));
                grids.Add(sensor, GridBuilder.build(coordsPath));
                Console.WriteLine("Grid" + sensor + " built");
            }
        }
        private static void initMaps()
        {
            for (EdkDll.EE_DataChannel_t sensor = EdkDll.EE_DataChannel_t.AF3;
                    sensor <= EdkDll.EE_DataChannel_t.AF4;
                    sensor++)
            {
                string SensorName = Enum.GetName(typeof(EdkDll.EE_DataChannel_t), sensor);
                string mapPath = Path.Combine(gridFolder, String.Format("{0}Map.som", SensorName));
                IFormatter formatter = new BinaryFormatter();
                if (File.Exists(mapPath))
                {
                    Stream stream = new FileStream(mapPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    maps.Add(sensor, (SOM.Neuron[,])formatter.Deserialize(stream));
                    stream.Close();
                }
            }
        }

        private static void SaveClassifier()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(classifierFolder));     // if directory does not exist
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(classifierFolder, FileMode.Create, FileAccess.Write, FileShare.None);
            formatter.Serialize(stream, grids);
        }

        private static void LoadClassifier()
        {
            IFormatter formatter = new BinaryFormatter();
            if (File.Exists(classifierFolder))
            {
                Stream stream = new FileStream(classifierFolder, FileMode.Open, FileAccess.Read, FileShare.Read);
                grids = (Dictionary<EdkDll.EE_DataChannel_t, Grid>)formatter.Deserialize(stream);
                stream.Close();
            }
        }

        static void Main(string[] args)
        {
            Program p = new Program();

            initMaps();
            if (load)
                LoadClassifier();
            else
            {
                buildGrids();
                SaveClassifier();
            }

            EmotypeEventSource source = new EmotypeEventSource();
            source.OnDataArrived += p.OnDataArrived;
            source.Start();
        }

       
    }
}
