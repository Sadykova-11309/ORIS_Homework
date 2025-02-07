namespace TemplateEngine.Core.Templaytor
{
    public class CustomTemplator : ICustomTemplator
    {
        public string GetHtmlByTemplate(string template, string name)
        {
            return template.Replace("{name}", name);
        }


        public string GetHtmlByTemplate<T>(string template, T obj)
        {
            var properties = obj.GetType().GetProperties();
            foreach (var property in properties)
            {
                var placeholder = $"{{{property.Name}}}";
                template = template.Replace(placeholder, property.GetValue(obj)?.ToString(), StringComparison.OrdinalIgnoreCase);
            }
            return template;
        }
    }
}
