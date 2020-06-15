using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using YamlDotNet.Comment;
using YamlDotNet.Serialization;
using YobotChart.Shared.Annotations;
using YobotChart.Shared.Configuration;
using YobotChart.Shared.Win32.ChartFramework;

namespace YobotChart.Shared.Win32
{
    public class SharedVm : INotifyPropertyChanged
    {
     

        private static SharedVm _default;
        private static object _defaultLock = new object();

        public static SharedVm Default
        {
            get
            {
                lock (_defaultLock)
                {
                    return _default ?? (_default = new SharedVm());
                }
            }
        }

        private SharedVm()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}