using System.IO;

namespace ClientUI.ViewModels;

using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Lab3.Models.Handlers;
using Models;

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
    private string _selectedPath = "";

    [ObservableProperty]
    private string _response;

    private readonly ObservableCollection<string> _availableDrives = new();

    public ObservableCollection<string> AvailableDrives => _availableDrives;
 
    [ObservableProperty]
    private ObservableCollection<FileSystemItem> _currentItems = new();

    [ObservableProperty]
    private string _currentPath;

    [RelayCommand]
    private async Task NavigateToAsync(FileSystemItem item)
    {
        if (item?.IsDirectory != true) return;
     
        SelectedPath = item.Path; 
        await SendRequestAsync();
    }

    [RelayCommand]
    private async Task GoUpAsync()
    {
        if (string.IsNullOrEmpty(CurrentPath)) return;
     
        var parent = System.IO.Path.GetDirectoryName(CurrentPath);
        if (parent != null) // Change to null check
        {
            SelectedPath = parent;
            await SendRequestAsync();
        }
        else
        {
            SelectedPath = ""; 
            await SendRequestAsync();
        }
    }

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

            // Optionally set the first drive as the SelectedPath for immediate display
            if (AvailableDrives.Count > 0)
            {
                SelectedPath = AvailableDrives[0];
                await SendRequestAsync(); // Immediately load the contents of the first drive
            }
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
            StatusMessage = "Loading...";
            await _clientHandler.SendMessageAsync(_stream, SelectedPath);
            var response = await _clientHandler.ReceiveMessageAsync(_stream);
         
            CurrentPath = SelectedPath;
            ParseResponse(response);
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
    private void ParseResponse(string response)
    {
        CurrentItems.Clear();
        var items = response.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (var item in items)
        {
            var isDirectory = item.EndsWith("/");
            var cleanName = isDirectory ? item.TrimEnd('/') : item;

            // Robustly combine paths and normalize
            string fullPath;
            if (string.IsNullOrEmpty(CurrentPath) || CurrentPath == "/")
            {
                fullPath = cleanName; // Handle root
            }
            else
            {
                fullPath = CurrentPath + "/" + cleanName;
            }

            CurrentItems.Add(new FileSystemItem
            {
                Name = cleanName,
                Path = fullPath,
                IsDirectory = isDirectory
            });
        }
    }
    
    
}