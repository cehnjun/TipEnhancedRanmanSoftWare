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

namespace TERS.CameraControl
{
    public partial class CameraControlViewModel : ObservableObject
    {
        private Camera camera = null;
        private PixelDataConverter converter = new PixelDataConverter();
        private Stopwatch stopWatch = new Stopwatch(); 

        public CameraControlViewModel()
        {

        }
    
        public ICommand startGrabCmd => new RelayCommand(StartGrab);

        private void StartGrab()
        {
            CameraTools.Grab();
        }


    }
}
