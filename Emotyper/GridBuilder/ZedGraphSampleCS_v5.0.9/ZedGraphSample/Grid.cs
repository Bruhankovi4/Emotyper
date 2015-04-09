using System;

namespace ZedGraphSample
{
    [Serializable]
    public class Grid
    {
        public double minval;
        public double maxval;
        public double[] xs;
        public double[] ys;
        public int nx;//число точек в сетке
        public int ny;
        public double[,] values;
     
        #region constructors
        public Grid(int n_x, int n_y, double xMin, double yMin, double xMax, double yMax)
        {
            double dx = (xMax - xMin) / n_x;
            double dy = (yMax - yMin) / n_y;

            nx = n_x;
            ny = n_y;
            xs = new double[nx];
            ys = new double[ny];
            for (int i = 0; i <nx; i++)
            {
                xs[i] = xMin + i * dx;
            }
            for (int i = 0; i <ny; i++)
            {
                ys[i] = yMin + i * dy;
            }
            values = new double[nx, ny ];          
        }
        public Grid(double dx, double dy, double xMin, double yMin, double xMax, double yMax)
        {
            nx = (int)((xMax - xMin) / dx);
            dx = (xMax - xMin) / (nx-1);

            ny = (int)((yMax - yMin) / dy);
            dy = (yMax - yMin) / (ny-1);

            xs = new double[nx];
            ys = new double[ny];
            for (int i = 0; i <nx; i++)
            {
                xs[i] = xMin + i * dx;
            }
            for (int i = 0; i <ny; i++)
            {
                ys[i] = yMin + i * dy;
            }
            values = new double[nx, ny];
        //    smoothed_values = new double[nx + 1, ny + 1];
           // colors = new Colour[nx, ny];
           // isolines = new ArrayList();
        //    points = pointsWithWeights;
        }
        //public Grid(double[,] nodes,double _minval,double _maxval)
        //{
        //    values = nodes;
        //    nx = nodes.GetLength(0);
        //    ny = nodes.GetLength(1);

        //    //Constants.paletteMax = _maxval;
        //    //Constants.paletteMin = _minval;

        //    xs = new double[nx];
        //    ys = new double[ny];
        //  //  colors = new Colour[nx, ny];
        //    minval = _minval; maxval = _maxval;
                    
        //    for (int i = 0; i < nx; i++)
        //        xs[i] = ((Constants.worldEndx-Constants.worldStartx)/nx)*i;
        //    for (int i = 0; i < ny; i++)
        //        ys[i] = ((Constants.worldEndy - Constants.worldStarty) / ny) * i;
        //    for (int i = 0; i < nx; i++)
        //        for (int k = 0; k < ny; k++)
        //        {
        //            colors[i, k] = getPointColour(i, k);

        //        }
        //}
        #endregion constructors
        #region accesmethods
        public double[] Getxs()
        {
            return xs;
        }
        public double[] Getys()
        {
            return ys;
        }
        public double getValue(int i, int j)
        {
            return values[i, j];
        }
        #endregion accesmethods
        //public Colour getPointColour(int i, int j)
        //{
        //     palett = new Palette();
        //    double value = values[i, j];
        //    if (value > palett.keyValues[0])
        //        return new Colour(1, 0, 0);
        //    else if (value >= palett.keyValues[1])
        //    {
        //        return new Colour(1, (1 - (value - palett.keyValues[1]) / palett.segmentSize), 0);//test change
        //    }
        //    else if (value >= palett.keyValues[2])
        //    {
        //        return new Colour((value - palett.keyValues[2]) / palett.segmentSize, 1, 0);
        //    }
        //    else if (value >= palett.keyValues[3])
        //    {
        //        return new Colour(0, 1, (1 - (value - palett.keyValues[3]) / palett.segmentSize));
        //    }
        //    else if (value >= palett.keyValues[4])
        //    {
        //        return new Colour(0, (value - palett.keyValues[4]) / palett.segmentSize, 1);
        //    }
        //    else
        //        return new Colour(0, 0, 1);

        //}
        public void Print()
        {
            Console.WriteLine(" ");
            for (int i = 0; i < nx + 1; i++)
            {
                for (int j = 0; j < ny + 1; j++)
                {
                    Console.Write((int)values[i, j] + "  ");
                }
                Console.WriteLine(" ");
            }
        }



        #region IDrawable Members

        #endregion
        private void setnormal(int i, int j)
        {
            double normx;
            double normy;
            double normz;
            double ax=0;
            double ay = 0;
            double az = 0;
            double bx = 0;
            double by = 0;
            double bz = 0;
            
            if (i == 0)
            {
               ax = xs[1] - xs[0];
               ay = 0;
               az = values[i + 1, j] - values[i, j];
            }
            else if (i == nx - 1)
            {
                ax = xs[1] - xs[0];
                ay = 0;
                az = values[i, j] - values[i-1, j];
            }
            if (j == 0)
            {
                bx = 0;
                by = ys[1] - ys[0];
                bz = values[i, j+1] - values[i, j];
            }
            else if (j == ny-1)
            {
                bx = 0;
                by = ys[1] - ys[0];
                bz = values[i, j ] - values[i, j-1];
            }
            if ((i > 0) && (i < nx - 1) && (j > 0) && (j < ny - 1))
            {
                ax = 2*(xs[1] - xs[0]);
                ay = 0;
                az = values[i+1, j] - values[i - 1, j];
                bx = 0;
                by = 2*(ys[1] - ys[0]);
                bz = values[i, j + 1] - values[i, j-1];
            }
            normx = ay * bz - by * az;
            normy = az * bx - bz * ax;
            normz = ax * by - bx * ay;
            //Gl.glNormal3d(normx, normy, normz);
        }
    }  
}
