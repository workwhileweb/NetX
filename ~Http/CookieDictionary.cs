using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;

namespace SharpNet
{
    /// <summary>
    /// Представляет коллекцию HTTP-куки.
    /// </summary>
    public class CookieDictionary : Dictionary<string, Cookie>
    {
        /// <summary>
        /// Возвращает или задает значение, указывающие, закрыты ли куки для редактирования
        /// </summary>
        /// <value>Значение по умолчанию — <see langword="false"/>.</value>
        public bool IsLocked { get; set; }

        /// <summary>
        /// Возвращает или задает значение, указывающее, следует ли удалять просроченные куки
        /// </summary>
        /// <value>Значение по умолчанию - <see langword="true"/>.</value>
        public bool CleanExpired { get; set; }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="CookieDictionary"/>.
        /// </summary>
        /// <param name="isLocked">Указывает, закрыты ли куки для редактирования.</param>
        public CookieDictionary(bool isLocked = false, bool cleanExpired = true) : base(StringComparer.OrdinalIgnoreCase)
        {
            IsLocked = isLocked;
            CleanExpired = cleanExpired;
        }


        /// <summary>
        /// Возвращает строку, состоящую из имён и значений куки.
        /// </summary>
        /// <returns>Строка, состоящая из имён и значений куки.</returns>
        public override string ToString()
        {
            var strBuilder = new StringBuilder();        

            foreach (var cookie in this)
                strBuilder.AppendFormat("{0}={1}; ", cookie.Key, cookie.Value.Value);

            if (strBuilder.Length > 0)
                strBuilder.Remove(strBuilder.Length - 2, 2);

            return strBuilder.ToString();
        }

        /// <summary>
        /// Удаляет все просроченные куки. Метод автоматически вызывается при отправке запросов.
        /// </summary>
        public void RemoveExpired()
        {
            var now = DateTime.Now;
            foreach (var key in Keys)
            {
                if (this[key].Expires != default(DateTime) && this[key].Expires < now)
                    Remove(key);
            }
        }
    }
}