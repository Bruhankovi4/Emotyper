using System.Windows.Controls.DataVisualization.Charting;
using Com.StellmanGreene.CSVReader;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WaveletStudio.Blocks;

namespace WpfToolkitChart
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      InitializeComponent();
      showColumnChart();
    }
    
    private void showColumnChart()
    {
        List<Chart> charts = new List<Chart>{ AF3Chart,F7Chart,F3Chart,FC5Chart,T7Chart,P7Chart,O1Chart,O2Chart,P8Chart,T8Chart,FC6Chart,F4Chart,F8Chart,AF4Chart};
        List<Chart> chartsPrime = new List<Chart> { AF3Chart_Copy, F7Chart_Copy, F3Chart_Copy, FC5Chart_Copy, T7Chart_Copy, P7Chart_Copy, O1Chart_Copy, O2Chart_Copy, P8Chart_Copy, T8Chart_Copy, FC6Chart_Copy, F4Chart_Copy, F8Chart_Copy, AF4Chart_Copy };
        DataTable table = CSVReader.ReadCSVFile("D://GitRepos//Emotyper//Emotyper//A//Sample0.csv", true);

        for (int i = 3; i < 17; i++)
        {
            ObservableCollection<KeyValuePair<double, double>> valueList = new ObservableCollection<KeyValuePair<double, double>>();
            ObservableCollection<KeyValuePair<double, double>> valueListPrime = new ObservableCollection<KeyValuePair<double, double>>();

            List<double> serie = new List<double>();
            List<double> timespan = new List<double>();
           // String sensorName = table.Columns[i].ColumnName;
            for (int j = 0; j < table.Rows.Count; j++)
            {
                DataRow row = table.Rows[j];
                double time;
                Double.TryParse(row[19].ToString(),out time);
                double val;
                Double.TryParse(row[i].ToString(), out val);                
                serie.Add(val);
                timespan.Add(time);
            }
            List<double> seriePrime = processOneSeries(serie);
           
            for (int k = 0; k < timespan.Count; k++)
            {                  
                valueList.Add(new KeyValuePair<double, double>(timespan[k],serie[k]));
               
            }
            for (int k = 0; k < seriePrime.Count; k++)
            {
                valueListPrime.Add(new KeyValuePair<double, double>(timespan[k], seriePrime[k]));
            }
            charts[i - 3].DataContext = valueList;
            chartsPrime[i - 3].DataContext = valueListPrime;
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

  }
}
