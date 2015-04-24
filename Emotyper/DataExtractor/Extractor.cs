using Calculations;
using CsvReader;
using DSP;
using Emotiv;
using SOM_Visualization;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using ParticleSwarm;
using WaveletStudio.Blocks;
using WaveletStudio.Functions;
using Task = System.Threading.Tasks.Task;

namespace DataExtractor
{
    static class Extractor
    {
        public static int SampleLength { get; set; }
        public static bool SliceSamples { get; set; }
        static Extractor()
        {
            SampleLength = 128;
            SliceSamples = false;
        }
        static void Main(string[] args)
        {    
            getEssentialDataPerSensor("D://GitRepos//Emotyper//Emotyper//A", "D://GitRepos//Emotyper//Emotyper//ExtractedMatrix//AExtracted", SampleLength,3, true);
            Console.SetBufferSize(400,4000);
            if (!SliceSamples)
            {
                //getEssentialDataFromSamples("D://GitRepos//Emotyper//Emotyper//BTest", "D://GitRepos//Emotyper//Emotyper//ExtractedMatrix//AExtracted", SampleLength, true);
                var task1 = Task.Factory.StartNew(() => getEssentialDataFromSamples("D://GitRepos//Emotyper//Emotyper//A", "D://GitRepos//Emotyper//Emotyper//ExtractedMatrix//AExtracted", SampleLength, true));
                var task2 = Task.Factory.StartNew(() => getEssentialDataFromSamples("D://GitRepos//Emotyper//Emotyper//B", "D://GitRepos//Emotyper//Emotyper//ExtractedMatrix//BExtracted", SampleLength, true));
                var task3 = Task.Factory.StartNew(() => getEssentialDataFromSamples("D://GitRepos//Emotyper//Emotyper//C", "D://GitRepos//Emotyper//Emotyper//ExtractedMatrix//CExtracted", SampleLength, true));
                Task.WaitAll(task1, task2, task3);
              //  Task.WaitAll(task1);
                Console.WriteLine("Extraction ended");
            }
            else
            {
                for (EdkDll.EE_DataChannel_t i = EdkDll.EE_DataChannel_t.AF3;
                       i <= EdkDll.EE_DataChannel_t.AF4;
                       i++)
                {
                    Slice("D://GitRepos//Emotyper//Emotyper//Neutral",
                                     "D://GitRepos//Emotyper//Emotyper//Extracted//NeutralExtracted" +
                                     Enum.GetName(typeof(EdkDll.EE_DataChannel_t), i), SampleLength, (int)i);
                }
            }
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
        public static List<List<double>> getEssentialDataPerSensor(String sourceSamplesDirestory, string targetSamplesDirectory, int FrameSize, int sensor, bool writefiles = false)
        {
            List<List<double>> resultSet = new List<List<double>>();
            List<List<double>> rawSeries = new List<List<double>>();
            List<Tuple<int, int, int, int, double>> pairCorrs = new List<Tuple<int, int, int, int, double>>(); //winsizeSmaller winsizeBigger indexsmaller indexBigger  correlation
            foreach (String file in Directory.GetFiles(sourceSamplesDirestory))
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

                double[] ser= serie.GetRange(0, 128).ToArray();
                double[] spectrum = FourierTransform.Spectrum(ref ser);
                MFCC.initWithrange(9.1089,40,80);
                double[] mel = MFCC.compute(ref spectrum);
                // rawSeries.Add(new List<double>(spectrum)); 
                //  rawSeries.Add(ProcessSingleSeries(serie));
            }
            // return rawSeries;
            //List<double> smaller;
            //List<double> bigger;
            //int startIndex1 = 3;
            //int startIndex2 = 15;
            //if (rawSeries[startIndex2].Count > rawSeries[startIndex1].Count)
            //{
            //    bigger = rawSeries[startIndex1];
            //    smaller = rawSeries[startIndex2];
            //}
            //else
            //{
            //    bigger = rawSeries[startIndex2];
            //    smaller = rawSeries[startIndex1];
            //}
            //int m = smaller.Count;
            //int n = bigger.Count;
            //for (int j = 0; j < m - FrameSize; j++)
            //    for (int i = 0; i < n - FrameSize; i++)
            //    {
            //        double correl = DistanceCalculator.DistanceFunc(smaller.GetRange(j, FrameSize).ToArray(), bigger.GetRange(i, FrameSize).ToArray());
            //        //  if (Math.Abs(correl) > 0.6)
            //        pairCorrs.Add(new Tuple<int, int, int, int, double>(0, 0, j, i, Math.Abs(correl)));
            //    }
            //pairCorrs.Sort((a, b) => a.Item5.CompareTo(b.Item5));
            //Tuple<int, int, int, int, double> tuple = pairCorrs.First();
            //resultSet.Add(smaller.GetRange(tuple.Item3, FrameSize));
            //resultSet.Add(bigger.GetRange(tuple.Item4, FrameSize));
            //if (startIndex2 > startIndex1)
            //{
            //    rawSeries.RemoveAt(startIndex2);
            //    rawSeries.RemoveAt(startIndex1);
            //}
            //else
            //{
            //    rawSeries.RemoveAt(startIndex1);
            //    rawSeries.RemoveAt(startIndex2);
            //}

            //for (int i = 0; i < rawSeries.Count; i++)
            //{
            //    pairCorrs.Clear();
            //    n = rawSeries[i].Count;
            //    for (int j = 0; j < n - FrameSize; j++)
            //    {
            //        double midleCorrel = 0;
            //        foreach (List<double> list in resultSet)
            //        {
            //            //   double correl = dnAnalytics.Statistics.Correlation.Pearson(list,rawSeries[i].GetRange(j, FrameSize));
            //            double correl = DistanceCalculator.DistanceFunc(list.ToArray(), rawSeries[i].GetRange(j, FrameSize).ToArray());
            //            midleCorrel += Math.Abs(correl);
            //        }
            //        //    double correl = Distance(resultSet.Last().ToArray(), rawSeries[i].GetRange(j, FrameSize).ToArray());
            //        midleCorrel = midleCorrel / resultSet.Count;
            //        //   if (Math.Abs(midleCorrel) > 0.4)
            //        pairCorrs.Add(Tuple.Create(0, 0, i, j, Math.Abs(midleCorrel)));
            //    }
            //    pairCorrs.Sort((a, b) => a.Item5.CompareTo(b.Item5));
            //    if (pairCorrs.Count > 0)
            //    {
            //        tuple = pairCorrs.First();
            //        // tuple = pairCorrs.Last();
            //        resultSet.Add(rawSeries[i].GetRange(tuple.Item4, FrameSize));
            //        if (writefiles)
            //        {
            //            CsvRow row = new CsvRow();
            //            StringBuilder s = new StringBuilder();
            //            foreach (double val in resultSet.First())
            //            {
            //                row.Add(val);
            //            }
            //            if (!Directory.Exists(targetSamplesDirectory))
            //                Directory.CreateDirectory(targetSamplesDirectory);
            //            CsvFileWriter writer = new CsvFileWriter(targetSamplesDirectory + Path.DirectorySeparatorChar + string.Format("sample{0}.csv", DateTime.Now.ToBinary()));
            //            writer.WriteRow(row);
            //            writer.Flush();
            //            writer.Close();
            //        }
            //    }
            //}
            return resultSet;
        }
        public static void getEssentialDataFromSamples(String sourceSamplesDirestory,string targetSamplesDirectory, int FrameSize, bool writefiles = false)
        {
            List<Dictionary<EdkDll.EE_DataChannel_t,List<double>>> rawData = new List<Dictionary<EdkDll.EE_DataChannel_t, List<double>>>();
          //  List<Tuple<int[], double>> correlations = new List<Tuple<int[], double>>(); 
            List<double> restrictions = new List<double>();
            List<double> minvalues  = new List<double>();
            List<Tuple<int, int, int, int, double>> pairCorrs = new List<Tuple<int, int, int, int, double>>(); //winsizeSmaller winsizeBigger indexsmaller indexBigger  correlation
            foreach (String file in Directory.GetFiles(sourceSamplesDirestory))
            {
                DataTable table = CSVReader.ReadCSVFile(file.Replace("\\", "//"), true);   
                if (table.Rows.Count - 1<FrameSize)
                    continue;
                var matrix =new Dictionary<EdkDll.EE_DataChannel_t, List<double>>();
                for (EdkDll.EE_DataChannel_t sensor = EdkDll.EE_DataChannel_t.AF3;
                    sensor <= EdkDll.EE_DataChannel_t.AF4;
                    sensor++)
                {
                  
                    List<double> serie = new List<double>();
                    for (int j = 0; j < table.Rows.Count - 1; j++)
                    {
                        DataRow row = table.Rows[j];
                        double val;
                        Double.TryParse(row[(int)sensor].ToString(), out val);
                        serie.Add(val);
                    }
                    matrix.Add(sensor,serie);
                }
                restrictions.Add(matrix[EdkDll.EE_DataChannel_t.AF3].Count-FrameSize-1-10);
                minvalues.Add(10);
                rawData.Add(matrix);               
            }

             Console.WriteLine(@"Samples Loaded");
            ParticleSwarm.Task task = new MatrixTask(minvalues.ToArray(),restrictions.ToArray(),rawData,FrameSize);
            Swarm s  = new Swarm(task,500,0.3,2,5);
            double[] prevPosition= new double[restrictions.Count] ;
            prevPosition[1] = -1;   //for first iteration
            do
            {     
                prevPosition = s.BestPosition;
                s.NextIteration();                
            } while (VectorlengthWithCast(VectorDifference(prevPosition, s.BestPosition)) != 0);
            double[] bestVector =   s.BestPosition;
           
                //var iterator = new CombinationEnumerator(restrictions.ToArray());
                //iterator.Step = 5;
                //int[] bestVector = new int[1];
                //double mindist = double.MaxValue;
                //List<double[]> correlations = new List<double[]>();
                //List<EdkDll.EE_DataChannel_t> exclude = new List<EdkDll.EE_DataChannel_t>();

                ////exclude.Add(EdkDll.EE_DataChannel_t.AF3);
                ////exclude.Add(EdkDll.EE_DataChannel_t.F7);
                ////exclude.Add(EdkDll.EE_DataChannel_t.F3);
                ////exclude.Add(EdkDll.EE_DataChannel_t.FC5);
                ////exclude.Add(EdkDll.EE_DataChannel_t.T7);
                ////exclude.Add(EdkDll.EE_DataChannel_t.P7);
                ////exclude.Add(EdkDll.EE_DataChannel_t.O1);
                ////exclude.Add(EdkDll.EE_DataChannel_t.O2);
                ////exclude.Add(EdkDll.EE_DataChannel_t.P8);
                ////exclude.Add(EdkDll.EE_DataChannel_t.T8);
                ////exclude.Add(EdkDll.EE_DataChannel_t.FC6);
                ////exclude.Add(EdkDll.EE_DataChannel_t.F4);
                ////exclude.Add(EdkDll.EE_DataChannel_t.F8);
                ////exclude.Add(EdkDll.EE_DataChannel_t.AF4);

                ////double[] weights = new double[14]{1,0.5,0.5,0,0.3,0,0,1,0,0,0,0,1,1};
                //while (iterator.MoveNext())
                //{
                //    //double[] corell = matrixCorrelation(rawData, iterator.Position, FrameSize,exclude,weights);
                //    double[] corell = matrixCorrelation(rawData, iterator.Position, FrameSize);
                //    double midcor = corell.Average() ;

                //    if (midcor < mindist)
                //    {   correlations.Add(corell);
                //        mindist = midcor;
                //        bestVector =(int[]) iterator.Position.Clone();
                //    }
                //  //  DrawArray(iterator.Position);
                //    UpdateArray(iterator.Position);
                //  //  DrawProgressBar(iterator.Position[0], restrictions[0], 100,'o');
                //}
                ////for (EdkDll.EE_DataChannel_t sensor = EdkDll.EE_DataChannel_t.AF3;
                ////     sensor <= EdkDll.EE_DataChannel_t.AF4;
                ////     sensor++)
                ////{
                ////    if (exclude.Contains(sensor))
                ////        continue;
                ////    Console.Write(sensor + "       ");
                ////}
                ////Console.WriteLine();
                ////foreach (double[] correlation in correlations)
                ////{
                ////   DrawArray(correlation);
                ////}
                Console.WriteLine(@"best vector found!");

            DrawArray(bestVector);

             Console.WriteLine();
            string samplesDir = "";
            for (int i = 0; i < rawData.Count; i++)
            { 
                foreach (KeyValuePair<EdkDll.EE_DataChannel_t, List<double>> keyValuePair in rawData[i])
                {
                    samplesDir=targetSamplesDirectory + Enum.GetName(typeof(EdkDll.EE_DataChannel_t), keyValuePair.Key);
                    if (!Directory.Exists(samplesDir))
                        Directory.CreateDirectory(samplesDir);
                     CsvRow row = new CsvRow();
                     foreach (double val in keyValuePair.Value.GetRange((int)bestVector[i], FrameSize))
                     {
                         row.Add(val);
                     }
                     CsvFileWriter writer = new CsvFileWriter(samplesDir + Path.DirectorySeparatorChar + string.Format("sample{0}.csv", DateTime.Now.ToBinary()));
                        writer.WriteRow(row);
                        writer.Flush();
                        writer.Close();
                }
            }
        }
        private static double[] VectorDifference(double[] v1, double[] v2)
           {
               double[] result = new double[v1.Length];
               for (int i = 0; i < v1.Length; i++)
               {
                   result[i] = v1[i] - v2[i];
               }
               return result;
           }
           private static int VectorlengthWithCast(double[] v1)
           {
               int result = 0;
               for (int i = 0; i < v1.Length; i++)
               {
                   result+= (int)v1[i] ;
               }
               return result;
           }

        private static void DrawProgressBar(int complete, int maxVal, int barSize, char progressCharacter)
        {
            Console.CursorVisible = false;
            int left = Console.CursorLeft;
            decimal perc = (decimal)complete / (decimal)maxVal;
            int chars = (int)Math.Floor(perc / ((decimal)1 / (decimal)barSize));
            string p1 = String.Empty, p2 = String.Empty;

            for (int i = 0; i < chars; i++) p1 += progressCharacter;
            for (int i = 0; i < barSize - chars; i++) p2 += progressCharacter;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(p1);
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.Write(p2);

            Console.ResetColor();
            Console.Write(" {0}%", (perc * 100).ToString("N2"));
            Console.CursorLeft = left;
        }
        public static double[] matrixCorrelation(List<Dictionary<EdkDll.EE_DataChannel_t, List<double>>> matrixes, double[] positions, int winsize, List<EdkDll.EE_DataChannel_t> exclude=null, double[] weights=null)
        {
            double[] matrixCorr=new double[14];
            for (EdkDll.EE_DataChannel_t sensor = EdkDll.EE_DataChannel_t.AF3;
                 sensor <= EdkDll.EE_DataChannel_t.AF4;
                 sensor++)
            {
                if (exclude != null)
                {
                    if (exclude.Contains(sensor))
                        continue;
                }
                double componentCorr=0;
                int corrsCount =0;
                for (int i = 0; i < matrixes.Count; i++)
                {
                    for (int j = i+1;j < matrixes.Count; j++)
                    {
                        componentCorr += DistanceCalculator.DistancePearson(
                            matrixes[i][sensor].GetRange((int)positions[i], winsize).ToArray(),
                            matrixes[j][sensor].GetRange((int)positions[j], winsize).ToArray());
                        corrsCount++;
                    }
                    
                }
                if (weights==null)
                matrixCorr[(int)(sensor - EdkDll.EE_DataChannel_t.AF3)] = componentCorr;
                else
                {
                    matrixCorr[(int)(sensor - EdkDll.EE_DataChannel_t.AF3)] = componentCorr * weights[sensor - EdkDll.EE_DataChannel_t.AF3];
                }
            }
            return matrixCorr;
        }
        public static void Slice(String sourceSamplesDirestory, string targetSamplesDirectory,
                                                     int FrameSize, int sensor)
        {
            List<List<double>> resultSet = new List<List<double>>();
            foreach (String file in Directory.GetFiles(sourceSamplesDirestory))
            {
                DataTable table = CSVReader.ReadCSVFile(file.Replace("\\", "//"), true);

                List<double> serie = new List<double>();
                for (int j = 0; j < table.Rows.Count; j++)
                {
                    DataRow row = table.Rows[j];
                    double val;
                    Double.TryParse(row[sensor].ToString(), out val);
                    serie.Add(val);
                    if (j % FrameSize == 0 && j > 0)
                    {
                        resultSet.Add(serie);
                        serie = new List<double>();
                    }
                }

            }
            foreach (List<double> list in resultSet)
            {

                CsvRow row = new CsvRow();
                StringBuilder s = new StringBuilder();
                foreach (double val in list)
                {
                    row.Add(val);
                }
                if (!Directory.Exists(targetSamplesDirectory))
                    Directory.CreateDirectory(targetSamplesDirectory);
                CsvFileWriter writer = new CsvFileWriter(targetSamplesDirectory + Path.DirectorySeparatorChar + string.Format("sample{0}.csv", DateTime.Now.ToBinary()));
                writer.WriteRow(row);
                writer.Flush();
                writer.Close();
            }
        }
        public static void DrawArray(IEnumerable<double> array)
        {
            foreach (double d in array)
            {
                Console.Write(d.ToString("0.000") + "    ");
            }
            Console.WriteLine();
        }
        public static void DrawArray(IEnumerable<int> array)
        {
            foreach (double d in array)
            {
                Console.Write(d + "    ");
            }
            Console.WriteLine();
        }
        public static void UpdateArray(IEnumerable<double> array)
        {
            Console.CursorVisible = false;
            int left = Console.CursorLeft;
            foreach (double d in array)
            {
                Console.Write(d + "    ");
            }
            Console.ResetColor();
            Console.CursorLeft = left;
        }
    }
}
