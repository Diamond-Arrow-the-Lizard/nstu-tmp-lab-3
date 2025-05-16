using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System;
using Lab3.Interfaces;
using Lab3.Models.Handlers;
using ServerUI.Models;

namespace ServerUI.ViewModels;

public partial class ServerViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool _isServerRunning;

    [ObservableProperty]
    private ObservableCollection<string> _logs = new();

    [ObservableProperty]
    private string _port = "5000";

    private ServerInstance? _serverInstance;
    private readonly IFileSystemService _fsService;
    private readonly IDiskService _diskService;
    private readonly ServerHandler _serverHandler;

    public ServerViewModel(
        IFileSystemService fsService,
        IDiskService diskService,
        ServerHandler serverHandler)
    {
        _fsService = fsService;
        _diskService = diskService;
        _serverHandler = serverHandler;
    }

    [RelayCommand]
    private async Task ToggleServer()
    {
        if (!int.TryParse(Port, out int port) || port < 1 || port > 65535)
        {
            AddLog("Invalid port number");
            return;
        }

        if (IsServerRunning)
        {
            StopServer();
        }
        else
        {
            await StartServer(port);
        }
    }

    private async Task StartServer(int port)
    {
        _serverInstance = new ServerInstance(
            _fsService,
            _diskService,
            _serverHandler,
            AddLog);

        IsServerRunning = true;
        await Task.Run(() => _serverInstance.StartAsync(port));
    }

    private void StopServer()
    {
        _serverInstance?.Stop();
        IsServerRunning = false;
        AddLog("Server stopped");
    }

    public void AddLog(string message)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            Logs.Insert(0, $"[{DateTime.Now:T}] {message}");
        });
    }
}