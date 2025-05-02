using System;
using System.Net;
using System.Net.Sockets;
using Lab3.Models.Handlers;
using Lab3.Models.Services;
using Lab3.Interfaces.Handlers;

public class ServerApp
{
    const int port = 5000;
    
    public static void Main(string[] args)
    {
        var fileSystemService = new FileSystemService();
        var diskService = new DiskService();
        var requestHandler = new RequestHandler(fileSystemService);
        var serverHandler = new ServerHandler(port);

        TcpListener listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        Console.WriteLine($"Сервер запущен на порту {port}");

        while (true)
        {
            using var client = listener.AcceptTcpClient();
            Console.WriteLine("Клиент подключен.");
            using var stream = client.GetStream();

            // Отправка списка дисков через DiskService
            var driveList = diskService.GetDrivesString();
            serverHandler.SendMessage(stream, driveList);

            // Получение пути от клиента
            string path = serverHandler.ReceiveMessage(stream);

            // Обработка запроса через RequestHandler
            string response = requestHandler.HandleRequest(path);
            
            // Отправка результата
            serverHandler.SendMessage(stream, response);
            Console.WriteLine("Ответ отправлен, клиент отключён.");
        }
    }
}