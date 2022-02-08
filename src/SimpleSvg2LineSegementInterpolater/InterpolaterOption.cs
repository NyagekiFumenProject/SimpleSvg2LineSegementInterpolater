using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSvg2LineSegementInterpolater
{
    public class InterpolaterOption
    {
        public Color DefaultStrokeColor { get; set; } = Color.Green;
        public bool EnableFillAsStroke { get; set; } = true;
        public float Scale { get; set; } = 1;
    }
}
