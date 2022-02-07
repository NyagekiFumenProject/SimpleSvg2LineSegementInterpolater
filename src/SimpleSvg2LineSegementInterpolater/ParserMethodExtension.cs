using AngleSharp.Dom;
using System;
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
    }
}
