# Master's Thesis: Visual Perception & Signage Accessibility Module

This directory contains the experimental framework, visual perception engine, and scenario testbeds developed for the Master's Thesis:
**"Evaluating Signage Visual Accessibility in Crowded Transit Environments through Microscopic Pedestrian Simulation."**

All thesis-specific research logic is encapsulated here and extends the core simulator in `Assets/CrowdSimulator`.

---

## 1. Perception Model Architecture

### Visual Catchment Area (VCA) & Sightline Occlusion (`Sign.cs`, `Vision.cs`)
- **Geometric Catchment**: Signs project a 3D Visual Catchment Area (VCA) defined by maximum viewing distance ($D_{\text{max}}$), horizontal aperture angle ($\theta_{\text{vca}}$), and height boundaries.
- **Raycast Line-of-Sight Sampling**: Agents cast multiple sightline raycasts from their empirical eye height ($H_{\text{eye}}$) to discretized points across the sign face.
- **Dynamic Human Occlusion**: Surrounding pedestrians act as dynamic physical colliders. A sightline is obstructed if another pedestrian's body or head intersects the ray.
- **Comprehension Threshold**: Detection requires continuous, unobstructed line-of-sight exceeding a designated comprehension duration ($T_{\text{comp}}$), typically $1.0\text{--}2.4\,\text{s}$.

### Target Audience Filtering (`Sign.cs`)
- Signs can specify `targetStartNodes` corresponding to particular concourse entry gates or platform corridors.
- `IsTargetAudience(startNode)` flags whether each passing pedestrian is part of the designated audience cohort, enabling precise evaluation of wayfinding utility.

### Experiment Data Collection (`DataCollector.cs`)
- Automatically logs agent perception outcomes upon reaching destination nodes.
- **Logged Columns**:
  - `AgentID`, `AgentType`, `Height`, `EyeHeight`
  - `StartNode`, `GoalNode`, `Route`
  - `TimeInVCA` (dwell time), `SawSign` (comprehension success)
  - `CrowdDensityAlpha` ($\alpha$ continuum congestion parameter)
  - `SignName`, `IsTargetAudience`
  - `ScenarioID`, `SignHeight`, `SignPositionX`, `SignPositionZ`, `SignComprehensionTime`
- Automatically writes timestamped CSV files to `Data_analysis/data/<ScenarioID>/`.

---

## 2. Demographic Cohorts

| Cohort | Eye Height ($H_{\text{eye}}$) | Stature ($H$) | Description |
| :--- | :--- | :--- | :--- |
| **Adult Male** | $\approx 1.63\,\text{m}$ | $1.75\,\text{m}$ | Upper percentile standing eye height |
| **Adult Female** | $\approx 1.51\,\text{m}$ | $1.63\,\text{m}$ | Lower percentile standing eye height |
| **Wheelchair** | $\approx 1.15\,\text{m}$ | $1.30\,\text{m}$ | Seated perspective; maximum occlusion vulnerability |

---

## 3. Pre-Baked Standalone Scenario Scenes

Located in `Assets/Porsbjer_Anton_Thesis/Scenes/Scenarios/`:

### `Scenario_A1.unity` (Corridor Baseline - Binary Adults)
- **Environment**: Linear corridor testbed ($3.0\,\text{m}$ width).
- **Demographics**: Binary adult population (Adult Male, Adult Female).
- **Signage**: 5 pre-positioned signs (`Sign_0` baseline, `Sign_A` through `Sign_D` corridor waypoints).

### `Scenario_A2.unity` (Corridor Baseline - Inclusive Accessibility)
- **Environment**: Linear corridor testbed.
- **Demographics**: 75% standing adults, 25% wheelchair users.
- **Objective**: Evaluates seated eye-height occlusion disparities against standing cohorts.

### `Scenario_B.unity` (Senri-Chuo Station Concourse Validation)
- **Environment**: Photogrammetric & architectural reconstruction of Senri-Chuo Station concourse.
- **Signage**:
  - `Sign_Main`: Concourse directional sign targeting Entrances 2, 4, and 6.
  - `Sign_Hotel`: Universal concourse circulation sign.
- **Objective**: Validates target audience filtering and corridor routing in complex geometry.

### `Scenario_C.unity` (RQ1: High-Density Sweeps)
- **Environment**: Controlled concourse testbed.
- **Density Sweeps**: Systematically evaluates continuum density $\alpha \in \{0.2, 0.4, 0.6, 0.8, 1.0\}$.
- **Objective**: Addresses **RQ1**: Two-Way ANOVA ($\alpha \times H_{\text{eye}}$) on visibility degradation and demographic inequity.

### `Scenario_D.unity` (RQ2: Geometric Placement Configurations)
- **Environment**: Multi-waypoint evaluation concourse.
- **Parameters**: Varied mounting heights ($H_{\text{sign}} \in [2.0, 3.5]\,\text{m}$), viewing distances, and aperture angles.
- **Objective**: Addresses **RQ2**: Multivariate regression modeling optimal placement trade-offs.

---

## 4. Running Experiments & Analysis

### Running Simulations in Unity
1. Open any scene in `Assets/Porsbjer_Anton_Thesis/Scenes/Scenarios/`.
2. Press **Play** in the Unity Editor (or execute via Unity Standalone/Batchmode).
3. The simulation logs agent data to `Data_analysis/data/`.

### Running Python Analysis
Execute the corresponding script in `Data_analysis/`:

```bash
# Scenario A (Corridor Baseline)
python Data_analysis/analyze_scenario_a.py --dir Data_analysis/data/scenario-A-1 --output Data_analysis/output/scenario_A_results

# Scenario B (Senri-Chuo Station Validation)
python Data_analysis/analyze_scenario_b.py --dir Data_analysis/data/Scenario_B --output Data_analysis/output/scenario_B_results

# Scenario C (RQ1: High-Density Sweeps)
python Data_analysis/analyze_scenario_c.py --dir Data_analysis/data/Scenario_C --output Data_analysis/output/scenario_C_results

# Scenario D (RQ2: Signage Placement Configurations)
python Data_analysis/analyze_scenario_d.py --dir Data_analysis/data/Scenario_D --output Data_analysis/output/scenario_D_results
```
All scripts automatically produce publication-quality charts (PNG 300 DPI), summary tables (CSV and LaTeX), and comprehensive Markdown reports.
