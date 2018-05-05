using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Leaf.Net
{
    /// <inheritdoc />
    /// <summary>
    /// Исключение, которое выбрасывается, в случае возникновения ошибки при работе с HTTP-протоколом.
    /// </summary>
    [Serializable]
    public sealed class HttpException : NetException
    {
        #region Свойства (открытые)

        /// <summary>
        /// Возвращает состояние исключения.
        /// </summary>
        public HttpExceptionStatus Status { get; internal set; }

        /// <summary>
        /// Возвращает код состояния ответа от HTTP-сервера.
        /// </summary>
        public HttpStatusCode HttpStatusCode { get; }

        #endregion


        internal bool EmptyMessageBody { get; set; }


        #region Конструкторы (открытые)

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="HttpException"/>.
        /// </summary>
        public HttpException() : this(Resources.HttpException_Default) { }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="HttpException"/> заданным сообщением об ошибке.
        /// </summary>
        /// <param name="message">Сообщение об ошибке с объяснением причины исключения.</param>
        /// <param name="innerException">Исключение, вызвавшее текущие исключение, или значение <see langword="null"/>.</param>
        public HttpException(string message, Exception innerException = null)
            : base(message, innerException) { }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="HttpException"/> заданным сообщением об ошибке и кодом состояния ответа.
        /// </summary>
        /// <param name="message">Сообщение об ошибке с объяснением причины исключения.</param>
        /// <param name="statusCode">Код состояния ответа от HTTP-сервера.</param>
        /// <param name="innerException">Исключение, вызвавшее текущие исключение, или значение <see langword="null"/>.</param>
        public HttpException(string message, HttpExceptionStatus status,
            HttpStatusCode httpStatusCode = HttpStatusCode.None, Exception innerException = null)
            : base(message, innerException)
        {
            Status = status;
            HttpStatusCode = httpStatusCode;
        }

        #endregion


        /// <inheritdoc />
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="T:Leaf.Net.HttpException" /> заданными экземплярами <see cref="T:System.Runtime.Serialization.SerializationInfo" /> и <see cref="T:System.Runtime.Serialization.StreamingContext" />.
        /// </summary>
        /// <param name="serializationInfo">Экземпляр класса <see cref="T:System.Runtime.Serialization.SerializationInfo" />, который содержит сведения, требуемые для сериализации нового экземпляра класса <see cref="T:Leaf.Net.HttpException" />.</param>
        /// <param name="streamingContext">Экземпляр класса <see cref="T:System.Runtime.Serialization.StreamingContext" />, содержащий источник сериализованного потока, связанного с новым экземпляром класса <see cref="T:Leaf.Net.HttpException" />.</param>
        protected HttpException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
            if (serializationInfo == null)
                return;

            Status = (HttpExceptionStatus)serializationInfo.GetInt32("Status");
            HttpStatusCode = (HttpStatusCode)serializationInfo.GetInt32("HttpStatusCode");
        }


        /// <inheritdoc />
        /// <summary>
        /// Заполняет экземпляр <see cref="T:System.Runtime.Serialization.SerializationInfo" /> данными, необходимыми для сериализации исключения <see cref="T:Leaf.Net.HttpException" />.
        /// </summary>
        /// <param name="serializationInfo">Данные о сериализации, <see cref="T:System.Runtime.Serialization.SerializationInfo" />, которые должны использоваться.</param>
        /// <param name="streamingContext">Данные о сериализации, <see cref="T:System.Runtime.Serialization.StreamingContext" />, которые должны использоваться.</param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            base.GetObjectData(serializationInfo, streamingContext);

            serializationInfo.AddValue("Status", (int)Status);
            serializationInfo.AddValue("HttpStatusCode", (int)HttpStatusCode);
        }
    }
}
