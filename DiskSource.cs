using System.IO;
using System;
using Microsoft.Extensions.Logging;

namespace Converter
{
    class DiskSource : IFile
    {
        private Stream stream;
        private string name = null;
        private readonly ILogger logger;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name = "logger"> Логгер </param>
        public DiskSource(ILogger logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Геттер и сеттер для потока данных
        /// </summary>
        public Stream Stream
        {
            get { return stream; }
            set { stream = value; }
        }

        /// <summary>
        /// Геттер и сеттер для названия переданного файла
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        /// <summary>
        /// Чтение файла из диска
        /// </summary>
        /// <param name = "fileName"></param>
        /// <exception cref = "FileNotFoundException"> Не удалось найти файл на диске </exception>
        public void Read(string fileName)
        {
            name = fileName;
            logger.LogInformation("Начало загрузки файла с диска");
            FileInfo file = new FileInfo(fileName);
            if (!file.Exists)
            {
                logger.LogWarning("Файл не найден");
                throw new FileNotFoundException("Файл не найден");
            }
            stream = file.OpenRead();
            logger.LogInformation("Файл успешно загружен из диска");
        }

        /// <summary>
        /// Запись файла на диск
        /// </summary>
        public void Write()
        {
            using var file = new FileStream(ReplaceExtension(name), FileMode.Create);
            logger.LogInformation("Начало выгрузки файла на диск");
            stream.CopyTo(file);
            file.FlushAsync();
            logger.LogInformation("Файл успешно загружен на диск");
        }

        /// <summary>
        /// Замена расширения файла
        /// </summary>
        /// <param name = "fileName"> Название файла </param>
        /// <returns> Название файла с расширением pdf </returns>
        /// <exception cref = "ArgumentNullException"> Некорректное название файла </exception>
        private string ReplaceExtension(string fileName)
        {
            if (fileName == null)
            {
                logger.LogWarning("Некорректное название файла");
                throw new ArgumentNullException("Некорректное название файла");
            }
            return fileName.Replace(".docx", ".pdf");
        }
    }
}

