using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using YobotChart.Shared.Annotations;

namespace YobotChart.UiComponents.NotificationComponent
{
    public class NotificationOption : INotifyPropertyChanged
    {
        #region Notify property

        private ControlTemplate _iconTemplate;
        private string _title = "Title";
        private string _content = "This is your content here";
        private TimeSpan _fadeoutTime;
        private NotificationType _type;
        private NotificationLevel _level;
        private bool _closeExplicitly;

        public ControlTemplate IconTemplate
        {
            get => _iconTemplate;
            set
            {
                if (value == _iconTemplate) return;
                _iconTemplate = value;
                OnPropertyChanged();
            }
        }

        public string Title
        {
            get => _title;
            set
            {
                if (value == _title) return;
                _title = value;
                OnPropertyChanged();
            }
        }

        public string Content
        {
            get => _content;
            set
            {
                if (value == _content) return;
                _content = value;
                OnPropertyChanged();
            }
        }

        public TimeSpan FadeoutTime
        {
            get => _fadeoutTime;
            set
            {
                if (value == _fadeoutTime) return;
                _fadeoutTime = value;
                OnPropertyChanged();
            }
        }

        public NotificationType Type
        {
            get => _type;
            set
            {
                if (value == _type) return;
                _type = value;
                OnPropertyChanged();
            }
        }

        public NotificationLevel Level
        {
            get => _level;
            set
            {
                if (value == _level) return;
                _level = value;
                OnPropertyChanged();
            }
        }

        public bool CloseExplicitly
        {
            get => _closeExplicitly;
            set
            {
                if (value == _closeExplicitly) return;
                _closeExplicitly = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        public string NotificationTypeString => Type.ToString();

        public bool IsEmpty => string.IsNullOrEmpty(Title) && string.IsNullOrEmpty(Content) && IconTemplate == null;

        public Action YesCallback { get; set; }
        public Action NoCallback { get; set; }
    }
}