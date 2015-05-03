using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDALabOnWheels
{
    class ViewModel : ViewModelEntity 
    {

        // Data binding to combo box - http://stackoverflow.com/questions/58743/databinding-an-enum-property-to-a-combobox-in-wpf


        private bool modeBluetooth;
        public bool ModeBluetooth
        {
            get { return modeBluetooth; }

            set
            {
                modeBluetooth = value; 
                if (modeBluetooth == true)
                {
                    BTPortsIsEnabled = true;
                    BTConnectIsEnabled = true;
                    StartStopIsEnabled = false;
                }
                else
                {
                    BTPortsIsEnabled = false;
                    BTConnectIsEnabled = false;
                    
                }
                NotifyPropertyChanged("ModeBluetooth");
            }
        }



        private bool btPortsIsEnabled;
        public bool BTPortsIsEnabled { get { return btPortsIsEnabled; } set { btPortsIsEnabled = value; NotifyPropertyChanged("BTPortsIsEnabled"); } }

        private bool btConnectIsEnabled;
        public bool BTConnectIsEnabled { get { return btConnectIsEnabled; } set { btConnectIsEnabled = value; NotifyPropertyChanged("BTConnectIsEnabled"); } }

        private bool startStopIsEnabled;
        public bool StartStopIsEnabled { get { return startStopIsEnabled; } set { startStopIsEnabled = value; NotifyPropertyChanged("StartStopIsEnabled"); } }


        public byte LeadNumber { get; set; }
        private bool lead1;
        public bool Lead1 { get { return lead1; } set { lead1 = value; if (lead1 == true) LeadNumber = 1; else LeadNumber = 0; NotifyPropertyChanged("Lead1"); } }

        private bool lead2;
        public bool Lead2 { get { return lead2; } set { lead2 = value; if (lead2 == true) LeadNumber = 0; else LeadNumber = 1; NotifyPropertyChanged("Lead2"); } }

        private bool rotateWorld;
        public bool RotateWorld { get { return rotateWorld; } set { rotateWorld = value; NotifyPropertyChanged("RotateWorld"); } }

        private bool notchON;
        public bool NotchON { get { return notchON; } set { notchON = value; NotifyPropertyChanged("NotchON"); } }


        /// <summary>
        /// Default settings for the View Model
        /// </summary>
        public void SetupDefaultGUI()
        {
            ModeBluetooth = true; // also sets up default model as E100LP
            Lead1 = true;
            StartStopIsEnabled = false;
            rotateWorld = false;
            NotchON = true;
        }


    }
}
