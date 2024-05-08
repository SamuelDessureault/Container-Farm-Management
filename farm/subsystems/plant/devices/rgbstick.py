from grove.grove_ws2813_rgb_led_strip import GroveWS2813RgbStrip
from rpi_ws281x import Color
from time import sleep
from classes.actuators import IActuator, ACommand, AState

class GroveRGBLEDStick(IActuator):
    def __init__(self, gpio: int, num_leds: int, type: ACommand.Type, initial_state: dict, model: str = "GroveRGBLEDStick"):
        self._device_model = model
        self._gpio = gpio
        self._num_leds = num_leds
        self.type = type
        self._current_state = initial_state
        self._led_strip = GroveWS2813RgbStrip(gpio, num_leds)

    def validate_command(self, command: ACommand) -> bool:
        return command.target_type == ACommand.Type.RGB
        

    def control_actuator(self, data: dict) -> bool:
        led_index = data.get("led_index")
        color = Color(*data.get("color"))

        if color != Color(0, 0, 0):
            print("Light " + str(led_index) + " On")
        else:
            print("Light " + str(led_index) + " Off")

        self._led_strip.setPixelColor(led_index, color)
        self._led_strip.show()

    def get_current_state(self) -> AState:
        led_states = {}
        for led_index in range(self._num_leds):
            color = self._led_strip.getPixelColor(led_index)
            is_on = color != Color(0, 0, 0)
            led_states[led_index] = is_on

        return AState(AState.Type.RGB, {'led_states': led_states})


if __name__ == "__main__":
    led_stick = GroveRGBLEDStick(18, 10, ACommand.Type.RGB, {"value": False})

    led_stick.control_actuator({"led_index": 1, "color": (255, 0, 0)})
    led_stick.control_actuator({"led_index": 2, "color": (0, 255, 0)})
    led_stick.control_actuator({"led_index": 3, "color": (0, 0, 255)})
    sleep(4)
    led_stick.control_actuator({"led_index": 1, "color": (0, 0, 0)})
    led_stick.control_actuator({"led_index": 2, "color": (0, 0, 0)})
    led_stick.control_actuator({"led_index": 3, "color": (0, 0, 0)})
