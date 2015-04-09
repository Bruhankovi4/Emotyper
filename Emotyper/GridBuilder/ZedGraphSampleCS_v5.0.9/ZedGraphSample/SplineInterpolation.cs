//using System;
//using System.Collections;
//// для работы с библиотекой FreeGLUT
//namespace ZedGraphSample
//{

//    class SplineInterpolation : IAlhorythm
//    {
//        private double[,] Xcut;
//        private double[,] Ycut;
//        private WellMark[] points;
//        private double[,] smoothed_values;

//        public SplineInterpolation(WellMark[] pointsWithWeights, Grid grid)
//        {

//            Xcut = new double[grid.nx, grid.ny];

//            Ycut = new double[grid.nx, grid.ny];

//            smoothed_values = new double[grid.nx, grid.ny];

//            points = pointsWithWeights;
//        }


//        private double assignValue(double x, double y)
//        {
//            double dist = 0;
//            int index = 0;
//            double mindist = Distance(x, y, points[0].x, points[0].y);
//            for (int p = 0; p < points.Length; p++)
//            {

//                dist = Distance(x, y, points[p].x, points[p].y);
//                if (dist < mindist)
//                {
//                    mindist = dist;
//                    index = p;
//                }

//            }
//            if (dist != 0)
//            {
//                dist = Distance(x, y, points[index].x, points[index].y);
//                if (dist < Constants.RadiusStep)
//                    return points[index].param;
//            }
//            return 0;
//        }
//        private void assignStartValues(Grid grid)
//        {
//            for (int i = 0; i < grid.nx; i++)
//                for (int j = 0; j < grid.ny; j++)
//                {
//                    grid.values[i, j] = assignValue(grid.xs[i], grid.ys[j]);
//                }
//            int[] indecies = assignBorderValues(grid);
//            BuildBorder(indecies,grid);
//        }

//        private int[] assignBorderValues(Grid grid)
//        {
//            int[] indexes = new int[4];

//            //leftbottomCorner
//            double mindistance = Distance(Constants.worldStartx, Constants.worldStarty, points[0].x, points[0].y);
//            double dist = 0;
//            int ind1 = 0;
//            int ind2 = 0;
//            for (int i = 1; i < points.Length; i++)
//            {
//                dist = Distance(Constants.worldStartx, Constants.worldStarty, points[i].x, points[i].y);
//                if (dist < mindistance)
//                {
//                    mindistance = dist;
//                    ind1 = i;
//                    grid.values[0, 0] = points[i].param;
//                }
//            }
//            for (int i = 1; i < points.Length; i++)
//            {
//                if (i != ind1)
//                {
//                    dist = Distance(Constants.worldStartx, Constants.worldStarty, points[i].x, points[i].y);
//                    if (dist < mindistance)
//                    {
//                        mindistance = dist;
//                        ind2 = i;
//                        //values[0, 0] =points[i].param;
//                    }
//                }
//            }


//            //leftTopCorner
//            mindistance = Distance(Constants.worldEndx, Constants.worldStarty, points[0].x, points[0].y);
//            dist = 0;
//            for (int i = 1; i < points.Length; i++)
//            {
//                dist = Distance(Constants.worldEndx, Constants.worldStarty, points[i].x, points[i].y);
//                if (dist < mindistance)
//                {
//                    mindistance = dist;
//                    grid.values[grid.nx-1, 0] = points[i].param;//проверить индекс
//                }
//            }
//            //rightTopCorner
//            mindistance = Distance(Constants.worldEndx, Constants.worldEndy, points[0].x, points[0].y);
//            dist = 0;
//            for (int i = 1; i < points.Length; i++)
//            {
//                dist = Distance(Constants.worldEndx, Constants.worldEndy, points[i].x, points[i].y);
//                if (dist < mindistance)
//                {
//                    mindistance = dist;
//                    grid.values[grid.nx - 1, grid.ny - 1] = points[i].param;
//                }
//            }
//            //rightBottomCorner
//            mindistance = Distance(Constants.worldStartx, Constants.worldEndy, points[0].x, points[0].y);
//            dist = 0;
//            for (int i = 1; i < points.Length; i++)
//            {
//                dist = Distance(Constants.worldStartx, Constants.worldEndy, points[i].x, points[i].y);
//                if (dist < mindistance)
//                {
//                    mindistance = dist;
//                    grid.values[0, grid.ny - 1] = points[i].param;
//                }
//            }
//            //midleLeftPoint
//            dist = 0;
//            int curind = 0;
//            for (int i = 1; i < points.Length; i++)
//            {
//                if (points[i].x == well.MINx)
//                {
//                    curind = i;
//                    mindistance = grid.ys[1] - points[0].y;
//                }
//            }
//            for (int i = 0; i < grid.ys.Length; i++)
//            {
//                if (points[curind].y == grid.ys[i])
//                {
//                    grid.values[0, i] = points[curind].param;
//                    indexes[0] = i;
//                }
//                if ((points[curind].y) > grid.ys[i] && (points[curind].y) < grid.ys[i + 1])
//                {
//                    grid.values[0, i] = points[curind].param;
//                    indexes[0] = i;
//                }
//            }

//            //midleTopPoint
//            dist = 0;
//            curind = 0;
//            for (int i = 1; i < points.Length; i++)
//            {
//                if (points[i].x == well.MAXy)
//                {
//                    curind = i;
//                    mindistance = grid.xs[1] - points[0].x;
//                }
//            }
//            for (int i = 0; i < grid.xs.Length; i++)
//            {
//                if (points[curind].x == grid.xs[i])
//                {
//                    grid.values[i, grid.ny - 1] = points[curind].param;
//                    indexes[1] = i;
//                }
//                if ((points[curind].x) > grid.xs[i] && (points[curind].x) < grid.xs[i + 1])
//                {
//                    grid.values[i, grid.ny-1] = points[curind].param;
//                    indexes[1] = i;
//                }
//            }
//            //midleRightPoint
//            dist = 0;
//            curind = 0;
//            for (int i = 1; i < points.Length; i++)
//            {
//                if (points[i].x == well.MAXx)
//                {
//                    curind = i;
//                    mindistance = grid.ys[1] - points[0].y;
//                }
//            }
//            for (int i = 0; i < grid.ys.Length; i++)
//            {
//                if (points[curind].y == grid.ys[i])
//                {
//                    grid.values[grid.nx - 1, i] = points[curind].param;
//                    indexes[2] = i;
//                }
//                if ((points[curind].y) > grid.ys[i] && (points[curind].y) < grid.ys[i + 1])
//                {
//                    grid.values[grid.nx - 1, i] = points[curind].param;
//                    indexes[2] = i;
//                }
//            }
//            //midleBottomPoint
//            dist = 0;
//            curind = 0;
//            for (int i = 1; i < points.Length; i++)
//            {
//                if (points[i].x == well.MINy)
//                {
//                    curind = i;
//                    mindistance = grid.xs[1] - points[0].x;
//                }
//            }
//            for (int i = 0; i < grid.xs.Length; i++)
//            {
//                if (points[curind].x == grid.xs[i])
//                {
//                    grid.values[i, 0] = points[curind].param;
//                    indexes[3] = i;
//                }
//                if ((points[curind].x) > grid.xs[i] && (points[curind].x) < grid.xs[i + 1])
//                {
//                    grid.values[i, 0] = points[curind].param;
//                    indexes[3] = i;
//                }
//            }

//            return indexes;
//        }


//        private void BuildBorder(int[] indexes, Grid grid)
//        {
//            //LeftSide
//            CubicSpline spline = new CubicSpline();
//            double[] xs = new double[3];
//            double[] ys = new double[3];
//            xs[0] = 0;
//            xs[1] = indexes[0];
//            xs[2] = grid.ny;

//            ys[0] = grid.values[0, 0];
//            ys[1] = grid.values[0, indexes[0]];
//            ys[2] = grid.values[0, grid.ny - 1];

//            spline.BuildSpline(xs, ys, 3);
//            for (int i = 0; i < grid.ny ; i++)
//                grid.values[0, i] = spline.Func(i);
//            //TopSide
//            xs = new double[3];
//            ys = new double[3];
//            xs[0] = 0;
//            xs[1] = indexes[1];
//            xs[2] = grid.nx;

//            ys[0] = grid.values[0, grid.ny - 1];
//            ys[1] = grid.values[indexes[1], grid.ny - 1];
//            ys[2] = grid.values[grid.nx - 1, grid.ny - 1];

//            spline.BuildSpline(xs, ys, 3);
//            for (int i = 0; i < grid.nx; i++)
//                grid.values[i, grid.ny - 1] = spline.Func(i);
//            //RightSide
//            xs = new double[3];
//            ys = new double[3];
//            xs[0] = 0;
//            xs[1] = indexes[2];
//            xs[2] = grid.ny;

//            ys[0] = grid.values[grid.nx - 1, 0];
//            ys[1] = grid.values[grid.nx - 1, indexes[2]];
//            ys[2] = grid.values[grid.nx - 1, grid.ny - 1];

//            spline.BuildSpline(xs, ys, 3);
//            for (int i = 0; i < grid.ny; i++)
//                grid.values[grid.nx - 1, i] = spline.Func(i);
//            //BottomSide
//            xs = new double[3];
//            ys = new double[3];
//            xs[0] = 0;
//            xs[1] = indexes[3];
//            xs[2] = grid.nx;

//            ys[0] = grid.values[0, 0];
//            ys[1] = grid.values[indexes[3], 0];
//            ys[2] = grid.values[grid.nx - 1, 0];

//            spline.BuildSpline(xs, ys, 3);
//            for (int i = 0; i < grid.nx; i++)
//                grid.values[i, 0] = spline.Func(i);

//        }
//        private void interpolate(Grid grid)
//        {
//            BuildYinterpolation(grid);
//            BuildXafterYinterpolation(grid);
//            BuildXinterpolation(grid);
//            BuildYafterXinterpolation(grid);

//            MergeInterpolations(grid);
//            for (int b = 0; b < Constants.RadiusSmoothCount; b++)
//            {
//                SlideAvarege(grid);
//                Console.WriteLine(b);
//            }
//        }

//        private void SlideAvarege(Grid grid)
//        {
//            smoothed_values = (double[,])grid.values.Clone();
//            double val = 0;
//            int count = 0;
//            int indent = Constants.RadiusIndent;
//            int leftborder = 0;
//            int rightborder = 0;
//            int topborder = 0;
//            int bottomborder = 0;
//            for (int i = 0; i < grid.nx ; i++)
//                for (int j = 0; j < grid.ny; j++)
//                {
//                    leftborder = i - indent;
//                    if (leftborder < 0)
//                        leftborder = 0;
//                    rightborder = i + indent;
//                    if (rightborder > grid.nx - 1)
//                        rightborder = grid.nx - 1;
//                    topborder = j + indent;
//                    if (topborder > grid.ny - 1)
//                        topborder = grid.ny - 1;
//                    bottomborder = j - indent;
//                    if (bottomborder < 0)
//                        bottomborder = 0;

//                    for (int k = leftborder; k <= rightborder; k++)
//                        for (int l = bottomborder; l <= topborder; l++)
//                        {
//                            if (i != k || j != l)
//                            {
//                                val += (grid.values[k, l]);
//                                count += 1;
//                            }
//                        }

//                    smoothed_values[i, j] = val / count;
//                    val = 0;
//                    count = 0;
//                }
//            grid.values = (double[,])smoothed_values.Clone();
//        }
//        private void MergeInterpolations(Grid grid)
//        {
//            for (int i = 1; i < grid.nx - 1; i++)
//                for (int j = 1; j < grid.ny - 1; j++)
//                    grid.values[i, j] = 0.5 * (Xcut[i, j] + Ycut[i, j]);
//        }
//        private void BuildXinterpolation(Grid grid)
//        {
//            CubicSpline spline = new CubicSpline();
//            ArrayList xs = new ArrayList();
//            ArrayList ys = new ArrayList();
//            for (int j = 0; j < grid.ny; j++)
//            {
//                for (int i = 0; i < grid.nx; i++)
//                {
//                    if (grid.values[i, j] != 0)
//                    {
//                        xs.Add((double)i);
//                        ys.Add(grid.values[i, j]);
//                    }
//                }
//                if (xs.Count > 2)
//                {
//                    double[] xarr = new double[xs.Count];
//                    double[] yarr = new double[xs.Count];
//                    for (int i = 0; i < xs.Count; i++)
//                    {
//                        xarr[i] = (double)xs[i];
//                        yarr[i] = (double)ys[i];

//                    }

//                    spline.BuildSpline(xarr, yarr, xs.Count);
//                    for (int i = 1; i < grid.nx - 1; i++)
//                    {
//                        Xcut[i, j] = spline.Func(i);

//                    }
//                }
//                xs = new ArrayList();
//                ys = new ArrayList();
//            }
//            Console.WriteLine("XInterpol");
//            //   PrintX();
//        }

//        private void BuildYinterpolation(Grid grid)
//        {
//            CubicSpline spline = new CubicSpline();
//            ArrayList xs = new ArrayList();
//            ArrayList ys = new ArrayList();

//            for (int i = 1; i < grid.nx; i++)
//            {
//                for (int j = 0; j < grid.ny; j++)
//                {

//                    if (grid.values[i, j] != 0)
//                    {
//                        xs.Add((double)j);
//                        ys.Add(grid.values[i, j]);
//                    }
//                }
//                // Print();
//                double[] xarr = new double[xs.Count];
//                double[] yarr = new double[xs.Count];
//                if (xs.Count > 2)
//                {
//                    for (int j = 0; j < xs.Count; j++)
//                    {
//                        xarr[j] = (double)xs[j];
//                        yarr[j] = (double)ys[j];
//                    }

//                    spline.BuildSpline(xarr, yarr, xs.Count);
//                    for (int j = 1; j < grid.nx - 1; j++)
//                    {
//                        Ycut[i, j] = spline.Func(j);

//                    }
//                }
//                xs = new ArrayList();
//                ys = new ArrayList();
//            }
//            Console.WriteLine("yInterpol");
//            //  PrintY();
//        }

//        private void BuildYafterXinterpolation(Grid grid)
//        {
//            CubicSpline spline = new CubicSpline();
//            ArrayList xs = new ArrayList();
//            ArrayList ys = new ArrayList();

//            for (int i = 0; i < grid.nx; i++)
//            {
//                for (int j = 0; j < grid.ny; j++)
//                {
//                    if (Xcut[i, j] != 0)
//                    {
//                        xs.Add((double)j);
//                        ys.Add(Xcut[i, j]);
//                    }
//                }
//                // PrintX();
//                double[] xarr = new double[xs.Count];
//                double[] yarr = new double[xs.Count];

//                for (int j = 0; j < xs.Count; j++)
//                {
//                    xarr[j] = (double)xs[j];
//                    yarr[j] = (double)ys[j];
//                }
//                if (xs.Count > 0)
//                {
//                    spline.BuildSpline(xarr, yarr, xs.Count);
//                    for (int j = 1; j < grid.nx - 1; j++)
//                    {
//                        Xcut[i, j] = spline.Func(j);

//                    }
//                }
//                xs = new ArrayList();
//                ys = new ArrayList();
//            }
//            Console.WriteLine("yafterxInterpol");
//            //  PrintY();
//        }

//        private void BuildXafterYinterpolation(Grid grid)
//        {
//            CubicSpline spline = new CubicSpline();
//            ArrayList xs = new ArrayList();
//            ArrayList ys = new ArrayList();
//            for (int j = 0; j < grid.ny; j++)
//            {
//                for (int i = 0; i < grid.nx; i++)
//                {
//                    if (Ycut[i, j] != 0)
//                    {
//                        xs.Add((double)i);
//                        ys.Add(Ycut[i, j]);
//                    }
//                }

//                double[] xarr = new double[xs.Count];
//                double[] yarr = new double[xs.Count];
//                for (int i = 0; i < xs.Count; i++)
//                {
//                    xarr[i] = (double)xs[i];
//                    yarr[i] = (double)ys[i];

//                }
//                if (xs.Count > 0)
//                {
//                    spline.BuildSpline(xarr, yarr, xs.Count);
//                    for (int i = 1; i < grid.nx - 1; i++)
//                    {
//                        Ycut[i, j] = spline.Func(i);

//                    }
//                }
//                xs = new ArrayList();
//                ys = new ArrayList();
//            }
//            Console.WriteLine("XafterYInterpol");
//        }


//        public void calculute(Grid grid)
//        {
//          assignStartValues(grid);
//            interpolate(grid);
//         }

//        private double Distance(double x1, double y1, double x2, double y2)
//        {
//            return Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
//        }

//    }

//}