﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Prism.Mvvm;
using Prism.Commands;
using LSX.PCService.Models;

namespace LSX.PCService.ViewModels
{
    class PageScanCarIdViewModel : BindableBase
    {
        private string _CarId;

        public string CarId
        {
            get { return _CarId; }
            set { SetProperty(ref _CarId, value); }
        }

        private DelegateCommand _CarIdEnter;

        public DelegateCommand CarIdEnter
        {
            get { return _CarIdEnter; }
            set { SetProperty(ref _CarIdEnter, value); }
        }
        private List<OrderRawAnalyzed> _OrderList;

        public List<OrderRawAnalyzed> OrderList
        {
            get { return _OrderList; }
            set { SetProperty(ref _OrderList, value); }
        }


        public PageScanCarIdViewModel()
        {
            OrderList = new List<OrderRawAnalyzed>();
            CarIdEnter = new DelegateCommand(() =>
            {//1.检查是否存在
                //2.提示确认开始订单
                //3.开始车牌对应的所有订单

            });
        }
    }
}