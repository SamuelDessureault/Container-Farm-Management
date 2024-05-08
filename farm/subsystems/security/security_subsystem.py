from classes.sensors import ISensor, AReading
from classes.actuators import IActuator, ACommand, AState
from .devices.security_buzzer import Buzzer
from .devices.noise_sensor import NoiseSensor
from .devices.motion_sensor import MotionSensor
from .devices.farm_door_lock_servo import FarmDoorLockServo
from .devices.farm_door_sensor import FarmDoorSensor
from .devices.luminosity_sensor import LuminositySensor
import time
from classes.subsystem import ISubSystem

class FarmSecuritySubsystem(ISubSystem):
    """Class for the container farm's security subsystem.

    IMPORTANT: If the buzzer sensor is added in the sensor list, must run this class with the root user.
    """

    def __init__(self, sensors: list[ISensor], actuators: list[IActuator]):
        self._sensor_list = sensors
        self._actuator_list = actuators

    def read_sensors(self) -> list[AReading]:
        readings: list[AReading] = []

        for sensor in self._sensor_list:
            readings.extend(sensor.read_sensor())

        return readings
        
    def control_actuators(self, command: ACommand) -> None:
        actuator_list: list[IActuator] = [ i for i in self._actuator_list if i.type == command.target_type]
        for actuator in actuator_list:
            if actuator.validate_command(command):
                actuator.control_actuator(command.data)   

    def get_current_actuator_states(self) -> list[AState]:
        state_list: list[AState] = []
        
        for actuator in self._actuator_list:
            print("step 1")
            state_list.append(actuator.get_current_state())
            print("step 2")

        return state_list

# CODE BELOW USED FOR TESTING
if __name__ == "__main__":
    SLEEP_TIME = 1

    buzzer: Buzzer = Buzzer(0, "reTerminal Buzzer", AReading.Type.BUZZER, ACommand.Type.BUZZER, {"value": 0})
    door_lock: FarmDoorLockServo = FarmDoorLockServo(12, "Servo", AReading.Type.LOCK, ACommand.Type.LOCK, {"value": 0})

    sensors: list[ISensor] = [
        buzzer,
        door_lock,
        MotionSensor(16, "PIR Motion Sensor", AReading.Type.MOTION),
        LuminositySensor(0, "reTerminal Illuminance Sensor", AReading.Type.LUMINOSITY),
        NoiseSensor(1, "Sound Detector", AReading.Type.NOISE),
        FarmDoorSensor(22, "Magnetic Dorr Sensor reed Switch", AReading.Type.DOOR)
    ]
    actuators: list[IActuator] = [
        buzzer,
        door_lock
    ]

    system: FarmSecuritySubsystem = FarmSecuritySubsystem(sensors, actuators)
    while True:
        # Read all sensors and collect all readings
        readings = system.read_sensors()
        print(readings)

        fake_servo_message_body = '{"value": 1}'
        fake_buzzer_message_body = '{"value": 1}'
        system.control_actuators(ACommand(ACommand.Type.BUZZER, fake_buzzer_message_body))
        system.control_actuators(ACommand(ACommand.Type.LOCK, fake_servo_message_body))
        time.sleep(SLEEP_TIME)

        fake_servo_message_body = '{"value": 0}'
        fake_buzzer_message_body = '{"value": 0}'
        system.control_actuators(ACommand(ACommand.Type.BUZZER, fake_buzzer_message_body))
        system.control_actuators(ACommand(ACommand.Type.LOCK, fake_servo_message_body))
        time.sleep(SLEEP_TIME)
