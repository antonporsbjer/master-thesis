import os
import glob
import pandas as pd
import matplotlib.pyplot as plt
import seaborn as sns

def main():
    data_dir = 'data'
    output_dir = 'output'
    
    if not os.path.exists(output_dir):
        os.makedirs(output_dir)

    print("Looking for CSV files...")
    csv_files = glob.glob(os.path.join(data_dir, 'visibility_data_*.csv'))
    
    if not csv_files:
        print("No visibility_data CSV files found in the data folder.")
        return

    print(f"Found {len(csv_files)} files. Reading data...")
    
    # We only need specific columns for our analysis to save memory with massive files
    cols_to_use = ['RunIndex', 'TotalAgents', 'SignPositionX', 'SignPositionZ', 'SawSign', 'EyeHeight']
    
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
    
    # 1. Infer Density Bins from RunIndex
    print("Binning Density Groups by RunIndex...")
    # Get unique RunIndex and sort them
    run_indices = sorted(df['RunIndex'].dropna().unique())
    num_runs = len(run_indices)
    print(f"Total unique runs found: {num_runs}")
    
    density_names = ['Very Low', 'Low', 'Medium', 'High', 'Extreme', 'Crush']
    
    def assign_density(r_idx):
        try:
            idx = run_indices.index(r_idx)
            if idx < len(density_names):
                return density_names[idx]
            else:
                return f'Density_{idx+1}'
        except ValueError:
            return 'Unknown'
            
    df['DensityGroup'] = df['RunIndex'].apply(assign_density)
        
    actual_categories = [assign_density(r) for r in run_indices]
    df['DensityGroup'] = pd.Categorical(df['DensityGroup'], categories=actual_categories, ordered=True)

    print("Agent Row Distribution per DensityGroup:")
    print(df['DensityGroup'].value_counts())
    
    # Also print the median agents per run for each group to verify
    run_totals = df.groupby(['RunIndex', 'DensityGroup'], observed=False)['TotalAgents'].first().reset_index()
    print("Median TotalAgents per Run in each DensityGroup:")
    print(run_totals.groupby('DensityGroup', observed=False)['TotalAgents'].median())

    # 2. Generate Heatmaps
    print("Generating Heatmaps...")
    for density_group in df['DensityGroup'].dropna().unique():
        sub_df = df[df['DensityGroup'] == density_group]
        if sub_df.empty:
            continue
            
        # Group by SignPositionX, SignPositionZ to get average visibility
        heatmap_data = sub_df.groupby(['SignPositionZ', 'SignPositionX'])['SawSignNumeric'].mean().reset_index()
        
        # Pivot for seaborn
        if not heatmap_data.empty:
            pivot_table = heatmap_data.pivot(index='SignPositionZ', columns='SignPositionX', values='SawSignNumeric')
            
            # Sort index to have Z descending
            pivot_table = pivot_table.sort_index(ascending=False)
            
            plt.figure(figsize=(10, 8))
            sns.heatmap(pivot_table, cmap='viridis', annot=False, vmin=0, vmax=1)
            
            # Get median agent count for this group specifically
            median_agents = run_totals[run_totals['DensityGroup'] == density_group]['TotalAgents'].median()
            plt.title(f'Sign Visibility Heatmap - {density_group} Density\n(Median Total Agents: {median_agents:.0f})')
            plt.xlabel('Sign Position X')
            plt.ylabel('Sign Position Z')
            
            output_path = os.path.join(output_dir, f'heatmap_density_{density_group.lower()}.png')
            plt.savefig(output_path, dpi=300, bbox_inches='tight')
            plt.close()
            print(f"Saved {output_path}")
            
    # 3. Height Group Analysis
    print("Generating Height Group Analysis...")
    df['EyeHeightRounded'] = df['EyeHeight'].round(2).astype(str) + 'm'
    
    height_group_data = df.groupby(['DensityGroup', 'EyeHeightRounded'], observed=False)['SawSignNumeric'].mean().reset_index()
    
    plt.figure(figsize=(10, 6))
    ax = sns.lineplot(data=height_group_data, x='DensityGroup', y='SawSignNumeric', hue='EyeHeightRounded', marker='o', linewidth=2, markersize=8)
    
    # Add text annotations to the points
    for line in ax.lines:
        for x, y in zip(line.get_xdata(), line.get_ydata()):
            if pd.notna(y) and not pd.isna(x):
                # Only annotate the actual data lines (seaborn sometimes adds extra dummy lines for legends)
                if len(line.get_xdata()) > 0:
                    ax.annotate(f'{y:.2f}', xy=(x, y), xytext=(0, 8), textcoords='offset points', ha='center', va='bottom', fontsize=9, fontweight='bold')

    plt.title('Visibility Ratio vs Crowd Density by Agent Eye Height')
    plt.xlabel('Crowd Density')
    plt.ylabel('Visibility Ratio')
    plt.ylim(0, 1)
    plt.grid(True, alpha=0.3)
    
    output_path = os.path.join(output_dir, 'visibility_vs_height.png')
    plt.savefig(output_path, dpi=300, bbox_inches='tight')
    plt.close()
    print(f"Saved {output_path}")

    print("Data analysis complete!")

if __name__ == "__main__":
    main()
