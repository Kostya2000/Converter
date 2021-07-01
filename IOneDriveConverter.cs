using System.IO;

namespace Converter
{
    interface IOneDriveConverter
    {
        const int MaxFilesize = 4000000;
        /// <summary>
        /// Отправляем файл размером больше 4МБ 
        /// </summary>
        /// <param name="stream">Поток данных</param>
        void SendLargeFile(Stream stream);

        /// <summary>
        /// Отправляем файл размером меньше 4МБ 
        /// </summary>
        /// <param name="stream">Поток данных</param>
        void SendSmallFile(Stream stream);

        /// <summary>
        /// Определяем запрос для отправки файла
        /// </summary>
        /// <param name="stream">Поток данных</param>
        /// <param name="sizeStream">[Необязательный параметр] Размер файла</param>
        public void SendFile(Stream stream, int sizeStream = 0)
        {
            if (sizeStream > MaxFilesize || stream.Length > MaxFilesize)
            {
                SendLargeFile(stream);
                return;
            }
            SendSmallFile(stream);
        }
    }
}
