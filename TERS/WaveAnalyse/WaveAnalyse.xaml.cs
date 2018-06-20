using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace TERS
{
    /// <summary>
    /// WaveAnalyse.xaml 的交互逻辑
    /// </summary>
    public partial class WaveAnalyse : Window
    {
        WaveAnalyseViewModel _waVM;

        public WaveAnalyse()
        {
            InitializeComponent();
            _waVM = new WaveAnalyseViewModel();
            DataContext = _waVM;
        }

        public WaveAnalyse(Data_1D initData)
        {
            InitializeComponent();
            _waVM = new WaveAnalyseViewModel(initData);
            DataContext = _waVM;
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = !_waVM.OnExit();
        }
    }
}
