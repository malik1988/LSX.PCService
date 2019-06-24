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

namespace LSX.PCService.Views
{
    /// <summary>
    /// WindowOnline.xaml 的交互逻辑
    /// </summary>
    public partial class WindowOnline : Window
    {
        public WindowOnline()
        {
            InitializeComponent();
        }

        private void xTitle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void xMin_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void xMax_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState != WindowState.Maximized)
                this.WindowState = WindowState.Maximized;
            else
                this.WindowState = WindowState.Normal;

        }

        private void xQuit_Click(object sender, RoutedEventArgs e)
        {

            this.Close();
        }
    }
}
