using HttpServerLibrary.Attributes;
using HttpServerLibrary.Core;
using HttpServerLibrary.HttpResponse;
using MyHttpServer.Models;
using System.Data.SqlClient;

namespace MyHttpServer.Endponts
{
    internal class UserEndPoint : BaseEndpoint
    {
        [Get("users")]
        public IHttpResponseResult GetUsers()
        {
            var users = new List<User>();
            string connectionString = @"Data Source=localhost;Initial Catalog=test1;User ID=sa;Password=P@ssw0rd; TrustServerCertificate=true;";
            //var context = new OMLCOntext()
            string sqlExpression = "SELECT * FROM Users";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows) // если есть данные
                {
                    while (reader.Read()) // построчно считываем данные
                    {
                        var user  = new User()
                        {
                            Id = reader.GetInt32(0),
                            Email = (string)reader.GetString(1),
                            Password = (string)reader.GetString(2)
                        };
                        object id = reader.GetValue(0);
                        object name = reader.GetValue(1);
                        object age = reader.GetValue(2);

                    }

                    users.Add(new User());
                }

                reader.Close();
            }

            return Json(users);
        }
    }
}
