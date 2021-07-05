using System;
using System.IO;
using System.Net;
using Microsoft.Extensions.Logging;

namespace Converter
{
    class OneDriveConverter
    {
        private const int MaxFilesize = 4000000;
        private const int pushSize = 327680;
        private string guid = Guid.NewGuid()
                              .ToString() + ".docx";
        private static ILogger logger;
        private Lazy<OneDriveConnection> connection=new Lazy<OneDriveConnection>(() => new OneDriveConnection(logger));

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name = "_logger"> Логгер </param>
        public OneDriveConverter(ILogger _logger)
        {
            logger = _logger;
        }

        /// <summary>
        /// Конвертация документа
        /// </summary>
        /// <param name = "docxStream"> поток данных </param>
        /// <param name = "streamSize"> [Необязательный параметр] Размер данных </param>
        /// <returns></returns>
        public Stream Converter(Stream docxStream, int streamSize = 0)
        {
            SendFile(docxStream, streamSize);
            var pdfStream = GetFile();
            DeleteFile();
            return pdfStream;
        }

        /// <summary>
        /// Отправка файла с размером меньше 4 МБ на сервер
        /// </summary>
        /// <param name = "stream"> Поток данных </param>
        /// <exception cref= "UnconnectedException"> Не удалось подключиться к серверу </exception>
        /// <exception cref= "DataNullException"> "Данные из файла не загружены" </exception>
        private void SendSmallFile(Stream stream)
        {
            logger.LogInformation("Запрос на отправку файла размером меньше 4МБ на OneDrive...");
            if (stream == null)
            {
                logger.LogWarning("Данные из файла не загружены");
                throw new DataNullException("Данные из файла не загружены");
            }
            WebRequest request;
            var reqStream = Stream.Null;
            connection.Value.Connect(ref reqStream, out request, "PUT", $"https://graph.microsoft.com/v1.0/drive/root:/{GUID}:/content", "multipart/form-data", true);
            stream.CopyTo(reqStream);
            _ = request.GetResponse();
            logger.LogInformation("Файл успешно отправлен на сервер");
        }

        /// <summary>
        /// Отправка файла с размром больше 4 МБ на сервер
        /// </summary>
        /// <param name= "stream"> Поток данных </param>
        /// <exception cref = "UnconnectedException"> Не удалось подключиться к серверу </exception>
        private void SendLargeFile(Stream stream)
        {
            logger.LogInformation("Запрос на отправку файла размером больше 4МБ на OneDrive...");
            int temporarySize = 0;
            int counter = 0;
            logger.LogInformation("Отправка фрагментов:");
            while (temporarySize + pushSize < stream.Length - 1)
            {
                logger.LogInformation((++counter).ToString()+" Content-Range " + "bytes " + (temporarySize).ToString() + "-" + (temporarySize + pushSize - 1).ToString() + "/" + (stream.Length).ToString());
                SendFrame(ref stream, temporarySize, temporarySize + pushSize - 1);
                if (temporarySize + pushSize >= stream.Length)
                {
                    stream.Position = temporarySize + 1;
                    break;
                }
                stream.Position = temporarySize + pushSize;
                temporarySize += pushSize;
            }
            logger.LogInformation("Отправка последнего фрагмента:");
            if (temporarySize != stream.Length - 1)
            {
                logger.LogInformation("Content-Range " + "bytes " + (temporarySize).ToString() + "-" + (stream.Length - 1).ToString() + "/" + (stream.Length).ToString());
                SendFrame(ref stream, temporarySize, stream.Length - 1);
            }
            logger.LogInformation("Файл успешно передан на сервера");
        }

        /// <summary>
        /// Отправка кадра
        /// </summary>
        /// <param name = "stream"> Поток данных </param>
        /// <param name = "beginPosition"> Стартовая позиция отправки данных </param>
        /// <param name = "endPosition"> Конечная позиция отправки данных </param>
        private void SendFrame(ref Stream stream, int beginPosition, long endPosition)
        {
            WebHeaderCollection webHeader = new WebHeaderCollection();
            webHeader.Add("Content-Length", (endPosition-beginPosition+1).ToString());
            webHeader.Add("Content-Range", "bytes " + (beginPosition).ToString() + "-" + (endPosition).ToString() + "/" + (stream.Length).ToString());
            var reqStream = Stream.Null;
            WebRequest request;
            connection.Value.Connect(ref reqStream, out request, "PUT", connection.Value.UploadUrl(GUID).ToString(), "multipart/form-data", true, true, webHeader);
            stream.CopyTo(reqStream, pushSize);
            var _ = request.GetResponse();
        }

        /// <summary>
        /// Получение файла с сервера
        /// </summary>
        ///<exception cref = "UnconnectedException"> Не удалось подключиться к серверу </exception>
        private Stream GetFile()
        {
            logger.LogInformation("Запрос на загрузку файла из OneDrive...");
            var reqStream = Stream.Null;
            WebRequest request;
            connection.Value.Connect(ref reqStream, out request, "GET", $"https://graph.microsoft.com/v1.0/drive/root:/{GUID}:/content?format=pdf", "pdf/application");
            var resp = request.GetResponse();
            if (resp == null)
            {
                logger.LogError("Не удалось подключится к серверу");
                throw new UnconnectedException("Не удалось подключится к серверу");
            }
            var stream = resp.GetResponseStream();
            logger.LogInformation("Файл успешно загружен из OneDrive");
            return stream;
        }

        /// <summary>
        /// Удаление файла с сервера
        /// </summary>
        /// <exception cref = "UnconnectedException"> Не удалось подключиться к серверу </exception>
        private void DeleteFile()
        {
            logger.LogInformation("Запрос на удаление файла из OneDrive");
            var reqStream = Stream.Null;
            WebRequest request;
            connection.Value.Connect(ref reqStream, out request, "DELETE", $"https://graph.microsoft.com/v1.0/drive/root:/{GUID}", null);
            if (!(request.GetResponse() is HttpWebResponse resp))
            {
                logger.LogError("Не удалось подключится к серверу");
                throw new UnconnectedException("Не удалось подключится к серверу");
            }
            using var stream = resp.GetResponseStream();
            logger.LogInformation("Файл успешно удален с сервера");
        }

        /// <summary>
        /// Определение запроса для отправки файла
        /// </summary>
        /// <param name = "stream"> Поток данных </param>
        /// <param name = "sizeStream"> [Необязательный параметр] Размер файла </param>
        private void SendFile(Stream stream, int sizeStream = 0)
        {
            logger.LogInformation("Определение запроса для отправки файла...");
            if (sizeStream > MaxFilesize || stream.Length > MaxFilesize)
            {
                SendLargeFile(stream);
                return;
            }
            SendSmallFile(stream);
            logger.LogInformation("Файл отправлен успешно");
        }

        /// <summary>
        /// Генерация guid по принципу lazy
        /// </summary>
        private string GUID
        {
            get { return guid; }
        }
    }
}
