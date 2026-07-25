import os
import glob
import pandas as pd
import numpy as np
from scipy.interpolate import griddata
import matplotlib.pyplot as plt
import seaborn as sns

def main():
    script_dir = os.path.dirname(os.path.abspath(__file__))
    data_dir = os.path.join(script_dir, 'data')
    output_dir = os.path.join(script_dir, 'output')
    
    if not os.path.exists(output_dir):
        os.makedirs(output_dir)

    # Find all visibility CSV files recursively in data_dir (including 1sec, 2sec, 3sec subfolders)
    csv_files = glob.glob(os.path.join(data_dir, '**', 'visibility_data_*.csv'), recursive=True)
    if not csv_files:
        csv_files = glob.glob(os.path.join(data_dir, 'visibility_data_*.csv'))
    
    csv_files = sorted(csv_files, key=os.path.getmtime)
    print(f"Found {len(csv_files)} visibility data files.")
    
    # Required columns for analysis
    cols_to_use = ['RunIndex', 'TotalAgents', 'SignPositionX', 'SignPositionZ', 'SawSign', 'EyeHeight', 'TimeInVCA', 'AgentType', 'SignComprehensionTime']
    
    df_list = []
    for f in csv_files:
        try:
            temp_df = pd.read_csv(f, usecols=cols_to_use)
            df_list.append(temp_df)
        except Exception as e:
            print(f"Error reading {f}: {e}")
            
    if not df_list:
        print("No data could be read.")
        return
        
    df = pd.concat(df_list, ignore_index=True)
    print(f"Total rows loaded: {len(df)}")
    
    # Preprocess data
    # Convert SawSign to numeric (0 or 1)
    df['SawSign'] = df['SawSign'].astype(str).str.lower() == 'true'
    df['SawSignNumeric'] = df['SawSign'].astype(int)
    
    # 1. Infer Density Bins dynamically from RunIndex
    print("Binning Density Groups by RunIndex...")
    run_indices = sorted(df['RunIndex'].dropna().unique())
    num_runs = len(run_indices)
    print(f"Total unique runs found: {num_runs}")
    
    # Map RunIndex to descriptive density names for 6-density setups
    default_names = {
        1: 'Density 1 (Very Low)',
        2: 'Density 2 (Low)',
        3: 'Density 3 (Medium)',
        4: 'Density 4 (High)',
        5: 'Density 5 (Very High)',
        6: 'Density 6 (Extreme)'
    }
    
    def assign_density(r_idx):
        return default_names.get(int(r_idx), f'Density_{int(r_idx)}')
            
    df['DensityGroup'] = df['RunIndex'].apply(assign_density)
    
    density_categories = [assign_density(r) for r in run_indices]
    df['DensityGroup'] = pd.Categorical(df['DensityGroup'], categories=density_categories, ordered=True)

    print("Agent Row Distribution per DensityGroup:")
    print(df['DensityGroup'].value_counts())
    
    run_totals = df.groupby(['RunIndex', 'DensityGroup'], observed=False)['TotalAgents'].first().reset_index()
    print("Median TotalAgents per Run in each DensityGroup:")
    print(run_totals.groupby('DensityGroup', observed=False)['TotalAgents'].median())

    # Filter to agents that entered the VCA for fair visibility evaluation
    vca_df = df[df['TimeInVCA'] > 0].copy()

    # 2. Generate Heatmaps for each Density Group
    print("Generating Heatmaps...")
    for density_group in df['DensityGroup'].dropna().unique():
        sub_df = vca_df[vca_df['DensityGroup'] == density_group]
        if sub_df.empty:
            continue
            
        # Group by SignPositionX, SignPositionZ to get average VCA visibility ratio (%)
        heatmap_data = sub_df.groupby(['SignPositionX', 'SignPositionZ'])['SawSignNumeric'].mean().reset_index()
        heatmap_data['VisibilityRatio'] = heatmap_data['SawSignNumeric'] * 100
        
        if len(heatmap_data) >= 3:
            x = heatmap_data['SignPositionX']
            y = heatmap_data['SignPositionZ']
            z = heatmap_data['VisibilityRatio']

            # Create grid for smooth 2D interpolation
            xi = np.linspace(x.min(), x.max(), 100)
            yi = np.linspace(y.min(), y.max(), 100)
            xi, yi = np.meshgrid(xi, yi)

            # Interpolate Z values onto grid (cubic with linear/nearest fallback)
            zi = griddata((x, y), z, (xi, yi), method='linear')
            if np.isnan(zi).all():
                zi = griddata((x, y), z, (xi, yi), method='nearest')

            plt.figure(figsize=(10, 8))
            contour = plt.contourf(xi, yi, zi, levels=20, cmap='viridis', vmin=0, vmax=100)
            cbar = plt.colorbar(contour)
            cbar.set_label('VCA Visibility Ratio (%)')
            
            # Scatter sampled sign positions
            plt.scatter(x, y, color='red', alpha=0.5, s=20, label='Sign Positions (336 Grid)')
            
            median_agents = run_totals[run_totals['DensityGroup'] == density_group]['TotalAgents'].median()
            plt.title(f'Sign Visibility Heatmap - {density_group}\n(Total Agents: {median_agents:.0f})')
            plt.xlabel('Sign Position X (m)')
            plt.ylabel('Sign Position Z (m)')
            plt.legend(loc='upper right')
            plt.grid(True, linestyle='--', alpha=0.3)
            
            safe_name = str(density_group).lower().replace(' ', '_').replace('(', '').replace(')', '')
            output_path = os.path.join(output_dir, f'heatmap_{safe_name}.png')
            plt.savefig(output_path, dpi=300, bbox_inches='tight')
            plt.close()
            print(f"Saved {output_path}")
            
    # 3. Height Group Line Chart Analysis
    print("Generating Height Group Analysis...")
    vca_df['EyeHeightRounded'] = vca_df['EyeHeight'].round(2).astype(str) + 'm'
    
    height_group_data = vca_df.groupby(['DensityGroup', 'EyeHeightRounded'], observed=False)['SawSignNumeric'].mean().reset_index()
    height_group_data['VisibilityPct'] = height_group_data['SawSignNumeric'] * 100
    
    plt.figure(figsize=(12, 6))
    ax = sns.lineplot(
        data=height_group_data,
        x='DensityGroup',
        y='VisibilityPct',
        hue='EyeHeightRounded',
        marker='o',
        linewidth=2.5,
        markersize=9
    )
    
    for line in ax.lines:
        for x_val, y_val in zip(line.get_xdata(), line.get_ydata()):
            if pd.notna(y_val) and not pd.isna(x_val):
                ax.annotate(f'{y_val:.1f}%', xy=(x_val, y_val), xytext=(0, 8), textcoords='offset points', ha='center', va='bottom', fontsize=9, fontweight='bold')

    plt.title('VCA Visibility Ratio vs Crowd Density by Demographic Eye Height')
    plt.xlabel('Crowd Density Level')
    plt.ylabel('VCA Visibility Ratio (%)')
    plt.ylim(0, 105)
    plt.grid(True, alpha=0.3)
    plt.xticks(rotation=15)
    
    output_path = os.path.join(output_dir, 'visibility_vs_density_by_demographic.png')
    plt.savefig(output_path, dpi=300, bbox_inches='tight')
    plt.close()
    print(f"Saved {output_path}")

    print("Data analysis complete!")

if __name__ == "__main__":
    main()
