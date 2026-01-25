using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVIInspection.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private object? _currentView;
        [ObservableProperty]
        private int _machineNumber = 1;

        public MainViewModel()
        {
            MachineNumber = 22;
        }

        public void NavigateToView(object view)
        {
            CurrentView = view;
        }
    }
}
