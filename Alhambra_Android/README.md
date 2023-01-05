This folder corresponds to the tablet part of the Alhambra ecosystem. The folder structure follows the generic structure gradle enforces for Android Java applications. 

# Dataset
This application relies on an initial dataset. This dataset should be at the root of this project, under the folder "SpotsInfo". To pack the dataset with the Android application, do:
- Copy all the files from "SpotsInfo" to "Alhambra_Android/app/src/main/assets/". 
- Change the file "SpotList.txt" to convert it under a csv file format:
    - Remove all comments
    - Separate columns with commas (and not spaces)
    - Put quotes around colors (e.g., "(r,g,b,a)")

    You should then have something like:
    ```
    0,-,-,-
    1,"(0,0,11,255)",2,11
    2,"(0,0,12,255)",2,12
    3,"(112,0,12,255)",0,112
    4,"(0,12,12,255)",1,12
    5,"(0,0,13,255)",2,13
    6,"(113,0,13,255)",0,113
    7,"(34,0,0,255)",0,34
    8,"(14,0,0,255)",0,14
    9,"(0,14,0,255)",1,14
    ```

    We did not use that file yet because this project is still under work, thus, the database format may change in the near future, which may create unnecessary commits.

# Configure the application
Upon installation, a file called "config.json" is created in the external Android file directory (e.g., /sdcard/Android/data/com.alhambra/files/).
This file can be edited manually after the application has started once (for this file to be created). Restart the application for the changes to take effect.

Especially, in this config.json file, you may need to change the IP address of the HoloLens to set up the connection between the two devices.

# Structure
The "MainActivity.java" class is the main file of this project. It synchronises every subcomponent of this application.

## UI elements and data model
Most components follows a MVP model. 

- The high-level components (e.g., Fragments) shared an instance of the Dataset.java that represents the overall status of the data.
- Low-level components (e.g., ColorPicker, TreeView) have their own model (e.g., ColorPickerView.java is associated with ColorPickerData.java). Those models do not represent the state of the application but rather the state of a given UI element. When designing this application, one should keep in mind that every model of those low-level components could be reconstructed (at least with default values) from the overall application status stored in the instance of Dataset.java without loosing information or being incoherent.

## Network structure
The SocketManager.java is the main manager to handle messages received from and sent to the HoloLens. Normally this file does not need given how the project is structured, as it handles just the sending and receiving of strings.

A message (binary speaking) is encoded in big endian and composed as follow:
-32 bits (unsigned short) : the size of the string
-n bits (unsigned char) : the string itself in an UTF-8 (preferably ASCII) format.

See examples in com.alhambra.network.receivingmsg and com.alhambra.network.sendingmsg to know how to write messages to send and parse received messages.

Reading message and sending messages are separated into two threads. As such, it is required to have a synchronisation process using mutex (lockable) objects. Hence the use of the "runOnUiThread" on the MainActivity.java network-related messages.