using ContainerFarmManagement.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//STS technologies
//Semester 6 - 2023-04-27
// App Dev III
// Object to represent a Reading


namespace ContainerFarmManagement.Models
{
    public class Reading : INotifyPropertyChanged
    {
        /// <summary>
        /// Enum of all sensor types for readings.
        /// </summary>
        public enum SensorTypes
        {
            [Description("temperature")]
            TEMPERATURE,
            [Description("humidity")]
            HUMIDITY,
            [Description("water")]
            WATERLVL,
            [Description("soil_moisture")]
            SOIL_MOISTURE,
            [Description("noise")]
            NOISE,
            [Description("luminosity")]
            LUMINOSITY,
            [Description("motion")]
            MOTION,
            [Description("door")]
            DOOR,
            [Description("door_lock")]
            DOOR_LOCK,
            [Description("gps")]
            GPS,
            [Description("pitch-roll")]
            PITCH_ROLL,
            [Description("vibration")]
            VIBRATION,
            [Description("buzzer")]
            BUZZER,
        }

        /// <summary>
        /// Enum of all sensor reading units.
        /// </summary>
        public enum Units
        {
            [Description("C")]
            CELCIUS,
            [Description("% HR")]
            HUMIDITY,
            [Description("degrees-latitude")]
            DEGREES_LATITUDE,
            [Description("degrees-longitude")]
            DEGREES_LONGITUDE,
            [Description("pitch")]
            PITCH,
            [Description("roll")]
            ROLL,
            [Description("vibration")]
            VIBRATION,
            [Description("mL")]
            MILLILITER,
            [Description("%")]
            MOISTURELVL,
            [Description("noise")]
            NOISE,
            [Description("")]
            UNITLESS
        }

        /// <summary>
        /// The reading's container key.
        /// </summary>
        public string ContainerKey { get; set; }

        /// <summary>
        /// The reading's sensor type.
        /// </summary>
        public SensorTypes SensorType { get; set; }

        /// <summary>
        /// The reading's unit type.
        /// </summary>
        public Units Unit { get; set; }

        /// <summary>
        /// The reading's value.
        /// </summary>
        public float Value { get; set; }

        public string DeviceId { get; set; }

        /// <summary>
        /// The reading's date of creation.
        /// </summary>
        public DateTime TimeStamp { get; set; }

        public Reading(string containerKey, SensorTypes sensorTypes, Units unit, float value, DateTime timeStamp)
        {
            ContainerKey = containerKey;
            SensorType = sensorTypes;
            Unit = unit;
            Value = value;
            TimeStamp = timeStamp;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}