using System.ComponentModel;
using System.Runtime.CompilerServices;
using PcrYobotExtension.Annotations;
using PcrYobotExtension.Models;

namespace PcrYobotExtension.ViewModels
{
    public class StatsVm : INotifyPropertyChanged
    {
        private YobotApiModel _apiObj;

        private int _cycleCount;

        private int _selectedCycle = -1;
        private StatsGraphVm _statsGraph;


        public YobotApiModel ApiObj
        {
            get => _apiObj;
            set
            {
                if (Equals(value, _apiObj)) return;
                _apiObj = value;
                OnPropertyChanged();
            }
        }

        public StatsGraphVm StatsGraph
        {
            get => _statsGraph;
            set
            {
                if (Equals(value, _statsGraph)) return;
                _statsGraph = value;
                OnPropertyChanged();
            }
        }

        public int CycleCount
        {
            get => _cycleCount;
            set
            {
                if (value == _cycleCount) return;
                _cycleCount = value;
                OnPropertyChanged();
            }
        }

        public int SelectedCycle
        {
            get => _selectedCycle;
            set
            {
                if (value == _selectedCycle) return;
                _selectedCycle = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}