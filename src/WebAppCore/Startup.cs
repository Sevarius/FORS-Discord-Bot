using System;
using System.IO;
using BambooServices;
using Common;
using Contract.Interfaces;
using DiscordServices;
using JiraServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ORM;
using PluginLoader;
using Serilog;
using StendService;

namespace WebAppCore
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Console.WriteLine("Запуск сервера");
            var root = configuration["root"];

            var logFilePath = Path.Combine(root, "Log.db");
            Log.Logger = new LoggerConfiguration()
                .WriteTo.SQLite(logFilePath)
                .CreateLogger();
            
            var secretsPath = Path.Combine(root, "secrets.json");
            Configuration = new ConfigurationBuilder()
                .AddJsonFile(secretsPath)
                .AddConfiguration(configuration)
                .Build();
        }

        private IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            
            services.AddControllers();
            services.AddDbContext<MainContext>(options =>
            {
                options.UseSqlite($"Filename={Configuration["db_path"]}");
                options.LogTo(message => Log.Logger.Information(message), LogLevel.Information);
                options.EnableSensitiveDataLogging();
            });
            services.AddTransient(_ => Configuration);
            services.AddSingleton<CommandResolver>();
            services.AddTransient<IPluginGetter, PluginGetter>();
            services.AddSingleton<IDiscordBot, DiscordBot>();
            services.AddSingleton<InternalCommandService>();
            services.AddTransient<IBambooBuildPlanService, BambooBuildPlanService>();
            services.AddTransient<IJiraLabelsService, JiraLabelsService>();
            services.AddTransient<ICommonRepository, CommonRepository>();
            services.AddTransient<IBambooPlanRepository, BambooPlanRepository>();
            services.AddTransient<IStendVersionService, StendVersionService>();
            Console.WriteLine("Конфигурация сервисов успешно");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IServiceProvider serviceProvider, IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime applicationLifetime)
        {
            var bot = serviceProvider.GetRequiredService<IDiscordBot>();
            var commandResolver = serviceProvider.GetRequiredService<CommandResolver>();

            commandResolver.PrepareResolver();
            bot.StartBot();
            applicationLifetime.ApplicationStopped.Register(Log.CloseAndFlush);
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action}");
            });
            Console.WriteLine("Конфигурация прошла успешно");
        }
    }
}