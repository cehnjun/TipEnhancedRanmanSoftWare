using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MicroMvvm;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace TERS
{
    using OxyPlot.Wpf;
    using System.Drawing;
    using System.IO;
    using static StaticUtils;
    /// <summary>
    /// MapMeasurement.xaml 的交互逻辑
    /// </summary>
    public partial class MapMeasurement : System.Windows.Window
    {
        public MMViewModel VM { get; }
        private Action _onClosed;

        public MapMeasurement(Action onClosed)
        {
            InitializeComponent();
            VM = new MMViewModel();
            _onClosed = onClosed;
            DataContext = VM;
        }

        public class MMViewModel : ObservableObject
        {
            private PlotModel _model = DEF_WAVEMODEL;
            private Data_1D _data;
            private readonly string[] _saveExts = new string[] { ".txt", ".1dat", ".png", };

            public PlotModel Model
            {
                get { return _model; }
                private set
                {
                    _model = value;
                    RaisePropertyChanged("Model");
                }
            }

            public ICommand SaveCmd => new RelayCommand(()=> {
                string path = SaveFilePath(_saveExts);
                if (path == null || _data == null) return;
                try
                {
                    switch (Path.GetExtension(path))
                    {
                        case ".1dat":
                            byte[] bytes = _data.CompressToBytes();
                            File.WriteAllBytes(path, bytes);
                            break;
                        case ".txt":
                            string text = _data.ToString();
                            File.WriteAllText(path, text);
                            break;
                        case ".png":
                            ((PlotView)Model.PlotView).SaveBitmap(path);
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message, "MapMeasurement");
                }
            });

            public void Update(IEnumerable<double> data, string title = "")
            {
                var ls = DEF_LINESERIES;
                _data = new Data_1D(data);
                ls.Points.AddRange(data.GenDataPoints());
                Model.Series.Clear();
                Model.Series.Add(ls);
                Model.Title = title;
                Model.InvalidatePlot(true);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _onClosed?.Invoke();
        }
    }
}
