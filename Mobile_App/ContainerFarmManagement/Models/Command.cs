using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//STS technologies
//Semester 6 - 2023-04-27
// App Dev III
// Object to store data for commands, to eventually be sent to actuators

namespace ContainerFarmManagement.Models
{
    public class Command
    {

        /// <summary>
        /// Enum of all actuator types.
        /// </summary>
        public enum ActuatorTypes
        {
            [Description("fan")]
            FAN,
            [Description("rgb")]
            RGB,
            [Description("buzzer")]
            BUZZER,
            [Description("lock")]
            LOCK,
        }

        /// <summary>
        /// The command's target actuator type.
        /// </summary>
        public ActuatorTypes Target { get; set; }

        /// <summary>
        /// The dictionary of values to be sent with the command.
        /// </summary>
        public Dictionary<string, object> Values;

        public Command(ActuatorTypes target)
        {
            Values = new Dictionary<string, object>();
        }
        public Command(ActuatorTypes target, Dictionary<string, object> values)
        {
            Target = target;
            Values = values;
        }
    }
}