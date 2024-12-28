using ATM10Updater;
using ATM10Updater.Config;
using ATM10Updater.Handlers;
using ATM10Updater.Managers;
using ATM10Updater.Providers;
using CurseForgeAPI;
using CurseForgeAPI.Config;
using DiscordAPI;
using DiscordAPI.Config;
using DiscordAPI.Wrappers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

internal class Program
{
    static async Task Main(string[] args)
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.dev.json", optional: true, reloadOnChange: true)
            .Build();

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(
                path: Path.Combine(AppContext.BaseDirectory, "Logs", "Log.log"),
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                shared: true,
                rollingInterval: RollingInterval.Infinite
            )
            .CreateLogger();

        services.AddLogging(builder =>
        {
            builder.AddConfiguration(configuration.GetSection("Logging"));
            builder.AddConsole();
            builder.AddSerilog(dispose:true);
        });


        services.Configure<CurseForgeConfig>(configuration.GetSection("CurseForgeConfig"));
        services.Configure<ModpackConfig>(configuration.GetSection("ModpackConfig"));
        services.Configure<ServerConfig>(configuration.GetSection("ServerConfig"));
        services.Configure<DiscordConfig>(configuration.GetSection("DiscordConfig"));

        services.AddHttpClient();

        services.AddSingleton<ICurseForgeClient, CurseForgeClient>();

        services.AddSingleton<IServerUpdateRunner, ServerUpdateRunner>();
        services.AddSingleton<IServerInstaller, ServerInstaller>();
        services.AddSingleton<IServerProcessStartup, ServerProcessStartup>();
        services.AddSingleton<IServerBackupManager, ServerBackupManager>();
        services.AddSingleton<IServerVersionProvider, ServerVersionProvider>();
        services.AddSingleton<IServerMetadataProvider, ServerMetadataProvider>();

        services.AddSingleton<IDiscordHandler, DiscordHandler>();
        services.AddSingleton<IDiscordWebhookClientWrapperFactory, DiscordWebhookClientWrapperFactory>();
        
        services.AddSingleton<IFileDownloader, FileDownloader>();
        services.AddSingleton<IFileExtractor, FileExtractor>();

        var provider = services.BuildServiceProvider();
        var runner = provider.GetRequiredService<IServerUpdateRunner>();
        await runner.RunAsync();
    }
}
