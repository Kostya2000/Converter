using System;
using System.IO;
using NLog;

namespace Converter
{
    class OneDriveConverter
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Отправляем файл на сервер
        /// </summary>
        /// <param name="stream">Поток данных</param>
        /// <exception cref="NullReferenceException">Не удалось подключится к серверу</exception>
        /// <exception cref="ArgumentNullException">"Данные из файла не загружены"</exception>
        public void SendFile(Stream stream)
        {
            logger.Info("Запрос на отправку файла на OneDrive");
            if (stream == null)
            {
                logger.Error("Данные из файла не загружены");
                throw new ArgumentNullException("Данные из файла не загружены");
            }
            var webHeader = new System.Net.WebHeaderCollection();
            webHeader.Add("Authorization", "Bearer " + Authentification.Token.ToString());
            var graphUrl = $"https://graph.microsoft.com/v1.0/drive/root:/{GUID}:/content";
            
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
        public Stream GetFile()
        {
            logger.Info("Запрос на загрузку файла из OneDrive");
            var webHeader = new System.Net.WebHeaderCollection();
            webHeader.Add("Authorization", "Bearer " + Authentification.Token.ToString());
            var graphUrl = $"https://graph.microsoft.com/v1.0/drive/root:/{GUID}:/content?format=pdf";

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
            var graphUrl = $"https://graph.microsoft.com/v1.0/drive/root:/{GUID}";
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

        private Authentification Authentification = new Authentification();
        private string guid = null;

        /// <summary>
        /// Генерация guid по принципу lazy
        /// </summary>
        private string GUID
        {
            get { return guid ??= Guid.NewGuid().ToString() + ".docx"; }
        }
    }
}
