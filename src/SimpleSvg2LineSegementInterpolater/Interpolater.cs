using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Io;
using SimpleSvg2LineSegementInterpolater.Base;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SimpleSvg2LineSegementInterpolater
{
    public static class Interpolater
    {
        private static Task<IDocument> GenerateDocument(string content, string contentType)
        {
            var config = Configuration.Default.WithDefaultLoader().WithXml();
            var context = BrowsingContext.New(config);
            return context.OpenAsync(res =>
            {
                res.Content(content);

                if (!string.IsNullOrEmpty(contentType))
                    res.Header(HeaderNames.ContentType, contentType);
            });
        }

        public static async Task<List<LineSegementCollection>> GenerateInterpolatedLineSegmentAsync(string svgFileContent, InterpolaterOption option)
        {
            var document = await GenerateDocument(svgFileContent, "image/svg+xml");
            var svg = document.QuerySelector("svg");

            return GenerateInterpolatedLineSegment(svg, option).ToList();
        }

        public static IEnumerable<LineSegementCollection> GenerateInterpolatedLineSegment(IElement element, InterpolaterOption option)
        {
            LineSegementCollection PostProcess(LineSegementCollection collection)
            {
                for (int i = 0; i < collection.Points.Count; i++)
                {
                    var p = collection.Points[i];
                    collection.Points[i] = new(p.X * option.Scale, p.Y * option.Scale);
                }
                return collection;
            }

            return GenerateInterpolatedLineSegmentInternal(element, option).Select(x => PostProcess(x));
        }

        private static IEnumerable<LineSegementCollection> GenerateInterpolatedLineSegmentInternal(IElement element, InterpolaterOption option)
        {
            var stroke = element.TryGetStoke(option, false);

            LineSegementCollection PostProcess2(LineSegementCollection collection, IElement e)
            {
                //todo overwrite props
                if (stroke != default)
                    collection.Color = stroke;
                collection.Name = e.Id;
                return collection;
            }
            LineSegementCollection PostProcess(LineSegementCollection collection)
                => PostProcess2(collection, element);

            switch (element.NodeName)
            {
                case "rect":
                    yield return PostProcess(GenerateInterpolatedLineSegmentByRect(element, option));
                    break;
                case "line":
                    yield return PostProcess(GenerateInterpolatedLineSegmentByLine(element, option));
                    break;
                case "polygon":
                    yield return PostProcess(GenerateInterpolatedLineSegmentByPolygon(element, option));
                    break;
                case "polyline":
                    yield return PostProcess(GenerateInterpolatedLineSegmentByPolygon(element, option, false));
                    break;
                case "circle":
                    yield return PostProcess(GenerateInterpolatedLineSegmentByCircle(element, option));
                    break;
                case "ellipse":
                    yield return PostProcess(GenerateInterpolatedLineSegmentByEllipse(element, option));
                    break;
                case "path":
                    foreach (var childSegment in GenerateInterpolatedLineSegmentByPath(element, option))
                        yield return PostProcess(childSegment);
                    break;
                case "text":
                    foreach (var childSegment in GenerateInterpolatedLineSegmentByText(element, option))
                        yield return PostProcess(childSegment);
                    break;
                case "g":
                default:
                    foreach (var child in element.Children)
                        foreach (var childSegment in GenerateInterpolatedLineSegmentInternal(child, option))
                            yield return PostProcess2(childSegment, child);
                    break;
            }
        }

        private static IEnumerable<LineSegementCollection> GenerateInterpolatedLineSegmentByText(IElement element, InterpolaterOption option)
        {
            GlyphRun ConvertTextLinesToGlyphRun(GlyphTypeface glyphTypeface, double renderingEmSize, double advanceWidth, double advanceHeight, System.Windows.Point baselineOrigin, string[] lines)
            {
                var glyphIndices = new List<ushort>();
                var advanceWidths = new List<double>();
                var glyphOffsets = new List<System.Windows.Point>();

                var y = baselineOrigin.Y;
                for (int i = 0; i < lines.Length; ++i)
                {
                    var line = lines[i];

                    var x = baselineOrigin.X;
                    for (int j = 0; j < line.Length; ++j)
                    {
                        var glyphIndex = glyphTypeface.CharacterToGlyphMap[line[j]];
                        glyphIndices.Add(glyphIndex);
                        advanceWidths.Add(0);
                        glyphOffsets.Add(new(x, y));

                        x += advanceWidth;
                    }

                    y += advanceHeight;
                }

                return new GlyphRun(
                    glyphTypeface,
                    0,
                    false,
                    renderingEmSize,
                    300,
                    glyphIndices,
                    baselineOrigin,
                    advanceWidths,
                    glyphOffsets,
                    null,
                    null,
                    null,
                    null,
                    null);
            }

            var fontSize = element.Attributes.TryGetAttrValue("font-size", 40);
            var text = element.Attributes.TryGetAttrValue("text", string.Empty);
            if (string.IsNullOrWhiteSpace(text))
                text = element.TextContent;
            var x = element.Attributes.TryGetAttrValue("x", 0f);
            var y = element.Attributes.TryGetAttrValue("y", 0f);
            var stroke = element.TryGetStoke(option);

            var glyphTypeface = new GlyphTypeface(new Uri(@"C:\WINDOWS\Fonts\msyh.ttc"));
            var run = ConvertTextLinesToGlyphRun(glyphTypeface, fontSize, fontSize, fontSize, default, new[] { text });
            var geometry = run.BuildGeometry();
            var list = new List<Geometry>();

            if (geometry is GeometryGroup group)
            {
                foreach (var subGeometry in group.Children)
                {
                    list.Add(subGeometry);
                }
            }
            else
            {
                list.Add(geometry);
            }

            foreach (var outputGeometry in list)
            {
                outputGeometry.Transform = new TranslateTransform(x, y);
                var path = outputGeometry.GetOutlinedPathGeometry().ToString().Trim();
                if (path.Length > 2 && path.StartsWith("F1"))
                    path = path.Substring(2);

                var collection = new LineSegementCollection();
                collection.Points = new List<PointF>();
                collection.Color = stroke;

                var svgPathData = new SvgPathData();
                svgPathData.Path(path);

                var reserializer = new BezierToVertex();
                reserializer.GetContours(svgPathData);

                foreach (var vec2 in reserializer.WorkVertices)
                    collection.Points.Add(new(vec2.X, vec2.Y));

                yield return collection;
            }
        }

        private static IEnumerable<LineSegementCollection> GenerateInterpolatedLineSegmentByPath(IElement element, InterpolaterOption option)
        {
            var d = element.Attributes.TryGetAttrValue("d", "");
            var stroke = element.TryGetStoke(option);

            var parts = d.Split(new char[] { 'z', 'Z' });

            for (int i = 0; i < parts.Length; i++)
            {
                var item = parts[i];

                var z = item.Trim().Replace("\r", string.Empty).Replace("\n", string.Empty);
                if (!item.EndsWith("z", StringComparison.InvariantCultureIgnoreCase))
                    z = z + "z";

                var collection = new LineSegementCollection();

                collection.Points = new List<PointF>();
                collection.Color = stroke;

                var svgPathData = new SvgPathData();
                svgPathData.Path(z);

                var reserializer = new BezierToVertex();
                reserializer.GetContours(svgPathData);

                foreach (var vec2 in reserializer.WorkVertices)
                    collection.Points.Add(new(vec2.X, vec2.Y));

                if (collection.Points.Count == 0)
                    continue;

                //手动闭环
                if (collection.Points.Count > 0)
                    collection.Points.Add(collection.Points.FirstOrDefault());

                yield return collection;
            }

        }

        private static LineSegementCollection GenerateInterpolatedLineSegmentByCircle(IElement element, InterpolaterOption option)
        {
            var cx = element.Attributes.TryGetAttrValue("cx", 0f);
            var cy = element.Attributes.TryGetAttrValue("cy", 0f);
            var r = element.Attributes.TryGetAttrValue("r", 0f);
            var stroke = element.TryGetStoke(option);

            var collection = new LineSegementCollection();

            collection.Points = new List<PointF>();
            collection.Color = stroke;

            var edges = option.CircleSimplyAsLessPolygon ? 6 : 360;

            for (int i = 0; i < edges; i++)
            {
                var angle = i * 1.0f / edges;
                var rad = angle * MathF.PI / 180.0f;
                var x = cx + r * MathF.Cos(rad);
                var y = cy + r * MathF.Sin(rad);

                collection.Points.Add(new(x, y));
            }

            //手动闭环
            collection.Points.Add(collection.Points.FirstOrDefault());

            return collection;
        }

        private static LineSegementCollection GenerateInterpolatedLineSegmentByEllipse(IElement element, InterpolaterOption option)
        {
            var cx = element.Attributes.TryGetAttrValue("cx", 0f);
            var cy = element.Attributes.TryGetAttrValue("cy", 0f);
            var rx = element.Attributes.TryGetAttrValue("rx", 0f);
            var ry = element.Attributes.TryGetAttrValue("ry", 0f);
            var stroke = element.TryGetStoke(option);

            var collection = new LineSegementCollection();

            collection.Points = new List<PointF>();
            collection.Color = stroke;

            var list1 = new List<PointF>();
            var list2 = new List<PointF>();
            var list3 = new List<PointF>();
            var list4 = new List<PointF>();

            void ellipsePlotPoints(float xCenter, float yCenter, float x, float y)
            {
                list1.Add(new(xCenter + x, yCenter + y));
                list2.Insert(0, new(xCenter + x, yCenter - y));
                list3.Add(new(xCenter - x, yCenter - y));
                list4.Insert(0, new(xCenter - x, yCenter + y));
            }

            var Rx2 = rx * rx;
            var Ry2 = ry * ry;
            var twoRx2 = 2 * Rx2;
            var twoRy2 = 2 * Ry2;
            var p = 0f;
            var x = 0f;
            var y = ry;
            var px = 0f;
            var py = twoRx2 * y;
            // Plot the initial point in each quadrant
            //ellipsePlotPoints(cx, cy, x, y);
            /* Region 1 */
            p = (Ry2 - (Rx2 * ry) + (0.25f * Rx2));
            while (px < py)
            {
                x++;
                px += twoRy2;
                if (p < 0)
                {
                    p += Ry2 + px;
                }
                else
                {
                    y--;
                    py -= twoRx2;
                    p += Ry2 + px - py;
                }
                ellipsePlotPoints(cx, cy, x, y);
            }
            /* Region 2 */
            p = (Ry2 * (x + 0.5f) * (x + 0.5f) + Rx2 * (y - 1f) * (y - 1f) - Rx2 * Ry2);
            while (y > 0)
            {
                y--;
                py -= twoRx2;
                if (p > 0)
                {
                    p += Rx2 - py;
                }
                else
                {
                    x++;
                    px += twoRx2;
                    p += Rx2 - py + px;
                }
                ellipsePlotPoints(cx, cy, x, y);
            }

            collection.Points.AddRange(list1);
            collection.Points.AddRange(list2);
            collection.Points.AddRange(list3);
            collection.Points.AddRange(list4);
            //手动闭环
            collection.Points.Add(collection.Points.FirstOrDefault());

            return collection;
        }

        private static LineSegementCollection GenerateInterpolatedLineSegmentByRect(IElement element, InterpolaterOption option)
        {
            var baseX = element.Attributes.TryGetAttrValue("x", 0f);
            var baseY = element.Attributes.TryGetAttrValue("y", 0f);
            var width = element.Attributes.TryGetAttrValue("width", 0f);
            var height = element.Attributes.TryGetAttrValue("height", 0f);
            var stroke = element.TryGetStoke(option);

            var collection = new LineSegementCollection();

            collection.Points = new List<PointF>();
            collection.Points.Add(new(baseX, baseY));
            collection.Points.Add(new(baseX + width, baseY));
            collection.Points.Add(new(baseX + width, baseY + height));
            collection.Points.Add(new(baseX, baseY + height));

            //手动闭环
            collection.Points.Add(collection.Points.FirstOrDefault());

            collection.Color = stroke;

            return collection;
        }

        private static LineSegementCollection GenerateInterpolatedLineSegmentByPolygon(IElement element, InterpolaterOption option, bool isClose = true)
        {
            var pointsStr = element.Attributes.TryGetAttrValue("points", string.Empty);
            PointF[] points = default;

            if (pointsStr.Contains(","))
            {
                points = pointsStr.Split(" ").Select(x =>
                {
                    var arr = x.Split(",");
                    return new PointF(arr[0].TryToFloat(), arr[1].TryToFloat());
                }).ToArray();
            }
            else
            {
                var f = pointsStr.Split(" ");
                points = new PointF[f.Length / 2];
                for (int i = 0; i < f.Length / 2; i++)
                    points[i] = new PointF(f[2 * i + 0].TryToFloat(), f[2 * i + 1].TryToFloat());
            }

            var stroke = element.TryGetStoke(option);

            var collection = new LineSegementCollection();

            collection.Points = new List<PointF>();
            for (int i = 0; i < points.Length; i++)
                collection.Points.Add(points[i]);

            //手动闭环
            if (isClose)
                collection.Points.Add(collection.Points.FirstOrDefault());

            collection.Color = stroke;
            return collection;
        }

        private static LineSegementCollection GenerateInterpolatedLineSegmentByLine(IElement element, InterpolaterOption option)
        {
            var fromX = element.Attributes.TryGetAttrValue("x1", 0f);
            var fromY = element.Attributes.TryGetAttrValue("y1", 0f);
            var toX = element.Attributes.TryGetAttrValue("x2", 0f);
            var toY = element.Attributes.TryGetAttrValue("y2", 0f);
            var stroke = element.TryGetStoke(option);

            var collection = new LineSegementCollection();

            collection.Points = new List<PointF>();
            collection.Points.Add(new(fromX, fromY));
            collection.Points.Add(new(toX, toY));

            collection.Color = stroke;

            return collection;
        }
    }
}
