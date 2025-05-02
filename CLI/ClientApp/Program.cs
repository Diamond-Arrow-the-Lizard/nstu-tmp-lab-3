using System;
using System.Net.Sockets;
using System.Text;
using Lab3.Models;

class ClientApp
{
    const int port = 5000;

    static void Main()
    {
        ClientHandler clientHandler = new ClientHandler(port);

        Console.Write("Введите IP-адрес сервера: ");
        string ip = Console.ReadLine() ?? throw new ArgumentNullException(nameof(ip));

        using var client = new TcpClient();
        client.Connect(ip, port);
        using var stream = client.GetStream();

        // Получение списка дисков
        string drives = clientHandler.ReceiveMessage(stream);
        Console.WriteLine("Доступные диски: " + drives);

        Console.Write("Введите путь к каталогу или файлу: ");
        string path = Console.ReadLine() ?? throw new ArgumentNullException(nameof(ip));
        clientHandler.SendMessage(stream, path);

        string response = clientHandler.ReceiveMessage(stream);
        Console.WriteLine("Ответ от сервера:\n" + response);
    }
}
