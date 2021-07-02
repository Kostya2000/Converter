using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace Converter
{
    class OneDriveConnection : IOneDriveConnection
    {
        private JToken token;
        private JToken uploadUrl;

        public void Connector(ref Stream reqStream, out WebRequest request, string method, string graphUrl, string contentType, bool returnReqStream = false, WebHeaderCollection header = null)
        {
            if (header == null)
            {
                header = new WebHeaderCollection();
            }
            header.Add("Authorization", "Bearer " + Token.ToString());
            request = WebRequest.Create(graphUrl);
            request.Headers = header;
            request.Method = method;
            request.ContentType = contentType;
            if (returnReqStream == false)
            {
                return;
            }
            reqStream = request.GetRequestStream();
            if (reqStream == null)
            {
                throw new UnconnectedException("Не удалось подключится к серверу");
            }
        }

        /// <summary>
        /// Аутентификация по принципу lazy
        /// </summary>
        public JToken Token
        {
            get { return token ??= GetToken(); }
        }

        /// <summary>
        /// Получаем ресурс для отправки диапозонов байт
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public JToken UploadUrl(string guid)
        {
           return uploadUrl ??= GetUploadUrl(guid); 
        }

        /// <summary>
        /// Соединение с OneDrive
        /// </summary>
        /// <returns>Токен доступа</returns>
        /// <exception cref="UnconnectedException">Не удалось подключиться к серверу</exception>
        private JToken GetToken()
        {
            var body = new Dictionary<string, string>();
            body.Add("client_id", IOneDriveConnection.client_id);
            body.Add("scope", ".default");
            body.Add("grant_type", "client_credentials");
            body.Add("client_secret", IOneDriveConnection.client_secret);

            using var encodedBody = new FormUrlEncodedContent(body);
            IOneDriveConnection.logger.Info("Выполняется запрос на соединение с OneDrive");
            using var httpClient = new HttpClient();
            using var responseMessage = httpClient.PostAsync(IOneDriveConnection.urlAuth, encodedBody).Result;
            if (responseMessage == null)
            {
                throw new UnconnectedException();
            }
            return ParseToken(responseMessage);
        }

        /// <summary>
        /// Парсим ответ от сервера
        /// </summary>
        /// <param name="responseMessage">Ответ от сервера</param>
        /// <returns>Токен доступа</returns>
        /// <exception cref="UnauthorizedAccessException">Аутентификация не прошла</exception>
        private JToken ParseToken(HttpResponseMessage responseMessage)
        {
            var JSON = responseMessage.Content.ReadAsStringAsync().Result;
            var document = JObject.Parse(JSON);
            var token = document["access_token"];
            if (token == null)
            {
                throw new UnauthorizedAccessException("Аутентификация не прошла");
            }
            IOneDriveConnection.logger.Debug("Аутентификация прошла успешно");
            return token;
        }

        /// <summary>
        /// Запрос на получение ресурса для отправки большого файла
        /// </summary>
        /// <returns>Url адрес ресурса</returns>
        /// <exception cref="UnconnectedException">Не удалось подключиться к серверу</exception>
        private JToken GetUploadUrl(string guid)
        {
            IOneDriveConnection.logger.Info("Выполняется запрос на получение URL с OneDrive");
            var reqStream = Stream.Null;
            WebRequest request;
            Connector(ref reqStream, out request, "POST", $"https://graph.microsoft.com/v1.0/drive/root:/{guid}:/createUploadSession",null);
            var response = request.GetResponse() as HttpWebResponse;
            var responseStream = new StreamReader(response.GetResponseStream());
            if (responseStream == null)
            {
                throw new UnconnectedException("Не удалось подключиться к серверу");
            }
            return ParseUploadUrl(responseStream);
        }

        /// <summary>
        /// Парсим ответ от сервера
        /// </summary>
        /// <param name="responseMessage">Ответ от сервера</param>
        /// <returns>Url адрес ресурса</returns>
        /// <exception cref="HttpRequestException">Неправильный запрос к серверу</exception>
        private JToken ParseUploadUrl(StreamReader responseMessage)
        {
            var responseString = responseMessage.ReadToEnd();
            var document = JObject.Parse(responseString);
            var uploadUrl = document["uploadUrl"];
            if (uploadUrl == null)
            {
                throw new HttpRequestException("Неправильный запрос к серверу");
            }
            IOneDriveConnection.logger.Debug("Url адрес ресурса получен успешно");
            return uploadUrl;
        }  
    }
}
