using System;

namespace PluginLoader
{
    public class PluginFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public PluginFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public PluginCommandHandler CreateInstance(string pluginName)
        {
            var instance = new PluginCommandHandler(_serviceProvider, pluginName);
            instance.PrepareHandler();
            return instance;
        }
    }
}