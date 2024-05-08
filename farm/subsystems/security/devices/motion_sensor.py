from classes.sensors import ISensor, AReading
from gpiozero import Button
import time

class MotionSensor(ISensor):
    """Class for the motion sensor device.
    """

    _device: Button

    def __init__(self, gpio: int, model: str, type: AReading.Type):
        self._device = Button(gpio)
        self._sensor_model = model
        self.reading_type = type

    def read_sensor(self) -> list[AReading]:
        readings = [AReading(self.reading_type, AReading.Unit.UNITLESS, self._device.value)]
        return readings

if __name__ == "__main__":
    SLEEP_TIME = 0
    sensor: MotionSensor = MotionSensor(16, "Grove Adjustable PIR Motion Sensor", AReading.Type.MOTION)
    while True:
        print(sensor.read_sensor()[0])
        time.sleep(SLEEP_TIME)
