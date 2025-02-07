using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

ServerObject server = new ServerObject();// создаем сервер
await server.ListenAsync(); // запускаем сервер

class ServerObject
{
    TcpListener tcpListener = new TcpListener(IPAddress.Any, 8888); // сервер для прослушивания
    List<ClientObject> clients = new List<ClientObject>(); // все подключения
    List<string> chatHistory = new List<string>(); // история сообщений чата

    protected internal void RemoveConnection(string id)
    {
        // получаем по id закрытое подключение
        ClientObject? client = clients.FirstOrDefault(c => c.Id == id);
        // и удаляем его из списка подключений
        if (client != null) clients.Remove(client);
        client?.Close();
    }

    // прослушивание входящих подключений
    protected internal async Task ListenAsync()
    {
        try
        {
            tcpListener.Start();
            Console.WriteLine("Сервер запущен. Ожидание подключений...");

            while (true)
            {
                TcpClient tcpClient = await tcpListener.AcceptTcpClientAsync();

                ClientObject clientObject = new ClientObject(tcpClient, this);
                clients.Add(clientObject);
                Task.Run(clientObject.ProcessAsync);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            Disconnect();
        }
    }

    // трансляция сообщения подключенным клиентам
    protected internal async Task BroadcastMessageAsync(string message, string id)
    {
        // Добавляем сообщение в историю
        chatHistory.Add(message);

        // Отправляем сообщение всем клиентам, кроме отправителя
        foreach (var client in clients)
        {
            if (client.Id != id) // если id клиента не равно id отправителя
            {
                await client.Writer.WriteLineAsync(message); //передача данных
                await client.Writer.FlushAsync();
            }
        }
    }

    // Отправка истории сообщений новому клиенту
    protected internal async Task SendChatHistoryAsync(ClientObject client)
    {
        foreach (var message in chatHistory)
        {
            await client.Writer.WriteLineAsync(message);
            await client.Writer.FlushAsync();
        }
    }

    // отключение всех клиентов
    protected internal void Disconnect()
    {
        foreach (var client in clients)
        {
            client.Close(); //отключение клиента
        }
        tcpListener.Stop(); //остановка сервера
    }
}

class ClientObject
{
    protected internal string Id { get; } = Guid.NewGuid().ToString();
    protected internal StreamWriter Writer { get; }
    protected internal StreamReader Reader { get; }

    TcpClient client;
    ServerObject server; // объект сервера

    public ClientObject(TcpClient tcpClient, ServerObject serverObject)
    {
        client = tcpClient;
        server = serverObject;
        var stream = client.GetStream();
        Reader = new StreamReader(stream, Encoding.UTF8);
        Writer = new StreamWriter(stream, Encoding.UTF8);
    }

    public async Task ProcessAsync()
    {
        try
        {
            // Получаем имя пользователя
            string? userName = await Reader.ReadLineAsync();
            string? message = $"{userName} вошел в чат";
            Console.WriteLine(message);

            // Отправляем историю сообщений новому клиенту
            await server.SendChatHistoryAsync(this);

            // Рассылаем сообщение о входе в чат
            await server.BroadcastMessageAsync(message, Id);

            // Чтение сообщений от клиента
            while (true)
            {
                try
                {
                    message = await Reader.ReadLineAsync();
                    if (message == null) continue;

                    message = $"{userName}: {message}";
                    Console.WriteLine(message);
                    await server.BroadcastMessageAsync(message, Id);
                }
                catch
                {
                    message = $"{userName} покинул чат";
                    Console.WriteLine(message);
                    await server.BroadcastMessageAsync(message, Id);
                    break;
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        finally
        {
            server.RemoveConnection(Id);
        }
    }

    protected internal void Close()
    {
        Writer.Close();
        Reader.Close();
        client.Close();
    }
}