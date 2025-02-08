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

var services = new ServiceCollection();
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("appsettings.dev.json", optional: true, reloadOnChange: true)
    .Build();

var logDirectory = Path.Combine(AppContext.BaseDirectory, "Logs");
if (!Directory.Exists(logDirectory))
{
    Directory.CreateDirectory(logDirectory);
}

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
    builder.AddSerilog(dispose: true);
});


services.Configure<CurseForgeConfig>(configuration.GetSection("CurseForgeConfig"));
services.Configure<ModpackConfig>(configuration.GetSection("ModpackConfig"));
services.Configure<ServerConfig>(configuration.GetSection("ServerConfig"));
services.Configure<DiscordConfig>(configuration.GetSection("DiscordConfig"));

services.AddHttpClient();

services.AddSingleton<ICurseForgeClient, CurseForgeClient>();

services.AddSingleton<IServerUpdateRunner, ServerUpdateRunner>();
services.AddSingleton<IServerInstaller, ServerInstaller>();
services.AddSingleton<IServerProcessHandler, ServerProcessHandler>();
services.AddSingleton<IServerBackupManager, ServerBackupManager>();
services.AddSingleton<IServerVersionProvider, ServerVersionProvider>();
services.AddSingleton<IServerMetadataProvider, ServerMetadataProvider>();
services.AddSingleton<IServerFileProvider, ServerFileProvider>();

services.AddSingleton<IDiscordHandler, DiscordHandler>();
services.AddSingleton<IDiscordWebhookClientWrapperFactory, DiscordWebhookClientWrapperFactory>();

services.AddSingleton<IFileDownloader, FileDownloader>();
services.AddSingleton<IFileExtractor, FileExtractor>();

services.AddSingleton<IArgsConfig, ArgsConfig>();

var provider = services.BuildServiceProvider();

provider.GetRequiredService<IServerProcessHandler>().EnsureProcessTerminated();

if (args.Length > 0)
{
    var argsConfig = provider.GetRequiredService<IArgsConfig>();
    await argsConfig.HandleArgsAsync(args);
}
else
{
    var runner = provider.GetRequiredService<IServerUpdateRunner>();
    await runner.RunAsync();
}
