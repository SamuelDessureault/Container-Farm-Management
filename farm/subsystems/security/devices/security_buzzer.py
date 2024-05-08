from classes.sensors import ISensor, AReading
from classes.actuators import IActuator, ACommand, AState
import seeed_python_reterminal.core as rt
import time    
    
class Buzzer(ISensor, IActuator):
    """Class for the buzzer device. Is a mix of a sensor and of an actuator.

    IMPORTANT: Must use this device as the root user.
    """

    def __init__(self, gpio: int, model: str, reading_type: AReading.Type, command_type: ACommand.Type, initial_state: dict) -> None:
        self._sensor_model = model
        self.reading_type = reading_type
        self.type = command_type
        self._current_state = initial_state

    def read_sensor(self) -> list[AReading]:
        value: float = 0
        if (rt.buzzer == True):
            value = 1
        readings = [AReading(self.reading_type, AReading.Unit.UNITLESS, value)]
        return readings

    def validate_command(self, command: ACommand) -> bool:
        try:
            value: int = int(command.data["value"])
            if (value == 0 or value == 1):
                return True
            else:
                print("ERROR: Command value is not '0' or '1'.")
                return False
        except:
            print("ERROR: Command could not be validated.")
            return False

    def control_actuator(self, data: dict) -> bool:
        value: int = int(data["value"])
        if (value == 0):
            rt.buzzer = False
        elif (value == 1):
            rt.buzzer = True
        self._current_state = {'value': rt.buzzer}

    def get_current_state(self) -> AState:
        return AState(AState.Type.BUZZER, { "value": self._current_state["value"]})

if __name__ == "__main__":
    SLEEP_TIME = 1
    buzzer: Buzzer = Buzzer(0, "reTerminal Buzzer", AReading.Type.BUZZER, ACommand.Type.BUZZER, {'value': rt.buzzer})
    buzzer_state: int = 0
    cmd: ACommand
    while True:
        print(buzzer.read_sensor()[0])
        buzzer_state = (buzzer_state + 1) % 2
        fake_buzzer_message_body: str 
        if (buzzer_state == 0):
            fake_buzzer_message_body = '{"value": 0}'
        elif (buzzer_state == 1):
            fake_buzzer_message_body = '{"value": 1}'
        print(fake_buzzer_message_body)
        cmd = ACommand(ACommand.Type.BUZZER, fake_buzzer_message_body)
        if (buzzer.validate_command(cmd)):
            buzzer.control_actuator(cmd.data)
        time.sleep(SLEEP_TIME)