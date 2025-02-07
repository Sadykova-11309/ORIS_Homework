using System.Text.Json;
using System.Text.Json.Serialization;

namespace HttpServerLibrary.Configuration
{
    public sealed class AppConfig
    {
        private static readonly Lazy<AppConfig> _instance = new(() => new AppConfig());
        public static AppConfig Instance => _instance.Value;

        [JsonInclude]
        public string Domain { get; set; } = "localhost";
        [JsonInclude]
        public uint Port { get; set; } = 6529;
        [JsonInclude]
        public string StaticDirectoryPath { get; set; } = @"public";

        private AppConfig() { }

        [JsonConstructor]
        public AppConfig(string domain, uint port, string staticDirectoryPath)
        {
            Domain = domain;
            Port = port;
            StaticDirectoryPath = staticDirectoryPath;
        }

        public async Task LoadConfigAsync()
        {
            if (File.Exists("config.json"))
            {
                try
                {
                    var fileConfig = await File.ReadAllTextAsync("config.json");
                    var config = JsonSerializer.Deserialize<AppConfig>(fileConfig);
                    if (config != null)
                    {
                        Domain = config.Domain;
                        Port = config.Port;
                        StaticDirectoryPath = config.StaticDirectoryPath;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка загрузки конфигурации: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Файл конфигурации 'config.json' не найден. Используются настройки по умолчанию.");
            }
        }
    }
}
