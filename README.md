# RAF v2 Simulation
### Description
This repository contains the unity project for the simulation of the RAF project at Cleveland State University. This project aims to provide a simulated experience of the robotic-assisted feeding system at CSU to obtain user feedback of the current iteration. More info about the project can be found [here](https://chms-raf.github.io/simulation). 
### Installation
#### Install Unity
Unity Editor Version: 2023.2.20f1  
You can download the specific Unity Editor Version through [Unity Hub](https://unity3d.com/get-unity/download).
#### Clone the RAFv2 Simulation Repo
Clone this repository:
```sh
git clone https://github.com/chms-raf/raf-v2-simulation.git
```

Then, open the `raf-v2-simulation` project in Unity.
#### Dependencies
Install the following dependencies using the [Unity Package Manager](https://docs.unity3d.com/Manual/upm-ui.html).  
Packages used:
- [Bio IK (v2.0d)](https://assetstore.unity.com/packages/tools/animation/bio-ik-67819)
- [URDF Importer (v0.5.2-preview)](https://github.com/Unity-Technologies/URDF-Importer)
- [DOTween (HOTween v2) (1.2.765)](https://assetstore.unity.com/packages/tools/animation/dotween-hotween-v2-27676)
- [UI Rounded Corners (3.4.1)](https://github.com/ReForge-Mode/Unity_UI_Rounded_Corners)

#### Running
- Open the `RAF_Scene` in the Scenes folder to run it in the Unity Software.
- To build you can:
	- Build for Windows/Linux applications
	- Build for OpenGL to use on websites

### Project Settings
The project settings should not need changed. If something goes wrong, these the are settings that were changed:
- Most of the setting changes came from the [Unity Robotics Hub tutorial](https://github.com/Unity-Technologies/Unity-Robotics-Hub/blob/main/tutorials/pick_and_place/1_urdf.md) to change the physics solver type.
- The `Default Contact Offset` was changed to allow collisions to be detected as such a small scale. (Value changed from `0.01` to `0.0001`)
- The `Default Solver Iterations` was changed to allow the physics to go through more iterations when calculating to be more stable. (Value changed from `6` to `20`)
