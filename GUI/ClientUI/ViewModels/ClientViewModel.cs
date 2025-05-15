namespace ClientUI.ViewModels;

using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Lab3.Models.Handlers;

public partial class ClientViewModel : ObservableObject
{
    private readonly ClientHandler _clientHandler;
    private TcpClient _client;
    private NetworkStream _stream;

    [ObservableProperty]
    private string _serverIp = "127.0.0.1";

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private string _selectedPath;

    [ObservableProperty]
    private string _response;

    private readonly ObservableCollection<string> _availableDrives = new();

    public ObservableCollection<string> AvailableDrives => _availableDrives;

    public ClientViewModel(ClientHandler clientHandler)
    {
        _clientHandler = clientHandler;
    }

    [RelayCommand]
    private async Task ConnectAsync()
    {
        try
        {
            StatusMessage = "Connecting...";
            _client = new TcpClient();
            await _client.ConnectAsync(IPAddress.Parse(ServerIp), _clientHandler.Port);
            _stream = _client.GetStream();

            var drives = await _clientHandler.ReceiveMessageAsync(_stream);
            AvailableDrives.Clear();
            foreach (var drive in drives.Split(';'))
            {
                AvailableDrives.Add(drive);
            }
            StatusMessage = "Connected";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            Disconnect();
        }
    }

    [RelayCommand]
    private async Task SendRequestAsync()
    {
        if (string.IsNullOrWhiteSpace(SelectedPath)) return;

        try
        {
            await _clientHandler.SendMessageAsync(_stream, SelectedPath);
            Response = await _clientHandler.ReceiveMessageAsync(_stream);
            StatusMessage = "Request completed";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            Disconnect();
        }
    }

    private void Disconnect()
    {
        _stream?.Dispose();
        _client?.Dispose();
        AvailableDrives.Clear();
        Response = string.Empty;
    }
}