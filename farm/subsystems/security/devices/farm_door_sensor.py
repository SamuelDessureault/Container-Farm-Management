from classes.sensors import ISensor, AReading
from gpiozero import Button
import time

class FarmDoorSensor(ISensor):
    """Class for the farm door sensor device.
    """
    
    _device: Button

    def __init__(self, gpio: int, model: str, type: AReading.Type):
        self._device = Button(gpio)
        self._sensor_model = model
        self.reading_type = type

    def read_sensor(self) -> list[AReading]:
        # 0 if the door is open, 1 if it is closed.
        readings = [AReading(self.reading_type, AReading.Unit.UNITLESS, self._device.value)]
        return readings

if __name__ == "__main__":
    SLEEP_TIME = 1
    sensor: FarmDoorSensor = FarmDoorSensor(22, "Magnetic Dorr Sensor reed Switch", AReading.Type.DOOR)
    while True:
        print(sensor.read_sensor()[0])
        time.sleep(SLEEP_TIME)
