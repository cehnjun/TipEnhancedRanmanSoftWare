using MicroMvvm;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;


namespace TERS
{
    using PropertyTools.Wpf;
    using System.Windows;
    using System.Windows.Forms;
    using static StaticUtils;

    public class MainViewModel : ObservableObject
    {
        private PlotModel _waveModel;
        private PlotModel _mapModel;
        private CalibrType _currCalibr;
        private Data_Map _currData;
        private Data_Map _originData;
        private string _currPointInfo;
        private DataPoint _currPoint;
        private LinearAxis _xAxis { get => (LinearAxis)WaveModel.Axes[1]; set { WaveModel.Axes[1] = value; } }
        private LinearAxis _yAxis { get => (LinearAxis)WaveModel.Axes[0]; set { WaveModel.Axes[0] = value; } }
        private readonly string[] _saveExts = new string[] { ".mdat", "txt" };
        private readonly string[] _loadExts = new string[] { ".mdat", ".mdat_t" };

        public PlotModel WaveModel
        {
            get => _waveModel;
            set
            {
                _waveModel = value;
                RaisePropertyChanged("WaveModel");
            }
        }
        public PlotModel MapModel
        {
            get => _mapModel;
            set
            {
                _mapModel = value;
                RaisePropertyChanged("MapModel");
            }
        }
        public CalibrType CurrCalibr
        {
            get => _currCalibr;
            set
            {
                if (value == CalibrType.Pixel)
                    _currCalibr = value;
                else if (value == CalibrType.RamanShift)
                    _currCalibr = currData?.RamanShiftCalibr != null ? value : CalibrType.Pixel;
                else if (value == CalibrType.WaveLength)
                    _currCalibr = currData?.WaveLengthCalibr != null ? value : CalibrType.Pixel;
                RaisePropertyChanged("CurrCalibr");
                updateWaveModel(currPoint);
            }
        }
        public string CurrPointInfo
        {
            get => _currPointInfo;
            set
            {
                _currPointInfo = value;
                RaisePropertyChanged("CurrPointInfo");
            }
        }
        private Data_Map currData
        {
            get => _currData;
            set
            {
                _currData = value;
                updateMapModel();
                updateWaveModel(currPoint);
            }
        }
        private DataPoint currPoint
        {
            get => _currPoint;
            set
            {
                _currPoint = value;
                updateWaveModel(currPoint);
            }
        }

        public MainViewModel()
        {
            WaveModel = DEF_WAVEMODEL;
            MapModel = DEF_MAPMODEL;
            currPoint = new DataPoint(0, 0);

            MapModel.TrackerChanged += (s, e) =>
            {
                if (e.HitResult != null && currData != null)
                {
                    var p = e.HitResult.DataPoint;
                    if (!currData.Map.IsPointInMatrix((int)p.X, (int)p.Y)) return;
                    currPoint = p;
                }                
            };
        }

        public ICommand SaveCmd => new RelayCommand(onSave, () => currData != null);
        public ICommand LoadCmd => new RelayCommand(onLoad);
        public ICommand WaveAnalyseCmd => new RelayCommand(() =>
        {
            WaveAnalyse wa;
            if (currData == null)
            {
                wa = new WaveAnalyse();
                wa.Show();
            }
            else
            {
                int currX = (int)currPoint.X, currY = (int)currPoint.Y;
                var intensity = currData.WaveMatrix[currX, currY].Select(_ => (double)_);
                wa = new WaveAnalyse(new Data_1D(intensity, currData.WaveLengthCalibr, currData.RamanShiftCalibr));
                wa.Show();
            }
        });
        public ICommand MapAnalyseCmd => new RelayCommand(() =>
        {
            MapAnalyse ma;    
            if (currData == null)
            {
                ma = new MapAnalyse();
                ma.Show();
            }
            else
            {
                ma = new MapAnalyse(currData.DeepCopy());
                ma.Show();
            }
        });
        public ICommand CameraControlCmd => new RelayCommand(() =>
        {
            CameraControl.CameraControl ca;
            if (currData == null)
            {
                ca = new CameraControl.CameraControl();
                ca.Show();
            }
            else
            {
                ca = new CameraControl.CameraControl();
                ca.Show();
            }
        });
        public ICommand SettingCmd => new RelayCommand<string>(para => 
        {
            if (para == "set")
            {
                if (PROPDLG.ShowDialog().Value)
                    SETTINGS.Save();
            }
            else if (para == "reset")
                SETTINGS.Reset();
            updateMapModel();
        });
        private async void onSave()
        {
            if (currData == null) return;
            string path = SaveFilePath(_saveExts);
            if (path == null) return;
            _originData = currData.DeepCopy();
            try
            {
                switch (Path.GetExtension(path))
                {
                    case ".mdat":
                        byte[] bytes = await currData.CompressToBytesAsync();
                        File.WriteAllBytes(path, bytes);
                        break;
                    case ".txt":
                        string text = await Task.Run(() => currData.ToString());
                        File.WriteAllText(path, text);
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "MainWindow");
            }
        }
        private async void onLoad()
        {
            string path = OpenFilePath(_loadExts);
            if (path == null) return;
            try
            {
                switch (Path.GetExtension(path))
                {
                    case ".mdat_t":
                        var path2 = OpenFilePath(".2dat_t", ".2dat");
                        if (path2 == null)
                        {
                            System.Windows.MessageBox.Show("Please select a .2dat/.2dat_t file.", "MainWindow");
                            break;
                        }
                        if (Path.GetExtension(path2) == ".2dat_t")
                        {
                            currData = await Data_Map.ImportFromTxtAsync(File.ReadLines(path), File.ReadLines(path2));
                            _originData = currData.DeepCopy();
                        }
                        else if (Path.GetExtension(path2) == ".2dat")
                        {
                            currData = await Data_Map.ImportFromTxtAsync(File.ReadLines(path), (Data_2D)DecompressFromBytes(File.ReadAllBytes(path)));
                            _originData = currData.DeepCopy();
                        }
                        break;
                    case ".mdat":
                        currData = (Data_Map)await DecompressFromBytesAsync(File.ReadAllBytes(path));
                        _originData = currData.DeepCopy();
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString(), "MainWindow");
            }
        }

        private void updateMapModel()
        {
            if (currData == null)
                return;
            MapModel.ResetAllAxes();
            WaveModel.ResetAllAxes();
            MapModel.Series.Clear();
            MapModel.Series.Add(new HeatMapSeries
            {
                Data = currData.Map,
                X0 = 0,
                X1 = currData.XSize,
                Y0 = 0,
                Y1 = currData.YSize,
                Interpolate = SETTINGS.Interpolate,
            });
            MapModel.Axes[2] = new LinearColorAxis { Position = AxisPosition.Right, Palette = GenPalette(SETTINGS.PaletteName, SETTINGS.PaletteSize), };
            MapModel.InvalidatePlot(true);
        }

        private void updateWaveModel(DataPoint point)
        {
            if (currData == null) return;
            int currX = (int)currPoint.X, currY = (int)currPoint.Y;
            IEnumerable<DataPoint> newPoints = null;
            switch (CurrCalibr)
            {
                case CalibrType.Pixel:
                    newPoints = currData.WaveMatrix[currX, currY].GenDataPoints();
                    _xAxis.Unit = "Count";
                    break;
                case CalibrType.RamanShift:
                    newPoints = currData.WaveMatrix[currX, currY].GenDataPoints(currData.RamanShiftCalibr);
                    _xAxis.Unit = "1/cm";
                    break;
                case CalibrType.WaveLength:
                    newPoints = currData.WaveMatrix[currX, currY].GenDataPoints(currData.WaveLengthCalibr);
                    _xAxis.Unit = "nm";
                    break;
            }
            _yAxis.Title = "Intensity";
            _yAxis.Unit = "Count";
            _xAxis.Title = CurrCalibr.ToString();
            CurrPointInfo = $"({currX}, {currY}, {currData.Map[currX, currY]})";
            
            if (WaveModel.Series.Count == 0)
            {
                var ls = DEF_LINESERIES;
                ls.Points.AddRange(newPoints);     
                WaveModel.Series.Add(ls);
            }
            else if (WaveModel.Series.Count == 1)
            {
                var points = (WaveModel.Series[0] as LineSeries).Points;
                points.Clear();
                points.AddRange(newPoints);    
            }         
            WaveModel.ResetAllAxes();
            WaveModel.InvalidatePlot(true);
        }
    }
}
