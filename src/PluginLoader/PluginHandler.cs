using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using BambooServices;
using Contract.Bamboo;
using Contract.Interfaces;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace PluginLoader
{
    public class PluginCommandHandler : IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly string _pluginName;
        private PluginLoadContext _pluginLoadContext;
        private readonly ILogger _logger;
        public Type BambooAnalyzerType { get; set; }

        private CommandService _commandService;

        internal PluginCommandHandler(IServiceProvider serviceProvider, string pluginName)
        {
            _serviceProvider = serviceProvider;
            _pluginName = pluginName;
            _pluginLoadContext = new PluginLoadContext(serviceProvider, pluginName);
            _logger = serviceProvider.GetRequiredService<ILogger<PluginCommandHandler>>();
            var config = new CommandServiceConfig
            {
                DefaultRunMode = RunMode.Sync,
                ThrowOnError = true,
                LogLevel = LogSeverity.Critical
            };
            _commandService = new CommandService(config);
            _commandService.Log += CommandServiceOnLog;
        }

        private Task CommandServiceOnLog(LogMessage arg)
        {
            _logger.Log(LogLevel.Critical, arg.ToString());
            return Task.CompletedTask;
        }

        public void PrepareHandler()
        {
            PrepareDll();
        }

        private void PrepareDll()
        {
            _logger.LogInformation($"Начинается подгрузка плагина {_pluginName}");
            _pluginLoadContext.LoadPlugin();
            _logger.LogInformation($"Подгрузка плагина плагина {_pluginName} прошла успешно");
            var assembly = _pluginLoadContext.GetAssembly();
            _logger.LogInformation($"Dll плагина {_pluginName} была успешно загружена");
            var types = assembly.DefinedTypes.Where(t => t.IsPublic).Where(t => t.IsSubclassOf(typeof(ModuleBase<ICommandContext>)));
            foreach (var typeInfo in types)
            {
                _commandService.AddModuleAsync(typeInfo, _serviceProvider.CreateScope().ServiceProvider);
            }
            _logger.LogInformation($"Классы плагина {_pluginName} успешно загружены");

            var commandsInfo = AllCommands.Select(c => new {c.Name, Group = c.Module.Group ?? c.Module.Name}).Select(c => $"{c.Group}{(!string.IsNullOrEmpty(c.Group) ? " " : "")}{c.Name}");
            _logger.LogInformation($"Для плагина {_pluginName} были добавлены следующие команды: \"{string.Join("\"; \"", commandsInfo)};\"");
            
            PrepareBambooAnalyzer(assembly);
        }

        private void PrepareBambooAnalyzer(Assembly assembly)
        {
            var bambooAnalyzerType = assembly.DefinedTypes.FirstOrDefault(t => t.IsSubclassOf(typeof(BambooAnalyzer)));
            if (bambooAnalyzerType == null)
            {
                _logger.LogInformation($"Для плагина {_pluginName} не определён объект типа {nameof(BambooAnalyzer)}");
                return;
            }

            BambooAnalyzerType = bambooAnalyzerType;
            _logger.LogInformation($"Для плагина {_pluginName} успешно определён тип {nameof(BambooAnalyzer)}");
        }

        public IEnumerable<CommandInfo> AllCommands => _commandService.Commands;

        public bool ContainsCommand(string command, string group = null)
        {
            return AllCommands.FirstOrDefault(c => c.Name == command && (group == null || c.Module.Group == group)) != null;
        }

        public Task InvokeCommand(ICommandContext context, int argPos)
        {
            Task.Run(() =>
            {
                var scopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();
                using (var scope = scopeFactory.CreateScope())
                {
                    var res = _commandService.ExecuteAsync(context, argPos, scope.ServiceProvider);
                    var executedInTime = res.Wait(TimeSpan.FromMinutes(5));
                    if (!executedInTime)
                    {
                        var message = $"Выполнении команды \"{context.Message.Content}\" для плагина {_pluginName} не было осуществленно за приемлемое время";
                        _logger.LogError(message);
                        context.Channel.SendMessageAsync(message);
                    }
                    if (!res.Result.IsSuccess)
                    {
                        var e = ((ExecuteResult) res.Result).Exception;
                        var message =
                            $"При выполнении команды \"{context.Message.Content}\" для плагина {_pluginName} произошла ошибка выполнения:\n{e?.Message}";
                        _logger.LogError(e, message +$"\nСтек вызоа:\n{e?.StackTrace}");
                        context.Channel.SendMessageAsync(message);
                    }
                }
            });
            return Task.CompletedTask;
        }

        public Task InvokeBambooStartCommand(IMessageSender messageSender, string planName)
        {
            Task.Run(() =>
            {
                if (BambooAnalyzerType == null)
                {
                    _logger.LogCritical($"У плагина {_pluginName} нет класса, который отвечает за обработку начала и окончания сборки планов bamboo");
                    return;
                }
                
                try
                {
                    var scopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();
                    using (var scope = scopeFactory.CreateScope())
                    {
                        var bambooAnalyzer =
                            Activator.CreateInstance(BambooAnalyzerType, scope.ServiceProvider) as BambooAnalyzer;
                        if (bambooAnalyzer == null)
                        {
                            var message =
                                $"Для плагина {_pluginName} не удалось создать объект типа {nameof(BambooAnalyzer)}";
                            _logger.LogError(message);
                            throw new ApplicationException(message);
                        }

                        _logger.LogInformation(
                            $"Непосредственный вызов команды начала сборки плана {planName} для плагина {_pluginName}");
                        var res = bambooAnalyzer.BuildStart(planName);
                        var executedInTime = res.Wait(TimeSpan.FromMinutes(2));
                        if (!executedInTime)
                        {
                            var message =
                                $"Выполнении команды \"{nameof(BambooAnalyzer)}.{nameof(BambooAnalyzer.BuildStart)}\" для плагина {_pluginName} не было осуществленно за приемлемое время";
                            _logger.LogError(message);
                            messageSender.SendMessageAsync(message);
                        }
                    }
                }
                catch (Exception e)
                {
                    var message =
                        $"При выполнении команды \"{nameof(BambooAnalyzer)}.{nameof(BambooAnalyzer.BuildStart)}\" для плагина {_pluginName} для плана {planName} произошла ошибка выполнения:\n{e.Message}";
                    _logger.LogError(e, message + $"\nСтек вызова:\n{e.StackTrace}");
                    messageSender.SendMessageAsync(message);
                }
            });
            return Task.CompletedTask;
        }
        
        public Task InvokeBambooEndCommand(IMessageSender messageSender, BambooBuildModel buildModel)
        {
            Task.Run(() =>
            {
                if (BambooAnalyzerType == null)
                {
                    _logger.LogCritical($"У плагина {_pluginName} нет класса, который отвечает за обработку начала и окончания сборки планов bamboo");
                    return;
                }
                
                try
                {
                    var scopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();
                    using (var scope = scopeFactory.CreateScope())
                    {
                        var bambooAnalyzer =
                            Activator.CreateInstance(BambooAnalyzerType, scope.ServiceProvider) as BambooAnalyzer;
                        if (bambooAnalyzer == null)
                        {
                            var message =
                                $"Для плагина {_pluginName} не удалось создать объект типа {nameof(BambooAnalyzer)}";
                            _logger.LogError(message);
                            throw new ApplicationException(message);
                        }

                        _logger.LogInformation(
                            $"Непосредственный вызов команды окончания сборки плана для плагина {_pluginName}\nАргументы вызова функции:{JsonConvert.SerializeObject(buildModel)}");
                        var res = bambooAnalyzer.BuildEnd(buildModel);
                        var executedInTime = res.Wait(TimeSpan.FromMinutes(2));
                        if (!executedInTime)
                        {
                            var message =
                                $"Выполнении команды \"{nameof(BambooAnalyzer)}.{nameof(BambooAnalyzer.BuildEnd)}\" для плагина {_pluginName} не было осуществленно за приемлемое время";
                            _logger.LogError(message);
                            messageSender.SendMessageAsync(message);
                            return;
                        }

                        _logger.LogInformation($"Вызов команды окончания сборки прошёл успешно");
                    }
                }
                catch (Exception e)
                {
                    var message =
                        $"При выполнении команды \"{nameof(BambooAnalyzer)}.{nameof(BambooAnalyzer.BuildEnd)}\" для плагина {_pluginName} для плана {buildModel.Build.BuildPlanName} произошла ошибка выполнения:\n{e.Message}";
                    _logger.LogError(e, message + $"\nСтек вызова:\n{e.StackTrace}");
                    messageSender.SendMessageAsync(message);
                }
            });
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _logger.LogInformation($"Начинается очистка {nameof(PluginCommandHandler)} для плагина {_pluginName}");
            var weak1 = new WeakReference(_pluginLoadContext);
            var weak2 = new WeakReference(_commandService);
            _pluginLoadContext.Dispose();
            foreach (var commandServiceModule in _commandService.Modules.ToList())
            {
                _commandService.RemoveModuleAsync(commandServiceModule);
            }
            _commandService = null;
            _pluginLoadContext = null;
            {
                for (int i = 0; i < 10 && (weak1.IsAlive || weak2.IsAlive); i++)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                }

                if (weak1.IsAlive || weak2.IsAlive)
                {
                    _logger.LogCritical($"Сборщику мусора не удалось очистить {nameof(PluginLoadContext)} для плагина {_pluginName}");
                    throw new ApplicationException(
                        $"Сборщику мусора не удалось очистить {nameof(PluginLoadContext)} для плагина {_pluginName}");
                }
            
                _logger.LogInformation($"Очистка {nameof(PluginCommandHandler)} для плагина {_pluginName} прошла успешно");
            }
        }
    }
}