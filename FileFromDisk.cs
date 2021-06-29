using System.IO;
using System;

namespace Converter
{
    class FileFromDisk : IFile
    {
        /// <summary>
        /// Геттер и сеттер для потока данных
        /// </summary>
        public Stream Stream
        {
            get { return stream; }
            set { stream = value; }
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
                IFile.logger.Error("Файл не найден");
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

        private Stream stream;
        private string name = null;

        /// <summary>
        /// меняем расширения файла
        /// </summary>
        /// <param name="fileName">Название файла</param>
        /// <returns>Название файла с расширением pdf</returns>
        private string ReplaceExtension(string fileName)
        {
            if (fileName == null)
            {
                IFile.logger.Error("Неккоректное название файла");
                throw new ArgumentNullException("Неккоректное название файла");
            }
            return fileName.Replace(".docx", ".pdf");
        }
    }
}
