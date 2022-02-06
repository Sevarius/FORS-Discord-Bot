using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contract.Bamboo;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ORM;

namespace TosPlugin
{
    public class TosCommandService : ModuleBase<ICommandContext>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TosCommandService> _logger;

        public TosCommandService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = serviceProvider.GetRequiredService<ILogger<TosCommandService>>();
        }


        [Command("commits")]
        [Summary("Получить коммиты последней сборки")]
        public async Task GetLastCommitsFromPlan()
        {
            await using var db = _serviceProvider.GetRequiredService<MainContext>();
            var lastCommits = db.Plans.AsQueryable()
                .Where(p => p.BambooPlanName == "BSBUILD")
                .Select(p => p.PreviousCommits)
                .FirstOrDefault();
            string message;
            if (!string.IsNullOrEmpty(lastCommits))
            {
                var jiraIssues = JsonConvert.DeserializeObject<List<JiraIssue>>(lastCommits);
                message = $"В сборку попали следующие коммиты:\n{string.Join('\n', jiraIssues.Select(ji => $"{ji.Key} - {ji.Summary}"))}";
            }
            else
            {
                message = $"В сборку стендов не попали новые коммиты";
            }

            await ReplyAsync(message);
        }
        
        [Command("echo")]
        [Summary("Эхо")]
        public Task SayAsync([Remainder] [Summary("Строка для повторения")] string echo)
        {
            return ReplyAsync(echo);
        }
    }
}