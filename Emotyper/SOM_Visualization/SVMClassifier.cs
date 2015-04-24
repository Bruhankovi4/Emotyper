using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.MachineLearning.VectorMachines;
using Accord.MachineLearning.VectorMachines.Learning;
using Accord.Statistics.Kernels;
using Calculations;

namespace Classifier
{
    class SVMClassifier
    {
        private double numSigma = 6.2;
        MulticlassSupportVectorMachine ksvm;
        public void init(List<double[]> patterns,List<int> labels)
        {
            IKernel kernel = createKernel(3);
            double complexity = 0.00001;
            double tolerance = 0.2;
            int cacheSize =500;
            SelectionStrategy strategy = SelectionStrategy.Sequential;


            // Create the Multi-class Support Vector Machine using the selected Kernel
            ksvm = new MulticlassSupportVectorMachine(128, kernel, 4);

            
            // Create the learning algorithm using the machine and the training data
            MulticlassSupportVectorLearning ml = new MulticlassSupportVectorLearning(ksvm, patterns.ToArray(), labels.ToArray())
            {
                // Configure the learning algorithm
                Algorithm = (svm, classInputs, classOutputs, i, j) =>

                    // Use Platt's Sequential Minimal Optimization algorithm
                    new SequentialMinimalOptimization(svm, classInputs, classOutputs)
                    {
                        Complexity = complexity,
                        Tolerance = tolerance,
                        CacheSize = cacheSize,
                        Strategy = strategy,
                        Compact = (kernel is Linear)
                    }
            };
            double error = ml.Run();
             Console.WriteLine(error);
        }

        private bool btnClassifyElimination = true;
        public int Classify(double[] series)
        {
            int output;
            if (btnClassifyElimination)
                output = ksvm.Compute(series, MulticlassComputeMethod.Elimination);
            else
                output = ksvm.Compute(series, MulticlassComputeMethod.Voting);

            return output;

        }

        private IKernel createKernel(int KernelNum=1)
        {
            if (KernelNum==1)
                return new Gaussian(numSigma);

            if (KernelNum == 2)
                return new Linear(2);

            return new Polynomial(2,1);
        }
    }
}
