using System;
using System.Threading.Tasks;
using Common;
using Contract.Bamboo;
using Contract.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using ORM;

namespace BambooServices
{
    public abstract class BambooAnalyzer
    {
        protected readonly IServiceProvider _serviceProvider;
        private readonly string _plugin;

        protected BambooAnalyzer(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _plugin = _serviceProvider.GetRequiredService<IPluginGetter>().GetExecutingPluginName();
        }

        public abstract Task BuildStart(string planName);

        public abstract Task BuildEnd(BambooBuildModel buildModel);

        protected Task WriteMessageToMainChannel(string message)
        {
            using var scope = _serviceProvider.CreateScope();
            var rep = _serviceProvider.GetRequiredService<ICommonRepository>();
            var userInfo = rep.GerUserInfo();
            if (!userInfo.MainChatId.HasValue)
            {
                throw new ApplicationException($"Для плагина {_plugin} не указан Id главного чата");
            }
            var discordBot = _serviceProvider.GetRequiredService<IDiscordBot>();
            discordBot.WriteMessageToChannel(userInfo.MainChatId.Value, message);
            return Task.CompletedTask;
        }

        protected Task WriteMessageToRelatedChannel(string planName, string message)
        {
            using var scope = _serviceProvider.CreateScope();
            var rep = _serviceProvider.GetRequiredService<IBambooPlanRepository>();
            var planInfo = rep.GetPlanInfo(planName);
            if (!planInfo.RelatedChatId.HasValue)
            {
                throw new ApplicationException($"Для плагина {_plugin} для плана {planInfo.BambooPlanName} не указан id связанного чата");
            }
            var discordBot = _serviceProvider.GetRequiredService<IDiscordBot>();
            discordBot.WriteMessageToChannel(planInfo.RelatedChatId.Value, message);
            return Task.CompletedTask;
        }

        protected Task WriteMessageToChannel(ulong channelId, string message)
        {
            using var scope = _serviceProvider.CreateScope();
            var discordBot = _serviceProvider.GetRequiredService<IDiscordBot>();
            discordBot.WriteMessageToChannel(channelId, message);
            return Task.CompletedTask;
        }
    }
}