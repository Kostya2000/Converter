using System;
using System.IO;
using System.Net;
using NLog;

namespace Converter
{
    class OneDriveConverter : IOneDriveConverter
    {
        private const int pushSize = 327680;
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private string guid = null;
        private Lazy<OneDriveConnection> connection=new Lazy<OneDriveConnection>();

        /// <summary>
        /// Отправляем файл с размером меньше 4 МБ на сервер
        /// </summary>
        /// <param name="stream">Поток данных</param>
        /// <exception cref="UnconnectedException">Не удалось подключиться к серверу</exception>
        /// <exception cref="DataNullException">"Данные из файла не загружены"</exception>
        public void SendSmallFile(Stream stream)
        {
            logger.Info("Запрос на отправку файла на OneDrive");
            if (stream == null)
            {
                throw new DataNullException("Данные из файла не загружены");
            }
            var webHeader = new WebHeaderCollection();
            webHeader.Add("Authorization", "Bearer " + connection.Value.Token.ToString());
            var graphUrl = $"https://graph.microsoft.com/v1.0/drive/root:/{GUID}:/content";
            
            var request = WebRequest.Create(graphUrl);
            request.Headers = webHeader;
            request.Method = "PUT";
            request.ContentType = "multipart/form-data";
            using var reqStream = request.GetRequestStream();
            if (reqStream == null)
            {
                throw new UnconnectedException("Не удалось подключится к серверу");
            }
            stream.CopyTo(reqStream);
            var resp = request.GetResponse();
            logger.Info("Файл успешно отправлен на сервер");
        }

        /// <summary>
        /// Отправляем файл с размром больше 4 МБ на сервер
        /// </summary>
        /// <param name="stream">Поток данных</param>
        /// <exception cref="UnconnectedException">Не удалось подключиться к серверу</exception>
        public void SendLargeFile(Stream stream)
        {
            logger.Info("Запрос на отправку большого файла на OneDrive");
            var connection = new OneDriveConnection();
            int temporarySize = 0;
            int counter = 0;
            var webHeader = new WebHeaderCollection();

            logger.Info("Отправка фрагментов:");
            while (temporarySize + pushSize < stream.Length - 1)
            {
                logger.Info((++counter).ToString()+"Content-Range " + "bytes " + (temporarySize).ToString() + "-" + (temporarySize + pushSize - 1).ToString() + "/" + (stream.Length).ToString());
                webHeader.Add("Authorization", "Bearer " + connection.Token.ToString());
                webHeader.Add("Content-Length", pushSize.ToString());
                webHeader.Add("Content-Range", "bytes " + (temporarySize).ToString() + "-" + (temporarySize + pushSize - 1).ToString() + "/" + (stream.Length).ToString());

                var request = WebRequest.Create(connection.UploadUrl(GUID).ToString());
                request.Headers = webHeader;
                request.Method = "PUT";
                
                request.ContentType = "multipart/form-data";

                using var reqStream2 = request.GetRequestStream();
                if (reqStream2 == null)
                {
                    throw new UnconnectedException("Не удалось подключится к серверу");
                }

                stream.CopyTo(reqStream2, pushSize);
                using var resp = request.GetResponse();
                if (temporarySize + pushSize >= stream.Length)
                {
                    stream.Position = temporarySize + 1;
                    break;
                }
                stream.Position = temporarySize + pushSize;
                temporarySize += pushSize;
                webHeader.Clear();
            }
            webHeader.Clear();
            logger.Info("Отправка последнего фрагмента:");
            if (temporarySize != stream.Length - 1)
            {
                logger.Info("Content-Range " + "bytes " + (temporarySize).ToString() + "-" + (stream.Length - 1).ToString() + "/" + (stream.Length).ToString());
                webHeader.Add("Authorization", "Bearer " + connection.Token.ToString());
                webHeader.Add("Content-Length", (stream.Length - temporarySize).ToString());
                webHeader.Add("Content-Range", "bytes " + (temporarySize).ToString() + "-" + (stream.Length - 1).ToString() + "/" + (stream.Length).ToString());

                var request = WebRequest.Create(connection.UploadUrl(GUID).ToString());
                request.Headers = webHeader;
                request.Method = "PUT";
                request.ContentType = "multipart/form-data";

                using var reqStream2 = request.GetRequestStream();
                if (reqStream2 == null)
                {
                    throw new UnconnectedException("Не удалось подключится к серверу");
                }
                stream.CopyTo(reqStream2);
                using var resp = request.GetResponse();
            }
            logger.Info("Файл успешно передан на сервера");
        }

        /// <summary>
        /// Получаем файл с сервера
        /// </summary>
        ///<exception cref="UnconnectedException">Не удалось подключиться к серверу</exception>
        public  Stream GetFile()
        {
            logger.Info("Запрос на загрузку файла из OneDrive");
            var webHeader = new WebHeaderCollection();
            var connect = new OneDriveConnection();
            webHeader.Add("Authorization", "Bearer " + connect.Token.ToString());
            var graphUrl = $"https://graph.microsoft.com/v1.0/drive/root:/{GUID}:/content?format=pdf";

            var request = WebRequest.Create(graphUrl);
            request.Headers = webHeader;
            request.Method = "GET";
            request.ContentType = "pdf/application";
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
        public void DeleteFile()
        {
            logger.Info("Запрос на удаление файла из OneDrive");
            var webHeader = new WebHeaderCollection();
            webHeader.Add("Authorization", "Bearer " + connection.Value.Token.ToString());
            var graphUrl = $"https://graph.microsoft.com/v1.0/drive/root:/{GUID}";
            var request = WebRequest.Create(graphUrl);
            request.Headers = webHeader;
            request.Method = "DELETE";
            using var resp = request.GetResponse() as HttpWebResponse;
            if (resp == null)
            {
                throw new UnconnectedException("Не удалось подключится к серверу");
            }
            using var stream = resp.GetResponseStream();
            logger.Info("Файл успешно удален с сервера");
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
