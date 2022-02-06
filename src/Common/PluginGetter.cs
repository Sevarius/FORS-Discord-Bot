using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Logging;

namespace Common
{
    public class PluginGetter : IPluginGetter
    {
        private readonly ILogger<PluginGetter> _logger;

        public PluginGetter(ILogger<PluginGetter> logger)
        {
            _logger = logger;
        }

        public string GetExecutingPluginName()
        {
            using (_logger.BeginScope("Определение плагина вызывающей функции"))
            {
                _logger.LogInformation("Начало определения имени плагина вызывающей функции");
                var st = new StackTrace();
                for (int i = 1; i < st.FrameCount; i++)
                {
                    var assembly = st.GetFrame(i)?.GetMethod()?.DeclaringType?.Assembly;
                    if (assembly == null)
                    {
                        continue;
                    }

                    var assemblyName = assembly.GetName().Name;
                    if (assemblyName == null || !assemblyName.ToLowerInvariant().EndsWith("plugin"))
                    {
                        continue;
                    }

                    var assemblyLocation = assembly.Location;
                    var dllName = Path.GetFileName(assemblyLocation);
                    var pluginName = new DirectoryInfo(Path.GetDirectoryName(assemblyLocation) ?? throw new InvalidOperationException()).Name;
                    _logger.LogInformation($"Путь к DLL: {assemblyLocation}; Имя Dll: {dllName}; Имя плагина: {pluginName};");
                    return pluginName;
                }

                throw new ApplicationException("Не удалось определить Плагин, откуда был вызван данный метод");
            }
        }
    }
}