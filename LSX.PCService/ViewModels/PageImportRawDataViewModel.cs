using LSX.PCService.Data;
using LSX.PCService.Models;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Interactivity.InteractionRequest;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Threading;

using LSX.PCService.Views;
using LSX.PCService.Service;
namespace LSX.PCService.ViewModels
{
    class PageImportRawDataViewModel : BindableBase
    {
        private bool _ImportRawEnable;

        public bool ImportRawEnable
        {
            get { return _ImportRawEnable; }
            set { SetProperty(ref _ImportRawEnable, value); }
        }

        private bool _ImportRawCheckEnable;

        public bool ImportRawCheckEnable
        {
            get { return _ImportRawCheckEnable; }
            set { SetProperty(ref _ImportRawCheckEnable, value); }
        }

        private DelegateCommand _ImportRawOrderCmd;

        public DelegateCommand ImportRawOrderCmd
        {
            get { return _ImportRawOrderCmd; }
            set { SetProperty(ref _ImportRawOrderCmd, value); }
        }

        private DelegateCommand _ImportRawCheckCmd;

        public DelegateCommand ImportRawCheckCmd
        {
            get { return _ImportRawCheckCmd; }
            set { SetProperty(ref _ImportRawCheckCmd, value); }
        }
        private List<AwmsRawData> _rawList;

        public List<AwmsRawData> RawList
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

        private DelegateCommand _rawCurPageSelectItemChangedCmd;

        public DelegateCommand RawCurPageSelectItemChangedCmd
        {
            get { return _rawCurPageSelectItemChangedCmd; }
            set { SetProperty(ref _rawCurPageSelectItemChangedCmd, value); }
        }


        private int _CurrentProgress;

        public int CurrentProgress
        {
            get { return _CurrentProgress; }
            set { SetProperty(ref _CurrentProgress, value); }
        }

        private int _MaxProgress;

        public int MaxProgress
        {
            get { return _MaxProgress; }
            set { SetProperty(ref _MaxProgress, value); }
        }

        private bool _NextPageEnable;

        public bool NextPageEnable
        {
            get { return _NextPageEnable; }
            set { SetProperty(ref _NextPageEnable, value); }
        }

        private DelegateCommand _NextPage;

        public DelegateCommand NextPage
        {
            get { return _NextPage; }
            set { SetProperty(ref _NextPage, value); }
        }



        public PageImportRawDataViewModel()
        {
            //debug

            RawPageSize = 30;
            RawCurPageId = 1;
            CurrentProgress = 0;
            MaxProgress = 100;
            ImportRawEnable = true;
            ImportRawCheckEnable = true;
            NextPageEnable = true;

            RawViewRefresh();
            RawViewRefreshCmd = new DelegateCommand(RawViewRefresh);
            RawCurPageSelectItemChangedCmd = new DelegateCommand(RawViewRefresh);
            ImportRawOrderCmd = new DelegateCommand(ImportRawData, new System.Func<bool>(() => { return ImportRawEnable; }));
            ImportRawCheckCmd = new DelegateCommand(ImportRawCheck, new System.Func<bool>(() => { return ImportRawCheckEnable; }));
            NextPage = new DelegateCommand(BeginNextPage, new System.Func<bool>(() => { return NextPageEnable; }));

        }

        void RawViewRefresh()
        {
            if (RawPageSize == 0)
            {//异常无效页面大小

                return;
            }
            RawList = Db.Context.Query<AwmsRawData>().TakePage(RawCurPageId, RawPageSize).ToList();
            RawTotalCount = Db.Context.Query<AwmsRawData>().Count();
            RawTotalPages = (RawTotalCount + RawPageSize - 1) / RawPageSize;

            CurPage = string.Format("{0}/{1}", RawCurPageId, RawTotalPages);
        }
        void ImportRawData()
        {
            ImportRawEnable = false;
            OpenFileDialog f = new OpenFileDialog()
            {
                Filter = "Excel2007文件|*.xlsx|Excel2003文件|*.xls",
                Title = "导入质检单"
            };
            if (f.ShowDialog() == true)
            {
                ThreadPool.QueueUserWorkItem(o =>
                {
                    ErrorCode code = ExcelToMysql.Import2AwmsRawData(f.FileName, PopWorker);
                    System.Windows.MessageBox.Show(code.ToString(), "结果");
                    if (code == ErrorCode.成功)
                    {
                        RawViewRefresh();
                    }
                    ImportRawEnable = true;
                });
            }
            else
            {
                ImportRawEnable = true;
            }
            //  main.Start();
        }

        void ImportRawCheck()
        {
            ImportRawCheckEnable = false;
            OpenFileDialog f = new OpenFileDialog()
            {
                Filter = "Excel2007文件|*.xlsx|Excel2003文件|*.xls",
                Title = "导入质检单"
            };
            if (f.ShowDialog() == true)
            {
                ThreadPool.QueueUserWorkItem(o =>
                {
                    ErrorCode code = ExcelToMysql.Import2AwmsRawData(f.FileName);
                    System.Windows.MessageBox.Show(code.ToString(), "结果");
                    if (code == ErrorCode.成功)
                    {
                        RawViewRefresh();
                    }
                    ImportRawCheckEnable = true;
                });
            }
            else
            {
                ImportRawCheckEnable = true;

            }

        }

        void PopWorker(int cur, int max)
        {
            CurrentProgress = cur;
            MaxProgress = max;
            System.Diagnostics.Debug.WriteLine("cur:" + cur.ToString() + " max" + max.ToString() + " progress" + (float)CurrentProgress / MaxProgress);
        }


        void BeginNextPage()
        { }
    }
}
