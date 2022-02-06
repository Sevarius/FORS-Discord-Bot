using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contract.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace PluginLoader
{
    public class InternalCommandService
    {
        private readonly IServiceProvider _serviceProvider;
        private Dictionary<List<string>, Func<IMessageSender, string, Task>> _commands;
        private readonly ILogger<InternalCommandService> _logger;

        public InternalCommandService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = serviceProvider.GetRequiredService<ILogger<InternalCommandService>>();
            _commands = new Dictionary<List<string>, Func<IMessageSender, string, Task>>
            {
                [new List<string> {"help", "h"}] = HelpCommand
            };
        }

        public bool ContainsCommand(string command)
        {
            foreach (var keys in _commands.Keys)
            {
                if (keys.Contains(command))
                {
                    return true;
                }
            }

            return false;
        }

        public Task InvokeCommand(IMessageSender sender, string command, string pluginName)
        {
            Task.Run(() =>
            {
                _logger.LogInformation($"Вызов команды {command} для плагина {pluginName}");
                foreach (var keys in _commands.Keys)
                {
                    if (keys.Contains(command))
                    {
                        try
                        {
                            _commands[keys].Invoke(sender, pluginName);
                        }
                        catch (Exception ex)
                        {
                            var message =
                                $"При выполнении команды \"{command}\" для плагина {pluginName} произошла ошибка выполнения:\n{ex.Message}";
                            _logger.LogError(message + $"\nСтек вызова:\n{ex.StackTrace}", ex);
                            sender.SendMessageAsync(message);
                        }

                        return;
                    }
                }
            });

            return Task.CompletedTask;
        }

        private async Task HelpCommand(IMessageSender sender, string pluginName)
        {
            _logger.LogInformation($"Вызов команды help для плагина {pluginName}");

            var sb = new StringBuilder();
            var internalCommands = WriteInternalCommands();
            sb.Append("Для данного сервера заданы следующие команды:\n");
            sb.Append(string.Join("\n", internalCommands));
            sb.Append('\n');
            var commandResolver = _serviceProvider.GetRequiredService<CommandResolver>();
            var pluginHandler = commandResolver[pluginName];
            AddPluginCommandsInfo(pluginHandler, sb, true);
            var result = sb.ToString();
            await sender.SendMessageAsync(result); 
        }

        private List<string> WriteInternalCommands()
        {
            var commands = new List<string>
            {
                "**help**, **h** - Вывод списка комманд"
            };
            return commands;
        }
        
        private void AddPluginCommandsInfo(PluginCommandHandler pluginHandler, StringBuilder builder, bool addArgumentInfo)
        {
            foreach (var commandInfo in pluginHandler.AllCommands)
            {
                var command = string.IsNullOrEmpty(commandInfo.Module.Group)
                    ? commandInfo.Name
                    : $"{commandInfo.Module.Group} {commandInfo.Name}";
                builder.Append($"**{command}**");
                if (!string.IsNullOrEmpty(commandInfo.Summary))
                {
                    builder.Append($" - {commandInfo.Summary}");
                }

                builder.Append('\n');

                if (commandInfo.Parameters.Any() && addArgumentInfo)
                {
                    foreach (var parameter in commandInfo.Parameters)
                    {
                        builder.Append($"    {parameter.Name}");
                        if (!string.IsNullOrEmpty(parameter.Summary))
                        {
                            builder.Append($" - {parameter.Summary}");
                        }

                        builder.Append('\n');
                    }
                }
            }
        }
    }
}