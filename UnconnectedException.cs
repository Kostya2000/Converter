using System.Net;

namespace Converter
{
    class UnconnectedException : WebException
    {
        public UnconnectedException(string message = null) 
            : base (message)
        {}
    }
}
