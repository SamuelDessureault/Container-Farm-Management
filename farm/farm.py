from connection_manager import ConnectionManager
from classes.sensors import ISensor, AReading
from classes.actuators import IActuator, ACommand, AState
from classes.subsystem import ISubSystem
from subsystems.security.security_subsystem import FarmSecuritySubsystem
from subsystems.geo_location.geolocationsubsystem import GeoLoactionSubSystem
from subsystems.plant.plant_subsystem import FarmPlantSubSystem


from subsystems.security.devices.noise_sensor import NoiseSensor
from subsystems.security.devices.luminosity_sensor import LuminositySensor
from subsystems.security.devices.motion_sensor import MotionSensor
from subsystems.security.devices.farm_door_sensor import FarmDoorSensor
from subsystems.security.devices.farm_door_lock_servo import FarmDoorLockServo
from subsystems.security.devices.security_buzzer import Buzzer


from subsystems.plant.devices.temphumi import TemperatureSensor
from subsystems.plant.devices.rgbstick import GroveRGBLEDStick
from subsystems.plant.devices.watersensor import WaterLevelSensor
from subsystems.plant.devices.soilsensor import SoilLevelSensor
from subsystems.plant.devices.fan_control import FanController

import asyncio
from azure.iot.device.aio import IoTHubDeviceClient
from azure.iot.device import MethodResponse
import seeed_python_reterminal.core as rt



class Farm:

    DEFAULT_LOOP_INTERVAL = 5
    TEMP_LIMIT:tuple = (20, 30)
    DEBUG = True
    LOOP_INTERVAL = DEFAULT_LOOP_INTERVAL # in seconds
    DESIRED_PROPERTY_TELEMETRY_INTERVAL_NAME = "telemetryInterval"
    TEMP_UPPER_LIMIT = 20
    TEMP_LOWER_LIMIT = 30
    DEFAULT_TEMP_UPPER_LIMIT = 20
    DEFAULT_TEMP_LOWER_LIMIT = 30
    TEMP_UPPER_LIMIT_NAME = "highTempThreshold"
    TEMP_LOWER_LIMIT_NAME = "lowTempThreshold"
    CURRENT_TEMP = 25

    def __init__(self) -> None:
        self.subsystems: list[ISubSystem] = [
            #Put Subsystems here
            FarmSecuritySubsystem([NoiseSensor(1, "Sound Detector", AReading.Type.NOISE), MotionSensor(16, "Grove Adjustable PIR Motion Sensor", AReading.Type.MOTION), LuminositySensor(0, "reTerminal Illuminance Sensor", AReading.Type.LUMINOSITY), FarmDoorSensor(22, "Magnetic Door Sensor reed Switch", AReading.Type.DOOR)], [FarmDoorLockServo(12, "Servo", AReading.Type.LOCK, ACommand.Type.LOCK, {"value": 0}), Buzzer(0, "reTerminal Buzzer", AReading.Type.BUZZER, ACommand.Type.BUZZER, {'value': rt.buzzer})]),
            GeoLoactionSubSystem(),
            FarmPlantSubSystem([TemperatureSensor(gpio=4, model='GroveTemperatureHumidityAHT20', type= AReading.Type.TEMPERATURE), WaterLevelSensor(0X04, "Grove Water Level Sensor", AReading.Type.WATER), SoilLevelSensor(0X04, "Grove Capacitive Soil Moisture Sensor", AReading.Type.SOIL_MOISTURE)], [GroveRGBLEDStick(18, 10, ACommand.Type.RGB, {"value": False}), FanController(24, ACommand.Type.FAN, 1)])
        ]
        self._connection_manager = ConnectionManager()

    async def read_sensors(self):
        readings:list[AReading] = list()
        for subsystem in self.subsystems:
            subsystemReadings = subsystem.read_sensors()
            for reading in subsystemReadings:
                readings.append(reading)
                if(reading.reading_type == AReading.Type.TEMPERATURE):#Is a temperature reading
                    self.CURRENT_TEMP = reading.value
        if self.DEBUG:
            print(readings)
            
        
        # Send collected readings
        await self._connection_manager.send_readings(readings)

    async def control_actuators(self, command: ACommand):
        for subsystem in self.subsystems:
            subsystem.control_actuators(command) #try to use the cmd, should fail gracefully (NO EXCEPTION THROWN) in the subsystem

    async def patch_callback(self, patch: dict):
        if (self.DEBUG):
            print(patch)
        try:
            self.LOOP_INTERVAL = int(patch[self.DESIRED_PROPERTY_TELEMETRY_INTERVAL_NAME])
            self.TEMP_LOWER_LIMIT = float(patch[self.TEMP_LOWER_LIMIT_NAME])
            self.TEMP_UPPER_LIMIT = float(patch[self.TEMP_UPPER_LIMIT_NAME])
        except:
            self.LOOP_INTERVAL = self.DEFAULT_LOOP_INTERVAL
            self.TEMP_LOWER_LIMIT = self.DEFAULT_TEMP_LOWER_LIMIT
            self.TEMP_UPPER_LIMIT = self.DEFAULT_TEMP_UPPER_LIMIT

    async def update_reported_actuator_states(self):
        #get the current state of each actuator
        states:list[AState] = list()
        for subsystem in self.subsystems:
            subsystemStates = subsystem.get_current_actuator_states()
            for state in subsystemStates:
                states.append(state)

        print(states)
        for state in states:
            reported_properties = {state.sender_type: state.state}
            print(reported_properties)
            await self._connection_manager.update_reported_properties(reported_properties)
            
    async def method_request_handler(self, method_request):
        # Determine how to respond to the method request based on the method name
        if method_request.name == "is_online":
            payload = {}  # set response payload
            status = 200  # set return status code
            print("executed " + method_request.name)
        elif method_request.name == "controlactuators":
            try:
                payload:dict =  method_request.payload # should not need to be parsed
                target_type:ACommand.Type = payload["target"]
                print(method_request.payload)
                command = ACommand(target_type, payload)
                await self.control_actuators(command)
                await self.update_reported_actuator_states()
                
                payload = {"details" : "Success: Actuator Controlled Successfully"}
                status = 200 #Everything is good
            except:
                payload = {"details" : "FAILED: Internal Error"}
                status = 500 #internal server error
        else:
            payload = {"details": "method name unknown"}  # set response payload
            status = 400  # set return status code
            print("executed unknown method: " + method_request.name)

        # Send the response
        method_response = MethodResponse.create_from_method_request(method_request, status, payload)
        await self._connection_manager._client.send_method_response(method_response)


    async def check_threshold(self):
        if self.CURRENT_TEMP >= self.TEMP_UPPER_LIMIT:
            #TOO HOT
            command = ACommand(ACommand.Type.FAN, {"value": True})
            await self.control_actuators(command)
        elif self.CURRENT_TEMP <= self.TEMP_LOWER_LIMIT:
            #too cold
            command = ACommand(ACommand.Type.FAN, {"value": False})
            await self.control_actuators(command)

    async def loop(self) -> None:
        """Main loop of the HVAC System. Collect new readings, send them to connection
        manager, collect new commands and dispatch them to device manager.
        """
        await self._connection_manager.connect()
        self._connection_manager.register_command_callback(
            self.control_actuators)
        self._connection_manager.register_method_callback(
            self.method_request_handler)
        self._connection_manager.set_receive_twin_desired_properties_patch_callback(
            self.patch_callback)
        
        # Get the desired property on start.
        twin = await self._connection_manager._client.get_twin()
        if (self.DEBUG):
            print(twin)
        self.LOOP_INTERVAL = twin['desired'][self.DESIRED_PROPERTY_TELEMETRY_INTERVAL_NAME]

        try:
            self.TEMP_UPPER_LIMIT = twin['desired'][self.TEMP_UPPER_LIMIT_NAME] #set thresholds on startup
            self.TEMP_LOWER_LIMIT = twin['desired'][self.TEMP_LOWER_LIMIT_NAME]
        except:
            self.TEMP_UPPER_LIMIT = self.DEFAULT_TEMP_UPPER_LIMIT #default to these values
            self.TEMP_LOWER_LIMIT = self.DEFAULT_TEMP_LOWER_LIMIT

        while True:
            # Collect new readings
            await self.read_sensors()
            await self.check_threshold()

            #await self.control_actuators(ACommand(ACommand.Type.RGB, '{"led_index": 1, "color": [0, 0, 0]}'))

            await asyncio.sleep(self.LOOP_INTERVAL)

    


"""This script is intented to be used as a module, however, code below can be used for testing.
"""


async def farm_main():
    farm = Farm()
    await farm.loop()

if __name__ == "__main__":
    asyncio.run(farm_main())