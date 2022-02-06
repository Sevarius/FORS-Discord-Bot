using System;
using System.Linq;
using Contract.Bamboo;
using Contract.Interfaces;
using DiscordServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ORM;
using PluginLoader;

namespace WebAppCore.Controllers
{
    [Route("bamboo")]
    public class BambooApiController : Controller
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IDiscordBot _discordBot;
        private readonly ILogger<BambooApiController> _logger;

        public BambooApiController(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = serviceProvider.GetRequiredService<ILogger<BambooApiController>>();
            _discordBot = _serviceProvider.GetService<IDiscordBot>();
        }

        [HttpGet]
        [Route("start/{planName}")]
        public void BuildStart(string planName)
        {
            using (_logger.BeginScope($"{nameof(BambooApiController)}.{nameof(BuildStart)} для плана {planName}"))
            {
                try
                {
                    _logger.LogInformation($"Начало работы для плана: {planName}");
                    using var db = _serviceProvider.GetRequiredService<MainContext>();
                    var projectName = db.Plans
                        .AsQueryable()
                        .Where(p => p.BambooPlanName == planName)
                        .Join(db.UserInfos, plan => plan.UserId, userInfo => userInfo.UserId, (id, userInfo) => userInfo.BambooProjectName)
                        .FirstOrDefault();
                    _logger.LogInformation($"Для плана: {planName} был найден проект: {projectName}");
                    if (string.IsNullOrEmpty(projectName))
                    {
                        _logger.LogError($"Нет информации о проекте с планом {planName}");
                        return;
                    }
                    
                    Console.WriteLine($"Начало сборки: {projectName}-{planName}");
                    
                    var mainChatId = db.UserInfos
                        .AsQueryable()
                        .Where(ui => ui.BambooProjectName == projectName)
                        .Select(ui => ui.MainChatId)
                        .FirstOrDefault();
                    _logger.LogInformation($"Для проекта: {projectName} был найден Id главного чата: {mainChatId}");
                    
                    if (mainChatId == null)
                    {
                        var message = $"Не удалось получить Id главного чата для проекта {projectName}";
                        _logger.LogError(message);
                        return;
                    }
                    
                    var pluginName = db.UserInfos.AsQueryable()
                        .Where(ui => ui.BambooProjectName == projectName)
                        .Select(ui => ui.PluginName)
                        .FirstOrDefault();
                    _logger.LogInformation($"Для проекта: {projectName} было найдено имя плагина: {pluginName}");
                    
                    if (string.IsNullOrEmpty(pluginName))
                    {
                        _discordBot.WriteMessageToChannel(mainChatId.Value,
                            $"Нет информации о плагине для проекта {projectName}");
                        return;
                    }

                    var channel = _serviceProvider.GetRequiredService<IDiscordBot>().GetChannelById(mainChatId.Value);

                    var messageSender = new MessageSender(channel);

                    _logger.LogInformation($"Вызов команды начала запуска плана {planName} для плагина {pluginName}");
                    _serviceProvider.GetRequiredService<CommandResolver>()
                        .InvokeBambooStartCommand(pluginName, messageSender, planName);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "При принятии запроса от Bamboo");
                }
            }
        }

        [HttpPost]
        [Route("end")]
        public void BuildEnd([FromBody] BambooBuildModel buildModel)
        {
            _logger.LogInformation($"Вызова команды endpoint'a build end c телом запроса: {JsonConvert.SerializeObject(buildModel)}");
            var projectName = buildModel.Build.BuildResultKey.Split("-")[0];
            var planName = buildModel.Build.BuildResultKey.Split("-")[1];
            _logger.LogInformation($"Имя проекта - {projectName}");
            Console.WriteLine($"Окончание сборки: {projectName}-{planName}");
            try
            {
                using var db = _serviceProvider.GetRequiredService<MainContext>();

                var mainChatId = db.UserInfos
                    .AsQueryable()
                    .Where(ui => ui.BambooProjectName == projectName)
                    .Select(ui => ui.MainChatId)
                    .FirstOrDefault();

                if (mainChatId == null)
                {
                    throw new ApplicationException($"Не удалось получить Id главного чата для проекта {projectName}");
                }
                
                var pluginName = db.UserInfos.AsQueryable()
                    .Where(ui => ui.BambooProjectName == projectName)
                    .Select(ui => ui.PluginName)
                    .FirstOrDefault();
                _logger.LogInformation($"Имя проекта - {projectName}");
                if (string.IsNullOrEmpty(pluginName))
                {
                    _discordBot.WriteMessageToChannel(mainChatId.Value,
                        $"Нет информации о плагине для проекта {projectName}");
                    return;
                }

                var channel = _discordBot.GetChannelById(mainChatId.Value);

                var messageSender = new MessageSender(channel);

                _serviceProvider.GetRequiredService<CommandResolver>()
                    .InvokeBambooEndCommand(pluginName, messageSender, buildModel);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "При принятии запроса от Bamboo");
            }
        }
    }
}