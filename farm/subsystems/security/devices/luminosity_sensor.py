from classes.sensors import ISensor, AReading
import time
import seeed_python_reterminal.core as rt

class LuminositySensor(ISensor):
    """Class for the luminosity sensor device.
    """

    def __init__(self, gpio: int, model: str, type: AReading.Type):
        self._sensor_model = model
        self.reading_type = type

    def read_sensor(self) -> list[AReading]:
        readings = [AReading(self.reading_type, AReading.Unit.UNITLESS, rt.illuminance)]
        return readings

if __name__ == "__main__":
    SLEEP_TIME = 1
    sensor: LuminositySensor = LuminositySensor(0, "reTerminal Illuminance Sensor", AReading.Type.LUMINOSITY)
    while True:
        print(sensor.read_sensor()[0])
        time.sleep(SLEEP_TIME)
