using System;
using System.Collections.Generic;
using System.Text;

namespace ZedGraphSample
{
    class Spreading:IAlhorythm
    {
        private List<WellMark> wellmarks = new List<WellMark>();
        private List<WellMark> wellmarks2 = new List<WellMark>();
        public Spreading(List<WellMark> _wellmarks)
        {
            wellmarks = _wellmarks;
        }
        public void calculute(Grid grid)
        {
            //while (true)
            //{
            //    for (int i = 0; i < grid.nx; i++)
            //        for (int j = 0; j < grid.ny; j++)
            //        {
            //            // double mindist = int.MaxValue;

            //            foreach (var wellmark in wellmarks)
            //            {
            //                double dist = Distance(i, j, wellmark.x, wellmark.y);

            //                if (dist == 1 && !wellmarks.Exists(x => x.x == i && x.y == j))
            //                {
            //                    wellmarks2.Add(new WellMark(i, j, wellmark.param));
            //                    break;
            //                    // mindist = dist;
            //                    // tempWellMark = new WellMark();= wellmark;
            //                }
            //            }

            //        }
            //    if (wellmarks2.Count == 0)
            //        break;
            //    wellmarks.AddRange(wellmarks2);
            //    wellmarks2 = new List<WellMark>();
            //}
            foreach (var wellmark in wellmarks)
            {
                grid.values[(int)wellmark.x, (int)wellmark.y] = wellmark.param;
            }
        }
        //public void calculute(Grid grid)
        //{            
        //        for (int i = 0; i < grid.nx; i++)
        //            for (int j = 0; j < grid.ny; j++)
        //            {
        //                //i = 13;
        //                //j = 57;
        //                 double mindist = int.MaxValue;
        //                foreach (var wellmark in wellmarks)
        //                {
        //                    double dist = Distance(i, j, wellmark.x, wellmark.y);

        //                    if (dist <mindist)
        //                    {
        //                        mindist = dist;
        //                            wellmarks2.Clear();
        //                        wellmarks2.Add(new WellMark(i, j, wellmark.param));
        //                    }
        //                    else  if (dist ==mindist)
        //                        wellmarks2.Add(new WellMark(i, j, wellmark.param));
        //                }
        //                if (wellmarks2.Count == 1)                       
        //                    grid.values[i, j] = wellmarks2[0].param;                                                   
        //                else
        //                {
        //                    int maxCount=int.MinValue;
        //                    int resultGroup = 0;
        //                        for (int group = 0; group <= 4; group++)
        //                        {
        //                            int count=wellmarks2.FindAll(x => x.param == group).Count;
        //                                   if (count > maxCount)
        //                                   {
        //                                       maxCount = count;
        //                                       resultGroup = group;
        //                                   }
        //                        }
        //                        grid.values[i, j] = resultGroup;
        //                }
        //                  wellmarks2.Clear();
        //            }                      
        //}
        private double Distance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
        }
    }
}
