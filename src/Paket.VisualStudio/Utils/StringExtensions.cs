using System.Text;

namespace Paket.VisualStudio.Utils
{
    public static class StringExtensions
    {
        public static string TrimQuotes(this string s)
        {
            if (!string.IsNullOrEmpty(s))
            {
                char c = s[0];
                bool flag = c == '"' || c == '\'';
                int num = s.Length - (flag ? 1 : 0);
                if (s.Length > 1)
                {
                    char c2 = s[s.Length - 1];
                    bool flag2;
                    if (flag)
                    {
                        flag2 = (c2 == c);
                    }
                    else
                    {
                        flag2 = (c2 == '"' || c2 == '\'');
                    }
                    num -= (flag2 ? 1 : 0);
                }
                return s.Substring(flag ? 1 : 0, num);
            }
            return s;
        }
        public static string AddQuotes(this string s)
        {
            StringBuilder sb = new StringBuilder(s.Length + 2);
            sb.Append('"');
            sb.Append(s);
            sb.Append('"');
            return sb.ToString();
        }
        public static bool IsQuoted(this string s)
        {
            return s.Length > 1 && s.StartsWith("\"") && s.EndsWith("\"");
        }
    }
}
