from gpiozero import OutputDevice
from classes.actuators import IActuator, ACommand, AState
from time import sleep

class FanController(IActuator):

    def __init__(self, gpio: int, type: ACommand.Type, initial_state: str) -> None:
        self.fan_pin = OutputDevice(gpio)
        self.fan_state = initial_state
        self.type = type



    def validate_command(self, command: ACommand) -> bool:
        return command.target_type == ACommand.Type.FAN


    def control_actuator(self, data: dict) -> bool:

        onOrOff = data.get("value", None)

        if onOrOff is None:
            return False

        if onOrOff == 1:
            self.fan_pin.value = True
            return True
        
        if onOrOff == 0:
            self.fan_pin.value = False
            return False
        
        return False
    
    def get_current_state(self) -> AState:
        result = True if self.fan_pin.value == 1 else False
        return AState(AState.Type.FAN, {"value": result})

        
        
        


if __name__ == "__main__":
    fan = FanController(24, ACommand.Type.FAN, 1)
    fan.control_actuator({"value": 1})
    print(fan.get_current_state())
    sleep(2)
    fan.control_actuator({"value": 0})
    print(fan.get_current_state())