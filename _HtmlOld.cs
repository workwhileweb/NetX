// Compile disabled
namespace Leaf.Net
{
    public static class _HtmlOld
    {
        #region Static Fields (private)

        private static readonly Dictionary<string, string> HtmlMnemonics = new Dictionary<string, string>
        {
            { "apos", "'" },
            { "quot", "\"" },
            { "amp", "&" },
            { "lt", "<" },
            { "gt", ">" }
        };

        #endregion

        /// <summary>
        /// Replace in Html-Entites on symbols
        /// </summary>
        /// <param name="str">String in which replacement will be made.</param>
        /// <returns>A string replaced with HTML-entities.</returns>
        /// <remarks>Replace only with the following mnemonics: apos, quot, amp, lt и gt. And all types codes.</remarks>
        public static string ReplaceEntities(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return string.Empty;
            }

            var regex = new Regex(@"(\&(?<text>\w{1,4})\;)|(\&#(?<code>\w{1,4})\;)", RegexOptions.Compiled);

            string result = regex.Replace(str, match =>
            {
                if (match.Groups["text"].Success)
                {
                    if (HtmlMnemonics.TryGetValue(match.Groups["text"].Value, out string value))
                        return value;
                }
                else if (match.Groups["code"].Success)
                {
                    int code = int.Parse(match.Groups["code"].Value);
                    return ((char)code).ToString();
                }

                return match.Value;
            });

            return result;
        }

        /// <summary>
        /// Replace in string Unicode-entities on symbols.
        /// </summary>
        /// <param name="str">String in which replacement will be made.</param>
        /// <returns>>A string replaced with Unicode-entites.</returns>
        /// <remarks>Unicode-enities: \u2320 or \U044F</remarks>
        public static string ReplaceUnicode(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;

            var regex = new Regex(@"\\u(?<code>[0-9a-f]{4})", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            string result = regex.Replace(str, match =>
            {
                int code = int.Parse(match.Groups["code"].Value, NumberStyles.HexNumber);

                return ((char)code).ToString();
            });

            return result;
        }
    }
}