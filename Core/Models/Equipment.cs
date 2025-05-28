using Lab3.Models.Enums;

namespace Lab3.Models;
public class Equipment
{
    private Random _random;
    public int Id { get; }
    public EquipmentState State { get; private set; }

    public Equipment(int id)
    {
        Id = id;
        State = EquipmentState.Working;
        _random = new Random();
    }

    public void SimulateStateChange()
    {
        switch (State)
        {
            case EquipmentState.Working:
                if (_random.NextDouble() < 0.2)
                {
                    State = EquipmentState.Fault;
                    Console.WriteLine($"Установка {Id} перешла в состояние АВАРИЯ.");
                }
                else
                {
                    State = EquipmentState.Working;
                }
                break;
            case EquipmentState.Fault:
                State = EquipmentState.Repair;
                Console.WriteLine($"Установка {Id} перешла в состояние РЕМОНТ.");
                break;
            case EquipmentState.Repair:
                if (_random.NextDouble() < 0.5)
                {
                    State = EquipmentState.Working;
                    Console.WriteLine($"Установка {Id} восстановлена, перешла в состояние РАБОТАЕТ.");
                }
                break;
        }
    }
}