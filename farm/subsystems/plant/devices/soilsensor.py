from grove.adc import ADC
from typing import List
from classes.sensors import AReading, ISensor
from time import sleep


class SoilLevelSensor(ISensor):



    def __init__(self, address: int, model: str, type: AReading.Type):

        self._adc = ADC(address)
        self._channel = address
        self._sensor_model = model
        self.reading_type = type


    def read_sensor(self) -> List[AReading]:

        raw_value = self._adc.read_voltage(2)
        moisture_percent = (200 - ((raw_value / 1023) * 100))
        return [AReading(self.reading_type, AReading.Unit.SOIL_MOISTURE, moisture_percent)]
                           
if __name__ == "__main__":
    
        soil_moisture_sensor = SoilLevelSensor(0X04, "Grove Capacitive Soil Moisture Sensor", AReading.Type.SOIL_MOISTURE)
        while True:
            readings = soil_moisture_sensor.read_sensor()
            for reading in readings:
                print(reading)
                sleep(1)