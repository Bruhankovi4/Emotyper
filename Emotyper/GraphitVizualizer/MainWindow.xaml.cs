using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using Emotiv;
using EmotyperDataUtility;
using TechNewLogic.GraphIT;
using TechNewLogic.GraphIT.Hv;
using TechNewLogic.GraphIT.Hv.Legend;
using TechNewLogic.GraphIT.Hv.Vertical;
using WaveletStudio.Blocks;

namespace GraphitVizualizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow 
    {
       static DateTime time = DateTime.UtcNow;
        public MainWindow()
        {
            EmotypeEventSource source = new EmotypeEventSource();
            source.OnDataArrived += OnDataArrived;
            source.Start();
            InitializeComponent();

            // Set the bounds of the time axis
            RawDisplay.TimeDoublePlottingSystem.TimeAxis.SetBounds(
                DateTime.UtcNow,
                DateTime.UtcNow.AddMinutes(2));

            TransformedDisplay.TimeDoublePlottingSystem.TimeAxis.SetBounds(
         DateTime.UtcNow,
         DateTime.UtcNow.AddMinutes(2));  

            //var plottingSystem = RawDisplay.TimeDoublePlottingSystem;
            //plottingSystem.EnableOnlineMode(DateTime.UtcNow);
            //plottingSystem.TimeAxis.SetBounds(
            //    plottingSystem.TimeAxis.ActualUpperBound.AddSeconds(-5),
            //    plottingSystem.TimeAxis.ActualUpperBound);     
            CreateCurves();
            RawDisplay.TimeDoublePlottingSystem.SetScalesVisibility(false, false, false);
            TransformedDisplay.TimeDoublePlottingSystem.SetScalesVisibility(false, false, false);
        }

        private List<TimeDoubleCurve> rawCurves = new List<TimeDoubleCurve>();
        private List<TimeDoubleCurve> transformedCurves = new List<TimeDoubleCurve>(); 

        private List<Color> cols = new List<Color>(){Colors.Red, Colors.Green, Colors.Blue, Colors.SeaShell, Colors.DeepPink, Colors.DimGray, Colors.Brown, Colors.BlueViolet, Colors.Chartreuse, Colors.Cyan, Colors.Magenta, Colors.CornflowerBlue, Colors.Salmon, Colors.SpringGreen};
        private void CreateCurves()
        {
            for (EdkDll.EE_DataChannel_t i = EdkDll.EE_DataChannel_t.AF3; i <= EdkDll.EE_DataChannel_t.AF4; i++)
            {
                TimeDoubleCurve _curve = RawDisplay.TimeDoublePlottingSystem.AddCurve(
             i.ToString(), 1000, ((int)i)*1000, cols[(int)i-3], RedrawTime.Ms500, AxisMatchingMode.None,
             CurveDrawingMode.SimpleLine(),
             new FloatingCommaValueFormater(),
             new InterpolationValueFetchStrategy(),
             AxisFormat.Double, 
             15000);             
                rawCurves.Add(_curve);
            }

            for (EdkDll.EE_DataChannel_t i = EdkDll.EE_DataChannel_t.AF3; i <= EdkDll.EE_DataChannel_t.AF4; i++)
            {
                TimeDoubleCurve _curve = TransformedDisplay.TimeDoublePlottingSystem.AddCurve(
             i.ToString()+"prime", 0, ((int)i) * 1000, cols[(int)i - 3], RedrawTime.Ms500, AxisMatchingMode.None,
             CurveDrawingMode.SimpleLine(),
             new FloatingCommaValueFormater(),
             new InterpolationValueFetchStrategy(),
             AxisFormat.Double,
             15000);
                transformedCurves.Add(_curve);
            }
              
        }
        void OnDataArrived(object sender, EmoEventDictionary e)
        {

            Dictionary<EdkDll.EE_DataChannel_t, double[]> data = e.Dictionary;
            if (data == null)
                return;
            //foreach (EdkDll.EE_DataChannel_t channel in data.Keys)  
            double[] timestamp = data[EdkDll.EE_DataChannel_t.TIMESTAMP];
            for (EdkDll.EE_DataChannel_t k = EdkDll.EE_DataChannel_t.AF3; k <= EdkDll.EE_DataChannel_t.AF4; k++)
            {

                List<double> serie = new List<double>(data[k]);
                List<double> seriePrime = processOneSeries(serie);

                List<TimeDoubleDataEntry> values = new List<TimeDoubleDataEntry>();
                List<TimeDoubleDataEntry> valuesPrime = new List<TimeDoubleDataEntry>();

                if (rawCurves == null || rawCurves.Count < 14 || transformedCurves == null || transformedCurves.Count < 14)
                    return;

                var curve = rawCurves[(int) k - 3];
                var curvePrime = transformedCurves[(int)k - 3];

                DateTime time = DateTime.UtcNow;
            for (int i = 0; i < timestamp.Length; i++)
            {
                values.Add(new TimeDoubleDataEntry(time.AddSeconds(timestamp[i]), serie[i]));
            }
            for (int j = 0; j < seriePrime.Count; j++)
            {
                valuesPrime.Add(new TimeDoubleDataEntry(time.AddSeconds(timestamp[j]), seriePrime[j]));
                Console.WriteLine(time.AddMilliseconds(timestamp[j]*1000));
            }
            if (curve != null)
            {
                curve.DataSeries.Append(values);
                curve.DataSeries.RemoveLeft(DateTime.UtcNow.AddSeconds(-3));
            }
            if (curvePrime != null)
            {
                curvePrime.DataSeries.Append(valuesPrime);
                curvePrime.DataSeries.RemoveLeft(time.AddSeconds(-3));
            }

            }
        }
        private List<double> processOneSeries(List<double> serie)
        {

            //Declaring the blocks
            var inputSeriesBlock = new InputSeriesBlock();
            inputSeriesBlock.SetSeries(serie);
            var dWTBlock = new DWTBlock
            {
                WaveletName = "Daubechies 10 (db10)",
                Level = 1,
                Rescale = false,
                ExtensionMode = WaveletStudio.SignalExtension.ExtensionMode.AntisymmetricWholePoint
            };
            var outputSeriesBlock = new OutputSeriesBlock();

            //Connecting the blocks
            inputSeriesBlock.OutputNodes[0].ConnectTo(dWTBlock.InputNodes[0]);
            dWTBlock.OutputNodes[1].ConnectTo(outputSeriesBlock.InputNodes[0]);

            //Appending the blocks to a block list and execute all
            var blockList = new BlockList();
            blockList.Add(inputSeriesBlock);
            blockList.Add(dWTBlock);
            blockList.Add(outputSeriesBlock);
            blockList.ExecuteAll();
            return outputSeriesBlock.GetSeries();
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            Environment.Exit(Environment.ExitCode);
        }       
	
    }
}
