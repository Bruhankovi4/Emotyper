using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows.Forms;
using CsvReader;
using Emotiv;
using ZedGraph;

namespace ZedGraphSample
{
    public partial class GridVisualiser : Form
    {
        private string path = "D://GitRepos//Emotyper//Emotyper//SOMCoordinates//{0}Coordinates.csv";
        private static string gridFolder = @"D:\GitRepos\Emotyper\Emotyper\SOMClassifiers\Classifier.cls";
        private static Dictionary<EdkDll.EE_DataChannel_t, Grid> grids = new Dictionary<EdkDll.EE_DataChannel_t, Grid>();
        private bool Initial = false;
        private bool BuiltGrids = false;
        //  private string _sensor;
        private static void LoadClassifier()
        {
            IFormatter formatter = new BinaryFormatter();
            if (File.Exists(gridFolder))
            {
                Stream stream = new FileStream(gridFolder, FileMode.Open, FileAccess.Read, FileShare.Read);
                grids = (Dictionary<EdkDll.EE_DataChannel_t, Grid>)formatter.Deserialize(stream);
                stream.Close();
            }
        }
        public GridVisualiser()
        {
            // _sensor = sensor;

            InitializeComponent();
            // this.Text = sensor;

            Timer timer = new Timer();
            timer.Tick += new EventHandler(timer_Tick); // Everytime timer ticks, timer_Tick will be called
            timer.Interval = (500) * (1);              // Timer will tick evert second
            timer.Enabled = true;                       // Enable the timer
            timer.Start();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (BuiltGrids)
                LoadClassifier();

            for (EdkDll.EE_DataChannel_t sensor = EdkDll.EE_DataChannel_t.AF3;
                 sensor <= EdkDll.EE_DataChannel_t.AF4;
                 sensor++)
            {
               var filepath = String.Format(path, sensor);
                double[,] graph;
                if (!Initial)
                {
                    Grid grid;
                    if (!BuiltGrids)
                        grid = GridBuilder.build(filepath);
                    else
                        grid = grids[sensor];
                    graph = new double[grid.nx * grid.ny, 3];
                    int iterator = 0;
                    for (int i = 0; i < grid.nx; i++)
                        for (int j = 0; j < grid.ny; j++)
                        {
                            graph[iterator, 0] = i;
                            graph[iterator, 1] = j;

                            double value = grid.values[i, j];

                            if (value < 0.5)
                                value = 0;
                            else if (value < 1.5)
                                value = 1;
                            else if (value < 2.5)
                                value = 2;
                            else if (value < 3.5)
                                value = 3;
                            else if (value < 4.5)
                                value = 4;
                            graph[iterator, 2] = value;
                            iterator++;
                        }
                }
                else
                {
                    DataTable tableSource = CSVReader.ReadCSVFile(filepath, false);
                    graph = new double[tableSource.Rows.Count, 3];
                    List<WellMark> points = new List<WellMark>();
                    for (int i = 0; i < tableSource.Rows.Count; i++)
                    {
                        graph[i, 0] = Double.Parse(tableSource.Rows[i][0].ToString());
                        graph[i, 1] = Double.Parse(tableSource.Rows[i][1].ToString());
                        graph[i, 2] = Double.Parse(tableSource.Rows[i][2].ToString());
                    }
                }
                CreateScatterplot((ZedGraphControl)this.Controls.Find(sensor.ToString(), true)[0], graph,sensor.ToString());
            }

            //  SetSize();
        }

        public void CreateScatterplot(ZedGraphControl zgc, double[,] graph,string title)
        {
            GraphPane myPane = zgc.GraphPane;
            // myPane.Title.Text = title;
            myPane.CurveList.Clear();

            // Set the titles
            myPane.Title.IsVisible = false;           
            myPane.XAxis.Title.IsVisible = false;//Text = sourceColumns[0];
            myPane.YAxis.Title.Text = title;


            // Classification problem
            PointPairList list1 = new PointPairList(); // Z = -1
            PointPairList list2 = new PointPairList(); // Z = +1
            PointPairList list3 = new PointPairList(); // Z = -1
            PointPairList list4 = new PointPairList(); // Z = +1
            PointPairList list5 = new PointPairList(); // Z = +1

            for (int i = 0; i < graph.GetLength(0); i++)
            {
                if (graph[i, 2] == 1)
                    list1.Add(graph[i, 0], graph[i, 1]);
                if (graph[i, 2] == 2)
                    list2.Add(graph[i, 0], graph[i, 1]);
                if (graph[i, 2] == 3)
                    list3.Add(graph[i, 0], graph[i, 1]);
                if (graph[i, 2] == 4)
                    list4.Add(graph[i, 0], graph[i, 1]);
            }
            list5.Add(0, 0);
            // Add the curve
            LineItem myCurve = myPane.AddCurve("Current", list5, Color.LimeGreen, SymbolType.Square);
            myCurve.Line.IsVisible = false;
            myCurve.Symbol.Border.IsVisible = false;
            myCurve.Symbol.Fill = new Fill(Color.LimeGreen);

            myCurve = myPane.AddCurve("A", list1, Color.Red, SymbolType.Circle);
            myCurve.Line.IsVisible = false;
            myCurve.Symbol.Border.IsVisible = false;
            myCurve.Symbol.Fill = new Fill(Color.Red);

            myCurve = myPane.AddCurve("B", list2, Color.DarkGreen, SymbolType.Circle);
            myCurve.Line.IsVisible = false;
            myCurve.Symbol.Border.IsVisible = false;
            myCurve.Symbol.Fill = new Fill(Color.DarkGreen);

            myCurve = myPane.AddCurve("C", list3, Color.Blue, SymbolType.Circle);
            myCurve.Line.IsVisible = false;
            myCurve.Symbol.Border.IsVisible = false;
            myCurve.Symbol.Fill = new Fill(Color.Blue);

            myCurve = myPane.AddCurve("Neutral", list4, Color.Black, SymbolType.Circle);
            myCurve.Line.IsVisible = false;
            myCurve.Symbol.Border.IsVisible = false;
            myCurve.Symbol.Fill = new Fill(Color.Black);



            // Fill the background of the chart rect and pane
            myPane.Fill = new Fill(Color.WhiteSmoke);

            zgc.AxisChange();
            zgc.Invalidate();
            zgc.Refresh();
        }

        public void updateCurrent(double x, double y, string sensor)
        {
            ZedGraphControl control = (ZedGraphControl)this.Controls.Find(sensor, true)[0];
            GraphPane myPane = control.GraphPane;
            if (myPane != null)
            {
                myPane.CurveList["Current"].Points[0].X = x;
                myPane.CurveList["Current"].Points[0].Y = y;
            }
        }

        //private void Form1_Resize(object sender, EventArgs e)
        //{
        //    SetSize();
        //}
        void timer_Tick(object sender, EventArgs e)
        {
            for (EdkDll.EE_DataChannel_t sensor = EdkDll.EE_DataChannel_t.AF3;
                 sensor <= EdkDll.EE_DataChannel_t.AF4;
                 sensor++)
            {
                var control = (ZedGraphControl)this.Controls.Find(sensor.ToString(), true)[0];
                control.Invalidate();
                control.Refresh();
            }
        }
        private void SetSize()
        {

            AF3.Location = new Point(10, 10);
            // Leave a small margin around the outside of the control
            AF3.Size = new Size(this.ClientRectangle.Width - 20, this.ClientRectangle.Height - 20);
        }
    }
}