# INTERFACES project

A simulation project combining GAMA modeling and Virtual Reality to visualize malfunction scenarios of a network of swale in a neighborhood's green area (ZAC des Deux Soeurs, Villeurbanne, France).

## Overview
This repository contains both GAMA modeling components and Unity VR implementation for the 3D visualization of swale malfunctions and maintenance.

## Repository structure
* **GAMA folder:** Contains all GAMA models
  * **models:** Contains the two main GAMA models:
    * `NBSS_AM_v2.1_forVR.gaml`: Base simulation model based on this work `https://github.com/EmmaGirot/NBSS_ABM/tree/master#`
    * `NBSS_AM_v2.1-VR.gaml`: VR file that works with the Unity project
* **INTERFACES Projet:** Unity project for VR visualization:
  * `/Assets/Scenes/NBSS_VR_v1`: Unity scene for PC testing and simulation (without headset)
  * `/Assets/Scenes/NBSS_VR_headset_v3`: Unity scene for VR simulation (with headset)

## Setup and Usage

### GAMA Setup
1. Install [GAMA Platform](https://gama-platform.org/)
2. Install the [SIMPLE Plugin for GAMA](https://github.com/project-SIMPLE/simple.toolchain/tree/Unity-6/GAMA%20Plugin)
3. Install the [middleware](https://github.com/project-SIMPLE/simple.webplatform) to link GAMA and Unity
4. Open the GAMA folder as a project
5. Navigate to /models to access simulation models

### Unity VR Setup
1. Open the **INTERFACES Project** folder in Unity
2. Ensure you have the appropriate VR hardware connected via USB-C
3. Ensure both the PC and headset are connected to the same Wifi network
4. Open `/Assets/Scripts/Gama Provider/WebSocketConnector.cs` and change `localhost` to the iP address of that network : `protected string DefaultIP = "localhost";`
5. Build and Run the project according to [this tutorial to launch simulation](https://doc.project-simple.eu/unity/how-to/Running-a-model-game)
