import pandas as pd
import glob
import os
import sys
import numpy as np
import matplotlib.pyplot as plt
from scipy.interpolate import griddata

# Directory containing the generated visibility CSV files
script_dir = os.path.dirname(os.path.abspath(__file__))
data_dir = os.path.join(script_dir, 'data')
output_dir = os.path.join(script_dir, 'results')

if not os.path.exists(output_dir):
    os.makedirs(output_dir)

csv_files = glob.glob(os.path.join(data_dir, '*.csv'))

if not csv_files:
    print("Error: No CSV files found. Make sure you place them in the 'data' directory.")
    sys.exit(1)

df_list = []
for file in csv_files:
    try:
        df_temp = pd.read_csv(file)
        if 'SignOrientation' not in df_temp.columns:
            # If older files don't have SignOrientation, assign a default or skip
            df_temp['SignOrientation'] = 0.0
        df_list.append(df_temp)
    except Exception as e:
        print(f"Skipping {file} due to error: {e}")

if not df_list:
    print("Error: No valid DataFrames loaded.")
    sys.exit(1)

df = pd.concat(df_list, ignore_index=True)

# -------------------------------------------------------------------------------------
# Visibility Optimization Analysis (Heatmaps)
# -------------------------------------------------------------------------------------
print("--- Generating Heatmaps ---")

# We only care about agents that actually entered the VCA, or arguably all agents in the environment
# For this plot, the "Visibility Ratio" is typically (Agents who saw sign) / (Total Agents who walked around)
# The paper considers all pedestrians as a potential audience
total_agents_per_run = df.groupby(['SignPositionX', 'SignPositionZ', 'SignOrientation', 'SignComprehensionTime'])['AgentID'].nunique().reset_index(name='TotalAgents')
saw_sign_per_run = df[df['SawSign'] == True].groupby(['SignPositionX', 'SignPositionZ', 'SignOrientation', 'SignComprehensionTime'])['AgentID'].nunique().reset_index(name='SawAgents')

# Merge to get the ratio
agg_df = pd.merge(total_agents_per_run, saw_sign_per_run, on=['SignPositionX', 'SignPositionZ', 'SignOrientation', 'SignComprehensionTime'], how='left')
agg_df['SawAgents'] = agg_df['SawAgents'].fillna(0)
agg_df['VisibilityRatio'] = (agg_df['SawAgents'] / agg_df['TotalAgents']) * 100

def plot_heatmaps(df_subset, title_prefix, filename_prefix):
    # Determine the unique comprehension times
    comp_times = sorted(df_subset['SignComprehensionTime'].unique())
    if not comp_times:
        print(f"No data for {title_prefix}")
        return

    # Set up matplotlib figure (1 row, N columns for each comprehension time)
    fig, axes = plt.subplots(1, len(comp_times), figsize=(5 * len(comp_times), 5), sharey=True)
    if len(comp_times) == 1:
        axes = [axes]

    for i, ct in enumerate(comp_times):
        ax = axes[i]
        ct_data = df_subset[df_subset['SignComprehensionTime'] == ct]
        
        x = ct_data['SignPositionX']
        y = ct_data['SignPositionZ'] # Using Z as the Y-axis for top-down view
        z = ct_data['VisibilityRatio']

        if len(ct_data) < 4:
            ax.set_title(f"{ct} Second(s)\nNot enough data points")
            continue

        # Create a grid for interpolation
        xi = np.linspace(x.min(), x.max(), 100)
        yi = np.linspace(y.min(), y.max(), 100)
        xi, yi = np.meshgrid(xi, yi)

        # Interpolate Z values onto the grid
        zi = griddata((x, y), z, (xi, yi), method='cubic')
        
        # If cubic fails due to collinearity or sparse points, fallback to linear or nearest
        if np.isnan(zi).all():
            zi = griddata((x, y), z, (xi, yi), method='nearest')

        # Contour plot similar to the reference paper
        contour = ax.contourf(xi, yi, zi, levels=20, cmap='viridis', vmin=0, vmax=100)
        
        # Add scatter points so we can see where the signs actually were placed
        ax.scatter(x, y, color='red', alpha=0.3, s=10)

        ax.set_title(f"{ct} Second{'s' if ct > 1 else ''}")
        ax.set_xlabel('X')
        if i == 0:
            ax.set_ylabel('Y (SignPositionZ)')
        ax.set_aspect('equal')

    # Add a global colorbar
    if len(comp_times) > 0 and len(df_subset) >= 4:
        cbar = fig.colorbar(contour, ax=axes, orientation='vertical', fraction=0.02, pad=0.04)
        cbar.set_label('Visibility Ratio (%)')
        cbar.ax.yaxis.set_major_formatter(plt.FuncFormatter(lambda val, pos: f"{int(val)}%"))

    plt.suptitle(title_prefix)
    plt.savefig(os.path.join(output_dir, f"{filename_prefix}.png"), dpi=300, bbox_inches='tight')
    plt.close()
    print(f"Saved {filename_prefix}.png")

# Assuming Yaw ~ 0 or 180 is Horizontal, ~ 90 or 270 is Vertical
horizontal_df = agg_df[agg_df['SignOrientation'].isin([0.0, 180.0, 360.0])]
vertical_df = agg_df[agg_df['SignOrientation'].isin([90.0, 270.0])]

plot_heatmaps(horizontal_df, 'Horizontal Signs (Orientation ~ 0°)', 'heatmap_horizontal')
plot_heatmaps(vertical_df, 'Vertical Signs (Orientation ~ 90°)', 'heatmap_vertical')

print("=" * 80)
print("Analysis Complete.")
print("=" * 80)
