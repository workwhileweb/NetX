using System;
using System.Collections.Generic;
// ReSharper disable UnusedMember.Global

namespace Leaf.xNet.Extensions
{
    /// <summary>
    /// Static class for help working with HTML and other text data
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Проверяет наличие слова в строке, аналогично <see cref="string.Contains"/>, но без учета реестра и региональных стандартов.
        /// </summary>
        /// <param name="str">Строка для поиска слова</param>
        /// <param name="value">Слово которое должно содержаться в строке</param>
        /// <returns>Вернет истину если значение было найдено в строке</returns>
        public static bool ContainsIgnoreCase(this string str, string value)
        {
            return str.IndexOf(value, StringComparison.OrdinalIgnoreCase) != -1;
        }

        #region Вырезание нескольких строк        
        /// <summary>
        /// Вырезает несколько строк между двумя подстроками.
        /// </summary>
        /// <param name="str">Строка где следует искать подстроки</param>
        /// <param name="left">Начальная подстрока</param>
        /// <param name="right">Конечная подстрока</param>
        /// <param name="startIndex">Искать начиная с индекса</param>
        /// <param name="comparison">Метод сравнения строк</param>
        /// <param name="limit">Максимальное число подстрок для поиска</param>
        /// <returns>Возвращает массив подстрок которые попадают под шаблон</returns>
        public static string[] Substrings(this string str, string left, string right,
            int startIndex = 0, StringComparison comparison = StringComparison.Ordinal, int limit = 0)
        {
            #region Проверка параметров
            if (string.IsNullOrEmpty(str))
                return new string[0];

            if (string.IsNullOrEmpty(left))
                throw new ArgumentNullException(nameof(left));

            if (string.IsNullOrEmpty(right))
                throw new ArgumentNullException(nameof(right));

            if (startIndex < 0 || startIndex >= str.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex), Resources.StringExtensions_Substrings_Invalid_Index);
            #endregion

            int currentStartIndex = startIndex;            
            int current = limit;
            var strings = new List<string>();

            while (true)
            {
                if (limit > 0)
                {
                    --current;
                    if (current < 0)
                        break;
                }

                // Ищем начало позиции левой подстроки.
                int leftPosBegin = str.IndexOf(left, currentStartIndex, comparison);
                if (leftPosBegin == -1)
                    break;

                // Вычисляем конец позиции левой подстроки.
                int leftPosEnd = leftPosBegin + left.Length;
                // Ищем начало позиции правой строки.
                int rightPos = str.IndexOf(right, leftPosEnd, comparison);
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
        #endregion

        #region Вырезание одной подстроки. Прямой порядок (слева направо)
        /// <summary>
        /// Вырезает одну строку между двумя подстроками.
        /// </summary>
        /// <param name="str">Строка где следует искать подстроки</param>
        /// <param name="left">Начальная подстрока</param>
        /// <param name="right">Конечная подстрока</param>
        /// <param name="startIndex">Искать начиная с индекса</param>
        /// <param name="comparison">Метод сравнения строк</param>
        /// <param name="notFoundValue">Значение в случае если подстрока не найдена</param>
        /// <returns>Возвращает строку между двумя подстроками</returns>
        public static string Substring(this string str, string left, string right,
            int startIndex = 0, StringComparison comparison = StringComparison.Ordinal, string notFoundValue = "")
        {
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(left) || string.IsNullOrEmpty(right) ||
                startIndex < 0 || startIndex >= str.Length)
                return notFoundValue;

            // Ищем начало позиции левой подстроки.
            int leftPosBegin = str.IndexOf(left, startIndex, comparison);
            if (leftPosBegin == -1)
                return notFoundValue;

            // Вычисляем конец позиции левой подстроки.
            int leftPosEnd = leftPosBegin + left.Length;
            // Ищем начало позиции правой строки.
            int rightPos = str.IndexOf(right, leftPosEnd, comparison);

            return rightPos != -1 ? str.Substring(leftPosEnd, rightPos - leftPosEnd) : notFoundValue;
        }

        /// <summary>
        /// Вырезает одну строку между двумя подстроками. Возвращает null если строка не найдена.
        /// <remarks>Создана для удобства, для написания исключений через ?? тернарный оператор.</remarks>
        /// <example>
        /// str.SubNull("<tag>","</tag>") ?? throw new Exception("Не найдена строка");
        /// </example>
        /// </summary>
        /// <param name="str">Строка где следует искать подстроки</param>
        /// <param name="left">Начальная подстрока</param>
        /// <param name="right">Конечная подстрока</param>
        /// <param name="startIndex">Искать начиная с индекса</param>
        /// <param name="comparison">Метод сравнения строк</param>
        /// <returns>Возвращает строку между двумя подстроками. </returns>
        public static string SubstringNull(this string str, string left, string right,
            int startIndex = 0, StringComparison comparison = StringComparison.Ordinal)
        {
            return Substring(str, left, right, startIndex, comparison, null);
        }
        #endregion

        #region Вырезание одной подстроки. Обратный порядок (справа налево)
        /// <summary>
        /// Вырезает одну строку между двумя подстроками, только в обратном направлении поиска.
        /// </summary>
        /// <param name="str">Строка где следует искать подстроки</param>
        /// <param name="right">Конечная подстрока</param>
        /// <param name="left">Начальная подстрока</param>
        /// <param name="startIndex">Искать начиная с индекса
        /// <remarks>Если указано значение -1, поиск ведется с самого конца строки</remarks>
        /// </param>
        /// <param name="comparison">Метод сравнения строк</param>
        /// <param name="notFoundValue">Значение в случае если подстрока не найдена</param>
        /// <returns>Возвращает строку между двумя подстроками</returns>
        public static string LastSubstring(this string str, string right, string left,
            int startIndex = -1, StringComparison comparison = StringComparison.Ordinal,
            string notFoundValue = "")
        {
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(right) || string.IsNullOrEmpty(left) ||
                startIndex < -1 || startIndex >= str.Length)
                return notFoundValue;

            if (startIndex == -1)
                startIndex = str.Length - 1;

            // Ищем начало позиции правой подстроки с конца строки
            int rightPosBegin = str.LastIndexOf(right, startIndex, comparison);
            if (rightPosBegin == -1 || rightPosBegin == 0) // в обратном поиске имеет смысл проверять на 0
                return notFoundValue;

            // Вычисляем начало позиции левой подстроки
            int leftPosBegin = str.LastIndexOf(left, rightPosBegin - 1, comparison);
            // Если не найден левый конец или правая и левая подстрока склеены вместе - вернем пустую строку
            if (leftPosBegin == -1 || rightPosBegin - leftPosBegin == 1)
                return notFoundValue;

            int leftPosEnd = leftPosBegin + 1;
            return str.Substring(leftPosEnd, rightPosBegin - leftPosEnd);
        }

        /// <summary>
        /// Вырезает одну строку между двумя подстроками, только в обратном направлении поиска.
        /// Возвращает null если строка не найдена.
        /// <remarks>Создана для удобства, для написания исключений через ?? тернарный оператор.</remarks>
        /// <example>
        /// str.LastSubNull("bar","foo") ?? throw new Exception("Не найдена строка");
        /// </example>
        /// </summary>
        /// <param name="str">Строка где следует искать подстроки</param>
        /// <param name="right">Конечная подстрока</param>
        /// <param name="left">Начальная подстрока</param>
        /// <param name="startIndex">Искать начиная с индекса
        /// <remarks>Если указано значение -1, поиск ведется с самого конца строки</remarks>
        /// </param>
        /// <param name="comparison">Метод сравнения строк</param>
        /// <returns>Возвращает строку между двумя подстроками</returns>
        public static string LastSubstringNull(this string str, string right, string left,
            int startIndex = -1, StringComparison comparison = StringComparison.Ordinal)
        {
            return LastSubstring(str, right, left, startIndex, comparison, null);
        }

        #endregion
    }
}
