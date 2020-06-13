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

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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
        private double _l;

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

        public double L
        {
            get => _l;
            set
            {
                if (value.Equals(_l)) return;
                _l = value;
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
        private DashBoardPageVm _viewModel;

        public DashBoardPage()
        {
            InitializeComponent();
            _viewModel = (DashBoardPageVm)DataContext;
        }

        private const int Unit = 300;

        public void AppendItem(int scale)
        {
            var w = Container.ActualWidth;
            var h = Container.ActualHeight;
            Console.WriteLine($"Canvas: {w}*{h}");
            var columnCount = (int)(w / 300);
            Console.WriteLine($"Column count: " + columnCount);

            var collections = _viewModel.Collections;
            var maxX = collections.Count == 0 ? 0 : collections.Max(k => k.PointX + k.PointScale - 1);
            var maxY = collections.Count == 0 ? 0 : collections.Max(k => k.PointY + k.PointScale - 1);
            var matrix = new bool[columnCount, maxY + 2];

            foreach (var collection in collections)
            {
                for (int i = 0; i < collection.PointScale; i++)
                {
                    for (int j = 0; j < collection.PointScale; j++)
                    {
                        matrix[collection.PointX + i, collection.PointY + j] = true;
                    }
                }
            }

            var @break = false;
            TestBox testBox = null;
            var yLen = matrix.GetLength(1);
            var xLen = matrix.GetLength(0);
            for (int y = 0; y < yLen; y++)
            {
                for (int x = 0; x < xLen; x++)
                {
                    if (matrix[x, y]) continue;
                    bool innerBreak = false;
                    for (int i = 0; i < scale; i++)
                    {
                        for (int j = 0; j < scale; j++)
                        {
                            if (x + i < xLen && y + j < yLen && matrix[x + i, y + j])
                            {
                                innerBreak = true;
                                break;
                            }
                        }

                        if (innerBreak) break;
                    }

                    if (innerBreak) continue;
                    else
                    {
                        @break = true;
                        testBox = new TestBox { PointX = x, PointY = y, PointScale = scale };
                        break;
                    }
                }

                if (@break) break;
            }

            if (testBox != null)
            {
                testBox.X = testBox.PointX * Unit;
                testBox.Y = testBox.PointY * Unit;
                testBox.L = 300 * scale - 20;
                _viewModel.Collections.Add(testBox);
            }
        }

        private void AddScale1_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            AppendItem(1);
        }

        private void AddScale2_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            AppendItem(2);
        }

        private void AddScale3_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            AppendItem(3);
        }

        private void AddScale4_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            AppendItem(4);
        }
    }
}
