using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using NLog;

namespace Converter
{
    class Authentification
    {
        string client_id;
        string client_secret;
        string tenant;
        string urlAuth;
        JToken token;
        static Logger logger = LogManager.GetCurrentClassLogger();

        public Authentification()
        {
            logger.Info("Выполняется загрузка конфигурации OneDrive");
            client_id = ConfigurationManager.AppSettings.Get("client_id");
            client_secret = ConfigurationManager.AppSettings.Get("client_secret");
            tenant = ConfigurationManager.AppSettings.Get("tenant");
            urlAuth = $"https://login.microsoftonline.com/{tenant}/oauth2/v2.0/token";
            logger.Info("Загрузка конфигурации OneDrive прошла успешно");
        }

        /// <summary>
        /// Аутентификация по принципу lazy
        /// </summary>
        public JToken Token
        {
            get { return token ??= Auth(); }
        }

        /// <summary>
        /// Аутентификация OneDrive
        /// </summary>
        /// <returns>Токен доступа</returns>
        /// <exception cref = "NullReferenceException">Не удалось аутентифицироваться с OneDrive</exception>
        private JToken Auth()
        {
            var body = new Dictionary<string, string>();
            body.Add("client_id", client_id);
            body.Add("scope", ".default");
            body.Add("grant_type", "client_credentials");
            body.Add("client_secret", client_secret);

            using var encodedBody = new FormUrlEncodedContent(body);
            logger.Info("Выполняется запрос на аутентификацию с OneDrive");
            using var httpClient = new HttpClient();
            using var responseMessage = httpClient.PostAsync(urlAuth, encodedBody).Result;
            if (responseMessage == null)
            {
                logger.Error("Не удалось выполнить запрос на аутентификацию");
                throw new NullReferenceException();
            }
            return ParseToken(responseMessage);
        }

        /// <summary>
        /// Парсим ответ от сервера
        /// </summary>
        /// <param name="responseMessage">Ответ от сервера</param>
        /// <returns>Токен доступа</returns>
        /// <exception cref="NullReferenceException">Не удалось аутентифицироваться с OneDrive</exception>
        private JToken ParseToken(HttpResponseMessage responseMessage)
        {
            var JSON = responseMessage.Content.ReadAsStringAsync().Result;
            var document = JObject.Parse(JSON);
            var token = document["access_token"];
            if (token == null)
            {
                logger.Error("Аутентификация не прошла");
                throw new NullReferenceException("Аутентификация не прошла");
            }
            logger.Debug("Аутентификация прошла успешно");
            return token;
        }
    }
}
