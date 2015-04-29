using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDALabOnWheels
{
    /// <summary>
    /// Standard viewmodel class base, simply allows
    /// property change notifications to be sent.
    /// </summary>
    public class ViewModelEntity : INotifyPropertyChanged
    {
        /// <summary>
        /// The property changed event.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the property changed event.
        /// </summary>
        /// <param name="propertyName">Name
        ///              of the property.</param>
        public virtual void NotifyPropertyChanged(string propertyName)
        {
            //  If the event has been subscribed to, fire it.
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
