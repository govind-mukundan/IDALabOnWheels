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
            _timer.Start(false, new TimeSpan(0, 0, 0));
        }

        public virtual bool Process()
        {
            return false;
        }

        public void Stop()
        {
            _timer.Stop();
        }


    }
}
