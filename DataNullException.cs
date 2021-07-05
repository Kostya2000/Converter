using System;

namespace Converter
{
    class DataNullException : Exception
    {
        /// <summary>
        /// Конструктоо
        /// </summary>
        /// <param name = "message"> Сообщение </param>
        public DataNullException(string message = null)
            : base(message)
        { }
    }
}
