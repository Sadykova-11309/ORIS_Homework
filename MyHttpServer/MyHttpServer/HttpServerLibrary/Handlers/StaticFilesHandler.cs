using HttpServerLibrary.Core;
using HttpServerLibrary.Configuration;
using System.Net;

namespace HttpServerLibrary.Handlers
{
    internal class StaticFilesHandler : Handler
    {
        private readonly string _staticDirectoryPath = $"{Directory.GetCurrentDirectory()}\\{AppConfig.Instance}";

        /// <summary>
        /// Метод обрабатывает входящий http-запрос
        /// Определяет является ли запрос GET и запрашивает файл
        /// Если файл найден он возвращает клиенту. Иначе отправляет страницу 404
        /// </summary>
        /// <param name="context"></param>
        public override void HandleRequest(HttpRequestContext context)
        {
            bool IsGet = context.Request.HttpMethod.Equals("Get", StringComparison.OrdinalIgnoreCase);
            string[] arr = context.Request.Url?.AbsolutePath.Split('/');
            //bool IsFile = arr.Length == 2;
            var link = arr[arr.Length - 1];

            bool IsFile = link.Contains('.');

            if (IsGet && IsFile)
            {
                // Получить файл
                string relativePath = context.Request.Url?.AbsolutePath.TrimStart('/');
                string filePath = Path.Combine(_staticDirectoryPath, string.IsNullOrEmpty(relativePath) ? "index.html" : relativePath);


                //if (!File.Exists(filePath))
                //{
                //    // TODO: Если нет файла "404.html" отправлять просто статус код и текст
                //    filePath = Path.Combine(_staticDirectoryPath, "404.html");
                //    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                //}
                try 
                {   byte[] responseFile = File.ReadAllBytes(filePath);
                    context.Response.ContentType = GetContentType(Path.GetExtension(filePath));
                    context.Response.ContentLength64 = responseFile.Length;
                    context.Response.OutputStream.Write(responseFile, 0, responseFile.Length);
                    context.Response.OutputStream.Close();
                }
                catch (Exception ex)
                {
                    //throw new Exception(ex.ToString());
                }
                

            }
            // передача запроса дальше по цепи при наличии в ней обработчиков
            else if (Successor != null)
            {
                // Не правим
                Successor.HandleRequest(context);
            }
        }

        /// <summary>
        /// Метод получения ContentType файла
        /// </summary>
        /// <param name="extension">передает расширение</param>
        /// <returns>Возвращает тип расширения файла</returns>
        private string GetContentType(string? extension)
        {
            if (extension == null)
            {
                throw new ArgumentNullException(nameof(extension), "Extension cannot be null.");
            }

            return extension.ToLower() switch
            {
                ".html" => "text/html",
                ".css" => "text/css",
                ".js" => "application/javascript",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                _ => "application/octet-stream",
            };
        }
    }
}



