import seeed_python_reterminal.core as rt
import seeed_python_reterminal.acceleration as rt_accel
from seeed_python_reterminal.acceleration import AccelerationName 
import time
import math
from classes.sensors import ISensor, AReading

class PitchRollSensor(ISensor):
    # Properties to be initialized in constructor of implementation classes
    _sensor_model: str
    reading_type: AReading.Type

    #Varriables to keep track of net accelarations over time
    _netAccX: int = 0
    _netAccY: int = 0
    _netAccZ: int = 0

    _priorPitch = 0
    _priorRoll  = 0
    _priorYaw   = 0    

    def __init__(self, gpio: int = 0,  model: str = 'LIS3DHTR 3-axis accelerometer', type: AReading.Type = AReading.Type.PITCH_ROLL):
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

        data = self.get_accel_data()

        accelerationX = data['x']
        accelerationY = data['y']
        accelerationZ = data['z']

        pitch = 0
        roll =  0
        yaw =   0

        
        try:
            pitch = 180 * math.atan (accelerationX / math.sqrt(accelerationY * accelerationY + accelerationZ * accelerationZ)) / math.pi
        except: #For divide by zeros that are a possibility
            pitch = 0

        try:
            roll =  180 * math.atan (accelerationY / math.sqrt(accelerationX * accelerationX + accelerationZ * accelerationZ)) / math.pi
        except:
            roll = 0


        readings.append(AReading(AReading.Type.PITCH_ROLL, AReading.Unit.PITCH, pitch))
        readings.append(AReading(AReading.Type.PITCH_ROLL, AReading.Unit.ROLL, roll))

        return readings


if __name__ == "__main__":
    pitchrollsensor = PitchRollSensor(0, 'LIS3DHTR 3-axis accelerometer', AReading.Type. PITCH_ROLL)

    while True:
        readings = pitchrollsensor.read_sensor()

        for reading in readings:
            print( reading.__repr__())

        print('\n')
        time.sleep(2)