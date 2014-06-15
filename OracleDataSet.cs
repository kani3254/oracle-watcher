using Oracle.DataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;

namespace oracle
{
    class OracleDataSet : DependencyObject
    {
        private DataSet dataset = new DataSet();
        private OracleDataAdapter adapter = null;

        private string constr = "User Id=KANI;Password=kanikani;Data Source=localhost/orcl";
        public String SelectColumn { get; set; }
        public String SelectTable { get; set; }

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

        // SqlText が変更されたときに OracleDataAdapter を作成しなおす
        private static void OnSqlTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            OracleDataSet oracleDataSet = obj as OracleDataSet;

            if (oracleDataSet != null)
            {
                String newValue = (String)e.NewValue;
                oracleDataSet.adapter = new OracleDataAdapter(newValue, oracleDataSet.constr);
            }
        }
    }
}
