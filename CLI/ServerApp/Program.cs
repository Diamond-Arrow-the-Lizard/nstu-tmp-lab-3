using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Lab3.Models.Handlers;

public class ServerApp
{
    const int port = 5000;
    public static void Main(string[] args)
    {
        TcpListener listener = new TcpListener(IPAddress.Any, port);
        ServerHandler serverHandler = new ServerHandler(port);

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
            serverHandler.SendMessage(stream, driveList);

            // Получение пути от клиента
            string path = serverHandler.ReceiveMessage(stream);

            string response;
            try
            {
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
                    response = "Ошибка: пути не существует.";
                }

                // Отправка результата
                serverHandler.SendMessage(stream, response);
                Console.WriteLine("Ответ отправлен, клиент отключён.");

            }

            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("Клиент не может получить информацию о запрошенном пути: доступ воспрещён.");
                serverHandler.SendMessage(stream, "У вас нет прав на просмотр этого пути.");
            }
        }

    }

}