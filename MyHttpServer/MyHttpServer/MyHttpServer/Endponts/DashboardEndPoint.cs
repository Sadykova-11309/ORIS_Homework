using HttpServerLibrary.Attributes;
using HttpServerLibrary.Core;
using HttpServerLibrary.HttpResponse;
using MyHttpServer.Session;


namespace MyHttpServer.Endponts
{
    internal class DashboardEndPoint : BaseEndpoint
    {

        [Get("dashboard")]
        public IHttpResponseResult GetPage()
        {
            if (!IsAuthorized(Context)) return Redirect("login");
            var file = File.ReadAllText(@"Templates/Pages/Dashboard/index.html");
            return Html(file);
        }

        public bool IsAuthorized(HttpRequestContext context)
        {
            // Проверка наличия Cookie с session-token
            if (context.Request.Cookies.Any(c => c.Name == "session-token"))
            {
                var cookie = context.Request.Cookies["session-token"];
                return SessionStorage.ValidateToken(cookie.Value);
            }

            return false;
        }
    }
}
