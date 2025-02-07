using HttpServerLibrary.Core;
using HttpServerLibrary.Attributes;
using HttpServerLibrary.HttpResponse;
using System.Text;

namespace MyHttpServer.Endponts
{
    internal class SparkEndpoint : BaseEndpoint
    {
        // spark GET
        [Get("spark")]
        public IHttpResponseResult Test()
        {
            Console.WriteLine("qwerty");

            return Html("<h1>HelloWorld</h1>");
        }

        // spark2 GET
        [Get("spark2")]
        public IHttpResponseResult Test2()
        {
            var user = new { Login = "123", Password = "123" };

            return Json(user);
        }
    }
}
