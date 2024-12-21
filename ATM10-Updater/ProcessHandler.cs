using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace ATM10Updater
{
    internal class ProcessHandler(IOptions<ServerInfo> serverInfo) : IProcessHandler
    {
        private readonly string filePath = Path.Combine(Environment.GetEnvironmentVariable(serverInfo.Value.EnvironmentName, EnvironmentVariableTarget.User)!, serverInfo.Value.StartFile);
        
        public async Task RunProcess()
        {
            await Task.Run(() =>
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true // Allow the process to run independently
                };

                Process.Start(processStartInfo);
            });
        }

        public async Task StartWarmupProcess()
        {
            using var cts = new CancellationTokenSource();

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = filePath,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = false, // Setting false to run it in a separate window
                },
                EnableRaisingEvents = true
            };

            // Event handler for process exit
            process.Exited += (sender, e) =>
            {
                Console.WriteLine("Process exited.");
                cts.Cancel(); // Cancel tracking if the process exits.
            };

            try
            {
                // Start the process
                process.Start();

                // Start reading output asynchronously
                var outputTask = MonitorOutputAsync(process.StandardOutput, "eula.txt", cts.Token);
                var errorTask = MonitorOutputAsync(process.StandardError, "eula.txt", cts.Token);

                // Wait for tasks to complete or process to exit
                await Task.WhenAny(outputTask, errorTask);

                // Kill the process if not already exited
                if (!process.HasExited)
                {
                    Console.WriteLine("Target text found. Terminating the process...");
                    process.Kill();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                if (!process.HasExited)
                {
                    process.Kill();
                }
                process.Dispose();
            }
        }

        private static async Task MonitorOutputAsync(StreamReader reader, string terminateOnText, CancellationToken token)
        {
            try
            {
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
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
