using SimpleSvg2LineSegementInterpolater.Base;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSvg2LineSegementInterpolater
{
    public class LineSegmentSimplifier
    {
        public static void SimplifySameGradientPoints(LineSegementCollection lineSegement, double removeGradientThreold = 0.1f)
        {
            float calcGradient(PointF a, PointF b)
            {
                if (a.X == b.X)
                    return int.MaxValue;

                return (a.Y - b.Y) / (a.X - b.X);
            }

            for (int i = 0; i < lineSegement.Points.Count - 2; i++)
            {
                var start = lineSegement.Points[i];
                var mid = lineSegement.Points[i + 1];
                var end = lineSegement.Points[i + 2];

                var sm_g = calcGradient(start, mid);
                var me_g = calcGradient(mid, end);

                var diff = Math.Abs(sm_g - me_g);

                //如果三点斜率相似，那就去掉中间的点
                if (diff < removeGradientThreold || (mid.X == start.X && mid.X == end.X))
                {
                    lineSegement.Points.Remove(mid);
                    i--;
                }
            }
        }

        public static void SimplifyTooClosePoints(LineSegementCollection lineSegement, double? removeCloseDistanceThreold = null)
        {
            if (lineSegement.Points.Count <= 2)
                return;

            removeCloseDistanceThreold = removeCloseDistanceThreold ?? lineSegement.CalculateBound().Width / 20;

            for (int i = 0; i < lineSegement.Points.Count - 1; i++)
            {
                var start = lineSegement.Points[i];
                var next = lineSegement.Points[i + 1];

                var distance = MathF.Sqrt(MathF.Pow(start.X - next.X, 2) + MathF.Pow(start.Y - next.Y, 2));

                if (distance < removeCloseDistanceThreold && i > 0 && i < lineSegement.Points.Count - 1)
                {
                    lineSegement.Points.Remove(start);
                }
            }
        }
    }
}
