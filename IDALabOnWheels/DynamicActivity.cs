using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace IDALabOnWheels
{
    class DynamicActivity : IActivity
    {
        // we need to find the roll rate in RPM
        float EXP_ROLL_RATE = 15f;
        float TOLERANCE = 5f;

        // To count a rotation, look for a transition from 359 to 0

        public bool Process(Attitude att)
        {
            return (false);
            if (!_running) return false;

            //if (att.angleX > EXP_X + TOLERANCE || att.angleX < EXP_X - TOLERANCE ||
            //    att.angleY > EXP_Y + TOLERANCE || att.angleY < EXP_Y - TOLERANCE ||
            //    att.heading > EXP_H + TOLERANCE || att.heading < EXP_H - TOLERANCE)
            //{
            //    Debug.WriteLine("Arritude Exceeded Activity Threshold! " + "X = {0}, Y = {1}, Head = {2}", att.angleX, att.angleY, att.heading);

            //    return true;
            //}
            //else
            //{
            //    return false;
            //}
        }

        /// <summary>
        /// Override the base stop(). The new keyword makes this clear
        /// </summary>
        public new void Stop()
        {
            MessageBox.Show("Dynamic Activity is Done!");
            base.Stop();
        }


    }
}
