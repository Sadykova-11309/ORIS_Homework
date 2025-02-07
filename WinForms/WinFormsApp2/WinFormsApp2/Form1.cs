using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsApp2
{
    public partial class Form1 : Form
    {
        string host = "127.0.0.1";
        int port = 8888;
        TcpClient client = new TcpClient();
        StreamReader? Reader = null;
        StreamWriter? Writer = null;
        string? userName;
        private CancellationTokenSource _cts = new CancellationTokenSource(); // Для отмены задачи при закрытии формы

        public Form1()
        {
            InitializeComponent();

            // Подключение к серверу
            client.Connect(host, port);
            Reader = new StreamReader(client.GetStream(), Encoding.UTF8); // Указание кодировки
            Writer = new StreamWriter(client.GetStream(), Encoding.UTF8); // Указание кодировки

            // Запрос имени пользователя
            userName = Microsoft.VisualBasic.Interaction.InputBox("Введите ваше имя:", "Имя пользователя", "User");
            if (string.IsNullOrEmpty(userName)) userName = "User";

            // Отправка имени на сервер
            Writer.WriteLine(userName);
            Writer.Flush();

            // Запуск потока для получения сообщений
            Task.Run(() => ReceiveMessageAsync(Reader, _cts.Token));

            // Подключение обработчика события закрытия формы
            this.FormClosing += Form1_FormClosing;
        }

        private async void button1_Click_1(object sender, EventArgs e)
        {
            string message = textBox1.Text;
            if (!string.IsNullOrEmpty(message))
            {
                // Добавляем своё сообщение в ListBox
                Print($"{userName}: {message}");

                // Отправляем сообщение на сервер
                await SendMessageAsync(message);
                textBox1.Clear();
            }
        }

        // Отправка сообщений
        async Task SendMessageAsync(string message)
        {
            if (Writer != null)
            {
                await Writer.WriteLineAsync(message);
                await Writer.FlushAsync();
            }
        }

        // Получение сообщений
        async Task ReceiveMessageAsync(StreamReader reader, CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    string? message = await reader.ReadLineAsync();
                    if (string.IsNullOrEmpty(message)) continue;

                    // Вывод сообщения в ListBox
                    Print(message);
                }
            }
            catch (Exception ex)
            {
                Print($"Ошибка: {ex.Message}");
            }
            finally
            {
                Disconnect();
            }
        }

        // Вывод сообщения в ListBox
        void Print(string message)
        {
            if (listBox1.InvokeRequired)
            {
                listBox1.Invoke(new Action<string>(Print), message);
            }
            else
            {
                listBox1.Items.Add(message);
                listBox1.TopIndex = listBox1.Items.Count - 1; // Прокрутка к последнему сообщению
            }
        }

        // Обработка закрытия формы
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _cts.Cancel(); // Отмена задачи при закрытии формы

            if (Writer != null)
            {
                Writer.WriteLine($"{userName} вышел из чата");
                Writer.Flush();
            }
            Writer?.Close();
            Reader?.Close();
            client.Close();
        }

        // Метод для отключения клиента
        private void Disconnect()
        {
            Writer?.Close();
            Reader?.Close();
            client.Close();
        }
    }
}