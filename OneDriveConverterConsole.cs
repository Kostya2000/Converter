using System;
using System.IO;
using System.Net;
using NLog;

namespace Converter
{
    class OneDriveConverterConsole
    {
        static void Main(string[] args)
        {
            Init();
            if (args.Length<1)
            {
                var logger = LogManager.GetCurrentClassLogger();
                logger.Error("Название файла не передано");
                throw new ArgumentNullException("Название файла не передано");
            }
            ConvertFileFromDiskSource(args[0]);
        }

        /// <summary>
        /// Загрузка данных из конфигурации
        /// </summary>
        static void Init()
        {
            IOneDriveConnection connection = new OneDriveConnection();
            connection.Init();
        }

        /// <summary>
        /// Конвертируем файл из диска
        /// </summary>
        /// <param name="fileName"></param>
        static void ConvertFileFromDiskSource(string fileName)
        {
            var driveConverter = new OneDriveConverter();
            IFile file = new DiskSource();
            try
            {
                file.Read(fileName);
                driveConverter.SendFile((file as DiskSource).Stream);
                (file as DiskSource).Stream = driveConverter.GetFile();
                file.Write();
                driveConverter.DeleteFile();
            }
            catch (FileNotFoundException)
            {
                IFile.logger.Error($"Файл {fileName} не найден");
            }
            catch (ArgumentNullException)
            {
                IFile.logger.Error("Данные из файла не загружены");
            }
            catch (UnconnectedException)
            {
                IFile.logger.Error("Не удалось подключиться к серверу");
            }
            catch (UnauthorizedAccessException)
            {
                IFile.logger.Error("Аутентификация не прошла");
            }
            catch (WebException)
            {
                IFile.logger.Error("Не удалось подключиться к серверу. Связь потеряна");
            }
            catch (AggregateException)
            {
                IFile.logger.Error("Нет доступа к интернету");
            }
        }
    }
}
