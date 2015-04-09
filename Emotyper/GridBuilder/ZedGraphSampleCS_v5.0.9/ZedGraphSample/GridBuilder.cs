using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Com.StellmanGreene.CSVReader;
namespace ZedGraphSample
{
    public class GridBuilder
    {
        public static Grid build(string filepath="D://GitRepos//Emotyper//Emotyper//SOMCoordinates//Coords3.csv")
        {
            DataTable tableSource = CSVReader.ReadCSVFile(filepath.Replace("\\", "//"), false);

            List<WellMark> points = new List<WellMark>();
            for (int i = 0; i < tableSource.Rows.Count; i++)
            {

                points.Add(new WellMark( Double.Parse(tableSource.Rows[i][0].ToString()), Double.Parse(tableSource.Rows[i][1].ToString()), Int32.Parse(tableSource.Rows[i][2].ToString())));
            }
                        
            Grid grid = new Grid(Constants.GridWidth, Constants.GridHeight, Constants.worldStartx, Constants.worldStarty, Constants.worldEndx, Constants.worldEndy);
            grid.minval = 1;
            grid.maxval = 4;

            //IAlhorythm algo = new InverseRadius(points.ToArray()); 
            IAlhorythm algo = new Spreading(points);
            algo.calculute(grid);
            return grid;
        }
    }
}
