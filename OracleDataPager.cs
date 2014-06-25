using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;
using System;
using System.ComponentModel;
using System.Data;
using System.Windows;

namespace oracle
{
    public partial class OracleDataPager : DependencyObject, INotifyPropertyChanged
    {
        private DataSet dataset = new DataSet();
        private OracleDataAdapter adapter = null;
        private OracleConnectionManager conMgr = OracleConnectionManager.Instance;
        
        private OracleCommand command; // Oracleに指示するためのコマンド実装
        private OracleRefCursor cursor; // Oracleカーソル
        
        private void OnConnectionChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "Connection") return;
            command = conMgr.Connection.CreateCommand();
            adapter = new OracleDataAdapter(command);
        }

        // テーブルの件数
        private int _count = 0;
        public int Count { get { return _count; } }
        
        // ページ数
        private int _pageCount = 0;
        public int PageCount { get { return _pageCount; } }
        
        // ページごとの件数
        public static readonly DependencyProperty PerPageProperty =
            DependencyProperty.Register("PerPage", typeof(int), typeof(OracleDataPager),
                new FrameworkPropertyMetadata(100, // デフォルト件数
                    new PropertyChangedCallback(OnPerPageChanged) ));
        public int PerPage
        {
            get { return (int)GetValue(PerPageProperty); }
            set { SetValue(PerPageProperty, value); }
        }
        // ページごとの件数が変更されたとき
        private static void OnPerPageChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            OracleDataPager oracleDataPager = obj as OracleDataPager;
            
            if (oracleDataPager != null)
            {
                oracleDataPager.ResetPageCount();
            }
        }
        
        // 現在のページ
        public static readonly DependencyProperty PageProperty =
            DependencyProperty.Register("Page", typeof(int), typeof(OracleDataPager),
                new FrameworkPropertyMetadata(0, // デフォルトページ
                    new PropertyChangedCallback(OnPageChanged) ));
        public int Page
        {
            get { return (int)GetValue(PageProperty); }
            set { SetValue(PageProperty, value); }
        }
        // ページが変更されたとき
        private static void OnPageChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            OracleDataPager oracleDataPager = obj as OracleDataPager;
            
            if (oracleDataPager != null)
            {
                oracleDataPager.FetchData();
            }
        }
        
        // テーブル名を格納するプロパティ
        public static readonly DependencyProperty TableProperty =
            DependencyProperty.Register("Table", typeof(String), typeof(OracleDataPager),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(OnTableChanged)));
        public String Table
        {
            get { return (String)GetValue(TableProperty); }
            set { SetValue(TableProperty, value); }
        }
        
        // テーブル名 が変更されたときにデータをリセットする
        private static void OnTableChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            OracleDataPager oracleDataPager = obj as OracleDataPager;
            
            if (oracleDataPager != null)
            {
                oracleDataPager.ResetData();
                oracleDataPager.Page = 0;
            }
        }
        
        // DataGrid に表示するためのデータを返却する
        public DataView TableData
        {
            get
            {
                if (cursor == null) return null;
                return dataset.Tables[Table].DefaultView;
            }
        }
        
        // コンストラクタ
        public OracleDataPager()
        {
            conMgr.PropertyChanged += OnConnectionChanged;
        }
        
        public void EventHandler(object sender, EventArgs e)
        {
            NotifyPropertyChanged("TableData");
        }

        // ページ数をリセット
        private void ResetPageCount()
        {
            _pageCount = (Count - 1) / PerPage + 1;
            NotifyPropertyChanged("PageCount");
        }
        
        // 件数をリセット
        private void ResetData()
        {
            // 件数を取得
            command.CommandText = "SELECT COUNT(1) FROM " + Table;
            _count = Convert.ToInt32(command.ExecuteScalar());
            NotifyPropertyChanged("Count");
            ResetPageCount();
        }
        
        // データを再取得
        private void FetchData()
        {
            // カーソルを取得
            if (cursor != null) cursor.Dispose();
            command.CommandText = "BEGIN OPEN :1 FOR SELECT * FROM " + Table + "; end;";
            OracleParameter p_rc = command.Parameters.Add(
                "p_rc",
                OracleDbType.RefCursor,
                DBNull.Value,
                ParameterDirection.Output);
            command.ExecuteNonQuery();
            command.Parameters.Clear();
            cursor = p_rc.Value as OracleRefCursor;
            // データセットを再構築
            dataset.Dispose();
            dataset = new DataSet();
            adapter.Fill(dataset, (Page - 1) * PerPage, PerPage, Table, cursor);
            NotifyPropertyChanged("TableData");
        }
        
        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged = (s, e) => { };

    }
}
