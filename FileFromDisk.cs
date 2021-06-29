using System.IO;
using NLog;

namespace Converter
{
    class FileFromDisk
    {
        static Logger logger = LogManager.GetCurrentClassLogger();
        Stream stream;
        string name;

        /// <summary>
        /// Геттер и сеттер для потока данных
        /// </summary>
        public Stream Stream
        {
            get
            {
                return stream;
            }
            set
            {
                stream = value;
            }
        }

        /// <summary>
        /// Чтение файла из диска
        /// </summary>
        /// <param name="fileName"></param>
        /// <exception cref="FileNotFoundException">Не удалось найти файл на диске</exception>
        public void Read(string fileName)
        {
            name = fileName;
            if (!File.Exists(fileName))
            {
                logger.Error("Файл не найден");
                throw new FileNotFoundException("Файл не найден");
            }
            stream = File.OpenRead(fileName);
            logger.Info("Файл успешно загружен из диска");
        }

        /// <summary>
        /// Запись файла на диск
        /// </summary>
        public void Write()
        {
            using (var file = new FileStream(ReplaceExtension(name), FileMode.Create))
            {
                stream.CopyTo(file);
                logger.Info("Файл успешно получен из сервера");
            }
        }

        /// <summary>
        /// меняем расширения файла
        /// </summary>
        /// <param name="fileName">Название файла</param>
        /// <returns>Название файла с расширением pdf</returns>
        private string ReplaceExtension(string fileName)
        {
            return fileName.Replace(".docx", ".pdf");
        }
    }
}
