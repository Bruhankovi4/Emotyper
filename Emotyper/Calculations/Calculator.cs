using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NDtw;

namespace Calculations
{
    public class Calculator
    {
        public static double DistanceEuclid(double[] vector1, double[] vector2)
        {

            double value = 0;
            for (int i = 0; i < vector1.Length; i++)
            {
                value += Math.Pow((vector1[i] - vector2[i]), 2);
            }
            return Math.Sqrt(value);
        }

        public static double DistancePearson(IEnumerable<double> vector1, IEnumerable<double> vector2)
        {
            return 1 / Math.Abs(dnAnalytics.Statistics.Correlation.Pearson(vector1, vector2));
        }
        public static double MeasurePearson(IEnumerable<double> vector1, IEnumerable<double> vector2)
        {
            return dnAnalytics.Statistics.Correlation.Pearson(vector1, vector2);
        }
        public static double DistanceFunc(double[] vector1, double[] vector2)
        {
            Dtw analyser = new Dtw(vector1, vector2, DistanceMeasure.Manhattan);
            return analyser.GetCost();
        }
    }
}
