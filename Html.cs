using System;
using System.Globalization;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Leaf.Net
{
    /// <summary>
    /// Static class for help working with HTML and other text data
    /// </summary>
    /// TODO: refactor. Remove dub code.
    public static class Html
    {
        #region Static Fields (private)

        private static readonly Dictionary<string, string> HtmlMnemonics = new Dictionary<string, string>()
        {
            { "apos", "'" },
            { "quot", "\"" },
            { "amp", "&" },
            { "lt", "<" },
            { "gt", ">" }
        };

        #endregion


        #region Static Methods (public)

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
                    string value;

                    if (HtmlMnemonics.TryGetValue(match.Groups["text"].Value, out value))
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

        #region Работа со строками

        /// <summary>
        /// Извлекает подстроку из строки. Подстрока начинается с конца позиции подстроки <paramref name="left"/> и до конца строки. Поиск начинается с заданной позиции.
        /// </summary>
        /// <param name="str">Строка, в которой будет поиск подстроки.</param>
        /// <param name="left">Строка, которая находится слева от искомой подстроки.</param>
        /// <param name="startIndex">Позиция, с которой начинается поиск подстроки. Отсчёт от 0.</param>
        /// <param name="comparsion">Одно из значений перечисления, определяющее правила поиска.</param>
        /// <returns>Найденая подстрока, иначе пустая строка.</returns>
        /// <exception cref="System.ArgumentNullException">Значение параметра <paramref name="left"/> равно <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentException">Значение параметра <paramref name="left"/> является пустой строкой.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Значение параметра <paramref name="startIndex"/> меньше 0.
        /// -или-
        /// Значение параметра <paramref name="startIndex"/> равно или больше длины строки <paramref name="str"/>.
        /// </exception>
        public static string Substring(this string str, string left,
            int startIndex, StringComparison comparsion = StringComparison.Ordinal)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;

            #region Проверка параметров

            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (left.Length == 0)
                throw ExceptionHelper.EmptyString(nameof(left));

            if (startIndex < 0)
                throw ExceptionHelper.CanNotBeLess(nameof(startIndex), 0);

            if (startIndex >= str.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex),
                    Resources.ArgumentOutOfRangeException_StringHelper_MoreLengthString);
            }

            #endregion

            // Ищем начало позиции левой подстроки.
            int leftPosBegin = str.IndexOf(left, startIndex, comparsion);

            if (leftPosBegin == -1)
                return string.Empty;

            // Вычисляем конец позиции левой подстроки.
            int leftPosEnd = leftPosBegin + left.Length;

            // Вычисляем длину найденной подстроки.
            int length = str.Length - leftPosEnd;

            return str.Substring(leftPosEnd, length);
        }

        /// <summary>
        /// Извлекает подстроку из строки. Подстрока начинается с конца позиции подстроки <paramref name="left"/> и до конца строки.
        /// </summary>
        /// <param name="str">Строка, в которой будет поиск подстроки.</param>
        /// <param name="left">Строка, которая находится слева от искомой подстроки.</param>
        /// <param name="comparsion">Одно из значений перечисления, определяющее правила поиска.</param>
        /// <returns>Найденая подстрока, иначе пустая строка.</returns>
        /// <exception cref="System.ArgumentNullException">Значение параметра <paramref name="left"/> равно <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentException">Значение параметра <paramref name="left"/> является пустой строкой.</exception>
        public static string Substring(this string str,
            string left, StringComparison comparsion = StringComparison.Ordinal)
        {
            return Substring(str, left, 0, comparsion);
        }

        /// <summary>
        /// Извлекает подстроку из строки. Подстрока ищется между двумя заданными строками, начиная с заданной позиции.
        /// </summary>
        /// <param name="str">Строка, в которой будет поиск подстроки.</param>
        /// <param name="left">Строка, которая находится слева от искомой подстроки.</param>
        /// <param name="right">Строка, которая находится справа от искомой подстроки.</param>
        /// <param name="startIndex">Позиция, с которой начинается поиск подстроки. Отсчёт от 0.</param>
        /// <param name="comparsion">Одно из значений перечисления, определяющее правила поиска.</param>
        /// <returns>Найденая подстрока, иначе пустая строка.</returns>
        /// <exception cref="System.ArgumentNullException">Значение параметра <paramref name="left"/> или <paramref name="right"/> равно <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentException">Значение параметра <paramref name="left"/> или <paramref name="right"/> является пустой строкой.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Значение параметра <paramref name="startIndex"/> меньше 0.
        /// -или-
        /// Значение параметра <paramref name="startIndex"/> равно или больше длины строки <paramref name="str"/>.
        /// </exception>
        public static string Substring(this string str, string left, string right,
            int startIndex, StringComparison comparsion = StringComparison.Ordinal)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;

            #region Проверка параметров

            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (left.Length == 0)
                throw ExceptionHelper.EmptyString(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (right.Length == 0)
                throw ExceptionHelper.EmptyString(nameof(right));

            if (startIndex < 0)
                throw ExceptionHelper.CanNotBeLess(nameof(startIndex), 0);

            if (startIndex >= str.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex),
                    Resources.ArgumentOutOfRangeException_StringHelper_MoreLengthString);

            #endregion

            // Ищем начало позиции левой подстроки.
            int leftPosBegin = str.IndexOf(left, startIndex, comparsion);

            if (leftPosBegin == -1)
                return string.Empty;

            // Вычисляем конец позиции левой подстроки.
            int leftPosEnd = leftPosBegin + left.Length;

            // Ищем начало позиции правой подстроки.
            int rightPos = str.IndexOf(right, leftPosEnd, comparsion);

            if (rightPos == -1)
                return string.Empty;

            // Вычисляем длину найденной подстроки.
            int length = rightPos - leftPosEnd;

            return str.Substring(leftPosEnd, length);
        }

        /// <summary>
        /// Извлекает подстроку из строки. Подстрока ищется между двумя заданными строками.
        /// </summary>
        /// <param name="str">Строка, в которой будет поиск подстроки.</param>
        /// <param name="left">Строка, которая находится слева от искомой подстроки.</param>
        /// <param name="right">Строка, которая находится справа от искомой подстроки.</param>
        /// <param name="comparsion">Одно из значений перечисления, определяющее правила поиска.</param>
        /// <returns>Найденая подстрока, иначе пустая строка.</returns>
        /// <exception cref="System.ArgumentNullException">Значение параметра <paramref name="left"/> или <paramref name="right"/> равно <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentException">Значение параметра <paramref name="left"/> или <paramref name="right"/> является пустой строкой.</exception>
        public static string Substring(this string str, string left, string right,
            StringComparison comparsion = StringComparison.Ordinal)
        {
            return str.Substring(left, right, 0, comparsion);
        }

        /// <summary>
        /// Извлекает последнею подстроку из строки. Подстрока начинается с конца позиции подстроки <paramref name="left"/> и до конца строки. Поиск начинается с заданной позиции.
        /// </summary>
        /// <param name="str">Строка, в которой будет поиск последней подстроки.</param>
        /// <param name="left">Строка, которая находится слева от искомой подстроки.</param>
        /// <param name="startIndex">Позиция, с которой начинается поиск подстроки. Отсчёт от 0.</param>
        /// <param name="comparsion">Одно из значений перечисления, определяющее правила поиска.</param>
        /// <returns>Найденая подстрока, иначе пустая строка.</returns>
        /// <exception cref="System.ArgumentNullException">Значение параметра <paramref name="left"/> равно <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentException">Значение параметра <paramref name="left"/> является пустой строкой.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Значение параметра <paramref name="startIndex"/> меньше 0.
        /// -или-
        /// Значение параметра <paramref name="startIndex"/> равно или больше длины строки <paramref name="str"/>.
        /// </exception>
        public static string LastSubstring(this string str, string left,
            int startIndex, StringComparison comparsion = StringComparison.Ordinal)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;

            #region Проверка параметров

            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (left.Length == 0)
                throw ExceptionHelper.EmptyString(nameof(left));

            if (startIndex < 0)
                throw ExceptionHelper.CanNotBeLess(nameof(startIndex), 0);

            if (startIndex >= str.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex),
                    Resources.ArgumentOutOfRangeException_StringHelper_MoreLengthString);
            }

            #endregion

            // Ищем начало позиции левой подстроки.
            int leftPosBegin = str.LastIndexOf(left, startIndex, comparsion);

            if (leftPosBegin == -1)
                return string.Empty;

            // Вычисляем конец позиции левой подстроки.
            int leftPosEnd = leftPosBegin + left.Length;

            // Вычисляем длину найденной подстроки.
            int length = str.Length - leftPosEnd;

            return str.Substring(leftPosEnd, length);
        }

        /// <summary>
        /// Извлекает последнею подстроку из строки. Подстрока начинается с конца позиции подстроки <paramref name="left"/> и до конца строки.
        /// </summary>
        /// <param name="str">Строка, в которой будет поиск последней подстроки.</param>
        /// <param name="left">Строка, которая находится слева от искомой подстроки.</param>
        /// <param name="comparsion">Одно из значений перечисления, определяющее правила поиска.</param>
        /// <returns>Найденая подстрока, иначе пустая строка.</returns>
        /// <exception cref="System.ArgumentNullException">Значение параметра <paramref name="left"/> равно <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentException">Значение параметра <paramref name="left"/> является пустой строкой.</exception>
        public static string LastSubstring(this string str,
            string left, StringComparison comparsion = StringComparison.Ordinal)
        {
            return !string.IsNullOrEmpty(str)
                ? LastSubstring(str, left, str.Length - 1, comparsion)
                : string.Empty;
        }

        /// <summary>
        /// Извлекает последнею подстроку из строки. Подстрока ищется между двумя заданными строками, начиная с заданной позиции.
        /// </summary>
        /// <param name="str">Строка, в которой будет поиск последней подстроки.</param>
        /// <param name="left">Строка, которая находится слева от искомой подстроки.</param>
        /// <param name="right">Строка, которая находится справа от искомой подстроки.</param>
        /// <param name="startIndex">Позиция, с которой начинается поиск подстроки. Отсчёт от 0.</param>
        /// <param name="comparsion">Одно из значений перечисления, определяющее правила поиска.</param>
        /// <returns>Найденая подстрока, иначе пустая строка.</returns>
        /// <exception cref="System.ArgumentNullException">Значение параметра <paramref name="left"/> или <paramref name="right"/> равно <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentException">Значение параметра <paramref name="left"/> или <paramref name="right"/> является пустой строкой.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Значение параметра <paramref name="startIndex"/> меньше 0.
        /// -или-
        /// Значение параметра <paramref name="startIndex"/> равно или больше длины строки <paramref name="str"/>.
        /// </exception>
        public static string LastSubstring(this string str, string left, string right, int startIndex, StringComparison comparsion = StringComparison.Ordinal)
        {
            while (true)
            {
                if (string.IsNullOrEmpty(str))
                    return string.Empty;

                #region Проверка параметров

                if (left == null)
                    throw new ArgumentNullException(nameof(left));

                if (left.Length == 0)
                    throw ExceptionHelper.EmptyString(nameof(left));

                if (right == null)
                    throw new ArgumentNullException(nameof(right));

                if (right.Length == 0)
                    throw ExceptionHelper.EmptyString(nameof(right));

                if (startIndex < 0)
                    throw ExceptionHelper.CanNotBeLess(nameof(startIndex), 0);

                if (startIndex >= str.Length)
                    throw new ArgumentOutOfRangeException(nameof(startIndex),
                        Resources.ArgumentOutOfRangeException_StringHelper_MoreLengthString);

                #endregion

                // Ищем начало позиции левой подстроки.
                int leftPosBegin = str.LastIndexOf(left, startIndex, comparsion);

                if (leftPosBegin == -1)
                    return string.Empty;

                // Вычисляем конец позиции левой подстроки.
                int leftPosEnd = leftPosBegin + left.Length;

                // Ищем начало позиции правой подстроки.
                int rightPos = str.IndexOf(right, leftPosEnd, comparsion);

                if (rightPos == -1)
                {
                    if (leftPosBegin == 0)
                        return string.Empty;

                    startIndex = leftPosBegin - 1;
                    continue;
                }

                // Вычисляем длину найденной подстроки.
                int length = rightPos - leftPosEnd;

                return str.Substring(leftPosEnd, length);
            }
        }

        /// <summary>
        /// Извлекает последнею подстроку из строки. Подстрока ищется между двумя заданными строками.
        /// </summary>
        /// <param name="str">Строка, в которой будет поиск последней подстроки.</param>
        /// <param name="left">Строка, которая находится слева от искомой подстроки.</param>
        /// <param name="right">Строка, которая находится справа от искомой подстроки.</param>
        /// <param name="comparsion">Одно из значений перечисления, определяющее правила поиска.</param>
        /// <returns>Найденая подстрока, иначе пустая строка.</returns>
        /// <exception cref="System.ArgumentNullException">Значение параметра <paramref name="left"/> или <paramref name="right"/> равно <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentException">Значение параметра <paramref name="left"/> или <paramref name="right"/> является пустой строкой.</exception>
        public static string LastSubstring(this string str, string left, string right,
            StringComparison comparsion = StringComparison.Ordinal)
        {
            return !string.IsNullOrEmpty(str)
                ? str.LastSubstring(left, right, str.Length - 1, comparsion)
                : string.Empty;
        }

        /// <summary>
        /// Извлекает подстроки из строки. Подстрока ищется между двумя заданными строками, начиная с заданной позиции.
        /// </summary>
        /// <param name="str">Строка, в которой будет поиск подстрок.</param>
        /// <param name="left">Строка, которая находится слева от искомой подстроки.</param>
        /// <param name="right">Строка, которая находится справа от искомой подстроки.</param>
        /// <param name="startIndex">Позиция, с которой начинается поиск подстрок. Отсчёт от 0.</param>
        /// <param name="comparsion">Одно из значений перечисления, определяющее правила поиска.</param>
        /// <returns>Найденые подстроки, иначе пустой массив строк.</returns>
        /// <exception cref="System.ArgumentNullException">Значение параметра <paramref name="left"/> или <paramref name="right"/> равно <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentException">Значение параметра <paramref name="left"/> или <paramref name="right"/> является пустой строкой.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Значение параметра <paramref name="startIndex"/> меньше 0.
        /// -или-
        /// Значение параметра <paramref name="startIndex"/> равно или больше длины строки <paramref name="str"/>.
        /// </exception>
        public static string[] Substrings(this string str, string left, string right,
            int startIndex, StringComparison comparsion = StringComparison.Ordinal)
        {
            if (string.IsNullOrEmpty(str))
                return new string[0];

            #region Проверка параметров

            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (left.Length == 0)
                throw ExceptionHelper.EmptyString(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (right.Length == 0)
                throw ExceptionHelper.EmptyString(nameof(right));

            if (startIndex < 0)
                throw ExceptionHelper.CanNotBeLess(nameof(startIndex), 0);

            if (startIndex >= str.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex),
                    Resources.ArgumentOutOfRangeException_StringHelper_MoreLengthString);
            }

            #endregion

            int currentStartIndex = startIndex;
            var strings = new List<string>();

            while (true)
            {
                // Ищем начало позиции левой подстроки.
                int leftPosBegin = str.IndexOf(left, currentStartIndex, comparsion);
                if (leftPosBegin == -1)
                    break;

                // Вычисляем конец позиции левой подстроки.
                int leftPosEnd = leftPosBegin + left.Length;

                // Ищем начало позиции правой строки.
                int rightPos = str.IndexOf(right, leftPosEnd, comparsion);

                if (rightPos == -1)
                    break;

                // Вычисляем длину найденной подстроки.
                int length = rightPos - leftPosEnd;

                strings.Add(str.Substring(leftPosEnd, length));

                // Вычисляем конец позиции правой подстроки.
                currentStartIndex = rightPos + right.Length;
            }

            return strings.ToArray();
        }

        /// <summary>
        /// Извлекает подстроки из строки. Подстрока ищется между двумя заданными строками.
        /// </summary>
        /// <param name="str">Строка, в которой будет поиск подстрок.</param>
        /// <param name="left">Строка, которая находится слева от искомой подстроки.</param>
        /// <param name="right">Строка, которая находится справа от искомой подстроки.</param>
        /// <param name="comparsion">Одно из значений перечисления, определяющее правила поиска.</param>
        /// <returns>Найденые подстроки, иначе пустой массив строк.</returns>
        /// <exception cref="System.ArgumentNullException">Значение параметра <paramref name="left"/> или <paramref name="right"/> равно <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentException">Значение параметра <paramref name="left"/> или <paramref name="right"/> является пустой строкой.</exception>
        public static string[] Substrings(this string str, string left, string right,
            StringComparison comparsion = StringComparison.Ordinal)
        {
            return str.Substrings(left, right, 0, comparsion);
        }

        #endregion

        #endregion
    }
}
