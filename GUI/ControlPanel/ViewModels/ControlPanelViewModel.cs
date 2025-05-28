using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using Lab3.Models.Enums;

namespace ControlPanel.ViewModels
{
    public partial class ControlPanelViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _serverIp = "127.0.0.1";
        [ObservableProperty]
        private int _serverPort = 8888;
        [ObservableProperty]
        private string _connectionStatus = "Отключено";
        [ObservableProperty]
        private bool _isConnected = false;

        public ObservableCollection<EquipmentButtonViewModel> EquipmentButtons { get; }

        private TcpClient? _client;
        private CancellationTokenSource? _cancellationTokenSource;

        public ControlPanelViewModel()
        {
            EquipmentButtons = new ObservableCollection<EquipmentButtonViewModel>();
        }

        [RelayCommand]
        private async Task ConnectToServer()
        {
            if (IsConnected) return;

            _client = new TcpClient();
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                ConnectionStatus = "Подключение...";
                await _client.ConnectAsync(ServerIp, ServerPort);
                IsConnected = true;
                ConnectionStatus = "Подключено к контроллеру";
                Console.WriteLine($"Подключено к контроллеру {ServerIp}:{ServerPort}");

                // Получаем поток для обмена данными
                var stream = _client.GetStream();
                byte[] buffer = new byte[256];
                int bytesRead;

                // 1. Ожидаем количество установок (INIT:X)
                bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, _cancellationTokenSource.Token);
                var initMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"Получено от контроллера: {initMessage}");

                if (initMessage.StartsWith("INIT:"))
                {
                    if (int.TryParse(initMessage.Substring(5), out int numberOfEquipment))
                    {
                        // Обновляем UI в основном потоке Avalonia
                        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                        {
                            EquipmentButtons.Clear();
                            for (int i = 0; i < numberOfEquipment; i++)
                            {
                                EquipmentButtons.Add(new EquipmentButtonViewModel(i + 1));
                            }
                        });
                        Console.WriteLine($"Создано {numberOfEquipment} кнопок.");

                        // Отправляем подтверждение (ACK)
                        var ackData = Encoding.UTF8.GetBytes("ACK");
                        await stream.WriteAsync(ackData, 0, ackData.Length);
                        Console.WriteLine("Отправлено ACK контроллеру.");

                        // 2. Начинаем слушать обновления состояний (STATES:...)
                        _ = ReceiveStatesLoop(_cancellationTokenSource.Token); // Запускаем цикл в фоновом режиме
                    }
                    else
                    {
                        Console.WriteLine("Неверный формат сообщения INIT.");
                        Disconnect();
                    }
                }
                else
                {
                    Console.WriteLine("Неожиданное сообщение от контроллера при инициализации.");
                    Disconnect();
                }
            }
            catch (SocketException ex)
            {
                ConnectionStatus = $"Ошибка подключения: {ex.Message}";
                Console.WriteLine($"Ошибка подключения: {ex.Message}");
                Disconnect();
            }
            catch (OperationCanceledException)
            {
                ConnectionStatus = "Подключение отменено.";
                Console.WriteLine("Подключение отменено.");
                Disconnect();
            }
            catch (Exception ex)
            {
                ConnectionStatus = $"Ошибка: {ex.Message}";
                Console.WriteLine($"Ошибка: {ex.Message}");
                Disconnect();
            }
        }

        private async Task ReceiveStatesLoop(CancellationToken cancellationToken)
        {
            var stream = _client?.GetStream()
                ?? throw new ArgumentNullException(nameof(_client));
            byte[] buffer = new byte[256];

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                    if (bytesRead == 0) // Соединение закрыто
                    {
                        Console.WriteLine("Контроллер закрыл соединение.");
                        Disconnect();
                        break;
                    }

                    var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    // Console.WriteLine($"Получено: {message}"); // Для отладки

                    if (message.StartsWith("STATES:"))
                    {
                        var stateValues = message.Substring(7).Split(',').Select(s => int.TryParse(s, out int val) ? (EquipmentState)val : EquipmentState.Working).ToList();

                        // Обновляем UI в основном потоке
                        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                        {
                            for (int i = 0; i < stateValues.Count && i < EquipmentButtons.Count; i++)
                            {
                                EquipmentButtons[i].State = stateValues[i];
                            }
                        });
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Прием данных отменен.");
            }
            catch (IOException ex) when (ex.InnerException is SocketException se && se.SocketErrorCode == SocketError.ConnectionReset)
            {
                Console.WriteLine("Соединение с контроллером было неожиданно разорвано.");
                Disconnect();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при приеме данных: {ex.Message}");
                Disconnect();
            }
        }

        [RelayCommand]
        private void Disconnect()
        {
            if (!IsConnected) return;

            _cancellationTokenSource?.Cancel();
            _client?.Close();
            _client?.Dispose();
            _client = null;
            _cancellationTokenSource = null;

            IsConnected = false;
            ConnectionStatus = "Отключено";
            Console.WriteLine("Отключено от контроллера.");
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                EquipmentButtons.Clear(); // Очищаем кнопки при отключении
            });
        }
    }
}