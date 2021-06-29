using System;
using System.IO;
using NLog;

namespace Converter
{
    class OneDriveConverter
    {
        Authentification Authentification = new Authentification();
        string guid;
        static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Отправляем файл на сервер
        /// </summary>
        /// <param name="stream">Поток данных</param>
        /// <exception cref="NullReferenceException">Не удалось подключится к серверу</exception>
        public void SendFile(Stream stream)
        {
            logger.Info("Запрос на отправку файла на OneDrive");
            var webHeader = new System.Net.WebHeaderCollection();
            webHeader.Add("Authorization", "Bearer " + Authentification.Token.ToString());
            var graphUrl = $"https://graph.microsoft.com/v1.0/drive/root:/{guid}:/content";
            
            var request = System.Net.WebRequest.Create(graphUrl);
            request.Headers = webHeader;
            request.Method = "PUT";
            request.ContentType = "text/plain";
            using var reqStream = request.GetRequestStream();
            if (reqStream == null)
            {
                logger.Error("Не удалось подключится к серверу");
                throw new NullReferenceException("Не удалось подключится к серверу");
            }
            stream.CopyTo(reqStream);
            using var resp = (System.Net.HttpWebResponse)request.GetResponse();
            logger.Info("Файл успешно отправлен на сервер");
        }

        /// <summary>
        /// Получаем файл с сервера
        /// </summary>
        ///<exception cref="NullReferenceException">Не удалось подключится к серверу</exception> 
        ///<exception cref="Exception">Возможно нет доступа к интернету</exception>
        public Stream GetFile()
        {
            logger.Info("Запрос на загрузку файла из OneDrive");
            var webHeader = new System.Net.WebHeaderCollection();
            webHeader.Add("Authorization", "Bearer " + Authentification.Token.ToString());
            var graphUrl = $"https://graph.microsoft.com/v1.0/drive/root:/{guid}:/content?format=pdf";

            var request = System.Net.WebRequest.Create(graphUrl);
            request.Headers = webHeader;
            request.Method = "GET";
            request.ContentType = "pdf/application";
            var resp = (System.Net.HttpWebResponse)request.GetResponse();
            if (resp == null)
            {
                logger.Error("Не удалось подключится к серверу");
                throw new NullReferenceException("Не удалось подключится к серверу");
            }
            var stream = resp.GetResponseStream();
            logger.Info("Файл успешно загружен из OneDrive");
            return stream;   
        }

        /// <summary>
        /// Удаляем файла с сервера
        /// </summary>
        /// <exception cref="NullReferenceException">Не удалось подключится к серверу</exception>
        public void DeleteFile()
        {
            logger.Info("Запрос на удаление файла из OneDrive");
            var webHeader = new System.Net.WebHeaderCollection();
            webHeader.Add("Authorization", "Bearer " + Authentification.Token.ToString());
            var graphUrl = $"https://graph.microsoft.com/v1.0/drive/root:/{guid}";
            var request = System.Net.WebRequest.Create(graphUrl);
            request.Headers = webHeader;
            request.Method = "DELETE";
            using var resp = (System.Net.HttpWebResponse)request.GetResponse();
            if (resp == null)
            {
                logger.Error("Не удалось подключится к серверу");
                throw new NullReferenceException("Не удалось подключится к серверу");
            }
            using var stream = resp.GetResponseStream();
            logger.Info("Файл успешно удален с сервера");
        }

        /// <summary>
        /// Конструктор с загрузкой конфигурационных данных
        /// </summary>
        public OneDriveConverter()
        {
            guid = Guid.NewGuid().ToString() + ".docx";
        }
    }
}
