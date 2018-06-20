using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace TERS
{
    using static StaticUtils;

    [Serializable]
    public class Data_1D : IData, IDeepCopy<Data_1D>
    {
        private IEnumerable<double> _waveLengthCalibr = null;
        private IEnumerable<double> _ramanShiftCalibr = null;
        private IEnumerable<double> _intensity;

        public int Length => Intensity.Count();
        public IEnumerable<double> Intensity { get => _intensity; set => _intensity = value; }
        public IEnumerable<double> RamanShiftCalibr { get => _ramanShiftCalibr; set => _ramanShiftCalibr = value; }
        public IEnumerable<double> WaveLengthCalibr { get => _waveLengthCalibr; set => _waveLengthCalibr = value; }

        public Data_1D(IEnumerable<double> intensity, IEnumerable<double> waveLengthCalibr = null, 
            IEnumerable<double> ramanShiftCalibr = null)
        {
            WaveLengthCalibr = waveLengthCalibr;
            RamanShiftCalibr = ramanShiftCalibr;
            Intensity = intensity ?? throw new ArgumentNullException();
        }

        public Data_1D DeepCopy()
        {
            IEnumsToList();
            return (Data_1D)SerializeObjectClone(this);
        }

        public void IEnumsToList()
        {
            WaveLengthCalibr = WaveLengthCalibr?.ToList();
            RamanShiftCalibr = RamanShiftCalibr?.ToList();
            Intensity = Intensity.ToList();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();           
            sb.AppendLine("#Intensity");
            sb.AppendLine(string.Join(",", Intensity));
            if (WaveLengthCalibr != null)
            {
                sb.AppendLine("#WaveLengthCalibr");
                sb.AppendLine(string.Join(",", WaveLengthCalibr));
            }
            if (RamanShiftCalibr != null)
            {
                sb.AppendLine("#RamanShiftCalibr");
                sb.AppendLine(string.Join(",", RamanShiftCalibr));
            }
            return sb.ToString();
        }

        public string GetExtension() => ".1dat";

        #region static region

        public static Data_1D ImportFromTxt(IEnumerable<string> lines)
        {
            IEnumerable<double> _waveLengthCalibr = null; 
            IEnumerable<double> _ramanShiftCalibr = null;
            IEnumerable<double> _intensity;

            var type = CalibrType.Pixel;
            foreach (var line in lines)
            {
                if (line.StartsWith("X=Wavelength"))
                {
                    type = CalibrType.WaveLength;
                    break;
                }
                else if (line.StartsWith("X=Raman shift"))
                {
                    type = CalibrType.RamanShift;
                    break;
                }
                else if (line.StartsWith("X=Wave"))
                {
                    type = CalibrType.Pixel;
                    break;
                }
                else if (Regex.IsMatch(line, @"^\d"))
                    break;
            }

            var entrys = from line in lines
                         where Regex.IsMatch(line, @"^\d")
                         select new
                         {
                             calibr = double.Parse(line.Split(';')[0]),
                             intensity = double.Parse(line.Split(';')[1])
                         };

            _intensity = from entry in entrys select entry.intensity;
            switch (type)
            {
                case CalibrType.Pixel:
                    break;
                case CalibrType.RamanShift:
                    _ramanShiftCalibr = from entry in entrys select entry.calibr;
                    break;
                case CalibrType.WaveLength:
                    _waveLengthCalibr = from entry in entrys select entry.calibr;
                    break;
            }

            return new Data_1D(_intensity, _waveLengthCalibr, _ramanShiftCalibr);
        }

        public async static Task<Data_1D> ImportFromTxtAsync(IEnumerable<string> lines)
            => await Task.FromResult(ImportFromTxt(lines));
        #endregion
    }
}
