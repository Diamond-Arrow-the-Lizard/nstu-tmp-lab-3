using System;
using System.Net;
using System.Net.Sockets;
using Lab3.Models.Handlers;
using Lab3.Models.Services;
using Lab3.Interfaces.Handlers;
using System.Threading.Tasks;

public class ServerApp
{
    const int port = 5000;

    public static async Task Main(string[] args)
    {
        var fileSystemService = new FileSystemService();
        var diskService = new DiskService();
        var requestHandler = new RequestHandler(fileSystemService);
        var serverHandler = new ServerHandler(port);

        TcpListener listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        Console.WriteLine($"[{DateTime.Now}] Server started on port {port}");

        while (true)
        {
            var client = await listener.AcceptTcpClientAsync(); // Accept client asynchronously
            _ = HandleClientAsync(client, diskService, serverHandler, requestHandler); // Handle client in a separate task
        }
    }

    static async Task HandleClientAsync(TcpClient client, DiskService diskService, ServerHandler serverHandler, RequestHandler requestHandler)
    {
        Console.WriteLine($"[{DateTime.Now}] Client connected.");
        using var stream = client.GetStream();

        try
        {
            // Send initial drive list
            var driveList = diskService.GetDrivesString();
            await serverHandler.SendMessageAsync(stream, driveList);

            // Handle multiple requests from the same client
            while (true)
            {
                string path = await serverHandler.ReceiveMessageAsync(stream);
                if (string.IsNullOrEmpty(path)) // Client might disconnect
                    break;

                string response = requestHandler.HandleRequest(path);
                await serverHandler.SendMessageAsync(stream, response);
                Console.WriteLine($"[{DateTime.Now}] Response sent to client.");
            }
        }
        catch (IOException ex) // Catch connection errors
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