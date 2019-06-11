using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using Prism.Interactivity.InteractionRequest;
namespace LSX.PCService.Views
{
    /// <summary>
    /// PopupScanTrafficOrderScan.xaml 的交互逻辑
    /// </summary>
    public partial class PopupScanTrafficOrderScan : Window, IInteractionRequestAware
    {
        public PopupScanTrafficOrderScan()
        {
            InitializeComponent();
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
