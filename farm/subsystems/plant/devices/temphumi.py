from grove.grove_temperature_humidity_aht20 import GroveTemperatureHumidityAHT20
import time
from typing import List
from classes.sensors import AReading, ISensor

class TemperatureSensor(ISensor):

    def __init__(self, gpio:int ,model: str, type: AReading.Type) -> None:
        self.sensor = GroveTemperatureHumidityAHT20(bus=gpio)
        self.model = model
        self.type = type

    def read_sensor(self) -> list[AReading]:
        temperature, humidity = self.sensor.read()
        return [AReading(AReading.Type.TEMPERATURE, AReading.Unit.CELCIUS, temperature),
                AReading(AReading.Type.HUMIDITY, AReading.Unit.HUMIDITY, humidity)]

if __name__ == '__main__':
    tempSensor = TemperatureSensor(gpio=4, model='GroveTemperatureHumidityAHT20', type= AReading.Type.TEMPERATURE)

    tempHumi = tempSensor.read_sensor()

    for i in tempHumi:
        print(i)
    time.sleep(1)