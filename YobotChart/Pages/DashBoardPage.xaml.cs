// 首要想法：基于固定的宽度增加列数，比如以300px为单位，则
// < 300px  0
// < 600px  1
// < 900px  2
// < 1200px 3
// < 1500px 4
// < 1800px 5，
// 当处于刻度之间间时，基于当前刻度数，将每个单元做动态缩放，比如
// 当前为1024px，则使用3列，如果规定单元Margin为10px，则每个单元宽度为(1024-20*3)/3=321.33333
// 当前为600px，则使用2列，如果规定单元Margin为10px，则每个单元宽度为(600-20*2)/2=280

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using YobotChart.Annotations;

namespace YobotChart.Pages
{
    public class DashBoardPageVm : INotifyPropertyChanged
    {
        public ObservableCollection<TestBox> Collections { get; set; }
        = new ObservableCollection<TestBox>();

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class TestBox : INotifyPropertyChanged
    {
        private double _x;
        private double _y;

        public double X
        {
            get => _x;
            set
            {
                if (value.Equals(_x)) return;
                _x = value;
                OnPropertyChanged();
            }
        }

        public double Y
        {
            get => _y;
            set
            {
                if (value.Equals(_y)) return;
                _y = value;
                OnPropertyChanged();
            }
        }

        public int PointX { get; set; }
        public int PointY { get; set; }
        public int PointScale { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// DashBoardPage.xaml 的交互逻辑
    /// </summary>
    public partial class DashBoardPage : Page
    {
        public DashBoardPage()
        {
            InitializeComponent();
        }
    }
}
