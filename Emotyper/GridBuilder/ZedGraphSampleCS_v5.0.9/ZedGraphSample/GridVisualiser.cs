using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Com.StellmanGreene.CSVReader;
using ZedGraph;

namespace ZedGraphSample
{
    public partial class GridVisualiser : Form
    {
        private string path= "D://GitRepos//Emotyper//Emotyper//SOMCoordinates//{0}Coordinates.csv";
        public GridVisualiser(string sensor)
        {
           path= String.Format(path, sensor);
            InitializeComponent();
        }
        private bool Initial = false;
        private void Form1_Load(object sender, EventArgs e)
        {
            double[,] graph;
            if (!Initial)
            {
                Grid grid = GridBuilder.build(path);
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
                DataTable tableSource = CSVReader.ReadCSVFile(path,false);
                graph = new double[tableSource.Rows.Count, 3];
                List<WellMark> points = new List<WellMark>();
                for (int i = 0; i < tableSource.Rows.Count; i++)
                {
                    graph[i, 0] = Double.Parse(tableSource.Rows[i][0].ToString());
                    graph[i, 1] = Double.Parse(tableSource.Rows[i][1].ToString());
                    graph[i, 2] = Double.Parse(tableSource.Rows[i][2].ToString());
                   // points.Add(new WellMark(Double.Parse(tableSource.Rows[i][0].ToString()), Double.Parse(tableSource.Rows[i][1].ToString()), Int32.Parse(tableSource.Rows[i][2].ToString())));
                }
            }

            CreateScatterplot(zg1, graph);
            SetSize();
        }

        public void CreateScatterplot(ZedGraphControl zgc, double[,] graph)
        {
            GraphPane myPane = zgc.GraphPane;
            myPane.CurveList.Clear();

            // Set the titles
            myPane.Title.IsVisible = false;
            //  myPane.XAxis.Title.Text = sourceColumns[0];
            // myPane.YAxis.Title.Text = sourceColumns[1];


            // Classification problem
            PointPairList list1 = new PointPairList(); // Z = -1
            PointPairList list2 = new PointPairList(); // Z = +1
            PointPairList list3 = new PointPairList(); // Z = -1
            PointPairList list4 = new PointPairList(); // Z = +1

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

            // Add the curve
            LineItem myCurve = myPane.AddCurve("G1", list1, Color.Blue, SymbolType.Square);
            myCurve.Line.IsVisible = false;
            myCurve.Symbol.Border.IsVisible = false;
            myCurve.Symbol.Fill = new Fill(Color.Blue);

            myCurve = myPane.AddCurve("G2", list2, Color.Green, SymbolType.Square);
            myCurve.Line.IsVisible = false;
            myCurve.Symbol.Border.IsVisible = false;
            myCurve.Symbol.Fill = new Fill(Color.Green);

            myCurve = myPane.AddCurve("G3", list3, Color.Red, SymbolType.Square);
            myCurve.Line.IsVisible = false;
            myCurve.Symbol.Border.IsVisible = false;
            myCurve.Symbol.Fill = new Fill(Color.Red);

            myCurve = myPane.AddCurve("G4", list4, Color.DarkGray, SymbolType.Square);
            myCurve.Line.IsVisible = false;
            myCurve.Symbol.Border.IsVisible = false;
            myCurve.Symbol.Fill = new Fill(Color.DarkGray);

            // Fill the background of the chart rect and pane
            myPane.Fill = new Fill(Color.WhiteSmoke);

            zgc.AxisChange();
            zgc.Invalidate();
        }


        private void Form1_Resize(object sender, EventArgs e)
        {
            SetSize();
        }

        private void SetSize()
        {
            zg1.Location = new Point(10, 10);
            // Leave a small margin around the outside of the control
            zg1.Size = new Size(this.ClientRectangle.Width - 20, this.ClientRectangle.Height - 20);
        }
    }
}