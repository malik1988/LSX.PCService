using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Prism.Ioc;
using Prism.Unity;

using LSX.PCService.Views;

namespace LSX.PCService
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<WindowRegionMain>(); 
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<PageImportRawData>();
            containerRegistry.RegisterForNavigation<PagePalletInput>();
            containerRegistry.RegisterForNavigation<PageChannelControl>();
            containerRegistry.RegisterForNavigation<PageLpnBindingC09>();
            containerRegistry.RegisterForNavigation<PageScanCarId>();
        }
    }
}
