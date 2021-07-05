using System.Net;

namespace Converter
{
    class UnconnectedException : WebException
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name = "message"> Сообщение </param>
        public UnconnectedException(string message = null) 
            : base (message)
        {}
    }
}
