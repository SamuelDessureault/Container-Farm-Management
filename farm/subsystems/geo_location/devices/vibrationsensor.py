import seeed_python_reterminal.core as rt
import seeed_python_reterminal.acceleration as rt_accel
from seeed_python_reterminal.acceleration import AccelerationName 
import time
import math
from classes.sensors import ISensor, AReading
import math

class VibrationSensor(ISensor):
    # Properties to be initialized in constructor of implementation classes
    _sensor_model: str
    reading_type: AReading.Type

    #Varriables to keep track of net accelarations over time
    _accX1: int = 0
    _accY1: int = 0
    _accZ1: int = 0   

    _accX2: int = 0
    _accY2: int = 0
    _accZ2: int = 0   

    def __init__(self, gpio: int = 0,  model: str = 'LIS3DHTR 3-axis accelerometer', type: AReading.Type = AReading.Type.VIBRATION):
        #Because the sensor is internal, gpio is once again redundant
        self._sensor_model = model
        self.reading_type = type

        self.device = rt.get_acceleration_device()

    def get_accel_data(self) -> dict[int]:
        xGot:bool = False
        yGot:bool = False
        zGot:bool = False

        vals:dict = dict()

        for event in self.device.read_loop():
            accelEvent = rt_accel.AccelerationEvent(event)
            if accelEvent.name != None:
                if accelEvent.name == AccelerationName.X:
                    vals['x'] = accelEvent.value
                    xGot = True
                elif accelEvent.name == AccelerationName.Y:
                    vals['y'] = accelEvent.value
                    yGot = True
                elif accelEvent.name == AccelerationName.Z:
                    vals['z'] = accelEvent.value
                    zGot = True

                if xGot and yGot and zGot:
                    return vals

    def read_sensor(self) -> list[AReading]:
        readings = list[AReading]()

        point1 = self.get_accel_data()
        point2 = self.get_accel_data()

        vibration = math.sqrt( math.pow(point1['x'] - point2['x'], 2) + math.pow(point1['y'] - point2['y'], 2) + math.pow(point1['z'] - point2['z'], 2))

        readings.append(AReading(AReading.Type.VIBRATION, AReading.Unit.VIBRATION, vibration))

        return readings


if __name__ == "__main__":
    vibrationsensor = VibrationSensor(0, 'LIS3DHTR 3-axis accelerometer', AReading.Type.VIBRATION)

    while True:
        readings = vibrationsensor.read_sensor()

        for reading in readings:
            print( reading.__repr__())

        print('\n')
        time.sleep(2)