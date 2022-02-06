using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Contract.Bamboo;
using Contract.Interfaces;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace PluginLoader
{
    public class CommandResolver
    {
        private readonly IServiceProvider _serviceProvider;

        private readonly Dictionary<string, PluginCommandHandler> _commandHandlers;

        private readonly ILogger _logger;
        
        public CommandResolver(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _commandHandlers = new Dictionary<string, PluginCommandHandler>();
            _logger = _serviceProvider.GetRequiredService<ILogger<CommandResolver>>();
            _logger.LogInformation("Создание объекта CommandResolver");
        }

        public void PrepareResolver()
        {
            _logger.LogInformation("Подготовка объекта CommandResolver");
            
            var config = _serviceProvider.GetRequiredService<IConfiguration>();
            var root = config["root"];
            var allPluginsPath = Path.Combine(root, "Plugins");
            var allDirectories = Directory.GetDirectories(allPluginsPath).Select(d => new DirectoryInfo(d).Name).ToList();

            _logger.LogInformation($"Были найдены следующие плагины:\n{string.Join("\n", allDirectories)}");
            var factory = new PluginFactory(_serviceProvider);
            foreach (var directory in allDirectories)
            {
                try
                {
                    var pluginHandler = factory.CreateInstance(directory);
                    _logger.LogInformation($"Плагин {directory} успешно загружен в систему");
                    _commandHandlers.Add(directory, pluginHandler);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Плагин {directory} не был загружен в систему");
                }
            }
        }

        public PluginCommandHandler this[string pluginName] => _commandHandlers[pluginName];

        public async Task InvokeCommand(string pluginName, ICommandContext context, int argPos)
        {
            AssertPluginExist(pluginName);

            await _commandHandlers[pluginName].InvokeCommand(context, argPos);
        }

        public async Task InvokeBambooStartCommand(string pluginName, IMessageSender sender, string planName)
        {
            AssertPluginExist(pluginName);

            await _commandHandlers[pluginName].InvokeBambooStartCommand(sender, planName);
        }
        
        public async Task InvokeBambooEndCommand(string pluginName, IMessageSender sender, BambooBuildModel buildModel)
        {
            AssertPluginExist(pluginName);

            await _commandHandlers[pluginName].InvokeBambooEndCommand(sender, buildModel);
        }

        #region Перезагрузка плагинов (не работает)
        private void UpdatePlugin(ICommandContext commandContext, string pluginName)
        {
            AssertThatNewPluginExist(pluginName);
            StopPlugin(pluginName);
            CopyNewFiles(pluginName);
            StartPlugin(pluginName);
            commandContext.Message.ReplyAsync($"Плагин {pluginName} успешно обновлён");
        }

        private void StopPlugin(string pluginName)
        {
            if (_commandHandlers.ContainsKey(pluginName))
            {
                _commandHandlers.Remove(pluginName, out var commandHandler);
                commandHandler?.Dispose();
            }
        }

        private void AssertThatNewPluginExist(string pluginName)
        {
            var config = _serviceProvider.GetRequiredService<IConfiguration>();
            var root = config["root"];
            var pluginsToUpdatePath = Path.Combine(root, "PluginsToUpdate", pluginName);
            if (!Directory.Exists(pluginsToUpdatePath))
            {
                throw new ApplicationException(
                    $"Не удалось найти плагин {pluginName} для обновления по пути {pluginsToUpdatePath}");
            }
        }

        private void CopyNewFiles(string pluginName)
        {
            AssertThatNewPluginExist(pluginName);
            var config = _serviceProvider.GetRequiredService<IConfiguration>();
            var root = config["root"];
            var pluginsToUpdatePath = Path.Combine(root, "PluginsToUpdate", pluginName);
            var pluginPath = Path.Combine(root, "Plugins", pluginName);
            foreach (var file in Directory.GetFiles(pluginsToUpdatePath, "*.dll", SearchOption.TopDirectoryOnly))
            {
                File.Copy(file, file.Replace(pluginsToUpdatePath, pluginPath), true);
            }
        }

        private void StartPlugin(string pluginName)
        {
            var config = _serviceProvider.GetRequiredService<IConfiguration>();
            var root = config["root"];
            var pluginPath = Path.Combine(root, "Plugins", pluginName);

            if (!Directory.Exists(pluginPath))
            {
                throw new ApplicationException($"Не удалось найти плагин {pluginName}");
            }
            
            var factory = new PluginFactory(_serviceProvider);
            var plugin = factory.CreateInstance(pluginName);
            _commandHandlers.Add(pluginName, plugin);
        }
        

        #endregion

        private void AssertPluginExist(string pluginName)
        {
            if (!_commandHandlers.ContainsKey(pluginName))
            {
                var message = $"Система не содержит плагин с именем : {pluginName}";
                _logger.LogError(message);
                throw new ApplicationException(message);
            }
        }
    }
}