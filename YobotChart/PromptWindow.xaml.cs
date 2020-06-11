using System.Windows;

namespace YobotChart
{
    /// <summary>
    /// PromptWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PromptWindow : Window
    {
        public PromptWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Text = TextBox.Text;
            Close();
        }

        public string Text { get; set; }
    }
}
