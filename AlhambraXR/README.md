This folder corresponds to the HoloLens part of the Alhambra ecosystem. This component relies on Unity 3D.

# Dependencies
This HoloLens application relies on MRTK 2.8 and OpenXR for the Augmented Reality experience. 

# Scene
The main (and for the moment only) Scene is Assets/Scenes/SampleScene. The main Game Object of this Scene is "Main", with its associated script Scrips/Main.cs being the starting point of the application.

## Cameras
Two cameras are set for this application.

- The first one "MixedRealityPlayspace -> MainCamera" is the main camera and the one that renders the final (stereoscopic) results to the user.
- The second one "RTCamera" serves as a proxy for shading computing, mostly for anchoring new annotations.

# Application model
This application uses a MVP structure with "Model.cs" being the data application status. The interaction follows a state machine model.

# Network
The main network manager is Network/AlhambraServer.cs that should be quite stable. It handles the receiving and sending parts of string messages (ultimately JSON messages). Reading message and sending messages are separated into two threads. As such, it is required to have a synchronisation process using mutex (lockable) objects.
That thread separation justifies why the Main::Update method locks the Main object, and why all network functionalities at one point locks the Main object to set its status.

Heavy tasks that need to be ran in the main thread should use the "m_tasksInMainThread" object to Enqueue new tasks.