using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace CamApi
{
    public static partial class CamTool
    {
        public class CamConnection : IDisposable
        {
            #region Cam hardware info 
            public int XPixelCount { get; private set; }
            public int YPixelCount { get; private set; }
            public int MinTemp { get; private set; }
            public int MaxTemp { get; private set;}
            #endregion

            private int _shutterOpenTime;
            private int _shutterCloseTime;
            private int _targetTemp;
            private float _expoTime;
            // shutter open/close time in millisecond
            public int ShutterOpenTime
            {
                get { return _shutterOpenTime; }
                set
                {
                    if (value < 0)
                        throw new CamApiException($"Invaild ShutterOpenTime: {value}.");
                    _shutterOpenTime = value;
                }
            }
            public int ShutterCloseTime
            {
                get { return _shutterCloseTime; }
                set
                {
                    if (value < 0)
                        throw new CamApiException($"Invaild ShutterCloseTime: {value}.");
                    _shutterCloseTime = value;
                }
            }
            public int TargetTemp
            {
                get => _targetTemp;
                set
                {
                    try
                    {
                        _targetTemp = value;
                        var retCode = (ERRCODES)SDK.SetTemperature(TargetTemp);
                        checkRetCode(retCode, "SetTemperature return " +
                            (retCode == ERRCODES.DRV_P1INVALID ? "Temperature invalid." : retCode.ToString()));
                    }
                    catch (CamApiException caex)
                    {
                        MessageBox.Show(caex.Message, "CamApi");
                    }                   
                }
            }
            /// <summary>
            ///  Expo time in seconds
            /// </summary>
            public float ExpoTime
            {
                get => _expoTime;
                set
                {
                    try
                    {
                        _expoTime = value;
                        var retCode = (ERRCODES)SDK.SetExposureTime(ExpoTime);
                        checkRetCode(retCode, $"SetExposureTime return " +
                           (retCode == ERRCODES.DRV_P1INVALID ? "Exposure Time invalid." : retCode.ToString()));
                    }
                    catch (CamApiException caex)
                    {
                        MessageBox.Show(caex.Message, "CamApi");
                    }
                }
            }

            private CamConnection(int targetTemp, float expoTime, int shutterOpenTime = 50, int shutterCloseTime = 50)
            {
                if (CamCount == 0)
                    throw new CamApiException("No cam detected.");
                if (CamCount > 1)
                    throw new CamApiException("Multiple cams detected.");
                var retCode = (ERRCODES)SDK.Initialize("");
                checkRetCode(retCode, $"Initialize return {retCode}");

                int xpc = 0, ypc = 0;
                retCode = (ERRCODES)SDK.GetDetector(ref xpc, ref ypc);
                checkRetCode(retCode, $"GetDetector return {retCode}");
                XPixelCount = xpc; YPixelCount = ypc;

                int mint = 0, maxt = 0;
                retCode = (ERRCODES)SDK.GetTemperatureRange(ref mint, ref maxt);
                checkRetCode(retCode, $"GetTemperatureRange return {retCode}");
                MinTemp = mint; MaxTemp = maxt;

                TargetTemp = targetTemp;
                ExpoTime = expoTime;
                ShutterOpenTime = shutterOpenTime;
                ShutterCloseTime = shutterCloseTime;

                // use single scan mode
                retCode = (ERRCODES)SDK.SetAcquisitionMode((int)ACQMODE.SINGLE_SCAN);
                checkRetCode(retCode, $"SetAcquisitionMode return {retCode}");
                // use full vertical binning mode
                retCode = (ERRCODES)SDK.SetReadMode((int)READMODE.FULL_VERTICAL_BINNING);
                checkRetCode(retCode, $"SetReadMode return {retCode}");
                // Output TTL low signal to open shutter, Shutter mode is fully auto
                retCode = (ERRCODES)SDK.SetShutter(0, 0, ShutterCloseTime, ShutterOpenTime);
                checkRetCode(retCode, $"SetShutter return {retCode}");
                // use internal trigger mode
                retCode = (ERRCODES)SDK.SetTriggerMode(0);
                checkRetCode(retCode, $"SetTriggerMode return {retCode}");
                // SetVSSpeed
                retCode = (ERRCODES)SDK.SetVSSpeed(0);
                checkRetCode(retCode, $"SetVSSpeed return {retCode}");
                // SetHSSpeed
                retCode = (ERRCODES)SDK.SetHSSpeed((int)HORIZONAL_SHIFT_TYPE.CONVENTIONAL, 0);
                checkRetCode(retCode, $"SetHSSpeed return {retCode}");
            }

            public static CamConnection CreateConnection(int targetTemp, float expoTime, int shutterOpenTime = 50, int shutterCloseTime = 50)
            {
                try
                {
                    var connection = new CamConnection(targetTemp, expoTime, shutterOpenTime, shutterCloseTime);
                    return connection;
                }
                catch (CamApiException caex)
                {
                    MessageBox.Show(caex.Message, "CamApi");
                    return null;
                }
            }

            public int[] AcqData()
            {
                try
                {
                    var retCode = (ERRCODES)SDK.StartAcquisition();
                    checkRetCode(retCode, $"StartAcquisition return {retCode}");
                    GetStatus(out ERRCODES status);
                    while (status != ERRCODES.DRV_IDLE)
                    {
                        if (status != ERRCODES.DRV_ACQUIRING)
                            throw new CamApiException($"GetStatus return {status}");
                        Thread.Sleep(CHECK_LOOP_TIME);
                        GetStatus(out status);
                    }
                    var size = (uint)XPixelCount;
                    var data = new int[size];
                    retCode = (ERRCODES)SDK.GetAcquiredData(data, size);
                    checkRetCode(retCode, $"GetAcquiredData return {retCode}");
                    return data;
                }
                catch (CamApiException caex)
                {
                    MessageBox.Show(caex.Message, "CamApi");
                    return null;
                }                
            }

            public Task<int[]> AcqDataAsync() => Task.Run(() => AcqData());

            //public Task CoolingDownAsync(bool fastMode) => Task.Run(() =>
            //{
            //    GetCamTemp(out ERRCODES tempStatus);
            //    while (tempStatus != ERRCODES.DRV_TEMPERATURE_STABILIZED || tempStatus != ERRCODES.DRV_TEMP_NOT_STABILIZED)
            //    {
            //        if (tempStatus != ERRCODES.DRV_TEMPERATURE_NOT_REACHED
            //            && tempStatus != ERRCODES.DRV_TEMPERATURE_DRIFT
            //            && tempStatus != ERRCODES.DRV_TEMP_NOT_STABILIZED)
            //            throw new CamApiException($"GetCamTemp return {tempStatus}");
            //        var t = GetCamTemp(out tempStatus);
            //        if (fastMode && t > TargetTemp - 3 && t < TargetTemp + 3)
            //            break;
            //        Thread.Sleep(CHECK_LOOP_TIME);
            //    }
            //});

            public void ShutDown(bool fastMode)
            {
                try
                {
                    if (IsCoolerOn)
                    {
                        var retCode1 = (ERRCODES)SDK.CoolerOFF();
                        checkRetCode(retCode1, $"CoolerOFF return {retCode1}.");
                        if (!fastMode)
                        {
                            while (GetCamTemp(out ERRCODES s) <= SHUTDOWN_TEMP_THR)
                                Thread.Sleep(CHECK_LOOP_TIME);
                        }
                    }
                    var retCode = (ERRCODES)SDK.ShutDown();
                    checkRetCode(retCode, $"ShutDown return {retCode}.");
                }
                catch (CamApiException caex)
                {
                    MessageBox.Show(caex.Message, "CamApi");
                }
                finally
                {
                    Thread.Sleep(3000);
                    SDK.CoolerOFF();
                    SDK.ShutDown();
                }
            }

            public async Task ShutDownAsync(bool fastMode) => await Task.Run(() => ShutDown(fastMode));

            /// <summary>
            /// get the status of current cam and check init status
            /// </summary>
            /// <param name="status"></param>
            /// <returns>true=>cam already inited, false => cam not yet inited</returns>
            public bool GetStatus(out ERRCODES status)
            {
                var s = 0;
                var retCode = (ERRCODES)SDK.GetStatus(ref s);
                status = (ERRCODES)s;
                return retCode == ERRCODES.DRV_SUCCESS ? true : false;
            }

            /// <summary>
            /// return the temp of current cam in degrees
            /// </summary>
            /// <param name="tempStatus"></param>
            /// <returns></returns>
            public float GetCamTemp(out ERRCODES tempStatus)
            {
                float temp = 0xFFFF;
                tempStatus = (ERRCODES)SDK.GetTemperatureF(ref temp);
                return temp;
            }

            public void CoolerOn()
            {
                try
                {
                    var retCode = (ERRCODES)SDK.CoolerON();
                    checkRetCode(retCode, $"CoolerON return {retCode}");
                }
                catch (CamApiException caex)
                {
                    MessageBox.Show(caex.Message, "CamApi");
                }                
            }

            public bool IsCoolerOn
            {
                get
                {
                    int coolerstatus = 0xFFFF;
                    var retCode = (ERRCODES)SDK.IsCoolerOn(ref coolerstatus);
                    //checkRetCode(retCode, $"IsCoolerOn return {retCode}");
                    return coolerstatus == 0 ? false : true;
                }
            }

            public void CoolerOff()
            {
                try
                {
                    var retCode = (ERRCODES)SDK.CoolerOFF();
                    checkRetCode(retCode, "sdk.CoolerOFF()");
                }
                catch(CamApiException caex)
                {
                    MessageBox.Show(caex.Message, "CamApi");
                }
            }

            public override string ToString()
            {
                return $"Pixel:{XPixelCount}*{YPixelCount},TempRange:{MinTemp}~{MaxTemp}," +
                    $"Temp:{GetCamTemp(out ERRCODES x):F2},TargetTemp:{TargetTemp}," +
                    $"Cooler:{(IsCoolerOn ? "On" : "Off")},ExpoTime:{ExpoTime:F2}";
            }

            public void Dispose()
            {
                ShutDown(true);
            }
        }
    }
}
