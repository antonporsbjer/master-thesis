# Welcome to the crowd simulator! 👩‍💻
![image](https://github.com/user-attachments/assets/92e9c724-d89c-44c6-af9f-71a534b34905)

Please fork this repo if you plan to create a project based on this version.

Verified to work with Unity 2022.3.18f1.

There are sample scenes in `MetropedInteractive/Assets/CrowdSimulator/Scenes`.

Prefabs needed for the simulation can be found in `MetropedInteractive/Assets/CrowdSimulator/Prefabs`.

Please use the agent models in `MetropedInteractive/Assets/CrowdSimulator/Prefabs/Agents`.

The models are (almost) scaled to match real life (1 unit in Unity = 1 meter).

This setup should work (Main.cs component):

![image](https://github.com/user-attachments/assets/04a53551-eb3d-4ce9-b0dd-79a390398049)

# Acknowledgements
### Jack Shabo

Original implementation of high density crowd simulator.

[High Density Simulation of Crowds with Groups in Real-Time](https://urn.kb.se/resolve?urn=urn:nbn:se:kth:diva-210564)

### Julian Ley

Created subway reconstruction.

[Metroped Interactive](https://github.com/JulianLey/MetropedInteractive)

### Jenna Smulter

Improvements and bug fixes to crowd simulator. Owner of this repo.

### Anton Porsbjer

Improvements and bug fixes to crowd simulator. Owner of this repo.



# How to use the simulator

## Components for simulation

The scene needs to have a Main object, and at least one Spawner and one Goal.

Nodes are placed in the world to guide agents. They will walk to their goal using the shortest path of nodes.

**Custom Node**

Agents will steer towards the center of the node, which works well for chokepoints like doorways, but might create unrealistic movement since all agents are moving towards the exact same point.

**Custom Node Lined**

Instead of moving towards the center of the node the agents will move towards the closest point along a line across the nodes diameter. This might make the movement look more realistic. It might look like the agents reach the node before they actually reach the node’s area if they steer towards a point closer to the edge.

### Spawner

Agent Editor Container

An empty game object of which the agent objects will be children to not make the editor hierarchy window cluttered.

**Custom Goal**

The goal node of the agents of this spawner. If no custom goal is set the agents will have the goal at index 0 as their goal.

#### Spawn Method

**Uniform Spawn**

The given number of agents will spawn spread out uniformly over the plane area.

**Circle Spawn**

Agents will spawn in a circle with the given radius and walk towards the center.

**Disc Spawn**

Agents will spawn in a disc with the given radius and number of rows and walk towards the center.

**Continuous Spawn**

Agents will spawn continuously with the given spawn rate and walk towards their goal. A lower spawn rate means less time between spawns.

**Area Spawn**

Agents will spawn in an area with the given dimensions an walk towards their goal.

### Main

**Max Number Of Agents**
- The maximum number of agents that can be active at any time when spawning agents continuously.

**Plane Size**
- Length of the side of the square plane. The plane should be big enough to cover the area where the agents will walk.

**Road Node Amount**
- Number of extra nodes to be placed automatically. Extra nodes can be placed automatically which might make it easier for agents to move around obstacles and might make their movement look more realistic.

**Cells Per Row**
- Number of cells per row in the staggered grid. A higher number allows for a more detailed representation of agent movement but at a higher computation cost.

**Neighbor bins**
- The number of neighbor bins used to calculate collision avoidance for agents. More bins allows for more realistic simulation at the expense of computational cost.

**Agent Max Speed, Agent Min Speed**
- Each agent’s walk speed will be a randomized number within this interval.

**Custom Time Step**
- Set this to true to use a custom time step. If it’s false Unity’s Time.deltaTime will be used.

**Time Step**
- The custom time step if custom time step is used.

**Alpha**
- A higher alpha value (closer to 1) allows for more perfect packing of agents and denser crowds, a lower value gives a sparser crowd.

**Solver**
- PSOR seems to be faster and more stable than the other two.

**Solver Max Iterations**
- A higher number gives more accurate simulation but takes more time.

**Epsilon**
- A lower value gives more accurate simulations but takes more time.

**Show Splatted Density**
- Visualizes the crowd density.

**Show Splatted Velocity**
- Visualizes the crowd velocity.

**Visible Map**
- True will show the nodes when playing and false will hide them.

**Walk back**
- If this option is checked agents will walk back to their previous target node if they loose sight of the next one. This might be good if agents get stuck, but it might look unrealistic in some scenarios.

**Skip Node If See Next**
- If this option is checked agents will start moving to the next node in their path as soon as it’s in their line of sight, otherwise they have to reach each node before proceeding to the next.

**Smooth Turns**
- This option makes agents turn more smoothly which might make their movement look more realistic.

**Handle Collision**
- Doesn’t seem to do anything. 🤷

**Agent Avoidance Radius**
- The preferred distance between two agents. 0.5 appears to be a good value.

**Use Preset Group Distances**
- When this option is true preset distances between agents for different group sizes are used. When it is false the same distance is used regardless of group size.

# Screenshots

![platform](https://github.com/user-attachments/assets/cf310d79-30d5-4939-ba05-bc617972f3e2)
![mycketfolk](https://github.com/user-attachments/assets/3859463f-a448-4c07-9edd-70f2e520feff)

