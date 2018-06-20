using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathWorks.MATLAB.NET.Arrays;
using System.IO;
using System.Windows;
using System.Windows.Forms;

namespace MatlabApi
{
    public class MatlabTool : IDisposable
    {
        #region Singleton 
        private static Lazy<MatlabTool> _instance =
            new Lazy<MatlabTool>(() => new MatlabTool());
        public async static Task<MatlabTool> GetInstanceAsync() 
            => await Task.Run(() => _instance.Value);
        // public static MatlabTool Instance { get => _instance.Value; }
        #endregion

        public void Dispose()
        {
            _lib?.Dispose();;
            _instance = new Lazy<MatlabTool>(() => new MatlabTool());
        }

        private static IEnumerable<T> selectVectorFormMatrix<T>(T[,] matrix, int selectRank, int selectIndex)
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
        private MatlabLib.Lib _lib;
        private MatlabTool()
        {
            try
            {
                _lib = new MatlabLib.Lib();
            }
            catch (TypeInitializationException tiex)
            {
                MessageBox.Show("MCRuntime init error. Please install MCR2016b and use a 64-bit OS.\n" + tiex.Message, "TERS-MatlabTool");
            }
        }

        #region MatlabLib
        public double[,] BCS_O(double[,] matrix)
        {
            try
            {
                var arr = new MWNumericArray(matrix);
                var m = new MWNumericArray(2);
                var n = new MWNumericArray(2);
                var result = _lib.BCS_O(arr, m, n).ToArray();
                return (double[,])result;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "TERS-MatlabApi");
                return null;
            }
        }
        public double[,] Adjust_BCS(double[,] matrix, bool isShowFig)
        {
            try
            {
                var arr = new MWNumericArray(matrix);
                var flag = new MWLogicalArray(isShowFig);
                var result = _lib.adjust_BCS(arr, flag).ToArray();
                return (double[,])result;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "TERS-MatlabApi");
                return null;
            }
        }
        public void Mesh(double[,] matrix)
        {
            var arr = new MWNumericArray(matrix);
            _lib.Mesh(arr);
        }
        public void Surf(double[,] matrix)
        {
            var arr = new MWNumericArray(matrix);
            _lib.Surf(arr);
        }
        public double[,] Resize(double[,] matrix, double scale, ResizeMethod method)
        {
            try
            {
                var arr = new MWNumericArray(matrix);
                var s = new MWNumericArray(scale);
                var m = new MWCharArray(method.ToString());
                return (double[,])_lib.Resize(arr, s, m).ToArray();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "TERS-MatlabApi");
                return null;
            }
        }
        public enum ResizeMethod : byte
        {
            bilinear, bicubic, lanczos2, lanczos3
        }
        public IEnumerable<double> ManualInterp(IEnumerable<double> x, IEnumerable<double> y, IEnumerable<double> interpX, InterpMethods method)
        {
            var x0 = new MWNumericArray(x.ToArray());
            var y0 = new MWNumericArray(y.ToArray());
            var xi = new MWNumericArray(interpX.ToArray());
            var m = new MWCharArray(method.ToString());
            try
            {
                var ret = (double[,])_lib.Interp1(x0, y0, xi, m).ToArray();
                return selectVectorFormMatrix(ret, 0, 0).Zip(y, (baseline, origin) => origin - baseline);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "TERS-MatlabLib");
                return null;
            }
        }
        public enum InterpMethods : byte
        {
            linear, spline, cubic
        }
        public double[,] HistEQ(double[,] matrix, int N)
        {
            try
            {
                var arr = new MWNumericArray(matrix);
                var n = new MWNumericArray(N);
                return (double[,])_lib.HistEQ(arr, n).ToArray();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "TERS-MatlabApi");
                return null;
            }
        }
        public double[,] DeconvB(double[,] matrix, int bt, int lt)
        {
            try
            {
                var arr = new MWNumericArray(matrix);
                var b = new MWNumericArray(bt);
                var l = new MWNumericArray(lt);
                return (double[,])_lib.tip_deconvB(arr, b, l).ToArray();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "TERS-MatlabApi");
                return null;
            }
        }
        public double[,] DeconvR(double[,] matrix, int ds, int st, int tt, int np, bool bcsflag)
        {
            try
            {
                var arr = new MWNumericArray(matrix);
                var d = new MWNumericArray(ds);
                var s = new MWNumericArray(st);
                var t = new MWNumericArray(tt);
                var n = new MWNumericArray(np);
                var b = new MWLogicalArray(bcsflag);
                return (double[,])_lib.tip_deconvR(arr, d, s, t, n, b).ToArray();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "TERS-MatlabApi");
                return null;
            }
        }
        public double[,] Criminisi(double[,] matrix, bool isShowFigure)
        {
            var arr = new MWNumericArray(matrix);
            var i = new MWLogicalArray(isShowFigure);
            try
            {
                return (double[,])_lib.Criminisi(4, arr, i)[0].ToArray();
            }
            catch (Exception ex)
            {
#if DEBUG
                MessageBox.Show(ex.Message, "TERS-MatlabApi");
#endif
                return null;
            }   
        }
        public double[,] TVRepair(double[,] matrix, int p)
        {
            var arr = new MWNumericArray(matrix);
            try
            {
                return (double[,])_lib.TVrepair(arr, p).ToArray();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "TERS-MatlabApi");
                return null;
            }
        }
        public IEnumerable<double> MedFilter(IEnumerable<double> originData, int n)
        {
            var data = new MWNumericArray(originData.ToArray());
            try
            {
                var ret = (double[,])_lib.MedFilt(data, n).ToArray();
                return selectVectorFormMatrix(ret, 0, 0);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "TERS-MatlabLib");
                return null;
            }
        }
        public IEnumerable<double> SGFilter(IEnumerable<double> originData, int order, int frameLength)
        {
            var data = new MWNumericArray(originData.ToArray());
            try
            {
                var ret = (double[,])_lib.SGFilter(data, order, frameLength).ToArray();
                return selectVectorFormMatrix(ret, 0, 0);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "TERS-MatlabLib");
                return null;
            }
        }
        public IEnumerable<double> PFC(IEnumerable<double> x0, IEnumerable<double> y0, int n, double thr)
        {
            var x = new MWNumericArray(x0.ToArray());
            var y = new MWNumericArray(y0.ToArray());
            try
            {
                var ret = (double[,])_lib.PFC(x, y, n, thr).ToArray();
                return selectVectorFormMatrix(ret, 0, 0).Zip(y0, (baseline, origin) => origin - baseline);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "TERS-MatlabLib");
                return null;
            }
        }       
        public IEnumerable<double> BSC(IEnumerable<double> x0, IEnumerable<double> y0, int m, double thr)
        {
            var x = new MWNumericArray(x0.ToArray());
            var y = new MWNumericArray(y0.ToArray());
            try
            {
                var ret = (double[,])_lib.BSC(x, y, m, thr).ToArray();
                return selectVectorFormMatrix(ret, 1, 0).Zip(y0, (baseline, origin) => origin - baseline);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "TERS-MatlabLib");
                return null;
            }
        }
        public IEnumerable<double> EMD(IEnumerable<double> y0, double a, int r)
        {
            var y = new MWNumericArray(y0.ToArray());
            try
            {
                var ret = (double[,])_lib.EMD(y, a, r).ToArray();
                return selectVectorFormMatrix(ret, 0, 0);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "TERS-MatlabLib");
                return null;
            }
        }
        #endregion

        #region Filter
        public double[,] Spfilter(double[,] matrix, SpfilterType type, int m, int n, int para)
        {
            try
            {
                var arr = new MWNumericArray(matrix);
                var t = new MWCharArray(type.ToString());
                var mm = new MWNumericArray(m);
                var nn = new MWNumericArray(n);
                var p = new MWNumericArray(para);
                if (type == SpfilterType.chmean || type == SpfilterType.artimmed)
                    return (double[,])_lib.spfilt(arr, t, mm, nn, para).ToArray();
                else
                    return (double[,])_lib.spfilt(arr, t, mm, nn).ToArray();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "TERS-MatlabLib");
                return null;
            }
        }
        public double[,] Fqfilter(double[,] matrix, FqfilterFlag flag, FqfilterType type, int D0, int n)
        {
            try
            {
                if (D0 <= 0)
                    throw new ArgumentException("Parameter D0 must be positive.");
                var arr = new MWNumericArray(matrix);
                MWCharArray f = new MWCharArray(flag.ToString()), t = new MWCharArray(type.ToString());
                var d = new MWNumericArray(D0);
                var nn = new MWNumericArray(n);
                return (double[,])_lib.fqfilter(2, arr, f, t, d, nn)[0].ToArray();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "TERS-MatlabLib");
                return null;
            }
        }
        public double[,] Bwt_Notchfilt(double[,] matrix, int c, int d, bool showFig)
        {
            try
            {
                var arr = new MWNumericArray(matrix);
                var cc = new MWNumericArray(c);
                var dd = new MWNumericArray(d);
                var s = new MWLogicalArray(showFig);
                return (double[,])_lib.bwt_Notchfilt(3, arr, cc, dd, s)[2].ToArray();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "TERS-MatlabLib");
                return null;
            }
        }

        public enum SpfilterType : byte
        {
            amean, gmean, hmean, chmean, medain, max, min, midpoint, artimmed,
        }
        public enum FqfilterFlag : byte
        {
            lowpass, highpass,
        }
        public enum FqfilterType : byte
        {
            ideal, btw, gaussian,
        }
        #endregion
    }
}