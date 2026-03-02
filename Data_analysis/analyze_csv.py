import pandas as pd
import glob
import os
import sys

# Directory containing the generated visibility CSV files
script_dir = os.path.dirname(os.path.abspath(__file__))
data_dir = os.path.join(script_dir, 'data')
output_dir = os.path.join(script_dir, 'results')

# 1. Grab all CSVs and load them into a single monolithic DataFrame
csv_files = glob.glob(os.path.join(data_dir, '*.csv'))

if not csv_files:
    print("Error: No CSV files found. Make sure you place them in the 'data' directory.")
    sys.exit(1)

df_list = []
for file in csv_files:
    df_temp = pd.read_csv(file)
    df_list.append(df_temp)

df = pd.concat(df_list, ignore_index=True)

print("=" * 80)
print(f"Loaded {len(df)} total agent records from {len(csv_files)} simulation runs.")
print("=" * 80)
print()

# -------------------------------------------------------------------------------------
# RQ1 Analysis: Density vs Visibility Ratio
# Hypothesis: Higher density levels (different ScenarioIDs/Spawn Rates) drop visibility.
# -------------------------------------------------------------------------------------
print("--- RQ1: Visibility Ratio by Scenario ---")
# Only looking at agents that entered the VCA and actually *could* see the sign
vca_mask = df['TimeInVCA'] > 0
visibility_by_scenario = df[vca_mask].groupby('ScenarioID')['SawSign'].mean() * 100
for scenario, ratio in visibility_by_scenario.items():
    print(f"{scenario}: {ratio:.2f}%")
print()

# -------------------------------------------------------------------------------------
# RQ2 Analysis: Sign Height vs Visibility
# Hypothesis: Lower signs are more vulnerable to dynamic occlusion.
# -------------------------------------------------------------------------------------
print("--- RQ2: Visibility Ratio by Sign Height ---")
visibility_by_height = df[vca_mask].groupby('SignHeight')['SawSign'].mean() * 100
for height, ratio in visibility_by_height.items():
    print(f"Sign at {height}m: {ratio:.2f}%")
print()

# -------------------------------------------------------------------------------------
# RQ3 Analysis: Demographic Inequities
# Hypothesis: Shorter agents (Wheelchairs) have worse view lines than tall agents (Males)
# -------------------------------------------------------------------------------------
print("--- RQ3: Visibility Ratio by Agent Demographic ---")
# First, let's establish a high-density condition if we want (or look globally)
visibility_by_agent_type = df[vca_mask].groupby('AgentType')['SawSign'].mean() * 100
for a_type, ratio in visibility_by_agent_type.items():
    print(f"{a_type}: {ratio:.2f}%")
print()

print("=" * 80)
print("Analysis Complete.")
print("=" * 80)
