using ATM10Updater.Config;
using ATM10Updater.Providers;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace ATM10Updater.Handlers
{
    public class ServerProcessHandler(
        IOptions<ServerConfig> serverInfo,
        IServerFileProvider serverFileProvider) : IServerProcessHandler
    {
        private readonly ServerConfig _serverConfig = serverInfo.Value;
        private Process? _process;

        public void StartProcess()
        {
            var serverFiles = serverFileProvider.GetServerFilesSortedByVersion();
            AddPermissionToRunScript(_serverConfig.ServerRunFile, serverFiles.First());

            EnsureProcessTerminated();

            var processStartInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(serverFiles.First(), _serverConfig.ServerRunFile),
                WorkingDirectory = serverFiles.First(),
                UseShellExecute = false
            };

            _process = Process.Start(processStartInfo);
        }

        public async Task StartWarmupProcessAsync(CancellationToken token)
        {
            using var cts = new CancellationTokenSource();

            if (string.IsNullOrEmpty(_serverConfig.ServerStartupFile))
            {
                return;
            }
            
            var serverFiles = serverFileProvider.GetServerFilesSortedByVersion();
            AddPermissionToRunScript(_serverConfig.ServerStartupFile, serverFiles.First());

            EnsureProcessTerminated();

            _process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(serverFiles.First(), _serverConfig.ServerStartupFile),
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                },
                EnableRaisingEvents = true
            };

            // Event handler for process exit
            _process.Exited += (sender, e) =>
            {
                Console.WriteLine("Process exited.");
                cts.Cancel(); // Cancel tracking if the process exits.
            };

            try
            {
                // Start the process
                _process.Start();

                // Start reading output asynchronously
                var outputTask = MonitorOutputAsync(_process.StandardOutput, "eula.txt", cts.Token);
                var errorTask = MonitorOutputAsync(_process.StandardError, "eula.txt", cts.Token);

                // Wait for tasks to complete or process to exit
                await Task.WhenAny(outputTask, errorTask);

                // Kill the process if not already exited
                if (!_process.HasExited)
                {
                    Console.WriteLine("Target text found. Terminating the process...");
                    _process.Kill();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                if (!_process.HasExited)
                {
                    _process.Kill();
                }
                _process.Dispose();
            }
        }

        public void EnsureProcessTerminated()
        {
            var processTerminating = false;
            try
            {
                if (_process != null && !_process.HasExited)
                {
                    _process.Kill();
                    _process.WaitForExit();
                    _process.Dispose();
                    processTerminating = true;
                }

                if (!processTerminating)
                {
                    foreach (var process in Process.GetProcessesByName("java"))
                    {
                        process.Kill();
                        process.WaitForExit();
                        process.CloseMainWindow();
                    }
                }
            }
            catch (InvalidOperationException)
            {
                // process has been disposed.
            }
        }

        /// <summary>
        /// Use only in Linux environment
        /// </summary>
        /// <param name="scriptName"></param>
        private void AddPermissionToRunScript(string scriptName, string serverVersionPath)
        {
            if (scriptName.Contains(".sh"))
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"chmod +x /{_serverConfig.LocalServerFolder}/{serverVersionPath}/{scriptName}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var permissionProcess = new Process { StartInfo = psi };
                permissionProcess.Start();
                permissionProcess.WaitForExit();
            }
        }

        private static async Task MonitorOutputAsync(StreamReader reader, string terminateOnText, CancellationToken token)
        {
            try
            {
                string line;
                while ((line = await reader.ReadLineAsync(token)) != null)
                {
                    Console.WriteLine(line);

                    // Check if the target text is found
                    if (line.Contains(terminateOnText, StringComparison.OrdinalIgnoreCase))
                    {
                        break;
                    }

                    // Cancel tracking if requested
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Expected if cancellation is requested
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading output: {ex.Message}");
            }
        }
    }
}
