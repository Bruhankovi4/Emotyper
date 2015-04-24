using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Calculations;
using Configuration;
using Emotiv;
using EmotyperDataUtility;
using SOM;
using SOM_Visualization;
using ZedGraphSample;

namespace Classifier
{
    class Classifier
    {

        private static string gridFolder = @"D:\GitRepos\Emotyper\Emotyper\SOMInstancesMFCC";
        private static string coordsFolder = @"D:\GitRepos\Emotyper\Emotyper\SOMCoordinates";
        private static string classifierFolder = @"D:\GitRepos\Emotyper\Emotyper\SOMClassifiers\Classifier.cls";
        private static Dictionary<EdkDll.EE_DataChannel_t, SOM.Neuron[,]> maps = new Dictionary<EdkDll.EE_DataChannel_t, Neuron[,]>();
        private static Dictionary<EdkDll.EE_DataChannel_t, Grid> grids = new Dictionary<EdkDll.EE_DataChannel_t, Grid>();
        private static Dictionary<EdkDll.EE_DataChannel_t, LinkedList<double>> windows = new Dictionary<EdkDll.EE_DataChannel_t, LinkedList<double>>();
        private static Dictionary<string, List<EdkDll.EE_DataChannel_t>> goodSensors = new Dictionary<string, List<EdkDll.EE_DataChannel_t>>();        
        private static int dimensions = Config.SampleLength;
        private static Neuron[,] outputs;
        private static bool load = false;
        private Dictionary<EdkDll.EE_DataChannel_t, Dictionary<string, int>> class_results = new Dictionary<EdkDll.EE_DataChannel_t, Dictionary<string, int>>();
        private static GridVisualiser form;
        public int sampleIndex = 0;
        List<EdkDll.EE_DataChannel_t> exclude = new List<EdkDll.EE_DataChannel_t>();
        public Classifier()
        {
            // exclude.Add(EdkDll.EE_DataChannel_t.O1);
            // exclude.Add(EdkDll.EE_DataChannel_t.O2);
            // exclude.Add(EdkDll.EE_DataChannel_t.P7);
            // exclude.Add(EdkDll.EE_DataChannel_t.P8);
            // exclude.Add(EdkDll.EE_DataChannel_t.T7);
            // exclude.Add(EdkDll.EE_DataChannel_t.T8);
            //exclude.Add(EdkDll.EE_DataChannel_t.FC5);
            // exclude.Add(EdkDll.EE_DataChannel_t.FC6);
            // exclude.Add(EdkDll.EE_DataChannel_t.AF4);
            // //exclude.Add(EdkDll.EE_DataChannel_t.AF3);
            // //exclude.Add(EdkDll.EE_DataChannel_t.F4);
            exclude.Add(EdkDll.EE_DataChannel_t.F8);
            //  exclude.Add(EdkDll.EE_DataChannel_t.F3);
            // exclude.Add(EdkDll.EE_DataChannel_t.F7);

            for (EdkDll.EE_DataChannel_t sensor = EdkDll.EE_DataChannel_t.AF3;
                 sensor <= EdkDll.EE_DataChannel_t.AF4;
                 sensor++)
            {

                if (exclude.Contains(sensor))
                    continue;
                windows.Add(sensor, new LinkedList<double>());
                class_results.Add(sensor, new Dictionary<string, int>() { { "A", 0 }, { "B", 0 }, { "C", 0 }, { "", 0 }, { ".", 0 } });
                Console.Write(sensor + "  ");
            }
            Console.SetBufferSize(200, 10000);
            Console.WriteLine();
        }

        public List<EdkDll.EE_DataChannel_t> GetGodSensors(Dictionary<EdkDll.EE_DataChannel_t, Dictionary<string, int>> class_results, string sampleName)
        {
            List<EdkDll.EE_DataChannel_t> result = new List<EdkDll.EE_DataChannel_t>();

            foreach (var key in class_results.Keys)
            {
                var sensorStatistics = class_results[key];
                double targetCount = sensorStatistics[sampleName];
                targetCount *= Config.ClassifierSensorThreshold;
                bool goodSensor = true;
                foreach (var name in Config.sampleNames)
                {
                    if (name == sampleName)
                        continue;
                    int otherSample = sensorStatistics[name];
                    if (otherSample > targetCount)
                    {
                        goodSensor = false;
                        break;
                    }

                }
                if (goodSensor)
                    result.Add(key);
            }
            return result;
        }

        public static bool emulationFlag = true;
        public void OnEmulationFinished(object sender)
        {
            if (sampleIndex < Config.sampleNames.Length-1)
            {
                string sampleName = Config.sampleNames[sampleIndex];
                goodSensors[sampleName].AddRange(GetGodSensors(class_results, sampleName));
                sampleIndex++;
                emulationFlag = false;
                //((EmotyperDataUtility.EmotypeEventSource)sender).Stop();
            }
        }

        void OnDataArrived(object sender, EmoEventDictionary e)
        {

            Dictionary<EdkDll.EE_DataChannel_t, double[]> data = e.Dictionary;
            if (data == null)
                return;

            for (EdkDll.EE_DataChannel_t sensor = EdkDll.EE_DataChannel_t.AF3;
            sensor <= EdkDll.EE_DataChannel_t.AF4;
            sensor++)
            {
                if (exclude.Contains(sensor))
                    continue;
                if (data.ContainsKey(sensor))
                    windows[sensor].AppendRange(data[sensor]);
            }

            while (windows[EdkDll.EE_DataChannel_t.AF3].Count > dimensions)
            {
                int g1 = 0;
                int g2 = 0;
                int g3 = 0;
                int g4 = 0;


                // for (int i = _bufferSize-128; i < _bufferSize; i++)
                for (EdkDll.EE_DataChannel_t sensor = EdkDll.EE_DataChannel_t.AF3;
                     sensor <= EdkDll.EE_DataChannel_t.AF4;
                     sensor++)
                {

                    if (exclude.Contains(sensor))
                    {
                        Console.Write(@"   .");
                        continue;
                    }
                    if (windows[sensor].Count < dimensions)
                        break;
                    var series = DataTransformations.Transform(Config.InputDataTransformation,
                                                               windows[sensor].GetdRange(dimensions).ToList());
                    double res = Classify(series.ToArray(), sensor);
                    windows[sensor].RemoveFirst();
                    string letter = getLetter(res);
                    class_results[sensor][letter]++;
                    //  Console.WriteLine(sensor + "   " + letter);
                    if (letter == Config.EmulationSample)
                        Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("   " + letter);
                    Console.ResetColor();
                    //if (res == 0)
                    //    g1++;
                    //else if (res == 2)
                    //    g2++;
                    //else if (res == 3)
                    //    g3++;
                    //else if (res == 4)
                    //    g4++;
                }
                //Console.WriteLine("========================================");
                // Console.WriteLine();
                int result = Math.Max(Math.Max(g1, g2), Math.Max(g3, g4));
                //string reslt = getLetter(result);
                // Console.WriteLine("Classification result: " + reslt);
                Console.WriteLine();
            }
        }

        private string getLetter(double digit)
        {
            string reslt = ".";
            if (1 == digit)
                reslt = "A";
            else if (2 == digit)
                reslt = "B";
            else if (3 == digit)
                reslt = "C";
            else if (4 == digit)
                reslt = ".";
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
                    double d = DistanceCalculator.DistancePearson(pattern, outputs[i, j].Weights);
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
            int part = length / 4;

            var task1 = Task.Factory.StartNew(() => getRangeWinner(length, 0, part, outputs, pattern));
            var task2 = Task.Factory.StartNew(() => getRangeWinner(length, part, part * 2, outputs, pattern));
            var task3 = Task.Factory.StartNew(() => getRangeWinner(length, part * 2, part * 3, outputs, pattern));
            var task4 = Task.Factory.StartNew(() => getRangeWinner(length, part * 3, part * 4, outputs, pattern));

            Task.WaitAll(task1, task2, task3, task4);
            WinnerDistance res1 = task1.Result;
            WinnerDistance res2 = task2.Result;
            WinnerDistance res3 = task3.Result;
            WinnerDistance res4 = task4.Result;
            double curmin = Math.Min(Math.Min(res1.distance, res2.distance), Math.Min(res3.distance, res4.distance));
            if (curmin < min)
                if (res1.distance == curmin)
                    return res1.winner;
                else if (res2.distance == curmin)
                    return res1.winner;
                else if (res3.distance == curmin)
                    return res3.winner;
                else if (res4.distance == curmin)
                    return res4.winner;
            return winner;
        }
        static double Classify(double[] series, EdkDll.EE_DataChannel_t sensor)
        {
            outputs = maps[sensor];
            SOM.Neuron n = WinnerParallel(series);
            // form.updateCurrent(n.X, n.Y,sensor.ToString());
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

            foreach (var sampleName in Config.sampleNames)
            {
                goodSensors.Add(sampleName,new List<EdkDll.EE_DataChannel_t>());
            }
            initMaps();
            if (load)
                LoadClassifier();
            else
            {
                buildGrids();
                SaveClassifier();
            } 
            EmotypeEventSource source = new EmotypeEventSource();
            Classifier p= new Classifier();
    
            source.OnEmulationFinish += p.OnEmulationFinished;
            source.OnDataArrived += p.OnDataArrived;

            for (int i = 0; i < Config.sampleNames.Length; i++)
            {

                Config.EmulationSample = Config.sampleNames[p.sampleIndex];
                
               
                source.StartEmulation(String.Format(@"{0}//{1}test", Config.SamplesSourceDirectory, Config.EmulationSample));
                while (Classifier.emulationFlag)
                {
                    
                }
                //source.Stop();
                Classifier.emulationFlag = true;
            }
            /*Start emulation     for each target sample
             after emulation finished get good sensors for each target sample
             start emulation again on the test data
             while this emulation use config frame and try to get coorest results basing on matrix threshold
             after that return percent of correct sasamles vs incorrect
             */

            //form= new GridVisualiser("AF3");              
            //form.Show();
            //  form = context.form;
            source.OnDataArrived += p.OnDataArrived;
            source.StartEmulation(String.Format(Config.EmulationDirectory, Config.EmulationSample));
            //  Application.Run(context);
        }
    }
    public static class LinkedListExtensions
    {
        public static void AppendRange<T>(this LinkedList<T> source,
                                          IEnumerable<T> items)
        {
            foreach (T item in items)
            {
                source.AddLast(item);
            }
        }

        public static void PrependRange<T>(this LinkedList<T> source,
                                           IEnumerable<T> items)
        {
            LinkedListNode<T> first = source.First;
            foreach (T item in items)
            {
                source.AddBefore(first, item);
            }
        }
        public static T[] GetdRange<T>(this LinkedList<T> source,
                                       int count)
        {
            T[] result = new T[count];
            var enumerator = source.GetEnumerator();
            enumerator.MoveNext();
            for (int i = 0; i < count; i++)
            {

                result[i] = enumerator.Current;
                if (!enumerator.MoveNext())
                    break;
            }
            return result;
        }
    }
}
