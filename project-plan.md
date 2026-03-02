# Unity Project Plan: High-Density Signage Visibility Simulation

## 1. Project Overview & Repository State
*   **Base Framework:** Jack Shabo's high-density crowd simulator (Unilateral Incompressibility Constraint - UIC).
*   **Objective:** Extend the Eulerian-Lagrangian crowd model with a Raycast-based Field of View (FOV) vision system to calculate dynamic signage occlusion (Visibility Ratio) under varying crowd densities.

## 2. Implementation Phases

### Phase 1: Environment and Signage Setup
1.  **Environment Setup:** Ensure static obstacles (walls, columns) have appropriate mesh colliders.
2.  **Signage Discretization:** Implement a script to convert target signage surfaces into a set of discrete cell unit nodes (e.g., $0.01\text{m}$ grid steps). This is essential for calculating the Detection Area Ratio ($DA_{ratio}$).
3.  **VCA Trigger Zones:** Attach a volumetric cone (sphere sector) trigger collider to each sign representing its theoretical Visibility Catchment Area (VCA). The maximum distance ($l$) is derived from the ISO 3864-1 distance factor formula: $l = z_0 \times h$.

### Phase 2: Agent Vision System (Modifying Shabo's Agents)
1.  **Eye GameObject Initialization:** Modify the agent spawner to instantiate an "Eye" GameObject for each agent. The height of this GameObject must dynamically adjust based on the agent's assigned demographic profile.
2.  **FOV Mechanics:** Implement the virtual FOV using `Physics.Raycast`. The FOV should be constrained to $30^\circ$ horizontally, $25^\circ$ vertically above the line of sight, and $30^\circ$ vertically below. 
3.  **Raycast Logic:** When an agent enters a sign's VCA trigger, calculate `Vector3.Angle` between the agent's forward vector and the vector to the sign. If within the FOV, cast rays to the sign's discrete nodes. If the ray hits a static obstacle or another agent's cylinder collider, register an occlusion.

### Phase 3: Comprehension Tracking and Data Collection
1.  **Exposure Timer:** For each agent, track the continuous `Time.deltaTime` during which the $DA_{ratio}$ remains above the critical threshold (e.g., $23\%$).
2.  **Detection Registration:** If the continuous exposure time exceeds the sign's defined "Comprehension Time" ($t$), flag the sign as "Effectively Detected" for that specific agent.
3.  **Data Exporter:** Write a data collector script that outputs CSV logs at the end of each simulation run, recording: Total Agents, Density Level ($\alpha$), Sign ID, Agent Demographic, and Effective Detections.

---

## 3. Simulation Parameters

### A. Agent Demographics & Anthropometrics
Agents must be spawned with specific physical boundaries and eye-levels to accurately simulate 3D occlusion. 

| Agent Type | Distribution | Agent Height | Eye Level GameObject |
| :--- | :--- | :--- | :--- |
| Adult Male | 50% | 1.71m | 1.58m |
| Adult Female | 35% | 1.58m | 1.45m |
| Wheelchair/Child | 15% | 1.30m | 1.17m |

*Note: Base speed for all agents is $1.4\text{ m/s}$.*

### B. Signage Properties (Derived from Motamedi et al. & ISO 3864-1)
Parameters for the target signs evaluated in the scenes.

| Sign Type | Height from Floor | Max Viewing Distance ($d$) | FOV Angle ($h$) | Comprehension Time ($t$) |
| :--- | :--- | :--- | :--- | :--- |
| Main Directional | 2.3m / 2.6m | 10.0m | $90^\circ$ | 2.4s |
| Informational | 2.4m | 15.0m | $90^\circ$ | 1.0s |
| Low Directional | 0.6m | 10.0m | $90^\circ$ | 2.4s |

### C. Crowd Dynamics (Shabo's UIC Parameters)
Adjust these parameters in the UIC solver to transition the crowd from normal flow to highly congested, incompressible states.

| Density State | Max Agents | Spawn Rate | UIC $\alpha$ Parameter | Expected Emergent Behavior |
| :--- | :--- | :--- | :--- | :--- |
| Low (Baseline) | ~800 | Normal | 1.0 | Free flow, high individual visibility. |
| Medium | ~1300 | High | 1.0 | Minor turbulence, lane formation. |
| High (Constrained)| ~3300 | Very High | 0.4 | High turbulence, severe occlusion. |

---

## 4. Experimental Scenarios

To answer your research questions, execute the following scenario matrix. Each scenario should be run multiple times to account for stochastic agent spawning, collecting the **Visibility Ratio** (Agents effectively exposed / Total target agents).

**Scenario 1: Density vs. Baseline Visibility (RQ1)**
*   *Environment:* Subway Passage or Shopping Mall.
*   *Signage:* Standard Exit Signs at 2.6m height.
*   *Variables:* Test across Low, Medium, and High density parameters ($\alpha$ = 1.0 vs 0.4).
*   *Goal:* Identify the exact density threshold where continuous line-of-sight breaks down and the theoretical VCA over-predicts actual detection.

**Scenario 2: Signage Height Optimization under Stress (RQ2)**
*   *Environment:* Shopping Mall.
*   *Signage:* Directional signs placed at varying heights: 0.6m (low) vs. 2.6m (high).
*   *Variables:* Run under High Density ($\alpha$ = 0.4).
*   *Goal:* Determine if lower signs are more susceptible to dynamic occlusion by pedestrian bodies compared to ceiling-mounted signs. 

**Scenario 3: Demographic Inequities in High-Density Flow (RQ3)**
*   *Environment:* Complex Intersection (e.g., Central Hall).
*   *Signage:* Fixed at standard 2.3m.
*   *Variables:* Filter the Visibility Ratio output strictly by Agent Demographic (Male vs. Wheelchair user). 
*   *Goal:* Quantify how much more severely shorter occupants suffer from "shadow regions" caused by surrounding dense crowds.