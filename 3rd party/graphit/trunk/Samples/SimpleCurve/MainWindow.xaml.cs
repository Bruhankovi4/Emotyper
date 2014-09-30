using System;
using System.Windows.Media;
using GraphIT.Hv;
using GraphIT.Hv.Vertical;
using Helper;

namespace SimpleCurve
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		public MainWindow()
		{
			Loaded += (s, e) => new InfoControl(this).Show();
			SplashScreenHelper.Show();
			InitializeComponent();
			SplashScreenHelper.Close();

			// Set the bounds of the time axis
			CurveDisplay.TimeDoublePlottingSystem.TimeAxis.SetBounds(
				DateTime.UtcNow.AddMinutes(-50),
				DateTime.UtcNow.AddMinutes(10));

			// Create a new curve
			var curve = CurveDisplay.TimeDoublePlottingSystem.AddCurve(
				"kg", -100, 100, Colors.Yellow, RedrawTime.Ms500, AxisMatchingMode.None, 
				CurveDrawingMode.SimpleLine(),
				new FloatingCommaValueFormater(),
				new InterpolationValueFetchStrategy(),
				AxisFormat.Double,
				int.MaxValue);

			// Create some data and add it to the curve
			var data = EntryGenerator.Generatedata(
				90,
				0,
				DateTime.UtcNow.AddMinutes(-40),
				TimeSpan.FromMinutes(40),
				2000);
			curve.DataSeries.Append(data);
		}
	}
}
