using HttpServerLibrary.HttpResponse;

namespace HttpServerLibrary.Core
{
    public class BaseEndpoint
    {
        protected HttpRequestContext Context { get; private set; }

        internal void SetContext(HttpRequestContext context)
        {
            Context = context;
        }

        protected IHttpResponseResult Html(string responseText) => new HtmlResult(responseText);

        protected IHttpResponseResult Json(object data) => new JsonResult(data);

        protected IHttpResponseResult Redirect(string location) => new RedirectResult(location);
    }
}
