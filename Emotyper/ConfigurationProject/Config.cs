using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Calculations;

namespace Configuration
{
    public static class Config
    {
        public static int SampleLength = 40;
        public static readonly string mapPath = "D://GitRepos//Emotyper//Emotyper//SOMInstancesMFCC//{0}Map.som";

        public static readonly string coordsPath =
            "D://GitRepos//Emotyper//Emotyper//SOMCoordinates//{0}Coordinates.csv";
        public static string SamplesSourceDirectory = "D://GitRepos//Emotyper//Emotyper";
        public static string SamplesExtractedDirectory = "D://GitRepos//Emotyper//Emotyper//ExtractedMatrix//{0}";
        public static readonly string[] sampleNames = new[] { "A", "B", "C"};

        /*--------------------Classification Configuration-------------------------*/
        public static int ClassifierWindowHeight = 6;
        public static double ClassifierSensorThreshold = 0.3; //if sensor have more than 30% good values than i'll use it
        public static double ClassifierSensorMatrixThreshold = 0.3;
        /*--------------------Extraction Configuration----------------------------*/
        public static int swarmSize = 500;
        public static double currentVelocityRatio = 0.3;
        public static double localVelocityRatio = 2;
        public static double globalVelocityRatio = 5;
        public static List<Func<List<double>, List<double>>> ExtractionTransformation = new List<Func<List<double>, List<double>>>();

        /*------------------------Map Configuration-------------------------------*/
        public static int MapLength = 100;
        public static double trainingLimit = 0.00001;
        public static bool loadMapInstance = false;
        
        /*----------------Data Transformation Configuration ----------------------*/
        public static int MFCCFrameSize = 32;
        public static double MFCCStartFreq = 9.1089;
        public static double MFCCEndFreq = 40;
        public static int MFCCFilterBanksCount = 40;
        /*----------------------Emotiv Configuration --------------------------*/
        public static float emotiveBufferSize = 1;
        public static int EmulatedSamplesCount = SampleLength + 2;
        public static int EmulationPeriod = 1000;
        public static string EmulationDirectory = @"D:\GitRepos\Emotyper\Emotyper\{0}test";

        public static string EmulationSample = @"C";
        /*----------------------Global Configuration --------------------------*/
        static Func<IEnumerable<double>, IEnumerable<double>, double> Distance =
            delegate(IEnumerable<double> vector1, IEnumerable<double> vector2)
            { return DistanceCalculator.DistancePearson(vector1, vector2); };

        public static List<Func<List<double>, List<double>>> InputDataTransformation = new List<Func<List<double>, List<double>>>();

        public static void InitConfig()
        {
            InputDataTransformation.Add(DataTransformations.MFCC);

           // ExtractionTransformation.Add(DataTransformations.MFCC);
        }
    }
}
