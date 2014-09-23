using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Emotiv;
using EmotyperDataUtility;

namespace UIRecorder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private FileStorageWriter filewriter;
        private bool isWriting = false;
        private int i = 0;
        EmotypeEventSource source = new EmotypeEventSource();

        public MainWindow()
        {
            InitializeComponent();
            filewriter = new FileStorageWriter("D:\\GitRepos\\Emotyper\\Emotyper");             
            source.OnRowsArrived += OnRowsArrived;
            source.OnDataArrived += OnDataArrived;
        }

        private void recordButton_Click(object sender, RoutedEventArgs e)
        {
            //if (!isWriting)
            //{
            //    Console.WriteLine("Writing started");
            //    recordButton.Content = "Stop";
            //    filewriter.StartWritingToFile(folderPath.Text, filenameTextbox.Text + i++);
            //}
            //else
            //{
            //    filewriter.StopWriting();
            //    Console.WriteLine("Writing ended");
            //    recordButton.Content = "Record";
            //}
            //isWriting = !isWriting;
            if (!isWriting)
            {                
                source.Start();
                recordButton.Content = "Stop";
            }
            else
            {
                source.Stop();
                recordButton.Content = "Record";
            }
            isWriting = !isWriting;
        }

        void OnRowsArrived(object sender, EmoEventRows e)
        {
            foreach (var row in e.Rows)
            {
                var line = String.Join(" ", row);
                Console.WriteLine(line);
            }
        }
        void OnDataArrived(object sender, EmoEventDictionary e)
        {
            Dictionary<EdkDll.EE_DataChannel_t, double[]> data = e.Dictionary;
            int bufferSize = data[EdkDll.EE_DataChannel_t.TIMESTAMP].Length;
            for (int i = 0; i < bufferSize; i++)
                    {
                        List<double> row = new List<double>(data.Keys.Select(channel => data[channel][i]));
                        var line = String.Join(" ", row);
                        Console.WriteLine(line);
                    }
        }
         
    }

    
}

   
