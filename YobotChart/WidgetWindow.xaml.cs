using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using YobotChart.Shared.Win32;
using YobotChart.Shared.Win32.ChartFramework;

namespace YobotChart
{
    /// <summary>
    /// WidgetWindow.xaml 的交互逻辑
    /// </summary>
    public partial class WidgetWindow : WindowEx
    {
        private readonly StatsViewModel _statsViewModel;

        public WidgetWindow(StatsViewModel statsViewModel)
        {
            _statsViewModel = statsViewModel;
            InitializeComponent();
            Test.DataContext = _statsViewModel;
        }

        private void WindowEx_Shown(object sender, RoutedEventArgs e)
        {
        }

        private void WindowEx_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                this.DragMove();
            }
            catch (Exception ex)
            {
                
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        //Attach this to the MouseDown event of your drag control to move the window in place of the title bar
        private void WindowDrag(object sender, MouseButtonEventArgs e) // MouseDown
        {
            ReleaseCapture();
            SendMessage(new WindowInteropHelper(this).Handle,
                0xA1, (IntPtr)0x2, (IntPtr)0);
        }

        //Attach this to the PreviewMousLeftButtonDown event of the grip control in the lower right corner of the form to resize the window
        private void WindowResize(object sender, MouseButtonEventArgs e) //PreviewMousLeftButtonDown
        {
            HwndSource hwndSource = PresentationSource.FromVisual((Visual)sender) as HwndSource;
            SendMessage(hwndSource.Handle, 0x112, (IntPtr)61448, IntPtr.Zero);
        }

        private void WindowEx_Loaded(object sender, RoutedEventArgs e)
        {
            this.ShowAsWidgets();
        }
    }
}
