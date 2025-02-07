namespace TemplateEngine.Core.Templaytor
{
    public interface ICustomTemplator
    {
        string GetHtmlByTemplate(string template, string name);

        string GetHtmlByTemplate<T>(string template, T obj);
    }
}
