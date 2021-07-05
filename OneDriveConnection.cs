using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;

namespace Converter
{
    class OneDriveConnection : IOneDriveConnection
    {
        private JToken token;
        private JToken uploadUrl;
        private readonly ILogger logger;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name = "logger"> Логгер </param>
        public OneDriveConnection(ILogger logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Установка соединения с сервером
        /// </summary>
        /// <param name = "reqStream"> [Ссылка ref] Поток для отправки тела запроса </param>
        /// <param name = "request"> [Ссылка out] Запрос </param>
        /// <param name = "method"> Метод </param>
        /// <param name = "graphUrl"> Конечная точка </param>
        /// <param name = "contentType"> Тип контента </param>
        /// <param name = "returnReqStream"> [Необязательный параметр] Открыть поток для открытия тела запроса </param>
        /// <param name = "needAuth"> [Необязательный параметр] Наличие необходимости в токене доступа </param>
        /// <param name = "header"> [Необязательный параметр] Дополнительные загаловки запроса. Примечание: указывать токен авторизации не нужно </param>
        public void Connect(ref Stream reqStream, out WebRequest request, string method, string graphUrl, string contentType, bool returnReqStream = false, bool needAuth = true, WebHeaderCollection header = null)
        {
            logger.LogInformation($"Выполнение {method} запроса к серверу...");
            if (header == null)
            {
                header = new WebHeaderCollection();
            }
            if (needAuth)
            {
                header.Add("Authorization", "Bearer " + Token.ToString());
            }
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
                logger.LogError("Не удалось подключится к серверу");
                throw new UnconnectedException("Не удалось подключится к серверу");
            }
            logger.LogInformation($"Запрос {method} успешно выполнен");
        }

        /// <summary>
        /// Аутентификация по принципу lazy
        /// </summary>
        public JToken Token
        {
            get { return token ??= GetToken(); }
        }

        /// <summary>
        /// Получение ресурс для отправки диапозонов байт
        /// </summary>
        /// <param name = "guid"> GUID файла </param>
        /// <returns> Url адрес ресурса </returns>
        public JToken UploadUrl(string guid)
        {
           return uploadUrl ??= GetUploadUrl(guid); 
        }

        /// <summary>
        /// Соединение с OneDrive
        /// </summary>
        /// <returns> Токен доступа </returns>
        /// <exception cref = "UnconnectedException"> Не удалось подключиться к серверу </exception>
        private JToken GetToken()
        {
            var body = new Dictionary<string, string>();
            body.Add("client_id", IOneDriveConnection.client_id);
            body.Add("scope", ".default");
            body.Add("grant_type", "client_credentials");
            body.Add("client_secret", IOneDriveConnection.client_secret);
            var encodedBody = new FormUrlEncodedContent(body).ReadAsByteArrayAsync();
            var reqStream = Stream.Null;
            Connect(ref reqStream, out var request, "POST", IOneDriveConnection.urlAuth, null, true, false);
            reqStream.Write(encodedBody.Result, 0, encodedBody.Result.Length);
            var responseMessage = request.GetResponse();
            if (responseMessage == null)
            {
                logger.LogCritical("Соединение разорвано");
                throw new UnconnectedException();
            }
            var response = new StreamReader(responseMessage.GetResponseStream());
            return GetAttrValue(response, "access_token");
        }

        /// <summary>
        /// Запрос на получение ресурса для отправки большого файла
        /// </summary>
        /// <returns> Url адрес ресурса </returns>
        /// <exception cref = "UnconnectedException"> Не удалось подключиться к серверу </exception>
        private JToken GetUploadUrl(string guid)
        {
            logger.LogInformation("Выполняется запрос на получение URL с OneDrive");
            var reqStream = Stream.Null;
            Connect(ref reqStream, out var request, "POST", $"https://graph.microsoft.com/v1.0/drive/root:/{guid}:/createUploadSession", null);
            var response = request.GetResponse() as HttpWebResponse;
            var responseStream = new StreamReader(response.GetResponseStream());
            if (responseStream == null)
            {
                logger.LogCritical("Не удалось подключиться к серверу");
                throw new UnconnectedException("Не удалось подключиться к серверу");
            }
            return GetAttrValue(responseStream, "uploadUrl");
        }

        /// <summary>
        /// Получаем значение по заданному атрибуту из сообщения от сервера
        /// </summary>
        /// <param name = "responseMessage"> Сообщение от сервера </param>
        /// <param name = "attr"> Заданный атрибут </param>
        /// <returns></returns>
        private JToken GetAttrValue(StreamReader responseMessage, string attr)
        {
            var responseString = responseMessage.ReadToEnd();
            var document = JObject.Parse(responseString);
            var attrValue = document[attr];
            if (attrValue == null)
            {
                logger.LogCritical("Неправильный запрос к серверу");
                throw new HttpRequestException("Неправильный запрос к серверу");
            }
            logger.LogInformation($"Значение атрибута {attr} получен успешно");
            return attrValue;
        }
    }
}
