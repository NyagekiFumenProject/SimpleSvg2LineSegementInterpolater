using SimpleSvg2LineSegementInterpolater;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Example
{
    class Program
    {
        static GlyphRun ConvertTextLinesToGlyphRun(GlyphTypeface glyphTypeface, double renderingEmSize, double advanceWidth, double advanceHeight, System.Windows.Point baselineOrigin, string[] lines)
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

        static async Task Main(string[] args)
        {
            var lineSegments = await Interpolater.GenerateInterpolatedLineSegmentAsync(await File.ReadAllTextAsync(@"I:\zz.svg"));
            using var image = SimpleSvg2LineSegementInterpolater.Drawing.DrawToImage(lineSegments);
            SimpleSvg2LineSegementInterpolater.Drawing.SaveImageToFile(@"I:\zz.png", image);
            /*
            var glyphTypeface = new GlyphTypeface(new Uri(@"C:\WINDOWS\Fonts\consola.ttf"));
            var run = ConvertTextLinesToGlyphRun(glyphTypeface, 40, 40, 40, new(0, 100), new[] { "I love SVG" });
            
            var geometryGroup = run.BuildGeometry();
            geometryGroup.Transform = new TranslateTransform(200, 200);
            var r = geometryGroup.GetOutlinedPathGeometry().ToString();*/

        }
    }
}
