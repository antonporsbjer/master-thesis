import json
import glob
import os

data_dir = 'data'
json_files = glob.glob(os.path.join(data_dir, '*.json'))

all_data = []

for json_file_path in json_files:
    with open(json_file_path, 'r', encoding='utf-8') as file:
        data = json.load(file)
        all_data.append(data)

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
    print(f"Sign height: {global_data['signHeight']} meters")
    print(f"Average time spent in VCA: {sum(agent['timeInVCA'] for agent in agent_data) / len(agent_data)} seconds")
    
    print('-' * 40)

    print(f"Total Visibility Ratio: {sum(agent['sawSign'] for agent in agent_data) / len(agent_data)* 100:.2f}%")
    agents_in_vca = [agent for agent in agent_data if (agent['timeInVCA'] > 0)]
    print(f"Number of agents in VCA: {len(agents_in_vca)}")
    print(f"Total Visibility Ratio of agents who entered the VCA: {sum(agent['sawSign'] for agent in agents_in_vca) / len(agents_in_vca)* 100:.2f}%")
    
    print('-' * 40)
    
    WheelchairAgent_agents = [agent for agent in agent_data if agent['type'] == 'WheelchairAgent(Clone)']
    print(f"Visibility Ratio of WheelchairAgent: {sum(agent['sawSign'] for agent in WheelchairAgent_agents) / len(WheelchairAgent_agents) * 100:.2f}%")
    WheelchairAgent_agents_in_vca = [agent for agent in agent_data if (agent['timeInVCA'] > 0) and agent['type'] == 'WheelchairAgent(Clone)']
    print(f"Number of WheelchairAgent in VCA: {len(WheelchairAgent_agents_in_vca)}")
    print(f"Visibility Ratio of WheelchairAgent who entered the VCA: {sum(agent['sawSign'] for agent in WheelchairAgent_agents_in_vca) / len(WheelchairAgent_agents_in_vca) * 100:.2f}%")

    print('-' * 40)

    AdultFemaleAgent_agents = [agent for agent in agent_data if agent['type'] == 'AdultFemaleAgent(Clone)']
    print(f"Visibility Ratio of AdultFemaleAgent: {sum(agent['sawSign'] for agent in AdultFemaleAgent_agents) / len(AdultFemaleAgent_agents) * 100:.2f}%")
    AdultFemaleAgent_agents_in_vca = [agent for agent in agent_data if (agent['timeInVCA'] > 0) and agent['type'] == 'AdultFemaleAgent(Clone)']
    print(f"Number of AdultFemaleAgent agents in VCA: {len(AdultFemaleAgent_agents_in_vca)}")
    print(f"Visibility Ratio of AdultFemaleAgent who entered the VCA: {sum(agent['sawSign'] for agent in AdultFemaleAgent_agents_in_vca) / len(AdultFemaleAgent_agents_in_vca) * 100:.2f}%")

    print('-' * 40)

    AdultMaleAgent_agents = [agent for agent in agent_data if agent['type'] == 'AdultMaleAgent(Clone)']
    print(f"Visibility Ratio of AdultMaleAgent: {sum(agent['sawSign'] for agent in AdultMaleAgent_agents) / len(AdultMaleAgent_agents) * 100:.2f}%")
    AdultMaleAgent_agents_in_vca = [agent for agent in agent_data if (agent['timeInVCA'] > 0) and agent['type'] == 'AdultMaleAgent(Clone)']
    print(f"Number of AdultMaleAgent agents in VCA: {len(AdultMaleAgent_agents_in_vca)}")
    print(f"Visibility Ratio of AdultMaleAgent who entered the VCA: {sum(agent['sawSign'] for agent in AdultMaleAgent_agents_in_vca) / len(AdultMaleAgent_agents_in_vca) * 100:.2f}%")
          
    print()