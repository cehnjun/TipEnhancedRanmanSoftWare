using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TERS
{
    using OxyPlot;
    using System.Text.RegularExpressions;
    using System.Windows;
    using static StaticUtils;

    [Serializable]
    public class Data_Map : Data_2D, IData, IDeepCopy<Data_Map>
    {
        private IEnumerable<int>[,] _waveMatrix;
        private IEnumerable<double> _waveLengthCalibr;
        private IEnumerable<double> _ramanShiftCalibr;
        private IEnumerable<double> _xPos;
        private IEnumerable<double> _yPos;

        public IEnumerable<double> YPos { get => _yPos; private set => _yPos = value; }
        public IEnumerable<double> XPos { get => _xPos; private set => _xPos = value; }
        public IEnumerable<double> RamanShiftCalibr { get => _ramanShiftCalibr; private set => _ramanShiftCalibr = value; }
        public IEnumerable<double> WaveLengthCalibr { get => _waveLengthCalibr; private set => _waveLengthCalibr = value; }
        public IEnumerable<int>[,] WaveMatrix { get => _waveMatrix; private set => _waveMatrix = value; }

        public Data_Map(Data_2D data2D, IEnumerable<int>[,] waveMatrix,
            IEnumerable<double> waveLengthCalibr, IEnumerable<double> ramanShiftCalibr,
            IEnumerable<double> xPos, IEnumerable<double> yPos) : base(data2D)
        {
            WaveMatrix = waveMatrix;
            WaveLengthCalibr = waveLengthCalibr;
            RamanShiftCalibr = ramanShiftCalibr;
            XPos = xPos;
            YPos = yPos;
        }

        public new void IEnumsToList()
        {
            base.IEnumsToList();
            for (int i = 0; i < XSize; i++)
                for (int j = 0; j < YSize; j++)
                    WaveMatrix[i, j].ToList();
            WaveLengthCalibr.ToList();
            RamanShiftCalibr.ToList();
            XPos.ToList();
            YPos.ToList();
        }

        public new Data_Map DeepCopy()
        {
            IEnumsToList();
            return (Data_Map)SerializeObjectClone(this);
        }

        public override string ToString()
        {
            var sb = new StringBuilder(base.ToString());
            sb.AppendLine("#WaveLengthCalibr");
            sb.AppendLine(string.Join(",", WaveLengthCalibr));
            sb.AppendLine("#RamanShiftCalibr");
            sb.AppendLine(string.Join(",", RamanShiftCalibr));
            sb.AppendLine("#XPos");
            sb.AppendLine(string.Join(",", XPos));
            sb.AppendLine("#YPos");
            sb.AppendLine(string.Join(",", YPos));
            sb.AppendLine("#WaveMatrix");
            for (int j = 0; j < YSize; j++)
                for (int i = 0; i < XSize; i++)
                    sb.AppendLine(string.Join(",", WaveMatrix[i, j]));
            return sb.ToString();
        }

        public new string GetExtension() => ".mdat";

        #region static region
        public static Data_Map ImportFromTxt(IEnumerable<string> lines, IEnumerable<string> lines_2D)
        {
            return ImportFromTxt(lines, Data_2D.ImportFromTxt(lines_2D));
        }

        public static Data_Map ImportFromTxt(IEnumerable<string> lines, Data_2D data_2D)
        {
            int xSize = 0xFF, ySize = 0xFF, length = 0xFF;
            foreach (var line in lines)
            {
                if (line.StartsWith("Map = zeros"))
                {
                    var t = (from Match match in Regex.Matches(line, @"\d+") select int.Parse(match.Value)).ToList();
                    ySize = t[0];
                    xSize = t[1];
                    length = t[2];
                    break;
                }
            }
            if (data_2D.XSize != xSize || data_2D.YSize != ySize)
                throw new ArgumentException();

            var waveMatrix = new List<int>[xSize, ySize];
            for (int i = 0; i < xSize; i++)
                for (int j = 0; j < ySize; j++)
                    waveMatrix[i, j] = new List<int>(length);

            var waveLengthCalibr = new List<double>(length);
            var ramanShiftCalibr = new List<double>(length);
            var xPos = new List<double>(xSize);
            var yPos = new List<double>(ySize);

            double tmpd = 0;
            int tmps = 0;
            var tmpl = new List<int>(xSize * ySize);

            foreach (var line in lines)
            {
                if (line.StartsWith("Map("))
                {
                    tmpl.Clear();
                    tmpl.AddRange(from s in line.Split(new string[] { ".0000 ", ";", "[", "]" }, StringSplitOptions.RemoveEmptyEntries)
                                  where int.TryParse(s, out tmps)
                                  select tmps);
                    int count = 0;
                    for (int j = 0; j < ySize; j++)
                        for (int i = 0; i < xSize; i++)
                            waveMatrix[i, j].Add(tmpl[count++]);
                    continue;
                }
                if (line.StartsWith("X ="))
                {
                    xPos.AddRange(from s in line.Split('[', ' ', ']')
                                  where double.TryParse(s, out tmpd)
                                  select tmpd);
                    continue;
                }
                if (line.StartsWith("Y ="))
                {
                    yPos.AddRange(from s in line.Split('[', ' ', ']')
                                  where double.TryParse(s, out tmpd)
                                  select tmpd);
                    continue;
                }
                if (line.StartsWith("WavelengthCalibr"))
                {
                    waveLengthCalibr.AddRange(from s in line.Split('[', ' ', ']')
                                              where double.TryParse(s, out tmpd)
                                              select tmpd);
                    continue;
                }
                if (line.StartsWith("RamanShiftCalibr"))
                {
                    ramanShiftCalibr.AddRange(from s in line.Split('[', ' ', ']')
                                              where double.TryParse(s, out tmpd)
                                              select tmpd);
                    continue;
                }
            }

            return new Data_Map(data_2D, waveMatrix, waveLengthCalibr, ramanShiftCalibr, xPos, yPos);
        }

        public async static Task<Data_Map> ImportFromTxtAsync(IEnumerable<string> lines, IEnumerable<string> lines_2D)
            => await Task.FromResult(ImportFromTxt(lines, lines_2D));

        public async static Task<Data_Map> ImportFromTxtAsync(IEnumerable<string> lines, Data_2D data2D)
            => await Task.FromResult(ImportFromTxt(lines, data2D));
        #endregion
    }
}
