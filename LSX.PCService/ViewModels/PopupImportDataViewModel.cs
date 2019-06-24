using LSX.PCService.Controllers;
using LSX.PCService.Data;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace LSX.PCService.ViewModels
{
    class PopupImportDataViewModel : BindableBase
    {
        private int _Value;

        public int Value
        {
            get { return _Value; }
            set { SetProperty(ref _Value, value); }
        }

        private DelegateCommand _OpenFile;

        public DelegateCommand OpenFile
        {
            get { return _OpenFile; }
            set { SetProperty(ref _OpenFile, value); }
        }

        private string _ImportMsg;

        public string ImportMsg
        {
            get { return _ImportMsg; }
            set { SetProperty(ref _ImportMsg, value); }
        }

        private SolidColorBrush _ImportState;

        public SolidColorBrush ImportState
        {
            get { return _ImportState; }
            set { SetProperty(ref _ImportState, value); }
        }
        private bool _CanOpenFile;
        public PopupImportDataViewModel()
        {
            ImportMsg = "等待导入";
            _CanOpenFile = true;
            OpenFile = new DelegateCommand(() =>
            {
                OpenFileDialog f = new OpenFileDialog()
                {
                    Filter = "Excel2007文件|*.xlsx|Excel2003文件|*.xls",
                    Title = "导入质检单"
                };
                if (f.ShowDialog() == true)
                {
                    ImportMsg = "数据导入中";
                    _CanOpenFile = false;
                    ThreadPool.QueueUserWorkItem(o =>
                    {
                        try
                        {
                            ErrorCode err = DbHelper.ImportExcelToAwms(f.FileName, PopWorker);
                            if (err == ErrorCode.成功)
                            {
                                ImportState = Brushes.Green;
                            }
                            else
                                ImportState = Brushes.Red;
                            ImportMsg = err.ToString();
                        }
                        catch (System.Exception ex)
                        {
                            ImportMsg = ex.Message;
                        }
                        _CanOpenFile = true;
                    });

                }
                else
                {
                    ImportMsg = "用户取消";
                    ImportState = Brushes.Yellow;
                    _CanOpenFile = true;
                }
            }, () => { return _CanOpenFile; });

        }

        private void PopWorker(object sender, int e)
        {
            Value = e;
        }
    }
}
