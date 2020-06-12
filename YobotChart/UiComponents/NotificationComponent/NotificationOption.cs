using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

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

        public ControlTemplate IconTemplate
        {
            get => _iconTemplate;
            set
            {
                _iconTemplate = value;
                OnPropertyChanged();
            }
        }

        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }

        public string Content
        {
            get => _content;
            set
            {
                _content = value;
                OnPropertyChanged();
            }
        }

        public TimeSpan FadeoutTime
        {
            get => _fadeoutTime;
            set
            {
                _fadeoutTime = value;
                OnPropertyChanged();
            }
        }

        public NotificationType Type
        {
            get => _type;
            set
            {
                _type = value;
                OnPropertyChanged();
            }
        }

        public NotificationLevel Level
        {
            get => _level;
            set
            {
                _level = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        public string NotificationTypeString => Type.ToString();

        public bool IsEmpty => string.IsNullOrEmpty(Title) && string.IsNullOrEmpty(Content) && IconTemplate == null;

        public Action YesCallback { get; set; }
        public Action NoCallback { get; set; }
        public bool CloseExplicitly { get; set; }
    }
}