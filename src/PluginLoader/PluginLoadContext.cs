using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace PluginLoader
{
    public class PluginLoadContext : AssemblyLoadContext, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        public readonly string PluginName;
        private AssemblyDependencyResolver _resolver;
        private string _dllPath;
        private readonly ILogger _logger;

        public PluginLoadContext(IServiceProvider serviceProvider, string pluginName) : base(isCollectible: true)
        {
            _serviceProvider = serviceProvider;
            PluginName = pluginName;
            _logger = serviceProvider.GetRequiredService<ILogger<PluginLoadContext>>();
        }

        public void LoadPlugin()
        {
            var config = _serviceProvider.GetRequiredService<IConfiguration>();
            var root = config["root"];
            var pathToPlugin = Path.Combine(root,"Plugins", PluginName);
            var dllName = Directory
                .GetFiles(pathToPlugin, "*.dll")
                .FirstOrDefault(d => Path.GetFileNameWithoutExtension(d).ToLower().Contains("plugin"));

            if (dllName == null)
            {
                _logger.LogError($"Не удалось найти dll с частью 'plugin' в названии в Plugins\\{PluginName}.");
                throw new ApplicationException(
                    $"Не удалось найти dll с частью 'plugin' в названии в Plugins\\{PluginName}.");
            }
            
            _logger.LogInformation($"Для плагина {PluginName} была найдена следующая главная dll: {dllName}");

            _dllPath = Path.Combine(pathToPlugin, dllName);
            
            _resolver = new AssemblyDependencyResolver(_dllPath);
        }

        public Assembly GetAssembly()
        {
            return LoadFromAssemblyPath(_dllPath);
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
            return assemblyPath != null ? LoadFromAssemblyPath(assemblyPath) : null;
        }

        public void Dispose()
        {
            _logger.LogInformation($"Идёт очистка {nameof(PluginLoadContext)} для плагина {PluginName}");
            _resolver = null;
            Unload();
            _logger.LogInformation($"Очистка {nameof(PluginLoadContext)} для плагина {PluginName} прошла успешно");
        }
    }
}