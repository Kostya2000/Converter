using System.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Converter
{
    interface IOneDriveConnection
    {
        public static string client_id;
        public static string client_secret;
        public static string tenant;
        public static string urlAuth;

        /// <summary>
        /// Загрузка конфигурации
        /// </summary>
        /// <param name = "logger"> Логгер </param>
        void Init(out Microsoft.Extensions.Logging.ILogger logger)
        {
            var loggerFactory = new LoggerFactory();
            var loggerConfig = new LoggerConfiguration()
            .WriteTo.File($"logs\\{System.DateTime.Today}.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();
            loggerFactory.AddSerilog(loggerConfig);
            logger = loggerFactory.CreateLogger<OneDriveConverterConsole>();
            logger.LogInformation("Выполняется загрузка конфигурации OneDrive");
            client_id = ConfigurationManager.AppSettings.Get("client_id");
            client_secret = ConfigurationManager.AppSettings.Get("client_secret");
            tenant = ConfigurationManager.AppSettings.Get("tenant");
            urlAuth = $"https://login.microsoftonline.com/{tenant}/oauth2/v2.0/token";
            logger.LogInformation("Загрузка конфигурации OneDrive прошла успешно");
        }
    }
}
