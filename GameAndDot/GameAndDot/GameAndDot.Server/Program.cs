using System.Net.Sockets;
using System.Net;
using System.Text.Json;
using GameAndDot.Packages;
using GameAndDot.Packages.Enums;
using GameAndDot.Packages.Models;

ServerObject server = new ServerObject();// создаем сервер
await server.ListenAsync(); // запускаем сервер

class ServerObject
{
    TcpListener tcpListener = new TcpListener(IPAddress.Any, 8888); // сервер для прослушивания
    List<ClientObject> clients = new List<ClientObject>(); // все подключения
    List<PointObject> points = new List<PointObject>(); // все подключения

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
    protected internal async Task BroadcastMessageAsync(MessageObject message, string id)
    {
        string package = JsonSerializer.Serialize(message);

        foreach (var client in clients)
        {
            await client.Writer.WriteLineAsync(package); //передача данных
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

    protected internal string GeUsersMessage()
    {
        var usernames = clients.Select(p => p.Username);
        string result = JsonSerializer.Serialize(usernames);

        return result;
    }

    protected internal void AddPoint(PointObject point)
    {
        points.Add(point);
    }
    protected internal string GetPoints()
    {
        string result = JsonSerializer.Serialize(points);

        return result;
    }
}
class ClientObject
{
    protected internal string Id { get; } = Guid.NewGuid().ToString();
    protected internal StreamWriter Writer { get; }
    protected internal StreamReader Reader { get; }

    public string Username { get; set; }

    TcpClient client;
    ServerObject server; // объект сервера

    public ClientObject(TcpClient tcpClient, ServerObject serverObject)
    {
        client = tcpClient;
        server = serverObject;
        // получаем NetworkStream для взаимодействия с сервером
        var stream = client.GetStream();
        // создаем StreamReader для чтения данных
        Reader = new StreamReader(stream);
        // создаем StreamWriter для отправки данных
        Writer = new StreamWriter(stream);
    }

    public async Task ProcessAsync()
    {
        try
        {
            while (true)
            {
                try
                {
                    string? str = await Reader.ReadLineAsync();
                    if (str == null) continue;

                    var message = JsonSerializer.Deserialize<MessageObject>(str);
                    MessageObject pkg = new MessageObject
                    {
                        Type = message.Type
                    };

                    switch (message.Type)
                    {
                        case MessageType.Register:
                            Username = message.Data;
                            pkg.Data = server.GeUsersMessage();

                            await server.BroadcastMessageAsync(pkg, Id);

                            break;

                        case MessageType.Draw:
                            var newPoint = JsonSerializer.Deserialize<PointObject>(message.Data);
                            server.AddPoint(newPoint);
                            pkg.Data = server.GetPoints();

                            await server.BroadcastMessageAsync(pkg, Id);

                            break;
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Произошла ошибка", ex);

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
            // в случае выхода из цикла закрываем ресурсы
            server.RemoveConnection(Id);
        }
    }

    // закрытие подключения
    protected internal void Close()
    {
        Writer.Close();
        Reader.Close();
        client.Close();
    }
}