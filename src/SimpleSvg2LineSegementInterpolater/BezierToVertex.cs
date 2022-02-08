/** BezierToVetex.cs
 * Code copy&modify from https://github.com/beinteractive/SVGMeshUnity/blob/master/Assets/SVGMeshUnity/Runtime/Internals/BezierToVertex.cs
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSvg2LineSegementInterpolater
{
    internal class BezierToVertex
    {
        // https://github.com/mattdesl/svg-path-contours

        public float Scale = 1f;

        public float PathDistanceEpsilon = 1f;
        public int RecursionLimit = 8;
        public float FLTEpsilon = 1.19209290e-7f;

        public float AngleEpsilon = 0.01f;
        public float AngleTolerance = 0f;
        public float CuspLimit = 0f;

        public List<Vector2> WorkVertices { get; set; } = new();

        public void GetContours(SvgPathData svg)
        {
            var pen = Vector2.Zero;

            var curves = svg.Curves;
            var l = curves.Count;
            for (var i = 0; i < l; ++i)
            {
                var curve = curves[i];
                if (curve.IsMove)
                {

                }
                else
                {
                    FillBezier(pen, curve.InControl, curve.OutControl, curve.Position);
                }
                pen = curve.Position;
            }
        }

        ////// Based on:
        ////// https://github.com/pelson/antigrain/blob/master/agg-2.4/src/agg_curves.cpp

        private void FillBezier(Vector2 start, Vector2 c1, Vector2 c2, Vector2 end)
        {
            var distanceTolerance = PathDistanceEpsilon / Scale;
            distanceTolerance *= distanceTolerance;
            BeginFillBezier(start, c1, c2, end, distanceTolerance);
        }

        private void BeginFillBezier(Vector2 start, Vector2 c1, Vector2 c2, Vector2 end, float distanceTolerance)
        {
            WorkVertices.Add(start);
            RecursiveFillBezier(start, c1, c2, end, distanceTolerance, 0);
            WorkVertices.Add(end);
        }

        private void RecursiveFillBezier(Vector2 v1, Vector2 v2, Vector2 v3, Vector2 v4, float distanceTolerance, int level)
        {
            if (level > RecursionLimit)
            {
                return;
            }

            var pi = MathF.PI;

            // Calculate all the mid-points of the line segments
            //----------------------
            var v12 = (v1 + v2) / 2f;
            var v23 = (v2 + v3) / 2f;
            var v34 = (v3 + v4) / 2f;
            var v123 = (v12 + v23) / 2f;
            var v234 = (v23 + v34) / 2f;
            var v1234 = (v123 + v234) / 2f;

            // Enforce subdivision first time
            if (level > 0)
            {
                // Try to approximate the full cubic curve by a single straight line
                //------------------
                var d = v4 - v1;

                var d2 = MathF.Abs((v2.X - v4.X) * d.Y - (v2.Y - v4.Y) * d.X);
                var d3 = MathF.Abs((v3.X - v4.X) * d.Y - (v3.Y - v4.Y) * d.X);

                if (d2 > FLTEpsilon && d3 > FLTEpsilon)
                {
                    // Regular care
                    //-----------------
                    if ((d2 + d3) * (d2 + d3) <= distanceTolerance * (d.X * d.X + d.Y * d.Y))
                    {
                        // If the curvature doesn't exceed the distanceTolerance value
                        // we tend to finish subdivisions.
                        //----------------------
                        if (AngleTolerance < AngleEpsilon)
                        {
                            WorkVertices.Add(v1234);
                            return;
                        }

                        // Angle & Cusp Condition
                        //----------------------
                        var a23 = MathF.Atan2(v3.Y - v2.Y, v3.X - v2.X);
                        var da1 = MathF.Abs(a23 - MathF.Atan2(v2.Y - v1.Y, v2.X - v1.X));
                        var da2 = MathF.Abs(MathF.Atan2(v4.Y - v3.Y, v4.X - v3.X) - a23);

                        if (da1 >= pi)
                        {
                            da1 = 2 * pi - da1;
                        }

                        if (da2 >= pi)
                        {
                            da2 = 2 * pi - da2;
                        }

                        if (da1 + da2 < AngleTolerance)
                        {
                            // Finally we can stop the recursion
                            //----------------------
                            WorkVertices.Add(v1234);
                            return;
                        }

                        if (CuspLimit > 0f)
                        {
                            if (da1 > CuspLimit)
                            {
                                WorkVertices.Add(v2);
                                return;
                            }

                            if (da2 > CuspLimit)
                            {
                                WorkVertices.Add(v3);
                                return;
                            }
                        }
                    }
                }
                else
                {
                    if (d2 > FLTEpsilon)
                    {
                        // p1,p3,p4 are collinear, p2 is considerable
                        //----------------------
                        if (d2 * d2 <= distanceTolerance * (d.X * d.X + d.Y * d.Y))
                        {
                            if (AngleTolerance < AngleEpsilon)
                            {
                                WorkVertices.Add(v1234);
                                return;
                            }

                            // Angle Condition
                            //----------------------
                            var da1 = MathF.Abs(MathF.Atan2(v3.Y - v2.Y, v3.X - v2.X) -
                                                MathF.Atan2(v2.Y - v1.Y, v2.X - v1.X));
                            if (da1 >= pi)
                            {
                                da1 = 2 * pi - da1;
                            }

                            if (da1 < AngleTolerance)
                            {
                                WorkVertices.Add(v2);
                                WorkVertices.Add(v3);
                                return;
                            }

                            if (CuspLimit > 0f)
                            {
                                if (da1 > CuspLimit)
                                {
                                    WorkVertices.Add(v2);
                                    return;
                                }
                            }
                        }
                    }
                    else if (d3 > FLTEpsilon)
                    {
                        // p1,p2,p4 are collinear, p3 is considerable
                        //----------------------
                        if (d3 * d3 <= distanceTolerance * (d.X * d.X + d.Y * d.Y))
                        {
                            if (AngleTolerance < AngleEpsilon)
                            {
                                WorkVertices.Add(v1234);
                                return;
                            }

                            // Angle Condition
                            //----------------------
                            var da1 = MathF.Abs(MathF.Atan2(v4.Y - v3.Y, v4.X - v3.X) -
                                                MathF.Atan2(v3.Y - v2.Y, v3.X - v2.X));
                            if (da1 >= pi)
                            {
                                da1 = 2 * pi - da1;
                            }

                            if (da1 < AngleTolerance)
                            {
                                WorkVertices.Add(v2);
                                WorkVertices.Add(v3);
                                return;
                            }

                            if (CuspLimit > 0f)
                            {
                                if (da1 > CuspLimit)
                                {
                                    WorkVertices.Add(v3);
                                    return;
                                }
                            }
                        }
                    }
                    else
                    {
                        // Collinear case
                        //-----------------
                        var dx = v1234.X - (v1.X + v4.X) / 2f;
                        var dy = v1234.Y - (v1.Y + v4.Y) / 2f;
                        if (dx * dx + dy * dy <= distanceTolerance)
                        {
                            WorkVertices.Add(v1234);
                            return;
                        }
                    }
                }
            }

            // Continue subdivision
            //----------------------
            RecursiveFillBezier(v1, v12, v123, v1234, distanceTolerance, level + 1);
            RecursiveFillBezier(v1234, v234, v34, v4, distanceTolerance, level + 1);
        }
    }
}
