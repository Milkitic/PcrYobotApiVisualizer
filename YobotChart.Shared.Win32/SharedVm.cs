using System.ComponentModel;
using System.Runtime.CompilerServices;
using YobotChart.Shared.Annotations;

namespace YobotChart.Shared.Win32
{
    public class SharedVm : INotifyPropertyChanged
    {
        private bool _isUpdatingData;
        private User _loginUser;

        public bool IsUpdatingData
        {
            get => _isUpdatingData;
            set
            {
                if (value == _isUpdatingData) return;
                _isUpdatingData = value;
                OnPropertyChanged();
            }
        }

        public User LoginUser
        {
            get => _loginUser;
            set
            {
                if (Equals(value, _loginUser)) return;
                _loginUser = value;
                OnPropertyChanged();
            }
        }

        private static SharedVm _default;
        private static object _defaultLock = new object();

        public static SharedVm Default
        {
            get
            {
                lock (_defaultLock) return _default ?? (_default = new SharedVm());
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

    public class User : INotifyPropertyChanged
    {
        private string _avatarUri;
        private long _qq;
        private string _qqNick;
        private long _clanId;
        private string _clanNick;

        //http://q1.qlogo.cn/g?b=qq&amp;nk=2241521134&amp;s=640
        public string AvatarUri
        {
            get => _avatarUri;
            set
            {
                if (value == _avatarUri) return;
                _avatarUri = value;
                OnPropertyChanged();
            }
        }

        public long QQ
        {
            get => _qq;
            set
            {
                if (value == _qq) return;
                _qq = value;
                OnPropertyChanged();
            }
        }

        public string QQNick
        {
            get => _qqNick;
            set
            {
                if (value == _qqNick) return;
                _qqNick = value;
                OnPropertyChanged();
            }
        }

        public long ClanId
        {
            get => _clanId;
            set
            {
                if (value == _clanId) return;
                _clanId = value;
                OnPropertyChanged();
            }
        }

        public string ClanNick
        {
            get => _clanNick;
            set
            {
                if (value == _clanNick) return;
                _clanNick = value;
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