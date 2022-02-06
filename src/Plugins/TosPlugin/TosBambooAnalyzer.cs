using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BambooServices;
using Contract.Bamboo;
using Contract.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ORM;

namespace TosPlugin
{
    public class TosBambooAnalyzer : BambooAnalyzer
    {
        private readonly ILogger<TosBambooAnalyzer> _logger;
        private readonly IJiraLabelsService _jira;
        private readonly IBambooBuildPlanService _bambooBuildPlanService;
        private readonly IBambooPlanRepository _planRepository;

        public TosBambooAnalyzer(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _logger = serviceProvider.GetRequiredService<ILogger<TosBambooAnalyzer>>();
            _jira = serviceProvider.GetRequiredService<IJiraLabelsService>();
            _bambooBuildPlanService = serviceProvider.GetRequiredService<IBambooBuildPlanService>();
            _planRepository = serviceProvider.GetRequiredService<IBambooPlanRepository>();
        }

        public override Task BuildStart(string planName)
        {
            var plan = _planRepository.GetPlanInfo(planName);

            if (plan == null)
            {
                WriteMessageToMainChannel($"Сборка плана {planName} начата");
                return Task.CompletedTask;
            }

            plan.BuildStartCount += 1;
            _planRepository.UpdatePlan(plan);

            switch (planName)
            {
                case "BSBUILD":
                case "IAS":
                    var dev = _planRepository.GetPlanInfo("BSBUILD").BuildStartCount;
                    var adp = _planRepository.GetPlanInfo("IAS").BuildStartCount;
                    if (dev != adp)
                    {
                        WriteMessageToMainChannel($"Стенды не работают - идет сборка");
                    }

                    break;
                case "DIS":
                    WriteMessageToMainChannel("Тестовый план начат");
                    break;
                case "UNIT":
                    break;
                case "SECURITY":
                    break;
                case "STBS":
                    WriteMessageToRelatedChannel("STBS", "Стенд security не работает - идёт сборка");
                    break;
            }

            return Task.CompletedTask;
        }

        public override Task BuildEnd(BambooBuildModel buildModel)
        {
            var planName = buildModel.Build.BuildResultKey.Split("-")[1];
            var plan = _planRepository.GetPlanInfo(planName);
            if (plan == null)
            {
                _logger.LogInformation($"План {planName} не отслеживается системой");
                WriteMessageToMainChannel($"Сборка плана {planName} окончена");
                return Task.CompletedTask;
            }

            plan.BuildEndCount += 1;
            _planRepository.UpdatePlan(plan);

            _logger.LogInformation($"Получена команда об окончании сборки плана {planName}");

            switch (planName)
            {
                case "BSBUILD":
                case "IAS":
                    var dev = _planRepository.GetPlanInfo("BSBUILD").BuildEndCount;
                    var adp = _planRepository.GetPlanInfo("IAS").BuildEndCount;
                    if (adp == dev)
                    {
                        SendCommits();
                    }
                    break;
                case "WEBSTEND":
                    WebBuild(buildModel.Build.Status,null,  "dev", "adptest");
                    break;
                case "DIS":
                    WriteMessageToMainChannel($"Сборка тестового плана окончена");
                    break;
                case "UNIT":
                    SendTestsInfo();
                    break;
                case "SECURITY":
                    WebBuild(buildModel.Build.Status, "STBS", "security");
                    break;
                case "STBS":
                    var sec = _planRepository.GetPlanInfo("STBS");
                    if (sec.BuildStartCount == sec.BuildEndCount)
                    {
                        SendCommitsSecurity();
                    }
                    break;
            }

            return Task.CompletedTask;
        }

        private void SendCommits()
        {
            var planInfo = _bambooBuildPlanService.GetPlanBuilds("BSBUILD").First();
            if (planInfo.SuccessfulTestCount == 0 && planInfo.SkippedTestCount == 0 && planInfo.FailedTestCount == 0)
            {
                WriteMessageToMainChannel("**Произошла ошибка сборки стендов dev, adptest!**");
                return;
            }

            var webPlan = _planRepository.GetPlanInfo("WEBSTEND");
            var webEndCount = webPlan.BuildEndCount;
            webPlan.BuildStartCount = 0;
            webPlan.BuildEndCount = 0;
            _planRepository.UpdatePlan(webPlan);
            var commits = new List<JiraIssue>(planInfo.JiraIssues);
            if (webEndCount > 0)
            {
                var webCommits = _bambooBuildPlanService.GetCommitsForPlan("WEBSTEND", webEndCount);
                commits.AddRange(webCommits);
            }

            var adpCommits = _bambooBuildPlanService.GetCommitsForPlan("IAS");
            commits.AddRange(adpCommits);
            commits = commits.Distinct().ToList();
            commits.RemoveAll(c => c.Key == "SURFNS-250");
            commits.ForEach(c =>
            {
                try
                {
                    _jira.PutLabelsToTask(c.Key, "$dev", "$adptest");
                }
                catch (Exception e)
                {
                    _logger.LogCritical(e, $"Не удалось добавить метки [\"$dev\" \"$adptest\"] к задаче {c.Key}");
                }
            });

            var devStend = _planRepository.GetPlanInfo("BSBUILD");
            var adpTestStend = _planRepository.GetPlanInfo("IAS");
            if (!string.IsNullOrEmpty(devStend.PreviousCommits))
            {
                var prevCommits = JsonConvert.DeserializeObject<List<JiraIssue>>(devStend.PreviousCommits);
                commits.RemoveAll(c => prevCommits.Contains(c));
            }
            var commitsJson = JsonConvert.SerializeObject(commits);
            devStend.PreviousCommits = commitsJson;
            adpTestStend.PreviousCommits = commitsJson;
            _planRepository.UpdatePlan(devStend);
            _planRepository.UpdatePlan(adpTestStend);
            string message;
            if (commits.Any())
            {
                message =
                    $"В сборку попали следующие коммиты:\n{string.Join('\n', commits.Select(ji => $"{ji.Key} - {ji.Summary}"))}";
            }
            else
            {
                message = $"В сборку стендов не попали новые коммиты";
            }

            WriteMessageToMainChannel(message);
        }

        private void SendCommitsSecurity()
        {
            var planInfo = _bambooBuildPlanService.GetPlanBuilds("STBS").First();
            if (planInfo.SuccessfulTestCount == 0 && planInfo.SkippedTestCount == 0 && planInfo.FailedTestCount == 0)
            {
                WriteMessageToMainChannel("**Произошла ошибка сборки стендa security!**");
                return;
            }

            var webPlan = _planRepository.GetPlanInfo("SECURITY");
            var webEndCount = webPlan.BuildEndCount;
            webPlan.BuildStartCount = 0;
            webPlan.BuildEndCount = 0;
            _planRepository.UpdatePlan(webPlan);
            var commits = new List<JiraIssue>(planInfo.JiraIssues);
            if (webEndCount > 0)
            {
                var webCommits = _bambooBuildPlanService.GetCommitsForPlan("SECURITY", webEndCount);
                commits.AddRange(webCommits);
            }

            var secCommits = _bambooBuildPlanService.GetCommitsForPlan("STBS");
            commits.AddRange(secCommits);
            commits = commits.Distinct().ToList();
            commits.RemoveAll(c => c.Key == "SURFNS-250");
            commits.ForEach(c =>
            {
                try
                {
                    _jira.PutLabelsToTask(c.Key, "$sec");
                }
                catch (Exception e)
                {
                    _logger.LogCritical(e, $"Не удалось добавить метку [\"$sec\"] к задаче {c.Key}");
                }
            });

            var secTestStend = _planRepository.GetPlanInfo("STBS");
            secTestStend.PreviousCommits = string.Join("\n", commits.Select(c => $"{c.Key} - {c.Summary}"));
            _planRepository.UpdatePlan(secTestStend);
            string message;
            if (commits.Any())
            {
                message =
                    $"В сборку попали следующие коммиты:\n{string.Join('\n', commits.Select(ji => $"{ji.Key} - {ji.Summary}"))}";
            }
            else
            {
                message = $"В сборку стендов не попали новые коммиты";
            }

            WriteMessageToRelatedChannel("STBS", message);
        }

        private void WebBuild(string status, string planName, params string[] stendName)
        {
            string message;
            switch (status)
            {
                case "SUCCESS":
                    message =
                        $"Фронт был загружен в репозиторий {(stendName.Length == 1 ? "стенда" : "стендов")} **{string.Join("**, **", stendName)}**";
                    break;
                case "FAILED":
                    message =
                        $"Возникла ошибка при сборке фронта в репозиторий {(stendName.Length == 1 ? "стенда" : "стендов")} **{string.Join("**, **", stendName)}**";
                    break;
                default:
                    message =
                        $"Пришёл **необработанный статус** сборки {status} при сборке фронта для репозитория {(stendName.Length == 1 ? "стенда" : "стендов")} **{string.Join("**, **", stendName)}**";
                    _logger.LogInformation(message);
                    break;
            }

            if (string.IsNullOrEmpty(planName))
            {
                WriteMessageToMainChannel(message);
            }
            else
            {
                WriteMessageToRelatedChannel(planName, message);
            }
        }

        private void SendTestsInfo()
        {
            var unitPlanInfo = _bambooBuildPlanService.GetPlanBuilds("UNIT").First();
            string message;
            if (unitPlanInfo.FailedTestCount == 0 && unitPlanInfo.SkippedTestCount == 0 &&
                unitPlanInfo.SuccessfulTestCount == 0)
            {
                message = "**Произошла ошибка выполнения плана UNIT!**";
                WriteMessageToMainChannel(message);
                return;
            }

            var mainPlanInfo = _bambooBuildPlanService.GetPlanBuilds("BSBUILD").First();

            var totalFailedTestCount = unitPlanInfo.FailedTestCount + mainPlanInfo.FailedTestCount;

            if (totalFailedTestCount != 0)
            {
                message = $"Количество упавших тестов: {totalFailedTestCount}";
                WriteMessageToMainChannel(message);
            }
        }
    }
}