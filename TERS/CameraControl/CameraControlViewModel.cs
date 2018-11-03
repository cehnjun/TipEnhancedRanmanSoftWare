using MicroMvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CameraApi;
using System.Windows.Input;
using Basler.Pylon;
using System.Diagnostics;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows;

namespace TERS.CameraControl
{
    public partial class CameraControlViewModel : ObservableObject
    {
        private Camera camera = null;
        private PixelDataConverter converter = new PixelDataConverter();
        private Stopwatch stopWatch = new Stopwatch();
        CameraControl CameraControl = null;

        public CameraControlViewModel()
        {

        }
    
        public ICommand ContinuousShotCmd => new RelayCommand(ContinuousShot);

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

        private void ShowException(Exception exception)
        {
            System.Windows.MessageBox.Show("Exception caught:\n" + exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

    }
}
