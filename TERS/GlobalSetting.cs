using MicroMvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TERS.Properties;
using PropertyTools.DataAnnotations;

namespace TERS
{
    public class GlobalSetting : System.ComponentModel.IDataErrorInfo
    {
        #region Map Settings
        [Category("Map|Display")]
        //[Description("This is not yet working.")]
        public bool Interpolate
        {
            get { return Settings.Default.Interpolate; }
            set { Settings.Default.Interpolate = value; }
        }
        [Category("Map|Display")]
        public int PaletteSize
        {
            get { return Settings.Default.Palette_Size; }
            set { Settings.Default.Palette_Size = value; }
        }
        [Category("Map|Display")]
        [SelectorStyle(SelectorStyle.ComboBox)]
        public StaticUtils.PaletteName PaletteName
        {
            get => (StaticUtils.PaletteName)Settings.Default.Palette_Name;
            set => Settings.Default.Palette_Name = (int)value;
        }
        [Category("Map|Deconv_R")]
        public int DS
        {
            get { return Settings.Default.DS; }
            set { Settings.Default.DS = value; }
        }
        [Category("Map|Deconv_R")]
        public int ST
        {
            get => Settings.Default.ST;
            set => Settings.Default.ST = value;
        }
        [Category("Map|Deconv_R")]
        public int TT
        {
            get => Settings.Default.TT;
            set => Settings.Default.TT = value;
        }
        [Category("Map|Deconv_R")]
        public int NP
        {
            get => Settings.Default.NP;
            set => Settings.Default.NP = value;
        }
        [Category("Map|Deconv_R")]
        public bool BCSFlag
        {
            get => Settings.Default.BCSFlag;
            set => Settings.Default.BCSFlag = value;
        }
        [Category("Map|Deconv_B")]
        public int BT
        {
            get => Settings.Default.BT;
            set => Settings.Default.BT = value;
        }
        [Category("Map|Deconv_B")]
        public int LT
        {
            get => Settings.Default.LT;
            set => Settings.Default.LT = value;
        }
        [Category("Map|Resize")]
        public double Scale
        {
            get => Settings.Default.Scale;
            set => Settings.Default.Scale = value;
        }
        [Category("Map|Resize")]
        [SelectorStyle(SelectorStyle.ComboBox)]
        public MatlabApi.MatlabTool.ResizeMethod Method
        {
            get => (MatlabApi.MatlabTool.ResizeMethod)Settings.Default.Method;
            set => Settings.Default.Method = (int)value;
        }
        [Category("Map|HistEQ")]
        public int N
        {
            get => Settings.Default.HistEQ_N;
            set => Settings.Default.HistEQ_N = value;
        }
        [Category("Map|Criminisi")]
        public bool ShowFig
        {
            get => Settings.Default.Cr_Showfig;
            set => Settings.Default.Cr_Showfig = value;
        }
        [Category("Map|TV_Repair")]
        public int TV_P
        {
            get => Settings.Default.TV_p;
            set => Settings.Default.TV_p = value;
        }
        #endregion

        #region Filter Setting
        [Category("Filter|Bwt_Notchfilt")]
        public bool BWT_Showfig
        {
            get => Settings.Default.BWT_Showfig;
            set => Settings.Default.BWT_Showfig = value;
        }
        [Category("Filter|Bwt_Notchfilt")]
        public int BWT_C
        {
            get => Settings.Default.BWT_C;
            set => Settings.Default.BWT_C = value;
        }
        [Category("Filter|Bwt_Notchfilt")]
        public int BWT_D
        {
            get => Settings.Default.BWT_D;
            set => Settings.Default.BWT_D = value;
        }
        [Category("Filter|Fqfilter")]
        [SelectorStyle(SelectorStyle.ComboBox)]
        public MatlabApi.MatlabTool.FqfilterFlag Fqflag
        {
            get => (MatlabApi.MatlabTool.FqfilterFlag)Settings.Default.Filter_Fqflag;
            set => Settings.Default.Filter_Fqflag = (int)value;
        }
        [Category("Filter|Fqfilter")]
        [SelectorStyle(SelectorStyle.ComboBox)]
        public MatlabApi.MatlabTool.FqfilterType Fqtype
        {
            get => (MatlabApi.MatlabTool.FqfilterType)Settings.Default.Filter_Fqtype;
            set => Settings.Default.Filter_Fqtype = (int)value;
        }
        [Description("Cutoff frequency")]
        [Category("Filter|Fqfilter")]       
        public int Fqd0
        {
            get => Settings.Default.Filter_Fqd0;
            set => Settings.Default.Filter_Fqd0 = value;
        }
        [Category("Filter|Fqfilter")]
        public int Fqn
        {
            get => Settings.Default.Filter_Fqn;
            set => Settings.Default.Filter_Fqn = value;
        }
        [Category("Filter|Spfilter")]
        [SelectorStyle(SelectorStyle.ComboBox)]
        public MatlabApi.MatlabTool.SpfilterType Sptype
        {
            get => (MatlabApi.MatlabTool.SpfilterType)Settings.Default.Filter_Sptype;
            set => Settings.Default.Filter_Sptype = (int)value;
        }
        [Category("Filter|Spfilter")]
        public int Spm
        {
            get => Settings.Default.Filter_Spm;
            set => Settings.Default.Filter_Spm = value;
        }
        [Category("Filter|Spfilter")]
        public int Spn
        {
            get => Settings.Default.Filter_Spn;
            set => Settings.Default.Filter_Spn = value;
        }
        [Category("Filter|Spfilter")]
        public int Sppara
        {
            get => Settings.Default.Filter_Sppara;
            set => Settings.Default.Filter_Sppara = value;
        }
        #endregion

        #region WaveAnalyse Settings
        [Category("WaveAnalyse|Spectro")]
        [System.ComponentModel.DataAnnotations.Range(-100, -20)]
        public int TargetTemp
        {
            get => Settings.Default.TargetTemp;
            set => Settings.Default.TargetTemp = value;
        }
        [Category("WaveAnalyse|Spectro")]
        [System.ComponentModel.DataAnnotations.Range(0, 50)]
        public float ExpoTime
        {
            get => Settings.Default.ExpoTime;
            set => Settings.Default.ExpoTime = value;
        }
        [Category("WaveAnalyse|Spectro")]
        public bool CoolingFastmode
        {
            get => Settings.Default.Cooling_FastMode;
            set => Settings.Default.Cooling_FastMode = value;
        }
        [Category("WaveAnalyse|Spectro")]
        public bool ShutdownFastmode
        {
            get => Settings.Default.ShutDown_FastMode;
            set => Settings.Default.ShutDown_FastMode = value;
        }
        [Category("WaveAnalyse|Spectro")]
        public int ShutterOpentime
        {
            get => Settings.Default.ShutterOpenTime;
            set => Settings.Default.ShutterOpenTime = value;
        }
        [Category("WaveAnalyse|Spectro")]
        public int ShutterClosetime
        {
            get => Settings.Default.ShutterCloseTime;
            set => Settings.Default.ShutterCloseTime = value;
        }
        [Category("WaveAnalyse|PolyBaseline")]
        public int Poly_N
        {
            get => Settings.Default.PolyBaseline_n;
            set => Settings.Default.PolyBaseline_n = value;
        }
        [Category("WaveAnalyse|PolyBaseline")]
        public double Poly_THR
        {
            get => Settings.Default.Polybaseline_thr;
            set => Settings.Default.Polybaseline_thr = value;
        }
        [Category("WaveAnalyse|SGFilter")]
        public int SGFilter_N
        {
            get => Settings.Default.SGFilter_n;
            set => Settings.Default.SGFilter_n = value;
        }
        [Category("WaveAnalyse|SGFilter")]
        public int SGFilter_LENGTH
        {
            get => Settings.Default.SGFilter_length;
            set => Settings.Default.SGFilter_length = value;
        }
        [Category("WaveAnalyse|MedianFilter")]
        public int MedFilter_N
        {
            get => Settings.Default.MedFilter_n;
            set => Settings.Default.MedFilter_n = value;
        }
        [Category("WaveAnalyse|BSplineBaseline")]
        [System.ComponentModel.DataAnnotations.Range(1, 20)]
        public int BSC_M
        {
            get => Settings.Default.BSC_m;
            set => Settings.Default.BSC_m = value;
        }
        [Category("WaveAnalyse|BSplineBaseline")]
        [System.ComponentModel.DataAnnotations.Range(0, 1)]
        public double BSC_THR
        {
            get => Settings.Default.BSC_thr;
            set => Settings.Default.BSC_thr = value;
        }
        [Category("WaveAnalyse|EMD")]
        [System.ComponentModel.DataAnnotations.Range(0, 1)]
        public double EMD_A
        {
            get => Settings.Default.EMD_a;
            set => Settings.Default.EMD_a = value;
        }
        [Category("WaveAnalyse|EMD")]
        [System.ComponentModel.DataAnnotations.Range(1, 3)]
        public int EMD_R
        {
            get => Settings.Default.EMD_r;
            set => Settings.Default.EMD_r = value;
        }
        #endregion

        public void Save() => Settings.Default.Save();
        public void Reset() => Settings.Default.Reset();
        string System.ComponentModel.IDataErrorInfo.Error => string.Empty;
        string System.ComponentModel.IDataErrorInfo.this[string columnName]
        {
            get
            {
                var pi = this.GetType().GetProperty(columnName);
                var value = pi.GetValue(this, null);

                var context = new System.ComponentModel.DataAnnotations.ValidationContext(this, null, null) { MemberName = columnName };
                var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
                if (!System.ComponentModel.DataAnnotations.Validator.TryValidateProperty(value, context, validationResults))
                {
                    var sb = new StringBuilder();
                    foreach (var vr in validationResults)
                        sb.AppendLine(vr.ErrorMessage);
                    return sb.ToString().Trim();
                }
                return null;
            }
        }
    }
}
