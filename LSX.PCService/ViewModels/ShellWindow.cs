using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LSX.PCService.ViewModels
{
    class ShellWindow:Window
    {
        public ShellWindow(IRegionManager regionManager)
        {
           
        }
        public IRegionManager RegionManager { get; set; }
    }
}
