using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Prism.Commands;
using LSX.PCService.Data;
using System.Collections.ObjectModel;
using System.Windows;
using Prism.Interactivity.InteractionRequest;

namespace LSX.PCService.ViewModels
{
    class PopupWindowAllEventsViewModel : BindableBase, IInteractionRequestAware
    {
        private ObservableCollection<object> _AllEvents
            ;

        public ObservableCollection<object> AllEvents
        {
            get { return _AllEvents; }
            set { SetProperty(ref _AllEvents, value); }
        }


        private int _TotalCount;

        public int TotalCount
        {
            get { return _TotalCount; }
            set { SetProperty(ref _TotalCount, value); }
        }
        private int _TotalPages;

        public int TotalPages
        {
            get { return _TotalPages; }
            set { SetProperty(ref _TotalPages, value); }
        }


        private int _CurrentPage;

        public int CurrentPage
        {
            get { return _CurrentPage; }
            set { SetProperty(ref _CurrentPage, value); }
        }


        private int _PageSize;

        public int PageSize
        {
            get { return _PageSize; }
            set { SetProperty(ref _PageSize, value); }
        }

        private DelegateCommand _PageSizeChanged;

        public DelegateCommand PageSizeChanged
        {
            get { return _PageSizeChanged; }
            set { SetProperty(ref _PageSizeChanged, value); }
        }
        private DelegateCommand _ViewRefreshCommand;

        public DelegateCommand ViewRefreshCommand
        {
            get { return _ViewRefreshCommand; }
            set { SetProperty(ref _ViewRefreshCommand, value); }
        }

        private ObservableCollection<string> _TableNames;

        public ObservableCollection<string> TableNames
        {
            get { return _TableNames; }
            set { SetProperty(ref _TableNames, value); }
        }

        private Dictionary<string, string> _realTableNameDict;

        private string _SelectedTableName;

        public string SelectedTableName
        {
            get { return _SelectedTableName; }
            set { SetProperty(ref _SelectedTableName, value); }
        }
   

        public PopupWindowAllEventsViewModel()
        {
            _realTableNameDict = new Dictionary<string, string>()
            {
                {"事件记录表","awms_events_dhl"},
                {"设备表","awms_device_dhl"},
                {"灯信息表","awms_light_dhl"},
                {"灯任务表","awms_light_tasks_dhl"},
                {"LPN表","awms_lpn_dhl"},
                {"订单表","awms_orders_dhl"},
                {"发货单表","awms_orders_tasks_dhl"},
                {"栈板表","awms_pallets_dhl"},
                {"原始数据表","awms_source_dhl"},
                {"库位表","awms_storges_dhl"}
            };
            TableNames = new ObservableCollection<string>();
            foreach (var x in _realTableNameDict.Keys)
            {
                TableNames.Add(x);
            }
            SelectedTableName = TableNames[0];

            PageSize = 50;

            ViewRefresh();
            PageSizeChanged = new DelegateCommand(() =>
            {
                if (PageSize <= 0)
                {
                    MessageBox.Show("页数必须是大于0的整数", "错误");
                    return;
                }
                CurrentPage = 1;
                string tableName = _realTableNameDict[SelectedTableName];
                TotalCount = DbHelper.GetTabelRecordCount(tableName);
                AllEvents = DbHelper.GetDataByPage(tableName, CurrentPage, PageSize);
                TotalPages = (TotalCount + PageSize - 1) / PageSize;

            });
            ViewRefreshCommand = new DelegateCommand(ViewRefresh);
        }

        void ViewRefresh()
        {
            if (PageSize <= 0)
            {
                return;
            }
            string tableName = _realTableNameDict[SelectedTableName];
            TotalCount = DbHelper.GetTabelRecordCount(tableName);
            AllEvents = DbHelper.GetDataByPage(tableName,CurrentPage, PageSize);
            TotalPages = (TotalCount + PageSize - 1) / PageSize;

        }

        #region IInteractionRequestAware 成员

        public Action FinishInteraction
        {
            get;
            set;
        }

        public INotification Notification
        {
            get;
            set;
        }

        #endregion
    }
}
