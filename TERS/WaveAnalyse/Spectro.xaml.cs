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
using PropertyTools.Wpf;

namespace TERS
{
    using static StaticUtils;
    using static CamApi.CamTool;
    using System.ComponentModel;
    using System.Timers;

    /// <summary>
    /// Spectro.xaml 的交互逻辑
    /// </summary>
    public partial class Spectro : Window
    {
        public bool IsOpen = false;
        private SpectroViewModel _vm;

        public Spectro(Action<IEnumerable<int>> passDatabackHandler)
        {
            InitializeComponent();
            _vm = new SpectroViewModel { passDataBack = passDatabackHandler };
            DataContext = _vm;
        }

        private void Spectro_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = !_vm.TryForceExit();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            IsOpen = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            IsOpen = true;
        }

        public class SpectroViewModel : ObservableObject
        {
            private CamConnection _connection;
            private Timer _timer;
            public Action<IEnumerable<int>> passDataBack;

            private string _CCDContent;
            private string _coolerContent;
            private string _info;            
            private bool _busyFlag = false;
            public string CCDContent
            {
                get => _CCDContent;
                set
                {
                    if (_CCDContent == value)
                        return;
                    _CCDContent = value;
                    RaisePropertyChanged("CCDContent");
                }
            }
            public string CoolerContent
            {
                get => _coolerContent;
                set
                {
                    if (_coolerContent == value)
                        return;
                    _coolerContent = value;
                    RaisePropertyChanged("CoolerContent");
                }
            }
            public string Info
            {
                get => _info;
                set
                {
                    if (_info == value)
                        return;
                    _info = value;
                    RaisePropertyChanged("Info");
                }
            }

            private void updateStatus()
            {
                if (_connection == null)
                {
                    Info = "No CCD connection";
                    CCDContent = "CCD off";
                    CoolerContent = "Cooler off";
                }
                else
                {
                    Info = $"Temp={_connection.GetCamTemp(out ERRCODES x):F2}=>{_connection.TargetTemp:F2}|ExpoTime={_connection.ExpoTime}";                   
                    CCDContent = "CCD on";
                    CoolerContent = _connection.IsCoolerOn ? "Cooler on" : "Cooler off";
                    if (_busyFlag)
                        Info = "Busy|" + Info;
                }               
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns>true => force exit; false => cancel exit</returns>
            public bool TryForceExit()
            {
                if (_connection == null)
                    return true;
                var res = MessageBox.Show("CCD now connected.Force exit?", "Spectro", MessageBoxButton.OKCancel);
                if (res == MessageBoxResult.Cancel)
                    return false;
                else
                {
                    _connection.ShutDown(true);
                    _connection = null;
                    return true;
                }               
            }

            public ICommand SettingCmd => new RelayCommand(() =>
            {
                if (PROPDLG.ShowDialog().Value)
                {
                    SETTINGS.Save();
                    if (_connection != null)
                    {
                        _connection.ExpoTime = SETTINGS.ExpoTime;
                        _connection.TargetTemp = SETTINGS.TargetTemp;
                    }
                }
                updateStatus();
            });

            public ICommand CCDCmd => new RelayCommand(async () => 
            {
                if (_connection == null)
                {
                    _connection = CamConnection.CreateConnection(SETTINGS.TargetTemp, SETTINGS.ExpoTime);
                }
                else
                {
                    if (MessageBox.Show("ShutDown CCD?", "Spectro", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {
                        _busyFlag = true;
                        //await Task.Run(() => System.Threading.Thread.Sleep(2000)).ContinueWith(preTask => _busyFlag = false);
                        await _connection.ShutDownAsync(SETTINGS.ShutdownFastmode).ContinueWith(preTask => _busyFlag = false);
                        _connection = null;
                    }                   
                }
                updateStatus();
            }, () => _busyFlag == false);

            public ICommand AcquireCmd => new RelayCommand(async () =>
            {
                if (_connection == null)
                    return;
                _busyFlag = true;
                await _connection.AcqDataAsync()
                .ContinueWith(preTask => { _busyFlag = false; passDataBack?.Invoke(preTask.Result); });
                updateStatus();
            }, () => _connection != null && _busyFlag == false);

            public ICommand CoolerCmd => new RelayCommand(() =>
            {
                if (_connection == null)
                    return;
                else if (_connection.IsCoolerOn) _connection.CoolerOff();
                else if (!_connection.IsCoolerOn) _connection.CoolerOn();
                updateStatus();
            }, () => _connection != null && _busyFlag == false);

            public SpectroViewModel()
            {
                _timer = new Timer(500);
                _timer.Elapsed += (s, e) => updateStatus();
                updateStatus();
                _timer.Start();
            }
        }
    }
}
