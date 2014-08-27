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
        public MainWindow()
        {
           // filewriter = new FileStorageWriter("D://GitRepos//Emotyper//Emotyper");
            InitializeComponent();
        }
        //End Writing to file
        private void Record_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                   Console.WriteLine("End writing");
            } 
        }
        //Start Writing to file
        private void Record_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                Console.WriteLine("Start writing");
            }        
        }
         //Start Writing to file
        private void recordButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine("Start writing");
        }
        //End Writing to file
        private void recordButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine("End writing");
        }
    }
}
