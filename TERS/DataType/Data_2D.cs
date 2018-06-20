using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TERS
{
    using System.IO;
    using System.Text.RegularExpressions;
    using static StaticUtils;

    [Serializable]
    public class Data_2D : IData, IDeepCopy<Data_2D>
    {
        public int XSize { get { return Map.GetLength(0); } }
        public int YSize { get { return Map.GetLength(1); } }

        private double _xShift;
        private double _xScale;
        private LengthUnit _xUnit;
        private double _yShift;
        private double _yScale;
        private LengthUnit _yUnit;
        private double _zShift;
        private double _zScale;
        private LengthUnit _zUnit;
        private double[,] _map;

        public double XShift { get => _xShift; set => _xShift = value; }
        public double XScale { get => _xScale; set => _xScale = value; }
        public LengthUnit XUnit { get => _xUnit; set => _xUnit = value; }
        public double YShift { get => _yShift; set => _yShift = value; }
        public double YScale { get => _yScale; set => _yScale = value; }
        public LengthUnit YUnit { get => _yUnit; set => _yUnit = value; }
        public double ZShift { get => _zShift; set => _zShift = value; }
        public double ZScale { get => _zScale; set => _zScale = value; }
        public LengthUnit ZUnit { get => _zUnit; set => _zUnit = value; }
        public double[,] Map { get => _map; set => _map = value; }

        public Data_2D(double xShift, double xScale, LengthUnit xUnit, double yShift, double yScale,
            LengthUnit yUnit, double zShift, double zScale, LengthUnit zUnit, double[,] map)
        {
            XShift = xShift;
            XScale = xScale;
            XUnit = xUnit;
            YShift = yShift;
            YScale = yScale;
            YUnit = yUnit;
            ZShift = zShift;
            ZScale = zScale;
            ZUnit = zUnit;
            Map = map;
        }

        public Data_2D(double[,] map)
        {
            Map = map;
        }

        protected Data_2D(Data_2D data)
        {
            XShift = data.XShift;
            XScale = data.XScale;
            XUnit = data.XUnit;
            YShift = data.YShift;
            YScale = data.YScale;
            YUnit = data.YUnit;
            ZShift = data.ZShift;
            ZScale = data.ZScale;
            ZUnit = data.ZUnit;
            Map = data.Map;
        }

        public Data_2D DeepCopy() => (Data_2D)SerializeObjectClone(this);

        public void IEnumsToList() { }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("#");
            sb.AppendLine(string.Join(",", "XShift", "XScale", "XUnit", "YShift", "YScale", "YUnit", "ZShift", "ZScale", "ZUnit"));
            sb.AppendLine(string.Join(",", XShift, XScale, XUnit, YShift, YScale, YUnit, ZShift, ZScale, ZUnit));
            sb.AppendLine("#Map Matrix");
            var tmp = new List<double>();
            for (int j = 0; j < YSize; j++)
            {             
                for (int i = 0; i < XSize; i++)
                    tmp.Add(Map[i, j]);
                sb.AppendLine(string.Join(",", tmp));
                tmp.Clear();
            }
            return sb.ToString();          
        }

        public string GetExtension() => ".2dat";

        #region static region
        public static Data_2D ImportFromTxt(IEnumerable<string> lines)
        {
            string name = "";
            var unitDic = new SortedList<string, LengthUnit>
            {
                { "um", LengthUnit.um },
                { "Angstrom", LengthUnit.angstrom },
                { "", LengthUnit.na },
            };

            var tokenDic = new SortedList<string, string>
            {
                {"XSize", "" }, {"YSize", "" },
                {"XShift", "" }, {"XScale", "" }, {"XUnits", "" },
                {"YShift", "" }, {"YScale", "" }, {"YUnits", "" },
                {"ZShift", "" }, {"ZScale", "" }, {"ZUnits", "" },
            };

            foreach (var line in lines)
            {
                if (line.StartsWith("%"))
                {
                    name = line.Split('%', ' ')[1];
                    continue;
                }
                var token = line.Split('=')[0];
                if (!tokenDic.Keys.Contains(token))
                    break;
                tokenDic[token] = line.Split('=', ';')[1].Trim('\'');
            }

            int xSize = int.Parse(tokenDic["XSize"]), ySize = int.Parse(tokenDic["YSize"]);
            double xShift = double.Parse(tokenDic["XShift"]), xScale = double.Parse(tokenDic["XScale"]),
                yShift = double.Parse(tokenDic["YShift"]), yScale = double.Parse(tokenDic["YScale"]),
                zShift = double.Parse(tokenDic["ZShift"]), zScale = double.Parse(tokenDic["ZScale"]);
            LengthUnit xUnit = unitDic[tokenDic["XUnits"]], yUnit = unitDic[tokenDic["YUnits"]], zUnit = unitDic[tokenDic["ZUnits"]];
            double[,] map = new double[xSize, ySize];

            name = "^" + name + @"|^(-?\d+)(\.\d+)?";
            int count = 0;
            double temd = 0;
            foreach (var line in lines)
            {
                if (!Regex.IsMatch(line, name))
                    continue;
                var entries = (from s in line.Split(' ', ';', '[', ']')
                               where double.TryParse(s, out temd)
                               select temd).ToArray();
                for (int i = 0; i < xSize; i++)
                    map[i, count] = entries[i];
                count++;
            }
            return new Data_2D(xShift, xScale, xUnit, yShift, yScale, yUnit, zShift, zScale, zUnit, map);
        }

        public async static Task<Data_2D> ImportFromTxtAsync(IEnumerable<string> lines)
            => await Task.FromResult(ImportFromTxt(lines));
        #endregion
    }
}
