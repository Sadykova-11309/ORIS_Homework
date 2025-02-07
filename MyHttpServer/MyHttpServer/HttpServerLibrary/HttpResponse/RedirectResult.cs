using HttpServerLibrary.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HttpServerLibrary.HttpResponse
{
    internal class RedirectResult : IHttpResponseResult
    {
        private readonly string _location;
        public RedirectResult(string location)
        {
            _location = location;
        }

        public void Execute(HttpListenerResponse response)
        {
            response.StatusCode = 302;
            response.Headers.Add("Location", _location); // Заголовок для указания пути
        }

    }
}
