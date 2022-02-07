using SimpleSvg2LineSegementInterpolater;
using System.IO;
using System.Threading.Tasks;

namespace Example
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var lineSegments = await Interpolater.GenerateInterpolatedLineSegmentAsync(await File.ReadAllTextAsync(@"I:\zz.svg"));
            using var image = Drawing.DrawToImage(lineSegments);
            Drawing.SaveImageToFile(@"I:\zz.png", image);
        }
    }
}
