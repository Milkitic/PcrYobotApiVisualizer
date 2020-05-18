﻿using System.ComponentModel;
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
        private bool _isLoading;

        /// <summary>
        /// 数据源
        /// </summary>
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

        /// <summary>
        /// 图表的图相关上下文
        /// </summary>
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
        /// 已选周目
        /// </summary>
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

        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}