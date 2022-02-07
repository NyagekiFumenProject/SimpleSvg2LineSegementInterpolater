using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Io;
using SimpleSvg2LineSegementInterpolater.Base;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public static async Task<List<LineSegementCollection>> GenerateInterpolatedLineSegmentAsync(string svgFileContent)
        {
            var document = await GenerateDocument(svgFileContent, "image/svg+xml");
            var svg = document.QuerySelector("svg");

            return svg.Children.SelectMany(x => GenerateInterpolatedLineSegment(x)).ToList();
        }

        public static List<LineSegementCollection> GenerateInterpolatedLineSegment(IElement element)
        {
            var collections = new List<LineSegementCollection>();

            void AppendResults(LineSegementCollection collection) => collections.Add(collection);

            switch (element.NodeName)
            {
                case "rect":
                    AppendResults(GenerateInterpolatedLineSegmentByRect(element));
                    break;
                case "g":

                    break;
                default:
                    break;
            }

            //todo overwrite props
            var fill = element.Attributes.TryGetAttrValue("fill", default(Color));
            if (fill != default)
                foreach (var item in collections)
                    item.Color = fill;

            return collections;
        }

        public static LineSegementCollection GenerateInterpolatedLineSegmentByRect(IElement element)
        {
            var baseX = element.Attributes.TryGetAttrValue("x", 0f);
            var baseY = element.Attributes.TryGetAttrValue("y", 0f);
            var width = element.Attributes.TryGetAttrValue("width", 0f);
            var height = element.Attributes.TryGetAttrValue("height", 0f);
            var stroke = element.Attributes.TryGetAttrValue("stroke", default(Color));
            var fill = element.Attributes.TryGetAttrValue("fill", default(Color));

            var collection = new LineSegementCollection();

            collection.Points = new List<PointF>();
            collection.Points.Add(new(baseX, baseY));
            collection.Points.Add(new(baseX + width, baseY));
            collection.Points.Add(new(baseX + width, baseY + height));
            collection.Points.Add(new(baseX, baseY + height));

            //手动闭环
            collection.Points.Add(new(baseX, baseY));

            collection.Color = stroke;

            return collection;
        }
    }
}
