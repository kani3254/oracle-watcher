using Oracle.DataAccess.Client;
using System;
using System.ComponentModel;
using System.Data;
using System.Windows;

namespace oracle
{
    public partial class OracleDataSet : DependencyObject, INotifyPropertyChanged
    {
        private DataSet dataset = new DataSet();
        private OracleDataAdapter adapter = null;
        private OracleConnectionManager conMgr = OracleConnectionManager.Instance;

        private void OnConnectionChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "Connection") return;
            Reset();
        }

        // SQL文字列を格納するプロパティ
        public static readonly DependencyProperty SqlTextProperty =
            DependencyProperty.Register("SqlText", typeof(String), typeof(OracleDataSet),
                new FrameworkPropertyMetadata("SELECT SYSDATE FROM DUAL",
                    new PropertyChangedCallback(OnSqlTextChanged) ));
        public String SqlText
        {
            get { return (String)GetValue(SqlTextProperty); }
            set { SetValue(SqlTextProperty, value); }
        }

        // SqlText が変更されたときに OracleDataAdapter を作成しなおす
        private static void OnSqlTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            OracleDataSet oracleDataSet = obj as OracleDataSet;

            if (oracleDataSet != null)
            {
                oracleDataSet.Reset();
            }
        }

        // テーブル名を格納するプロパティ
        public static readonly DependencyProperty SelectTableProperty =
            DependencyProperty.Register("SelectTable", typeof(String), typeof(OracleDataSet),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(OnSelectTableChanged)));
        public String SelectTable
        {
            get { return (String)GetValue(SelectTableProperty); }
            set { SetValue(SelectTableProperty, value); }
        }

        // テーブル名 が変更されたときに SqlText を作成しなおす
        private static void OnSelectTableChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            OracleDataSet oracleDataSet = obj as OracleDataSet;

            if (oracleDataSet != null)
            {
                String newValue = (String)e.NewValue;
                oracleDataSet.SqlText = "SELECT * FROM " + newValue;
            }
        }

        // DataGrid に表示するためのデータを返却する
        public DataView TableData
        {
            get
            {
                if (adapter == null) Reset();
                try
                {
                    dataset.Clear();
                    adapter.Fill(dataset, "TABLE");
                    return dataset.Tables["TABLE"].DefaultView;
                }
                catch (Exception e)
                {
                    conMgr.Message = e.Message;
                    return null;
                }                
            }
        }

        public OracleDataSet()
        {
            conMgr.PropertyChanged += OnConnectionChanged;
        }

        public void EventHandler(object sender, EventArgs e)
        {
            NotifyPropertyChanged("TableData");
        }

        // DataSet を初期化して DataAdapter を再セット
        private void Reset()
        {
            dataset.Dispose();
            dataset = new DataSet();
            adapter = new OracleDataAdapter(SqlText, conMgr.Connection);
            NotifyPropertyChanged("TableData");
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged = (s, e) => { };

    }
}
