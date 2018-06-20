using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TERS
{
    using OxyPlot;
    using OxyPlot.Axes;
    using OxyPlot.Series;
    using System.Drawing;
    using System.IO.Compression;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Windows.Media.Imaging;
    using static Path;
    using static Math;
    using MatlabApi;
    using Microsoft.Win32;
    using OxyPlot.Annotations;
    using System.Windows;

    public static class StaticUtils
    {
        public readonly static GlobalSetting SETTINGS = new GlobalSetting();
        public static PropertyTools.Wpf.PropertyDialog PROPDLG
        {
            get => new PropertyTools.Wpf.PropertyDialog
            {
                DataContext = SETTINGS,
                MaxHeight = SystemParameters.PrimaryScreenHeight * 0.7,
                Title = "GlobalSetting"
            };
        }   
        public static PlotModel DEF_WAVEMODEL
        {
            get
            {
                var pm = new PlotModel { PlotAreaBackground = OxyColors.LightGray, };
                pm.Axes.Add(new LinearAxis
                {
                    MajorGridlineStyle = LineStyle.Solid,
                    MinorGridlineStyle = LineStyle.Dash,
                    Position = AxisPosition.Left,
                    //Title = "Intensity",
                });
                pm.Axes.Add(new LinearAxis
                {
                    MajorGridlineStyle = LineStyle.Solid,
                    MinorGridlineStyle = LineStyle.Dash,
                    Position = AxisPosition.Bottom, 
                    //Title = "Count",
                });
                return pm;
            }
        }
        public static PlotModel DEF_MAPMODEL
        {
            get
            {
                var pm = new PlotModel { PlotAreaBackground = OxyColors.LightGray, PlotType = PlotType.Cartesian, };                              
                pm.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Title = "X", });
                pm.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Title = "Y", });
                pm.Axes.Add(new LinearColorAxis());
                return pm;
            }
        }
        public static LineSeries DEF_LINESERIES
        {
            get
            {
                return new LineSeries { Color = OxyColors.Red, StrokeThickness = 1, };
            }
        }
        public static ArrowAnnotation DEF_ARROW
        {
            get
            {
                return new ArrowAnnotation { HeadWidth = 2, StrokeThickness = 1, Color = OxyColors.Red, };
            }
        }      
        public static readonly BinaryFormatter FORMATTER = new BinaryFormatter();
        public static MatlabTool Matlab { get; internal set; }

        /// <summary>
        /// Generates a new path for duplicate filenames.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public static string GetNewPathForDupes(string path)
        {
            string directory = GetDirectoryName(path);
            string filename = GetFileNameWithoutExtension(path);
            string extension = GetExtension(path);
            int counter = 1;
            string newFullPath = path;
            while (File.Exists(newFullPath))
            {
                string newFilename = $"{filename}({counter}).{extension}";
                newFullPath = Combine(directory, newFilename);
                counter++;
            }
            return newFullPath;
        }

        public static string OpenFilePath(IEnumerable<string> exts) => getFilePath(exts, false);
        public static string OpenFilePath(params string[] exts) => getFilePath(exts, false);
        public static string SaveFilePath(IEnumerable<string> exts) => getFilePath(exts, true);
        public static string SaveFilePath(params string[] exts) => getFilePath(exts, true);
        private static string getFilePath(IEnumerable<string> exts, bool isSave)
        {
            var filter = string.Join("|", exts.Select(ext => $"{ext} files|*{ext}"));
            var dlg = isSave ? (FileDialog)new SaveFileDialog { Filter = filter }
                : new OpenFileDialog { Filter = filter };
            return dlg.ShowDialog().Value ? dlg.FileName : null;
        }
        public static IEnumerable<DataPoint> GenDataPoints(this IEnumerable<double> ys, IEnumerable<double> calibr = null)
            => calibr == null ? ys.Zip(Enumerable.Range(0, ys.Count()), (y, x) => new DataPoint(x, (double)y)) :
            ys.Zip(calibr, (y, x) => new DataPoint(x, (double)y));

        public static IEnumerable<DataPoint> GenDataPoints(this IEnumerable<int> ys, IEnumerable<double> calibr = null)
            => calibr == null ? ys.Zip(Enumerable.Range(0, ys.Count()), (y, x) => new DataPoint(x, (double)y)) :
            ys.Zip(calibr, (y, x) => new DataPoint(x, (double)y));

        /// <summary>
        /// 返回path目录下同名（不含扩展名）且扩展名合法的文件迭代器
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetSameNameFiles(string path, List<string> filter)
            => from fi in (new DirectoryInfo(GetDirectoryName(path))).GetFiles()
               where GetFileNameWithoutExtension(path) == GetFileNameWithoutExtension(fi.Name)
               where filter.Contains(GetExtension(fi.Name))
               orderby GetExtension(fi.Name)
               select fi.FullName;

        /// <summary>
        /// 序列化克隆object
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static object SerializeObjectClone(object o)
        {
            var ms = new MemoryStream();
            FORMATTER.Serialize(ms, o);
            ms.Position = 0;
            return FORMATTER.Deserialize(ms);
        }

        public enum LengthUnit : byte
        {
            angstrom, nm, um, m, na, 
        }

        public enum CalibrType : byte
        {
            Pixel, WaveLength, RamanShift
        }

        public enum PaletteName : byte
        {
            BlueWhiteRed, BlackWhiteRed, Cool, Gray, Hot, Hue, HueDistinct, Jet, Rainbow,
        }

        public static OxyPalette GenPalette(PaletteName paletteName, int size)
        {
            return (OxyPalette)typeof(OxyPalettes)
                .InvokeMember(paletteName.ToString(), System.Reflection.BindingFlags.InvokeMethod, null, null, new object[] { size });
        }

        public static OxyPalette GenPalette(IEnumerable<OxyColor> colors, int size)
        {
            return OxyPalette.Interpolate(size, colors.ToArray());
        }

        public static OxyPalette ChangePalette(OxyPalette originPalette, int startPoint = 0, int endPoint = 100)
        {
            if (startPoint < 0 || endPoint > 100 || endPoint - startPoint < 0)
                throw new ArgumentException();
            if (startPoint == endPoint)
            {
                if (startPoint == 0)
                    endPoint++;
                else
                    startPoint--;
            }
            var originSize = originPalette.Colors.Count();
            var skipCount1 = originSize / 100 * startPoint;
            var skipCount2 = originSize / 100 * (100 - endPoint);
            return GenPalette(Enumerable.Repeat(originPalette.Colors.First(), skipCount1).Concat(originPalette.Colors).Concat(Enumerable.Repeat(originPalette.Colors.Last(), skipCount2)), originSize);
        }

        public static byte[] CompressToBytes(this IData data)
        {
            var ms = new MemoryStream();
            data.IEnumsToList();
            FORMATTER.Serialize(ms, data);
            ms.Position = 0;
            var outms = new MemoryStream();
            using (var gzs = new GZipStream(outms, CompressionLevel.Optimal, false))
            {
                ms.CopyTo(gzs);
            }
            return outms.ToArray();
        }

        public static async Task<byte[]> CompressToBytesAsync(this IData data)
            => await Task.FromResult(data.CompressToBytes());

        public static object DecompressFromBytes(byte[] bytes)
        {
            var ms = new MemoryStream(bytes);
            return FORMATTER.Deserialize(new GZipStream(ms, CompressionMode.Decompress));
        }

        public static async Task<object> DecompressFromBytesAsync(byte[] bytes) 
            => await Task.Run(() => DecompressFromBytes(bytes));

        public static BitmapSource Convert2BitmapSource(this Bitmap source)
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                          source.GetHbitmap(),
                          IntPtr.Zero,
                          System.Windows.Int32Rect.Empty,
                          BitmapSizeOptions.FromEmptyOptions());
        }

        public static Bitmap BitmapFromSource(this BitmapSource bitmapsource)
        {
            Bitmap bitmap;
            using (var outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapsource));
                enc.Save(outStream);
                bitmap = new Bitmap(outStream);
            }
            return bitmap;
        }

        public static T[,] MatrixTranspose<T>(this T[,] matrix) where T : struct
        {
            if (matrix == null) return null;
            int w = matrix.GetLength(0), h = matrix.GetLength(1);
            var res = new T[h, w];
            for (int i = 0; i < w; i++)
                for (int j = 0; j < h; j++)
                    res[j, i] = matrix[i, j];
            return res;
        }

        public static T[,] RotateMatrixCounterClockwise<T>(this T[,] oldMatrix) where T : struct
        {
            if (oldMatrix == null) return null;
            int M = oldMatrix.GetLength(0), N = oldMatrix.GetLength(1);
            var res = new T[N, M];
            for (int i = 0; i < M; i++)
                for (int j = 0; j < N; j++)
                    res[N - 1 - j, i] = oldMatrix[i, j];
            return res;
        }

        public static T[,] RotateMatrixClockwise<T>(this T[,] oldMatrix) where T : struct
        {
            if (oldMatrix == null) return null;
            int M = oldMatrix.GetLength(0), N = oldMatrix.GetLength(1);
            var res = new T[N, M];
            for (int i = 0; i < M; i++)
                for (int j = 0; j < N; j++)
                    res[j, M - 1 - i] = oldMatrix[i, j];
            return res;
        }

        public static double[,] LoadMatrixFromCSV(this IEnumerable<string> lines)
        {
            try
            {
                var data = lines.SkipWhile(line => line.StartsWith("#"))
                    .Select(line => line.Split(' ', '\t', ',', ';').Select(s => double.Parse(s)).ToList()).ToList();
                var res = new double[data.Count, data[0].Count];
                for (int i = 0; i < data.Count; i++)
                    for (int j = 0; j < data[0].Count; j++)
                        res[i, j] = data[i][j];
                return res;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "TERS-StaticUtils");
                return null;
            }         
        }

        public static List<List<double>> LoadVectorsFromCSV(this IEnumerable<string> lines)
        {
            try
            {
                var data = lines.SkipWhile(line => line.StartsWith("#"))
                    .Select(line => line.Split(' ', '\t', ',', ';').Select(s => double.Parse(s)).ToList()).ToList();
                return data;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "TERS-StaticUtils");
                return null;
            }
        }

        public static bool IsPointInMatrix<T>(this T[,] matrix, int x, int y)
            => x >= 0 && x < matrix.GetLength(0) && y >= 0 && y < matrix.GetLength(1);

        public static IEnumerable<T> SelectVectorFormMatrix<T>(this T[,] matrix, int selectRank, int selectIndex)
        {
            if (selectRank == 0)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                    yield return matrix[selectIndex, j];
            }
            else if (selectRank == 1)
            {
                for (int i = 0; i < matrix.GetLength(0); i++)
                    yield return matrix[i, selectIndex];
            }
            else
                throw new ArgumentException();
        }

        public static List<double> GetMatrixDiag(this double[,] matrix, int startX, int startY, int endX, int endY) 
        {
            List<double> res;
            if (startX == endX && startY == endY)
                return new List<double>();
            else
            {   
                var count = (int)Round(Sqrt(Pow(startX - endX, 2) + Pow(startY - endY, 2)));
                res = new List<double>(count);
                double incX = (endX - startX) / (double)count, incY = (endY - startY) / (double)count;
                res.Add(matrix[startX, startY]);
                res.AddRange(Enumerable.Range(1, count - 1).Select(i => biInterp(matrix, startX + incX * i, startY + incY * i)));
                res.Add(matrix[endX, endY]);
                return res;
            }

            double biInterp(double[,] m, double X, double Y)
            {
                int x1 = (int)Floor(X), x2 = (int)Ceiling(X), y1 = (int)Floor(Y), y2 = (int)Ceiling(Y);
                if (x1 == x2) x2 += 1;
                if (y1 == y2) y2 += 1;
                return (x2 - X) * (y2 - Y) * m[x1, y1] + (X - x1) * (y2 - Y) * m[x2, y1] + (x2 - X) * (Y - y1) * m[x1, y2] + (X - x1) * (Y - y1) * m[x2, y2];
            }
        }
    }
}
