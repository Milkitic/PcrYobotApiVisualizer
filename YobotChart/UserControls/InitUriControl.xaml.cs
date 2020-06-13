using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using YobotChart.UiComponents.FrontDialogComponent;

namespace YobotChart.UserControls
{
    /// <summary>
    /// InitUriControl.xaml 的交互逻辑
    /// </summary>
    public partial class InitUriControl : UserControl
    {
        public InitUriControl()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Text = string.IsNullOrWhiteSpace(TextBox.Text) ? null : TextBox.Text.Trim();
            FrontDialogOverlay.Default.RaiseOk();
        }

        public string Text { get; set; }

        private void TextBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                Button_Click(sender, e);
            }
        }

        private void InitUriControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            TextBox.Focus();
        }
    }
}
