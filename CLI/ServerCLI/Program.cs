using System;
using System.Net;
using System.Net.Sockets;
using Lab3.Models.Handlers;
using Lab3.Models.Services;
using Lab3.Interfaces.Handlers;
using System.Threading.Tasks;
using Lab3.Interfaces;

public class ServerApp
{
    const int port = 5000;

    public static async Task Main(string[] args)
    {
        var fileSystemService = new FileSystemService();
        var diskService = new DiskService();
        var requestHandler = new RequestHandler(fileSystemService, diskService); 
        var serverHandler = new ServerHandler(port);

        TcpListener listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        Console.WriteLine($"[{DateTime.Now}] Server started on port {port}");

        while (true)
        {
            var client = await listener.AcceptTcpClientAsync();
            _ = HandleClientAsync(client, serverHandler, requestHandler, diskService); 
        }
    }

    static async Task HandleClientAsync(TcpClient client, ServerHandler serverHandler, RequestHandler requestHandler, IDiskService diskService) 
    {
        Console.WriteLine($"[{DateTime.Now}] Client connected.");
        using var stream = client.GetStream();

        try
        {
            var driveList = diskService.GetDrivesString();
            await serverHandler.SendMessageAsync(stream, driveList);
            Console.WriteLine($"[{DateTime.Now}] Drive list sent to client: {driveList}");
            
            while (true)
            {
                string request = await serverHandler.ReceiveMessageAsync(stream);
                if (string.IsNullOrEmpty(request))
                    break;

                string response = await requestHandler.HandleRequestAsync(request); 
                await serverHandler.SendMessageAsync(stream, response);
                Console.WriteLine($"[{DateTime.Now}] Response sent to client.");
            }
        }
        catch (IOException ex)
        {
            Console.WriteLine($"[{DateTime.Now}] Client disconnected unexpectedly: {ex.Message}");
        }
        finally
        {
            stream.Close();
            client.Close();
            Console.WriteLine($"[{DateTime.Now}] Client disconnected.");
        }
    }
}