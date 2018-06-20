using MicroMvvm;
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

namespace TERS
{
    /// <summary>
    /// ColorAxisChange.xaml 的交互逻辑
    /// </summary>
    public partial class ColorAxisChangeWindow : Window
    {
        public ColorAxisChangeWindow(Action<int, int> handler)
        {
            InitializeComponent();
            DataContext = new CACViewModel { ValueChangedHandler = handler };
        }

        public class CACViewModel : ObservableObject
        {
            private int _lowerValue = 0;
            private int _higherValue = 100;
            private readonly int MINGAP = 3;

            public Action<int, int> ValueChangedHandler;

            public int HigherValue
            {
                get { return _higherValue; }
                set
                {
                    if (value >= _lowerValue + MINGAP && _higherValue != value)
                    {
                        _higherValue = value;
                        ValueChangedHandler?.Invoke(_lowerValue, _higherValue);
                        RaisePropertyChanged("HigherValue");
                    }                 
                }
            }

            public int LowerValue
            {
                get { return _lowerValue; }
                set
                {
                    if (value <= _higherValue - MINGAP && _lowerValue != value)
                    {
                        _lowerValue = value;
                        ValueChangedHandler?.Invoke(_lowerValue, _higherValue);
                        RaisePropertyChanged("LowerValue");
                    }                  
                }
            }
        }
    }
}
