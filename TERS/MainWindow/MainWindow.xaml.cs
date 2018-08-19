﻿using MatlabApi;
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

namespace TERS
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel _vm;

        public MainWindow()
        {
            InitializeComponent();
            _vm = new MainViewModel();
            DataContext = _vm;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            StaticUtils.Matlab = await MatlabTool.GetInstanceAsync();          
        }

        private void HeatMapView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var view = (OxyPlot.Wpf.PlotView)sender;
            view.ResetAllAxes();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
