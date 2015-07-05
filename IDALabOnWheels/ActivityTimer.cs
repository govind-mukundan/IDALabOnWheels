using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDALabOnWheels
{
    // Timer routines to measure the activity duration
    class ActivityTimer
    {
        System.Timers.Timer _utcTimer;
        TimeSpan _time;
        TimeSpan _oneSec;
        public Action NotifyPerSec;
        public Action NotifyOnCompletion;
        public bool Pause;
        bool CountdownMode;

        private void OneSecTimer(object sender, EventArgs e)
        {

            if (!CountdownMode)
            {
                _utcTimer.Start();
                if (!Pause) _time = _time.Add(_oneSec);
            }
            else
            {
                if (_time >= _oneSec)
                {
                    _utcTimer.Start();
                    if (!Pause) _time = _time.Subtract(_oneSec);
                }
                else
                {
                    if (NotifyOnCompletion != null)
                    {
                        NotifyOnCompletion();
                    }
                }
            }

            if (NotifyPerSec != null)
            {
                NotifyPerSec();
            }
        }

        public void Start(bool countdownMode, TimeSpan startTime)
        {
            if (_utcTimer == null)
            {
                _utcTimer = new System.Timers.Timer();
                _utcTimer.Interval = 1000;
                _utcTimer.Elapsed += OneSecTimer;
                _utcTimer.AutoReset = false; // Imp: Autoreset = false will ensure that the timer does not trigger more than once without a renewed call to Start()
                _oneSec = new TimeSpan(0, 0, 1);
            }
            CountdownMode = countdownMode;
            _utcTimer.Start();
            _time = startTime;
            if (NotifyPerSec != null)
            {
                NotifyPerSec();
            }
        }

        public void Stop()
        {
            if (_utcTimer == null) return;

            _utcTimer.Stop();
            _utcTimer = null;
        }

        public TimeSpan GetElapsedTime()
        {
            return (_time);
        }

    }
}
