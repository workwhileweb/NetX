using System;
using System.Collections.Generic;
using System.Text;

namespace Leaf.xNet
{
    /// <inheritdoc />
    /// <summary>
    /// Представляет тело запроса в виде параметров запроса.
    /// </summary>
    public class FormUrlEncodedContent : BytesContent
    {
        /// <inheritdoc />
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="T:Leaf.xNet.FormUrlEncodedContent" />.
        /// </summary>
        /// <param name="content">Содержимое тела запроса в виде параметров запроса.</param>
        /// <param name="dontEscape">Указывает, нужно ли кодировать значения параметров.</param>
        /// <param name="encoding">Кодировка, применяемая для преобразования параметров запроса. Если значение параметра равно <see langword="null" />, то будет использовано значение <see cref="P:System.Text.Encoding.UTF8" />.</param>
        /// <exception cref="T:System.ArgumentNullException">Значение параметра <paramref name="content" /> равно <see langword="null" />.</exception>
        /// <remarks>По умолчанию используется тип контента - 'application/x-www-form-urlencoded'.</remarks>
        public FormUrlEncodedContent(IEnumerable<KeyValuePair<string, string>> content, bool dontEscape = false, Encoding encoding = null)
        {
            #region Проверка параметров

            if (content == null)
                throw new ArgumentNullException(nameof(content));

            #endregion

            string queryString = Http.ToPostQueryString(content, dontEscape, encoding);

            Content = Encoding.ASCII.GetBytes(queryString);
            Offset = 0;
            Count = Content.Length;

            MimeContentType = "application/x-www-form-urlencoded";
        }
    }
}
