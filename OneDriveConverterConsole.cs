using System;
using Microsoft.Extensions.Logging;

namespace Converter
{
    class OneDriveConverterConsole
    {
        static ILogger logger;
        static void Main(string[] args)
        {
            Init();
            if (args.Length < 1)
            {
                throw new ArgumentNullException("Название файла не передано");
            }
            ConvertFileFromDiskSource(args[0]);
        }

        /// <summary>
        /// Загрузка данных из конфигурации
        /// </summary>
        private static void Init()
        {
            IOneDriveConnection connection = new OneDriveConnection(logger);
            connection.Init(ref logger);
        }

        /// <summary>
        /// Конвертация файла из диска
        /// </summary>
        /// <param name = "fileName"> Название файла </param>
        private static void ConvertFileFromDiskSource(string fileName)
        {
            var driveConverter = new OneDriveConverter(logger);
            IFile file = new DiskSource(logger);
            file.Read(fileName);
            (file as DiskSource).Stream = driveConverter.Converter((file as DiskSource).Stream);
            file.Write();
        }
    }
}
