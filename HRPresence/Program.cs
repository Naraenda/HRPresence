using DiscordRPC;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Tomlyn;
using Tomlyn.Model;

namespace HRPresence
{
    class Config : ITomlMetadataProvider
    {
        public float TimeOutInterval { get; set; } = 4f;
        public float RestartDelay { get; set; } = 4f;
        public bool UseDiscordRPC { get; set; } = true;
        public string DiscordRPCId { get; set; } = "385821357151223818";
        public bool UseOSC { get; set; } = true;
        public int OSCPort { get; set; } = 9000;
        public TomlPropertiesMetadata PropertiesMetadata { get; set; }
    }

    class Program
    {
        static DiscordRpcClient discord;
        static HeartRateService heartrate;
        static HeartRateReading reading;
        static OscService       osc;

        static DateTime lastUpdate = DateTime.MinValue;

        static void Main() {
            var config = new Config();
            if (File.Exists("config.toml")) {
                config = Toml.ToModel<Config>(File.OpenText("config.toml").ReadToEnd());
            } else {
                File.WriteAllText("config.toml", Toml.FromModel(config));
            }

            Console.CursorVisible = false;
            Console.WindowHeight = 4;
            Console.WindowWidth = 32;

            if (config.UseDiscordRPC) {
                discord = new DiscordRpcClient(config.DiscordRPCId);
                discord.Initialize();
                Console.WriteLine($"> Discord RPC [on]");
            }

            if (config.UseOSC) {
                osc = new OscService();
                osc.Initialize(System.Net.IPAddress.Loopback, config.OSCPort);
                Console.WriteLine($"> OSC [on]");
            }

            heartrate = new HeartRateService();
            heartrate.HeartRateUpdated += heart => {
                reading = heart;

                Console.Write($"{DateTime.Now}  \n{reading.BeatsPerMinute} BPM   ");
                Console.SetCursorPosition(0, 0);

                lastUpdate = DateTime.Now;
                File.WriteAllText("rate.txt", $"{reading.BeatsPerMinute}");

                osc?.Update(reading.BeatsPerMinute);
            };

            Console.WriteLine($"> awaiting heart beat");
            Console.SetCursorPosition(0, 0);

            while (true) {

                if (DateTime.Now - lastUpdate > TimeSpan.FromSeconds(config.TimeOutInterval)) {
                    Debug.WriteLine("Hearrate monitor uninitialized. Starting...");
                    while(true) {
                        try {
                            heartrate.InitiateDefault();
                            break;
                        } catch (Exception e) {
                            Debug.WriteLine($"Failure while initiating heartrate service, retrying in {config.RestartDelay} seconds:");
                            Debug.WriteLine(e);
                            Thread.Sleep((int)(config.RestartDelay * 1000));
                        }
                    }
                }

                discord?.SetPresence(new RichPresence() {
                    Details = $"Heart Rate",
                    State = $"{reading.BeatsPerMinute} BPM",
                });

                Thread.Sleep(2000);
            }
        }
    }
}
