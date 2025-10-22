import json
import glob
import os
import pandas as pd
import seaborn as sns
import matplotlib.pyplot as plt
import numpy as np

# --- Configuration ---
# Directory where your JSON data files are stored
data_dir = 'data'

# --- Data Loading and Processing ---
json_files = glob.glob(os.path.join(data_dir, '*.json'))

if not json_files:
    print(f"No JSON files found in the '{data_dir}' directory. Please check the path.")
else:
    all_data = []
    for json_file_path in json_files:
        with open(json_file_path, 'r', encoding='utf-8') as file:
            try:
                data = json.load(file)
                all_data.append((data, os.path.basename(json_file_path)))
            except json.JSONDecodeError:
                print(f"Warning: Could not decode JSON from file: {json_file_path}")

    heatmap_data = []

    # --- Detailed Analysis (from your original script) ---
    for data, filename in all_data:
        print('=' * 80)
        print(f"Data from file {filename}")
        print()

        global_data = data.get('global', {})
        agent_data = data.get('agents', [])

        if not global_data or not agent_data:
            print("Skipping file due to missing 'global' or 'agents' data.")
            print()
            continue

        print(f"Scenario Name: {global_data.get('scenarioId', 'N/A')}")
        print(f"Total number of agents: {global_data.get('totalAgents', 'N/A')}")
        print(f"Sign comprehension time: {global_data.get('signComprehensionTime', 'N/A')} seconds")
        print(f"Sign height: {global_data.get('signHeight', 'N/A')} meters")
        print(f"Sign position: ({global_data.get('signPositionX', 'N/A')},{global_data.get('signPositionZ', 'N/A')})")
        
        if agent_data:
            avg_time_in_vca = sum(agent.get('timeInVCA', 0) for agent in agent_data) / len(agent_data)
            print(f"Average time spent in VCA: {avg_time_in_vca:.2f} seconds")
            
            print('-' * 40)

            visibility_ratio = sum(agent.get('sawSign', 0) for agent in agent_data) / len(agent_data)
            print(f"Total Visibility Ratio: {visibility_ratio * 100:.2f}%")

            # Collect data for the heatmap
            heatmap_data.append({
                'x': global_data.get('signPositionX'),
                'z': global_data.get('signPositionZ'),
                'visibility': visibility_ratio
            })

            agents_in_vca = [agent for agent in agent_data if agent.get('timeInVCA', 0) > 0]
            print(f"Number of agents in VCA: {len(agents_in_vca)}")
            if agents_in_vca:
                vca_visibility_ratio = sum(agent.get('sawSign', 0) for agent in agents_in_vca) / len(agents_in_vca)
                print(f"Total Visibility Ratio of agents who entered the VCA: {vca_visibility_ratio * 100:.2f}%")
            
            # Analysis by agent type
            agent_types = set(agent.get('type') for agent in agent_data)
            for agent_type in sorted(list(agent_types)):
                print('-' * 40)
                type_agents = [agent for agent in agent_data if agent.get('type') == agent_type]
                type_agents_in_vca = [agent for agent in agents_in_vca if agent.get('type') == agent_type]
                
                if type_agents:
                    type_visibility_ratio = sum(agent.get('sawSign', 0) for agent in type_agents) / len(type_agents)
                    print(f"Visibility Ratio of {agent_type}: {type_visibility_ratio * 100:.2f}%")
                
                print(f"Number of {agent_type} in VCA: {len(type_agents_in_vca)}")

                if type_agents_in_vca:
                    type_vca_visibility_ratio = sum(agent.get('sawSign', 0) for agent in type_agents_in_vca) / len(type_agents_in_vca)
                    print(f"Visibility Ratio of {agent_type} who entered the VCA: {type_vca_visibility_ratio * 100:.2f}%")
        
        print()

    # --- Heatmap Generation ---
    if heatmap_data:
        df = pd.DataFrame(heatmap_data)
        
        # Check for missing position data
        df.dropna(subset=['x', 'z'], inplace=True)

        if not df.empty:
            # Create a pivot table for the heatmap. Use mean for duplicate positions.
            try:
                heatmap_pivot = df.pivot_table(index='z', columns='x', values='visibility', aggfunc='mean')
                
                # Sort the pivot table for a correct plot
                heatmap_pivot.sort_index(axis=0, ascending=False, inplace=True)
                heatmap_pivot.sort_index(axis=1, ascending=True, inplace=True)

                plt.figure(figsize=(14, 10))
                sns.heatmap(heatmap_pivot, annot=True, fmt=".2%", cmap="viridis", linewidths=.5, cbar_kws={'label': 'Total Visibility Ratio'})
                plt.title('Heatmap of Total Visibility Ratio by Sign Position', fontsize=16)
                plt.xlabel('Sign Position X', fontsize=12)
                plt.ylabel('Sign Position Z', fontsize=12)
                plt.tight_layout()
                plt.savefig('results/heatmap.png')
                print("\nHeatmap successfully saved as heatmap.png")
            except Exception as e:
                print(f"\nCould not generate heatmap. Error: {e}")
        else:
            print("\nCould not generate heatmap because no valid position data was found.")
    else:
        print("\nNo data available to generate a heatmap.")