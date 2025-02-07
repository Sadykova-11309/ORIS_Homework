using HttpServerLibrary.Core;
using HttpServerLibrary.Attributes;

namespace MyHttpServer.Endponts
{
    internal class EmailSenderEndpoint : BaseEndpoint
    {
        // lol GET
        [Get("lol")]
        public void LOLPage(string email, string password)
        {
            Console.WriteLine("УРА!!!");

            string responseText = "УРА Ты лол!!!";

            //GetResult(responseText);
        }

        // request GET
        [Get("request")]
        public void RequestPage()
        {
            string responseText = "request";

            //GetResult(responseText);
        }

        // homework GET
        [Get("homework")]
        public void HomeworkPage()
        {

        }

        // lol POST
        [Post("lol")]
        public void SendEmailToLOL(string email, string password)
        {

        }

        // request POST
        [Post("request")]
        public void SendEmailToRequest()
        {

        }

        // homework POST
        [Post("homework")]
        public void SendEmailToHomework()
        {

        }
    }
}
