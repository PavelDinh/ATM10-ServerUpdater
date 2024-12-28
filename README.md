# ATM10-ServerUpdater
Supports only Windows, .NET 8, Visual Studio 2022

* Simple Minecraft modpack ATM10 server updater.
* Automatically downloads latest version and setup ATM10 server to run.
* Support backup from previous versions.
* Support for Discord notifications when new server version was installed.

## Setup
Build the project on your server machine.
Create .bat in Startup folder to point to ATM10Updater.exe file.
Any time you restart server machine it will automatically run .bat file.

Startup folder
```
C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Startup
```

Example Startup.bat
```
$(SolutionRoot)\\ATM10-ServerUpdater\src\ATM10-Updater\bin\Release\net8.0\ATM10Updater.exe
```

## First time running
Build and run ATM10Updater.
Running first time will just install ATM10 latest server version and run it.

## Already have a server
If you already running ATM10 server on your machine, the app will check for latest release and downloads it. 
It will also copy world folder defined in configs to the new version as well.
You can set it up in appsettings.json.

appsettings.json
```json
"CurseForgeConfig": {
  "Endpoint": "https://api.curseforge.com/",
  "ApiKey": "<CurseForge API Key>"
},
"ModpackConfig": {
  "GameId": 432, // Minecraft
  "ModId": 925200 // ATM10 : ModId can be found on official website CurseForge.com website, under ProjectID
},
"ServerConfig": {
  "LocalServerFolder": "<Path to your server folder>",
  "NamingConvention": "ATM10-",
  "ServerFileEnv": "ATM10",
  "StartFile": "startserver.bat",
  "BackupFiles": [ "world", "server.properties", "user_jvm_args.txt", "eula.txt" ],
  "CustomDomain": "<Your server public IP address>"
},
"DiscordConfig": {
  "WebhookUrl": "<Discord Webhook URL>"
}
```

Keep in mind this app was intended for ATM10 server update purpose only, other modpacks may not work.
