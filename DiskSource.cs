using System.IO;
using System;

namespace Converter
{
    class DiskSource : IFile
    {
        private Stream stream;
        private string name = null;

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
        /// <param name="fileName"></param>
        /// <exception cref="FileNotFoundException">Не удалось найти файл на диске</exception>
        public void Read(string fileName)
        {
            name = fileName;
            if (!File.Exists(fileName))
            {
               throw new FileNotFoundException("Файл не найден");
            }
            stream = File.OpenRead(fileName);
            IFile.logger.Info("Файл успешно загружен из диска");
        }

        /// <summary>
        /// Запись файла на диск
        /// </summary>
        public void Write()
        {
            using (var file = new FileStream(ReplaceExtension(name), FileMode.Create))
            {
                stream.CopyTo(file);
                IFile.logger.Info("Файл успешно загружен на диск");
            }
        }

        /// <summary>
        /// меняем расширения файла
        /// </summary>
        /// <param name="fileName">Название файла</param>
        /// <returns>Название файла с расширением pdf</returns>
        /// <exception cref="ArgumentNullException">Некорректное название файла</exception>
        private string ReplaceExtension(string fileName)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException("Некорректное название файла");
            }
            return fileName.Replace(".docx", ".pdf");
        }
    }
}

