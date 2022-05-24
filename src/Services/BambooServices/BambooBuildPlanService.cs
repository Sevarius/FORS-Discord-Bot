using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Common;
using Contract.Bamboo;
using Contract.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ORM;

namespace BambooServices
{
    public class BambooBuildPlanService : IBambooBuildPlanService
    {
        private readonly ILogger<BambooBuildPlanService> _logger;
        private readonly ICommonRepository _repository;
        private string _plugin;
        private string _token;
        private string _project;
        private readonly string _baseUrlArguments = "/bamboo/rest/api/latest/result/";
        private readonly string _baseUrl;


        public BambooBuildPlanService(ILogger<BambooBuildPlanService> logger, ICommonRepository repository, IPluginGetter pluginGetter, IConfiguration configuration)
        {
            _logger = logger;
            _repository = repository;
            _baseUrl = configuration["Bamboo:BaseUrl"];
            SetInitialInfo(pluginGetter);
        }

        private void SetInitialInfo(IPluginGetter pluginGetter)
        {
            _plugin = pluginGetter.GetExecutingPluginName();
            _token = _repository.GetBambooToken();
            _project = _repository.GetBambooProjectName();
        }

        public List<JiraIssue> GetCommitsForPlan(string planName, int count = 1, int start = 0)
        {
            if (string.IsNullOrEmpty(planName))
            {
                throw new ArgumentNullException(nameof(count),
                    "Имя плана не может быть пустым");
            }
                    
            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count),
                    "Значение количества сборок для выгрузки не может быть меньше либо равна нулю");
            }

            if (start < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(start),
                    "Значение стратового индекса выгрызки сборок плана не может быть меньше нуля");
            }

            var plansInfo = GetPlanBuilds(planName, count, start);

            var result = new List<JiraIssue>();
            plansInfo.ForEach(p => result.AddRange(p.JiraIssues));
            result = result.Distinct().ToList();
            return result;
        }

        public List<PlanInfo> GetPlanBuilds(string planName, int count = 1, int start = 0)
        {
            using (_logger.BeginScope($"Запрос к Bamboo на выгрузку списка сборок: Плагин-{_plugin}; Проект-{_project}"))
            {
                try
                {
                    if (string.IsNullOrEmpty(planName))
                    {
                        throw new ArgumentNullException(nameof(planName),
                            "Имя плана не может быть пустым");
                    }
                    
                    if (count <= 0)
                    {
                        throw new ArgumentOutOfRangeException(nameof(count),
                            "Значение количества сборок для выгрузки не может быть меньше либо равна нулю");
                    }

                    if (start < 0)
                    {
                        throw new ArgumentOutOfRangeException(nameof(start),
                            "Значение стратового индекса выгрызки сборок плана не может быть меньше нуля");
                    }
                    
                    var url = $"{_baseUrl}{_baseUrlArguments}{_project}-{planName}.json?start-index={start}&max-result={count}&expand=results.result.jiraIssues";

                    _logger.LogInformation($"Отправляем запрос по адресу: {url}");

                    var request = WebRequest.Create(url);
                    request.Method = "GET";
                    request.Headers.Add("Authorization", $"Bearer {_token}");

                    using var webResponse = request.GetResponse();
                    using var webStream = webResponse.GetResponseStream();
                    using var reader = new StreamReader(webStream ?? throw new InvalidOperationException());
                    var json = reader.ReadToEnd();

                    _logger.LogInformation($"Получен ответ: {json}");

                    var parsedJson = JObject.Parse(json);

                    if (parsedJson.ContainsKey("status-code"))
                    {
                        throw new ApplicationException(json);
                    }
                    
                    var plans = parsedJson["results"]?["result"]?.ToList();

                    var result = new List<PlanInfo>(count);

                    if (plans != null)
                    {
                        foreach (var token in plans)
                        {
                            var planInfo = token.ToObject<PlanInfo>();
                            if (planInfo == null)
                            {
                                throw new ApplicationException(
                                    $"Не удалось преобразовать json: {token} к объекту {nameof(PlanInfo)}");
                            }
                            planInfo.JiraIssues = GetCommitsForBuild(planName, planInfo.BuildNumber);
                            result.Add(planInfo);
                        }
                    }

                    _logger.LogInformation(
                        $"Была получена следующая информация о запусках плана:\n{JsonConvert.SerializeObject(result)}");

                    return result;
                }
                catch (Exception ex)
                {
                    var message = "Возникла ошибка при получении информации о запуске стендов";
                    _logger.LogError(ex, message);
                    throw new ApplicationException(message, ex);
                }
            }
        }

        private List<JiraIssue> GetCommitsForBuild(string planName, long buildNumber)
        {
            using (_logger.BeginScope($"Запрос к Bamboo на выгрузку информации об определённой сборке: Плагин-{_plugin}; Проект-{_project}; План-{planName}; Номер-{buildNumber}"))
            {
                if (string.IsNullOrEmpty(planName))
                {
                    throw new ArgumentNullException(nameof(planName),
                        "Имя плана не может быть пустым");
                }

                var url = $"{_baseUrl}{_baseUrlArguments}{_project}-{planName}-{buildNumber}.json?&expand=changes.change";

                _logger.LogInformation($"Отправляем запрос по адресу: {url}");

                var request = WebRequest.Create(url);
                request.Method = "GET";
                request.Headers.Add("Authorization", $"Bearer {_token}");

                using var webResponse = request.GetResponse();
                using var webStream = webResponse.GetResponseStream();
                using var reader = new StreamReader(webStream ?? throw new InvalidOperationException());
                var json = reader.ReadToEnd();

                _logger.LogInformation($"Получен ответ: {json}");

                var parsedJson = JObject.Parse(json);

                if (parsedJson.ContainsKey("status-code"))
                {
                    throw new ApplicationException(json);
                }
                
                var commits = parsedJson["changes"]?["change"]?.ToList() ?? new List<JToken>();

                var result = new List<JiraIssue>(commits.Count);
                
                foreach (var commit in commits)
                {
                    var commitComment = commit["comment"]?.Value<string>();
                    if (!string.IsNullOrEmpty(commitComment) && commitComment.ToLowerInvariant().StartsWith(_project.ToLowerInvariant()))
                    {
                        try
                        {
                            var jiraIssue = ParseCommitCommentString(commitComment);
                            result.AddRange(jiraIssue);
                        }
                        catch (Exception e)
                        {
                            _logger.LogCritical(e, $"Не удалось распарсить строку:{commitComment} для получения информации о PR");
                        }
                    }
                    
                }

                _logger.LogInformation(
                $"Была получена следующая информация о запусках плана:\n{JsonConvert.SerializeObject(result)}");

                return result;
            }
        }

        private List<JiraIssue> ParseCommitCommentString(string commitComment)
        {
            var result = new List<JiraIssue>();

            var secondTaskIndex = 0;
            do
            {
                var firstTaskIndex = secondTaskIndex;
                var temp = commitComment.IndexOf(_project, firstTaskIndex + 1, StringComparison.Ordinal);
                secondTaskIndex = temp;
                if (temp == -1){
                    temp = commitComment.Length;
                }
		
                var commit = SplitIssue(commitComment, firstTaskIndex, temp);
                result.Add(commit);
            }
            while(secondTaskIndex != -1);

            return result;
        }
        
        private JiraIssue SplitIssue(string message, int firstIndex, int endIndex)
        {
            var startIndex = firstIndex;
            
            if (endIndex - startIndex <= _project.Length)
            {
                throw new ApplicationException(
                    $"Ошибка при распарсивании подстроки с {startIndex} до {endIndex} для строки {message}");
            }
	
            startIndex += _project.Length;
            if (message[startIndex] == '-'){
                startIndex += 1;
            }
	
            var number = "";
	
            while(startIndex < message.Length && char.IsDigit(message[startIndex])){
                number += message[startIndex++];
            }

            var result = new JiraIssue();
            result.Key = $"{_project}-{number}";
            if (startIndex == number.Length)
            {
                result.Summary = string.Empty;
            }
            else
            {
                result.Summary = message.Substring(startIndex, endIndex - startIndex).Trim(' ', '-', '	', '\n', '\t', '\r', ';');
            }

            result.CommitMessage = message.Substring(firstIndex, endIndex - startIndex);

            return result;
        }
    }
}