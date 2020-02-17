using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace NexusRPC
{
    public static class ClientWrapper
    {
        public static void Execute(string cmd, Func<string, Task> callback)
        {            
            var escapedArgs = cmd.Replace("\"", "\\\"");
            
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            
            process.Start();

            while (!process.HasExited)
            {
                var output = process.StandardOutput.ReadLine();
                
                if (output == default) continue;

                Task.Run(async () => await callback(output));
            }
        }
    }
}