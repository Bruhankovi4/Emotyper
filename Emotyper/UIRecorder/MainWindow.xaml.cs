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
using EmotyperStorageUtility;

namespace UIRecorder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private FileStorageWriter filewriter;
        private bool isWriting= false;
        private int i = 0;
        public MainWindow()
        {
          
            InitializeComponent(); 
            filewriter = new FileStorageWriter("D://GitRepos//Emotyper//Emotyper");
        }

        private void recordButton_Click(object sender, RoutedEventArgs e)
        {
           
            if (!isWriting)
            {
                Console.WriteLine("Writing started");
                recordButton.Content = "Stop";
                 filewriter.StartWritingToFile("A","sample"+i++);

            }
            else
            {
                filewriter.StopWriting();
                Console.WriteLine("Writing ended");
                recordButton.Content = "Record";
            }
            isWriting = !isWriting;
        }
     
    }
}
