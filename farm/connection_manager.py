import asyncio
from typing import Callable

from classes.sensors import AReading
from classes.actuators import ACommand

from azure.iot.device.aio import IoTHubDeviceClient
from azure.iot.device import Message

from dotenv import load_dotenv
import os
import json
import datetime

class ConnectionConfig:
    """Represents all information required to successfully connect client to cloud gateway.
    """

    # Key names for configuration values inside .env file. See .env.example
    # Constants included as static class property
    DEVICE_CONN_STR = "IOTHUB_DEVICE_CONNECTION_STRING"
    DEVICE_ID = "IOTHUB_DEVICE_ID"

    def __init__(self, device_str: str) -> None:
        self._device_connection_str = device_str


class ConnectionManager:
    """Component of HVAC system responsible for communicating with cloud gateway.
    Includes registering command and reading endpoints and sending and receiving data.

    """

    def __init__(self) -> None:
        """Constructor for ConnectionManager and initializes an internal cloud gateway client.
        """
        self._connected = False
        self._config: ConnectionConfig = self._load_connection_config()
        self._client = IoTHubDeviceClient.create_from_connection_string(
            self._config._device_connection_str)

    def _load_connection_config(self) -> ConnectionConfig:
        """Loads connection credentials from .env file in the project's top-level directory.

        :return ConnectionConfig: object with configuration information loaded from .env file.
        """
        if load_dotenv():
            connectionStr = os.environ[ConnectionConfig.DEVICE_CONN_STR]
            deviceId = os.environ[ConnectionConfig.DEVICE_ID]
            config = ConnectionConfig(connectionStr)
            config.deviceId = deviceId
            return config
        raise FileNotFoundError()

    def _on_message_received(self, message: Message) -> None:
        """Callback for handling new messages received from cloud gateway. Once the message is
        received and processed, it dispatches an ACommand to DeviceManager using _command_callback()

        :param Message message: Incoming cloud gateway message. Messages with actuator commands
        must contain a custom property of "command-type" and a json encoded string as the body.
        """
        try:
            self._command_callback(ACommand(message.custom_properties['command-type'], message.data ))#exception will be thrown if any of this is invalid
        except:
            print("Invalid message")

    def set_receive_twin_desired_properties_patch_callback(self, callback: Callable[[dict], None]):
        self._client.on_twin_desired_properties_patch_received = callback

    async def receive_twin_desired_properties_patch(self) -> dict:
        """Returns the latest update on the device twin's desired properties.

        :returns: Twin desired properties patch as a JSON dict.
        """
        return await self._client.receive_twin_desired_properties_patch()

    async def connect(self) -> None:
        """Connects to cloud gateway using connection credentials and setups up a message handler
        """
        await self._client.connect()
        self._connected = True
        print("Connected")
        # Setup the callback handler for on_message_received of the IoTHubDeviceClient instance.
        self._client.on_message_received = self._on_message_received

    def register_command_callback(self, command_callback: Callable[[ACommand], None]) -> None:
        """Registers an external callback function to handle newly received commands.

        :param Callable[[ACommand], None] command_callback: function to be called whenever a new command is received.
        """
        self._command_callback = command_callback

    def register_method_callback(self, method_callback: Callable):
        # Set the method request handler on the client
        self._client.on_method_request_received = method_callback

    async def update_reported_properties(self, reported_properties):
        await self._client.patch_twin_reported_properties(reported_properties)

    async def send_readings(self, readings: list[AReading]) -> None:
        """Send a list of sensor readings as messages to the cloud gateway.

        :param list[AReading] readings: List of readings to be sent.
        """
        for reading in readings:
            now = datetime.datetime.now()
            readingData:str = json.dumps({
                "value": reading.value,
                "unit": reading.reading_unit.value,
                "type": reading.reading_type.value,
                "timestamp": now.isoformat(),
                "deviceId": self._config.deviceId
            })
            message:Message = Message(data=readingData)
            
            await self._client.send_message(message)


"""This script is intented to be used as a module, however, code below can be used for testing.
"""


async def main_demo():

    def dummy_callback(command: ACommand):
        print(command)

    connection_manager = ConnectionManager()
    connection_manager.register_command_callback(dummy_callback)
    await connection_manager.connect()

    TEST_SLEEP_TIME = 3

    while True:

        # ===== Create a list of fake readings =====
        fake_temperature_reading = AReading(
            AReading.Type.TEMPERATURE, AReading.Unit.CELCIUS, 12.34)
        fake_humidity_reading = AReading(
            AReading.Type.HUMIDITY, AReading.Unit.HUMIDITY, 56.78)

        # ===== Send fake readings =====
        await connection_manager.send_readings([
            fake_temperature_reading,
            fake_humidity_reading
        ])

        await asyncio.sleep(TEST_SLEEP_TIME)

if __name__ == "__main__":
    asyncio.run(main_demo())