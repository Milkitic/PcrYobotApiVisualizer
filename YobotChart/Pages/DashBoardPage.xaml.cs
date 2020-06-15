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
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Animation;
using YobotChart.Shared.Annotations;
using YobotChart.Shared.Win32;
using YobotChart.UiComponents;

namespace YobotChart.Pages
{
    public class DashBoardPageVm : INotifyPropertyChanged
    {
        private double _maxWidth;
        private double _maxHeight;

        public ObservableCollection<TestBox> Collections { get; set; }
        = new ObservableCollection<TestBox>();

        public double MaxWidth
        {
            get => _maxWidth;
            set
            {
                if (value.Equals(_maxWidth)) return;
                _maxWidth = value;
                OnPropertyChanged();
            }
        }

        public double MaxHeight
        {
            get => _maxHeight;
            set
            {
                if (value.Equals(_maxHeight)) return;
                _maxHeight = value;
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

    public class TestBox : INotifyPropertyChanged
    {
        private double _x;
        private double _y;
        private double _width;
        private double _height;
        private double _opacity = 1;
        private int _zIndex;

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

        public double Width
        {
            get => _width;
            set
            {
                if (value.Equals(_width)) return;
                _width = value;
                OnPropertyChanged();
            }
        }

        public double Height
        {
            get => _height;
            set
            {
                if (value.Equals(_height)) return;
                _height = value;
                OnPropertyChanged();
            }
        }

        public double Opacity
        {
            get => _opacity;
            set
            {
                if (value.Equals(_opacity)) return;
                _opacity = value;
                OnPropertyChanged();
            }
        }

        public int ZIndex
        {
            get => _zIndex;
            set
            {
                if (value == _zIndex) return;
                _zIndex = value;
                OnPropertyChanged();
            }
        }

        public int PointX { get; set; }
        public int PointY { get; set; }
        public int PointScaleX { get; set; }
        public int PointScaleY { get; set; }

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
        private (bool flag, int sourceX, int sourceY)[,] _m;
        private int _xLen;
        private int _yLen;
        private int _columnCount;

        public DashBoardPage()
        {
            InitializeComponent();
            _viewModel = (DashBoardPageVm)DataContext;
        }

        private const int UnitX = 350;
        private const int UnitY = 270;

        public void AppendItem(int scaleX, int scaleY)
        {
            var matrix = GetMatrix(out int xLen, out int yLen, out int columnCount);

            var @break = false;
            TestBox testBox = null;
            for (int y = 0; y < yLen; y++)
            {
                for (int x = 0; x < xLen; x++)
                {
                    if (matrix[x, y].flag) continue;
                    bool innerBreak = false;
                    for (int i = 0; i < scaleX; i++)
                    {
                        for (int j = 0; j < scaleY; j++)
                        {
                            if (x + i >= xLen)
                            {
                                innerBreak = true;
                                break;
                            }

                            if (x + i < xLen && y + j < yLen && matrix[x + i, y + j].flag)
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
                        testBox = new TestBox { PointX = x, PointY = y, PointScaleX = scaleX, PointScaleY = scaleY };
                        break;
                    }
                }

                if (@break) break;
            }

            if (testBox != null)
            {
                testBox.X = testBox.PointX * UnitX;
                testBox.Y = testBox.PointY * UnitY;
                testBox.Width = UnitX * scaleX;
                testBox.Height = UnitY * scaleY;
                _viewModel.Collections.Add(testBox);
            }

            matrix = GetMatrix(out xLen, out yLen, out columnCount);
            _viewModel.MaxWidth = UnitX * Math.Max((xLen - 1), columnCount);
            _viewModel.MaxHeight = UnitY * (yLen - 1);
        }

        private (bool flag, int sourceX, int sourceY)[,] GetMatrix(out int xLen, out int yLen, out int columnCount)
        {
            var w = ViewArea.ActualWidth - Container.Padding.Left - Container.Padding.Right;
            var h = ViewArea.ActualHeight - Container.Padding.Top - Container.Padding.Bottom;
            Console.WriteLine($"Canvas: {w}*{h}");
            columnCount = (int)(w / UnitX);
            Console.WriteLine($"Column count: " + columnCount);

            var collections = _viewModel.Collections;
            var maxX = collections.Count == 0 ? 0 : collections.Max(k => k.PointX + k.PointScaleX - 1);
            var maxY = collections.Count == 0 ? 0 : collections.Max(k => k.PointY + k.PointScaleY - 1);
            var matrix = new (bool, int, int)[Math.Max(columnCount, maxX + 1), maxY + 2];

            foreach (var collection in collections)
            {
                for (int i = 0; i < collection.PointScaleX; i++)
                {
                    for (int j = 0; j < collection.PointScaleY; j++)
                    {
                        matrix[collection.PointX + i, collection.PointY + j] = (true, collection.PointX, collection.PointY);
                    }
                }
            }

            yLen = matrix.GetLength(1);
            xLen = matrix.GetLength(0);
            return matrix;
        }

        private void Thumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            var thumb = (Thumb)sender;
            var testBox = (TestBox)thumb.Tag;

            testBox.Opacity = 0.5;
            testBox.ZIndex = 1;
            thumb.Cursor = Cursors.SizeAll;

            _m = GetMatrix(out _xLen, out _yLen, out _columnCount);
            _viewModel.MaxHeight = UnitY * (_yLen);
        }

        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var thumb = (Thumb)sender;
            var testBox = (TestBox)thumb.Tag;

            var preX = testBox.X;
            var preY = testBox.Y;
            double dx = e.HorizontalChange + preX;
            double dy = e.VerticalChange + preY;
            testBox.X = dx;
            testBox.Y = dy;
        }

        private void Thumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            var thumb = (Thumb)sender;
            var testBox = (TestBox)thumb.Tag;

            testBox.Opacity = 1;
            thumb.Cursor = Cursors.Arrow;
            var centerX = testBox.X + testBox.Width / (2 * testBox.PointScaleX);
            var centerY = testBox.Y + testBox.Height / (2 * testBox.PointScaleY);
            var newPointX = (int)(centerX / UnitX);
            var newPointY = (int)(centerY / UnitY);
            Console.WriteLine($"new point: {newPointX},{newPointY}");
            var matrix = _m;

            bool change = true;
            bool innerBreak = false;
            for (int i = 0; i < testBox.PointScaleX; i++)
            {
                for (int j = 0; j < testBox.PointScaleY; j++)
                {
                    if (newPointX + i < _xLen && newPointY + j < _yLen && matrix[newPointX + i, newPointY + j].flag)
                    {
                        if (matrix[newPointX + i, newPointY + j].sourceX != testBox.PointX ||
                            matrix[newPointX + i, newPointY + j].sourceY != testBox.PointY)
                        {
                            innerBreak = true;
                            change = false;
                            break;
                        }
                    }
                }

                if (innerBreak) break;
            }

            if (change)
            {
                testBox.PointX = newPointX;
                testBox.PointY = newPointY;
            }

            _m = null;
            matrix = GetMatrix(out int xLen, out int yLen, out int columnCount);
            _viewModel.MaxHeight = UnitY * (yLen - 1);

            Task.Run(() =>
            {
                var sw = Stopwatch.StartNew();
                var duration = TimeSpan.FromMilliseconds(700);

                var oldX = testBox.X;
                var oldY = testBox.Y;

                var newX = testBox.PointX * UnitX;
                var newY = testBox.PointY * UnitY;
                while (sw.Elapsed < duration)
                {
                    var easing = new QuinticEase() { EasingMode = EasingMode.EaseOut };
                    var ratio = easing.Ease(sw.ElapsedMilliseconds / duration.TotalMilliseconds);
                    Execute.OnUiThread(() =>
                    {
                        testBox.X = oldX + (newX - oldX) * ratio;
                        testBox.Y = oldY + (newY - oldY) * ratio;
                    });
                }

                Execute.OnUiThread(() =>
                {
                    testBox.X = newX;
                    testBox.Y = newY;
                    testBox.ZIndex = 0;
                });

                sw.Stop();
            });
        }

        private void AddScale1_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            AppendItem(1, 1);
        }

        private void AddScale2_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            AppendItem(2, 2);
        }

        private void AddScale3_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            AppendItem(3, 2);
        }

        private void AddScale4_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            AppendItem(4, 1);
        }

        private void BtnAdd_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            AnimatedFrame.Default?.AnimateNavigate(SingletonPageHelper.Get<SelectTemplatePage>());
        }
    }
}
