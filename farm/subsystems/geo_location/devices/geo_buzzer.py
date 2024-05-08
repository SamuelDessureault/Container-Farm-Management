import seeed_python_reterminal.core as rt
import time
from classes.actuators import IActuator, ACommand, AState
import json

class Buzzer(IActuator):

    # Properties to be initialized in constructor of implementation classes
    _current_state: dict
    type: ACommand.Type

    def __init__(self, gpio: int, type: ACommand.Type, initial_state: dict) -> None:
        #Buzzer is internal and thus gpio is redundant
        self.type = type
        self._current_state = initial_state
        
        firstCommand:ACommand = ACommand(ACommand.Type.BUZZER, initial_state )
        if(self.validate_command(firstCommand)):
            self.control_actuator(firstCommand.data)
        else:
            self._current_state['value'] = False

    def validate_command(self, command: ACommand) -> bool:
        return command.target_type == self.type and isinstance(command.data.get('value'), bool)

    def control_actuator(self, data: dict) -> bool:
        if isinstance(data["value"], bool):
            desiredState = data["value"]
            #rt.buzzer = desiredState
            self._current_state['value'] = desiredState

    def get_current_state(self) -> AState:
        """Returns the current state of the actuator
        :return AState: Returns the current state of the actuator.
        """
        return AState(self.type, self._current_state['value']. __repr__())
    
if __name__ == "__main__":
    initialState = dict()
    initialState['value'] = True
    buzzer = Buzzer(0, ACommand.Type.BUZZER, initialState)

    print(buzzer.get_current_state())

    time.sleep(1)

    initialState['value'] = False
    buzzer.control_actuator(initialState)

    print(buzzer.get_current_state())
