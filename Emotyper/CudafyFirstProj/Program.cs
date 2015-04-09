using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CudaCalculation;
using Cudafy;
using Cudafy.Translator;

namespace CudafyFirstProj
{
    class Program
    {
        static void Main(string[] args)
        {
            CudafyModes.Target = eGPUType.Cuda;
            CudafyModes.DeviceId = 0;
            CudafyTranslator.Language = CudafyModes.Target == eGPUType.OpenCL ? eLanguage.OpenCL : eLanguage.Cuda;
            CudafyClassHelper.TestFunction(1000);
        }
    }
}
