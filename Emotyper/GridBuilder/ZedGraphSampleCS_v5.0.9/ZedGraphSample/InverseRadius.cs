using System;


namespace ZedGraphSample
{
   struct Segment
    {
        public double x1;
        public double y1;
        public double z1;
       
        public double x2;
        public double y2;
        public double z2;
       
    }

    class InverseRadius : IAlhorythm
    {
        private double[,] smoothed_values;   
        private WellMark[] wellmarks;
       
        public InverseRadius(WellMark[] _points)
        { wellmarks = _points;
        }
        //TODO Fix
        private void SlideAvarege(Grid grid)
        {
            smoothed_values = (double[,])grid.values.Clone();
            double val = 0;
            int count = 0;
            double dist = 0;
            int indent = Constants.RadiusIndent;
            int leftborder = 0;
            int rightborder = 0;
            int topborder = 0;
            int bottomborder = 0;
            for (int i = 0; i < grid.nx; i++)
                for (int j = 0; j < grid.ny; j++)
                {
                    double mindist = Distance(grid.xs[i], grid.ys[j], wellmarks[0].x, wellmarks[0].y);

                    leftborder = i - indent;
                    if (leftborder < 0)
                        leftborder = 0;
                    rightborder = i + indent;
                    if (rightborder > grid.nx - 1)
                        rightborder = grid.nx - 1;
                    topborder = j + indent;
                    if (topborder > grid.ny - 1)
                        topborder = grid.ny - 1;
                    bottomborder = j - indent;
                    if (bottomborder < 0)
                        bottomborder = 0;

                    for (int k = leftborder; k <= rightborder; k++)
                        for (int l = bottomborder; l <= topborder; l++)
                        {
                            if (i != k || j != l)
                            {
                                val += (grid.values[k, l]);
                                count += 1;
                            }
                        }

                    smoothed_values[i, j] = val / count;
                    int index = 0;
                    for (int p = 0; p < wellmarks.Length; p++)
                    {
                        dist = Distance(grid.xs[i], grid.ys[j], wellmarks[p].x, wellmarks[p].y);
                        if (dist < mindist)
                        {
                            mindist = dist;
                            index = p;
                        }
                    }
                    if (dist != 0)
                    {
                        dist = Distance(grid.xs[i], grid.ys[j], wellmarks[index].x, wellmarks[index].y);
                        if (dist < Constants.RadiusStep)
                            smoothed_values[i, j] = wellmarks[index].param;
                    }
                    val = 0;
                    count = 0;
                    dist = 0;
                }
            grid.values = (double[,])smoothed_values.Clone();
        }
       
        double calcPointParam(double x, double y)
        {
            double val = 0;
            double sumOfReverseRadiuses = 0;
            double revrad = 0;
            for (int i = 0; i < wellmarks.Length; i++)
            {
                double dist = Distance(x, y, wellmarks[i].x, wellmarks[i].y);
                if (dist < Constants.RadiusStep)
                    return wellmarks[i].param;

                revrad = 1 / dist;
                val += (wellmarks[i].param * revrad);
                sumOfReverseRadiuses += revrad;

            }
            return val / (sumOfReverseRadiuses);

        }
          /*------------------------------------------------Вспомогательные функции----------------------------------------------------*/

        private double Distance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
        }
       

        #region IAlhorythm Members

      public void calculute(Grid grid)
        {
            for (int i = 0; i < grid.nx ; i++)
                for (int j = 0; j < grid.ny; j++)
                    grid.values[i, j] = calcPointParam(grid.xs[i], grid.ys[j]);

            
            for (int b = 0; b < Constants.RadiusSmoothCount; b++)
            {
                SlideAvarege(grid);
                Console.WriteLine(b);
            }
            Constants.RadiusIndent = 3;
            Constants.RadiusStep = 3;

            for (int b = 0; b < 2; b++)
                SlideAvarege(grid);
           
           
        }

        #endregion
    }
}
