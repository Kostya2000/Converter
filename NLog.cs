using NLog;

namespace Converter
{
    class NLog : ILogger
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        private Logger Logger
        {
            get { return logger; }
        }

        public void Debug(string message)
        {
            Logger.Debug(message);
        }

        public void Error(string message)
        {
            Logger.Error(message);
        }

        public void Fatal(string message)
        {
            Logger.Fatal(message);
        }

        public void Info(string message)
        {
            Logger.Info(message);
        }

        public void Trace(string message)
        {
            Logger.Trace(message);
        }

        public void Warn(string message)
        {
            Logger.Warn(message);
        }
    }
}
