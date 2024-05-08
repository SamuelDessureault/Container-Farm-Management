from abc import ABC, abstractmethod

class IOutputDevice(ABC):
    """Interface for all output devices."""

    @abstractmethod
    def __init__(self, gpio: int, model: str):
        """Constructor for the IOutputDevice class.
        :param int gpio: The GPIO number or pin identifier for the output device.
        :param str model: The model of the output device hardware.
        """
        pass

    @abstractmethod
    def set_output(self, *args, **kwargs):
        """Sets the output for the device.
        :param *args: Variable-length non-keyword arguments.
        :param **kwargs: Variable-length keyword arguments.
        """
        pass
