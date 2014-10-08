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
            this.dimensions = 110;
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
            loadDateFromAllFiles();
            NormalisePatterns();
            Train(0.0000007);
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
           private void loadDateFromAllFiles()
        {
            
            //for (int i = 3; i < 17; i++)
            //{
               int aind = 0,bind=0; 
               int ind = 5;
               List<List<double>> Asamples = new List<List<double>>();
               List<List<Tuple<int, int, int, int, double>>> correlations = new List<List<Tuple<int, int, int, int, double>>>();
               int MinWinSize = 64;
            foreach (String file in Directory.GetFiles("D://Emotyper//Emotyper//A"))
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
                serie = processOneSeries(serie);
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
                   for (int ws1 = MinWinSize; ws1 < m; ws1++)
                       //for (int ws2=MinWinSize;ws2<n;ws2++)
                   {
                       int ws2 = ws1;
                       for (int j = 0; j < m - ws1; j++)
                           for (int i = 0; i < n - ws2; i++)
                           {
                               double correl = dnAnalytics.Statistics.Correlation.Pearson(smaller.GetRange(j, ws1),bigger.GetRange(i, ws2));
                               if (Math.Abs(correl)>0.5)
                               pairCorrs.Add(new Tuple<int, int, int, int, double>(ws1, ws2, j, i,
                                  correl));
                           }
                   }
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
                serie = processOneSeries(serie);
                patterns.Add(serie.GetRange(0, 110).ToArray());
                labels.Add(String.Format("B{0}", bind));
                bind++;
            }
            //}
        }
           public static List<double> processOneSeries(List<double> serie)
           {

               //Declaring the blocks
               var inputSeriesBlock = new InputSeriesBlock();
               inputSeriesBlock.SetSeries(serie);
               var dWTBlock = new DWTBlock
               {
                   WaveletName = "Daubechies 10 (db10)",
                   Level = 1,
                   Rescale = false,
                   ExtensionMode = WaveletStudio.SignalExtension.ExtensionMode.AntisymmetricWholePoint
               };
               var outputSeriesBlock = new OutputSeriesBlock();

               //Connecting the blocks
               inputSeriesBlock.OutputNodes[0].ConnectTo(dWTBlock.InputNodes[0]);
               dWTBlock.OutputNodes[1].ConnectTo(outputSeriesBlock.InputNodes[0]);

               //Appending the blocks to a block list and execute all
               var blockList = new BlockList();
               blockList.Add(inputSeriesBlock);
               blockList.Add(dWTBlock);
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
                List<double[]> TrainingSet = new List<double[]>();
                foreach (double[] pattern in patterns)
                {
                    TrainingSet.Add(pattern);
                }
                for (int i = 0; i < patterns.Count; i++)
                {
                    double[] pattern = TrainingSet[rnd.Next(patterns.Count - i)];
                    currentError += TrainPattern(pattern);
                    TrainingSet.Remove(pattern);
                }
                Console.WriteLine(currentError.ToString("0.0000000"));
            }
        }

        private double TrainPattern(double[] pattern)
        {
            double error = 0;
            SOM.Neuron winner = Winner(pattern);
            for (int i = 0; i < length; i++)
            {
                for (int j = 0; j < length; j++)
                {
                    error += outputs[i, j].UpdateWeights(pattern, winner, iteration);
                }
            }
            iteration++;
            return Math.Abs(error/(length*length));
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

        //private double Distance(double[] vector1, double[] vector2)
        //{
        //    return dnAnalytics.Statistics.Correlation.Pearson(vector1, vector2);
        //}

    }

    public class Neuron
    {
        public double[] Weights;
        public int X;
        public int Y;
        private int length;
        private double nf;

        public Neuron(int x, int y, int length)
        {
            X = x;
            Y = y;
            this.length = length;
            nf = 1000/Math.Log(length);
        }

        private double Gauss(SOM.Neuron win, int it)
        {
            double distance = Math.Sqrt(Math.Pow(win.X - X, 2) + Math.Pow(win.Y - Y, 2));
            return Math.Exp(-Math.Pow(distance, 2)/(Math.Pow(Strength(it), 2)));
        }

        private double LearningRate(int it)
        {
            return Math.Exp(-it/1000)*0.1;
        }

        private double Strength(int it)
        {
            return Math.Exp(-it/nf)*length;
        }

        public double UpdateWeights(double[] pattern, SOM.Neuron winner, int it)
        {
            double sum = 0;
            for (int i = 0; i < Weights.Length; i++)
            {
                double delta = LearningRate(it)*Gauss(winner, it)*(pattern[i] - Weights[i]);
                Weights[i] += delta;
                sum += delta;
            }
            return sum/Weights.Length;
        }
    }
}
