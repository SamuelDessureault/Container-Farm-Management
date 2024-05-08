# Container Farm Management

## Team Information

- Team Name: STS Technologies
- Team Letter: L
- Team Members:
- - Samuel Dessureault, ID: 2044199
- - Samuel Augunas, ID: 2031185
- - Tyler Johnson, ID: 2076242

## Project Description

This project involves the development of an IoT solution for monitoring and managing container farms, with a primary focus on creating a highly efficient, fully automated, and remotely controllable farming system.

## Project Hardware Overview

### Plant Subsystem

- **Water Level Sensor**: 
    - Type: Analog
    - Library: ADC library for Grove Base Hat

- **Soil Moisture Sensor**: 
    - Type: Analog
    - Library: ADC library for Grove Base Hat

- **RGB LED Stick**: 
    - Type: PWM
    - Library: grove_ws2813_rgb_led_strip.py module from Grove.py package

- **Cooling Fan**: 
    - Type: Digital
    - Controlled via: Relay module

- **AHT20 Temp & Humidity Sensor**: 
    - Type: I2C
    - Library: Grove_temperature_humidity_aht20.py from Grove

### Geo-location Subsystem

- **GPS (Air530)**: 
    - Type: Serial
    - Libraries: pyserial (to read serial data), pynmea2 (to parse gps messages)

- **USB Power Bank**

- **reTerminal’s built-in accelerometer**: 
    - Type: I2C
    - Library: acceleration.py in Seeed_Python_ReTerminal

- **reTerminal’s built-in buzzer**: 
    - Type: I2C (I/O expander)
    - Library: core.py in Seeed_Python_ReTerminal

### Security Subsystem

- **PIR Motion Sensor**: 
    - Type: Digital
    - Library: grove_mini_pir_motion_sensor.py can be used

- **Magnetic door sensor reed switch**: 
    - Type: Digital
    - Controlled via: Button class from gpiozero

- **MG90S 180° Micro Servo**: 
    - Type: PWM
    - Library: Servo class from gpiozero

- **Sound Sensor/ Noise Detector**: 
    - Type: Analog
    - Library: ADC library for Grove Base Hat




## Mobile App

### App Overview

Our container farming mobile app makes farming easier and more efficient. It gives users a strong handle on their farming work, letting them see and control what's happening in their farms from anywhere. Here’s what our app can do:

- Control containers remotely: We are able to control the containers from anywhere and we can see what’s happening with the containers
- Show historical data with graphs: We can see the historical data of the sensors with graphs so we know if there is a problem
- Different access for different users: There are two types of users, technician and owner. They each have different views for their needs.

### App Setup

IMPORTANT: Due to a bug that prevents the app to create an IoT Hub device, please keep the Firebase strings in ResourceStrings intact and manually create a device in your IoT Hub called "simDevice"

Firstly, navigate to the azure portal and go to your IoT Hub. 

Then, click devices on the left side and click on your device. Copy the device id and the primary device connection string. For the last string, go back and go to shared access policies and go to iothubowner and copy the primary connection string. After you put these 3 strings in your .env file you can start running farm.py.

In the app you need multiple connection strings. First go to your storage and then go to access keys and copy that connection string. Then go to your blob container and copy the name. Then go to your IoT hub and go to built-in endpoints and copy the rest of the strings and put these strings in appsettings.json. For the login we used Firebase Authentication. The user is able to create either a technician or owner account then login with that account. For this go to the ResourceStrings in the config file and change the strings to match your firebase strings. 

After this is done, you can use these test accounts to sign in to the app. For the owner account use the email: tester@gmail.com, Password: password. For the technician account use the email: test@test.test, Password: test123.

## App Functionality

### Login

By using Firebase Authentication, the user can create a new account or sign into an existing account by writing an email and password. Upon creating a new account,  the user will be asked to choose which type of account they want to create (OWNER and TECHNICIAN).

### Container List

When an existing user logs in, they will be shown a scrollable list of all container farms they have created or have been assigned to. Owner accounts can create container farms and add them to the Firebase Database as well as edit their label. Technician accounts can only assign themselves to an existing container farm but can edit temperature thresholds which will determine when to automatically activate or deactivate the fan, the crop name and the telemetry interval.

### Owner Views

Upon tapping on a container farm, the owner will be able to view live geolocation data of this said container, such as its GPS location shown on a map, its pitch and roll and its level of vibration, which will be updated by a set telemetry interval. They will also have access to controls for the container’s alarm system and lock.

The owner will also have access to historical security data through charts, showing the noise level, luminosity level, motion and door state.

### Technician Views

Upon tapping on a container farm, the technician will be able to view live and historical plant data of this said container, such as the temperature, humidity, water level and soil moisture level. They will also have access to manual controls for the fan, light and lock.

### Features

| Features | Owner | Technician |
|---|:---:|:---:|
|Account Creation and Login|X|X|
|Create New Containers|X||
|Assign to Existing Containers||X|
|Edit Container|X|X|
|Delete Container|X||
|Unassign from Container||X|
|View Live and Historical Data of Plant Subsystem||X|
|View Live and Historical Data of Geolocation Subsystem|X||
|View Live and Historical Data of Security Subsystem|X||
|Manually Control Fan||X|
|Manually Control Light||X|
|Manually Control Alarm|X||
|Manually Control Lock|X|X|

### Plant Subsystem

- Temperature Readings
- Humidity Readings
- Water Level Readings
- Soil Moisture Readings
- Fan Control
- Light Control

### Geolocation Subsystem

- GPS Readings (Latitude and Longitude)
- Pitch-and-roll Readings
- Vibration Readings

### Security Subsystem

- Noise Readings
- Motion Readings
- Door Sensor Readings
- Luminosity Readings
- Buzzer Control
- Lock Control

### App Screenshots

![Login](./app%20documentation%20images/signin.png)
![SignUp](./app%20documentation%20images/signup.png)

The login and sign up page.

![containerList](./app%20documentation%20images/containerselect.png)
![containerEdit](./app%20documentation%20images/editcontainer.png)
![containerDelete](./app%20documentation%20images/deletecontainer.png)

Sliding a container to the right will show an edit button, leading to the container edit page. Sliding a container to the left will delete that container. Tapping the first icon on the top right will lead the user to the add container page while tapping the second icon will allow the user to sign out of their account.

![ownerGeo](./app%20documentation%20images/ownergeo.png)
![ownerHistory](./app%20documentation%20images/ownerhistory.png)
![ownerSelectHistory](./app%20documentation%20images/ownerselecthistory.png)

Those are the owner pages, allowing the user to view live data and control devices from the geolocation and security subsystems. The user can also choose the type of data to view the historical data of.

![ownerAdd](./app%20documentation%20images/owneradd.png)
![ownerEdit](./app%20documentation%20images/owneredit.png)

The owner can create a new container by creating a label and a device ID, which will create a device in the Azure IoT Hub.

![techView](./app%20documentation%20images/techview.png)
![techHistory](./app%20documentation%20images/techhistory.png)
![techSelectHistory](./app%20documentation%20images/techselecthistory.png)

Those are the technician pages, allowing the user to view live data and control devices from the plant subsystem. The user can also choose the type of data to view the historical data of.

![techAdd](./app%20documentation%20images/techadd.png)
![techEdit](./app%20documentation%20images/techedit.png)

The technician can assign themselves to an existing container by choosing the device ID of all devices unassigned to them from the IoT Hub. They can also edit the low and high temperature thresholds and the telemetry interval, which will be added to the desired device twin properties.

## Future Work

- There currently exists a bug with the owner registering a new container where, while they are able to create the object and add it to the database, it is not able to be registered on the IOT Hub.

- Add growing period functionality as discussed in milestone 1. To allow both owner and technician to monitor and predict the growth in the farm.

- Highlight container once growing period has passed. Giving an indication that whatever crop contained in the container is due to be harvested.

- Add functionality to Pi LED's to turn on, indicating that the container’s temperature is outside the set parameters.

- Notifications for owners, to provide a warning when the app detects rapid or large changes in pitch, roll, etc or even when the door is opened whilst the door is still locked. This is to alert the owner to situations that might require his attention.

- Notifications for technicians, provides a warning when plant telemetry goes outside the bounds that were acceptable for that plant, alerting the technician to any situation that could hinder the growth of, or even kill the crop inside.

- For the owner, create a map, to show all containers, as opposed to the map showing an individual container.

- Create a graphic to display the pitch and roll of a container.

## Contributions

| Name              | Role        | Responsibilities                                          |
|-------------------|-------------|-----------------------------------------------------------|
| Samuel Dessureault| Python Farm | Security subsystem and devices, Device twin communication, Controlling actuators |
|                   | Mobile App  | Technician views front-end and back-end, Subsystem data binding, Historical data chart functionality |
| Samuel Augunas    | Python Farm | Plant subsystem and devices, Controlling actuators, Sending readings of sensors |
|                   | Mobile App  | Owner views front and back-end, Historical data for owner, Map with pin of location of container |
| Tyler Johnson     | Python Farm | Geolocation subsystem and devices (Milestone 2), Merged Subsystems (Milestone 4), Connection manager and farm (Milestone 4), Direct Methods and Twin Properties (Milestone 4), Controlling actuators (Milestone 6) |
|                   | Mobile App  | Model and Database creation (Milestone 3), Authentication Back-end (Milestone 3), Managing Azure Connection (Milestone 5), Reading azure messages (Milestone 5), Invoking Direct Methods (Milestone 5), Updating Desired Device Twin Properties (Milestone 5) |

## D2C Messages

For our app, we used a simple format, to send a single reading per message. Which sends the value of the reading, the unit of the measurement, the type of sensor that read this information, when this information was read and finally the ID of the device that send the reading. Which are sent in the format below.

~~~
az iot device send-d2c-message -n {Iot Hub Name} -d {Device Name} --data {\ \'value\':{Reading Value},\ \'unit\':\"{Reading Unit}\",\ \'type\':\"{Reading Type}\",\ \'timestamp\':\"{Reading Timestamp}\",\ \'deviceId\':\"{Device Name}\"\}
~~~

To elaborate a bit further, I’ll explain what each placeholder, which is underlined, is meant to be. Starting of the Iot Hub Name, is fairly self-explanatory and is meant to contain the name of the Iot Hub, likewise the Device Name is meant to contain the name of the device being tested. On to Reading Value, it can be any number that is capable of being parsed as a float. Both Reading Unit and Reading Type refer to enums and as such have specific and pre-defined values detailed below. 
Unit Values:
"%", "mL", "noise", "vibration", "roll", "pitch", "degrees-longitude", "degrees-latitude", "% HR", "C", ""
Type Values:
"temperature", "humidity", "gps", "pitch-roll", "vibration", "noise", "motion", "water", "luminosity", "soil-moisture", "door"

Below is an example with all placeholders filled:

~~~
az iot device send-d2c-message -n ConnectedObjIotHub -d simDevice --data {\ \'value\':0.5423634667117333,\ \'unit\':\"roll\",\ \'type\':\"pitch-roll\",\ \'timestamp\':\"2023-05-21T13:06:52.147307\",\ \'deviceId\':\"simDevice\"\}
~~~

Here is an example of a full payload
~~~
az iot device send-d2c-message -n ConnectedObjIotHub -d simDevice --data {\ \'value\':18,\ \'unit\':\"C\",\ \'type\':\"temperature\"\ \'timestamp\':\"2023-05-21T13:06:52.147307\",\ \'deviceId\':\"simDevice\"\}

az iot device send-d2c-message -n ConnectedObjIotHub -d simDevice --data {\ \'value\':3,\ \'unit\':\"% HR\",\ \'type\':\"humidity\"\ \'timestamp\':\"2023-05-21T13:06:52.147307\",\ \'deviceId\':\"simDevice\"\}

az iot device send-d2c-message -n ConnectedObjIotHub -d simDevice --data {\ \'value\':0.5423634667117333,\ \'unit\':\"roll\",\ \'type\':\"pitch-roll\"\ \'timestamp\':\"2023-05-21T13:06:52.147307\",\ \'deviceId\':\"simDevice\"\}

az iot device send-d2c-message -n ConnectedObjIotHub -d simDevice --data {\ \'value\':0.5423634667117333,\ \'unit\':\"pitch\",\ \'type\':\"pitch-roll\"\ \'timestamp\':\"2023-05-21T13:06:52.147307\",\ \'deviceId\':\"simDevice\"\}

az iot device send-d2c-message -n ConnectedObjIotHub -d simDevice --data {\ \'value\':10,\ \'unit\':\"mL\",\ \'type\':\"water\",\ \'timestamp\':\"2023-05-21T13:06:52.147307\",\ \'deviceId\':\"simDevice\"\}

az iot device send-d2c-message -n ConnectedObjIotHub -d simDevice --data {\ \'value\':12,\ \'unit\':\"%\",\ \'type\':\"soil-moisture\",\ \'timestamp\':\"2023-05-21T13:06:52.147307\",\ \'deviceId\':\"simDevice\"\}

az iot device send-d2c-message -n ConnectedObjIotHub -d simDevice --data {\ \'value\':2.5423634667117333,\ \'unit\':\"degrees-longitude\",\ \'type\':\"gps\"\ \'timestamp\':\"2023-05-21T13:06:52.147307\",\ \'deviceId\':\"simDevice\"\}

az iot device send-d2c-message -n ConnectedObjIotHub -d simDevice --data {\ \'value\':4.5423634667117333,\ \'unit\':\"degrees-latitude\",\ \'type\':\"gps\"\ \'timestamp\':\"2023-05-21T13:06:52.147307\",\ \'deviceId\':\"simDevice\"\}

az iot device send-d2c-message -n ConnectedObjIotHub -d simDevice --data {\ \'value\':4,\ \'unit\':\"vibration\",\ \'type\':\"vibration\",\ \'timestamp\':\"2023-05-21T13:06:52.147307\",\ \'deviceId\':\"simDevice\"\}

az iot device send-d2c-message -n ConnectedObjIotHub -d simDevice --data {\ \'value\':880,\ \'unit\':\"noise\",\ \'type\':\"noise\",\ \'timestamp\':\"2023-05-21T13:06:52.147307\",\ \'deviceId\':\"simDevice\"\}

az iot device send-d2c-message -n ConnectedObjIotHub -d simDevice --data {\ \'value\':4,\ \'unit\':\"\",\ \'type\':\"luminosity\",\ \'timestamp\':\"2023-05-21T13:06:52.147307\",\ \'deviceId\':\"simDevice\"\}

az iot device send-d2c-message -n ConnectedObjIotHub -d simDevice --data {\ \'value\':0,\ \'unit\':\"\",\ \'type\':\"motion\",\ \'timestamp\':\"2023-05-21T13:06:52.147307\",\ \'deviceId\':\"simDevice\"\}

az iot device send-d2c-message -n ConnectedObjIotHub -d simDevice --data {\ \'value\':0,\ \'unit\':\"\",\ \'type\':\"door\",\ \'timestamp\':\"2023-05-21T13:06:52.147307\",\ deviceId:\"simDevice\"\}
~~~

## Instructions

IMPORTANT: Due to a bug that prevents the app to create an IoT Hub device, please keep the Firebase strings in ResourceStrings intact and manually create a device in your IoT Hub called "simDevice"

Firstly, navigate to the azure portal and go to your IoT Hub. 

Then, click devices on the left side and click on your device. Copy the device id and the primary device connection string. For the last string, go back and go to shared access policies and go to iothubowner and copy the primary connection string. After you put these 3 strings in your .env file you can start running farm.py.

In the app you need multiple connection strings. First go to your storage and then go to access keys and copy that connection string. Then go to your blob container and copy the name. Then go to your IoT hub and go to built-in endpoints and copy the rest of the strings and put these strings in appsettings.json. For the login we used Firebase Authentication. The user is able to create either a technician or owner account then login with that account. For this go to the ResourceStrings in the config file and change the strings to match your firebase strings. 

After this is done, you can use these test accounts to sign in to the app. For the owner account use the email: tester@gmail.com, Password: password. For the technician account use the email: test@test.test, Password: test123.

As for the python farm, refer to the chart below for device ports and library installation.

| Actuators      | Grove Base Hat Port |
| -------------- | ------------------- |
| RGB LED Stick  | Port D18            |
| Fan            | Port D24            |
| Buzzer         | Built-in            |
| MG90S 180° Micro Servo | PWM Port    |



| Sensors        | Grove Base Hat Port |
| -------------- | ------------------- |
| Water Level Sensor | Port A4         |
| Soil Moisture Sensor | Port A2       |
| AHT20 Temp & Humidity Sensor | Port D26 |
| Magnetic Door Sensor Reed Switch | Port D22 |
| Luminosity Sensor  | Built-in        |
| PIR Motion Sensor  | Port D16        |
| Sound Sensor/ Noise Detector | Port A0 |
| Accelerometer      | Built-in        |
| GPS (Air530)       | UART Port       |

* Must be root user to access buzzer and RBG LED stick
*  To ensure the Grove.py library is properly installed, please execute the following command: **sudo pip3 install grove.py** This step is necessary because the library is not included in the requirements.txt file.
* install these packages as sudo **sudo pip install azure-iot-device** and **sudo pip install python-dotenv**

Switch to the root user on your device with the command below and run the farm.py file:
~~~
sudo -s
~~~
