using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MIConvexHull;

namespace ZedGraphSample
{
    class VoronoiAlhorythm :IAlhorythm
    {
        private List<WellMark> wellmarks = new List<WellMark>();
       

        public VoronoiAlhorythm(List<WellMark> _wellmarks)
        {
            wellmarks = _wellmarks;
        }
        public void calculute(Grid grid)
        {
            var config = new TriangulationComputationConfig();
            //    new TriangulationComputationConfig
            //{
            //    PointTranslationType = PointTranslationType.TranslateInternal,
            //    PlaneDistanceTolerance = 0.00001,
            //    // the translation radius should be lower than PlaneDistanceTolerance / 2
            //    PointTranslationGenerator = TriangulationComputationConfig.RandomShiftByRadius(0.000001, 0)
            //};
            VoronoiMesh<WellMark, Cell, VoronoiEdge<WellMark, Cell>> voronoiMesh = VoronoiMesh.Create<WellMark, Cell>(wellmarks, config);

             //for (int i = 0; i < grid.nx; i++)
             //    for (int j = 0; j < grid.ny; j++)
             //    {
            Console.WriteLine("Points: " + wellmarks.Count);
            Console.WriteLine("voronoiMesh.Vertices : " + voronoiMesh.Vertices.Count());
            int i = 0;
                     foreach (var cell in voronoiMesh.Edges)
                     {
                                   
                         foreach (var wellmark in wellmarks)
                         {
                               if (PointInCell(cell.Target, wellmark.ToPoint()))
                               {
                                   i++;
                               }
                         }
                         //if (PointInCell(cell, new Point(i, j)))
                         //{
                         //    foreach (var wellmark in wellmarks)
                         //    {
                         //        if (PointInCell(cell, wellmark.ToPoint()))
                         //        {
                         //            grid.values[i, j] = wellmark.param;
                         //        }
                         //    }
                         //}
                    // }
                 }
                     Console.WriteLine("i: " + i);
             
        }
        static bool PointInCell(Cell c, Point p)
        {
            var v1 = c.Vertices[0].ToPoint();
            var v2 = c.Vertices[1].ToPoint();
            var v3 = c.Vertices[2].ToPoint();

            var s0 = IsLeft(v1, v2, p);
            var s1 = IsLeft(v2, v3, p);
            var s2 = IsLeft(v3, v1, p);

            return (s0 == s1) && (s1 == s2);
        }
        static int IsLeft(Point a, Point b, Point c)
        {
            return ((b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X)) > 0 ? 1 : -1;
        }
    }
}
