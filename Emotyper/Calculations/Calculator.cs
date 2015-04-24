using System;
using System.Collections.Generic;
using System.Linq;
using CudaCalculation;
using NDtw;
using SimpleDTW = CudaCalculation.SimpleDTW;

namespace Calculations
{
    public class DistanceCalculator
    {
        public static double DistanceEuclid(IEnumerable<double> vector1, IEnumerable<double> vector2)
        {                       
            double value = 0;            
            for (int i = 0; i < vector1.Count(); i++)
            {
                value += Math.Pow((vector1.ElementAt(i) - vector2.ElementAt(i)), 2);
            }
            return Math.Sqrt(value);
        }

        public static double DistancePearson(IEnumerable<double> vector1, IEnumerable<double> vector2)
        {
            return 1 / Math.Abs(dnAnalytics.Statistics.Correlation.Pearson(vector1, vector2));
        }
       
        public static double KoiffPearson(IEnumerable<double> vector1, IEnumerable<double> vector2)
        {
            return dnAnalytics.Statistics.Correlation.Pearson(vector1, vector2);
        }
       
        public static double DistanceFunc(IEnumerable<double> vector1, IEnumerable<double> vector2)
        {
            Dtw analyser = new Dtw(vector1.ToArray(), vector2.ToArray(), DistanceMeasure.SquaredEuclidean);
            return analyser.GetCost();
        }                          
    }
}
