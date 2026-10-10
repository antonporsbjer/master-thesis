# Spawner Manager (`SpawnerManager.cs`)

Central controller for inspecting, grouping, and configuring all pedestrian spawners (`NewSpawner`) in a simulation scene.

---

## Quick Start (3 Steps)

1. **Select or Create the GameObject**: In your scene, select the `SpawnManager` GameObject (or create an empty GameObject and attach `SpawnerManager.cs`).
2. **Discover Spawners**: In the Inspector, click **`Find All in Scene`**. It will automatically detect all spawners, group them by their parent object (e.g. `zone1`–`zone6`), and display them.
3. **Configure Rates**: Set rates individually per door/entrance, or by zone. Hit **Play**—changes made during simulation take effect immediately.

---

## Core Modes

| Mode | Property | Behavior |
| :--- | :--- | :--- |
| **Independent Mode** *(Default)* | `Use Global Spawn Rate = false` | Each spawner uses its own `Spawn Rate (ag/s)` independently. |
| **Global Mode** | `Use Global Spawn Rate = true` | All spawners share `Global Spawn Rate`, except those with **Override Global** ticked. |

* **Spawn Rate Multiplier** (`spawnRateMultiplier`): Scales all rates proportionally (e.g., `2.0` doubles everyone's rate; `0.5` halves it). Ideal for crowd density sweeps while keeping per-door arrival ratios.

---

## Per-Spawner Configuration

Each spawner entry in the list allows independent control of:

* **Active (Checkbox)**: Enable or disable an individual spawner without removing it or breaking graph nodes.
* **Spawner Reference**: Direct link to the `NewSpawner` in the hierarchy (click to ping/select).
* **Display Label**: Custom label (e.g., *"Car 2 - Door 1"*).
* **Rate (`ag/s`)**: Agents spawned per second for this specific spawner.
* **Poisson**: Toggles Poisson distributed arrival intervals vs. deterministic fixed intervals.
* **Goal**: Optional custom destination node (`CustomNode`) for pedestrians from this spawner.
* **Override Global**: (Visible in Global mode) Forces this spawner to use its individual rate regardless of global settings.

---

## Zone & Batch Controls

Spawners under parent transforms (such as `zone1`, `zone2`, ..., `zone6`) are automatically organized into foldouts:

* **Zone Foldout**: Shows active spawner count and combined zone throughput (e.g., `zone3 (5/5 active, 2.60 ag/s)`).
* **Enable / Disable Zone**: Toggle all spawners in a train car or concourse sector with one click.
* **Set Zone Rate**: Enter a rate and click `Set All in Zone` to batch-assign rates to an entire platform section.
* **Enable / Disable All**: Quick global toggles in the batch toolbar.
* **Search / Filter**: Type in the filter bar to instantly isolate specific doors or zones.

---

## Runtime & Integration

* **Live Play-Mode Sync**: Editing rates, toggling spawners, or adjusting the multiplier during simulation takes effect on the very next spawn cycle without restarting.
* **RunManager Compatibility**: When `RunManager` runs automated test suites, it calls `spawnerManager.ApplyCrowdDensity(param.crowdDensity)`. In independent mode, this applies the run's density as a proportional multiplier across all custom spawner rates.

---

## Script API

```csharp
// Get singleton
var sm = SpawnerManager.instance;

// Adjust individual spawner
sm.SetSpawnerRate(spawnerComponent, 1.5f);
sm.SetSpawnerEnabled(spawnerComponent, false);

// Adjust by zone
sm.SetZoneRate("zone1", 0.8f);
sm.SetZoneEnabled("zone1", true);

// Global & batch controls
sm.SetGlobalSpawnRate(2.0f);
sm.SetAllRates(0.5f);
sm.SetAllEnabled(true);

// Metrics
float totalThroughput = sm.TotalSpawnRate; // Total agents/sec currently spawning
int activeSpawners = sm.ActiveSpawnerCount;
```
