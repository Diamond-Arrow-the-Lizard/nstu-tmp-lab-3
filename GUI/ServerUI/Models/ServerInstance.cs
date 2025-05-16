using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Lab3.Interfaces;
using Lab3.Interfaces.Handlers;
using Lab3.Models.Handlers;
using Lab3.Models.Services;

namespace ServerUI.Models;

public class ServerInstance
{
    private TcpListener? _listener;
    private bool _isRunning;
    private readonly RequestHandler _requestHandler;
    private readonly IDiskService _diskService;
    private readonly ServerHandler _serverHandler;
    private readonly Action<string> _logHandler;

    public ServerInstance(
        IFileSystemService fsService,
        IDiskService diskService,
        ServerHandler serverHandler,
        Action<string> logHandler)
    {
        _diskService = diskService;
        _serverHandler = serverHandler;
        _requestHandler = new RequestHandler(fsService, diskService);
        _logHandler = logHandler;
    }

    public async Task StartAsync(int port)
    {
        _isRunning = true;
        _listener = new TcpListener(IPAddress.Any, port);
        _listener.Start();
        _logHandler($"Server started on port {port}");

        while (_isRunning)
        {
            var client = await _listener.AcceptTcpClientAsync();
            _ = HandleClientAsync(client);
        }
    }

    public void Stop()
    {
        _isRunning = false;
        _listener?.Stop();
        _logHandler("Server stopped");
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        _logHandler("Client connected");
        using var stream = client.GetStream();

        try
        {
            var driveList = _diskService.GetDrivesString();
            await _serverHandler.SendMessageAsync(stream, driveList);
            _logHandler($"Sent drive list: {driveList}");

            while (true)
            {
                string request = await _serverHandler.ReceiveMessageAsync(stream);
                if (string.IsNullOrEmpty(request)) break;

                string response = await _requestHandler.HandleRequestAsync(request);
                await _serverHandler.SendMessageAsync(stream, response);
                _logHandler($"Response sent: {response.Substring(0, Math.Min(20, response.Length))}...");
            }
        }
        catch (IOException ex)
        {
            _logHandler($"Client error: {ex.Message}");
        }
        finally
        {
            client.Close();
            _logHandler("Client disconnected");
        }
    }
}