using MicroMvvm;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace TERS
{
    using OxyPlot.Annotations;
    using OxyPlot.Axes;
    using OxyPlot.Series;
    using PropertyTools.Wpf;
    using System.IO;
    using System.Windows;
    using System.Windows.Input;
    using static StaticUtils;

    public partial class WaveAnalyseViewModel : ObservableObject
    {
        private Data_1D _currData;
        private Data_1D _originData;
        private LinearAxis _xAxis { get => (LinearAxis)WaveModel.Axes[1]; set { WaveModel.Axes[1] = value; } }
        private LinearAxis _yAxis { get => (LinearAxis)WaveModel.Axes[0]; set { WaveModel.Axes[0] = value; } }
        private string _message = "Ready";
        private readonly string[] _saveExts = new string[] { ".txt", ".1dat", ".png" };
        private readonly string[] _loadExts = new string[] { ".1dat", ".1dat_t", ".mat_t"};
        private PlotModel _wavePlot;
        private CalibrType _currCalibrType;
        private bool _isSelectChecked;
        private Spectro _spectroWindow;
        private IEnumerable<double> _currCalibr
        {
            get
            {
                IEnumerable<double> calibr = null;
                switch (CurrCalibrType)
                {
                    case CalibrType.Pixel:
                        calibr = Enumerable.Range(0, currData.Intensity.Count()).Select(_ => (double)_);
                        break;
                    case CalibrType.RamanShift:
                        calibr = currData.RamanShiftCalibr;
                        break;
                    case CalibrType.WaveLength:
                        calibr = currData.WaveLengthCalibr;
                        break;
                    default:
                        calibr = calibr = Enumerable.Range(0, currData.Intensity.Count()).Select(_ => (double)_);
                        break;
                }
                return calibr;
            }
        }

        private EventHandler<OxyMouseDownEventArgs> onMouseDown;
        private EventHandler<OxyMouseEventArgs> onMouseMove, onMouseUp;

        public WaveAnalyseViewModel()
        {
            WaveModel = DEF_WAVEMODEL;
        }

        public WaveAnalyseViewModel(Data_1D initData)
        {
            WaveModel = DEF_WAVEMODEL;
            currData = initData;
            _originData = initData.DeepCopy();
        }

        public CalibrType CurrCalibrType
        {
            get => _currCalibrType;
            set
            {
                if (value == CalibrType.Pixel)
                    _currCalibrType = value;
                else if (value == CalibrType.RamanShift)
                    _currCalibrType = currData?.RamanShiftCalibr != null ? value : CalibrType.Pixel;
                else if (value == CalibrType.WaveLength)
                    _currCalibrType = currData?.WaveLengthCalibr != null ? value : CalibrType.Pixel;
                RaisePropertyChanged("CurrCalibrType");
                updateWaveModel();
            }
        }
        public string Message
        {
            get => _message;
            private set
            {
                _message = value;
                RaisePropertyChanged("Message");
            }
        }
        public PlotModel WaveModel
        {
            get => _wavePlot;
            private set
            {
                _wavePlot = value;
                RaisePropertyChanged("WaveModel");
            }
        }
        public bool IsSelectChecked
        {
            get => _isSelectChecked;
            set
            {
                if (currData != null)
                    _isSelectChecked = value;
                onSelect();
                RaisePropertyChanged("IsSelectChecked");
            }
        }
        private Data_1D currData
        {
            get => _currData;
            set
            {
                _currData = value;
                updateWaveModel();
            }
        }

        public bool OnExit()
        {
            if (_spectroWindow == null || !_spectroWindow.IsOpen)
                return true;
            else
            {
                _spectroWindow.Activate();
                return false;
            }
        }
        public ICommand SaveCmd => new RelayCommand(onSave, () => currData != null);
        public ICommand LoadCmd => new RelayCommand(onLoad);
        public ICommand SettingCmd => new RelayCommand<string>(onSetting);
        public ICommand SpectroMeterCmd => new RelayCommand(onSpectroMeter);
        public ICommand ResetCmd => new RelayCommand(() => 
        {
            if (currData == null) return;
            currData = _originData.DeepCopy();
        }, () => currData != null);
        public ICommand PolyCmd => new RelayCommand(async () =>
        {
            if (currData == null) return;
            var ret = await Task.Run(() => Matlab?.PFC(_currCalibr, currData.Intensity, SETTINGS.Poly_N, SETTINGS.Poly_THR));
            if (ret != null)
            {
                currData.Intensity = ret;
                updateWaveModel();
            }           
        }, () => currData != null);
        public ICommand EMDCmd => new RelayCommand(async () =>
        {
            if (currData == null) return;
            var ret = await Task.Run(() => Matlab?.EMD(currData.Intensity, SETTINGS.EMD_A, SETTINGS.EMD_R));
            if (ret != null)
            {
                currData.Intensity = ret;
                updateWaveModel();
            }
        }, () => currData != null);
        public ICommand BSplineBaselineCmd => new RelayCommand(async () =>
        {
            if (currData == null) return;
            var ret = await Task.Run(() => Matlab?.BSC(_currCalibr,currData.Intensity, SETTINGS.BSC_M, SETTINGS.BSC_THR));
            if (ret != null)
            {
                currData.Intensity = ret;
                updateWaveModel();
            }
        }, () => currData != null);
        public ICommand SGFilterCmd => new RelayCommand(async () =>
        {
            if (currData == null) return;
            var ret = await Task.Run(() => Matlab?.SGFilter(currData.Intensity, SETTINGS.SGFilter_N, SETTINGS.SGFilter_LENGTH));
            if (ret != null)
            {
                currData.Intensity = ret;
                updateWaveModel();
            }
        }, () => currData != null);
        public ICommand MedianFilterCmd => new RelayCommand(async () =>
        {
            if (currData == null) return;
            var ret = await Task.Run(() => Matlab?.MedFilter(currData.Intensity, SETTINGS.MedFilter_N));
            if (ret != null)
            {
                currData.Intensity = ret;
                updateWaveModel();
            }
        }, () => currData != null);
        // to fix
        public ICommand ManualBaselineCmd => new RelayCommand(() =>
        {
            if (currData == null) return;
            var originLs = (LineSeries)WaveModel.Series[0];
            LineSeries ls = null;
            onMouseDown = (s, e) =>
            {
                if (WaveModel.Series.Count == 2)
                {
                    WaveModel.Series.RemoveAt(1);
                    ls = null;
                }
                if (e.ChangedButton == OxyMouseButton.Left)
                {
                    ls = new LineSeries
                    {
                        MarkerType = MarkerType.None,
                        StrokeThickness = 2,
                        LineStyle = LineStyle.Dash,
                    };
                    ls.Points.Add(_xAxis.InverseTransform(e.Position.X, e.Position.Y, _yAxis));
                    WaveModel.Series.Add(ls);
                    WaveModel.InvalidatePlot(true);
                    e.Handled = true;
                }
            };
            onMouseMove = (s, e) =>
            {
                if (ls != null)
                {
                    var dp = _xAxis.InverseTransform(e.Position.X, e.Position.Y, _yAxis);
                    ls.Points.Add(dp);
                    WaveModel.InvalidatePlot(true);
                    e.Handled = true;
                }
            };
            onMouseUp = (s, e) =>
            {
                if (ls != null)
                {
                    ls = null;
                    e.Handled = true;
                }
            };
            WaveModel.MouseDown += onMouseDown;
            WaveModel.MouseMove += onMouseMove;
            WaveModel.MouseUp += onMouseUp;
        }, () => currData != null);
        // to fix
        public ICommand CurvefitCmd => new RelayCommand(() =>
        {
            if (currData == null) return;
            
        }, () => currData != null);
        // to fix
        public ICommand SelectCmd => new RelayCommand(() => IsSelectChecked = !IsSelectChecked, () => currData != null);

        private void onSpectroMeter()
        {
            _spectroWindow = new Spectro(data => 
            {
                if (data == null)
                    return;
                if (currData != null)
                {
                    var result = MessageBox.Show("Current data will be overwritted.", "WaveAnalyse", MessageBoxButton.OKCancel);
                    if (result == MessageBoxResult.Cancel)
                        return;
                }
                currData = new Data_1D(from i in data select (double)i);
                _originData = currData.DeepCopy();
            });
            _spectroWindow.Show();
        }
        private void onSetting(string p)
        {
            if (p == "set")
            {
                if (PROPDLG.ShowDialog().Value)
                    SETTINGS.Save();
            }
            else if (p == "reset")
            {
                SETTINGS.Reset();
            }             
        }
        private void onSave()
        {
            if (currData == null) return;
            string path = SaveFilePath(_saveExts);
            if (path == null) return;
            _originData = currData.DeepCopy();
            try
            {
                switch (Path.GetExtension(path))
                {
                    case ".1dat":
                        byte[] bytes = currData.CompressToBytes();
                        File.WriteAllBytes(path, bytes);
                        break;
                    case ".txt":
                        string text = currData.ToString();
                        File.WriteAllText(path, text);
                        break;
                    case ".png":
                        ((OxyPlot.Wpf.PlotView)WaveModel.PlotView).SaveBitmap(path);
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "WaveAnalyse");
            }
        }
        private void onLoad()
        {
            string path = OpenFilePath(_loadExts);
            if (path == null) return;
            try
            {
                switch (Path.GetExtension(path))
                {
                    case ".1dat_t":
                        currData = Data_1D.ImportFromTxt(File.ReadAllLines(path));
                        _originData = currData?.DeepCopy();
                        break;
                    case ".1dat":
                        currData = (Data_1D)DecompressFromBytes(File.ReadAllBytes(path));
                        _originData = currData?.DeepCopy();
                        break;
                    case ".mat_t":
                        var lines = File.ReadAllLines(path).LoadVectorsFromCSV();
                        var count = lines.Count;
                        currData = new Data_1D(lines[0], count > 1 ? lines[1] : null, count > 2 ? lines[2] : null);
                        _originData = currData?.DeepCopy();
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "WaveAnalyse");
            }
        }
        private void onSelect()
        {
            if (currData == null) return;        
            
            WaveModel.MouseDown -= onMouseDown;
            WaveModel.MouseMove -= onMouseMove;
            WaveModel.MouseUp -= onMouseUp;
            WaveModel.Annotations.Clear();
            WaveModel.InvalidatePlot(true);
            Message = "Ready";

            if (IsSelectChecked == true)
            {
                var range = new RectangleAnnotation
                { Fill = OxyColor.FromAColor(100, OxyColors.SkyBlue), MinimumX = 0, MaximumX = 0, };
                WaveModel.Annotations.Add(range);
                WaveModel.InvalidatePlot(true);

                double startx = double.NaN;
                onMouseDown = (s, e) =>
                {
                    if (e.ChangedButton == OxyMouseButton.Left)
                    {
                        startx = range.InverseTransform(e.Position).X;
                        range.MinimumX = startx;
                        range.MaximumX = startx;
                        WaveModel.InvalidatePlot(true);
                        e.Handled = true;
                    }
                };
                onMouseMove = (s, e) =>
                {
                    if (!double.IsNaN(startx))
                    {
                        var x = range.InverseTransform(e.Position).X;
                        range.MinimumX = Math.Min(x, startx);
                        range.MaximumX = Math.Max(x, startx);
                        Message = $"Select from {range.MinimumX:F2} to {range.MaximumX:F2}";
                        WaveModel.InvalidatePlot(true);
                        e.Handled = true;
                    }
                };
                onMouseUp = (s, e) =>
                {
                    startx = double.NaN;
                };
                WaveModel.MouseDown += onMouseDown;
                WaveModel.MouseMove += onMouseMove;
                WaveModel.MouseUp += onMouseUp;
            }                
        }
        private void updateWaveModel()
        {
            if (currData == null) return;
            WaveModel.ResetAllAxes();
            var ls = DEF_LINESERIES;

            if (CurrCalibrType == CalibrType.Pixel)
            {
                ls.Points.AddRange(currData.Intensity.GenDataPoints());
                _xAxis.Unit = "Count";
            }             
            else if (CurrCalibrType == CalibrType.RamanShift)
            {
                ls.Points.AddRange(currData.Intensity.GenDataPoints(currData.RamanShiftCalibr));
                _xAxis.Unit = "1/cm";
            }               
            else if (CurrCalibrType == CalibrType.WaveLength)
            {
                ls.Points.AddRange(currData.Intensity.GenDataPoints(currData.WaveLengthCalibr));
                _xAxis.Unit = "nm";
            }

            _yAxis.Title = "Intensity";
            _yAxis.Unit = "Count";
            _xAxis.Title = CurrCalibrType.ToString();
            WaveModel.Series.Clear();
            WaveModel.Series.Add(ls);
            WaveModel.InvalidatePlot(true);
        }
    }
}
