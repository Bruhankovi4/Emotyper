using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Statistics.Distributions.Univariate;


namespace DataExtractor
{
    class CombinationEnumerator
    {
        private int[] restrictions;
       // public int[] Position { get; private set; }
        public double[] Position { get; private set; }
        private int length;
        public int Step { get; set; }

        public CombinationEnumerator(int[] _restrictions)
        {
            restrictions = _restrictions;
            length = restrictions.Length;
            Position = new double[length];
            Step = 1;
        }
        public bool MoveNext()
        {
            int pos = length;
            do
            {
                pos--;
                if (pos < 0)
                    return false;
            } while (Position[pos] >= restrictions[pos]);
            Position[pos] += Step;
            if (Position[pos] > restrictions[pos])
                Position[pos] = restrictions[pos];
            for (int i = pos+1; i < length; i++)
            {
                Position[i]=0;
            }
            return true;
        }
    }
}
