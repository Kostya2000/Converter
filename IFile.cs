using NLog;

namespace Converter
{
    interface IFile : IReadFile, IWriteFile
    {
        static Logger logger = LogManager.GetCurrentClassLogger();
    }
}
