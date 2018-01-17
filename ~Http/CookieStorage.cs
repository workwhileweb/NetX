using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leaf.Net
{
    public class CookieStorage
    {
        private CookieContainer _container;

        public CookieStorage(bool isLocked = false, CookieContainer container = null)
        {
            IsLocked = isLocked;
            _container = container ?? new CookieContainer();
        }

        /// <summary>
        /// Возвращает или задает значение, указывающие, закрыты ли куки для редактирования через ответы сервера.
        /// </summary>
        /// <value>Значение по умолчанию — <see langword="false"/>.</value>
        public bool IsLocked { get; set; }

        public void Set(CookieCollection cookies)
        {
            _container.Add(cookies);
        }

        public void Set(Cookie cookie)
        {
            _container.Add(cookie);
        }

        public void Set(string name, string value, string domain, string path = "/")
        {
            _container.Add(new Cookie(name, value, path, domain));
        }

        public void Set(string url, string rawCookie)
        {
            _container.SetCookies(new Uri(url), rawCookie);
        }

        public void Set(Uri uri, string rawCookie)
        {
            _container.SetCookies(uri, rawCookie);
        }

        public void Clear()
        {
            _container = new CookieContainer();
        }

        public void Remove(string url)
        {
            Remove(new Uri(url));
        }

        public void Remove(Uri uri)
        {
            var cookies = _container.GetCookies(uri);
            foreach (Cookie cookie in cookies)
                cookie.Expired = true;
        }

        public void Remove(string url, string name)
        {
            Remove(new Uri(url), name);
        }

        public void Remove(Uri uri, string name)
        {
            var cookies = _container.GetCookies(uri);
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
            return _container.GetCookieHeader(uri);
        }

        public CookieCollection GetCookies(string url)
        {
            return GetCookies(new Uri(url));
        }

        public CookieCollection GetCookies(Uri uri)
        {
            return _container.GetCookies(uri);
        }

        public bool ContainsKey(string url, string name)
        {
            return ContainsKey(new Uri(url), name);
        }

        public bool ContainsKey(Uri uri, string name)
        {
            if (_container.Count <= 0)
                return false;

            var cookies = _container.GetCookies(uri);
            return cookies[name] != null;
        }

        public int Count => _container.Count;
    }
}
