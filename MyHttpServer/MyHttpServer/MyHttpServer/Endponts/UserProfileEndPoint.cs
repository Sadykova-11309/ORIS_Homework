using HttpServerLibrary.Attributes;
using HttpServerLibrary.Core;
using HttpServerLibrary.HttpResponse;
using MyHttpServer.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHttpServer.Endponts
{
    public class UserProfileEndpoint : BaseEndpoint
    {
        [Get("userpage")]
        public IHttpResponseResult GetPage()
        {
            if (!IsAuthorized(Context)) return Redirect("login");
            var file = File.ReadAllText(@"Templates/Pages/UserProfile/profile.html");
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
