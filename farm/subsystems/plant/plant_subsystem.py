from classes.sensors import AReading, ISensor
from classes.actuators import IActuator, ACommand, AState
from classes.outputdevice import IOutputDevice
from .devices.fan_control import FanController
from .devices.rgbstick import GroveRGBLEDStick
from .devices.soilsensor import SoilLevelSensor
from .devices.temphumi import TemperatureSensor
from .devices.watersensor import WaterLevelSensor
from time import sleep
from abc import ABC, abstractmethod
from rpi_ws281x import Color
from classes.subsystem import ISubSystem

class FarmPlantSubSystem(ISubSystem):
    #Note: for RBG must run as sudo


    def __init__(self, sensors: list[ISensor], actuators: list[IActuator]) -> None:
        self._sensor_list = sensors
        self._actuator_list = actuators

    
    def read_sensors(self) -> list[AReading]:
        """Reads data from all sensors. 
        :return list[AReading]: a list containing all readings collected from sensors.
        """
        readings: list[AReading] = []

        for sensor in self._sensor_list:
            readings.extend(sensor.read_sensor())

        return readings
    

    def control_actuators(self, command: ACommand) -> None:
        """Controls actuators according to a command.
        :param ACommand command: the command to be dispatched to the corresponding actuator.
        """
        actuator_list: list[IActuator] = [ i for i in self._actuator_list if i.type == command.target_type]
        for actuator in actuator_list:
            if actuator.validate_command(command):
                actuator.control_actuator(command.data)   

    def get_current_actuator_states(self) -> list[AState]:
        state_list: list[AState] = []
        
        for actuator in self._actuator_list:
            state_list.append(actuator.get_current_state())

        return state_list
    
    def loop_script(self) -> None:
        while True:
            sensor_readings = self.read_sensors()
            print(sensor_readings)
            sleep(2)
            self.control_actuators(ACommand(ACommand.Type.RGB, '{"led_index": 1, "color": [255, 0, 0]}'))
            self.control_actuators(ACommand(ACommand.Type.RGB, '{"led_index": 6, "color": [0, 255, 0]}'))
            sleep(2)
            self.control_actuators(ACommand(ACommand.Type.RGB, '{"led_index": 1, "color": [0, 0, 0]}'))
            self.control_actuators(ACommand(ACommand.Type.RGB, '{"led_index": 6, "color": [0, 0, 0]}'))

            self.control_actuators(ACommand(ACommand.Type.FAN, "1"))
            sleep(2)
            self.control_actuators(ACommand(ACommand.Type.FAN, "0"))



    
if __name__ == "__main__":

    
    sensors: list[ISensor] = [
        TemperatureSensor(gpio=4, model='GroveTemperatureHumidityAHT20', type= AReading.Type.TEMPERATURE),
        SoilLevelSensor(0x04, "Grove Capacitive Soil Moisture Sensor", AReading.Type.SOIL_MOISTURE),
        WaterLevelSensor(0x04, "Grove Water Level Sensor", AReading.Type.WATER)
    ]


    actuators: list[IActuator] = [
        FanController(24, ACommand.Type.FAN, "True"),
        GroveRGBLEDStick(18, 10, ACommand.Type.RGB, {"value": Color(0, 0, 0)})
    ]
    

    farm = FarmPlantSubSystem(sensors, actuators)
    farm.loop_script()


