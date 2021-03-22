using System.Text;

namespace Core.Extensions
{
    public static class SlugExtension
    {
        // From https://stackoverflow.com/questions/25259/how-does-stack-overflow-generate-its-seo-friendly-urls
        /// <summary>
        /// Produces optional, URL-friendly version of a title, "like-this-one".
        /// hand-tuned for speed, reflects performance refactoring contributed
        /// by John Gietzen (user otac0n)
        /// </summary>
        public static string Slugify(this string title)
        {
            if (title == null) return "";

            const int maxlen = 80;
            var len = title.Length;
            var prevdash = false;
            var sb = new StringBuilder(len);
            char c;

            for (var i = 0; i < len; i++)
            {
                c = title[i];
                switch (c)
                {
                    case >= 'a' and <= 'z':
                    case >= '0' and <= '9':
                        sb.Append(c);
                        prevdash = false;
                        break;
                    case >= 'A' and <= 'Z':
                        // tricky way to convert to lowercase
                        sb.Append((char)(c | 32));
                        prevdash = false;
                        break;
                    case ' ':
                    case ',':
                    case '.':
                    case '/':
                    case '\\':
                    case '-':
                    case '_':
                    case '=':
                    {
                        if (!prevdash && sb.Length > 0)
                        {
                            sb.Append('-');
                            prevdash = true;
                        }

                        break;
                    }
                    default:
                    {
                        if (c >= 128)
                        {
                            var prevlen = sb.Length;
                            sb.Append(c.RemapInternationalCharToAscii());
                            if (prevlen != sb.Length) prevdash = false;
                        }

                        break;
                    }
                }
                if (i == maxlen) break;
            }

            return prevdash ? sb.ToString().Substring(0, sb.Length - 1) : sb.ToString();
        }

        // From https://meta.stackexchange.com/questions/7435/non-us-ascii-characters-dropped-from-full-profile-url/7696#7696
        private static string RemapInternationalCharToAscii(this char c)
        {
            string s = c.ToString().ToLowerInvariant();
            if ("àåáâäãåą".Contains(s))
            {
                return "a";
            }

            if ("èéêëę".Contains(s))
            {
                return "e";
            }

            if ("ìíîïı".Contains(s))
            {
                return "i";
            }

            if ("òóôõöøőð".Contains(s))
            {
                return "o";
            }

            if ("ùúûüŭů".Contains(s))
            {
                return "u";
            }

            if ("çćčĉ".Contains(s))
            {
                return "c";
            }

            if ("żźž".Contains(s))
            {
                return "z";
            }

            if ("śşšŝ".Contains(s))
            {
                return "s";
            }

            if ("ñń".Contains(s))
            {
                return "n";
            }

            if ("ýÿ".Contains(s))
            {
                return "y";
            }

            if ("ğĝ".Contains(s))
            {
                return "g";
            }

            if (c == 'ř')
            {
                return "r";
            }

            if (c == 'ł')
            {
                return "l";
            }

            if (c == 'đ')
            {
                return "d";
            }

            if (c == 'ß')
            {
                return "ss";
            }

            if (c == 'Þ')
            {
                return "th";
            }

            if (c == 'ĥ')
            {
                return "h";
            }

            if (c == 'ĵ')
            {
                return "j";
            }

            return "";
        }
    }
}