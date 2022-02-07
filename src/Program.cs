using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Io;
using AngleSharp.Svg.Dom;
using AngleSharp.Xml.Parser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleSvg2LineSegementInterpolater
{
    class Program
    {
        private static Task<IDocument> GenerateDocument(string content, string contentType)
        {
            var config = Configuration.Default.WithDefaultLoader().WithXml();
            var context = BrowsingContext.New(config);
            return context.OpenAsync(res =>
            {
                res.Content(content);

                if (!String.IsNullOrEmpty(contentType))
                {
                    res.Header(HeaderNames.ContentType, contentType);
                }
            });
        }

        static async Task Main(string[] args)
        {
            var document = await GenerateDocument(File.ReadAllText(@"I:\zz.svg"), "image/svg+xml");
            var svg = document.QuerySelector("svg");

            var lineSegments = svg.Children.SelectMany(x => Interpolater.GenerateInterpolatedLineSegment(x)).ToArray();

            using var image = Drawing.DrawToImage(lineSegments);
            Drawing.SaveImageToFile(@"I:\zz.png", image);
        }
    }
}
