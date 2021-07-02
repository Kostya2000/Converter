using System;
using System.IO;
using System.Net;
using System.Net.Http;
using NLog;

namespace Converter
{
    class OneDriveConverterConsole
    {
        static ILogger logger = new NLog();
        static void Main(string[] args)
        {
            Init();
            if (args.Length < 1)
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
        private static void Init()
        {
            IOneDriveConnection connection = new OneDriveConnection(logger);
            connection.Init(logger);
        }

        /// <summary>
        /// Конвертируем файл из диска
        /// </summary>
        /// <param name="fileName"></param>
        private static void ConvertFileFromDiskSource(string fileName)
        {
            var driveConverter = new OneDriveConverter(logger);
            IFile file = new DiskSource(logger);
            try
            {
                file.Read(fileName);
                (file as DiskSource).Stream = driveConverter.Converter((file as DiskSource).Stream);
                file.Write();
            }
            catch (FileNotFoundException)
            {
                logger.Error($"Файл {fileName} не найден");
            }
            catch (ArgumentNullException)
            {
                logger.Error("При передаче аргумента был передан null");
            }
            catch (UnconnectedException)
            {
                logger.Error("Не удалось подключиться к серверу");
            }
            catch (UnauthorizedAccessException)
            {
                logger.Error("Аутентификация не прошла");
            }
            catch (WebException)
            {
                logger.Error("Не удалось подключиться к серверу. Связь потеряна");
            }
            catch (AggregateException)
            {
                logger.Error("Нет доступа к интернету");
            }
            catch (DataNullException)
            {
                logger.Error("Данные из файла не загружены");
            }
            catch (HttpRequestException)
            {
                logger.Error("Неправильный запрос к серверу");
            }
        }
    }
}
