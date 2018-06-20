using MicroMvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TERS
{
    using OxyPlot;
    using OxyPlot.Annotations;
    using PropertyTools.Wpf;
    using System.IO;
    using System.Windows;
    using System.Windows.Input;
    using static StaticUtils;
    public partial class MapAnalyseViewModel : ObservableObject
    {
        MeasurementOptions _currMeasurementOption;
        public MeasurementOptions CurrMeasurementOption
        {
            get => _currMeasurementOption;
            set
            {
                if (_currMeasurementOption != value)
                {
                    _currMeasurementOption = value;
                    onMeasurementChange(_currMeasurementOption);
                    RaisePropertyChanged("CurrMeasurementOption");
                }
            }
        }
        public enum MeasurementOptions : byte
        {
            None, MinMax, XSection, YSection, Arbitrary
        }

        EventHandler<OxyMouseDownEventArgs> onMouseDown;
        EventHandler<OxyMouseEventArgs> onMouseMove, onMouseUp;

        MapMeasurement _measurementWindow = null;
        ColorAxisChangeWindow _cacw = null;

        #region UI Command 
        public void OnExit()
        {
            if (_measurementWindow != null)
            {
                _measurementWindow.Close();
                _measurementWindow = null;
            }
            if (_cacw != null)
            {
                _cacw.Close();
                _cacw = null;
            }
        }
        public ICommand SaveCmd => new RelayCommand(onSave, () => currData != null);
        public ICommand LoadCmd => new RelayCommand(onLoad);
        public ICommand BCSCmd => new RelayCommand(onBCS, () => currData != null);
        public ICommand AdjBCS => 
            new RelayCommand(async () => currMatrix = await Task.Run(() => Matlab?.Adjust_BCS(currMatrix, false)), 
                () => currData != null);
        public ICommand PlotCmd => new RelayCommand<string>(onPlot, para => currData != null);
        public ICommand ResetCmd => new RelayCommand(() => { if (currData != null) currData = _originData.DeepCopy(); }, () => currData != null);
        public ICommand SettingCmd => new RelayCommand<string>(onSetting);
        public ICommand CriminisiCmd => new RelayCommand(onCriminisi, () => currData != null);
        public ICommand DeconvBCmd => new RelayCommand(onDeconvB, () => currData != null);
        public ICommand DeconvRCmd => new RelayCommand(onDeconvR, () => currData != null);
        public ICommand HistEQCmd => new RelayCommand(onHistEQ, () => currData != null);
        public ICommand ResizeCmd => new RelayCommand(onResize, () => currData != null);
        public ICommand RotateCmd => new RelayCommand<string>(onRotate, para => currData != null);
        public ICommand FilterCmd => new RelayCommand<string>(onFliter, para => currData != null);
        public ICommand ColorAxisChangeCmd => new RelayCommand(onColorAxisChange, () => currData != null);
        public ICommand TVCmd => new RelayCommand(async () =>
        {
            if (currData == null) return;
            currMatrix = await Task.Run(() => Matlab?.TVRepair(currMatrix, SETTINGS.TV_P)) ?? currMatrix;
        }, () => currData != null);

        private void onMeasurementChange(MeasurementOptions para)
        {
            if (currData == null) return;
            MapModel.Annotations.Clear();
            MapModel.InvalidatePlot(false);
            MapModel.MouseDown -= onMouseDown;
            MapModel.MouseMove -= onMouseMove;
            MapModel.MouseUp -= onMouseUp;
            switch (para)
            {
                case MeasurementOptions.None:
                    if (_measurementWindow != null)
                    {
                        _measurementWindow.Close();
                        _measurementWindow = null;
                    }
                    break;
                case MeasurementOptions.MinMax:
                    minMax();
                    break;
                case MeasurementOptions.XSection:
                    xySection(LineAnnotationType.Vertical);
                    break;
                case MeasurementOptions.YSection:
                    xySection(LineAnnotationType.Horizontal);
                    break;
                case MeasurementOptions.Arbitrary:
                    arbitrary();
                    break;
            }

            void arbitrary()
            {
                if (_measurementWindow == null)
                {
                    _measurementWindow = new MapMeasurement(() => { CurrMeasurementOption = MeasurementOptions.None; });
                    _measurementWindow.Show();
                }
                ArrowAnnotation arrow = null;
                onMouseDown = (s, e) =>
                {
                    if (e.ChangedButton == OxyMouseButton.Left)
                    {
                        var startPoint = MapModel.Axes[0].InverseTransform(e.Position.X, e.Position.Y, MapModel.Axes[1]);
                        var endPoint = startPoint;
                        if (!currMatrix.IsPointInMatrix((int)startPoint.X, (int)startPoint.Y)) return;
                        arrow = DEF_ARROW;
                        arrow.StartPoint = startPoint;
                        arrow.EndPoint = endPoint;
                        MapModel.Annotations.Clear();
                        MapModel.Annotations.Add(arrow);
                        e.Handled = true;
                    }
                };
                onMouseMove = (s, e) =>
                {
                    if (arrow != null)
                    {
                        arrow.EndPoint = MapModel.Axes[0].InverseTransform(e.Position.X, e.Position.Y, MapModel.Axes[1]);
                        int x1 = (int)arrow.StartPoint.X, y1 = (int)arrow.StartPoint.Y, x2 = (int)arrow.EndPoint.X, y2 = (int)arrow.EndPoint.Y;
                        if (!currMatrix.IsPointInMatrix(x2, y2)) return;
                        MapModel.InvalidatePlot(false);
                        _measurementWindow.VM.Update(currMatrix.GetMatrixDiag(x1, y1, x2, y2), $"From ({x1}, {y1}) to ({x2}, {y2})");
                        e.Handled = true;
                    }
                };
                onMouseUp = (s, e) =>
                {
                    if (arrow != null)
                    {
                        arrow = null;
                        e.Handled = true;
                    }
                };
                MapModel.MouseDown += onMouseDown;
                MapModel.MouseMove += onMouseMove;
                MapModel.MouseUp += onMouseUp;
            }
            void minMax()
            {
                var minValue = currMatrix.Min2D();
                var maxValue = currMatrix.Max2D();
                for (int i = 0; i < currMatrix.GetLength(0); i++)
                    for (int j = 0; j < currMatrix.GetLength(1); j++)
                        if (currMatrix[i, j] == minValue)
                        {
                            MapModel.Annotations.Add(new PointAnnotation
                            {
                                X = i,
                                Y = j,
                                Text = $"Min(s):{minValue:f3} at ({i}, {j})",
                                Shape = MarkerType.Cross,
                                Stroke = OxyColors.Blue,
                                StrokeThickness = 2,
                            });
                        }
                        else if (currMatrix[i, j] == maxValue)
                        {
                            MapModel.Annotations.Add(new PointAnnotation
                            {
                                X = i,
                                Y = j,
                                Text = $"Max(s):{maxValue:f3} at ({i}, {j})",
                                Shape = MarkerType.Cross,
                                Stroke = OxyColors.Red,
                                StrokeThickness = 2,
                            });
                        }
                MapModel.InvalidatePlot(false);
            }
            void xySection(LineAnnotationType type)
            {
                if (_measurementWindow == null)
                {
                    _measurementWindow = new MapMeasurement(() => { CurrMeasurementOption = MeasurementOptions.None; });
                    _measurementWindow.Show();
                }
                LineAnnotation la = new LineAnnotation
                {
                    Type = type,
                    StrokeThickness = 2,
                    Color = OxyColors.Red,
                    LineStyle = LineStyle.Solid
                };

                if (type == LineAnnotationType.Vertical)
                {
                    la.X = currMatrix.GetLength(0) / 2;
                    _measurementWindow.VM.Update(currMatrix.SelectVectorFormMatrix(0, (int)la.X), $"X={(int)la.X}");
                }
                else
                {
                    la.Y = currMatrix.GetLength(1) / 2;
                    _measurementWindow.VM.Update(currMatrix.SelectVectorFormMatrix(1, (int)la.Y), $"Y={(int)la.Y}");
                }

                la.MouseDown += (s, e) =>
                {
                    if (e.ChangedButton == OxyMouseButton.Left)
                    {
                        la.StrokeThickness *= 2;
                        MapModel.InvalidatePlot(false);
                        e.Handled = true;
                    }
                };
                la.MouseMove += (s, e) =>
                {
                    var dp = la.InverseTransform(e.Position);
                    if (!currMatrix.IsPointInMatrix((int)dp.X, (int)dp.Y)) return;
                    if (type == LineAnnotationType.Vertical)
                    {
                        la.X = dp.X;
                        _measurementWindow.VM.Update(currMatrix.SelectVectorFormMatrix(0, (int)la.X), $"X={(int)la.X}");
                    }
                    else
                    {
                        la.Y = dp.Y;
                        _measurementWindow.VM.Update(currMatrix.SelectVectorFormMatrix(1, (int)la.Y), $"Y={(int)la.Y}");
                    }
                    MapModel.InvalidatePlot(false);
                    e.Handled = true;
                };
                la.MouseUp += (s, e) =>
                {
                    la.StrokeThickness /= 2;
                    MapModel.InvalidatePlot(false);
                    e.Handled = true;
                };

                MapModel.Annotations.Add(la);
                MapModel.InvalidatePlot(false);
            }
        }       
        private async void onFliter(string para)
        {
            if (currData == null) return;
            switch (para)
            {
                case "Bwt":
                    currMatrix =
                        await Task.Run(() => Matlab?.Bwt_Notchfilt(currMatrix.RotateMatrixCounterClockwise(), SETTINGS.BWT_C, SETTINGS.BWT_D, SETTINGS.BWT_Showfig).RotateMatrixClockwise());
                    break;
                case "Sp":
                    currMatrix =
                        await Task.Run(() => Matlab?.Spfilter(currMatrix, SETTINGS.Sptype, SETTINGS.Spm, SETTINGS.Spn, SETTINGS.Sppara));
                    break;
                case "Fq":
                    currMatrix =
                        await Task.Run(() => Matlab?.Fqfilter(currMatrix, SETTINGS.Fqflag, SETTINGS.Fqtype, SETTINGS.Fqd0, SETTINGS.Fqn));
                    break;
                default:
                    break;
            }
        }
        private void onColorAxisChange()
        {
            if (currData == null) return;
            var originAxis = (OxyPlot.Axes.LinearColorAxis)MapModel.Axes.Last();
            _cacw = new ColorAxisChangeWindow((lower, higher) =>
            {
                MapModel.Axes[2] = new OxyPlot.Axes.LinearColorAxis { Position = OxyPlot.Axes.AxisPosition.Right, Palette = ChangePalette(originAxis.Palette, lower, higher), };
                MapModel.InvalidatePlot(true);
            });
            _cacw.Show();
        }
        private void onSetting(string p)
        {
            if (p == "set")
            {
                if (PROPDLG.ShowDialog().Value)
                    SETTINGS.Save();
            }
            else if (p == "reset")
                SETTINGS.Reset();
            updateMapModel();
        }
        private async void onSave()
        {
            if (currData == null) return;
            string path = SaveFilePath(_saveExts);
            if (path == null ) return;
            _originData = currData.DeepCopy();
            try
            {
                switch (Path.GetExtension(path))
                {
                    case ".2dat":
                        byte[] bytes = await currData.CompressToBytesAsync();
                        File.WriteAllBytes(path, bytes);
                        break;
                    case ".txt":
                        string text = await Task.Run(() => currData.ToString());
                        File.WriteAllText(path, text);
                        break;
                    case ".png":
                        ((OxyPlot.Wpf.PlotView)MapModel.PlotView).SaveBitmap(path);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "MapAnalyse");
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
                    case ".2dat_t":
                        currData = await Data_2D.ImportFromTxtAsync(File.ReadAllLines(path));
                        _originData = currData.DeepCopy();
                        break;
                    case ".2dat":
                        currData = (Data_2D)DecompressFromBytes(File.ReadAllBytes(path));
                        _originData = currData.DeepCopy();
                        break;
                    case ".mat_t":
                        currData = new Data_2D(File.ReadAllLines(path)?.LoadMatrixFromCSV()?.MatrixTranspose());
                        _originData = currData.DeepCopy();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "MapAnalyse");
            }
        }
        private async void onBCS()
        {
            if (currData == null) return;
            currMatrix = await Task.Run(() => Matlab?.BCS_O(currMatrix));
        }
        private async void onPlot(string cmdPara)
        {
            if (currData == null)
                return;
            switch (cmdPara)
            {
                case "Surf":
                    await Task.Run(() => Matlab?.Surf(currMatrix));
                    break;
                case "Mesh":
                    await Task.Run(() => Matlab?.Mesh(currMatrix));
                    break;
                default:
                    break;
            }
        }
        private async void onDeconvB()
        {
            if (currData == null) return;
            currMatrix = await Task.Run(() => Matlab?.DeconvB(currMatrix, SETTINGS.BT, SETTINGS.LT));
        }
        private async void onDeconvR()
        {
            if (currData == null) return;
            currMatrix = await Task.Run(() => Matlab?.DeconvR(currMatrix, SETTINGS.DS, SETTINGS.ST, SETTINGS.TT, SETTINGS.NP, SETTINGS.BCSFlag));
        }
        private async void onHistEQ()
        {
            if (currData == null) return;
            currMatrix = await Task.Run(() => Matlab?.HistEQ(currMatrix, SETTINGS.N));
        }
        private async void onResize()
        {
            if (currData == null) return;
            currMatrix = await Task.Run(() => Matlab?.Resize(currMatrix, SETTINGS.Scale, SETTINGS.Method));
        }
        private async void onCriminisi()
        {
            if (currData == null) return;
            currMatrix = await Task.Run(() => Matlab?.Criminisi(currMatrix, SETTINGS.ShowFig)) ?? currMatrix;
        }
        private void onRotate(string para)
        {
            if (currData == null) return;
            if (para == "RC")
                currMatrix = currMatrix.RotateMatrixClockwise();
            else if (para == "RCC")
                currMatrix = currMatrix.RotateMatrixCounterClockwise();
            else if (para == "TR")
                currMatrix = currMatrix.MatrixTranspose();
            return;
        }
        #endregion
    }
}
