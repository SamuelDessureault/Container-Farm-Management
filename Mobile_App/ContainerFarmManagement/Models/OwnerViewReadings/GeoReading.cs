using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ContainerFarmManagement.Models.OwnerViewReadings
{
    public class GeoReading : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private double _pitch;
        public double Pitch
        {
            get { return _pitch; }
            set
            {
                _pitch = value;
                OnPropertyChanged(nameof(Pitch));
            }
        }

        private double _roll;
        public double Roll
        {
            get { return _roll; }
            set
            {
                _roll = value;
                OnPropertyChanged(nameof(Roll));
            }
        }

        private double _vibration;
        public double Vibration
        {
            get { return _vibration; }
            set
            {
                _vibration = value;
                OnPropertyChanged(nameof(Vibration));
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
