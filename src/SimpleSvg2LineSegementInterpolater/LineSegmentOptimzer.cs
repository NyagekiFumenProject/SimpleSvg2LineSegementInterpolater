using SimpleSvg2LineSegementInterpolater.Base;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSvg2LineSegementInterpolater
{
    public class LineSegmentOptimzer
    {
        public static void Optimze(LineSegementCollection lineSegement)
        {
            float calcGradient(PointF a, PointF b)
            {
                if (a.Y == b.Y)
                    return float.MaxValue;

                return (a.Y - b.Y) / (a.X - b.X);
            }

            for (int i = 0; i < lineSegement.Points.Count - 2; i++)
            {
                var start = lineSegement.Points[i];
                var mid = lineSegement.Points[i + 1];
                var end = lineSegement.Points[i + 2];

                var sm_g = calcGradient(start, mid);
                var me_g = calcGradient(mid, end);

                //如果三点斜率相似，那就去掉中间的点
                if (Math.Abs(sm_g - me_g) < 0.001)
                {
                    lineSegement.Points.Remove(mid);
                    i--;
                    continue;
                }
            }
        }
    }
}
