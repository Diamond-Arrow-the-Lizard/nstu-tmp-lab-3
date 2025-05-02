using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Lab3.Models;

public class ServerApp
{
    const int port = 5000;
    public static void Main(string[] args)
    {
        TcpListener listener = new TcpListener(IPAddress.Any, port);
        Server server = new Server(port);

        listener.Start();
        Console.WriteLine($"Сервер запущен на порту {port}");

        while (true)
        {
            using var client = listener.AcceptTcpClient();
            Console.WriteLine("Клиент подключен.");
            using var stream = client.GetStream();

            // Отправка списка дисков
            var drives = DriveInfo.GetDrives();
            var driveList = string.Join(";", Array.ConvertAll(drives, d => d.Name));
            server.SendMessage(stream, driveList);

            // Получение пути от клиента
            string path = server.ReceiveMessage(stream);

            string response;
            if (Directory.Exists(path))
            {
                var entries = Directory.GetFileSystemEntries(path);
                response = string.Join(Environment.NewLine, entries);
            }
            else if (File.Exists(path))
            {
                response = File.ReadAllText(path);
            }
            else
            {
                response = "Ошибка: путь не существует.";
            }

            // Отправка результата
            server.SendMessage(stream, response);
            Console.WriteLine("Ответ отправлен, соединение закрыто.");
        }

    }

}