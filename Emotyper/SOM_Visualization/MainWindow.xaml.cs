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
using SOM;
using WaveletStudio.Blocks;
using WaveletStudio.Functions;

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
            this.length = 50; 
            this.dimensions = 70;
            InitializeComponent();
            for (int i = 0; i < this.length; i++)
            {               
                gridControl.ColumnDefinitions.Add(new ColumnDefinition());
                gridControl.RowDefinitions.Add(new RowDefinition());
                
            }
            gridControl.ShowGridLines = true;
            
            Initialise();
            //LoadData("Food.csv");
           // LoadData("testAB.csv");

           // loadDateFromAllFiles();
            loadCropedDateFromAllFiles();

            NormalisePatterns();
            Train(0.0000001);
            DumpCoordinates();
        }
             

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

        private void LoadData(string file)
        {
            StreamReader reader = File.OpenText(file);
            //reader.ReadLine(); // Ignore first line.
            while (!reader.EndOfStream)
            {
                string[] line = reader.ReadLine().Split(';');
                labels.Add(line[0]);
                double[] inputs = new double[dimensions];
                for (int i = 0; i < dimensions; i++)
                {
                    inputs[i] = double.Parse(line[i + 1]);
                }
                patterns.Add(inputs);
            }
            reader.Close();
        }

        private void loadCropedDateFromAllFiles()
        {
            List<List<double>> aData = getEssentialData("D://GitRepos//Emotyper//Emotyper//A", dimensions, 3);
            List<List<double>> bData = getEssentialData("D://GitRepos//Emotyper//Emotyper//B", dimensions, 3);
            int index = 0;
            foreach (List<double> list in aData)
            {
                patterns.Add(list.ToArray());
                labels.Add("A" + index);
                index++;
            }
            foreach (List<double> list in bData)
            {
                patterns.Add(list.ToArray());
                labels.Add("B" + index);
                index++;
            }
        }

        private void loadDateFromAllFiles()
        {
            
            //for (int i = 3; i < 17; i++)
            //{
               int aind = 0,bind=0; 
               int ind = 3;
               List<List<double>> Asamples = new List<List<double>>();
               List<List<Tuple<int, int, int, int, double>>> correlations = new List<List<Tuple<int, int, int, int, double>>>();
               int MinWinSize = 64;
            foreach (String file in Directory.GetFiles("D://GitRepos//Emotyper//Emotyper//A"))
            {
               
                DataTable table = CSVReader.ReadCSVFile(file.Replace("\\","//"), true);
              
                List<double> serie = new List<double>();
                // String sensorName = table.Columns[i].ColumnName;
                for (int j = 0; j < table.Rows.Count; j++)
                {
                    DataRow row = table.Rows[j];
                    double val;
                    Double.TryParse(row[ind].ToString(), out val);
                    serie.Add(val);
                }
                //serie = processOneSeries(serie);
                Asamples.Add(serie);
                patterns.Add(serie.GetRange(0, 110).ToArray());
                labels.Add(String.Format("A{0}", aind));
                aind++;
            }
               for (int k = 1; k < Asamples.Count; k++)
               {
                   List<double> smaller;
                   List<double> bigger;
                   List <Tuple<int, int, int, int,double>> pairCorrs = new List<Tuple<int, int, int, int,double>>(); //winsizeSmaller winsizeBigger indexsmaller indexBigger  correlation
                   if (Asamples[k].Count > Asamples[k - 1].Count)
                   {
                       bigger = Asamples[k];
                       smaller = Asamples[k - 1];
                   }
                   else
                   {
                       bigger = Asamples[k-1];
                       smaller = Asamples[k];
                   }
                   int m = smaller.Count;
                   int n = bigger.Count;
                  // for (int ws1 = MinWinSize; ws1 < m; ws1++)
                       //for (int ws2=MinWinSize;ws2<n;ws2++)
                   //{
                   int ws2 = 140;
                   int ws1 = ws2;
                       for (int j = 0; j < m - ws1; j++)
                           for (int i = 0; i < n - ws2; i++)
                           {
                               double correl = dnAnalytics.Statistics.Correlation.Pearson(smaller.GetRange(j, ws1),bigger.GetRange(i, ws2));
                              // double correl = Distance(smaller.GetRange(j, ws1).ToArray(), bigger.GetRange(i, ws2).ToArray());
                               if (Math.Abs(correl)>0.7)
                               pairCorrs.Add(new Tuple<int, int, int, int, double>(k, k-1, j, i,
                                  correl));
                           }
//}
                   correlations.Add(pairCorrs);    
               }
            foreach (String file in Directory.GetFiles("D://Emotyper//Emotyper//B"))
            {
                DataTable table = CSVReader.ReadCSVFile(file.Replace("\\","//"), true);
                List<double> serie = new List<double>();
                // String sensorName = table.Columns[i].ColumnName;
                for (int j = 0; j < table.Rows.Count; j++)
                {
                    DataRow row = table.Rows[j];
                    double val;
                    Double.TryParse(row[ind].ToString(), out val);
                    serie.Add(val);
                }
                serie = ProcessSingleSeries(serie);
                patterns.Add(serie.GetRange(0, 110).ToArray());
                labels.Add(String.Format("B{0}", bind));
                bind++;
            }
            //}
        }

        public  List<List<double>> getEssentialData(String samplesDirestory, int FrameSize,int sensor)
        {
           List<List<double>> resultSet = new List<List<double>>();
           List<List<double>> rawSeries = new List<List<double>>();
           List<Tuple<int, int, int, int, double>> pairCorrs = new List<Tuple<int, int, int, int, double>>(); //winsizeSmaller winsizeBigger indexsmaller indexBigger  correlation
           foreach (String file in Directory.GetFiles(samplesDirestory))
            {
                var list = File.ReadLines(file.Replace("\\", "//")).Skip(1).Select(line =>
                {
                    var arr = line.Split(';');
                    return Double.Parse(arr[sensor]);
                }).ToList();
                rawSeries.Add(ProcessSingleSeriesFFT(list));  
            }
           List<double> smaller;
           List<double> bigger;
            int startIndex1 = 3;
            int startIndex2 = 12;
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
                        double correl = dnAnalytics.Statistics.Correlation.Pearson(list,rawSeries[i].GetRange(j, FrameSize));
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
                   WaveletName = "Coiflet 5(coif5)",
                   Level = 1,
                   Rescale = false,
                   ExtensionMode = WaveletStudio.SignalExtension.ExtensionMode.AntisymmetricWholePoint
               };
               var dWTBlock2 = new DWTBlock
               {
                   WaveletName = "Symlet 8(sym8)",
                   Level = 1,
                   Rescale = false,
                   ExtensionMode = WaveletStudio.SignalExtension.ExtensionMode.AntisymmetricWholePoint
               };
               var outputSeriesBlock = new OutputSeriesBlock();

               //Connecting the blocks
               inputSeriesBlock.OutputNodes[0].ConnectTo(dWTBlock.InputNodes[0]);
            dWTBlock.OutputNodes[1].ConnectTo(dWTBlock2.InputNodes[0]);
               dWTBlock2.OutputNodes[1].ConnectTo(outputSeriesBlock.InputNodes[0]);

               //Appending the blocks to a block list and execute all
               var blockList = new BlockList();
               blockList.Add(inputSeriesBlock);
               blockList.Add(dWTBlock);
               blockList.Add(dWTBlock2);
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
               
                  var task1 = Task.Factory.StartNew(() => trainPatternRange(TrainingSet,0,patterns.Count/2));
                  var task2 = Task.Factory.StartNew(() =>trainPatternRange(TrainingSet,patterns.Count / 2,patterns.Count ));
                Task.WaitAll(task1,task2);
                currentError = task1.Result + task2.Result;
                
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
              
        private void DumpCoordinates()
        {
            for (int i = 0; i < patterns.Count; i++)
            {
                SOM.Neuron n = Winner(patterns[i]);
                Console.WriteLine("{0},{1},{2}", labels[i], n.X, n.Y);
                Label label = new Label();
                label.Content = labels[i];
                //label.Width = 100;                
               // label.Height = 20;
                if (labels[i].Contains("A"))
                {
                    label.Background= new SolidColorBrush(Colors.Red);
                }
                else
                {
                    label.Background = new SolidColorBrush(Colors.Green);
                }
                
                label.Foreground = new SolidColorBrush(Colors.White);
                label.FontSize = 12;                   
                Grid.SetRow(label,n.X);
                Grid.SetColumn(label,n.Y);
                gridControl.Children.Add(label);
            }           
        }

        private SOM.Neuron Winner(double[] pattern)
        {
            SOM.Neuron winner = null;
            double min = double.MaxValue;
            for (int i = 0; i < length; i++)
                for (int j = 0; j < length; j++)
                {
                    double d = Distance(pattern, outputs[i, j].Weights);
                    //double d = Distance2(pattern, outputs[i, j].Weights);
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

    }
}
