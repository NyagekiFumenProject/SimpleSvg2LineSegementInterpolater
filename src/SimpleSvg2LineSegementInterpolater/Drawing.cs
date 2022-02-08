using SimpleSvg2LineSegementInterpolater.Base;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSvg2LineSegementInterpolater
{
    public static class Drawing
    {
        public static void SaveImageToFile(string filePath, Image image)
        {
            image.Save(filePath, ImageFormat.Png);
        }

        public static Image DrawToImage(IEnumerable<LineSegementCollection> lineSegements)
        {
            //calculate size
            var width = (int)Math.Floor(lineSegements.SelectMany(x => x.Points).Max(x => x.X) + 1);
            var height = (int)Math.Floor(lineSegements.SelectMany(x => x.Points).Max(x => x.Y) + 1);

            var bitmap = new Bitmap(width, height);
            using var graphics = Graphics.FromImage(bitmap);

            foreach (var lineSegement in lineSegements)
                Draw(graphics, lineSegement);

            graphics.Flush();

            return bitmap;
        }

        private static void Draw(Graphics graphics, LineSegementCollection lineSegement)
        {
            if (lineSegement.Points.Count == 0)
                return;
            using var pen = new Pen(lineSegement.Color);
            graphics.DrawLines(pen, lineSegement.Points.ToArray());
        }
    }
}
