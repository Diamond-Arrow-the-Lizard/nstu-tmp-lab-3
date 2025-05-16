using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ClientUI.Models;
using Lab3.Models;
using Lab3.Models.Handlers;

namespace ClientUI.ViewModels
{
    public partial class ClientViewModel : ViewModelBase
    {
        private readonly ClientHandler _clientHandler;
        private TcpClient _client;
        private NetworkStream _stream;
        [ObservableProperty] private string _currentPath = string.Empty;
        [ObservableProperty] private string _serverIp = "127.0.0.1";
        [ObservableProperty] private string _statusMessage = "Disconnected";
        [ObservableProperty] private ObservableCollection<string> _availableDrives = new();
        [ObservableProperty] private ObservableCollection<FileSystemItem> _currentItems = new();
        [ObservableProperty] private string _selectedPath; // This will now represent the selected item in the upper ListBox

        public ClientViewModel()
        {
            _clientHandler = new ClientHandler(5000);
        }

        [RelayCommand]
        private async Task ConnectAsync()
        {
            try
            {
                StatusMessage = "Connecting...";
                _client = new TcpClient();
                await _client.ConnectAsync(ServerIp, _clientHandler.Port);
                _stream = _client.GetStream();

                var drives = await _clientHandler.ReceiveMessageAsync(_stream);
                AvailableDrives.Clear();
                foreach (var drive in drives.Split(';'))
                {
                    AvailableDrives.Add(drive);
                }
                CurrentPath = "/"; // Initialize current path to root for navigation
                StatusMessage = "Connected";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                Disconnect();
            }
        }

        [RelayCommand]
        private async Task GoUpAsync()
        {
            if (string.IsNullOrEmpty(CurrentPath) || CurrentPath == "/")
            {
                // Already at the root (drives level), no need to go up further
                AvailableDrives.Clear();
                var drivesResponse = await _clientHandler.SendMessageAndReceiveResponseAsync(_stream, "LIST_DRIVES");
                if (drivesResponse != null)
                {
                    foreach (var drive in drivesResponse.Split(';'))
                    {
                        AvailableDrives.Add(drive);
                    }
                    CurrentPath = "/";
                    CurrentItems.Clear(); // Clear the lower list as well
                }
                return;
            }

            var parentPath = Directory.GetParent(CurrentPath)?.FullName?.Replace('\\', '/');
            if (parentPath == null)
            {
                CurrentPath = "/"; // Go back to the root (drives)
                AvailableDrives.Clear();
                var drivesResponse = await _clientHandler.SendMessageAndReceiveResponseAsync(_stream, "LIST_DRIVES");
                if (drivesResponse != null)
                {
                    foreach (var drive in drivesResponse.Split(';'))
                    {
                        AvailableDrives.Add(drive);
                    }
                    CurrentItems.Clear(); // Clear the lower list
                }
            }
            else
            {
                CurrentPath = parentPath;
                await LoadCurrentPathContentsAsync();
            }
        }

        partial void OnSelectedPathChanged(string value)
        {
            if (_client != null && !string.IsNullOrEmpty(value))
            {
                // Check if the selected item in the upper list is a directory (ends with '/')
                if (value.EndsWith("/"))
                {
                    CurrentPath = value;
                    LoadCurrentPathContentsAsync().ConfigureAwait(false);
                }
                else
                {
                    // Assume it's a file, request its content
                    LoadFileContentAsync(value).ConfigureAwait(false);
                }
            }
        }

        private async Task LoadCurrentPathContentsAsync()
        {
            if (_client == null || _stream == null || string.IsNullOrEmpty(CurrentPath))
                return;

            var response = await _clientHandler.SendMessageAndReceiveResponseAsync(_stream, $"LIST {CurrentPath}");
            if (response != null)
            {
                ParseDirectoryResponse(response);
            }
        }

        private void ParseDirectoryResponse(string response)
        {
            AvailableDrives.Clear(); // Clear the upper list to show directory contents
            CurrentItems.Clear();    // Clear the lower list

            var items = response.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in items)
            {
                var isDirectory = item.EndsWith("/");
                var cleanName = isDirectory ? item.TrimEnd('/') : item;
                AvailableDrives.Add(CurrentPath == "/" ? cleanName + (isDirectory ? "/" : "") : Path.Combine(CurrentPath, cleanName).Replace('\\', '/') + (isDirectory ? "/" : ""));
                CurrentItems.Add(new FileSystemItem { Name = cleanName, IsDirectory = isDirectory });
            }
        }

        private async Task LoadFileContentAsync(string filePath)
        {
            if (_client == null || _stream == null || string.IsNullOrEmpty(filePath))
                return;

            var response = await _clientHandler.SendMessageAndReceiveResponseAsync(_stream, $"GET {filePath}");
            if (response != null)
            {
                CurrentItems.Clear();
                CurrentItems.Add(new FileSystemItem { Path = filePath.Split('/').Last(), Name = response, IsDirectory = false});
            }
        }

        private void Disconnect()
        {
            _client?.Close();
            _stream?.Close();
            _client = null;
            _stream = null;
            StatusMessage = "Disconnected";
            AvailableDrives.Clear();
            CurrentItems.Clear();
            CurrentPath = string.Empty;
        }
    }
}