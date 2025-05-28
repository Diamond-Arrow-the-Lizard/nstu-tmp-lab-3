using System;
using System.Threading.Tasks;
using Lab3.Models;

namespace EquipmentMonitor;

    class Program
    {
        static async Task Main(string[] args)
        {
            var simulator = new EquipmentSimulator();
            await simulator.StartAsync();

            Console.WriteLine("Нажмите любую клавишу для остановки контроллера...");
            Console.ReadKey();
            simulator.Stop();
        }
    }
