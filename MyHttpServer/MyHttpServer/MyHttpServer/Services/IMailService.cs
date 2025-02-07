
namespace MyHttpServer.Services
{
    /// <summary>
    /// Определяет интерфейс для службы отправки писем.
    /// </summary>
    internal interface IMailServices
    {
        Task SendAsync(string emailTo, string body, string Path); // Сигнатура метода для асинхронной отправки электронного письма.
                                                                  // Метод принимает в качестве входных данных адрес электронной почты
                                                                  // получателя, тело электронного письма и путь.
    }
}
