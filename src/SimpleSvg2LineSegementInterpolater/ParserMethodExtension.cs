using AngleSharp;
using AngleSharp.Common;
using AngleSharp.Css;
using AngleSharp.Css.Dom;
using AngleSharp.Css.Parser;
using AngleSharp.Dom;
using AngleSharp.Text;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSvg2LineSegementInterpolater
{
    internal static class ParserMethodExtension
    {
        public static int TryToInt(this string str, int defaultVal = default)
            => int.TryParse(str, out var d) ? d : defaultVal;

        public static long TryToLong(this string str, long defaultVal = default)
            => long.TryParse(str, out var d) ? d : defaultVal;

        public static bool TryToBool(this string str, bool defaultVal = default)
            => bool.TryParse(str, out var d) ? d : defaultVal;

        public static float TryToFloat(this string str, float defaultVal = default)
            => float.TryParse(str, out var d) ? d : defaultVal;

        public static double TryToDouble(this string str, double defaultVal = default)
            => double.TryParse(str, out var d) ? d : defaultVal;

        public static Color TryToColor(this string str, Color defaultVal = default)
        {
            if (str == "none")
                return defaultVal;
            if (str.StartsWith("rgb("))
            {
                var color = str.ReplaceFirst("rgb(", string.Empty).ReplaceFirst(")", string.Empty).Split(",");
                return Color.FromArgb(255, color[0].ToInteger(0), color[1].ToInteger(0), color[2].ToInteger(0));
            }
            if (str.StartsWith("rgba("))
            {
                var color = str.ReplaceFirst("rgba(", string.Empty).ReplaceFirst(")", string.Empty).Split(",");
                return Color.FromArgb((int)(color[3].ToDouble(0d) * 255), color[0].ToInteger(0), color[1].ToInteger(0), color[2].ToInteger(0));
            }

            return ColorTranslator.FromHtml(str);
        }

        public static int TryGetAttrValue(this INamedNodeMap map, string attrName, int defaultVal = default)
            => map[attrName]?.Value?.TryToInt(defaultVal) ?? defaultVal;

        public static double TryGetAttrValue(this INamedNodeMap map, string attrName, double defaultVal = default)
            => map[attrName]?.Value?.TryToDouble(defaultVal) ?? defaultVal;

        public static long TryGetAttrValue(this INamedNodeMap map, string attrName, long defaultVal = default)
            => map[attrName]?.Value?.TryToLong(defaultVal) ?? defaultVal;

        public static float TryGetAttrValue(this INamedNodeMap map, string attrName, float defaultVal = default)
            => map[attrName]?.Value?.TryToFloat(defaultVal) ?? defaultVal;

        public static bool TryGetAttrValue(this INamedNodeMap map, string attrName, bool defaultVal = default)
            => map[attrName]?.Value?.TryToBool(defaultVal) ?? defaultVal;

        public static Color TryGetAttrValue(this INamedNodeMap map, string attrName, Color defaultVal = default)
            => map[attrName]?.Value?.TryToColor(defaultVal) ?? defaultVal;

        public static string TryGetAttrValue(this INamedNodeMap map, string attrName, string defaultVal = default)
            => map[attrName]?.Value ?? defaultVal;

        private static CssParser parser = new CssParser();

        public static Color TryGetFill(this IElement element, InterpolaterOption option)
        {
            var fillFromAttr = element.Attributes.TryGetAttrValue("fill", default(Color));
            if (fillFromAttr != default(Color))
                return fillFromAttr;

            var result = parser.ParseDeclaration(element.GetAttribute("style"));
            var fillStyleColor = result.FirstOrDefault(x => x.Name == "fill")?.RawValue;

            if (fillStyleColor is not null)
            {
                var rgba = fillStyleColor.AsRgba();
                var color = Color.FromArgb((rgba << (3 * 8)) | (rgba >> 8));
                return color;
            }

            return default;
        }

        public static Color TryGetStoke(this IElement element, InterpolaterOption option, bool enableDefaultValue = true)
        {
            var fill = element.TryGetFill(option);
            var strokeBackValue = option.EnableFillAsStroke ? fill : default;
            strokeBackValue = strokeBackValue == default ? (enableDefaultValue ? option.DefaultStrokeColor : default) : strokeBackValue;

            var stroke = element.Attributes.TryGetAttrValue("stroke", default(Color));
            if (stroke == default)
            {
                var result = parser.ParseDeclaration(element.GetAttribute("style"));
                var strokeStyleColor = result.FirstOrDefault(x => x.Name == "stroke")?.RawValue;
                if (strokeStyleColor is not null)
                {
                    var rgba = strokeStyleColor.AsRgba();
                    var color = Color.FromArgb((rgba << (3 * 8)) | (rgba >> 8));
                    return color;
                }

                return strokeBackValue;
            }

            return stroke;
        }

        public static (T min, T max) MaxMinBy<T>(this IEnumerable<T> collection)
            => collection.MaxMinBy(x => x, (a, b) => Comparer<T>.Default.Compare(a, b));
        public static (T min, T max) MaxMinBy<T>(this IEnumerable<T> collection, Func<T, T, int> comparer = default)
            => collection.MaxMinBy(x => x, comparer);
        public static (X min, X max) MaxMinBy<T, X>(this IEnumerable<T> collection, Func<T, X> keySelector, Func<X, X, int> comparer)
        {
            var first = collection.FirstOrDefault();
            X min = keySelector(first);
            X max = keySelector(first);

            foreach (var item in collection)
            {
                var v = keySelector(item);
                if (comparer(min, v) > 0)
                    min = v;
                if (comparer(v, max) > 0)
                    max = v;
            }

            return (min, max);
        }
    }
}
