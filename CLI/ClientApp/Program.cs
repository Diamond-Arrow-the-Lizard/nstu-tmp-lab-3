using System;
using System.Net;
using System.Net.Sockets;
using Lab3.Models.Handlers;

class ClientApp
{
    const int port = 5000;
    const string defaultIp = "127.0.0.1";

    static async Task Main()
    {
        var clientHandler = new ClientHandler(port);

        Console.Write($"Введите IP-адрес сервера [по умолчанию {defaultIp}]: ");
        string? ipInput = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(ipInput))
        {
            ipInput = defaultIp;
            Console.WriteLine($"Используется адрес по умолчанию: {defaultIp}");
        }

        if (!IPAddress.TryParse(ipInput, out IPAddress? ipAddress))
        {
            Console.WriteLine($"Неверный формат IP-адреса. Используется {defaultIp}");
            ipAddress = IPAddress.Parse(defaultIp);
        }

        using var client = new TcpClient();
        try
        {
            client.Connect(ipAddress, port);
            using var stream = client.GetStream();

            // Получение списка дисков
            var drives = await clientHandler.ReceiveMessage(stream);
            Console.WriteLine("Доступные диски:\n" + drives.Replace(";", "\n"));

            Console.Write("Введите путь к каталогу или файлу: ");
            var path = Console.ReadLine()?.Trim() ?? "";
            await clientHandler.SendMessage(stream, path);

            // Получение ответа
            var response = await clientHandler.ReceiveMessage(stream);
            Console.WriteLine("\nРезультат:\n" + response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }
}