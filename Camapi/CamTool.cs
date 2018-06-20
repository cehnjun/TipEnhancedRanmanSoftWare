using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ATMCD64CS;
using System.Windows;

namespace CamApi
{
    public static partial class CamTool
    {
        private static void checkRetCode(ERRCODES retCode, string msg)
        {
            if (retCode != ERRCODES.DRV_SUCCESS && retCode != ERRCODES.DRV_ACQUIRING)
                throw new CamApiException(msg, retCode);
        }

        private static readonly AndorSDK SDK;

        /// <summary>
        /// Temperature of cam MUST be higher than this while shutting down
        /// </summary>
        private static readonly int SHUTDOWN_TEMP_THR = -20;

        /// <summary>
        /// temperature check loop time
        /// </summary>
        private static readonly int CHECK_LOOP_TIME = 500;

        public class CamApiException : ApplicationException
        {
            public ERRCODES? ErrCode { get; }
            public CamApiException(string message, ERRCODES errCode) : base(message) { ErrCode = errCode; }
            public CamApiException(string message) : base(message) { }
        }

        public static int CamCount
        {
            get
            {
                try
                {
                    int camCount = 0;
                    var retCode = (ERRCODES)SDK.GetAvailableCameras(ref camCount);
                    checkRetCode(retCode, $"GetAvailableCameras return {retCode}.");
                    return camCount;
                }
                catch(CamApiException caex)
                {
                    MessageBox.Show(caex.Message, "CamApi");
                    return 0;
                }
            }
        }
 
        static CamTool()
        {
            try
            {
                SDK = new AndorSDK();
            }
            catch (Exception ex)
            {
                throw new CamApiException(ex.Message + "\n" + "AndorSDK init failed.");
            }
        }

        #region enums
        /// <summary>
        /// Errcode which SDK Function returns
        /// </summary>
        public enum ERRCODES : short
        {
            DRV_ERROR_CODES = 20001, DRV_P1INVALID = 20066,
            DRV_SUCCESS = 20002, DRV_P2INVALID = 20067,
            DRV_VXDNOTINSTALLED = 20003, DRV_P3INVALID = 20068,
            DRV_ERROR_SCAN = 20004, DRV_P4INVALID = 20069,
            DRV_ERROR_CHECK_SUM = 20005, DRV_INIERROR = 20070,
            DRV_ERROR_FILELOAD = 20006, DRV_COFERROR = 20071,
            DRV_UNKNOWN_FUNCTION = 20007, DRV_ACQUIRING = 20072,
            DRV_ERROR_VXD_INIT = 20008, DRV_IDLE = 20073,
            DRV_ERROR_ADDRESS = 20009, DRV_TEMPCYCLE = 20074,
            DRV_ERROR_PAGELOCK = 20010, DRV_NOT_INITIALIZED = 20075,
            DRV_ERROR_PAGE_UNLOCK = 20011, DRV_P5INVALID = 20076,
            DRV_ERROR_BOARDTEST = 20012, DRV_P6INVALID = 20077,
            DRV_ERROR_ACK = 20013, DRV_INVALID_MODE = 20078,
            DRV_ERROR_UP_FIFO = 20014, DRV_INVALID_FILTER = 20079,
            DRV_ERROR_PATTERN = 20015, DRV_I2CERRORS = 20080,
            DRV_ACQUISITION_ERRORS = 20017, DRV_DRV_I2CDEVNOTFOUND = 20081,
            DRV_ACQ_BUFFER = 20018, DRV_I2CTIMEOUT = 20082,
            DRV_ACQ_DOWNFIFO_FULL = 20019, DRV_P7INVALID = 20083,
            DRV_PROC_UNKNOWN_INSTRUCTION = 20020, DRV_USBERROR = 20089,
            DRV_ILLEGAL_OP_CODE = 20021, DRV_IOCERROR = 20090,
            DRV_KINETIC_TIME_NOT_MET = 20022, DRV_VRMVERSIONERROR = 20091,
            DRV_USB_INTERRUPT_ENDPOINT_ERROR = 20093, DRV_GPIBERROR = 20054,
            DRV_ACCUM_TIME_NOT_MET = 20023, DRV_RANDOM_TRACK_ERROR = 20094,
            DRV_NO_NEW_DATA = 20024, DRV_INVALID_TRIGGER_MODE = 20095,
            DRV_SPOOLERROR = 20026, DRV_LOAD_FIRMWARE_ERROR = 20096,
            DRV_SPOOLSETUPERROR = 20027, DRV_DIVIDE_BY_ZERO_ERROR = 20097,
            DRV_TEMPERATURE_CODES = 20033, DRV_INVALID_RINGEXPOSURES = 20098,
            DRV_TEMPERATURE_OFF = 20034, DRV_BINNING_ERROR = 20099,
            DRV_TEMP_NOT_STABILIZED = 20035, DRV_ERROR_NOCAMERA = 20990,
            DRV_TEMPERATURE_STABILIZED = 20036, DRV_NOT_SUPPORTED = 20991,
            DRV_TEMPERATURE_NOT_REACHED = 20037, DRV_NOT_AVAILABLE = 20992,
            DRV_TEMPERATURE_OUT_RANGE = 20038, DRV_ERROR_MAP = 20115,
            DRV_TEMPERATURE_NOT_SUPPORTED = 20039, DRV_ERROR_UNMAP = 20116,
            DRV_TEMPERATURE_DRIFT = 20040, DRV_ERROR_MDL = 20117,
            DRV_GENERAL_ERRORS = 20049, DRV_ERROR_UNMDL = 20118,
            DRV_INVALID_AUX = 20050, DRV_ERROR_BUFFSIZE = 20119,
            DRV_COF_NOTLOADED = 20051, DRV_ERROR_NOHANDLE = 20121,
            DRV_FPGAPROG = 20052, DRV_GATING_NOT_AVAILABLE = 20130,
            DRV_FLEXERROR = 20053, DRV_FPGA_VOLTAGE_ERROR = 20131,
            DRV_DATATYPE = 20064, DRV_INVALID_AMPLIFIER = 20100,
            DRV_DRIVER_ERRORS = 20065, DRV_INVALID_COUNTCONVERT_MODE = 20101,
        }

        /// <summary>
        /// the readout mode to be used on the subsequent acquisitions
        /// </summary>
        public enum READMODE : byte
        {
            FULL_VERTICAL_BINNING,
            MULTI_TRACK,
            RANDOM_TRACK,
            SINGLE_TRACK,
            IMAGE,
        }

        /// <summary>
        /// the acquisition mode to be used on the next StartAcquisition
        /// </summary>
        public enum ACQMODE : byte
        {
            SINGLE_SCAN = 1,
            ACCUMULATE,
            KINETICS,
            FAST_KINETICS,
            RUN_TILL_ABORT,
        }

        /// <summary>
        /// HORIZONAL SHIFT output amplification type
        /// </summary>
        public enum HORIZONAL_SHIFT_TYPE : byte
        {
            CONVENTIONAL,
            EM,
        }
        #endregion
    }
}
