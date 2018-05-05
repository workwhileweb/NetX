using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;

namespace Leaf.Net
{
    [Serializable]
    public class CookieStorage
    {
        /// <summary>
        /// Оригинальный Cookie контейнер <see cref="CookieContainer"/> из .NET Framework.
        /// </summary>
        public CookieContainer Container { get; private set; }

        private static BinaryFormatter _binaryFormatter;
        private static BinaryFormatter Bf => _binaryFormatter ?? (_binaryFormatter = new BinaryFormatter());

        public CookieStorage(bool isLocked = false, CookieContainer container = null)
        {
            IsLocked = isLocked;
            Container = container ?? new CookieContainer();
        }

        /// <summary>
        /// Возвращает или задает значение, указывающие, закрыты ли куки для редактирования через ответы сервера.
        /// </summary>
        /// <value>Значение по умолчанию — <see langword="false"/>.</value>
        public bool IsLocked { get; set; }

        /// <summary>
        /// Добавляет или изменяет существующие Cookies в <see cref="CookieContainer"/>.
        /// </summary>
        /// <param name="cookies">Коллекция Cookie</param>
        public void Set(CookieCollection cookies)
        {
            Container.Add(cookies);
        }

        /// <inheritdoc cref="Set(System.Net.CookieCollection)"/>
        /// <param name="cookie">Кука</param>
        public void Set(Cookie cookie)
        {
            Container.Add(cookie);
        }

        /// <inheritdoc cref="Set(System.Net.CookieCollection)"/>
        /// <param name="name">Имя куки</param>
        /// <param name="value">Значение куки</param>
        /// <param name="domain">Домен (без протокола)</param>
        /// <param name="path">Путь</param>
        public void Set(string name, string value, string domain, string path = "/")
        {
            Container.Add(new Cookie(name, value, path, domain));
        }

        /// <inheritdoc cref="Set(System.Net.CookieCollection)"/>
        /// <param name="url">Url куки</param>
        /// <param name="rawCookie">Сырой формат записи в виде строки</param>
        public void Set(string url, string rawCookie)
        {
            Container.SetCookies(new Uri(url), rawCookie);
        }

        /// <inheritdoc cref="Set(System.Net.CookieCollection)"/>
        /// <param name="uri">Uri куки</param>
        /// <param name="rawCookie">Сырой формат записи в виде строки</param>
        public void Set(Uri uri, string rawCookie)
        {
            Container.SetCookies(uri, rawCookie);
        }

        /// <summary>
        /// Очистить <see cref="CookieContainer"/>.
        /// </summary>
        public void Clear()
        {
            Container = new CookieContainer();
        }

        /// <summary>
        /// Удалить все <see cref="Cookie"/> связанные с URL адресом.
        /// </summary>
        /// <param name="url">URL адрес ресурса</param>
        public void Remove(string url)
        {
            Remove(new Uri(url));
        }

        /// <inheritdoc cref="Remove(string)"/>
        /// <param name="uri">URI адрес ресурса</param>
        public void Remove(Uri uri)
        {
            var cookies = Container.GetCookies(uri);
            foreach (Cookie cookie in cookies)
                cookie.Expired = true;
        }

        /// <summary>
        /// Удалить <see cref="Cookie"/> по имени для определенного URL.
        /// </summary>
        /// <param name="url">URL ресурса</param>
        /// <param name="name">Имя куки которую нужно удалить</param>
        public void Remove(string url, string name)
        {
            Remove(new Uri(url), name);
        }

        /// <inheritdoc cref="Remove(string, string)"/>
        /// <param name="uri"></param>
        public void Remove(Uri uri, string name)
        {
            var cookies = Container.GetCookies(uri);
            foreach (Cookie cookie in cookies)
            {
                if (cookie.Name == name)
                    cookie.Expired = true;
            }
        }

        /// <summary>
        /// Получает Cookies в формате строки-заголовка для HTTP запроса (<see cref="HttpRequestHeader"/>).
        /// </summary>
        /// <param name="uri">URI адрес ресурса</param>
        /// <returns>Вернет строку содержащую все куки для адреса.</returns>
        public string GetCookieHeader(Uri uri)
        {
            return Container.GetCookieHeader(uri);
        }

        /// <inheritdoc cref="GetCookieHeader(System.Uri)"/>
        /// <param name="url">URL адрес ресурса</param>
        public string GetCookieHeader(string url)
        {
            return GetCookieHeader(new Uri(url));
        }

        /// <summary>
        /// Получает коллекцию всех <see cref="Cookie"/> связанных с адресом ресурса.
        /// </summary>
        /// <param name="uri">URI адрес ресурса</param>
        /// <returns>Вернет коллекцию <see cref="Cookie"/> связанных с адресом ресурса</returns>
        public CookieCollection GetCookies(Uri uri)
        {
            return Container.GetCookies(uri);
        }

        /// <inheritdoc cref="GetCookies(System.Uri)"/>
        /// <param name="url">URL адрес ресурса</param>
        public CookieCollection GetCookies(string url)
        {
            return GetCookies(new Uri(url));
        }

        /// <summary>
        /// Проверяет существование <see cref="Cookie"/> в <see cref="CookieContainer"/> по адресу ресурса и имени ключа куки.
        /// </summary>
        /// <param name="uri">URI адрес ресурса</param>
        /// <param name="name">Имя-ключ куки</param>
        /// <returns>Вернет <see langword="true"/> если ключ найден по запросу.</returns>
        public bool ContainsKey(Uri uri, string name)
        {
            if (Container.Count <= 0)
                return false;

            var cookies = Container.GetCookies(uri);
            return cookies[name] != null;
        }

        /// <inheritdoc cref="ContainsKey(System.Uri,string)"/>
        public bool ContainsKey(string url, string name)
        {
            return ContainsKey(new Uri(url), name);
        }

        /// <summary>
        /// Сохраняет куки в файл.
        /// <remarks>Рекомендуется расширение .jar.</remarks>
        /// </summary>
        /// <param name="filePath">Пусть для сохранения файла</param>
        /// <param name="overwrite">Перезаписать файл если он уже существует</param>
        public void SaveToFile(string filePath, bool overwrite = true)
        {
            if (!overwrite && File.Exists(filePath))
                throw new ArgumentException(string.Format(Resources.CookieStorage_SaveToFile_FileAlreadyExists, filePath), nameof(filePath));

            using (var fs = new FileStream(filePath, FileMode.OpenOrCreate))
                Bf.Serialize(fs, this);
        }

        /// <summary>
        /// Загружает <see cref="CookieStorage"/> из файла.
        /// </summary>
        /// <param name="filePath">Путь к файлу с куками</param>
        /// <returns>Вернет <see cref="CookieStorage"/>, который задается в свойстве <see cref="HttpRequest"/> Cookies.</returns>
        public static CookieStorage LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Файл с куками '${filePath}' не найден", nameof(filePath));

            using (var fs = new FileStream(filePath, FileMode.Open))
                return (CookieStorage)Bf.Deserialize(fs);
        }

        /// <summary>
        /// Число <see cref="Cookie"/> в <see cref="CookieContainer"/> (для всех адресов).
        /// </summary>
        public int Count => Container.Count;
    }
}
