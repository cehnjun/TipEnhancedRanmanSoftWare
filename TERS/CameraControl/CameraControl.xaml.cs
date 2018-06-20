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

namespace TERS.CameraControl
{
    /// <summary>
    /// CameraControl.xaml 的交互逻辑
    /// </summary>
    public partial class CameraControl : Window
    {
        CameraControlViewModel _cavm;
        public CameraControl()
        {
            InitializeComponent();
            _cavm = new CameraControlViewModel();
            DataContext = _cavm;
        }
    }
}
