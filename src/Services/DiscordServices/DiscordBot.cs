using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Contract.Interfaces;
using Discord;
using Discord.Commands;
using Discord.Net.WebSockets;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ORM;
using PluginLoader;

namespace DiscordServices
{
    public class DiscordBot : IDiscordBot, IDisposable
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly InternalCommandService _internalCommandService;
        private DiscordSocketClient _client;
        private string _botToken;
        private readonly ILogger<DiscordBot> _logger;
        private readonly IConfiguration _configuration;
        private readonly CommandResolver _commandResolver;

        public DiscordBot(IServiceScopeFactory scopeFactory, InternalCommandService internalCommandService, ILogger<DiscordBot> logger, IConfiguration configuration, CommandResolver commandResolver)
        {
            _scopeFactory = scopeFactory;
            _internalCommandService = internalCommandService;
            _logger = logger;
            _configuration = configuration;
            _commandResolver = commandResolver;
        }

        public void StartBot()
        {
            Console.WriteLine("Запуск discord бота");
            using var scope = _scopeFactory.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<MainContext>();
            _botToken = _configuration["Discord:BotToken"];
            
            var config = new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Warning
            };
            
            var proxyUrl = _configuration["proxy"];
            if (proxyUrl != "none" && Uri.TryCreate(proxyUrl, UriKind.Absolute, out var uri))
            {
                config.WebSocketProvider = DefaultWebSocketProvider.Create(new WebProxy(uri));
            }
            
            _client = new DiscordSocketClient(config);
            
            _client.Log += Log;
            _client.MessageReceived += ClientOnMessageReceived;
            _client.LoginAsync(TokenType.Bot, _botToken);
            _client.StartAsync();

            for (int i = 0; i < 10 && _client.ConnectionState != ConnectionState.Connected; i++)
            {
                Console.WriteLine(_client.ConnectionState);
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
            Console.WriteLine(_client.ConnectionState);

            if (_client.ConnectionState != ConnectionState.Connected)
            {
                Dispose();
                Console.WriteLine("Не удалось запустить бота");
                throw new ApplicationException("Не удалось запустить бота");
            }

            Console.WriteLine("Бот успешно запущен");
        }

        private Task Log(LogMessage msg)
        {
            _logger.LogInformation(msg.ToString());
            return Task.CompletedTask;
        }

        private async Task ClientOnMessageReceived(SocketMessage arg)
        {
            if (arg is not SocketUserMessage message) return;
            var argPos = 0;
            if (!message.HasCharPrefix('!', ref argPos) || message.Author.IsBot || message.Content.Length == 1)
            {
                return;
            }
            
            var context = new SocketCommandContext(_client, message);
            
            try
            {
                _logger.LogInformation($"Вызов команды \"{message.Content}\"");
            
                var channel = (IGuildChannel) arg.Channel;
                var serverId = channel.GuildId;
                using var scope = _scopeFactory.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<MainContext>();
                var pluginName = db.UserInfos.AsQueryable().Where(ui => ui.ServerId == serverId)
                    .Select(ui => ui.PluginName).FirstOrDefault();
                
                if (pluginName == null)
                {
                    _logger.LogWarning($"Для сервера {channel.Name} не указано имя плагина в базе данных");
                    await message.ReplyAsync(
                        $"Для данного {channel.Name} не установлена связь с плагином. Проверьте наличия имени плагина для вашего сервера в БД");
                }

                Console.WriteLine($"Вызвана команда: {pluginName} - {message.Content}");
                
                var command = message.Content[1..];
                if (_internalCommandService.ContainsCommand(command))
                {
                    var sender = new MessageSender(arg.Channel);
                    await _internalCommandService.InvokeCommand(sender, command, pluginName);
                    return;
                }
                
                await _commandResolver.InvokeCommand(pluginName, context, argPos);
            }
            catch (Exception e)
            {
                var errorMessage =
                    $"При выполнении команды \"{context.Message.Content}\" произошла ошибка выполнения:\n{e.Message}";
                _logger.LogError(e, errorMessage + $"\nСтек вызова:\n{e.StackTrace}");
                await context.Channel.SendMessageAsync(errorMessage);
            }
        }

        public IMessageChannel GetChannelById(ulong channelId)
        {
            IMessageChannel channel = null;
            _logger.LogInformation($"Запрос на подключение чата c id {channelId}");
            for (int i = 0; i < 10 && channel == null; i++)
            {
                channel = _client.GetChannel(channelId) as IMessageChannel;
                Thread.Sleep(TimeSpan.FromSeconds(0.2));
            }

            if (channel == null)
            {
                _logger.LogError($"Не удалось получить чат с id : {channelId}");
                throw new ApplicationException($"Не удалось получить чат с id : {channelId}");
            }
            _logger.LogInformation($"чат c id {channelId} успешно получен");
            return channel;
        }

        public async Task WriteMessageToChannel(ulong channelId, string message)
        {
            IMessageChannel channel = GetChannelById(channelId);

            await channel.SendMessageAsync(message);
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}