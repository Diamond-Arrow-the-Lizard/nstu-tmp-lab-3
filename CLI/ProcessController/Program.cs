using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProcessController
{
    class ControllerProgram
    {
        private static readonly int ServerPort = 8888; // Dispatcher port
        private static readonly Random RandomGenerator = new Random();
        private static readonly double MinTemperature = 0.0;
        private static readonly double MaxTemperature = 1000.0;
        private static readonly double MinPressure = 0.0;
        private static readonly double MaxPressure = 6.0;
        private static readonly TimeSpan SendInterval = TimeSpan.FromSeconds(1);

        private static string? ServerAddress = ""; // Dispatcher IP address

        static async Task Main(string[] args)
        {
            Console.WriteLine("Enter the dispatcher's IP address (press Enter for localhost)");
            ServerAddress = Console.ReadLine(); 

            if(string.IsNullOrEmpty(ServerAddress))
            {
                ServerAddress = "127.0.0.1";
            }

            Console.WriteLine("Process controller started.");
            Console.WriteLine($"Attempting to connect to dispatcher at {ServerAddress}:{ServerPort}...");

            while (true) // Infinite loop for reconnect attempts
            {
                TcpClient? client = null;
                NetworkStream? stream = null;

                try
                {
                    client = new TcpClient();
                    await client.ConnectAsync(ServerAddress, ServerPort);
                    stream = client.GetStream();
                    Console.WriteLine("Connection with dispatcher established.");

                    // Loop for generating and sending data
                    while (client.Connected)
                    {
                        double temperature = MinTemperature + (RandomGenerator.NextDouble() * (MaxTemperature - MinTemperature));
                        double pressure = MinPressure + (RandomGenerator.NextDouble() * (MaxPressure - MinPressure));

                        string dataToSend = $"{temperature:F2};{pressure:F2}"; // Format to 2 decimal places
                        byte[] buffer = Encoding.UTF8.GetBytes(dataToSend + Environment.NewLine); // Add NewLine as message delimiter

                        await stream.WriteAsync(buffer, 0, buffer.Length);
                        Console.WriteLine($"Sent: Temperature = {temperature:F2}°C, Pressure = {pressure:F2} atm");

                        await Task.Delay(SendInterval);
                    }
                }
                catch (SocketException ex)
                {
                    Console.WriteLine($"Connection or send error: {ex.Message}. Retrying in 5 seconds...");
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"I/O error (connection possibly broken): {ex.Message}. Retrying in 5 seconds...");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected error: {ex.Message}. Retrying in 5 seconds...");
                }
                finally
                {
                    stream?.Close();
                    client?.Close();
                    Console.WriteLine("Connection with dispatcher terminated or failed to establish.");
                }

                // Pause before next connection attempt
                await Task.Delay(TimeSpan.FromSeconds(5));
                Console.WriteLine("Retrying connection...");
            }
        }
    }
}