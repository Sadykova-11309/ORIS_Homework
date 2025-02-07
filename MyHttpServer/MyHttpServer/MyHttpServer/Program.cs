using HttpServerLibrary;
using HttpServerLibrary.Configuration;

namespace MyHttpServer
{
    internal class Program
    {static async Task Main(string[] args)
        {
            await AppConfig.Instance.LoadConfigAsync();
            var prefixes = new[] { $"http://{AppConfig.Instance.Domain}:{AppConfig.Instance.Port}/" };
            var server = new HttpServer(prefixes, AppConfig.Instance.StaticDirectoryPath);
            await server.StartAsync();

        }
       
        
    }
}
