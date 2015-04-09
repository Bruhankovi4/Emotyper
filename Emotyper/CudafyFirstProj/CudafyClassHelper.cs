using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cudafy;
using Cudafy.Host;
using Cudafy.Translator;
namespace CudaCalculation
{

    public class SimpleDTW
    {
        private double[] x;
        private double[] y;
        private double[,] distance;
        private double[,] f;
        //ArrayList pathX;
        //ArrayList pathY;
        //ArrayList distanceList;
        private double sum;

        public SimpleDTW(double[] _x, double[] _y)
        {
            x = _x;
            y = _y;
            distance = new double[x.Length, y.Length];
            f = new double[x.Length + 1, y.Length + 1];

            for (int i = 0; i < x.Length; ++i)
            {
                for (int j = 0; j < y.Length; ++j)
                {
                    distance[i, j] = Math.Abs(x[i] - y[j]);
                }
            }

            for (int i = 0; i <= x.Length; ++i)
            {
                for (int j = 0; j <= y.Length; ++j)
                {
                    f[i, j] = -1.0;
                }
            }

            for (int i = 1; i <= x.Length; ++i)
            {
                f[i, 0] = double.PositiveInfinity;
            }
            for (int j = 1; j <= y.Length; ++j)
            {
                f[0, j] = double.PositiveInfinity;
            }

            f[0, 0] = 0.0;
            sum = 0.0;

            //pathX = new ArrayList();
            //pathY = new ArrayList();
            //distanceList = new ArrayList();
        }

        //public ArrayList getPathX()
        //{
        //    return pathX;
        //}

        //public ArrayList getPathY()
        //{
        //    return pathY;
        //}

        public double getSum()
        {
            return sum;
        }

        public double[,] getFMatrix()
        {
            return f;
        }

        //public ArrayList getDistanceList()
        //{
        //    return distanceList;
        //}

        public void computeDTW()
        {
            //sum = computeFBackward(x.Length, y.Length);
            sum = computeFForward();
        }

        public double computeFForward()
        {
            for (int i = 1; i <= x.Length; ++i)
            {
                for (int j = 1; j <= y.Length; ++j)
                {
                    if (f[i - 1, j] <= f[i - 1, j - 1] && f[i - 1, j] <= f[i, j - 1])
                    {
                        f[i, j] = distance[i - 1, j - 1] + f[i - 1, j];
                    }
                    else if (f[i, j - 1] <= f[i - 1, j - 1] && f[i, j - 1] <= f[i - 1, j])
                    {
                        f[i, j] = distance[i - 1, j - 1] + f[i, j - 1];
                    }
                    else if (f[i - 1, j - 1] <= f[i, j - 1] && f[i - 1, j - 1] <= f[i - 1, j])
                    {
                        f[i, j] = distance[i - 1, j - 1] + f[i - 1, j - 1];
                    }
                }
            }
            return f[x.Length, y.Length];
        }

        public double computeFBackward(int i, int j)
        {
            if (!(f[i, j] < 0.0))
            {
                return f[i, j];
            }
            else
            {
                if (computeFBackward(i - 1, j) <= computeFBackward(i, j - 1) &&
                    computeFBackward(i - 1, j) <= computeFBackward(i - 1, j - 1)
                    && computeFBackward(i - 1, j) < double.PositiveInfinity)
                {
                    f[i, j] = distance[i - 1, j - 1] + computeFBackward(i - 1, j);
                }
                else if (computeFBackward(i, j - 1) <= computeFBackward(i - 1, j) &&
                         computeFBackward(i, j - 1) <= computeFBackward(i - 1, j - 1)
                         && computeFBackward(i, j - 1) < double.PositiveInfinity)
                {
                    f[i, j] = distance[i - 1, j - 1] + computeFBackward(i, j - 1);
                }
                else if (computeFBackward(i - 1, j - 1) <= computeFBackward(i - 1, j) &&
                         computeFBackward(i - 1, j - 1) <= computeFBackward(i, j - 1)
                         && computeFBackward(i - 1, j - 1) < double.PositiveInfinity)
                {
                    f[i, j] = distance[i - 1, j - 1] + computeFBackward(i - 1, j - 1);
                }
            }
            return f[i, j];
        }
    }

    public class CudafyClassHelper
    {
       
        
        private static GPGPU gpu = CudafyHost.GetDevice(CudafyModes.Target, 0);
        static eArchitecture arch = gpu.GetArchitecture();
       private static CudafyModule km = CudafyTranslator.Cudafy(arch);
        public static double DTW(double[] _x, double[] _y)
        {
           // gpu.EnableMultithreading();
           // gpu.EnableSmartCopy();
            CudafyModes.Target = eGPUType.Cuda;
            CudafyTranslator.AllowClasses = true;
            CudafyTranslator.GenerateDebug = true;
            gpu = CudafyHost.GetDevice(CudafyModes.Target, 0);
            arch = gpu.GetArchitecture();
            km = CudafyTranslator.Cudafy(arch);
           // km = CudafyTranslator.Cudafy(arch);
            // gpu.EnableMultithreading();
          //  gpu.SetCurrentContext();
            gpu.LoadModule(km);
            return CalcDTW(_x, _y);
        }

        public static double CalcDTW(double[] _x, double[] _y)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            SimpleDTW dtw = new SimpleDTW(_x, _y);
            dtw.computeDTW();
            double res = dtw.getSum();
            var dtwF = dtw.getFMatrix();
            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds);
            watch.Restart();

            double[] gpu_x = gpu.CopyToDevice(_x);
            double[] gpu_y = gpu.CopyToDevice(_y);
            double[,] gpu_distance = gpu.Allocate<double>(_x.Length, _y.Length);
            double[,] gpu_f = gpu.Allocate<double>(_x.Length + 1, _y.Length + 1);
            double sum;
            double[,] resultdistance = new double[_x.Length, _y.Length];
            double[,] resultF = new double[_x.Length + 1, _y.Length + 1];
            
            //gpu.Launch(_x.Length, _y.Length).initDistances(gpu_distance, gpu_x, gpu_y);
            //Console.WriteLine(watch.ElapsedMilliseconds);
            //gpu.Launch(new dim3(_x.Length + 1, _y.Length + 1), 1).initF(gpu_f);
            Console.WriteLine(watch.ElapsedMilliseconds);
            Console.WriteLine("-------------------------------------------------------");
            gpu.Launch(new dim3(_x.Length / 32, _y.Length / 32), new dim3(32, 32)).initDistances(gpu_distance, gpu_x, gpu_y);
            Console.WriteLine(watch.ElapsedMilliseconds);
            gpu.Launch(new dim3(_x.Length / 8, _y.Length / 8), new dim3(8, 8)).initDistances(gpu_distance, gpu_x, gpu_y);
            Console.WriteLine(watch.ElapsedMilliseconds);
            Console.WriteLine("-------------------------------------------------------");
            for (int i = 0; i < _x.Length + 1; i++)
            {
                gpu.Launch(new dim3(_x.Length, _y.Length), 1).Calculate(gpu_distance, gpu_f, gpu_x, gpu_y); 
            }
            Console.WriteLine(watch.ElapsedMilliseconds);
             gpu.Synchronize();
            //gpu.CopyFromDevice(gpu_distance, resultdistance);
            //gpu.CopyFromDevice(gpu_f, resultF);
            gpu.FreeAll();
            return resultF[_x.Length, _y.Length];
            //Console.WriteLine(watch.ElapsedMilliseconds);
            //watch.Stop();
            sum = 0.0;
            return sum;
        }
        [Cudafy]
        public static void Calculate(GThread thread, double[,] distance, double[,] f, double[] x, double[] y)
        {
            int i = thread.blockIdx.x * thread.blockDim.x + thread.threadIdx.x+1;
            int j = thread.blockIdx.y * thread.blockDim.y + thread.threadIdx.y+1;

            if (f[i - 1, j] == -1 || f[i - 1, j - 1] == -1 || f[i, j - 1] == -1)
            {
              return;
            }
            if (f[i - 1, j] <= f[i - 1, j - 1] && f[i - 1, j] <= f[i, j - 1])
            {
                f[i, j] = distance[i - 1, j - 1] + f[i - 1, j];
            }
            else if (f[i, j - 1] <= f[i - 1, j - 1] && f[i, j - 1] <= f[i - 1, j])
            {
                f[i, j] = distance[i - 1, j - 1] + f[i, j - 1];
            }
            else if (f[i - 1, j - 1] <= f[i, j - 1] && f[i - 1, j - 1] <= f[i - 1, j])
            {
                f[i, j] = distance[i - 1, j - 1] + f[i - 1, j - 1];
            }
            //return f[x.Length, y.Length];
        }

        [Cudafy]
        public static void initDistances(GThread thread, double[,] distance, double[] x, double[] y)
        {
            int i = thread.blockIdx.x * thread.blockDim.x + thread.threadIdx.x;
            int j = thread.blockIdx.y * thread.blockDim.y + thread.threadIdx.y;
        //    int i = thread.blockIdx.x;
        //    int j = thread.blockIdx.y;
            distance[i, j] = Math.Abs(x[i] - y[j]);

        }
        [Cudafy]
        public static void initF(GThread thread, double[,] f)
        {

            int i = thread.blockIdx.x;
            int j = thread.blockIdx.y;
            f[i, j] = -1.0;
            f[i, 0] = double.PositiveInfinity;
            f[0, j] = double.PositiveInfinity;
            f[0, 0] = 0.0;

        }
        #region Test
        public static void TestFunction(int threads)
        {
            CudafyTranslator.AllowClasses = true;
            //CudafyModule km = CudafyTranslator.Cudafy(new Type[] { typeof(CudafyClassHelper) });
            // GPGPU gpu = CudafyHost.GetDevice(CudafyModes.Target, 0);
            gpu.LoadModule(km);
            Example1(gpu, threads);
        }

        public static void Example1(GPGPU gpu, int threads)
        {
            double[] a = new double[threads];
            double[] b = new double[threads];
            Random r = new Random();
            for (int i = 0; i < threads; i++)
            {
                a[i] = r.NextDouble();
                b[i] = r.NextDouble();
            }

            double[] gpuarr1 = gpu.CopyToDevice(a);
            double[] gpuarr2 = gpu.CopyToDevice(b);

            double[] result = new double[threads];
            var gpuresult = gpu.Allocate<double>(result);

            gpu.Launch(threads, 1).Test2(gpuarr1, gpuarr2, gpuresult);

            gpu.CopyFromDevice(gpuresult, result);
            gpu.FreeAll();
        }

        [Cudafy]
        public static void Test2(GThread thread, double[] arrayView1, double[] arrayView2, double[] result)
        {
            int i = thread.blockIdx.x;
            result[i] = arrayView1[i] + arrayView2[i];
        }

        #endregion


    }
}   
