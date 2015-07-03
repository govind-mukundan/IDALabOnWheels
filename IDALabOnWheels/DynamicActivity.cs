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

        class RotStamp
        {
            public int Quadrant;
            public TimeSpan ElapsedTime;

            public RotStamp(int quad, TimeSpan ts)
            {
                this.Quadrant = quad;
                this.ElapsedTime = ts;
            }
        }
        // we need to find the roll rate in RPM
        float EXP_ROLL_RATE = 15f;
        float TOLERANCE = 5f;
        Queue<RotStamp> _queue;
        bool _debug = true;
        public int CurrentRPM;
        TimeSpan _timeoutStamp;
        int RESET_TIME_S = 10; // 15s without new data will reset the RPM
        TimeSpan _timeout;
        int C_ROLL_MOTION = 1;
        int C_HEAD_MOTION = 2;
        int _rotateAxis;

        public DynamicActivity()
        {
            _queue = new Queue<RotStamp>();
            _timeout = new TimeSpan(0,0,RESET_TIME_S);
            _rotateAxis = C_ROLL_MOTION;
        }

        // To count a rotation:

        /// <summary>
        /// For simplicity, rotation is restricted to only the heading vector (i.e. along the axis perpendicular to the baord)
        /// To count a revolution:
        /// 1. Classify the current angle into a quadrant and add it to a buffer if it's different from the last classification
        /// 2. Look for sequence of mod(1-2-3-4-1) in the last 4 entries and calculate the speed by finding the time difference between entry 1 and 5
        /// </summary>
        /// <param name="att"></param>
        /// <returns></returns>
        public bool Process(Attitude att)
        {

            int[] refQuad = new int[] { 1, 2, 3, 4, 1, 2, 3, 4 };
            int refIndex;
            float angle = 0f;

            if (!_running) return false;

            // if heading is -ve, shift it into the range of 180 - 360 for more intutive calculations
            if (_rotateAxis == C_HEAD_MOTION)
            {
                if (att.heading < 0f)
                    angle = 360.0f + att.heading;
                else
                    angle = att.heading;
            }
            else if (_rotateAxis == C_ROLL_MOTION)
            {
                if (att.angleX < 0f)
                    angle = 360.0f + att.angleX;
                else
                    angle = att.angleX;
            }

            int quad = GetQuadrantFromAngle(angle);
            Debug.WriteLineIf(_debug, "Quadrant = " + quad.ToString());
            if (_queue.Count == 0 || quad != _queue.ElementAt(_queue.Count - 1).Quadrant)
            {
                // We've entered a new quadrant snapshot the time
                RotStamp point = new RotStamp(quad, this.ElapsedTime);
                _queue.Enqueue(point);
                _timeoutStamp = this.ElapsedTime;
            }

            // reset if there is no new quadrant in 15 seconds
            if (this.ElapsedTime - _timeoutStamp > _timeout)
                CurrentRPM = 0;

            // See if there are 5 elements matching the sequence 1-2-3-4-1
            bool revFlag = true;

            while (_queue.Count >= 5)
            {
                refIndex = _queue.ElementAt(0).Quadrant - 1;
                for (int j = 0; j < 5; j++)
                {
                    // Clockwise
                    if (refQuad[refIndex + j] != _queue.ElementAt(j).Quadrant)
                    {
                        revFlag = false;
                        break;
                    }
                }

                refIndex = _queue.ElementAt(0).Quadrant + 4 - 1;
                for (int j = 0; j < 5 && revFlag == false; j++)
                {
                    // Antii-clockwise
                    if (refQuad[refIndex - j] != _queue.ElementAt(j).Quadrant)
                    {
                        revFlag = false;
                        break;
                    }
                }
                if (revFlag == true)
                {
                    Debug.WriteLineIf(_debug, "Found a revolution! + Start: " + _queue.ElementAt(0).Quadrant + " End : " + _queue.ElementAt(4).Quadrant);
                    Debug.WriteLineIf(_debug, "Found a revolution! + Start: " + _queue.ElementAt(0).ElapsedTime.ToString() + " End : " + _queue.ElementAt(4).ElapsedTime.ToString());
                    Debug.WriteLine("Speed = " + GetRPM(_queue.ElementAt(4).ElapsedTime - _queue.ElementAt(0).ElapsedTime).ToString());
                    CurrentRPM = Convert.ToInt32(GetRPM(_queue.ElementAt(4).ElapsedTime - _queue.ElementAt(0).ElapsedTime) + 0.5f);
                }
                _queue.Dequeue(); //pop out an element for every 5 entries
            }

            return (false);
        }

        float GetRPM(TimeSpan ts)
        {
            return (60f * 1000f / (float)ts.TotalMilliseconds);
        }

        int GetQuadrantFromAngle(float angle)
        {
            if (angle >= 0.0f && angle < 90.0f)
            {
                return 1;
            }
            else if (angle >= 90.0f && angle < 180.0f)
            {
                return 2;
            }
            else if (angle >= 180.0f && angle < 270.0f)
            {
                return 3;
            }
            else if (angle >= 270.0f && angle <= 360.0f)
            {
                return 4;
            }
            else
            {
                Debug.WriteLine("Error: Unknown Quadrant!!");
                return 4;
            }

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
