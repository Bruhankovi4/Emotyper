using Calculations;
using Com.StellmanGreene.CSVReader;
using Emotiv;
using SOM_Visualization;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            if (!SliceSamples)
            {
                var task1 = Task.Factory.StartNew(() =>
                    {
                        for (EdkDll.EE_DataChannel_t i = EdkDll.EE_DataChannel_t.AF3;
                             i <= EdkDll.EE_DataChannel_t.AF4;
                             i++)
                        {
                            getEssentialData("D://GitRepos//Emotyper//Emotyper//A",
                                             "D://GitRepos//Emotyper//Emotyper//Extracted//AExtracted" +
                                             Enum.GetName(typeof(EdkDll.EE_DataChannel_t), i), SampleLength, (int)i,
                                             true);
                        }
                    });
                var task2 = Task.Factory.StartNew(() =>
                    {
                        for (EdkDll.EE_DataChannel_t i = EdkDll.EE_DataChannel_t.AF3;
                             i <= EdkDll.EE_DataChannel_t.AF4;
                             i++)
                        {
                            getEssentialData("D://GitRepos//Emotyper//Emotyper//B",
                                             "D://GitRepos//Emotyper//Emotyper//Extracted//BExtracted" +
                                             Enum.GetName(typeof(EdkDll.EE_DataChannel_t), i), SampleLength, (int)i,
                                             true);
                        }
                    });
                var task3 = Task.Factory.StartNew(() =>
                    {
                        for (EdkDll.EE_DataChannel_t i = EdkDll.EE_DataChannel_t.AF3;
                             i <= EdkDll.EE_DataChannel_t.AF4;
                             i++)
                        {
                            getEssentialData("D://GitRepos//Emotyper//Emotyper//C",
                                             "D://GitRepos//Emotyper//Emotyper//Extracted//CExtracted" +
                                             Enum.GetName(typeof(EdkDll.EE_DataChannel_t), i), SampleLength, (int)i,
                                             true);
                        }
                    });
                Task.WaitAll(task1, task2, task3);
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
        public static List<List<double>> getEssentialData(String sourceSamplesDirestory, string targetSamplesDirectory, int FrameSize, int sensor, bool writefiles = false)
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

                // double[] ser =serie.ToArray();
                //double[] spectrum =FourierTransform.Spectrum(ref ser);
                // double[] mel = MFCC.compute(ref ser);
                // rawSeries.Add(new List<double>(spectrum)); 
                //  rawSeries.Add(ProcessSingleSeries(serie));
            }
            // return rawSeries;
            List<double> smaller;
            List<double> bigger;
            int startIndex1 = 3;
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
                    double correl = Calculator.DistanceFunc(smaller.GetRange(j, FrameSize).ToArray(), bigger.GetRange(i, FrameSize).ToArray());
                    //  if (Math.Abs(correl) > 0.6)
                    pairCorrs.Add(new Tuple<int, int, int, int, double>(0, 0, j, i, Math.Abs(correl)));
                }
            pairCorrs.Sort((a, b) => a.Item5.CompareTo(b.Item5));
            Tuple<int, int, int, int, double> tuple = pairCorrs.First();
            resultSet.Add(smaller.GetRange(tuple.Item3, FrameSize));
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
                        double correl = Calculator.DistanceFunc(list.ToArray(), rawSeries[i].GetRange(j, FrameSize).ToArray());
                        midleCorrel += Math.Abs(correl);
                    }
                    //    double correl = Distance(resultSet.Last().ToArray(), rawSeries[i].GetRange(j, FrameSize).ToArray());
                    midleCorrel = midleCorrel / resultSet.Count;
                    //   if (Math.Abs(midleCorrel) > 0.4)
                    pairCorrs.Add(Tuple.Create(0, 0, i, j, Math.Abs(midleCorrel)));
                }
                pairCorrs.Sort((a, b) => a.Item5.CompareTo(b.Item5));
                if (pairCorrs.Count > 0)
                {
                    tuple = pairCorrs.First();
                    // tuple = pairCorrs.Last();
                    resultSet.Add(rawSeries[i].GetRange(tuple.Item4, FrameSize));
                    if (writefiles)
                    {
                        CsvRow row = new CsvRow();
                        StringBuilder s = new StringBuilder();
                        foreach (double val in resultSet.First())
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
            }
            return resultSet;
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
    }
}
