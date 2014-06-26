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
        public int Count { get; private set; }
        
        // ページ数
        public int PageCount { get; private set; }
        
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
            // 値の強制。。。。こうなの？
            if (oracleDataPager.Page < 1) oracleDataPager.Page = 1;
            if (oracleDataPager.Page > oracleDataPager.PageCount) oracleDataPager.Page = oracleDataPager.PageCount;

            if (oracleDataPager != null)
            {
                oracleDataPager.ResetCursor();
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
                oracleDataPager.Sql = "SELECT * FROM " + (String)e.NewValue;
            }
        }
        
        // SQL文字列を格納するプロパティ
        public static readonly DependencyProperty SqlProperty =
            DependencyProperty.Register("Sql", typeof(String), typeof(OracleDataPager),
                new FrameworkPropertyMetadata("SELECT SYSDATE FROM DUAL",
                    new PropertyChangedCallback(OnSqlChanged) ));
        public String Sql
        {
            get { return (String)GetValue(SqlProperty); }
            set { SetValue(SqlProperty, value); }
        }
        
        // Sql が変更されたときに OracleDataAdapter を作成しなおす
        private static void OnSqlChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            OracleDataPager oracleDataPager = obj as OracleDataPager;
            
            if (oracleDataPager != null)
            {
                oracleDataPager.ResetCount();
                oracleDataPager.Page = 1;
                oracleDataPager.ResetCursor();
                oracleDataPager.FetchData();
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
            Count = 0;
            PageCount = 0;
            conMgr.PropertyChanged += OnConnectionChanged;
        }
        
        public void EventHandler(object sender, EventArgs e)
        {
            ResetCursor();
            FetchData();
        }

        // ページ数をリセット
        private void ResetPageCount()
        {
            PageCount = (Count - 1) / PerPage + 1;
            NotifyPropertyChanged("PageCount");
        }
        
        // 件数をリセット
        private void ResetCount()
        {
            command.CommandText = "SELECT COUNT(1) FROM (" + Sql + ")";
            Count = Convert.ToInt32(command.ExecuteScalar());
            NotifyPropertyChanged("Count");
            ResetPageCount();
        }
        
        // カーソルを取得
        private void ResetCursor()
        {
            if (cursor != null) cursor.Dispose();
            command.CommandText = "BEGIN OPEN :1 FOR SELECT * FROM (" + Sql + "); end;";
            OracleParameter p_rc = command.Parameters.Add(
                "p_rc",
                OracleDbType.RefCursor,
                DBNull.Value,
                ParameterDirection.Output);
            command.ExecuteNonQuery();
            command.Parameters.Clear();
            cursor = p_rc.Value as OracleRefCursor;
        }

        // データを再取得
        private void FetchData()
        {
            // データセットを再構築
            dataset.Dispose();
            dataset = new DataSet();
            adapter.Fill(dataset, (Page - 1) * PerPage, PerPage, Table, cursor);
            NotifyPropertyChanged("TableData");
        }

        #region command
        private DelegateCommand<string> _pageNavigateCommand;
        public DelegateCommand<string> PageNavigateCommand
        {
            get
            {
                return _pageNavigateCommand = _pageNavigateCommand ?? new DelegateCommand<string>(DoPageNavigate, CanPageNavigate);
            }
        }

        public void DoPageNavigate(string param)
        {
            Page += Convert.ToInt32(param);
        }

        public bool CanPageNavigate(string param)
        {
            return true;
        }
        #endregion

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged = (s, e) => { };

    }
}
