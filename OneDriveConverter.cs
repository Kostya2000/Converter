using System;
using System.IO;
using System.Net;

namespace Converter
{
    class OneDriveConverter
    {
        private const int MaxFilesize = 4000000;
        private const int pushSize = 327680;
        private string guid = null;
        private static ILogger logger;
        private Lazy<OneDriveConnection> connection=new Lazy<OneDriveConnection>(()=>new OneDriveConnection(logger));

        public OneDriveConverter(ILogger _logger)
        {
            logger = _logger;
        }
        public Stream Converter(Stream docxStream, int streamSize = 0)
        {
            SendFile(docxStream, streamSize);
            var pdfStream = GetFile();
            DeleteFile();
            return pdfStream;
        }

        /// <summary>
        /// Отправляем файл с размером меньше 4 МБ на сервер
        /// </summary>
        /// <param name="stream">Поток данных</param>
        /// <exception cref="UnconnectedException">Не удалось подключиться к серверу</exception>
        /// <exception cref="DataNullException">"Данные из файла не загружены"</exception>
        private void SendSmallFile(Stream stream)
        {
            logger.Info("Запрос на отправку файла на OneDrive");
            if (stream == null)
            {
                throw new DataNullException("Данные из файла не загружены");
            }
            WebRequest request;
            var reqStream = Stream.Null;
            connection.Value.Connector(ref reqStream, out request, "PUT", $"https://graph.microsoft.com/v1.0/drive/root:/{GUID}:/content", "multipart/form-data", true);
            stream.CopyTo(reqStream);
            var resp = request.GetResponse();
            logger.Info("Файл успешно отправлен на сервер");
        }

        /// <summary>
        /// Отправляем файл с размром больше 4 МБ на сервер
        /// </summary>
        /// <param name="stream">Поток данных</param>
        /// <exception cref="UnconnectedException">Не удалось подключиться к серверу</exception>
        private void SendLargeFile(Stream stream)
        {
            logger.Info("Запрос на отправку большого файла на OneDrive");
            int temporarySize = 0;
            int counter = 0;
            logger.Info("Отправка фрагментов:");
            while (temporarySize + pushSize < stream.Length - 1)
            {
                logger.Info((++counter).ToString()+"Content-Range " + "bytes " + (temporarySize).ToString() + "-" + (temporarySize + pushSize - 1).ToString() + "/" + (stream.Length).ToString());
                SendFrame(ref stream, temporarySize, temporarySize + pushSize - 1);
                if (temporarySize + pushSize >= stream.Length)
                {
                    stream.Position = temporarySize + 1;
                    break;
                }
                stream.Position = temporarySize + pushSize;
                temporarySize += pushSize;
            }
            logger.Info("Отправка последнего фрагмента:");
            if (temporarySize != stream.Length - 1)
            {
                logger.Info("Content-Range " + "bytes " + (temporarySize).ToString() + "-" + (stream.Length - 1).ToString() + "/" + (stream.Length).ToString());
                SendFrame(ref stream, temporarySize, stream.Length - 1);
            }
            logger.Info("Файл успешно передан на сервера");
        }

        private void SendFrame(ref Stream stream, int beginPosition, long endPosition)
        {
            WebHeaderCollection webHeader = new WebHeaderCollection();
            webHeader.Add("Content-Length", (endPosition-beginPosition+1).ToString());
            webHeader.Add("Content-Range", "bytes " + (beginPosition).ToString() + "-" + (endPosition).ToString() + "/" + (stream.Length).ToString());
            var reqStream = Stream.Null;
            WebRequest request;
            connection.Value.Connector(ref reqStream, out request, "PUT", connection.Value.UploadUrl(GUID).ToString(), "multipart/form-data", true, webHeader);
            stream.CopyTo(reqStream, pushSize);
            var resp = request.GetResponse();
        }

        /// <summary>
        /// Получаем файл с сервера
        /// </summary>
        ///<exception cref="UnconnectedException">Не удалось подключиться к серверу</exception>
        private Stream GetFile()
        {
            logger.Info("Запрос на загрузку файла из OneDrive");
            var reqStream = Stream.Null;
            WebRequest request;
            connection.Value.Connector(ref reqStream, out request, "GET", $"https://graph.microsoft.com/v1.0/drive/root:/{GUID}:/content?format=pdf", "pdf/application");
            var resp = request.GetResponse();
            if (resp == null)
            {
                throw new UnconnectedException("Не удалось подключится к серверу");
            }
            var stream = resp.GetResponseStream();
            logger.Info("Файл успешно загружен из OneDrive");
            return stream;
        }

        /// <summary>
        /// Удаляем файла с сервера
        /// </summary>
        /// <exception cref="UnconnectedException">Не удалось подключиться к серверу</exception>
        private void DeleteFile(params WebHeaderCollection[] webHeader)
        {
            logger.Info("Запрос на удаление файла из OneDrive");
            var reqStream = Stream.Null;
            WebRequest request;
            connection.Value.Connector(ref reqStream, out request, "DELETE", $"https://graph.microsoft.com/v1.0/drive/root:/{GUID}", null);
            using var resp = request.GetResponse() as HttpWebResponse;
            if (resp == null)
            {
                throw new UnconnectedException("Не удалось подключится к серверу");
            }
            using var stream = resp.GetResponseStream();
            logger.Info("Файл успешно удален с сервера");
        }

        /// <summary>
        /// Определяем запрос для отправки файла
        /// </summary>
        /// <param name="stream">Поток данных</param>
        /// <param name="sizeStream">[Необязательный параметр] Размер файла</param>
        private void SendFile(Stream stream, int sizeStream = 0)
        {
            logger.Debug("Определение запроса для отправки файла...");
            if (sizeStream > MaxFilesize || stream.Length > MaxFilesize)
            {
                SendLargeFile(stream);
                return;
            }
            SendSmallFile(stream);
        }

        /// <summary>
        /// Генерация guid по принципу lazy
        /// </summary>
        private string GUID
        {
            get { return guid ??= Guid.NewGuid().ToString() + ".docx"; }
        }
    }
}
