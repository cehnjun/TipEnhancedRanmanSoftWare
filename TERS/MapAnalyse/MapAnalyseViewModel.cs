using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroMvvm;
using Microsoft.Win32;
using System.Windows.Input;
using System.IO;
using TERS.Properties;

namespace TERS
{
    using OxyPlot;
    using OxyPlot.Axes;
    using OxyPlot.Series;
    using static StaticUtils;

    public partial class MapAnalyseViewModel : ObservableObject
    {
        private Data_2D _currData;
        private Data_2D _originData;
        private PlotModel _mapModel;
        private string _message = "Ready"; 
        private readonly string[] _saveExts = new string[] { ".txt", ".2dat", ".png", };
        private readonly string[] _loadExts = new string[] { ".2dat", ".2dat_t", ".mat_t"};

        private double[,] currMatrix
        {
            get { return currData?.Map; }
            set { if (value != null) { currData.Map = value; updateMapModel(); } }
        }
        private Data_2D currData
        {
            get => _currData;
            set
            {
                _currData = value;
                updateMapModel();
            }
        }
        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                RaisePropertyChanged("Message");
            }
        }
        public PlotModel MapModel
        {
            get => _mapModel;
            private set
            {
                _mapModel = value;
                RaisePropertyChanged("MapModel");
            }
        }
        
        private void updateMapModel()
        {
            if (currData == null) return;           
            MapModel.ResetAllAxes();
            CurrMeasurementOption = MeasurementOptions.None;
            MapModel.Series.Clear();
            MapModel.Series.Add(new HeatMapSeries
            {
                Data = currMatrix,
                X0 = 0,
                X1 = currMatrix.GetLength(0),
                Y0 = 0,
                Y1 = currMatrix.GetLength(1),
                Interpolate = SETTINGS.Interpolate,
            });
            MapModel.Axes[2] = new LinearColorAxis { Position = AxisPosition.Right, Palette = GenPalette(SETTINGS.PaletteName, SETTINGS.PaletteSize), };
            MapModel.InvalidatePlot(true);
        }

        public MapAnalyseViewModel()
        {
            MapModel = DEF_MAPMODEL;
        }

        public MapAnalyseViewModel(Data_2D initData)
        {
            MapModel = DEF_MAPMODEL;
            currData = initData;
            _originData = initData.DeepCopy();
        }
    }
}
