using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Basler.Pylon;

namespace TERS.CameraControl
{
    /// <summary>
    /// CameraControl.xaml 的交互逻辑
    /// </summary>
    public partial class CameraControl : Window
    {
        CameraControlViewModel _cavm;
        private PixelDataConverter converter = new PixelDataConverter();
        private Camera camera = null;
        private Stopwatch stopWatch = new Stopwatch();

        public CameraControl()
        {

            InitializeComponent();
            _cavm = new CameraControlViewModel();
            DataContext = _cavm;

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateDeviceList();

        }

        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        public ImageSource ImageSourceForBitmap(Bitmap bmp)
        {
            var handle = bmp.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally { DeleteObject(handle); }
        }

        private void deviceListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Destroy the old camera object.
            if (camera != null)
            {
                DestroyCamera();
            }


            // Open the connection to the selected camera device.
            if (deviceListView.SelectedItems.Count > 0)
            {
                // Get the first selected item.
                ListViewItem item = deviceListView.SelectedItems[0] as ListViewItem;
                // Get the attached device data.
                ICameraInfo selectedCamera = item.Tag as ICameraInfo;
                try
                {
                    // Create a new camera object.
                    camera = new Camera(selectedCamera);

                    camera.CameraOpened += Configuration.AcquireContinuous;

                    // Register for the events of the image provider needed for proper operation.
                    camera.ConnectionLost += OnConnectionLost;
                    camera.CameraOpened += OnCameraOpened;
                    camera.CameraClosed += OnCameraClosed;
                    camera.StreamGrabber.GrabStarted += OnGrabStarted;
                    camera.StreamGrabber.ImageGrabbed += OnImageGrabbed;
                    camera.StreamGrabber.GrabStopped += OnGrabStopped;

                    // Open the connection to the camera device.
                    camera.Open();

                    // Set the parameter for the controls.
                    //testImageControl.Parameter = camera.Parameters[PLCamera.TestImageSelector];
                    //pixelFormatControl.Parameter = camera.Parameters[PLCamera.PixelFormat];
                    //widthSliderControl.Parameter = camera.Parameters[PLCamera.Width];
                    //heightSliderControl.Parameter = camera.Parameters[PLCamera.Height];
                    //if (camera.Parameters.Contains(PLCamera.GainAbs))
                    //{
                    //    gainSliderControl.Parameter = camera.Parameters[PLCamera.GainAbs];
                    //}
                    //else
                    //{
                    //    gainSliderControl.Parameter = camera.Parameters[PLCamera.Gain];
                    //}
                    //if (camera.Parameters.Contains(PLCamera.ExposureTimeAbs))
                    //{
                    //    exposureTimeSliderControl.Parameter = camera.Parameters[PLCamera.ExposureTimeAbs];
                    //}
                    //else
                    //{
                    //    exposureTimeSliderControl.Parameter = camera.Parameters[PLCamera.ExposureTime];
                    //}
                }
                catch (Exception exception)
                {
                    ShowException(exception);
                }
            }
        }

        // Occurs when a device with an opened connection is removed.
        private void OnConnectionLost(Object sender, EventArgs e)
        {
            if (!Dispatcher.CheckAccess())
            {
                // If called from a different thread, we must use the Invoke method to marshal the call to the proper thread.
                Dispatcher.BeginInvoke(new EventHandler<EventArgs>(OnConnectionLost), sender, e);
                return;
            }

            // Close the camera object.
            DestroyCamera();
            // Because one device is gone, the list needs to be updated.
            UpdateDeviceList();
        }


        // Occurs when the connection to a camera device is opened.
        private void OnCameraOpened(Object sender, EventArgs e)
        {
            if (!Dispatcher.CheckAccess())
            {
                // If called from a different thread, we must use the Invoke method to marshal the call to the proper thread.
                Dispatcher.BeginInvoke(new EventHandler<EventArgs>(OnCameraOpened), sender, e);
                return;
            }

            // The image provider is ready to grab. Enable the grab buttons.
            EnableButtons(true, false);
        }


        // Occurs when the connection to a camera device is closed.
        private void OnCameraClosed(Object sender, EventArgs e)
        {
            if (!Dispatcher.CheckAccess())
            {
                // If called from a different thread, we must use the Invoke method to marshal the call to the proper thread.
                Dispatcher.BeginInvoke(new EventHandler<EventArgs>(OnCameraClosed), sender, e);
                return;
            }

            // The camera connection is closed. Disable all buttons.
            EnableButtons(false, false);
        }


        // Occurs when a camera starts grabbing.
        private void OnGrabStarted(Object sender, EventArgs e)
        {
            if (!Dispatcher.CheckAccess())
            {
                // If called from a different thread, we must use the Invoke method to marshal the call to the proper thread.
                Dispatcher.BeginInvoke(new EventHandler<EventArgs>(OnGrabStarted), sender, e);
                return;
            }

            // Reset the stopwatch used to reduce the amount of displayed images. The camera may acquire images faster than the images can be displayed.

            stopWatch.Reset();

            // Do not update the device list while grabbing to reduce jitter. Jitter may occur because the GUI thread is blocked for a short time when enumerating.
            //updateDeviceListTimer.Stop();

            // The camera is grabbing. Disable the grab buttons. Enable the stop button.
            EnableButtons(false, true);
        }


        // Occurs when an image has been acquired and is ready to be processed.
        private void OnImageGrabbed(Object sender, ImageGrabbedEventArgs e)
        {
            if (!Dispatcher.CheckAccess())
            {
                // If called from a different thread, we must use the Invoke method to marshal the call to the proper GUI thread.
                // The grab result will be disposed after the event call. Clone the event arguments for marshaling to the GUI thread.
                Dispatcher.BeginInvoke(new EventHandler<ImageGrabbedEventArgs>(OnImageGrabbed), sender, e.Clone());
                return;
            }

            try
            {
                // Acquire the image from the camera. Only show the latest image. The camera may acquire images faster than the images can be displayed.

                // Get the grab result.
                IGrabResult grabResult = e.GrabResult;

                // Check if the image can be displayed.
                if (grabResult.IsValid)
                {
                    // Reduce the number of displayed images to a reasonable amount if the camera is acquiring images very fast.
                    if (!stopWatch.IsRunning || stopWatch.ElapsedMilliseconds > 33)
                    {
                        stopWatch.Restart();

                        Bitmap bitmap = new Bitmap(grabResult.Width, grabResult.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                        // Lock the bits of the bitmap.
                        BitmapData bmpData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
                        // Place the pointer to the buffer of the bitmap.
                        converter.OutputPixelFormat = PixelType.BGRA8packed;
                        IntPtr ptrBmp = bmpData.Scan0;
                        converter.Convert(ptrBmp, bmpData.Stride * bitmap.Height, grabResult); //Exception handling TODO
                        bitmap.UnlockBits(bmpData);

                        // Assign a temporary variable to dispose the bitmap after assigning the new bitmap to the display control.
                        //Bitmap bitmapOld = MicroVideo.Image as Bitmap;
                        // Provide the display control with the new bitmap. This action automatically updates the display.
                        //MicroVideo.Image = bitmap;
                        MicroVideo.Source = ImageSourceForBitmap(bitmap);
                        //if (bitmapOld != null)
                        //{
                        //    // Dispose the bitmap.
                        //    bitmapOld.Dispose();
                        //}
                    }
                }
            }
            catch (Exception exception)
            {
                ShowException(exception);
            }
            finally
            {
                // Dispose the grab result if needed for returning it to the grab loop.
                e.DisposeGrabResultIfClone();
            }
        }


        // Occurs when a camera has stopped grabbing.
        private void OnGrabStopped(Object sender, GrabStopEventArgs e)
        {
            if (!Dispatcher.CheckAccess())
            {
                // If called from a different thread, we must use the Invoke method to marshal the call to the proper thread.
                Dispatcher.BeginInvoke(new EventHandler<GrabStopEventArgs>(OnGrabStopped), sender, e);
                return;
            }

            // Reset the stopwatch.
            stopWatch.Reset();

            // Re-enable the updating of the device list.
            //updateDeviceListTimer.Start();

            // The camera stopped grabbing. Enable the grab buttons. Disable the stop button.
            EnableButtons(true, false);

            // If the grabbed stop due to an error, display the error message.
            if (e.Reason != GrabStopReason.UserRequest)
            {
                MessageBox.Show("A grab error occured:\n" + e.ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateDeviceList()
        {
            try
            {
                // Ask the camera finder for a list of camera devices.
                List<ICameraInfo> allCameras = CameraFinder.Enumerate();

                ItemCollection items = deviceListView.Items;

                // Loop over all cameras found.
                foreach (ICameraInfo cameraInfo in allCameras)
                {
                    // Loop over all cameras in the list of cameras.
                    bool newitem = true;
                    foreach (ListViewItem item in items)
                    {
                        ICameraInfo tag = item.Tag as ICameraInfo;

                        // Is the camera found already in the list of cameras?
                        if (tag[CameraInfoKey.FullName] == cameraInfo[CameraInfoKey.FullName])
                        {
                            tag = cameraInfo;
                            newitem = false;
                            break;
                        }
                    }

                    // If the camera is not in the list, add it to the list.
                    if (newitem)
                    {
                        // Create the item to display.
                        ListViewItem item = new ListViewItem
                        {
                            Content = cameraInfo[CameraInfoKey.FriendlyName]
                        };
                        // Create the tool tip text.
                        string toolTipText = "";
                        foreach (KeyValuePair<string, string> kvp in cameraInfo)
                        {
                            toolTipText += kvp.Key + ": " + kvp.Value + "\n";
                        }
                        item.ToolTip = toolTipText;

                        // Store the camera info in the displayed item.
                        item.Tag = cameraInfo;
                        // Attach the device data.
                        deviceListView.Items.Add(item);
                    }
                }


                // Remove old camera devices that have been disconnected.
                foreach (ListViewItem item in items)
                {
                    bool exists = false;

                    // For each camera in the list, check whether it can be found by enumeration.
                    foreach (ICameraInfo cameraInfo in allCameras)
                    {
                        if (((ICameraInfo)item.Tag)[CameraInfoKey.FullName] == cameraInfo[CameraInfoKey.FullName])
                        {
                            exists = true;
                            break;
                        }
                    }
                    // If the camera has not been found, remove it from the list view.
                    if (!exists)
                    {
                        deviceListView.Items.Remove(item);
                    }
                }
            }
            catch (Exception exception)
            {
                ShowException(exception);
            }
        }

        private void DestroyCamera()
        {
            // Disable all parameter controls.
            try
            {
                if (camera != null)
                {
                    //testImageControl.Parameter = null;
                    //pixelFormatControl.Parameter = null;
                    //widthSliderControl.Parameter = null;
                    //heightSliderControl.Parameter = null;
                    //gainSliderControl.Parameter = null;
                    //exposureTimeSliderControl.Parameter = null;
                }
            }
            catch (Exception exception)
            {
                ShowException(exception);
            }

            // Destroy the camera object.
            try
            {
                if (camera != null)
                {
                    camera.Close();
                    camera.Dispose();
                    camera = null;
                }
            }
            catch (Exception exception)
            {
                ShowException(exception);
            }
        }

        private void EnableButtons(bool canGrab, bool canStop)
        {
            //toolStripButtonContinuousShot.Enabled = canGrab;
            //toolStripButtonOneShot.Enabled = canGrab;
            //toolStripButtonStop.Enabled = canStop;
        }

        // Shows exceptions in a message box.
        private void ShowException(Exception exception)
        {
            MessageBox.Show("Exception caught:\n" + exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ContinuousShot(); // Start the grabbing of images until grabbing is stopped.
        }

        private void ContinuousShot()
        {
            try
            {
                // Start the grabbing of images until grabbing is stopped.
                camera.Parameters[PLCamera.AcquisitionMode].SetValue(PLCamera.AcquisitionMode.Continuous);
                camera.StreamGrabber.Start(GrabStrategy.OneByOne, GrabLoop.ProvidedByStreamGrabber);
            }
            catch (Exception exception)
            {
                ShowException(exception);
            }
        }
    }

}
