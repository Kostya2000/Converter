using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace Converter
{
    class OneDriveConnection : IOneDriveConnection
    {
        private JToken token;

        /// <summary>
        /// Аутентификация по принципу lazy
        /// </summary>
        public JToken Token
        {
            get { return token ??= Connect(); }
        }

        /// <summary>
        /// Соединение с OneDrive
        /// </summary>
        /// <returns>Токен доступа</returns>
        /// <exception cref="NullReferenceException">Не удалось выполнить запрос на соединение</exception>
        private JToken Connect()
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
                IOneDriveConnection.logger.Error("Не удалось выполнить запрос на соединение");
                throw new NullReferenceException();
            }
            return ParseResponse(responseMessage);
        }

        /// <summary>
        /// Парсим ответ от сервера
        /// </summary>
        /// <param name="responseMessage">Ответ от сервера</param>
        /// <returns>Токен доступа</returns>
        /// <exception cref="NullReferenceException">Аутентификация не прошла</exception>
        private JToken ParseResponse(HttpResponseMessage responseMessage)
        {
            var JSON = responseMessage.Content.ReadAsStringAsync().Result;
            var document = JObject.Parse(JSON);
            var token = document["access_token"];
            if (token == null)
            {
                IOneDriveConnection.logger.Error("Аутентификация не прошла");
                throw new NullReferenceException("Аутентификация не прошла");
            }
            IOneDriveConnection.logger.Debug("Аутентификация прошла успешно");
            return token;
        }
    }
}
