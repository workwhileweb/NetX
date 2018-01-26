using System;
using System.Net;

namespace Leaf.Net
{
    [Serializable]
    public class CookieStorage
    {
        public CookieContainer Container { get; private set; }

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

        public void Set(CookieCollection cookies)
        {
            Container.Add(cookies);
        }

        public void Set(Cookie cookie)
        {
            Container.Add(cookie);
        }

        public void Set(string name, string value, string domain, string path = "/")
        {
            Container.Add(new Cookie(name, value, path, domain));
        }

        public void Set(string url, string rawCookie)
        {
            Container.SetCookies(new Uri(url), rawCookie);
        }

        public void Set(Uri uri, string rawCookie)
        {
            Container.SetCookies(uri, rawCookie);
        }

        public void Clear()
        {
            Container = new CookieContainer();
        }

        public void Remove(string url)
        {
            Remove(new Uri(url));
        }

        public void Remove(Uri uri)
        {
            var cookies = Container.GetCookies(uri);
            foreach (Cookie cookie in cookies)
                cookie.Expired = true;
        }

        public void Remove(string url, string name)
        {
            Remove(new Uri(url), name);
        }

        public void Remove(Uri uri, string name)
        {
            var cookies = Container.GetCookies(uri);
            foreach (Cookie cookie in cookies)
            {
                if (cookie.Name == name)
                    cookie.Expired = true;
            }
        }

        public string GetCookieHeader(string url)
        {
            return GetCookieHeader(new Uri(url));
        }

        public string GetCookieHeader(Uri uri)
        {
            return Container.GetCookieHeader(uri);
        }

        public CookieCollection GetCookies(string url)
        {
            return GetCookies(new Uri(url));
        }

        public CookieCollection GetCookies(Uri uri)
        {
            return Container.GetCookies(uri);
        }

        public bool ContainsKey(string url, string name)
        {
            return ContainsKey(new Uri(url), name);
        }

        public bool ContainsKey(Uri uri, string name)
        {
            if (Container.Count <= 0)
                return false;

            var cookies = Container.GetCookies(uri);
            return cookies[name] != null;
        }

        public int Count => Container.Count;
    }
}
