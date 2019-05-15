using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Prism.Mvvm;
using Prism.Commands;
using LSX.PCService.Data;
using LSX.PCService.Models;
namespace LSX.PCService.ViewModels
{
    class WindowMainViewModel : BindableBase
    {

        private InputDataModel _inputData;

        public InputDataModel InputData
        {
            get { return _inputData; }
            set { this.SetProperty(ref _inputData, value); }
        }

        private List<OrderRaw> _rawList;

        public List<OrderRaw> RawList
        {
            get { return _rawList; }
            set { this.SetProperty(ref _rawList, value); }
        }

        private int _rawTotalCount;

        public int RawTotalCount
        {
            get { return _rawTotalCount; }
            set { SetProperty(ref _rawTotalCount, value); }
        }

        private int _rawTotalPages;

        public int RawTotalPages
        {
            get { return _rawTotalPages; }
            set { this.SetProperty(ref _rawTotalPages, value); }
        }

        private int _rawCurPageId;

        public int RawCurPageId
        {
            get { return _rawCurPageId; }
            set { this.SetProperty(ref _rawCurPageId, value); }
        }

        private int _rawPageSize;

        public int RawPageSize
        {
            get { return _rawPageSize; }
            set { this.SetProperty(ref _rawPageSize, value); }
        }

        private string _curPage;

        public string CurPage
        {
            get { return _curPage; }
            set { this.SetProperty(ref _curPage, value); }
        }
        private DelegateCommand _rawViewRefreshCmd;

        public DelegateCommand RawViewRefreshCmd
        {
            get { return _rawViewRefreshCmd; }
            set { this.SetProperty(ref _rawViewRefreshCmd, value); }
        }

        private List<OrderRunning> _orderRunningList;

        public List<OrderRunning> OrderRunningList
        {
            get { return _orderRunningList; }
            set { this.SetProperty(ref _orderRunningList, value); }
        }

        private DelegateCommand _rawCurPageSelectItemChangedCmd;

        public DelegateCommand RawCurPageSelectItemChangedCmd
        {
            get { return _rawCurPageSelectItemChangedCmd; }
            set { SetProperty(ref _rawCurPageSelectItemChangedCmd, value); }
        }

        private DelegateCommand _importRawDataCmd;

        public DelegateCommand ImportRawDataCmd
        {
            get { return _importRawDataCmd; }
            set { SetProperty(ref _importRawDataCmd, value); }
        }




        public WindowMainViewModel()
        {
            RawPageSize = 30;
            RawCurPageId = 1;
            RawViewRefresh();
            OrderViewUpdate();
            this.RawViewRefreshCmd = new DelegateCommand(RawViewRefresh);
            this.RawCurPageSelectItemChangedCmd = new DelegateCommand(RawViewRefresh);
            this.ImportRawDataCmd = new DelegateCommand(ImportRawData);
        }

        void RawViewRefresh()
        {
            if (RawPageSize == 0)
            {//异常无效页面大小

                return;
            }
            RawList = Db.Context.Query<OrderRaw>().TakePage(RawCurPageId, RawPageSize).ToList();
            RawTotalCount = Db.Context.Query<OrderRaw>().Count();
            RawTotalPages = (RawTotalCount + RawPageSize - 1) / RawPageSize;

            CurPage = string.Format("{0}/{1}", RawCurPageId, RawTotalPages);
        }

        void OrderViewUpdate()
        {
            OrderRunningList = Db.Context.Query<OrderRunning>().ToList();
        }
        void ImportRawData()
        {
            DbHelper.EmptyTable("order_raw");
            Microsoft.Win32.OpenFileDialog f = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "Excel2007文件|*.xlsx|Excel2003文件|*.xls",
                Title = "导入总捡单"

            };
            if (f.ShowDialog() == true)
            {
                ErrorCode code = ExcelToMysql.Import(f.FileName);
                System.Windows.MessageBox.Show(code.ToString(), "结果");
                if(code==ErrorCode.成功)
                {
                    RawViewRefresh();
                }
            }
        }
    }
}
