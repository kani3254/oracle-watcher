using Oracle.DataAccess.Client;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace oracle
{
    public sealed class OracleConnectionManager : DependencyObject, ICommand, INotifyPropertyChanged
    {
        private static OracleConnectionManager _instance = new OracleConnectionManager();
        public static OracleConnectionManager Instance { get { return _instance; } }

        private OracleConnection _connection = new OracleConnection();
        public OracleConnection Connection { get { return _connection; } }

        public String UserID { get; set; }
        public String Password { get; set; }
        public String Ezconstr { get; set; }

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(String), typeof(OracleConnectionManager));
        public String Message
        {
            get { return (String)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        private OracleConnectionManager()
        {
            UserID = "UserID";
            Password = "Password";
            Ezconstr = "EZ connection string";
        }

        public bool CanExecute(object parameter)
        {
            OracleConnectionManager conMgr = OracleConnectionManager.Instance;
            if (conMgr.UserID != null && conMgr.Password != null && conMgr.Ezconstr != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public event EventHandler CanExecuteChanged {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            try
            {
                OracleConnectionStringBuilder builder = new OracleConnectionStringBuilder();
                builder.UserID = UserID;
                builder.Password = Password;
                builder.DataSource = Ezconstr;

                OracleConnection testConn = new OracleConnection(builder.ConnectionString);
                testConn.Open();
                testConn.Close();
                _connection = testConn;
                NotifyPropertyChanged("Connection");

                Message = "OK";
            }
            catch (OracleException e)
            {
                Message = e.Message;
            }
        }


        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged = (s, e) => { };

    }
}
