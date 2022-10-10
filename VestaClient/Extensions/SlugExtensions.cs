using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace VestaClient.Extensions
{
    internal static class SlugExtensions
    {
        public static string ToSlug(this string title, bool remapToAscii = true, int maxlength = 200)
        {
            if (title == null)
            {
                return string.Empty;
            }

            var length = title.Length;
            var prevdash = false;
            var stringBuilder = new StringBuilder(length);

            for (var i = 0; i < length; ++i)
            {
                var c = title[i];
                if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
                {
                    stringBuilder.Append(c);
                    prevdash = false;
                }
                else if (c >= 'A' && c <= 'Z')
                {
                    // tricky way to convert to lower-case
                    stringBuilder.Append((char) (c | 32));
                    prevdash = false;
                }
                else if ((c == ' ') || (c == ',') || (c == '.') || (c == '/') ||
                         (c == '\\') || (c == '-') || (c == '_') || (c == '='))
                {
                    if (!prevdash && (stringBuilder.Length > 0))
                    {
                        stringBuilder.Append('-');
                        prevdash = true;
                    }
                }
                else if (c >= 128)
                {
                    var previousLength = stringBuilder.Length;

                    if (remapToAscii)
                    {
                        stringBuilder.Append(RemapInternationalCharToAscii(c));
                    }
                    else
                    {
                        stringBuilder.Append(c);
                    }

                    if (previousLength != stringBuilder.Length)
                    {
                        prevdash = false;
                    }
                }

                if (i == maxlength)
                {
                    break;
                }
            }

            return prevdash
                ? stringBuilder.ToString().Substring(0, stringBuilder.Length - 1)
                : stringBuilder.ToString();
        }

        public static string ToFileName(this string name)
        {
            var regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            var r = new Regex($"[{Regex.Escape(regexSearch)}]");
            name = r.Replace(name, "");
            return name;
        }

        private static string RemapInternationalCharToAscii(char character)
        {
            var s = character.ToString().ToLowerInvariant();

            if ("àåáâäãåąā".Contains(s))
            {
                return "a";
            }

            if ("èéêëę".Contains(s))
            {
                return "e";
            }

            if ("ìíîïıİ".Contains(s))
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

            if (character == 'ř')
            {
                return "r";
            }

            if (character == 'ł')
            {
                return "l";
            }

            if (character == 'đ')
            {
                return "d";
            }

            if (character == 'ß')
            {
                return "ss";
            }

            if (character == 'Þ')
            {
                return "th";
            }

            if (character == 'ĥ')
            {
                return "h";
            }

            if (character == 'ĵ')
            {
                return "j";
            }

            if (character == 'ə')
            {
                return "e";
            }

            return string.Empty;
        }
    }
}