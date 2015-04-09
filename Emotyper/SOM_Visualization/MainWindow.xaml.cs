using System.Diagnostics;
using CodeArtEng.DspToolbox;
using CodeArtEng.DspToolbox.Filters;
using Com.StellmanGreene.CSVReader;
using Emotiv;
using DSP;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using WaveletStudio.Blocks;
using WaveletStudio.Functions;
using Calculations;

namespace SOM_Visualization
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SOM.Neuron[,] outputs; // Collection of weights.
        private int iteration; // Current iteration.
        private int length; // Side length of output grid.
        private int dimensions; // Number of input dimensions.
        private Random rnd = new Random();
        private List<SOM.Neuron> winners = new List<SOM.Neuron>();
        private bool loadInstance = true;
        private string mapPath = "D://GitRepos//Emotyper//Emotyper//SOMInstances//AF3Map.som";

        private List<string> labels = new List<string>();
        private List<double[]> patterns = new List<double[]>();

        private double currMax = 0;
        public MainWindow()
        {
            InitializeComponent();
            this.length = 100;
            this.dimensions = 75;

            for (int i = 0; i < this.length; i++)
            {
                gridControl.ColumnDefinitions.Add(new ColumnDefinition());
                gridControl.RowDefinitions.Add(new RowDefinition());

            }

            //Crossover		= 80%
            //Mutation		=  5%
            //Population size = 100
            //Generations		= 2000
            //Genome size		= 11
            //GA ga = new GA(0.8, 0.5, 100, 2000, 4);
            //ga.FitnessFunction = new GAFunction(theActualFunction);
            //ga.Elitism = false;
            // ga.Go();

            // double[] values = { 6.75, 14.5, 17.375, 17.5, 20, 23, 24, 25.25, 26.5, 28.5, 30.5 };
            //  double fitness;
            //    ga.GetBest(out values, out fitness);
            //   DSP.MFCC.melWorkingFrequencies = values;
            for (EdkDll.EE_DataChannel_t sensor = EdkDll.EE_DataChannel_t.AF3;
                 sensor <= EdkDll.EE_DataChannel_t.AF4;
                 sensor++)
            {
           // EdkDll.EE_DataChannel_t sensor = EdkDll.EE_DataChannel_t.AF4;
            string SensorName = Enum.GetName(typeof(EdkDll.EE_DataChannel_t), sensor);
                mapPath = String.Format("D://GitRepos//Emotyper//Emotyper//SOMInstances//{0}Map.som", SensorName);
                string coordsPath = String.Format("D://GitRepos//Emotyper//Emotyper//SOMCoordinates//{0}Coordinates.csv", SensorName);
                Initialise();
                LoadCropedDateFromAllFiles(SensorName);
                // NormalisePatterns();
                if (loadInstance)
                {
                    LoadMap();
                    DumpCoordinates();
                    SaveCoordinates(coordsPath);
                }
                else
                {
                    Train(0.00001);
                    SaveMap();
                    //DumpCoordinates();
                    //SaveCoordinates(coordsPath);
                    Console.WriteLine(sensor + " Finished");
                }

            }
        }

        private void SaveMap()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(mapPath));     // if directory does not exist
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(mapPath, FileMode.Create, FileAccess.Write, FileShare.None);
            formatter.Serialize(stream, outputs);
        }

        private void LoadMap()
        {
            IFormatter formatter = new BinaryFormatter();
            if (File.Exists(mapPath))
            {
                Stream stream = new FileStream(mapPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                outputs = (SOM.Neuron[,])formatter.Deserialize(stream);
                stream.Close();
            }
        }

        public double theActualFunction(double[] vals)
        {
            List<double> values = new List<double>(vals);
            values.Sort();
            for (int index = 1; index < values.Count; index++)
            {
                if (values[index] == values[index - 1])
                {
                    values[index]++;
                }
            }
            //DSP.MFCC.d1 = values[0];
            //DSP.MFCC.d2 = values[1];
            //DSP.MFCC.d3 = values[2];
            //Console.WriteLine("_____________________MelCoifs___________________________________");
            //Console.WriteLine(values[0]);
            //Console.WriteLine(values[1]);
            //Console.WriteLine(values[2]);
            Console.WriteLine("_____________________Frequincies___________________________________");
            for (int i = 0; i < 4; i++)
            {
                MFCC.melWorkingFrequencies[i] = values[i];
                Console.WriteLine(values[i]);
            }
            iteration = 0;
            Initialise();
            LoadCropedDateFromAllFiles();
            Train(0.1);
            return DumpCoordinates();
        }

        //public static void Main()
        //{
        //    //  Crossover		= 80%
        //    //  Mutation		=  5%
        //    //  Population size = 100
        //    //  Generations		= 2000
        //    //  Genome size		= 2
        //    GA ga = new GA(0.8, 0.05, 100, 2000, 2);

        //    ga.FitnessFunction = new GAFunction(theActualFunction);

        //    //ga.FitnessFile = @"H:\fitness.csv";
        //    ga.Elitism = true;
        //    ga.Go();

        //    double[] values;
        //    double fitness;
        //    ga.GetBest(out values, out fitness);
        //    System.Console.WriteLine("Best ({0}):", fitness);
        //    for (int i = 0; i < values.Length; i++)
        //        System.Console.WriteLine("{0} ", values[i]);

        //    //ga.GetWorst(out values, out fitness);
        //    //System.Console.WriteLine("\nWorst ({0}):", fitness);
        //    //for (int i = 0 ; i < values.Length ; i++)
        //    //    System.Console.WriteLine("{0} ", values[i]);
        //    System.Console.ReadLine();

        //}     

        private void Initialise()
        {
            iteration = 0;
            outputs = null;
            outputs = new SOM.Neuron[length, length];
            for (int i = 0; i < length; i++)
            {
                for (int j = 0; j < length; j++)
                {
                    outputs[i, j] = new SOM.Neuron(i, j, length);
                    outputs[i, j].Weights = new double[dimensions];
                    for (int k = 0; k < dimensions; k++)
                    {
                        outputs[i, j].Weights[k] = rnd.NextDouble();
                    }
                }
            }
        }

        private void LoadCropedDateFromAllFiles(string sensorName = "AF3")
        {
            patterns.Clear();
            labels.Clear();
            List<IEnumerable<double>> aData = getSensorData(String.Format("D://GitRepos//Emotyper//Emotyper//Extracted//AExtracted{0}", sensorName));
            List<IEnumerable<double>> bData = getSensorData(String.Format("D://GitRepos//Emotyper//Emotyper//Extracted//BExtracted{0}", sensorName));
            List<IEnumerable<double>> cData = getSensorData(String.Format("D://GitRepos//Emotyper//Emotyper//Extracted//CExtracted{0}", sensorName));
            List<IEnumerable<double>> nData = getSensorData(String.Format("D://GitRepos//Emotyper//Emotyper//Extracted//NeutralExtracted{0}", sensorName));
            int index = 0;
            foreach (IEnumerable<double> list in aData)
            {
                patterns.Add(list.ToArray());
                labels.Add("A" + index);
                index++;
            }
            foreach (IEnumerable<double> list in cData)
            {
                patterns.Add(list.ToArray());
                labels.Add("C" + index);
                index++;
            }
            foreach (IEnumerable<double> list in bData)
            {
                patterns.Add(list.ToArray());
                labels.Add("B" + index);
                index++;
            }
            foreach (IEnumerable<double> list in nData)
            {
                patterns.Add(list.ToArray());
                labels.Add("Neutral" + index);
                index++;
            }
        }

        private List<IEnumerable<double>> getSensorData(string samplesDirestory)
        {
            List<IEnumerable<double>> rawSeries = new List<IEnumerable<double>>();
            foreach (String file in Directory.GetFiles(samplesDirestory))
            {
              
                DataTable table = CSVReader.ReadCSVFile(file.Replace("\\", "//"), false, ";");
                List<double> serie = new List<double>();

                DataRow row = table.Rows[0];
                double val;
                if (row.ItemArray.Length == 1)
                    continue;
                foreach (object d in row.ItemArray)
                {
                    Double.TryParse(d.ToString(), out val);
                    serie.Add(val);
                }

                //serie.Add(val);
                // rawSeries.Add(ProcessSingleSeries(serie));

                // Console.WriteLine("===================================FilteringStarted================================");
                //SecondOrderBandPassFilter deltaWavesFilter = new SecondOrderBandPassFilter(1.75, 1.25);
                //SecondOrderBandPassFilter tethaWavesFilter = new SecondOrderBandPassFilter(6, 2);
                //SecondOrderBandPassFilter alphaWavesFilter = new SecondOrderBandPassFilter(10.5, 2.5);
                SecondOrderBandStopFilter betaWavesFilter = new SecondOrderBandStopFilter(21.5, 8.5);
                DiscreteTimeSignal signal = new DiscreteTimeSignal();
                signal.SamplingRate = 128;
                signal.AddRange(serie);
                DiscreteTimeSignal result = betaWavesFilter.ProcessSignal(signal);
                rawSeries.Add(ProcessSingleSeries(new List<double>(result)));
                //double[] ser = serie.ToArray();
                ////double[] ser = result.ToArray();

                //double[] xre = new double[ser.Length]; // Real part
                //double[] xim = new double[ser.Length]; // Imaginary part
                ////double[] spectrum = new double[ser.Length / 2];

                //DSP.FourierTransform.Compute((uint)ser.Length, ser, null, xre, xim, false);

                ////double[] mel = MFCC.compute(ref xre);
                //double[] mel = MFCC.compute(ref ser);
                // rawSeries.Add(new List<double>(mel));
                //rawSeries.Add(serie);
                // rawSeries.Add(xre);
            }
            //double[,] sourceMatrix= To2dArray(rawSeries);

            //// Creates the Principal Component Analysis of the given source
            //var pca = new PrincipalComponentAnalysis(sourceMatrix, AnalysisMethod.Center);

            //// Compute the Principal Component Analysis
            //pca.Compute();
            //   int sercount = rawSeries.Count();
            //// Creates a projection considering 80% of the information
            //double[,] components = pca.Transform(sourceMatrix, sercount);

            //rawSeries.Clear();
            //List<double> templist;
            //for (int i = 0; i < sercount; i++)
            //{
            //    templist = new List<double>();
            //    for (int j = 0; j < dimensions; j++)
            //    {
            //        templist.Add(components[j, i]);
            //    }
            //    rawSeries.Add(templist);
            //}
            return rawSeries;
        }
        public static double[,] To2dArray(List<IEnumerable<double>> list)
        {
            if (list.Count == 0 || list[0].ToArray().Length == 0)
                throw new ArgumentException("The list must have non-zero dimensions.");

            var result = new double[list[0].ToArray().Length, list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                for (int j = 0; j < list[i].ToArray().Length; j++)
                {
                    if (list[i].ToArray().Length != list[0].ToArray().Length)
                        throw new InvalidOperationException("The list cannot contain elements (lists) of different sizes.");
                    result[j, i] = list[i].ToArray()[j];
                }
            }

            return result;
        }

        public static List<double> ProcessSingleSeries(List<double> serie)
        {

            //Declaring the blocks
            var inputSeriesBlock = new InputSeriesBlock();
            inputSeriesBlock.SetSeries(serie);
            var dWTBlock = new DWTBlock
            {
                WaveletName = "coif4",
                Level = 1,
                Rescale = false,
                ExtensionMode = WaveletStudio.SignalExtension.ExtensionMode.AntisymmetricWholePoint
            };
            //var dWTBlock2 = new DWTBlock
            //{
            //    WaveletName = "Discreete Meyer(dmeyer)",
            //    Level = 1,
            //    Rescale = false,
            //    ExtensionMode = WaveletStudio.SignalExtension.ExtensionMode.AntisymmetricWholePoint
            //};
            var outputSeriesBlock = new OutputSeriesBlock();

            //Connecting the blocks
            inputSeriesBlock.OutputNodes[0].ConnectTo(dWTBlock.InputNodes[0]);
            // dWTBlock.OutputNodes[1].ConnectTo(dWTBlock2.InputNodes[0]);
            dWTBlock.OutputNodes[1].ConnectTo(outputSeriesBlock.InputNodes[0]);

            //Appending the blocks to a block list and execute all
            var blockList = new BlockList();
            blockList.Add(inputSeriesBlock);
            blockList.Add(dWTBlock);
            //blockList.Add(dWTBlock2);
            blockList.Add(outputSeriesBlock);
            blockList.ExecuteAll();
            return outputSeriesBlock.GetSeries();
        }
        public static List<double> ProcessSingleSeriesFFT(List<double> serie)
        {

            //Declaring the blocks
            var inputSeriesBlock = new InputSeriesBlock();
            inputSeriesBlock.SetSeries(serie);
            var outputSeriesBlock = new OutputSeriesBlock();

            var fFTBlock = new FFTBlock
            {
                Mode = ManagedFFTModeEnum.UseLookupTable
            };


            //Connecting the blocks
            inputSeriesBlock.OutputNodes[0].ConnectTo(fFTBlock.InputNodes[0]);
            fFTBlock.OutputNodes[1].ConnectTo(outputSeriesBlock.InputNodes[0]);
            //Appending the blocks to a block list and execute all
            var blockList = new BlockList();
            blockList.Add(inputSeriesBlock);
            blockList.Add(fFTBlock);
            blockList.Add(outputSeriesBlock);
            blockList.ExecuteAll();
            return outputSeriesBlock.GetSeries();
        }

        private void NormalisePatterns()
        {
            for (int j = 0; j < dimensions; j++)
            {
                double sum = 0;
                for (int i = 0; i < patterns.Count; i++)
                {
                    sum += patterns[i][j];
                }
                double average = sum / patterns.Count;
                for (int i = 0; i < patterns.Count; i++)
                {
                    patterns[i][j] = patterns[i][j] / average;
                }
            }
        }

        private void Train(double maxError)
        {
            double currentError = double.MaxValue;
            while (currentError > maxError)
            {
                currentError = 0;
                List<double[]> TrainingSet = patterns.ToList();

                var task1 = Task.Factory.StartNew(() => trainPatternRange(TrainingSet, 0, patterns.Count / 4));
                var task2 = Task.Factory.StartNew(() => trainPatternRange(TrainingSet, patterns.Count / 4, patterns.Count / 2));
                var task3 = Task.Factory.StartNew(() => trainPatternRange(TrainingSet, patterns.Count / 2, patterns.Count / 4 * 3));
                var task4 = Task.Factory.StartNew(() => trainPatternRange(TrainingSet, patterns.Count / 4 * 3, patterns.Count));
                Task.WaitAll(task1, task2, task3, task4);
                currentError = task1.Result + task2.Result + task3.Result + task4.Result;
                //for (int i = 0; i < TrainingSet.Count; i++)
                //{
                //    currentError = TrainPattern(TrainingSet[i]);
                //}

                Console.WriteLine(currentError.ToString("0.0000000"));
            }
        }

        private double trainPatternRange(List<double[]> TrainingSet, int startInd, int endInd)
        {
            double currentError = 0;
            for (int i = startInd; i < endInd; i++)
            {
                double[] pattern = TrainingSet[rnd.Next(endInd - i)];
                currentError += TrainPattern(pattern);
                TrainingSet.Remove(pattern);
            }
            return currentError;
        }

        private double TrainPattern(double[] pattern)
        {
            double error = 0;
            SOM.Neuron winner = Winner(pattern);
            error = ActionTrain(pattern, winner, 0, length);
            iteration++;
            return Math.Abs(error / (length * length));
        }

        private double ActionTrain(double[] pattern, SOM.Neuron winner, int iLengthStart, int iLengthEnd)
        {
            double error = 0;

            for (int i = iLengthStart; i < iLengthEnd; i++)
            {
                for (int j = 0; j < length; j++)
                {
                    error += outputs[i, j].UpdateWeights(pattern, winner, iteration);
                }
            }
            return error;
        }


        private double DumpCoordinates()
        {

            List<Point> a = new List<Point>();
            List<Point> b = new List<Point>();
            List<Point> c = new List<Point>();
            winners.Clear();
            for (int i = 0; i < patterns.Count; i++)
            {
               
                //SOM.Neuron n = Winner(patterns[i]);
                //Stopwatch sw = new Stopwatch();
                //sw.Start();
                SOM.Neuron n = WinnerParallel(patterns[i]);
                //sw.Stop();
                //Console.WriteLine("Parallel= " + sw.ElapsedMilliseconds);
                //sw.Restart();
                //n = Winner(patterns[i]);
                //sw.Stop();
                //Console.WriteLine("Sequential= " + sw.ElapsedMilliseconds);
                winners.Add(n);
                if (labels[i].Contains("A"))
                {
                    drawPoint(new Point(n.X, n.Y), Colors.Red, (a.Count(p => p.X == n.X && p.Y == n.Y) + 1).ToString());
                    a.Add(new Point(n.X, n.Y));
                }
                else if (labels[i].Contains("B"))
                {
                    drawPoint(new Point(n.X, n.Y), Colors.Green, (b.Count(p => p.X == n.X && p.Y == n.Y) + 1).ToString());
                    b.Add(new Point(n.X, n.Y));
                }
                else if (labels[i].Contains("C"))
                {
                    // drawPoint(new Point(n.X, n.Y), Colors.Yellow, labels[i]);
                    drawPoint(new Point(n.X, n.Y), Colors.Yellow, (c.Count(p => p.X == n.X && p.Y == n.Y) + 1).ToString());
                    c.Add(new Point(n.X, n.Y));
                }
                else if (labels[i].Contains("Neutral"))
                {
                    drawPoint(new Point(n.X, n.Y), Colors.Gray, (c.Count(p => p.X == n.X && p.Y == n.Y) + 1).ToString());
                }
            }

            Point acenter = getSetCenter(a);
            Point bcenter = getSetCenter(b);
            Point ccenter = getSetCenter(c);

            drawPoint(acenter, Colors.DarkRed, "");
            drawPoint(ccenter, Colors.Goldenrod, "");
            drawPoint(bcenter, Colors.Lime, "");
            double dist = Math.Min(Math.Min(GetDistance(acenter, bcenter), GetDistance(acenter, ccenter)), GetDistance(ccenter, bcenter));
            Console.WriteLine("Distance= " + dist);
            return dist;
        }
        private void SaveCoordinates(string filename)
        {
            if (!Directory.Exists(Path.GetDirectoryName(filename)))
                Directory.CreateDirectory(Path.GetDirectoryName(filename));
            //filename = string.Format(filename + Path.DirectorySeparatorChar +
            //                 string.Format("sample{0}.csv", DateTime.Now.ToBinary()));
            CsvFileWriter writer = new CsvFileWriter(filename);
            for (int i = 0; i < winners.Count; i++)
            {
                CsvRow row = new CsvRow();
                SOM.Neuron n = winners[i];
                row.Add(n.X);
                row.Add(n.Y);

                if (labels[i].Contains("A"))
                {
                    row.Add(1);
                }
                else if (labels[i].Contains("B"))
                {
                    row.Add(2);
                }
                else if (labels[i].Contains("C"))
                {
                    row.Add(3);
                }
                else if (labels[i].Contains("Neutral"))
                {
                    row.Add(4);
                }
                writer.WriteRow(row);
                writer.Flush();
            }
            writer.Close();
        }
        private void drawResults()
        {
            for (int i = 0; i < patterns.Count; i++)
            {
                SOM.Neuron n = Winner(patterns[i]);
                Console.WriteLine("{0},{1},{2}", labels[i], n.X, n.Y);
                if (labels[i].Contains("A"))
                {

                    drawPoint(new Point(n.X, n.Y), Colors.Red, labels[i]);
                }
                else if (labels[i].Contains("B"))
                {
                    drawPoint(new Point(n.X, n.Y), Colors.Green, labels[i]);
                }
                else if (labels[i].Contains("C"))
                {
                    drawPoint(new Point(n.X, n.Y), Colors.Yellow, labels[i]);
                }

            }
        }
        private static double GetDistance(Point point1, Point point2)
        {
            //pythagorean theorem c^2 = a^2 + b^2
            //thus c = square root(a^2 + b^2)
            double a = (double)(point2.X - point1.X);
            double b = (double)(point2.Y - point1.Y);

            return Math.Sqrt(a * a + b * b);
        }
        private void drawPoint(Point coordinates, Color color, String text)
        {
            Label label = new Label();
            label.Content = text;
            label.Background = new SolidColorBrush(color);
            label.Foreground = new SolidColorBrush(Colors.White);
            label.FontSize = 12;
            Grid.SetRow(label, (int)coordinates.X);
            Grid.SetColumn(label, (int)coordinates.Y);
            gridControl.Children.Add(label);
        }

        private Point getSetCenter(List<Point> points)
        {
            Point center = new Point();
            foreach (Point point in points)
            {
                center.Offset(point.X, point.Y);
            }
            center.X /= points.Count;
            center.Y /= points.Count;
            return center;
        }
        private SOM.Neuron Winner(double[] pattern)
        {
            SOM.Neuron winner = null;
            double min = double.MaxValue;
            //Stopwatch sw = new Stopwatch();
            //sw.Start();
            for (int i = 0; i < length; i++)
                for (int j = 0; j < length; j++)
                {
                    // double d = Distance(pattern, outputs[i, j].Weights);
                   // double d = Calculator.DistanceFunc(pattern, outputs[i, j].Weights);
                   double d = Calculator.DistancePearson(pattern, outputs[i, j].Weights);
                    if (d < min)
                    {
                        min = d;
                        winner = outputs[i, j];
                    }
                }
            //sw.Stop();
            //Console.WriteLine(sw.ElapsedMilliseconds);
            return winner;
        }

        private SOM.Neuron WinnerParallel(double[] pattern)
        {
            SOM.Neuron winner = null;
            double min = double.MaxValue;


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

        private WinnerDistance getRangeWinner(int length, int start, int end, SOM.Neuron[,] outputs, double[] pattern)
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

        private void SavevMap_Click(object sender, RoutedEventArgs e)
        {
            SaveMap();
        }
        private void SaveCoords_Click(object sender, RoutedEventArgs e)
        {
            SaveCoordinates("D://GitRepos//Emotyper//Emotyper//SOMCoordinates//Coords3.csv");
        }
    }


    public class WinnerDistance
    {
        public SOM.Neuron winner;
        public double distance;
        public WinnerDistance(double dist, SOM.Neuron neuron)
        {
            winner = neuron;
            distance = dist;
        }
    }
}
