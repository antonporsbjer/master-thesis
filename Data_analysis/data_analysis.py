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
    print(f"Data from file {json_files[idx]}:")
    print(data)
    print('-' * 40)