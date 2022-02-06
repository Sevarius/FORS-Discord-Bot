using System;
using System.Threading.Tasks;
using Contract.Interfaces;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using TosPlugin.Dto;

namespace TosPlugin
{
    public class TosHttpService : ModuleBase<ICommandContext>
    {
        private readonly ILogger<TosHttpService> _logger;
        private readonly IStendVersionService _sendVersionService;

        public TosHttpService(IServiceProvider serviceProvider)
        {
            _logger = serviceProvider.GetRequiredService<ILogger<TosHttpService>>();
            _sendVersionService = serviceProvider.GetRequiredService<IStendVersionService>();
        }

        [Command("version")]
        [Summary("Получить версию компонент на стенде")]
        public async Task GetStendVersion([Summary("Имя стенда: dev, adptest, sec, koetest, kpetest")]string stendName)
        {
            string json;
            try
            {
                json = _sendVersionService.GetVersionFromStend(stendName.ToLowerInvariant());
            }
            catch (Exception e)
            {
                var message = $"Возникла ошибка при отправке запроса на сервер стенда:\n{e.Message}";
                _logger.LogError(e, message);
                await ReplyAsync(message);
                return;
            }
            var jObject = JObject.Parse(json);
            if (jObject["is_error"].Value<bool>())
            {
                await ReplyAsync($"Возникла ошибка при получении информации со стенда **{stendName}**:\n{jObject["Error"]}");
                return;
            }

            var versions = jObject["result"].ToObject<FullVersionDto>();

            var result =
                $"Версия компонент на стенде **{stendName}**\nВерсия КПИ: {versions.KpiVersion}\nВерсия Фронт: {versions.FrontVersion}\nВерсия АПИ: {versions.ApiVersion}\nДата формирования БД: {versions.DbModelDate}";
            await ReplyAsync(result);
        }
    }
}