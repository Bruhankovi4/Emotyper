using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Statistics.Analysis;
using WaveletStudio;
using WaveletStudio.Blocks;
using WaveletStudio.Functions;
using DSP;

namespace Calculations
{
    public class DataTransformations
    {
        public static List<double> WaveletTransform(List<double> serie)
        {
            //Declaring the blocks
            var inputSeriesBlock = new InputSeriesBlock();
            inputSeriesBlock.SetSeries(serie);
            var dWTBlock = new DWTBlock
            {
                WaveletName = "coif4",
                Level = 1,
                Rescale = false,
                ExtensionMode = SignalExtension.ExtensionMode.AntisymmetricWholePoint
            };
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
        public static List<double> FFTTransform(List<double> serie)
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
        public static List<double> MFCC(List<double> serie)
        {
            double[] ser = serie.GetRange(0, 128).ToArray();
            double[] spectrum = FourierTransform.Spectrum(ref ser);
            DSP.MFCC.initWithrange(9.1089, 40, 40);
            return DSP.MFCC.compute(ref spectrum).ToList();
        }
        public static double[,] PCA(double[,] sourceMatrix)
        {            
            // Creates the Principal Component Analysis of the given source
            var pca = new PrincipalComponentAnalysis(sourceMatrix, AnalysisMethod.Center);
            // Compute the Principal Component Analysis
            pca.Compute();
            var length1 = sourceMatrix.GetLength(0);
            var length2 = sourceMatrix.GetLength(1);
           // int sercount = rawSeries.Count();
            // Creates a projection considering 80% of the information
           // pca.Transform(sourceMatrix, 0.8f, true);
            return pca.Transform(sourceMatrix, length1);
        }

        public static List<double> Transform(
          List<Func<List<double>, List<double>>> transformationChain,List<double> series )
        {
            foreach (var func in transformationChain)
            {
                series = func.Invoke(series);
            }
            return series;
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
        public static double[,] To2dArray(List<double[]> list)
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
    }
}
