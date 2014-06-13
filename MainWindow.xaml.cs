using Oracle.DataAccess.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

        private void Exec_Click(object sender, RoutedEventArgs e)
        {
            string constr = "User Id=scott;Password=tiger;Data Source=oracle";
            OracleConnection con = new OracleConnection(constr);
            con.Open();

            OracleCommand cmd = new OracleCommand(this.sqlText.Text, con);
            
            this.DataContext = new { X = cmd.ExecuteStream() };

            // Clean up
            cmd.Dispose();
            con.Dispose();

        }

    }
}
