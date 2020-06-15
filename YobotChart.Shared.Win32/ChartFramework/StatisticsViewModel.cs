using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using YobotChart.Shared.Win32.Annotations;
using YobotChart.Shared.Win32.ChartFramework.ConfigModels;

namespace YobotChart.Shared.Win32.ChartFramework
{
    public class StatisticsViewModel : INotifyPropertyChanged
    {
        private int _cycleCount;

        private int? _selectedCycle = -1;
        private DateTime? _selectedDate;

        private bool _isLoading;
        private List<DateTime> _dateList;
        private IChartConfigModel _configModel;

        public Guid StatsProviderGuid { get; set; }
        public string StatsProviderMethodName { get; set; }

        /// <summary>
        /// 周目数
        /// </summary>
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

        /// <summary>
        /// 周目数
        /// </summary>
        public List<DateTime> DateList
        {
            get => _dateList;
            set
            {
                if (Equals(value, _dateList)) return;
                _dateList = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 已选周目
        /// </summary>
        public int? SelectedCycle
        {
            get => _selectedCycle;
            set
            {
                if (value == _selectedCycle) return;
                _selectedCycle = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 已选日期
        /// </summary>
        public DateTime? SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (value.Equals(_selectedDate)) return;
                _selectedDate = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 数据是否正在准备，即图表是否正在加载
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (value == _isLoading) return;
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public IChartConfigModel ConfigModel
        {
            get => _configModel;
            set
            {
                if (Equals(value, _configModel)) return;
                _configModel = value;
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