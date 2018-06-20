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
using PropertyTools.Wpf;

namespace TERS
{
    /// <summary>
    /// MapAnalyse.xaml 的交互逻辑
    /// </summary>
    public partial class MapAnalyse : Window
    {
        MapAnalyseViewModel mapVM;

        public MapAnalyse()
        {
            InitializeComponent();
            mapVM = new MapAnalyseViewModel();
            DataContext = mapVM;
        }

        public MapAnalyse(Data_2D initData)
        {
            InitializeComponent();
            mapVM = new MapAnalyseViewModel(initData);
            DataContext = mapVM;
        }

        private void PlotView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var plotView = (OxyPlot.Wpf.PlotView)sender;
            plotView.ResetAllAxes();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            mapVM.OnExit();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
