using System;

namespace Converter
{
    class DataNullException : Exception
    {
        public DataNullException(string message = null)
            : base(message)
        { }
    }
}
