
using System.Windows;
using MIConvexHull;

namespace ZedGraphSample
{
    public class WellMark :IVertex
    {
        public double x;
        public double y;
        public double param;
        public WellMark(double x, double y, double param)
        {
            this.x = x;
            this.y = y;
            this.param = param;
        }

        public double[] Position {
            get { return new double[]{x,y};} 
             }

        public Point ToPoint()
        {
            return new Point(Position[0], Position[1]);
        }
    }

}
