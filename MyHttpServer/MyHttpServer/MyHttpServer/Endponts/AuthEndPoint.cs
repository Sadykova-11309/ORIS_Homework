using System.Data.SqlClient;
using System.Net;
using System.Web;
using HttpServerLibrary.Attributes;
using HttpServerLibrary.Core;
using HttpServerLibrary.HttpResponse;
using MyHttpServer.Models;
using MyHttpServer.Session;
using MyORMLibrary;

namespace MyHttpServer.Endponts
{
    public class AuthEndPoint : BaseEndpoint
    {

        [Get("login")]
        public IHttpResponseResult GetLogin()
        {
            var file = File.ReadAllText(@"Templates/Pages/Auth/login.html");
            if (IsAuthorized(Context)) return Redirect("dashboard");
            return Html(file);
        }

        [Post("login")]
        public IHttpResponseResult AuthPost(string email, string password)
        {
            string connectionString =
                @"Server=localhost; Database=testBD; User Id=sa; Password=P@ssw0rd;TrustServerCertificate=true;";
            var connection = new SqlConnection(connectionString);
            var dBcontext = new ORMContext<User>(connection);
            var user = dBcontext.FirstOrDefault(u => u.Email == email && u.Password == password);

            if (user == null)
            {
                return Redirect("login");
            }

            string token = Guid.NewGuid().ToString();
            Cookie cookie = new Cookie("session-token", token);
            Context.Response.SetCookie(cookie);
            SessionStorage.SaveSession(token, user.Id);

            return Redirect("dashboard");
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