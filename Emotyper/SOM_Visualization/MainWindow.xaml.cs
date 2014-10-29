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
            gridControl.ShowGridLines = true;
            
            Initialise();
            //LoadData("Food.csv");
           // LoadData("testAB.csv");

           // loadDateFromAllFiles();
            loadCropedDateFromAllFiles();

            //NormalisePatterns();
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
       
        private void loadCropedDateFromAllFiles()
        {
            List<List<double>> aData = getEssentialData("D://GitRepos//Emotyper//Emotyper//A", dimensions, 3);
            List<List<double>> bData = getEssentialData("D://GitRepos//Emotyper//Emotyper//B", dimensions, 3);
           List<List<double>> cData = getEssentialData("D://GitRepos//Emotyper//Emotyper//C", dimensions, 3);
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

        public  List<List<double>> getEssentialData(String samplesDirestory, int FrameSize,int sensor)
        {
           List<List<double>> resultSet = new List<List<double>>();
           List<List<double>> rawSeries = new List<List<double>>();
           List<Tuple<int, int, int, int, double>> pairCorrs = new List<Tuple<int, int, int, int, double>>(); //winsizeSmaller winsizeBigger indexsmaller indexBigger  correlation
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
                //var s = ProcessSingleSeriesFFT(serie);
                //rawSeries.Add(s);
                if (serie.Count < 256)
                {
                    continue;
                }
                else
                {
                    serie = serie.GetRange(0, 256);
                }

                double[] ser = serie.ToArray();
                double[] spectrum =FourierTransform.Spectrum(ref ser);
                double[] mel = DSP.MFCC.compute(ref spectrum);                
                rawSeries.Add(new List<double>(mel)); 
               // rawSeries.Add(ProcessSingleSeries(serie));
            }
            return rawSeries;
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
                   WaveletName = "haar",
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
                else if (labels[i].Contains("B"))
                {
                    label.Background = new SolidColorBrush(Colors.Green);
                }
                else if (labels[i].Contains("C"))
                {
                    label.Background = new SolidColorBrush(Colors.Yellow);
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
                   // double d = Distance(pattern, outputs[i, j].Weights);
                    double d = Distance2(pattern, outputs[i, j].Weights);
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
          Dtw analyser = new Dtw(vector1,vector2,DistanceMeasure.Manhattan,sakoeChibaMaxShift:300);
            return analyser.GetCost();
        }

    }
}
