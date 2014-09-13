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
        List<Chart> charts = new List<Chart>{ AF3Chart,F7Chart,F3Chart,FC5Chart,T7Chart,P7Chart,O1Chart,O2Chart,P8Chart,T8Chart,FC6Chart,
      F4Chart,F8Chart,AF4Chart};
        DataTable table = CSVReader.ReadCSVFile("D://GitRepos//Emotyper//Emotyper//A//Sample0.csv", true);

        for (int i = 3; i < 17; i++)
        {
            ObservableCollection<KeyValuePair<double, double>> valueList = new ObservableCollection<KeyValuePair<double, double>>();
           // String sensorName = table.Columns[i].ColumnName;
            for (int j = 0; j < table.Rows.Count; j++)
            {
                DataRow row = table.Rows[j];
                double time;
                Double.TryParse(row[19].ToString(),out time);
                double val;
                Double.TryParse(row[i].ToString(), out val);
                valueList.Add(new KeyValuePair<double, double>(time, val));          
            }
            
            charts[i - 3].DataContext = valueList;
        }
    

      //valueList.Add(new KeyValuePair<string, int>("Developer",60));
      //valueList.Add(new KeyValuePair<string, int>("Misc", 20));
      //valueList.Add(new KeyValuePair<string, int>("Tester", 50));
      //valueList.Add(new KeyValuePair<string, int>("QA", 30));
      //valueList.Add(new KeyValuePair<string, int>("Project Manager", 40));
        //int[] arr = valueList.Select(x => x.Value).ToArray();
      //Setting data for column chart
      //columnChart.DataContext = valueList;
      //// Setting data for pie chart
      //pieChart.DataContext = valueList;
      ////Setting data for area chart
      //areaChart.DataContext = valueList;
      ////Setting data for bar chart
      //barChart.DataContext = valueList;
      //Setting data for line chart

      //AF3Chart.DataContext = valueList;
      //F7Chart.DataContext = valueList;
      //F3Chart.DataContext = valueList;
      //FC5Chart.DataContext = valueList;
      //T7Chart.DataContext = valueList;
      //P7Chart.DataContext = valueList;
      //O1Chart.DataContext = valueList;
      //O2Chart.DataContext = valueList;
      //P8Chart.DataContext = valueList;
      //T8Chart.DataContext = valueList;
      //FC6Chart.DataContext = valueList;
      //F4Chart.DataContext = valueList;
      //F8Chart.DataContext = valueList;
      //AF4Chart.DataContext = valueList;
    }

  }
}
