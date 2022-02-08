using System.Numerics;

namespace SimpleSvg2LineSegementInterpolater.Base
{
    public struct Curve
    {
        public bool IsMove { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 InControl { get; set; }
        public Vector2 OutControl { get; set; }
    }
}