using System.Windows;
using System.Windows.Controls;

namespace oracle
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public void TextAllSelect(object o, RoutedEventArgs e)
        {
            TextBox textBox = e.OriginalSource as TextBox;

            if (textBox == null)
                return;
            textBox.SelectAll();
        }
    }
}
