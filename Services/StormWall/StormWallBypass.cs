using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace Leaf.Net.Services.StormWall
{
    /// <summary>
    /// Класс-расширение для обхода AntiDDoS защиты StormWall.
    /// </summary>
    /// 
    // TODO: not static session
    public static class StormWallBypass
    {
        public static bool IsStormWalled(this HttpResponse resp)
        {
            string respStr = resp.ToString();
            return respStr.Contains("<h1>Stormwall DDoS protection</h1>")
                   || respStr.Contains("://reports.stormwall.pro");
        }

        private static void ThrowNotFoundJsValue(string variable)
        {
            throw new StormWallException($"Not found \"{variable}\" variable or const in StormWall code");
        }

        private static string ParseJsConstValue(this string resp, string variable, bool isString = true)
        {
            string patternBegin, patternEnd;

            if (isString)
            {
                patternBegin = "const {0} = \"";
                patternEnd = "\";";
            }
            else
            {
                patternBegin = "const {0} = ";
                patternEnd = ";";
            }

            string value = resp.Substring(string.Format(patternBegin, variable), patternEnd);
            if (value == string.Empty)
                ThrowNotFoundJsValue(variable);

            return value;
        }

        private static string ParseJsVariableValue(this string resp, string variable)
        {
            string value = resp.Substring($"var {variable}=\"", "\",");
            if (value == string.Empty)
                ThrowNotFoundJsValue(variable);

            return value;
        }
        
        // Csr
        private static string Csr(string rpct, int t, char c)
        {
            var r = rpct.Length - 1;
            var e = new StringBuilder();
            var a = c;

            if (!_rRpct.ContainsKey(a))
                e.Append(a);
            else
            {
                int o = _rRpct[a] + t;
                if (o > r)
                    o = o - r - 1;
                else if (0 > o)
                    o = r + o + 1;
                
                e.Append(rpct[o]);
            }

            return e.ToString();

        }

        private static string Vgd(string rpct, int t, string c) // c = cE, t = cK
        {
            int r = rpct.Length - 1;
            int e = t;

            var n = new StringBuilder();

            foreach (var o in c) {
                n.Append(Csr(rpct, -1 * e, o));
                ++e;

                if (e > r)
                    e = 0;
            }

            return n.ToString();
        }

        // Создает обратную коллекцию значение - ключ

        private static Dictionary<char, int> _rRpct;

        private static void Gtb(string rpct) // out Dictionary<char, byte> rRpct
        {
            _rRpct = new Dictionary<char, int>();
            for (int i = 0; i < rpct.Length; i++)
                _rRpct[rpct[i]] = i;

            // throw new NotImplementedException();
        }

        // TODO: HttpResp or string as param
        // TODO: loop with sleep (increasing delay sleep)
        public static HttpResponse GetThroughStormWall(this HttpRequest req, string url, string rawResp)
        {

            string resp = rawResp.ToString();
            var uri = new Uri(url);

            // cE
            string cE = resp.ParseJsConstValue("cE");

            // cK
            string cKstr = resp.ParseJsConstValue("cK", false);
            int cK = 0;

            if (cKstr == string.Empty || !int.TryParse(cKstr, out cK))
                ThrowNotFoundJsValue("cK");

            // cN
            string cN = resp.ParseJsConstValue("cN");

            // max age 30
            string rpct = resp.ParseJsVariableValue("abc");

            Gtb(rpct);

            string key = Vgd(rpct, cK, cE);
            var cookie = new Cookie(cN, key, "/", uri.Host) {Expires = DateTime.Now.AddSeconds(30)};
            req.Cookies.Container.Add(cookie);


            string oldReferer = req.Referer;
            req.Referer = url;
            var respResult = req.Get(url);
            req.Referer = oldReferer;

            if (respResult.IsStormWalled())
                throw new StormWallException("Unable to pass StormWall at URL: " + url);

            return respResult;
            // throw new NotImplementedException();
        }
    }
}
