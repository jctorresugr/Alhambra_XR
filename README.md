# Alhambra-XR
This application allows one to explore the Cuarto Dorado museum within an XR environment using a hybrid combinarion of a HoloLens 2nd gen. as a main device and a Android multi-touch tablet as an interative tool.

The HoloLens acts as a server application on which the Android device connects itself as a client. As such, it is recommanded to launch the HoloLens program first. Because this project is still in development, some synchronisation issues can arise when one device restarts and the other does not. We recommand then to restart both the HoloLens and the Android device if a restart is needed.

## HoloLens
The AlhambraXR subfolder contains an Unity 3D program that can be launched on a desktop environment or on a HoloLens 2nd gen. Please, read the [XR Readme](./AlhambraXR/README.md) file to better understand the structure of the program.

### Installation
1. Download and install [Unity Hub](https://unity.com/download) 
2. Open the AlhambraXR subfolder. Unity Hub shall detect which version of Unity this project relies on and propose you to install it. Do it so.
    - Install the "Universal Windows Platform Build Support" and the "Windows Build Support (IL2CPP)" modules.
3. Install [Visual Studio](https://visualstudio.microsoft.com/)
    - Check that C++ development, UWP support, an ARM64 bits C/C++ compiler, and Unity development support are installed correctly. You may also need the component "connectivity with USB devices".
4. Launch the Unity project AlhambraXR and installed it on the HoloLens as usual (see the documentation of MRTK and Microsoft for more details about the usual workflow of HoloLens' development).


## Android tablet 
The Alhambra_Android subfolder contains an Android studio program to launch on an Android device version API level 23 (Android 6) or above. Please, read the [Android Readme](./Alhambra_Android/README.md) file to better understand the structure of the program.

### Installation
1. Download and install [Android Studio](https://developer.android.com/studio).
2. Install on your Android device the program that is under the "Alhambra_Android" root subfolder.

## Communication protocol
The communication relies on TCP/IP sockets and on JSON messages. Each JSON message has the following structure:
{
    "action": "actionName",
    "data": {}
}

With "action" being the name of the action (should be unique per action to differenciate them), and "data" being the data associated to this action.

## Annotations
Annotations are represented internally as (1) a surface area using a 2D texture and (2) as a unique 3-channels color (the transparency being used as a boolean on whether there is an annotation or not at this spot). Each channel corresponds to a given annotation layer. 

The "highest-level" layer is the red component. All annotations that share the same red value are supposed to be "linked" or to share common surface areas.

The second "higher-level" layer is the green component, with all annotations that share the same green value are supposed to be linked or to share common surface areas.

Then the last layer if the blue component, with similar behavior. Newly added annotations should start with this layer (with green and red set as 0) if no surface overlaps the annotation, then with the second highest one (green) with red set as 0 if one surface overlaps it, and then with the red layer as a last resort, as a bottom-up approach.

## Interactions
Our application handles multiple interactions:

- The query of registered annotation by either "brushing" the AR environment (and using a Tap gesture to have more details on the tablet) or by using the multi-touch tablet which can also highlights, in the 3D space, a given annotation.
    - If an annotation is selected, the user need to make a Tap Gesture (or use the multi-touch tablet) to quite the Highlight status.
- The anchoring of new annotations in the 3D virtual space:
    - In the Annotation panel on the Android multi-touch tablet, start the annotation process.
    - Take a screenshot of the 3D scene on the HoloLens using a Tap gesture.
    - Draw on the multi-touch tablet a new area corresponding to the annotation and its associated text.