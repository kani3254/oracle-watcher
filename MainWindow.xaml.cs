using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

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
            DefaultTimer ti = this.timer.DataContext as DefaultTimer;
            OracleDataSet ds = this.dataGrid.DataContext as OracleDataSet;
            ti.Timer.Tick += new EventHandler(ds.EventHandler);
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
