from classes.sensors import ISensor, AReading
from classes.actuators import IActuator, ACommand, AState
from .devices.geo_buzzer import Buzzer
from .devices.accelarationsensor import PitchRollSensor
from .devices.gpssensor import GPSSensor
from .devices.vibrationsensor import VibrationSensor
import time
import json
from classes.subsystem import ISubSystem

class GeoLoactionSubSystem(ISubSystem):

    def __init__(self) -> None:
        self._sensors: list[ISensor] = self._initialize_sensors()
        self._actuators: list[IActuator] = self._initialize_actuators()

    def _initialize_sensors(self) -> list[ISensor]:
        """Initializes all sensors and returns them as a list. Intended to be used in class constructor.

        :return List[ISensor]: List of initialized sensors.
        """

        return [
            # Instantiate each sensor inside this list, separate items by comma.
            PitchRollSensor(0, 'LIS3DHTR 3-axis accelerometer', AReading.Type. PITCH_ROLL),
            VibrationSensor(0, 'LIS3DHTR 3-axis accelerometer', AReading.Type.VIBRATION),
            GPSSensor(9600, 'Air530', AReading.Type.GPS)
        ]

    def _initialize_actuators(self) -> list[IActuator]:
        """Initializes all actuators and returns them as a list. Intended to be used in class constructor

        :return list[IActuator]: List of initialized actuators.
        """
        initialState = dict()
        initialState['value'] = False

        return [
            # Instantiate each actuator inside this list, separate items by comma.
            Buzzer(0, ACommand.Type.BUZZER, initialState)

        ]

    def read_sensors(self) -> list[AReading]:
        """Reads data from all initialized sensors. 

        :return list[AReading]: a list containing all readings collected from sensors.
        """
        readings: list[AReading] = []

        for sensor in self._sensors:
            tmp_readings:list[AReading] = sensor.read_sensor()
            for reading in tmp_readings:
                readings.append(reading)
        return readings

    def control_actuators(self, command: ACommand) -> None:
        """Controls actuators according to a list of commands. Each command is applied to it's respective actuator.

        :param list[ACommand] commands: List of commands to be dispatched to corresponding actuators.
        """
        for actuator in self._actuators:
            if(command.target_type == actuator.type):
                actuator.control_actuator(command.data)

    def get_current_actuator_states(self) -> list[AState]:
        states: list[AState] = []

        for actuator in self._actuators:
            state:AState = actuator.get_current_state()
            states.append(state)
        return states

if __name__ == "__main__":
    """This script is intented to be used as a module, however, code below can be used for testing.
    """

    device_manager = GeoLoactionSubSystem()

    TEST_SLEEP_TIME = 2

    actuatorDict = dict()
    is_on:bool = False

    while True:
        print(device_manager.read_sensors())
        print(device_manager.read_actuators())
        
        if is_on:
            fake_buzzer_message_body = '{"value": false}'
            is_on = False
        else:
            fake_buzzer_message_body = '{"value": true}'
            is_on = True
        fake_buzzer_command = ACommand(
            ACommand.Type.BUZZER, fake_buzzer_message_body)

        device_manager.control_actuators([fake_buzzer_command])

        time.sleep(TEST_SLEEP_TIME)