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
using System.Windows.Navigation;
using System.Windows.Shapes;

using LSX.PCService.Data;
using System.Data;

namespace LSX.PCService
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        CameraController camera = CameraController.Instance;
        MainThread mainThread;
        public MainWindow()
        {
            InitializeComponent();
            mainThread = new MainThread();
            init();
            Test();
        }
        void init()
        {

            Microsoft.Win32.OpenFileDialog f = new Microsoft.Win32.OpenFileDialog();
            f.Filter = "Excel2007文件|*.xlsx|Excel2003文件|*.xls";
            if (f.ShowDialog() == true)
            {
                ErrorCode code = ExcelToMysql.Import(f.FileName);
            }
        }

        void Test()
        {
            mainThread.Start();
        }

        private void test_Click(object sender, RoutedEventArgs e)
        {
            mainThread.Stop();
        }
    }
}
