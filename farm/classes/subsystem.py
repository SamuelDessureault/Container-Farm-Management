from abc import ABC, abstractmethod
from classes.sensors import ISensor, AReading
from classes.actuators import IActuator, ACommand, AState

class ISubSystem(ABC):
    """Interface for all subsystems. 
    
    *Will be moved to a different location at a later milestone.
    """
    _sensor_list: list[ISensor]
    _actuator_list: list[IActuator]

    @abstractmethod
    def __init__(self, sensors: list[ISensor], actuators: list[IActuator]) -> None:
        """Constructor for SubSystem class. Must define interface's class properties

        :param list[ISensor] sensors: List of sensors
        :param list[IActuator] actuators: List of actuators
        """
        pass

    @abstractmethod
    def read_sensors(self) -> list[AReading]:
        """Takes a reading form the sensors

        :return list[AReading]: List of readinds measured by the sensors. Most sensors return a list with a single item.
        """
        pass

    @abstractmethod
    def control_actuators(self, command: ACommand) -> None:
        """Sets the actuators to the value passed as argument.

        :param dict value: dictionary containing keys and values with command information.
        :return bool: True if the state of the actuator changed, false otherwise.
        """
        pass

    @abstractmethod
    def get_current_actuator_states(self) -> list[AState]:
        """Get the current state of all actuators.

        :return list[AState]: list of all actuators' current states.
        """
        pass