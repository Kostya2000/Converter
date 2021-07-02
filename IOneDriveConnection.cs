using System.Configuration;
using System.IO;
using NLog;

namespace Converter
{
    interface IOneDriveConnection
    {
        public static string client_id;
        public static string client_secret;
        public static string tenant;
        public static string urlAuth;

        /// <summary>
        /// Загрузка конфигурационных данных
        /// </summary>
        void Init(ILogger logger)
        {
            logger.Info("Выполняется загрузка конфигурации OneDrive");
            client_id = ConfigurationManager.AppSettings.Get("client_id");
            client_secret = ConfigurationManager.AppSettings.Get("client_secret");
            tenant = ConfigurationManager.AppSettings.Get("tenant");
            urlAuth = $"https://login.microsoftonline.com/{tenant}/oauth2/v2.0/token";
            logger.Info("Загрузка конфигурации OneDrive прошла успешно");
        }
    }
}
