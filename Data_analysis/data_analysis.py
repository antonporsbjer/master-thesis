import json
import glob
import os
import sys

# --- Create results directory and redirect output ---
output_dir = 'results'
os.makedirs(output_dir, exist_ok=True)
original_stdout = sys.stdout
sys.stdout = open(os.path.join(output_dir, 'result.txt'), 'w')
# --- End of new code ---

data_dir = 'data'
json_files = glob.glob(os.path.join(data_dir, '*.json'))

all_data = []

for json_file_path in json_files:
    with open(json_file_path, 'r', encoding='utf-8') as file:
        data = json.load(file)
        all_data.append(data)

# --- New code for overall statistics ---
# Initialize accumulators for overall averages
total_agents_all_sims = 0
total_saw_sign_all_sims = 0
total_time_in_vca_all_sims = 0
total_agents_in_vca_all_sims = 0
total_saw_sign_in_vca_all_sims = 0

agent_type_stats = {
    'WheelchairAgent(Clone)': {'total': 0, 'saw_sign': 0, 'in_vca': 0, 'saw_sign_in_vca': 0},
    'AdultFemaleAgent(Clone)': {'total': 0, 'saw_sign': 0, 'in_vca': 0, 'saw_sign_in_vca': 0},
    'AdultMaleAgent(Clone)': {'total': 0, 'saw_sign': 0, 'in_vca': 0, 'saw_sign_in_vca': 0},
}
# --- End of new code ---

# Print all loaded data to inspect
for idx, data in enumerate(all_data):
    filename = json_files[idx]
    print('=' * 80)
    print(f"Data from file {filename}")
    print()

    global_data = data['global']
    # print(global_data)

    agent_data = data['agents']
    # print(agent_data)
    # print("Agents:")
    # for agent in agent_data:
    #     print(f"  Agent ID: {agent['agentId']}")
    #     print(f"  Type: {agent['type']}")
    #     print(f"  startNode: {agent['startNode']}")
    #     print(f"  goalNode: {agent['goalNode']}")
    #     print(f"  height: {agent['height']}")
    #     print(f"  eyeHeight: {agent['eyeHeight']}")
    #     print(f"  timeInVCA: {agent['timeInVCA']}")
    #     print(f"  sawSign: {agent['sawSign']}")
    #     print()
    
    print(f"Scenario Name: {global_data['scenarioId']}")
    print(f"Total number of agents: {global_data['totalAgents']}")
    print(f"Sign comprehension time: {global_data['signComprehensionTime']} seconds")
    print(f"Sign height: {global_data['signHeight']} meters")
    print(f"Sign position: ({global_data['signPositionX']},{global_data['signPositionZ']})")
    
    avg_time_in_vca = sum(agent['timeInVCA'] for agent in agent_data) / len(agent_data)
    print(f"Average time spent in VCA: {avg_time_in_vca} seconds")
    
    print('-' * 40)

    # --- New code for accumulating totals ---
    num_agents = len(agent_data)
    num_saw_sign = sum(agent['sawSign'] for agent in agent_data)
    total_agents_all_sims += num_agents
    total_saw_sign_all_sims += num_saw_sign
    total_time_in_vca_all_sims += sum(agent['timeInVCA'] for agent in agent_data)
    # --- End of new code ---

    print(f"Total Visibility Ratio: {num_saw_sign / num_agents * 100:.2f}%")
    agents_in_vca = [agent for agent in agent_data if (agent['timeInVCA'] > 0)]
    
    # --- New code for accumulating totals ---
    num_agents_in_vca = len(agents_in_vca)
    num_saw_sign_in_vca = sum(agent['sawSign'] for agent in agents_in_vca)
    total_agents_in_vca_all_sims += num_agents_in_vca
    total_saw_sign_in_vca_all_sims += num_saw_sign_in_vca
    # --- End of new code ---

    print(f"Number of agents in VCA: {num_agents_in_vca}")
    if num_agents_in_vca > 0:
        print(f"Total Visibility Ratio of agents who entered the VCA: {num_saw_sign_in_vca / num_agents_in_vca * 100:.2f}%")
    else:
        print("Total Visibility Ratio of agents who entered the VCA: 0.00%")

    print('-' * 40)
    
    # --- Modified agent type analysis to be more robust and accumulate stats ---
    agent_types = set(agent['type'] for agent in agent_data)
    for agent_type in sorted(list(agent_types)):
        if agent_type in agent_type_stats:
            type_agents = [agent for agent in agent_data if agent['type'] == agent_type]
            type_agents_in_vca = [agent for agent in agents_in_vca if agent['type'] == agent_type]

            num_type_agents = len(type_agents)
            num_type_saw_sign = sum(agent['sawSign'] for agent in type_agents)
            num_type_agents_in_vca = len(type_agents_in_vca)
            num_type_saw_sign_in_vca = sum(agent['sawSign'] for agent in type_agents_in_vca)

            # Accumulate for overall average
            agent_type_stats[agent_type]['total'] += num_type_agents
            agent_type_stats[agent_type]['saw_sign'] += num_type_saw_sign
            agent_type_stats[agent_type]['in_vca'] += num_type_agents_in_vca
            agent_type_stats[agent_type]['saw_sign_in_vca'] += num_type_saw_sign_in_vca

            # Print stats for current file
            if num_type_agents > 0:
                print(f"Visibility Ratio of {agent_type.replace('(Clone)', '')}: {num_type_saw_sign / num_type_agents * 100:.2f}%")
            else:
                print(f"Visibility Ratio of {agent_type.replace('(Clone)', '')}: 0.00%")
            
            print(f"Number of {agent_type.replace('(Clone)', '')} in VCA: {num_type_agents_in_vca}")

            if num_type_agents_in_vca > 0:
                print(f"Visibility Ratio of {agent_type.replace('(Clone)', '')} who entered the VCA: {num_type_saw_sign_in_vca / num_type_agents_in_vca * 100:.2f}%")
            else:
                print(f"Visibility Ratio of {agent_type.replace('(Clone)', '')} who entered the VCA: 0.00%")
            
            print('-' * 40)
    # --- End of modified section ---
          
    print()

# --- New code to print combined averages ---
print('=' * 80)
print("COMBINED AVERAGE OF ALL SIMULATIONS")
print('=' * 80)
print()

if total_agents_all_sims > 0:
    print(f"Overall Average time spent in VCA: {total_time_in_vca_all_sims / total_agents_all_sims:.2f} seconds")
    print(f"Overall Total Visibility Ratio: {total_saw_sign_all_sims / total_agents_all_sims * 100:.2f}%")
else:
    print("No agent data found across all simulations.")

if total_agents_in_vca_all_sims > 0:
    print(f"Overall Total Visibility Ratio of agents who entered the VCA: {total_saw_sign_in_vca_all_sims / total_agents_in_vca_all_sims * 100:.2f}%")
else:
    print("No agents entered VCA across all simulations.")

print()

for agent_type, stats in agent_type_stats.items():
    print('-' * 40)
    type_name = agent_type.replace('(Clone)', '')
    if stats['total'] > 0:
        print(f"Overall Visibility Ratio of {type_name}: {stats['saw_sign'] / stats['total'] * 100:.2f}%")
    else:
        print(f"No {type_name} agents found.")
    
    if stats['in_vca'] > 0:
        print(f"Overall Visibility Ratio of {type_name} who entered the VCA: {stats['saw_sign_in_vca'] / stats['in_vca'] * 100:.2f}%")
    else:
        print(f"No {type_name} agents entered VCA.")

# --- End of new code ---

# --- Restore original output and print confirmation ---
sys.stdout.close()
sys.stdout = original_stdout
print("Analysis complete. Results saved to results/result.txt")
# --- End of new code ---