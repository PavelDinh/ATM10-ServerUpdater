using ATM10Updater;
using ATM10Updater.Config;
using ATM10Updater.Handlers;
using ATM10Updater.Managers;
using ATM10Updater.Providers;
using CurseForgeAPI;
using DiscordWebhookMessanger;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

internal class Program
{
    static async Task Main(string[] args)
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());

        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.dev.json", optional: true, reloadOnChange: true)
            .Build();

        services.Configure<CurseForgeConfig>(configuration.GetSection("CurseForge"));
        services.Configure<ModpackInfo>(configuration.GetSection("ModpackInfo"));
        services.Configure<ServerInfo>(configuration.GetSection("ServerInfo"));
        services.Configure<DiscordInfo>(configuration.GetSection("DiscordInfo"));

        services.AddHttpClient();

        services.AddSingleton<ICurseForgeClient, CurseForgeClient>();
        services.AddSingleton<IServerUpdateRunner, ServerUpdateRunner>();
        services.AddSingleton<IServerInstaller, ServerInstaller>();
        services.AddSingleton<IServerProcessStartup, ServerProcessStartup>();
        services.AddSingleton<IServerBackupManager, ServerBackupManager>();
        services.AddSingleton<IDiscordHandler, DiscordHandler>();
        services.AddSingleton<IFileDownloader, FileDownloader>();
        services.AddSingleton<IServerVersionProvider, ServerVersionProvider>();
        services.AddSingleton<IServerMetadataProvider, ServerMetadataProvider>();
        services.AddSingleton<IFileExtractor, FileExtractor>();

        var provider = services.BuildServiceProvider();
        var runner = provider.GetRequiredService<IServerUpdateRunner>();
        await runner.RunAsync();
    }
}
