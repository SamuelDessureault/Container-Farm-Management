from classes.sensors import ISensor, AReading
from grove.adc import ADC
import time

class NoiseSensor(ISensor):
    """Class for the noise sensor device.
    """

    _device: ADC

    def __init__(self, gpio: int, model: str, type: AReading.Type):
        self._device = ADC(0x04)
        self._sensor_model = model
        self.reading_type = type

    def read_sensor(self) -> list[AReading]:
        # The higher the sensor's value is, the quieter it is.
        readings = [AReading(self.reading_type, AReading.Unit.NOISE, self._device.read_voltage(0))]
        return readings
    
if __name__ == "__main__":
    SLEEP_TIME = 1
    sensor: NoiseSensor = NoiseSensor(1, "Sound Detector", AReading.Type.NOISE)
    while True:
        print(sensor.read_sensor()[0])
        time.sleep(SLEEP_TIME)