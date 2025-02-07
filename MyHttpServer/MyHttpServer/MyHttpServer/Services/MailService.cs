using System.Net.Mail;
using System.Net;


namespace MyHttpServer.Services
{
    /// <summary>
    /// Служба для отправки писем с использованием SMTP.
    /// </summary>
    internal class MailService : IMailServices // Класс, реализующий интерфейс IMailService.
    {
        /// <summary>
        /// Асинхронно отправляет электронное письмо с помощью SMTP. Текст электронного письма меняется в зависимости от указанного пути.
        /// </summary>
        public async Task SendAsync(string email, string message, string path)
        {
            SmtpClient smtp = new SmtpClient("smtp.yandex.ru", 587); // Создает новый объект SmtpClient с указанным SMTP-сервером и портом.
            string attachmentPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "ZIP", "MyHttpServer.txt"); // Создает полный путь к файлу вложения.


            using (smtp) // Использует оператор using, чтобы гарантировать, что объект SmtpClient будет правильно удален после использования
            {
                smtp.Credentials = new NetworkCredential("n3kawi@yandex.ru", "bymvjygtohvqltle"); // Устанавливает учетные данные SMTP-сервера.
                smtp.EnableSsl = true; // Включает шифрование SSL для безопасной связи с SMTP-сервером.

                using (MailMessage m = new MailMessage()) // Создает новый объект MailMessage для хранения сведений о сообщении электронной почты.
                {
                    m.From = new MailAddress("n3kawi@yandex.ru"); // Устанавливает адрес электронной почты отправителя.
                    m.To.Add(email); // Добавляет адрес электронной почты получателя.
                    m.Subject = "Тест";// Устанавливает тему письма.
                    Console.WriteLine(path); // Выводит путь к консоли.
                    switch (path.Split(@"\")[^1]) // Оператор switch для определения содержимого тела письма на основе последней части предоставленного пути.
                    {
                        case "EASports.html":
                            m.Body = $"Добро пожаловать в систему ваш логин {email} ваш пароль {message}";
                            break;

                        case "lol.html":
                            m.Body =
                                $"Ха-ха вы попались, ваш логин {email} ваш пароль {message} теперь знаю я Низамов Алмаз";
                            break;

                        case "requests.html":
                            m.Body = $"Вы подписались на рассылку ваш логин {email} ваш пароль {message}";
                            break;

                        case "home-work.html":
                            m.Body = $"Мое ДЗ. Садыкова Алина Дамировна 11-309";
                            m.Attachments.Add(new Attachment(attachmentPath)); // Добавляет вложение к электронному письму.
                            break;
                    }

                    try
                    {
                        smtp.Send(m); // Отправляет электронное письмо асинхронно.
                        Console.WriteLine("Сообщение отправлено"); // Выводит сообщение об успешном завершении на консоль.
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Ошибка отправки сообщения: {e.Message}");
                        Console.WriteLine($"Ошибка отправки сообщения: {e.StackTrace}");
                    }
                }
            }
        }
    }
}