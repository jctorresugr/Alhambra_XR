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

# Build

## Step 1
First check `Edit` > `Project Settings` > `Player` > `Universal Windows Platform Setting` tab > `Publishing Settings` > `Certificate`
Currently, the project use `debugCertificate.pfx` (the old one was expired)
The password of the certificate is `debug`.
If the certificate is expired, please add a new one, by Unity or OpenSSL.
Just ensure there is a `pfx` file. 

For building process: (li2cpp UWP ARM program)
`File` > `Build Settings` > `Universal Windows Platform` in Platform tab > 
- Target Device: `Hololens`
- Architecture: `ARM64` or `ARM`
- Build Type: `D3d Project`
- Build and run on: `Remote Device`
- for SDK version, make sure it is not lower than 10.0.xxx version

Then, click `build`, put all generated file in `AlhambraXR/Build/` folder

## Step 2
After that, navigate to `AlhambraXR/Build/AlhambraXR/AlhambraXR.vcxproj`
Open this file with notepad of Visual Code
Find code :
```xml
    <PackageCertificateKeyFile>debugCertificate.pfx</PackageCertificateKeyFile>
```
change to
```xml
    <PackageCertificateKeyFile>debugCertificate.pfx</PackageCertificateKeyFile>
    <PackageCertificatePassword>debug</PackageCertificatePassword>
```
Save the file

## Step 3
Open `AlhambraXR.sln` with Visual Studio 2019 (other version are not tested)
Build Config: switch to `Release` and `ARM` or `ARM64`

Right Click `AlhambraXR (Universal Windows)` > `Property` > `Debug` 
Select Config `Release` and `ARM` or `ARM64`
Select `Remote machine`,
For computer name, fill it with the Hololens IP, example: `192.168.1.110`
You can find IP in Hololens Wifi Config

## Step 4

Build the whole solution
If you encounter certificate issue, please generate the certificate file in different way (ex. OpenSSL). Also check if you finish the step 2.

## Step 5

Wear your hololens, unlock it
Find `Update and Security` tab in `Config`
Enable all developing options

## Step 6

In Visual Studio, run the program with `Remote Computer`
You may need to input a pair key, which could be found in Develop Options on Hololens.
Finally, wait for 30 mins for uploading (depends on route and network)...
Everything is done, now you can use this on the Hololens!

#### other

I waste several hours today for debugging the certificate issue (╯‵□′)╯︵┻━┻





