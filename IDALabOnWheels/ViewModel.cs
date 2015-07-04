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


        private bool staticActivity;
        public bool StaticActivity
        {
            get { return staticActivity; }

            set
            {
                staticActivity = value; 
                //if (staticActivity == true)
                //{
                //    BTPortsIsEnabled = true;
                //    ActivityIsEnabled = true;
                //    StartStopIsEnabled = false;
                //}
                //else
                //{
                //    BTPortsIsEnabled = false;
                //    ActivityIsEnabled = false;
                    
                //}
                NotifyPropertyChanged("StaticActivity");
            }
        }

        private bool dynamicActivity;
        public bool DynamicActivity
        {
            get { return dynamicActivity; }

            set
            {
                dynamicActivity = value;
                NotifyPropertyChanged("DynamicActivity");
            }
        }



        private bool btPortsIsEnabled;
        public bool BTPortsIsEnabled { get { return btPortsIsEnabled; } set { btPortsIsEnabled = value; NotifyPropertyChanged("BTPortsIsEnabled"); } }

        private bool activityIsEnabled;
        public bool ActivityIsEnabled { get { return activityIsEnabled; } set { activityIsEnabled = value; NotifyPropertyChanged("ActivityIsEnabled"); } }

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

        private string timeElapsed;
        public string TimeElapsed { get { return timeElapsed; } set { timeElapsed = value; NotifyPropertyChanged("TimeElapsed"); } }

        private string displayMsg;
        public string DisplayMessage { get { return displayMsg; } set { displayMsg = value; NotifyPropertyChanged("DisplayMessage"); } }

        private string altitude;
        public string Altitude { get { return altitude; } set { altitude = "Altitude: " + value; NotifyPropertyChanged("Altitude"); } }

        private string temperature;
        public string Temperature { get { return temperature; } set { temperature = "Temperature: " + value + "°C"; NotifyPropertyChanged("Temperature"); } }

        private bool accX;
        public bool AccXDisp { get { return accX; } set { accX = value; NotifyPropertyChanged("AccXDisp"); } }

        private bool accY;
        public bool AccYDisp { get { return accY; } set { accY = value; NotifyPropertyChanged("AccYDisp"); } }

        private bool accZ;
        public bool AccZDisp { get { return accZ; } set { accZ = value; NotifyPropertyChanged("AccZDisp"); } }

        private bool magX;
        public bool MagXDisp { get { return magX; } set { magX = value; NotifyPropertyChanged("MagXDisp"); } }

        private bool magY;
        public bool MagYDisp { get { return magY; } set { magY = value; NotifyPropertyChanged("MagYDisp"); } }

        private bool magZ;
        public bool MagZDisp { get { return magZ; } set { magZ = value; NotifyPropertyChanged("MagZDisp"); } }

        private bool roll;
        public bool RollDisp { get { return roll; } set { roll = value; NotifyPropertyChanged("RollDisp"); } }

        private bool pitch;
        public bool PitchDisp { get { return pitch; } set { pitch = value; NotifyPropertyChanged("PitchDisp"); } }

        private bool yaw;
        public bool YawDisp { get { return yaw; } set { yaw = value; NotifyPropertyChanged("YawDisp"); } }

        /// <summary>
        /// Default settings for the View Model
        /// </summary>
        public void SetupDefaultGUI()
        {
            StaticActivity = true; // also sets up default model as E100LP
            Lead1 = true;
            ActivityIsEnabled = false;
            rotateWorld = false;
            NotchON = true;
            DisplayMessage = "Click Start to begin!";
            Temperature = "0.0";
            Altitude = "0.0";
        }


    }
}
