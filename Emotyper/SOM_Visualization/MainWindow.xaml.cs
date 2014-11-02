using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Com.StellmanGreene.CSVReader;
using DSP;
using NDtw;
using SOM;
using WaveletStudio.Blocks;
using WaveletStudio.Functions;
using btl.generic;

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

        private List<string> labels = new List<string>();
        private List<double[]> patterns = new List<double[]>();
        public MainWindow()
        {        
            InitializeComponent();
            this.length = 100; 
            this.dimensions = 11;
            
            for (int i = 0; i < this.length; i++)
            {               
                gridControl.ColumnDefinitions.Add(new ColumnDefinition());
                gridControl.RowDefinitions.Add(new RowDefinition());
                
            }
           // gridControl.ShowGridLines = true;
            
           // Initialise();
           // //LoadData("Food.csv");
           //// LoadData("testAB.csv");

           //// loadDateFromAllFiles();
           // loadCropedDateFromAllFiles();        
           // //NormalisePatterns();
           // Train(0.0000001); 
           
           // DumpCoordinates();

            //  Crossover		= 80%
            //  Mutation		=  5%
            //  Population size = 100
            //  Generations		= 2000
            //  Genome size		= 2
            GA ga = new GA(0.8, 0.05, 100, 2000, 3);

            ga.FitnessFunction = new GAFunction(theActualFunction);

            //ga.FitnessFile = @"H:\fitness.csv";
            ga.Elitism = true;
            ga.Go();

            double[] values;
            double fitness;
            ga.GetBest(out values, out fitness);
            //System.Console.WriteLine("Best ({0}):", fitness);
            //for (int i = 0; i < values.Length; i++)
            //    System.Console.WriteLine("{0} ", values[i]);

            //ga.GetWorst(out values, out fitness);
            //System.Console.WriteLine("\nWorst ({0}):", fitness);
            //for (int i = 0 ; i < values.Length ; i++)
            //    System.Console.WriteLine("{0} ", values[i]);
         //   System.Console.ReadLine();
            DSP.MFCC.d1 = values[0];
            DSP.MFCC.d2 = values[1];
            DSP.MFCC.d3 = values[2];
            Initialise();
            loadCropedDateFromAllFiles();
            Train(0.0000001);
             drawResults();
        }
        public double theActualFunction(double[] vals)
        {
                                List<double> values= new List<double>(vals);
           // values.Sort();
            DSP.MFCC.d1 = values[0];
            DSP.MFCC.d2 = values[1];
            DSP.MFCC.d3 = values[2];
            Console.WriteLine("_____________________MelCoifs___________________________________");
            Console.WriteLine(values[0]);
            Console.WriteLine(values[1]);
            Console.WriteLine(values[2]);
            //Console.WriteLine("_____________________Frequincies___________________________________");
            //for (int i = 3; i < values.Length; i++)
            //{
            //    MFCC.melWorkingFrequencies[i - 3] = values[i];
            //    Console.WriteLine(values[i]);
            //}
                Initialise();
            loadCropedDateFromAllFiles();
            Train(0.0000001);
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
            outputs = new SOM.Neuron[length,length];
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
       
        private void loadCropedDateFromAllFiles()
        {
            patterns.Clear();
            //labels.Clear();
            //List<List<double>> aData = getEssentialData("D://GitRepos//Emotyper//Emotyper//A", dimensions, 3);
            //List<List<double>> bData = getEssentialData("D://GitRepos//Emotyper//Emotyper//B", dimensions, 3);
            //List<List<double>> cData = getEssentialData("D://GitRepos//Emotyper//Emotyper//C", dimensions, 3);
            List<List<double>> aData = getSensorData("D://GitRepos//Emotyper//Emotyper//Aaf3");
            List<List<double>> bData = getSensorData("D://GitRepos//Emotyper//Emotyper//Baf3");
            List<List<double>> cData = getSensorData("D://GitRepos//Emotyper//Emotyper//Caf3");
            int index = 0;
            foreach (List<double> list in aData)
            {
                patterns.Add(list.ToArray());
                labels.Add("A" + index);
                index++;
            }
            foreach (List<double> list in cData)
            {
                patterns.Add(list.ToArray());
                labels.Add("C" + index);
                index++;
            }
            foreach (List<double> list in bData)
            {
                patterns.Add(list.ToArray());
                labels.Add("B" + index);
                index++;
            } 
            
        }

        private List<List<double>> getSensorData(string samplesDirestory)
        {
            List<List<double>> rawSeries = new List<List<double>>();
            foreach (String file in Directory.GetFiles(samplesDirestory))
            {

                DataTable table = CSVReader.ReadCSVFile(file.Replace("\\", "//"), false,";");
                List<double> serie = new List<double>();
               
                    DataRow row = table.Rows[0];
                    double val;
                foreach (object d in row.ItemArray)
                {
                    Double.TryParse(d.ToString(), out val);
                    serie.Add(val);                   
                }
                    //Double.TryParse(row[sensor].ToString(), out val);
                    //serie.Add(val);

                double[] ser = serie.ToArray();
                //double[] spectrum = FourierTransform.Spectrum(ref ser);
                double[] mel = MFCC.compute(ref ser);
                rawSeries.Add(new List<double>(mel)); 
                //rawSeries.Add(serie);
            }
            return rawSeries;
        }

      

        public  List<List<double>> getEssentialData(String samplesDirestory, int FrameSize,int sensor,bool writefiles = false)
        {
           List<List<double>> resultSet = new List<List<double>>();
           List<List<double>> rawSeries = new List<List<double>>();
           List<Tuple<int, int, int, int, double>> pairCorrs = new List<Tuple<int, int, int, int, double>>(); //winsizeSmaller winsizeBigger indexsmaller indexBigger  correlation
           CsvFileWriter writer;
            foreach (String file in Directory.GetFiles(samplesDirestory))
            {
               
                DataTable table = CSVReader.ReadCSVFile(file.Replace("\\", "//"), true);
                List<double> serie = new List<double>();
                // String sensorName = table.Columns[i].ColumnName;
                for (int j = 0; j < table.Rows.Count; j++)
                {
                    DataRow row = table.Rows[j];
                    double val;
                    Double.TryParse(row[sensor].ToString(), out val);
                    serie.Add(val);
                }
                rawSeries.Add(serie);
                //var s = ProcessSingleSeriesFFT(serie);
                //rawSeries.Add(s);
               // serie = ProcessSingleSeries(serie);
               // if (serie.Count < 128)
               // {
               //     continue;
               // }
               // else
               // {
               //     serie = serie.GetRange(0, 128);
               // }

               // double[] ser =serie.ToArray();
               //double[] spectrum =FourierTransform.Spectrum(ref ser);
               // double[] mel = MFCC.compute(ref ser);
               // rawSeries.Add(new List<double>(spectrum)); 
              //  rawSeries.Add(ProcessSingleSeries(serie));
            }
           // return rawSeries;
           List<double> smaller;
           List<double> bigger;
            int startIndex1 = 12;
            int startIndex2 = 15;
            if (rawSeries[startIndex2].Count > rawSeries[startIndex1].Count)
           {
               bigger = rawSeries[startIndex1];
               smaller = rawSeries[startIndex2];
           }
           else
           {
               bigger = rawSeries[startIndex2];
               smaller = rawSeries[startIndex1];
           }
           int m = smaller.Count;
           int n = bigger.Count;
           for (int j = 0; j < m - FrameSize; j++)
               for (int i = 0; i < n - FrameSize; i++)
               {
                   double correl = dnAnalytics.Statistics.Correlation.Pearson(smaller.GetRange(j, FrameSize), bigger.GetRange(i, FrameSize));
                 //  if (Math.Abs(correl) > 0.6)
                       pairCorrs.Add(new Tuple<int, int, int, int, double>(0, 0, j, i,Math.Abs(correl)));
               }
          pairCorrs.Sort((a,b)=>a.Item5.CompareTo(b.Item5));
            Tuple<int, int, int, int, double> tuple = pairCorrs[0];
          resultSet.Add(smaller.GetRange(tuple.Item3,FrameSize));
          resultSet.Add(bigger.GetRange(tuple.Item4, FrameSize));
            if (startIndex2 > startIndex1)
            {
                rawSeries.RemoveAt(startIndex2);
                rawSeries.RemoveAt(startIndex1);
            }
            else
            {
                rawSeries.RemoveAt(startIndex1);
                rawSeries.RemoveAt(startIndex2);
            }

            for (int i = 0; i < rawSeries.Count; i++)
            {
                pairCorrs.Clear();
                n = rawSeries[i].Count;
                for (int j = 0; j < n - FrameSize; j++)
                {
                    double midleCorrel = 0;
                    foreach (List<double> list in resultSet)
                    {
                     //   double correl = dnAnalytics.Statistics.Correlation.Pearson(list,rawSeries[i].GetRange(j, FrameSize));
                        double correl = Distance3(list.ToArray(), rawSeries[i].GetRange(j, FrameSize).ToArray());
                        midleCorrel += Math.Abs(correl);
                    }
                    //    double correl = Distance(resultSet.Last().ToArray(), rawSeries[i].GetRange(j, FrameSize).ToArray());
                    midleCorrel = midleCorrel/resultSet.Count;
                 //   if (Math.Abs(midleCorrel) > 0.4)
                        pairCorrs.Add(Tuple.Create(0, 0, i, j,Math.Abs(midleCorrel)));
                }
                pairCorrs.Sort((a, b) => a.Item5.CompareTo(b.Item5));
                if (pairCorrs.Count > 0)
                {
                    tuple = pairCorrs[0];
                   // tuple = pairCorrs.Last();
                    resultSet.Add(rawSeries[i].GetRange(tuple.Item4, FrameSize));
                    if (writefiles)
                    {
                        CsvRow row = new CsvRow();
                        StringBuilder s = new StringBuilder();
                        foreach (double val in resultSet.Last())
                        {
                            row.Add(val);

                        }

                        writer =
                            new CsvFileWriter(
                                samplesDirestory.Replace("A", "AFilter").Replace("B", "BFilter").Replace("C", "CFilter") +
                                String.Format("sample{0}.csv", DateTime.Now.ToBinary()));
                        writer.WriteRow(row);
                        writer.Flush();
                        writer.Close();
                    }
                }
            }
            return resultSet;
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
                double average = sum/patterns.Count;
                for (int i = 0; i < patterns.Count; i++)
                {
                    patterns[i][j] = patterns[i][j]/average;
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
               
                  var task1 = Task.Factory.StartNew(() => trainPatternRange(TrainingSet,0,patterns.Count/4));
                  var task2 = Task.Factory.StartNew(() =>trainPatternRange(TrainingSet,patterns.Count / 4,patterns.Count/2 ));
                  var task3 = Task.Factory.StartNew(() => trainPatternRange(TrainingSet, patterns.Count / 2, patterns.Count/4*3));
                  var task4 = Task.Factory.StartNew(() => trainPatternRange(TrainingSet, patterns.Count / 4*3, patterns.Count));
                Task.WaitAll(task1,task2,task3,task4);
                currentError = task1.Result + task2.Result + task3.Result + task4.Result;
                
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
            for (int i = 0; i < patterns.Count; i++)
            {
                SOM.Neuron n = Winner(patterns[i]);
                Console.WriteLine("{0},{1},{2}", labels[i], n.X, n.Y);
                if (labels[i].Contains("A"))
                {
                    drawPoint(new Point(n.X, n.Y), Colors.Red, labels[i]);
                    a.Add(new Point(n.X,n.Y));
                }
                else if (labels[i].Contains("B"))
                {
                    drawPoint(new Point(n.X, n.Y), Colors.Green, labels[i]);
                    b.Add(new Point(n.X, n.Y));
                }
                else if (labels[i].Contains("C"))
                {
                    drawPoint(new Point(n.X, n.Y), Colors.Yellow, labels[i]);
                    c.Add(new Point(n.X, n.Y));
                }
            }
            Point acenter = getSetCenter(a);
            Point bcenter = getSetCenter(b);
            Point ccenter = getSetCenter(c);
            drawPoint(acenter, Colors.Brown, "");
            drawPoint(ccenter, Colors.Goldenrod, "");
            drawPoint(bcenter, Colors.Lime, "");
            double dist =  GetDistance(acenter, bcenter) + GetDistance(acenter, ccenter) + GetDistance(ccenter, bcenter);
            Console.WriteLine("Distance= "+dist);
            return dist;
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
            Grid.SetColumn(label,(int) coordinates.Y);
            gridControl.Children.Add(label);
        }

        private Point getSetCenter(List<Point> points)
        {
            Point center = new Point();
            foreach (Point point in points)
            {
                center.Offset(point.X,point.Y);
            }
            center.X /= points.Count;
            center.Y /= points.Count;
            return center;
        }

        private SOM.Neuron Winner(double[] pattern)
        {
            SOM.Neuron winner = null;
            double min = double.MaxValue;
            for (int i = 0; i < length; i++)
                for (int j = 0; j < length; j++)
                {
                   // double d = Distance(pattern, outputs[i, j].Weights);
                    double d = Distance(pattern, outputs[i, j].Weights);
                    if (d < min)
                    {
                        min = d;
                        winner = outputs[i, j];
                    }
                }
            return winner;
        }

        private double Distance(double[] vector1, double[] vector2)
        {

            double value = 0;
            for (int i = 0; i < vector1.Length; i++)
            {
                value += Math.Pow((vector1[i] - vector2[i]), 2);
            }
            return Math.Sqrt(value);
        }

        private double Distance2(double[] vector1, double[] vector2)
        {
            return 1/Math.Abs(dnAnalytics.Statistics.Correlation.Pearson(vector1, vector2));
        }
        private double Distance3(double[] vector1, double[] vector2)
        {
          Dtw analyser = new Dtw(vector1,vector2,DistanceMeasure.Manhattan);
            return analyser.GetCost();
        }

    }
}
