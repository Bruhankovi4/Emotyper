

using System.Windows.Controls.DataVisualization.Charting;
using System.Windows.Threading;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using CsvReader;
using Emotiv;
using EmotyperDataUtility;
using WaveletStudio.Blocks;
using System.Timers;
namespace WpfToolkitChart
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
      public static List<ObservableCollection<KeyValuePair<double, double>>> dataContexts = new List<ObservableCollection<KeyValuePair<double, double>>>();
      public static List<ObservableCollection<KeyValuePair<double, double>>> dataContextsPrime = new List<ObservableCollection<KeyValuePair<double, double>>>();
      private List<Chart> charts;
      private List<Chart> chartsPrime;
      static Timer _timer;

    public MainWindow()
    {
      InitializeComponent();  
        EmotypeEventSource source = new EmotypeEventSource();
       // source.fireDictionary = false;
        _timer = new Timer(1000);
        _timer.Elapsed += new ElapsedEventHandler(_timer_Elapsed);
        _timer.Enabled = true;
        initContexts();
      source.OnDataArrived += OnDataArrived;
     // showColumnChart();
        source.Start();
    }

      private void _timer_Elapsed(object sender, ElapsedEventArgs e)
      {
          Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>cropContexts()));
      }

      private void cropContexts()
      {
          int length;
          for (int i = 0; i < charts.Count; i++)
          {
              List<KeyValuePair<double, double>> temp = new List<KeyValuePair<double, double>>();
              length = dataContexts[i].Count;
              if (length > 200)
              {
                  lock (dataContexts[i])
                  {
                                                  
                     dataContexts[i] = new ObservableCollection<KeyValuePair<double, double>>(dataContexts[i].ToList().GetRange(length-201,200));
                      charts[i].DataContext = dataContexts[i];                  
                   }
              }
              length = dataContextsPrime[i].Count;
              if (length > 200)
              {
                  lock (dataContextsPrime[i])
                  {
                      dataContextsPrime[i] = new ObservableCollection<KeyValuePair<double, double>>(dataContextsPrime[i].ToList().GetRange(length - 201, 200));
                      chartsPrime[i].DataContext = dataContextsPrime[i];
                  }

              }
          }
      }


      private void initContexts()
    {
        charts = new List<Chart> { AF3Chart, F7Chart, F3Chart, FC5Chart, T7Chart, P7Chart, O1Chart, O2Chart, P8Chart, T8Chart, FC6Chart, F4Chart, F8Chart, AF4Chart };
        chartsPrime = new List<Chart> { AF3Chart_Copy, F7Chart_Copy, F3Chart_Copy, FC5Chart_Copy, T7Chart_Copy, P7Chart_Copy, O1Chart_Copy, O2Chart_Copy, P8Chart_Copy, T8Chart_Copy, FC6Chart_Copy, F4Chart_Copy, F8Chart_Copy, AF4Chart_Copy };
       
        for (int i = 0; i < charts.Count(); i++)
        {
            ObservableCollection<KeyValuePair<double, double>> context = new ObservableCollection<KeyValuePair<double, double>>(); 
            dataContexts.Add(context);
            charts[i].DataContext = context;
        }
        for (int i = 0; i < chartsPrime.Count(); i++)
        {
            ObservableCollection<KeyValuePair<double, double>> context = new ObservableCollection<KeyValuePair<double, double>>();
            dataContextsPrime.Add(context);
            chartsPrime[i].DataContext =context;
        }
        
    }
     void addPointToContext(int index, KeyValuePair<double, double> pair)
     {
         try
         {
              dataContexts[index].Add(pair);
         }
         catch (Exception)
         {
         }
        
     }
     void addPointsToContext(int index, List<double> time, List<double> vals)
     {
         List< KeyValuePair<double,double>> temp = new List<KeyValuePair<double, double>>(); 
         for (int i = 0; i < vals.Count; i++)
         {
             temp.Add(new KeyValuePair<double, double>(time[i], vals[i]));
         }
         try
         {
             dataContexts[index]=new ObservableCollection<KeyValuePair<double, double>>(dataContexts[index].Concat(temp));
            charts[index].DataContext = dataContexts[index];

         }
         catch (Exception)
         {
         }

     }
     void addPointToContextPrime(int index, KeyValuePair<double, double> pair)
     {
         try
         {
           dataContextsPrime[index].Add(pair);
         }
         catch (Exception)
         {
         }
         
     }

      void OnDataArrived(object sender, EmoEventDictionary e)
    {
       
        Dictionary<EdkDll.EE_DataChannel_t, double[]> data = e.Dictionary;
        if (data==null)
            return;
       //foreach (EdkDll.EE_DataChannel_t channel in data.Keys)
        for (int k = 3; k < 16;k++ )
        {
            double[] timestamp = data[EdkDll.EE_DataChannel_t.TIMESTAMP];
            //data[channel].Add(data[channel][i]);
            // int k = 2;
            //if (k > 17)
            //    break;
            // k++;
            //ObservableCollection<KeyValuePair<double, double>> valueList = new ObservableCollection<KeyValuePair<double, double>>();
            //ObservableCollection<KeyValuePair<double, double>> valueListPrime = new ObservableCollection<KeyValuePair<double, double>>();

            List<double> serie = new List<double>(data[data.Keys.ElementAt(k)]);
            //List<double> seriePrime = processOneSeries(serie);

           
                //dataContexts[0].Add(new KeyValuePair<double, double>(timestamp[j], serie[j]));
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(() => addPointsToContext(k - 3, new List<double>(timestamp), serie)));
            
            //for (int j = 0; j < seriePrime.Count - 1; j++)
            //{
            //    // dataContextsPrime[k-3].Add(new KeyValuePair<double, double>(timestamp[j], seriePrime[j]));
            //    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => addPointToContextPrime(k - 3, new KeyValuePair<double, double>(timestamp[j], seriePrime[j]))));

            //}
        }   
  }
    
    private void showColumnChart()
    {
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
    public static List<double> processOneSeries(List<double> serie)
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

    private void Button_Click(object sender, RoutedEventArgs e)
    {

        //AF3Chart.DataContext=new List<KeyValuePair<double, double>>() { new KeyValuePair<double, double>(2, 2), new KeyValuePair<double, double>(3, 4) }; 
       // dataContexts[0].Add(new KeyValuePair<double, double>(2, 2));
      //  dataContexts[0].Add(new KeyValuePair<double, double>(3,4));
       // invalidateWindow();
    }

  }
}
