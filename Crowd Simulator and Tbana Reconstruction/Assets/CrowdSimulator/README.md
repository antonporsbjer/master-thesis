# Crowd Simulator Core Engine

This folder contains the core Continuum Crowds / Velocity-Obstacle hybrid pedestrian simulation framework. It serves as the base locomotion and navigation engine, completely decoupled from any thesis-specific or application-specific sightline logic.

---

## 1. Core Architecture & Architectural Principles

- **Continuum Crowds Formulation**: Solves dynamic potential fields and velocity fields across a staggered spatial grid (`SimulationGrid.cs`).
- **Locomotion & Path Planning**: Agents navigate using graph-based waypoints (`Node.cs`, `CustomNode.cs`, `LinedNode.cs`) integrated with local collision avoidance.
- **Strict Decoupling**: The core simulator has **zero dependencies** on `Porsbjer_Anton_Thesis`. Perception, sightline occlusion, and experimental signage logic extend the simulator externally via `Assets/Porsbjer_Anton_Thesis`.

---

## 2. Advanced Performance & Engine Features

### A. Unity C# Job System Parallelization (`GridParallelBridge.cs`)
- Offloads continuum density splatting and velocity grid calculations onto worker threads via Unity's C# Job System and `NativeArray` memory buffers.
- Eliminates CPU bottlenecks during large-scale simulations (e.g., 500+ agents in Senri-Chuo Station).

### B. Decoupled Rectangular Grid Dimensions (`SimulationGrid.cs`)
- Supports non-square simulation environments, allowing independent width, length, and cell resolution.
- Includes dynamic alpha accessibility: `SimulationGrid.alpha` exposes `currentAlpha` to inspect and dynamically modulate crowd packing tightness during runtime density sweeps.

### C. Proportional Demographic Spawner (`NewSpawner.cs`)
- Supports both standard uniform agent instantiation and custom demographic cohort weighting (`useCustomDemographics = true`).
- Configurable `demographicWeights` array allows arbitrary cohort proportions (e.g., exact 75% standing adults / 25% wheelchair users) with deterministic or pseudo-random selection.

### D. Boundary Safety & Spatial Hashing
- Robust out-of-bounds guards prevent grid index out-of-range exceptions near concourse perimeters and station walls.
- Optimized neighbor bin spatial hashing accelerates local agent-to-agent avoidance checks.

### E. Experiment HUD
- Runtime performance overlay displaying active agent count, real-time FPS, solver iteration counts, and current alpha density parameters.

---

## 3. Key Components & Inspector Parameters

### Main Simulation Controller (`Main.cs`)
- **Max Number Of Agents**: Capacity limit for continuously spawned agents.
- **Plane Size**: Dimensions of the simulation plane.
- **Cells Per Row**: Staggered grid resolution; controls discretization granularity.
- **Neighbor Bins**: Spatial hash bins used for local collision avoidance.
- **Alpha ($\alpha$)**: Continuum packing parameter ($\alpha \in [0, 1]$). Higher values permit tighter agent packing and elevated crowd congestion.
- **Solver**: Selection of iterative solver (PSOR recommended for numerical stability and performance).
- **Solver Max Iterations / Epsilon**: Convergence tolerance criteria.

### Spawner (`NewSpawner.cs`)
- **Agent Editor Container**: Target hierarchy parent to keep editor scene clean.
- **Custom Goal**: Destination node assigned to spawned pedestrians.
- **Spawn Method**: Uniform, Continuous, Circle, Disc, or Area spawning.
- **Use Custom Demographics**: When enabled, spawns agent models according to normalized percentages in `Demographic Weights`.

### Guidance Nodes (`CustomNode.cs`, `LinedNode.cs`)
- **Custom Node**: Point-based attraction node; optimal for doorways and turnstiles.
- **Custom Node Lined**: Dispersed line-segment attraction node; creates natural, wide-front pedestrian corridors across platforms and station concourses.
