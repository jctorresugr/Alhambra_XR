This project corresponds to the tablet part of the Alhambra ecosystem.

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