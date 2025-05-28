using CommunityToolkit.Mvvm.ComponentModel;
using Avalonia.Media;
using Lab3.Models.Enums;

namespace ControlPanel.ViewModels
{
    public partial class EquipmentButtonViewModel : ObservableObject
    {
        [ObservableProperty]
        private int _id;

        [ObservableProperty]
        private EquipmentState _state;

        public IBrush StateColor
        {
            get
            {
                return State switch
                {
                    EquipmentState.Working => Brushes.Green,
                    EquipmentState.Fault => Brushes.Red,
                    EquipmentState.Repair => Brushes.Gray,
                    _ => Brushes.LightGray 
                };
            }
        }

        partial void OnStateChanged(EquipmentState value)
        {
            OnPropertyChanged(nameof(StateColor));
        }

        public EquipmentButtonViewModel(int id)
        {
            Id = id;
            State = EquipmentState.Working; 
        }
    }
}