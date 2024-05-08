from classes.sensors import ISensor, AReading
from classes.actuators import IActuator, ACommand, AState
from gpiozero import Servo
import time
import json

class FarmDoorLockServo(ISensor, IActuator):
    """Class for the farm door lock servo device. Is a mix of a sensor and of an actuator.
    """

    _device: Servo

    def __init__(self, gpio: int, model: str, reading_type: AReading.Type, command_type: ACommand.Type, initial_state: dict):
        self._device = Servo(gpio, min_pulse_width=0.5/1000, max_pulse_width=2.5/1000)
        self._sensor_model = model
        self.reading_type = reading_type
        self.type = command_type
        self._current_state = initial_state
    
    def read_sensor(self) -> list[AReading]:
        readings = [AReading(self.reading_type, AReading.Unit.UNITLESS, self._device.value)]
        return readings
    
    def validate_command(self, command: ACommand) -> bool:
        # Check if the command data value is 0 or 1
        value: float
        try:
            print(type(command.data))
            value = command.data.get("value")
        except:
            print("ERROR: The command value is the wrong type. Make sure the value is '0' or '1'.")
            return False
        
        if (value == 0 or value == 1):
            return True
        else:
            print("ERROR: The command value is not '0' or '1'.")
            return False
            
    
    def control_actuator(self, data: dict) -> bool:
        value: float = float(data.get("value"))
        if (value == 0):
            self._device.mid()
            self._current_state = {"value": False}
        elif (value == 1):
            self._device.max()
            self._current_state = {"value": True}
        else:
            return False
        return True
    
    def get_current_state(self) -> AState:
        return AState(AState.Type.SERVO, { "value": self._current_state["value"]})

if __name__ == "__main__":
    SLEEP_TIME = 5
    servo: FarmDoorLockServo = FarmDoorLockServo(12, "Servo", AReading.Type.LOCK, ACommand.Type.LOCK, {"value": 0})
    value: int = 0
    while True:
        value = (value + 1) % 2
        print(value)
        cmd: ACommand
        if (value == 0):
            cmd = ACommand(servo.type, json.loads('{"value": 0}'))
        else:
            cmd = ACommand(servo.type, json.loads('{"value": 1}'))
            
        if (servo.validate_command(cmd)):
            servo.control_actuator(cmd.data)
        print(servo.read_sensor()[0])
        time.sleep(SLEEP_TIME)