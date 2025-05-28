using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using ScottPlot;
using ScottPlot.Avalonia;
using ScottPlot.Plottables; 
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProcessDispatcher.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private TcpListener? _tcpListener;
    private CancellationTokenSource? _cancellationTokenSource;
    private readonly int _port = 8888;

    private readonly List<double> _timeData = new List<double>(); 
    private readonly List<double> _temperatureData = new List<double>();
    private readonly List<double> _pressureData = new List<double>();
    private double _currentTime = 0;

    private Signal? _tempSignalPlot;
    private Signal? _pressureSignalPlot;
    private Scatter? _pressureVsTempScatterPlot;

    [ObservableProperty]
    private AvaPlot _temperaturePlot = new AvaPlot();

    [ObservableProperty]
    private AvaPlot _pressurePlot = new AvaPlot();

    [ObservableProperty]
    private AvaPlot _pressureVsTemperaturePlot = new AvaPlot();

    [ObservableProperty]
    private string _statusMessage = "Ожидание данных от контроллера...";

    [ObservableProperty]
    private string _currentTemperature = "N/A";

    [ObservableProperty]
    private string _currentPressure = "N/A";

    public MainWindowViewModel()
    {
        InitializePlots();
        StartListening();
    }

    private void InitializePlots()
    {
        // Temperature vs. Time
        TemperaturePlot.Plot.Title("Температура (°C) от Времени (с)");
        TemperaturePlot.Plot.XLabel("Время (с)");
        TemperaturePlot.Plot.YLabel("Температура (°C)");
        // Initialize Signal plot with the data list
        _tempSignalPlot = TemperaturePlot.Plot.Add.Signal(_temperatureData); 
        _tempSignalPlot.Color = Colors.Red;
        TemperaturePlot.Plot.Axes.SetLimits(0, 60, 0, 1000); // Initial view window
        TemperaturePlot.Refresh();

        // Pressure vs. Time
        PressurePlot.Plot.Title("Давление (атм) от Времени (с)");
        PressurePlot.Plot.XLabel("Время (с)");
        PressurePlot.Plot.YLabel("Давление (атм)");
        // Initialize Signal plot with the data list
        _pressureSignalPlot = PressurePlot.Plot.Add.Signal(_pressureData); 
        _pressureSignalPlot.Color = Colors.Blue;
        PressurePlot.Plot.Axes.SetLimits(0, 60, 0, 6);
        PressurePlot.Refresh();

        // Pressure vs. Temperature
        PressureVsTemperaturePlot.Plot.Title("Давление (атм) от Температуры (°C)");
        PressureVsTemperaturePlot.Plot.XLabel("Температура (°C)");
        PressureVsTemperaturePlot.Plot.YLabel("Давление (атм)");
        // Initialize Scatter plot with both data lists
        _pressureVsTempScatterPlot = PressureVsTemperaturePlot.Plot.Add.Scatter(_temperatureData, _pressureData); 
        _pressureVsTempScatterPlot.Color = Colors.Green;
        _pressureVsTempScatterPlot.MarkerStyle.Size = 3;
        PressureVsTemperaturePlot.Plot.Axes.SetLimits(0, 1000, 0, 6);
        PressureVsTemperaturePlot.Refresh();
    }

    public void StartListening()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        _tcpListener = new TcpListener(IPAddress.Any, _port);
        Task.Run(async () => await ListenForConnections(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
        StatusMessage = $"Прослушивание порта {_port}...";
    }

    private async Task ListenForConnections(CancellationToken token)
    {
        try
        {
            _tcpListener!.Start();
            while (!token.IsCancellationRequested)
            {
                StatusMessage = $"Ожидание подключения контроллера на порту {_port}...";
                TcpClient client = await _tcpListener.AcceptTcpClientAsync(token);
                StatusMessage = $"Контроллер подключен: {client.Client.RemoteEndPoint}";
                _ = HandleClient(client, token);
            }
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Прослушивание остановлено.";
        }
        catch (SocketException ex)
        {
            StatusMessage = $"Ошибка сети TcpListener: {ex.Message}";
            Debug.WriteLine($"TcpListener SocketError: {ex}");
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка TcpListener: {ex.Message}";
            Debug.WriteLine($"TcpListener Error: {ex}");
        }
        finally
        {
            _tcpListener?.Stop();
        }
    }

    private async Task HandleClient(TcpClient client, CancellationToken token)
    {
        try
        {
            using (client)
            using (var stream = client.GetStream())
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                while (!token.IsCancellationRequested && client.Connected)
                {
                    string? line = await reader.ReadLineAsync(token);
                    if (line == null)
                    {
                        Debug.WriteLine("Client disconnected (ReadLineAsync returned null).");
                        break;
                    }
                    ProcessData(line);
                }
            }
        }
        catch (OperationCanceledException)
        {
            Debug.WriteLine("HandleClient: OperationCanceledException.");
        }
        catch (IOException ex)
        {
            StatusMessage = $"Ошибка чтения данных (контроллер мог отключиться): {ex.Message}";
            Debug.WriteLine($"IO Error with client: {ex.Message}");
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка при обработке клиента: {ex.Message}";
            Debug.WriteLine($"Error handling client: {ex}");
        }
        finally
        {
            StatusMessage = "Контроллер отключился.";
            Debug.WriteLine($"Client {client.Client.RemoteEndPoint} disconnected or handler finished.");
        }
    }

    private void ProcessData(string data)
    {
        var parts = data.Split(';');
        if (parts.Length == 2 &&
            double.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out double temperature) &&
            double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out double pressure))
        {
            Dispatcher.UIThread.Post(() =>
            {
                _currentTime += 1.0;

                _timeData.Add(_currentTime);
                _temperatureData.Add(temperature);
                _pressureData.Add(pressure);

                CurrentTemperature = $"{temperature:F2} °C";
                CurrentPressure = $"{pressure:F2} атм";
                StatusMessage = $"Данные получены: T={temperature:F2}, P={pressure:F2}";

                // No need to call .Update() for SignalPlot or ScatterPlot in ScottPlot 5
                // They reference the underlying lists, so just modifying the lists and refreshing is enough.

                double displayWindow = 60.0;
                double newXMin = Math.Max(0, _currentTime - displayWindow);
                double newXMax = _currentTime;

                TemperaturePlot.Plot.Axes.SetLimitsX(newXMin, newXMax);
                PressurePlot.Plot.Axes.SetLimitsX(newXMin, newXMax);

                TemperaturePlot.Plot.Axes.AutoScaleY();
                PressurePlot.Plot.Axes.AutoScaleY();
                PressureVsTemperaturePlot.Plot.Axes.AutoScale(); 
                
                TemperaturePlot.Refresh();
                PressurePlot.Refresh();
                PressureVsTemperaturePlot.Refresh();

                const int maxPoints = 1000;
                if (_timeData.Count > maxPoints)
                {
                    _timeData.RemoveAt(0);
                    _temperatureData.RemoveAt(0);
                    _pressureData.RemoveAt(0);
                }
            });
        }
        else
        {
            StatusMessage = $"Ошибка парсинга данных: {data}";
            Debug.WriteLine($"Parse error: {data}");
        }
    }

    public void Cleanup()
    {
        StatusMessage = "Остановка служб...";
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _tcpListener?.Stop();
        Debug.WriteLine("Cleanup called, listener stopped, token cancelled.");
    }
}