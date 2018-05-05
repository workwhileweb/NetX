using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace Leaf.Net
{
    /// <inheritdoc />
    /// <summary>
    /// Представляет цепочку из различных прокси-серверов.
    /// </summary>
    public class ChainProxyClient : ProxyClient
    {
        #region Статические поля (закрытые)

        [ThreadStatic] private static Random _rand;
        private static Random Rand => _rand ?? (_rand = new Random());

        #endregion


        #region Свойства (открытые)

        /// <summary>
        /// Возвращает или задает значение, указывающие, нужно ли перемешивать список цепочки прокси-серверов, перед созданием нового подключения.
        /// </summary>
        public bool EnableShuffle { get; set; }

        /// <summary>
        /// Возвращает список цепочки прокси-серверов.
        /// </summary>
        public List<ProxyClient> Proxies { get; } = new List<ProxyClient>();

        #region Переопределённые

        /// <summary>
        /// Данное свойство не поддерживается.
        /// </summary>
        /// <exception cref="System.NotSupportedException">При любом использовании этого свойства.</exception>
        public override string Host
        {
            // ReSharper disable once ArrangeAccessorOwnerBody
            get => throw new NotSupportedException();
            //set => throw new NotSupportedException();
        }

        /// <summary>
        /// Данное свойство не поддерживается.
        /// </summary>
        /// <exception cref="System.NotSupportedException">При любом использовании этого свойства.</exception>
        public override int Port
        {
            // ReSharper disable once ArrangeAccessorOwnerBody
            get => throw new NotSupportedException();
            //set => throw new NotSupportedException();
        }

        /// <summary>
        /// Данное свойство не поддерживается.
        /// </summary>
        /// <exception cref="System.NotSupportedException">При любом использовании этого свойства.</exception>
        public override string Username
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        /// <summary>
        /// Данное свойство не поддерживается.
        /// </summary>
        /// <exception cref="System.NotSupportedException">При любом использовании этого свойства.</exception>
        public override string Password
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        /// <summary>
        /// Данное свойство не поддерживается.
        /// </summary>
        /// <exception cref="System.NotSupportedException">При любом использовании этого свойства.</exception>
        public override int ConnectTimeout
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        /// <summary>
        /// Данное свойство не поддерживается.
        /// </summary>
        /// <exception cref="System.NotSupportedException">При любом использовании этого свойства.</exception>
        public override int ReadWriteTimeout
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        #endregion

        #endregion


        /// <inheritdoc />
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="T:Leaf.Net.ChainProxyClient" />.
        /// </summary>
        /// <param name="enableShuffle">Указывает, нужно ли перемешивать список цепочки прокси-серверов, перед созданием нового подключения.</param>
        public ChainProxyClient(bool enableShuffle = false)
            : base(ProxyType.Chain)
        {
            EnableShuffle = enableShuffle;
        }


        #region Методы (открытые)

        /// <inheritdoc />
        /// <summary>
        /// Создаёт соединение с сервером через цепочку прокси-серверов.
        /// </summary>
        /// <param name="destinationHost">Хост сервера, с которым нужно связаться через прокси-сервер.</param>
        /// <param name="destinationPort">Порт сервера, с которым нужно связаться через прокси-сервер.</param>
        /// <param name="tcpClient">Соединение, через которое нужно работать, или значение <see langword="null" />.</param>
        /// <returns>Соединение с сервером через цепочку прокси-серверов.</returns>
        /// <exception cref="T:System.InvalidOperationException">
        /// Количество прокси-серверов равно 0.
        /// -или-
        /// Значение свойства <see cref="P:Leaf.Net.ChainProxyClient.Host" /> равно <see langword="null" /> или имеет нулевую длину.
        /// -или-
        /// Значение свойства <see cref="P:Leaf.Net.ChainProxyClient.Port" /> меньше 1 или больше 65535.
        /// -или-
        /// Значение свойства <see cref="P:Leaf.Net.ChainProxyClient.Username" /> имеет длину более 255 символов.
        /// -или-
        /// Значение свойства <see cref="P:Leaf.Net.ChainProxyClient.Password" /> имеет длину более 255 символов.
        /// </exception>
        /// <exception cref="T:System.ArgumentNullException">Значение параметра <paramref name="destinationHost" /> равно <see langword="null" />.</exception>
        /// <exception cref="T:System.ArgumentException">Значение параметра <paramref name="destinationHost" /> является пустой строкой.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">Значение параметра <paramref name="destinationPort" /> меньше 1 или больше 65535.</exception>
        /// <exception cref="!:Leaf.Net.Net.ProxyException">Ошибка при работе с прокси-сервером.</exception>
        public override TcpClient CreateConnection(string destinationHost, int destinationPort, TcpClient tcpClient = null)
        {
            #region Проверка состояния

            if (Proxies.Count == 0)
            {
                throw new InvalidOperationException(
                    Resources.InvalidOperationException_ChainProxyClient_NotProxies);
            }

            #endregion

            List<ProxyClient> proxies;

            if (EnableShuffle)
            {
                proxies = Proxies.ToList();

                // Перемешиваем прокси.
                for (int i = 0; i < proxies.Count; i++)
                {
                    int randI = Rand.Next(proxies.Count);

                    var proxy = proxies[i];
                    proxies[i] = proxies[randI];
                    proxies[randI] = proxy;
                }
            }
            else
                proxies = Proxies;

            int length = proxies.Count - 1;
            var curTcpClient = tcpClient;

            for (int i = 0; i < length; i++)
            {
                curTcpClient = proxies[i].CreateConnection(
                    proxies[i + 1].Host, proxies[i + 1].Port, curTcpClient);
            }

            curTcpClient = proxies[length].CreateConnection(
                destinationHost, destinationPort, curTcpClient);

            return curTcpClient;
        }

        /// <inheritdoc />
        /// <summary>
        /// Формирует список строк вида - хост:порт, представляющую адрес прокси-сервера.
        /// </summary>
        /// <returns>Список строк вида - хост:порт, представляющая адрес прокси-сервера.</returns>
        public override string ToString()
        {
            var strBuilder = new StringBuilder();

            foreach (var proxy in Proxies)
                strBuilder.AppendLine(proxy.ToString());

            return strBuilder.ToString();
        }

        /// <summary>
        /// Формирует список строк вида - хост:порт:имя_пользователя:пароль. Последние два параметра добавляются, если они заданы.
        /// </summary>
        /// <returns>Список строк вида - хост:порт:имя_пользователя:пароль.</returns>
        public virtual string ToExtendedString()
        {
            var strBuilder = new StringBuilder();

            foreach (var proxy in Proxies)
                strBuilder.AppendLine(proxy.ToExtendedString());

            return strBuilder.ToString();
        }

        #region Добавление прокси-серверов

        /// <summary>
        /// Добавляет в цепочку новый прокси-клиент.
        /// </summary>
        /// <param name="proxy">Добавляемый прокси-клиент.</param>
        /// <exception cref="System.ArgumentNullException">Значение параметра <paramref name="proxy"/> равно <see langword="null"/>.</exception>
        public void AddProxy(ProxyClient proxy)
        {
            #region Проверка параметров

            if (proxy == null)
                throw new ArgumentNullException(nameof(proxy));

            #endregion

            Proxies.Add(proxy);
        }

        /// <summary>
        /// Добавляет в цепочку новый HTTP-прокси клиент.
        /// </summary>
        /// <param name="proxyAddress">Строка вида - хост:порт:имя_пользователя:пароль. Три последних параметра являются необязательными.</param>
        /// <exception cref="System.ArgumentNullException">Значение параметра <paramref name="proxyAddress"/> равно <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentException">Значение параметра <paramref name="proxyAddress"/> является пустой строкой.</exception>
        /// <exception cref="System.FormatException">Формат порта является неправильным.</exception>
        public void AddHttpProxy(string proxyAddress)
        {
            Proxies.Add(HttpProxyClient.Parse(proxyAddress));
        }

        /// <summary>
        /// Добавляет в цепочку новый Socks4-прокси клиент.
        /// </summary>
        /// <param name="proxyAddress">Строка вида - хост:порт:имя_пользователя:пароль. Три последних параметра являются необязательными.</param>
        /// <exception cref="System.ArgumentNullException">Значение параметра <paramref name="proxyAddress"/> равно <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentException">Значение параметра <paramref name="proxyAddress"/> является пустой строкой.</exception>
        /// <exception cref="System.FormatException">Формат порта является неправильным.</exception>
        public void AddSocks4Proxy(string proxyAddress)
        {
            Proxies.Add(Socks4ProxyClient.Parse(proxyAddress));
        }

        /// <summary>
        /// Добавляет в цепочку новый Socks4a-прокси клиент.
        /// </summary>
        /// <param name="proxyAddress">Строка вида - хост:порт:имя_пользователя:пароль. Три последних параметра являются необязательными.</param>
        /// <exception cref="System.ArgumentNullException">Значение параметра <paramref name="proxyAddress"/> равно <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentException">Значение параметра <paramref name="proxyAddress"/> является пустой строкой.</exception>
        /// <exception cref="System.FormatException">Формат порта является неправильным.</exception>
        public void AddSocks4AProxy(string proxyAddress)
        {
            Proxies.Add(Socks4AProxyClient.Parse(proxyAddress));
        }

        /// <summary>
        /// Добавляет в цепочку новый Socks5-прокси клиент.
        /// </summary>
        /// <param name="proxyAddress">Строка вида - хост:порт:имя_пользователя:пароль. Три последних параметра являются необязательными.</param>
        /// <exception cref="System.ArgumentNullException">Значение параметра <paramref name="proxyAddress"/> равно <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentException">Значение параметра <paramref name="proxyAddress"/> является пустой строкой.</exception>
        /// <exception cref="System.FormatException">Формат порта является неправильным.</exception>
        public void AddSocks5Proxy(string proxyAddress)
        {
            Proxies.Add(Socks5ProxyClient.Parse(proxyAddress));
        }

        #endregion

        #endregion
    }
}
