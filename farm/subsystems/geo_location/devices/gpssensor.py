import io
import time
import pynmea2
import serial
from classes.sensors import ISensor, AReading


class GPSSensor(ISensor):
    """Interface for all sensors.
    """

    # Properties to be initialized in constructor of implementation classes
    _sensor_model: str
    reading_type: AReading.Type

    def __init__(self, gpio: int = 0,  model: str = 'Air530', type: AReading.Type = AReading.Type.GPS):
        """Constructor for Sensor  class. May be called from childclass.

        :param str model: specific model of sensor hardware. Ex. AHT20 or LTR-303ALS-01
        :param ReadingType type: Type of reading this sensor produces. Ex. 'TEMPERATURE'
        """
        self._sensor_model = model
        self.reading_type = type

        #Because of the comunication with the gps the gpio is entirely redundant 
        self.ser = serial.Serial('/dev/ttyAMA0', 9600, timeout=1)
        self.ser.reset_input_buffer()
        self.ser.flush()

        self.sio = io.TextIOWrapper(io.BufferedRWPair(self.ser, self.ser))

    def read_sensor(self) -> list[AReading]:
        """Takes a reading form the sensor

        :return list[AReading]: List of readinds measured by the sensor. Most sensors return a list with a single item.
        """
        readings = list[AReading]()

        try:
            line = self.sio.readline()
            msg = pynmea2.parse(line)
            lat = pynmea2.dm_to_sd(msg.lat)
            lon = pynmea2.dm_to_sd(msg.lon)

            if msg.lat_dir == 'S':
                lat = lat * -1

            if msg.lon_dir == 'W':
                lon = lon * -1

            if(int(msg.num_sats) > 0):
                readings.append( AReading(AReading.Type.GPS, AReading.Unit.DEGREES_LATITUDE, lat) )
                readings.append( AReading(AReading.Type.GPS, AReading.Unit.DEGREES_LONGITUDE, lon) )
            else:
                print("Currently synced with 0 satlites and thus cannot derive latitude or longitude, please check reception")
        except UnicodeDecodeError:
            return self.read_sensor()
        except AttributeError:
            return self.read_sensor()
        except pynmea2.ParseError as e:
            return self.read_sensor()
        except serial.SerialException as e:
            print('Device error: {}'.format(e))
        
        return readings

if __name__ == "__main__":
    gpssensor = GPSSensor(9600, 'Air530', AReading.Type.GPS)

    while True:
        readings = gpssensor.read_sensor()

        for reading in readings:
            print( reading.__repr__())

        print('\n')
        time.sleep(5)


