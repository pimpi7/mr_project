# Medical Robotics Project

Migration from CoppeliaSim to Unity and improvement of the dVRK model

## Authors
- Gabriel Adam
- Andrea Gravili
- Kristjan Tarantelli
- Francesco Pimpinelli

## Overview

![Project Overview](images/dvrk_all.png)

This project focuses on migrating the da Vinci Research Kit (dVRK) simulation from CoppeliaSim to Unity, with improvements to the model and implementation.

## Features

- Migrated dVRK simulation environment to Unity
- Enhanced robotic model with improved physics and control
- Interactive surgical simulation capabilities

![dVRK Simulation](images/dvrk_all_ph.png)

## Project Structure

```
dVRK/
├── Assets/          # Unity assets and resources
├── Scenes/          # Unity scene files
├── Scripts/         # C# scripts for simulation
├── Settings/        # Project configuration
└── ...
```

## Requirements

- Unity 6000.2.5f1
- Required Unity packages (see Packages/manifest.json)

## Getting Started

1. Clone the repository
2. Open the project in Unity
3. Navigate to the Scenes folder and open the main scene
4. Press Play to start the simulation

With the left and right arrow keys, you select the joint to actuate and with the top and bottom arrow keys you increase or decrease the joint angle (or position if prismatic).

To open and close the grippers, you can inspect the grippers object and choose the keys. By default are: "I", "K" and "U", "J". 

Also if you inspect the setupJointBaseRespondable, you can see the joint selected for the testing (and also change the joint characteristics, like force, dumping, limits, ...).

## License

See [LICENSE](LICENSE) file for details.
