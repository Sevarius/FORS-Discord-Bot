using System;
using System.Linq;
using System.Net;
using System.Text;
using Common;
using Contract.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ORM;

namespace JiraServices
{
    public class JiraLabelsService : IJiraLabelsService
    {
        private readonly ILogger<JiraLabelsService> _logger;
        private readonly ICommonRepository _commonRepository;
        private object _plugin;
        private string _token;
        private string _baseUrlArguments = "/jira/rest/api/latest/issue/";
        private readonly string _baseUrl;

        public JiraLabelsService(ILogger<JiraLabelsService> logger, ICommonRepository commonRepository, IPluginGetter pluginGetter, IConfiguration configuration)
        {
            _logger = logger;
            _commonRepository = commonRepository;
            _baseUrl = configuration["Jira:BaseUrl"];
            SetInitialInfo(pluginGetter);
        }

        private void SetInitialInfo(IPluginGetter pluginGetter)
        {
            _plugin = pluginGetter.GetExecutingPluginName();
            _token = _commonRepository.GetJiraToken();
            if (string.IsNullOrEmpty(_token))
            {
                var message = $"Не удалось найти информацию о токене jira для плагина: {_plugin}";
                _logger.LogError(message);
                throw new ApplicationException(message);
            }
        }

        public void PutLabelsToTask(string taskKey, params string[] labels)
        {
            using (_logger.BeginScope($"Запрос к Jira на добавление метjr {string.Join(", ", labels)} к задаче {taskKey}"))
            {
                try
                {
                    if (string.IsNullOrEmpty(taskKey))
                    {
                        throw new ArgumentNullException(nameof(taskKey),
                            "Ключ задачи не может быть пустым");
                    }
                    
                    var url = $"{_baseUrl}{_baseUrlArguments}{taskKey}";

                    _logger.LogInformation($"Отправляем запрос по адресу: {url}");

                    var request = WebRequest.Create(url);
                    request.Method = "PUT";
                    request.Headers.Add("Authorization", $"Bearer {_token}");
                    var operations = string.Join(",", labels.Select(l => $"{{\"add\":\"{l}\"}}"));
                    var requestBody = $@"{{""update"":{{""labels"":[{operations}]}}}}";
                    ASCIIEncoding encoding = new ASCIIEncoding();
                    byte[] bodyBytes = encoding.GetBytes(requestBody);
                    request.ContentType = "application/json";
                    request.ContentLength = bodyBytes.Length;
                    var stream = request.GetRequestStream();
                    stream.Write(bodyBytes, 0, bodyBytes.Length);

                    using var webResponse = request.GetResponse();
                    if (((HttpWebResponse) webResponse).StatusCode != (HttpStatusCode) 204)
                    {
                        throw new ApplicationException(
                            $"Не удалось установить метку к задаче {taskKey}; Запрос вернул код {((HttpWebResponse) webResponse).StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    var message = "Возникла ошибка при добавлении метки к задаче";
                    _logger.LogError(ex, message);
                    throw new ApplicationException(message, ex);
                }
            }
        }
    }
}