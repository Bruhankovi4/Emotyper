using System;
using System.Collections.Generic;
using System.Text;

namespace ZedGraphSample
{
    public class Constants
    {
        public const bool _3Dmode = true;
        public const bool _2Dmode = false;

        public static double worldStartx = 0;
        public static double worldStarty = 0;
        public static double worldEndx = 100;
        public static double worldEndy = 100;
        public static int indent = 100;
        public static double paletteMin = 0;
        public static double paletteMax = 1000;
        public static int MaxVertices = 300;
        public static int MaxTriangles = 1000;
        public static int RadiusStep = 8;//зона влияния скважины
        public static int RadiusIndent = 6;//шаг осрелнения
        public static int RadiusSmoothCount =40;//количество осреднений
        public static double isolineStep = 2;
        public static int GridWidth = 100;
        public static int GridHeight = 100;

        public static int GridZscale = 10;

        //public static double translateToWorldX(double x)
        //{
        //    return (x - well.MINx) / (well.MAXx - well.MINx) * (Constants.worldEndx - Constants.worldStartx - 2 * Constants.indent) + Constants.indent;
        //}
        //public static double translateToWorldY(double y)
        //{
        //    return (y - well.MINy) / (well.MAXy - well.MINy) * (Constants.worldEndy - Constants.worldStarty - 2 * Constants.indent) + Constants.indent;
        //}
    }
}
