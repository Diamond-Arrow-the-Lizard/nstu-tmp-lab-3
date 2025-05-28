using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProcessController
{
    class ControllerProgram
    {
        private static readonly int ServerPort = 8888; // Порт диспетчера
        private static readonly Random RandomGenerator = new Random();
        private static readonly double MinTemperature = 0.0;
        private static readonly double MaxTemperature = 1000.0;
        private static readonly double MinPressure = 0.0;
        private static readonly double MaxPressure = 6.0;
        private static readonly TimeSpan SendInterval = TimeSpan.FromSeconds(1);

        private static string? ServerAddress = ""; // IP-адрес диспетчера

        static async Task Main(string[] args)
        {
            Console.WriteLine("Введите IP адрес диспетчера (нажмите Enter для localhost)");
            ServerAddress = Console.ReadLine(); 

            if(string.IsNullOrEmpty(ServerAddress))
            {
                ServerAddress = "127.0.0.1";
            }

            Console.WriteLine("Контроллер технологического процесса запущен.");
            Console.WriteLine($"Попытка подключения к диспетчеру по адресу {ServerAddress}:{ServerPort}...");

            while (true) // Бесконечный цикл для попыток переподключения
            {
                TcpClient? client = null;
                NetworkStream? stream = null;

                try
                {
                    client = new TcpClient();
                    await client.ConnectAsync(ServerAddress, ServerPort);
                    stream = client.GetStream();
                    Console.WriteLine("Соединение с диспетчером установлено.");

                    // Цикл генерации и отправки данных
                    while (client.Connected)
                    {
                        double temperature = MinTemperature + (RandomGenerator.NextDouble() * (MaxTemperature - MinTemperature));
                        double pressure = MinPressure + (RandomGenerator.NextDouble() * (MaxPressure - MinPressure));

                        string dataToSend = $"{temperature:F2};{pressure:F2}"; // Форматируем до 2 знаков после запятой
                        byte[] buffer = Encoding.UTF8.GetBytes(dataToSend + Environment.NewLine); // Добавляем NewLine как разделитель сообщений

                        await stream.WriteAsync(buffer, 0, buffer.Length);
                        Console.WriteLine($"Отправлено: Температура = {temperature:F2}°C, Давление = {pressure:F2} атм");

                        await Task.Delay(SendInterval);
                    }
                }
                catch (SocketException ex)
                {
                    Console.WriteLine($"Ошибка подключения или отправки: {ex.Message}. Попытка переподключения через 5 секунд...");
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"Ошибка ввода-вывода (возможно, соединение разорвано): {ex.Message}. Попытка переподключения через 5 секунд...");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Непредвиденная ошибка: {ex.Message}. Попытка переподключения через 5 секунд...");
                }
                finally
                {
                    stream?.Close();
                    client?.Close();
                    Console.WriteLine("Соединение с диспетчером разорвано или не удалось установить.");
                }

                // Пауза перед следующей попыткой подключения
                await Task.Delay(TimeSpan.FromSeconds(5));
                Console.WriteLine("Повторная попытка подключения...");
            }
        }
    }
}
