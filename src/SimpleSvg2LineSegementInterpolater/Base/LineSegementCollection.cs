using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SimpleSvg2LineSegementInterpolater.Base
{
    public class LineSegementCollection
    {
        public List<PointF> Points { get; set; }
        public Color Color { get; set; }

        public Rect CalculateBound()
        {
            (var minX, var maxX) = Points.Select(x => x.X).MaxMinBy();
            (var minY, var maxY) = Points.Select(x => x.Y).MaxMinBy();

            return new Rect(minX, minY, maxX - minX, maxY - minY);
        }
    }
}
