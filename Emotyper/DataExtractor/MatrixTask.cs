using System.Collections.Generic;
using System.Linq;
using Emotiv;
using ParticleSwarm;
namespace DataExtractor
{
    class MatrixTask : Task
    {
        private readonly List<Dictionary<EdkDll.EE_DataChannel_t, List<double>>> matrixes;
        private int _winsize = 128;

        public MatrixTask(double[] minvalues, double[] maxvalues, List<Dictionary<EdkDll.EE_DataChannel_t, List<double>>> rawData, int winsize)
            : base(minvalues, maxvalues)
        {
            matrixes = rawData;
            _winsize = winsize;
        }

        public override double FinalFunction(double[] position)
        {
            double[] corell = Extractor.matrixCorrelation(matrixes, position, _winsize);
            return corell.Average();
        }
    }
}
