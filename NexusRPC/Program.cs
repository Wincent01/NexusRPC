using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscordRPC;
using DiscordRPC.Logging;

namespace NexusRPC
{
    internal static class Program
    {
        public const string ApplicationId = "679062437076402178";
        
        public static DiscordRpcClient Client { get; set; }
        
        public static string Details { get; set; }
        
        public static string State { get; set; }
        
        public static string LargeImage { get; set; }
        
        public static string LargeImageText { get; set; }
        
        public static string SmallImage { get; set; }
        
        public static string SmallImageText { get; set; }
        
        public static string SourceDirectory { get; set; }

        private static async Task Main(string[] args)
        {
            if (args.Length == default)
            {
                Console.WriteLine("Usage: ./NexusRPC <path/to/legouniverse.exe>");
                
                return;
            }
            
            var executable = args[0];

            var commands = $"WINEDLLOVERRIDES=\"dinput8.dll=n,b\" wine \"{executable}\"";
            
            SourceDirectory = Directory.GetCurrentDirectory();

            Directory.SetCurrentDirectory(Path.GetDirectoryName(executable));
            
            await SetupRpc();
            
            ClientWrapper.Execute(commands, HandleOutputAsync);
        }

        private static async Task SetupRpc()
        {
            Client = new DiscordRpcClient(ApplicationId)
            {
                Logger = new ConsoleLogger
                {
                    Level = LogLevel.Info,
                    Colored = true,
                    Coloured = true
                }
            };
            
            Client.OnReady += (sender, e) =>
            {
                Console.WriteLine($"Received Ready from user {e.User.Username}");
            };
            
            Client.OnPresenceUpdate += (sender, e) =>
            {
                Console.WriteLine($"Received Update! {e.Presence}");
            };
	
            Client.Initialize();

            Details = "Preparing for exploration";

            State = "Selecting character";

            LargeImage = "select";

            LargeImageText = "Character select";
            
            UpdateDiscord();
        }

        public static void UpdateDiscord()
        {
            Client.SetPresence(new RichPresence
            {
                Details = Details,
                State = State,
                Assets = new Assets
                {
                    LargeImageKey = LargeImage,
                    LargeImageText = LargeImageText,
                    SmallImageKey = SmallImage,
                    SmallImageText = SmallImageText
                }
            });	
        }

        private static async Task HandleOutputAsync(string output)
        {
            lock (Client)
            {
                Console.ForegroundColor = ConsoleColor.Green;

                if (output.StartsWith("2 :Received Load Zone, ID"))
                {
                    var parts = output.Split('(');

                    parts = parts.Last().Split(':');

                    var zoneId = int.Parse(parts.First());

                    var id = (ZoneId) zoneId;

                    if (id == ZoneId.VentureExplorerCinematic)
                        id = ZoneId.VentureExplorer;

                    var formatted = FormatZoneId(id);
                    
                    Details = $"Exploring {formatted}";

                    LargeImage = ((int) id).ToString();

                    LargeImageText = formatted;
                    
                    Console.WriteLine(Details);
                    
                    UpdateDiscord();
                }
                else if (output.StartsWith("2 :Received Create Character, ID"))
                {
                    var parts = output.Split(" ");

                    var character = parts.Last();
                    
                    State = $"Adventuring as {character}";

                    Console.WriteLine(State);
                    
                    UpdateDiscord();
                }

                Console.ResetColor();
            }
        }

        private static string FormatZoneId(ZoneId zoneId)
        {
            var str = zoneId.ToString();
            
            var builder = new StringBuilder();

            builder.Append(str.First());

            for (var index = 1; index < str.Length; index++)
            {
                var character = str[index];

                if (char.IsUpper(character))
                {
                    builder.Append(" ");
                }

                builder.Append(character);
            }

            return builder.ToString();
        }
    }
}