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
        private CancellationTokenSource _cts = new CancellationTokenSource(); // ��� ������ ������ ��� �������� �����

        public Form1()
        {
            InitializeComponent();

            // ����������� � �������
            client.Connect(host, port);
            Reader = new StreamReader(client.GetStream(), Encoding.UTF8); // �������� ���������
            Writer = new StreamWriter(client.GetStream(), Encoding.UTF8); // �������� ���������

            // ������ ����� ������������
            userName = Microsoft.VisualBasic.Interaction.InputBox("������� ���� ���:", "��� ������������", "User");
            if (string.IsNullOrEmpty(userName)) userName = "User";

            // �������� ����� �� ������
            Writer.WriteLine(userName);
            Writer.Flush();

            // ������ ������ ��� ��������� ���������
            Task.Run(() => ReceiveMessageAsync(Reader, _cts.Token));

            // ����������� ����������� ������� �������� �����
            this.FormClosing += Form1_FormClosing;
        }

        private async void button1_Click_1(object sender, EventArgs e)
        {
            string message = textBox1.Text;
            if (!string.IsNullOrEmpty(message))
            {
                // ��������� ��� ��������� � ListBox
                Print($"{userName}: {message}");

                // ���������� ��������� �� ������
                await SendMessageAsync(message);
                textBox1.Clear();
            }
        }

        // �������� ���������
        async Task SendMessageAsync(string message)
        {
            if (Writer != null)
            {
                await Writer.WriteLineAsync(message);
                await Writer.FlushAsync();
            }
        }

        // ��������� ���������
        async Task ReceiveMessageAsync(StreamReader reader, CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    string? message = await reader.ReadLineAsync();
                    if (string.IsNullOrEmpty(message)) continue;

                    // ����� ��������� � ListBox
                    Print(message);
                }
            }
            catch (Exception ex)
            {
                Print($"������: {ex.Message}");
            }
            finally
            {
                Disconnect();
            }
        }

        // ����� ��������� � ListBox
        void Print(string message)
        {
            if (listBox1.InvokeRequired)
            {
                listBox1.Invoke(new Action<string>(Print), message);
            }
            else
            {
                listBox1.Items.Add(message);
                listBox1.TopIndex = listBox1.Items.Count - 1; // ��������� � ���������� ���������
            }
        }

        // ��������� �������� �����
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _cts.Cancel(); // ������ ������ ��� �������� �����

            if (Writer != null)
            {
                Writer.WriteLine($"{userName} ����� �� ����");
                Writer.Flush();
            }
            Writer?.Close();
            Reader?.Close();
            client.Close();
        }

        // ����� ��� ���������� �������
        private void Disconnect()
        {
            Writer?.Close();
            Reader?.Close();
            client.Close();
        }
    }
}