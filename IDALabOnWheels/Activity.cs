using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDALabOnWheels
{
    // Every activity has a timer and a process() function that decides whether the activity is complete or not
    class IActivity
    {
        protected ActivityTimer _timer;
        protected bool _running;
        public Action NotifyPerSec;

        public TimeSpan ElapsedTime
        {
            get { 
            if(_timer != null) return(_timer.GetElapsedTime());
            else return(new TimeSpan(0));
            }
        }

        public void Start()
        {
            _timer = new ActivityTimer();
            _timer.NotifyPerSec = NotifyPerSec;
            _timer.Pause = false;
            _timer.Start(false, new TimeSpan(0, 0, 0));
            _running = true;
        }

        public void Pause()
        {
            _timer.Pause = true;
        }

        public void Resume()
        {
            _timer.Pause = false;
        }

        public virtual bool Process()
        {
            return false;
        }

        public void Stop()
        {
            _timer.Stop();
            _running = false;
        }


    }
}
