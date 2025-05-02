using System;
using System.Net;
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
        string ipInput = Console.ReadLine()?.Trim()
            ?? throw new ArgumentNullException(nameof(ipInput));

        if (!IPAddress.TryParse(ipInput, out IPAddress? ipAddress))
        {
            Console.WriteLine("Неверный формат IP-адреса.");
            return;
        }

        using var client = new TcpClient();
        client.Connect(ipAddress, port);
        using var stream = client.GetStream();

        // Получение списка дисков
        string drives = clientHandler.ReceiveMessage(stream);
        Console.WriteLine("Доступные диски: " + drives);

        Console.Write("Введите путь к каталогу или файлу: ");
        string path = Console.ReadLine()?.Trim()
            ?? throw new ArgumentNullException(nameof(path));
        clientHandler.SendMessage(stream, path);

        string response = clientHandler.ReceiveMessage(stream);
        Console.WriteLine("Ответ от сервера:\n" + response);

    }
}
