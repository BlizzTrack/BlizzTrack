using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Core.Extensions
{
    public static class ListExtensions
    {
        public static IOrderedEnumerable<T> OrderByAlphaNumeric<T>(this IEnumerable<T> source, Func<T, string> selector)
        {
            var enumerable = source as T[] ?? source.ToArray();
            var max = enumerable.SelectMany(i => Regex.Matches(selector(i), @"\d+").Select(m => (int?)m.Value.Length)).Max() ?? 0;
            return enumerable.OrderBy(i => Regex.Replace(selector(i), @"\d+", m => m.Value.PadLeft(max, '0')));
        }
    }
}