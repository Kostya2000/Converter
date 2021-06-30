using System;

namespace Converter
{
    class UnconnectedException : System.Net.WebException
    {
        public UnconnectedException(string message = null) 
            : base (message)
        {}
    }
}
