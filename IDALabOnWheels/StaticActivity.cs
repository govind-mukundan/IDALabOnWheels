using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDALabOnWheels
{
    class StaticActivity : IActivity
    {

        float EXP_X = 0f;
        float EXP_Y = 0f;
        float EXP_H = 0f;
        float TOLERANCE = 10f; // 10 degree tolerance

        /// <summary>
        /// Returns TRUE if attitude is within threshold, false if out of threshold
        /// </summary>
        /// <param name="att"></param>
        /// <returns></returns>
        public bool Process(Attitude att)
        {
            if (att.angleX > EXP_X + TOLERANCE || att.angleX < EXP_X - TOLERANCE ||
                att.angleY > EXP_Y + TOLERANCE || att.angleY < EXP_Y - TOLERANCE ||
                att.heading > EXP_H + TOLERANCE || att.heading < EXP_H - TOLERANCE)
            {
                Debug.WriteLine("Arritude Exceeded Activity Threshold! " + "X = {0}, Y = {1}, Head = {2}", att.angleX, att.angleY, att.heading);

                _timer.Stop();
                return true;
            }
            else
            {
                return false;
            }
            
        }
    }
}
