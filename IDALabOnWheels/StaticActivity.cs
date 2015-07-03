using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace IDALabOnWheels
{
    class StaticActivity : IActivity
    {

        float EXP_X = 0f;
        float EXP_Y = 0f;
        float EXP_H = 0f;
        float TOLERANCE = 10f; // 10 degree tolerance
        Attitude _attOutOfRange;

        /// <summary>
        /// To configure the starting point of the static activity
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void SetReference(float x, float y, float h)
        {
            EXP_X = x; EXP_Y = y; EXP_H = h;
        }
        /// <summary>
        /// Returns TRUE if attitude is within threshold, false if out of threshold
        /// </summary>
        /// <param name="att"></param>
        /// <returns></returns>
        public bool Process(Attitude att)
        {
            if (!_running) return false;

            if (att.angleX > EXP_X + TOLERANCE || att.angleX < EXP_X - TOLERANCE ||
                att.angleY > EXP_Y + TOLERANCE || att.angleY < EXP_Y - TOLERANCE ||
                att.heading > EXP_H + TOLERANCE || att.heading < EXP_H - TOLERANCE)
            {
                Debug.WriteLine("Arritude Exceeded Activity Threshold! " + "X = {0}, Y = {1}, Head = {2}", att.angleX, att.angleY, att.heading);
                _attOutOfRange = att;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Override the base stop(). The new keyword makes this clear
        /// </summary>
        public new void Stop()
        {
            base.Stop();
            string message = "";
            if (_attOutOfRange.angleX > EXP_X + TOLERANCE || _attOutOfRange.angleX < EXP_X - TOLERANCE)
            {
                message += "Pitch out of range, expected " + EXP_X + "° Got " + _attOutOfRange.angleX.ToString() + "°";
            }
            if (_attOutOfRange.angleY > EXP_Y + TOLERANCE || _attOutOfRange.angleY < EXP_Y - TOLERANCE)
            {
                message += "Roll out of range, expected " + EXP_Y + "° Got " + _attOutOfRange.angleY.ToString() + "°";
            }
            if (_attOutOfRange.heading > EXP_H + TOLERANCE || _attOutOfRange.heading < EXP_H - TOLERANCE)
            {
                message += "Heading out of range, expected " + EXP_H + "° Got " + _attOutOfRange.heading.ToString() + "°";
            }

            message += "\n Total time elapsed = " + this.ElapsedTime.ToString();
            MessageBox.Show(message);
        }
    }
}
