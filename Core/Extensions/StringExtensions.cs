using System.Globalization;

namespace Core.Extensions
{
    public static class StringExtensions
    {
        public static string ToTitle(this string s)
        {
            return new CultureInfo("en").TextInfo.ToTitleCase(s.ToLower());
        }
    }
}