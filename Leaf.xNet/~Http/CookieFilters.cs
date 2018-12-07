using System;

namespace Leaf.xNet
{
    public static class CookieFilters
    {
        public static bool Enabled = true;

        public static bool Trim = true;
        public static bool Path = true;
        public static bool CommaEndingValue = true;

        /// <summary>
        /// Фильтруем Cookie для дальнейшего использования в нативном хранилище.
        /// </summary>
        /// <param name="rawCookie">Запись Cookie как строка со всеми параметрами</param>
        /// <returns>Отфильтрованная Cookie в виде строки со всеми отфильтрованными параметрами</returns>
        public static string Filter(string rawCookie)
        {
            return !Enabled ? rawCookie
                : rawCookie
                    .TrimWhitespace()
                    .FilterPath()
                    .FilterCommaEndingValue();
        }

        /// <summary>Убираем любые пробелы в начале и конце</summary>
        private static string TrimWhitespace(this string rawCookie)
        {
            return !Trim ? rawCookie : rawCookie.Trim();
        }

        /// <summary>Заменяем все значения path на "/"</summary>
        private static string FilterPath(this string rawCookie)
        {
            if (!Path)
                return rawCookie;

            const string path = "path=/";
            int pathIndex = rawCookie.IndexOf(path, 0, StringComparison.OrdinalIgnoreCase);
            if (pathIndex == -1)
                return rawCookie;

            pathIndex += path.Length;
            if (pathIndex >= rawCookie.Length - 1 || rawCookie[pathIndex] == ';')
                return rawCookie;

            int endPathIndex = rawCookie.IndexOf(';', pathIndex);
            if (endPathIndex == -1)
                endPathIndex = rawCookie.Length;

            return rawCookie.Remove(pathIndex, endPathIndex - pathIndex);
        }


        /// <summary>Заменяем значения кук завершающиеся запятой (escape)</summary>
        private static string FilterCommaEndingValue(this string rawCookie)
        {
            if (!CommaEndingValue)
                return rawCookie;

            int equalIndex = rawCookie.IndexOf('=');
            if (equalIndex == -1 || equalIndex >= rawCookie.Length - 1)
                return rawCookie;

            int endValueIndex = rawCookie.IndexOf(';', equalIndex + 1);
            if (endValueIndex == -1)
                endValueIndex = rawCookie.Length - 1;

            int lastCharIndex = endValueIndex - 1;
            return rawCookie[lastCharIndex] != ','
                ? rawCookie
                : rawCookie.Remove(lastCharIndex, 1).Insert(lastCharIndex, "%2C");
        }
    }
}