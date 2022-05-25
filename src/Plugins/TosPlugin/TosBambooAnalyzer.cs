using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using BambooServices;
using Contract.Bamboo;
using Contract.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ORM;
using Polly;

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
            using var db = _serviceProvider.GetService<MainContext>();
            using var tr = CreateTransaction(db, IsolationLevel.Serializable);
            var plan = db.Plans.AsQueryable().FirstOrDefault(p => p.BambooPlanName == planName);

            if (plan == null)
            {
                tr.Commit();
                WriteMessageToMainChannel($"Сборка плана {planName} начата");
                return Task.CompletedTask;
            }

            plan.BuildStartCount += 1;
            db.SaveChanges();

            switch (planName)
            {
                case "BSBUILD":
                case "IAS":
                    var dev = db.Plans.AsQueryable()
                        .Where(p => p.BambooPlanName == "BSBUILD")
                        .Select(p => p.BuildStartCount)
                        .FirstOrDefault();
                    var adp = db.Plans.AsQueryable()
                        .Where(p => p.BambooPlanName == "IAS")
                        .Select(p => p.BuildStartCount)
                        .FirstOrDefault();
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
            tr.Commit();
            return Task.CompletedTask;
        }

        public override Task BuildEnd(BambooBuildModel buildModel)
        {
            var planName = buildModel.Build.BuildResultKey.Split("-")[1];
            
            using var db = _serviceProvider.GetService<MainContext>();
            using var tr = CreateTransaction(db, IsolationLevel.Serializable);
            
            var plan = db.Plans.AsQueryable().FirstOrDefault(p => p.BambooPlanName == planName);
            if (plan == null)
            {
                tr.Commit();
                _logger.LogInformation($"План {planName} не отслеживается системой");
                WriteMessageToMainChannel($"Сборка плана {planName} окончена");
                return Task.CompletedTask;
            }

            plan.BuildEndCount += 1;
            db.SaveChanges();

            _logger.LogInformation($"Получена команда об окончании сборки плана {planName}");

            switch (planName)
            {
                case "BSBUILD":
                case "IAS":
                    var dev = db.Plans.AsQueryable()
                        .Where(p => p.BambooPlanName == "BSBUILD")
                        .Select(p => p.BuildEndCount)
                        .FirstOrDefault();
                    var adp = db.Plans.AsQueryable()
                        .Where(p => p.BambooPlanName == "IAS")
                        .Select(p => p.BuildEndCount)
                        .FirstOrDefault();
                    if (adp == dev)
                    {
                        SendCommits(db);
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

            tr.Commit();
            return Task.CompletedTask;
        }

        private void SendCommits(MainContext db)
        {
            var planInfo = _bambooBuildPlanService.GetPlanBuilds("BSBUILD").First();
            if (planInfo.SuccessfulTestCount == 0 && planInfo.SkippedTestCount == 0 && planInfo.FailedTestCount == 0)
            {
                WriteMessageToMainChannel("**Произошла ошибка сборки стендов dev, adptest!**");
                return;
            }

            var webPlan = db.Plans.AsQueryable().FirstOrDefault(p => p.BambooPlanName == "WEBSTEND");
            var webEndCount = webPlan.BuildEndCount;
            webPlan.BuildStartCount = 0;
            webPlan.BuildEndCount = 0;
            db.SaveChanges();
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

            var devStend = db.Plans.AsQueryable().FirstOrDefault(p => p.BambooPlanName == "BSBUILD");
            var adpTestStend = db.Plans.AsQueryable().FirstOrDefault(p => p.BambooPlanName == "IAS");
            if (!string.IsNullOrEmpty(devStend.PreviousCommits))
            {
                var prevCommits = JsonConvert.DeserializeObject<List<JiraIssue>>(devStend.PreviousCommits);
                commits.RemoveAll(c => prevCommits.Contains(c));
            }
            var commitsJson = JsonConvert.SerializeObject(commits);
            devStend.PreviousCommits = commitsJson;
            adpTestStend.PreviousCommits = commitsJson;
            db.SaveChanges();
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

        private IDbContextTransaction CreateTransaction(DbContext db, IsolationLevel isolationLevel)
        {
            var tr = Policy
                .Handle<SqliteException>(e => e.SqliteErrorCode == 5)
                .WaitAndRetry(3, x => TimeSpan.FromSeconds(2))
                .Execute(() => db.Database.BeginTransaction(isolationLevel));

            return tr;
        }
    }
}