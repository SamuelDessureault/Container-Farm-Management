from grove.adc import ADC
from typing import List
from classes.sensors import AReading, ISensor
from time import sleep


class WaterLevelSensor(ISensor):


    _sensor_model = "Grove Water Level Sensor"
    reading_type = AReading.Type.WATER

    def __init__(self, address: int,  model: str, type: AReading.Type) -> None:

        self._adc = ADC(address)
        self._address = address
        self._max_voltage = 5  
        self._sensor_model = model
        self.reading_type = type

    def read_sensor(self) -> list[AReading]:


        voltage = self._adc.read_voltage(4) * self._max_voltage / 1023.0


        water_ml = (voltage / 3.3) * 1000

        return [AReading(self.reading_type, AReading.Unit.WATER, water_ml)]
    
if __name__ == "__main__":
    water_level_sensor = WaterLevelSensor(0X04, "Grove Water Level Sensor", AReading.Type.WATER)
    while True:
        readings = water_level_sensor.read_sensor()
        print(readings)
        sleep(2)

