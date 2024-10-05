using System.Reflection;
using System.Runtime.InteropServices;
using CounterStrikeSharp.API.Core.Commands;
using CounterStrikeSharp.API.Core.Hosting;
using CounterStrikeSharp.API.Core.Interfaces;
using CounterStrikeSharp.API.Core.Logging;
using CounterStrikeSharp.API.Core.Plugin.Host;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Admin;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace CounterStrikeSharp.API
{
    /// <summary>
    /// Provides the entry point for bootstrapping and initializing the application.
    /// </summary>
    public static class Bootstrap
    {
        /// <summary>
        /// Entry point for the .NET Host in C++ to initiate loading.
        /// </summary>
        /// <returns>1 if the bootstrap process was successful, 0 otherwise.</returns>
        [UnmanagedCallersOnly]
        public static int Run()
        {
            try
            {
                var services = new ServiceCollection();
                services.ConfigureServices();
                using var serviceProvider = services.BuildServiceProvider();

                var application = serviceProvider.GetRequiredService<Application>();
                application.Start();

                return 1;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                Log.Fatal(ex, "Failed to start application");
                return 0;
            }
        }

        private static void ConfigureServices(this IServiceCollection services)
        {
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddCoreLogging(GetContentRoot());
            });

            // Service registrations
            services.AddSingleton<IScriptHostConfiguration, ScriptHostConfiguration>();
            services.AddTransient<Application>();
            services.AddSingleton<IPluginManager, PluginManager>();
            services.AddSingleton<IPlayerLanguageManager, PlayerLanguageManager>();
            services.AddTransient<IPluginContextQueryHandler, PluginContextQueryHandler>();
            services.AddSingleton<ICommandManager, CommandManager>();
            services.AddSingleton<IGameData, GameData>();
            services.AddSingleton<IAdminManager, AdminManager>();

            services.Scan(scan => scan
                .FromCallingAssembly()
                .AddClasses(classes => classes.AssignableTo<IStartupService>())
                .AsSelfWithInterfaces()
                .WithSingletonLifetime());
        }

        private static string GetContentRoot() => new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.Parent.FullName;
    }
}
