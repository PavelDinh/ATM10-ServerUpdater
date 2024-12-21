using ATM10Updater;
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
        services.AddSingleton<IUpdateRunner, UpdateRunner>();
        services.AddSingleton<IServerInstallHandler, ServerInstallHandler>();
        services.AddSingleton<IProcessHandler, ProcessHandler>();
        services.AddSingleton<IBackupHandler, BackupHandler>();
        services.AddSingleton<IDiscordHandler, DiscordHandler>();
        
        var provider = services.BuildServiceProvider();
        var runner = provider.GetRequiredService<IUpdateRunner>();
        await runner.RunAsync();
    }
}
