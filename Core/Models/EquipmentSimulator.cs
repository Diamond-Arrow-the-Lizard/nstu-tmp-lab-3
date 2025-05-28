using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lab3.Models;
public class EquipmentSimulator
{
    private const int Port = 8888;
    private const string ConfigFileName = "config.txt";
    private const int UpdateIntervalMs = 2000; // 2 секунды
    private int _numberOfEquipment;
    private List<Equipment> _equipmentList;
    private TcpListener? _tcpListener;
    private TcpClient? _client;
    private CancellationTokenSource _cancellationTokenSource;

    public EquipmentSimulator()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        _equipmentList = new List<Equipment>();
    }

    public async Task StartAsync()
    {
        Console.WriteLine("Запуск контроллера технологического процесса...");

        if (!LoadConfig())
        {
            Console.WriteLine("Ошибка загрузки конфигурации. Выход.");
            return;
        }

        InitializeEquipment();

        _tcpListener = new TcpListener(IPAddress.Any, Port);
        _tcpListener.Start();
        Console.WriteLine($"Контроллер запущен на порту {Port}. Ожидание подключения диспетчера...");

        try
        {
            _client = await _tcpListener.AcceptTcpClientAsync();
            Console.WriteLine("Диспетчер подключен!");

            await SendNumberOfEquipment();
            await WaitForDispatcherResponse();

            StartSimulationLoop(_cancellationTokenSource.Token);
            Console.WriteLine("Начата имитация состояний установок и передача данных.");

            // Keep the server running until manually stopped
            await Task.Delay(-1, _cancellationTokenSource.Token); // Waits indefinitely until cancellation
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Симуляция отменена.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка в работе контроллера: {ex.Message}");
        }
        finally
        {
            Stop();
        }
    }

    private bool LoadConfig()
    {
        if (File.Exists(ConfigFileName))
        {
            var lines = File.ReadAllLines(ConfigFileName);
            foreach (var line in lines)
            {
                if (line.StartsWith("NumberOfEquipment="))
                {
                    if (int.TryParse(line.Split('=')[1], out int count))
                    {
                        _numberOfEquipment = count;
                        Console.WriteLine($"Количество установок загружено из файла: {_numberOfEquipment}");
                        return true;
                    }
                }
            }
        }
        Console.WriteLine($"Файл конфигурации '{ConfigFileName}' не найден или имеет неверный формат. Создаем файл с 3 установками по умолчанию.");
        _numberOfEquipment = 3;
        File.WriteAllText(ConfigFileName, $"NumberOfEquipment={_numberOfEquipment}");
        return true;
    }

    private void InitializeEquipment()
    {
        for (int i = 0; i < _numberOfEquipment; i++)
        {
            _equipmentList.Add(new Equipment(i + 1)); // Нумерация установок с 1
        }
        Console.WriteLine($"Инициализировано {_numberOfEquipment} установок.");
    }

    private async Task SendNumberOfEquipment()
    {
        if (_client == null || !_client.Connected) return;

        var stream = _client.GetStream();
        var data = Encoding.UTF8.GetBytes($"INIT:{_numberOfEquipment}");
        await stream.WriteAsync(data, 0, data.Length);
        Console.WriteLine($"Отправлено количество установок диспетчеру: {_numberOfEquipment}");
    }

    private async Task WaitForDispatcherResponse()
    {
        if (_client == null || !_client.Connected) return;

        var stream = _client.GetStream();
        byte[] buffer = new byte[256];
        int bytesRead;

        Console.WriteLine("Ожидание ответа от диспетчера...");
        bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
        var response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

        if (response == "ACK")
        {
            Console.WriteLine("Получен ответ 'ACK' от диспетчера.");
        }
        else
        {
            Console.WriteLine($"Получен неожиданный ответ от диспетчера: {response}. Ожидался 'ACK'.");
        }
    }

    private async void StartSimulationLoop(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            foreach (var equipment in _equipmentList)
            {
                equipment.SimulateStateChange();
            }
            await SendEquipmentStates();
            await Task.Delay(UpdateIntervalMs, cancellationToken);
        }
    }

    private async Task SendEquipmentStates()
    {
        if (_client == null || !_client.Connected) return;

        try
        {
            var stream = _client.GetStream();
            var states = string.Join(",", _equipmentList.Select(e => $"{(int)e.State}"));
            var data = Encoding.UTF8.GetBytes($"STATES:{states}");
            await stream.WriteAsync(data, 0, data.Length);
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Ошибка при отправке состояний (Broken Pipe / Connection Reset): {ex.Message}");
            HandleClientDisconnect();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Общая ошибка при отправке состояний: {ex.Message}");
            HandleClientDisconnect();
        }

    }

    public void Stop()
    {
        _cancellationTokenSource.Cancel();
        Console.WriteLine("Остановка контроллера...");
        _client?.Close();
        _tcpListener?.Stop();
        Console.WriteLine("Контроллер остановлен.");
    }

    private void HandleClientDisconnect()
    {
        if (_client != null)
        {
            Console.WriteLine("Клиент отключен. Очистка ресурсов клиента.");
            _client.Close();
            _client.Dispose();
            _client = null;
        }
        Console.WriteLine("Ожидание нового подключения...");
    }
}
