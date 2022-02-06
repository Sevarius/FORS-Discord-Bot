using System;
using System.IO;
using System.Linq;
using System.Net;
using Common;
using Contract.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ORM;

namespace StendService
{
    public class StendVersionService : IStendVersionService
    {
        private readonly MainContext _db;
        private readonly IConfiguration _configuration;
        private readonly ILogger<StendVersionService> _logger;
        private readonly string _plugin;

        public StendVersionService(MainContext db, IConfiguration configuration, IPluginGetter pluginGetter, ILogger<StendVersionService> logger)
        {
            _db = db;
            _configuration = configuration;
            _logger = logger;
            _plugin = pluginGetter.GetExecutingPluginName();
            logger.LogInformation($"Сервис получения версии на стенде был создан для плагина {_plugin}");
        }

        public string GetVersionFromStend(string stendName)
        {
            var stendInfo = _db.Stends
                .AsQueryable()
                .FirstOrDefault(s => s.User.Info.PluginName == _plugin && s.StendName == stendName);

            if (stendInfo == null)
            {
                var message = $"Для плагина {_plugin} не удалось найти информацию о стенде с именем **{stendName}**";
                _logger.LogError(message);
                throw new ApplicationException(message);
            }

            if (string.IsNullOrEmpty(stendInfo.Url))
            {
                var message =
                    $"Для плагина {_plugin} для стенда **{stendName}** не указана URL для получения версии стенда";
                _logger.LogError(message);
                throw new ApplicationException(message);
            }

            if (!Uri.TryCreate(stendInfo.Url, UriKind.Absolute, out var uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
            {
                var message = $"Для плагина {_plugin} для стенда **{stendName}** URL:'{stendInfo.Url}' не является валидной";
                _logger.LogError(message);
                throw new ApplicationException(message);

            }
            
            var forsUser = new {Login = _configuration["Fors:Login"], Password = _configuration["Fors:Password"]};
            if (forsUser.Login == null || forsUser.Password == null)
            {
                var message = "Не удалось получить данные о пользователе для обращения к стендам из файла конфигурации";
                _logger.LogError(message);
                throw new ApplicationException(message);
            }
            
            var request = WebRequest.Create(stendInfo.Url);
            request.Method = "GET";
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("requestId", "Discord bot");
            request.Credentials = new NetworkCredential(forsUser.Login, forsUser.Password);
            
            using var webResponse = request.GetResponse();
            using var webStream = webResponse.GetResponseStream();
            using var reader = new StreamReader(webStream);
            var response = reader.ReadToEnd();

            return response;
        }
    }
}