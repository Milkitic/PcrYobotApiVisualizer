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
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Animation;
using YamlDotNet.Comment;
using YamlDotNet.Serialization;
using YobotChart.Shared.Annotations;
using YobotChart.Shared.Configuration;
using YobotChart.Shared.Win32;
using YobotChart.Shared.Win32.ChartFramework;
using YobotChart.UiComponents;

namespace YobotChart.Pages
{
    public class DashBoardPageVm : INotifyPropertyChanged
    {
        private double _maxWidth;
        private double _maxHeight;
        private ObservableCollection<StatsViewModel> _collections = new ObservableCollection<StatsViewModel>();

        public ObservableCollection<StatsViewModel> Collections
        {
            get => _collections;
            set
            {
                if (Equals(value, _collections)) return;
                _collections = value;
                OnPropertyChanged();
            }
        }

        public void LoadStatsViewModels()
        {
            if (File.Exists(AppSettings.Files.StatsFile))
            {
                var content = File.ReadAllText(AppSettings.Files.StatsFile);
                var builder = new YamlDotNet.Serialization.DeserializerBuilder();
                //builder.WithTagMapping("tag:yaml.org,2002:test", typeof(Test));
                var ymlDeserializer = builder.Build();
                Collections = ymlDeserializer.Deserialize<ObservableCollection<StatsViewModel>>(content) ??
                    new ObservableCollection<StatsViewModel>();
                foreach (var statsVm in Collections)
                {
                    statsVm.DashboardInfo.X = statsVm.DashboardInfo.PointX * DashboardInfo.UnitX;
                    statsVm.DashboardInfo.Y = statsVm.DashboardInfo.PointY * DashboardInfo.UnitY;
                    statsVm.DashboardInfo.Width = DashboardInfo.UnitX * statsVm.DashboardInfo.PointScaleX;
                    statsVm.DashboardInfo.Height = DashboardInfo.UnitY * statsVm.DashboardInfo.PointScaleY;
                }
            }
        }

        public void RemoveStatsViewModelAndSave(StatsViewModel statsVm)
        {
            Collections.Remove(statsVm);
            Save();
        }

        public void Save()
        {
            var converter = new SerializerBuilder()
                .WithTypeInspector(inner => new CommentGatheringTypeInspector(inner))
                .WithEmissionPhaseObjectGraphVisitor(args => new CommentsObjectGraphVisitor(args.InnerVisitor))
                .Build();
            //builder.WithTagMapping("tag:yaml.org,2002:test", typeof(Test));
            var content = converter.Serialize(Collections);
            File.WriteAllText(AppSettings.Files.StatsFile, content);
        }

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
        }

        private void Page_Initialized(object sender, EventArgs e)
        {
            _viewModel = (DashBoardPageVm)DataContext;
            _viewModel.LoadStatsViewModels();

            ResizeCanvas();
        }

        public void AddStatsViewModelAndSave(StatsViewModel statsVm)
        {
            var matrix = GetMatrix(out int xLen, out int yLen, out int columnCount);

            var @break = false;
            for (int y = 0; y < yLen; y++)
            {
                for (int x = 0; x < xLen; x++)
                {
                    if (matrix[x, y].flag) continue;
                    bool innerBreak = false;
                    for (int i = 0; i < statsVm.DashboardInfo.PointScaleX; i++)
                    {
                        for (int j = 0; j < statsVm.DashboardInfo.PointScaleY; j++)
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
                        statsVm.DashboardInfo.PointX = x;
                        statsVm.DashboardInfo.PointY = y;
                        break;
                    }
                }

                if (@break) break;
            }

            if (statsVm != null)
            {
                statsVm.DashboardInfo.X = statsVm.DashboardInfo.PointX * DashboardInfo.UnitX;
                statsVm.DashboardInfo.Y = statsVm.DashboardInfo.PointY * DashboardInfo.UnitY;
                statsVm.DashboardInfo.Width = DashboardInfo.UnitX * statsVm.DashboardInfo.PointScaleX;
                statsVm.DashboardInfo.Height = DashboardInfo.UnitY * statsVm.DashboardInfo.PointScaleY;
            }

            _viewModel.Collections.Add(statsVm);
            _viewModel.Save();

            ResizeCanvas();
        }

        private void BtnRemove_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var thumb = (Button)sender;
            var statsVm = (StatsViewModel)thumb.Tag;
            _viewModel.RemoveStatsViewModelAndSave(statsVm);

            ResizeCanvas();
        }

        private void Thumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            var thumb = (Thumb)sender;
            var statsVm = (StatsViewModel)thumb.Tag;
            var dashboardInfo = statsVm.DashboardInfo;

            dashboardInfo.Opacity = 0.5;
            dashboardInfo.ZIndex = 1;
            thumb.Cursor = Cursors.SizeAll;

            _m = GetMatrix(out _xLen, out _yLen, out _columnCount);
            _viewModel.MaxHeight = DashboardInfo.UnitY * (_yLen);
        }

        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var thumb = (Thumb)sender;
            var statsVm = (StatsViewModel)thumb.Tag;
            var dashboardInfo = statsVm.DashboardInfo;

            var preX = dashboardInfo.X;
            var preY = dashboardInfo.Y;
            double dx = e.HorizontalChange + preX;
            double dy = e.VerticalChange + preY;
            dashboardInfo.X = dx;
            dashboardInfo.Y = dy;
        }

        private void Thumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            var thumb = (Thumb)sender;
            var statsVm = (StatsViewModel)thumb.Tag;
            var dashboardInfo = statsVm.DashboardInfo;

            dashboardInfo.Opacity = 1;
            thumb.Cursor = Cursors.Arrow;
            var centerX = dashboardInfo.X + dashboardInfo.Width / (2 * dashboardInfo.PointScaleX);
            var centerY = dashboardInfo.Y + dashboardInfo.Height / (2 * dashboardInfo.PointScaleY);
            var newPointX = (int)(centerX / DashboardInfo.UnitX);
            var newPointY = (int)(centerY / DashboardInfo.UnitY);
            Console.WriteLine($"new point: {newPointX},{newPointY}");
            var matrix = _m;

            bool change = true;
            bool innerBreak = false;
            for (int i = 0; i < dashboardInfo.PointScaleX; i++)
            {
                for (int j = 0; j < dashboardInfo.PointScaleY; j++)
                {
                    if (newPointX + i < _xLen && newPointY + j < _yLen && matrix[newPointX + i, newPointY + j].flag)
                    {
                        if (matrix[newPointX + i, newPointY + j].sourceX != dashboardInfo.PointX ||
                            matrix[newPointX + i, newPointY + j].sourceY != dashboardInfo.PointY)
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
                dashboardInfo.PointX = newPointX;
                dashboardInfo.PointY = newPointY;
            }

            _m = null;
            matrix = GetMatrix(out int xLen, out int yLen, out int columnCount);
            _viewModel.MaxHeight = DashboardInfo.UnitY * (yLen - 1);

            Task.Run(() =>
            {
                var sw = Stopwatch.StartNew();
                var duration = TimeSpan.FromMilliseconds(700);

                var oldX = dashboardInfo.X;
                var oldY = dashboardInfo.Y;

                var newX = dashboardInfo.PointX * DashboardInfo.UnitX;
                var newY = dashboardInfo.PointY * DashboardInfo.UnitY;
                while (sw.Elapsed < duration)
                {
                    var easing = new QuinticEase() { EasingMode = EasingMode.EaseOut };
                    var ratio = easing.Ease(sw.ElapsedMilliseconds / duration.TotalMilliseconds);
                    Execute.OnUiThread(() =>
                    {
                        dashboardInfo.X = oldX + (newX - oldX) * ratio;
                        dashboardInfo.Y = oldY + (newY - oldY) * ratio;
                    });
                }

                Execute.OnUiThread(() =>
                {
                    dashboardInfo.X = newX;
                    dashboardInfo.Y = newY;
                    dashboardInfo.ZIndex = 0;
                });

                sw.Stop();
            });

            _viewModel.Save();
        }

        private void ResizeCanvas()
        {
            GetMatrix(out int xLen, out int yLen, out int columnCount);
            _viewModel.MaxWidth = DashboardInfo.UnitX * Math.Max((xLen - 1), columnCount);
            _viewModel.MaxHeight = DashboardInfo.UnitY * (yLen - 1);
        }

        private (bool flag, int sourceX, int sourceY)[,] GetMatrix(out int xLen, out int yLen, out int columnCount)
        {
            var w = ViewArea.ActualWidth - Container.Padding.Left - Container.Padding.Right;
            var h = ViewArea.ActualHeight - Container.Padding.Top - Container.Padding.Bottom;
            Console.WriteLine($"Canvas: {w}*{h}");
            columnCount = (int)(w / DashboardInfo.UnitX);
            Console.WriteLine($"Column count: " + columnCount);

            var collections = _viewModel.Collections;
            var maxX = collections.Count == 0
                ? 0
                : collections.Max(k => k.DashboardInfo.PointX + k.DashboardInfo.PointScaleX - 1);
            var maxY = collections.Count == 0
                ? 0
                : collections.Max(k => k.DashboardInfo.PointY + k.DashboardInfo.PointScaleY - 1);
            var matrix = new (bool, int, int)[Math.Max(columnCount, maxX + 1), maxY + 2];

            foreach (var collection in collections)
            {
                var dashboardInfo = collection.DashboardInfo;
                for (int i = 0; i < dashboardInfo.PointScaleX; i++)
                {
                    for (int j = 0; j < dashboardInfo.PointScaleY; j++)
                    {
                        matrix[dashboardInfo.PointX + i, dashboardInfo.PointY + j] =
                            (true, dashboardInfo.PointX, dashboardInfo.PointY);
                    }
                }
            }

            yLen = matrix.GetLength(1);
            xLen = matrix.GetLength(0);
            return matrix;
        }

        private void BtnAdd_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            AnimatedFrame.Default?.AnimateNavigate(SingletonPageHelper.Get<SelectTemplatePage>());
        }

        #region Debug

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

        public void AppendItem(int scaleX, int scaleY)
        {
            var matrix = GetMatrix(out int xLen, out int yLen, out int columnCount);

            var @break = false;
            StatsViewModel statsVm = null;
            DashboardInfo dashboardInfo = null;
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
                        dashboardInfo = new DashboardInfo
                        { PointX = x, PointY = y, PointScaleX = scaleX, PointScaleY = scaleY };
                        statsVm = new StatsViewModel()
                        {
                            DashboardInfo = dashboardInfo
                        };
                        break;
                    }
                }

                if (@break) break;
            }

            if (statsVm != null)
            {
                dashboardInfo.X = dashboardInfo.PointX * DashboardInfo.UnitX;
                dashboardInfo.Y = dashboardInfo.PointY * DashboardInfo.UnitY;
                dashboardInfo.Width = DashboardInfo.UnitX * scaleX;
                dashboardInfo.Height = DashboardInfo.UnitY * scaleY;
                _viewModel.Collections.Add(statsVm);
            }

            matrix = GetMatrix(out xLen, out yLen, out columnCount);
            _viewModel.MaxWidth = DashboardInfo.UnitX * Math.Max((xLen - 1), columnCount);
            _viewModel.MaxHeight = DashboardInfo.UnitY * (yLen - 1);
        }

        #endregion
    }
}