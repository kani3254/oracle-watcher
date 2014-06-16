using Oracle.DataAccess.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;

namespace oracle
{
    public partial class OracleDataSet : DependencyObject, INotifyPropertyChanged
    {
        private DataSet dataset = new DataSet();
        private OracleDataAdapter adapter = null;

        private string constr = "User Id=KANI;Password=kanikani;Data Source=localhost/orcl";

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
                // DataSet を初期化して DataAdapter を再セット
                String newValue = (String)e.NewValue;
                oracleDataSet.dataset.Dispose();
                oracleDataSet.dataset = new DataSet();
                oracleDataSet.adapter = new OracleDataAdapter(newValue, oracleDataSet.constr);
                // TableData が変更されたことを通知する
                oracleDataSet.NotifyPropertyChanged("TableData");
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

        // テーブル名 が変更されたときに OracleDataAdapter を作成しなおす
        private static void OnSelectTableChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            OracleDataSet oracleDataSet = obj as OracleDataSet;

            if (oracleDataSet != null)
            {
                String newValue = (String)e.NewValue;
                oracleDataSet.SqlText = "SELECT * FROM " + newValue;
            }
        }


        //public OracleDataSet()
        //{
        //    SelectColumn = "SYSDATE";
        //    SelectTable = "DUAL";
        //}

        public DataView TableData
        {
            get
            {
                if (adapter == null)
                {
                    //if (SqlText == null)
                    //{
                    //    SqlText = "SELECT " + SelectColumn + " FROM " + SelectTable;
                    //}
                    adapter = new OracleDataAdapter(SqlText, constr);
                }
                adapter.Fill(dataset, "TABLE");
                
                return dataset.Tables["TABLE"].DefaultView;
            }
        }

        // 通知イベント
        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged = (s, e) => { };

    }
}
