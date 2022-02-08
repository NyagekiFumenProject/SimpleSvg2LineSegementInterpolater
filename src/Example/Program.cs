using SimpleSvg2LineSegementInterpolater;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Example
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var lineSegments = await Interpolater.GenerateInterpolatedLineSegmentAsync(await File.ReadAllTextAsync(@"I:\zz.svg"), new InterpolaterOption()
            {
                DefaultStrokeColor = System.Drawing.Color.Pink
            });
            Debug.WriteLine($"before optimze points count: {lineSegments.Sum(x => x.Points.Count)}");
            foreach (var lineSegment in lineSegments)
                LineSegmentOptimzer.Optimze(lineSegment);
            Debug.WriteLine($"after optimze points count: {lineSegments.Sum(x => x.Points.Count)}");
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
