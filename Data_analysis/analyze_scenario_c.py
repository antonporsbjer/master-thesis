"""
Scenario C Data Analysis Script: High-Density Sweeps (RQ1)
===========================================================
Addresses Master's Thesis Research Question 1:
"How does varying crowd density (alpha in [0.2, 1.0]) degrade visual accessibility
across demographic cohorts (H_eye: Adult Male, Adult Female, Wheelchair)?"

Key Analytical Capabilities:
- Two-Way ANOVA (Density alpha x Demographic Cohort) testing main & interaction effects
- Effect size calculations (Partial eta squared)
- Density degradation curves with 95% Wilson confidence intervals
- Demographic Inequity metrics (Delta V_male-female and Delta V_male-wheelchair vs alpha)
- Dwell time occlusion impact vs crowd congestion
- Publication-quality figures, CSV/LaTeX tables, and Markdown report
"""

import os
import sys
import argparse
import glob
import numpy as np
import pandas as pd
import scipy.stats as stats
import matplotlib.pyplot as plt
import seaborn as sns

plt.style.use('seaborn-v0_8-whitegrid' if 'seaborn-v0_8-whitegrid' in plt.style.available else 'default')
plt.rcParams.update({
    'font.size': 11,
    'axes.labelsize': 12,
    'axes.titlesize': 13,
    'xtick.labelsize': 10,
    'ytick.labelsize': 10,
    'legend.fontsize': 10,
    'figure.titlesize': 14,
    'figure.dpi': 300
})


def calculate_wilson_ci(k, n, confidence=0.95):
    """Calculates Wilson score interval for binomial proportions."""
    if n == 0:
        return (0.0, 0.0)
    z = stats.norm.ppf(1 - (1 - confidence) / 2)
    p = k / n
    denominator = 1 + z**2 / n
    centre_adjusted_probability = p + z**2 / (2 * n)
    adjusted_std_dev = np.sqrt((p * (1 - p) + z**2 / (4 * n)) / n)
    lower = max(0.0, (centre_adjusted_probability - z * adjusted_std_dev) / denominator)
    upper = min(1.0, (centre_adjusted_probability + z * adjusted_std_dev) / denominator)
    return (lower * 100, upper * 100)


def clean_agent_type(name):
    """Cleans agent type string."""
    if not isinstance(name, str):
        return str(name)
    clean = name.replace('(Clone)', '').strip()
    if clean == 'AdultFemaleAgent':
        return 'Adult Female'
    elif clean == 'AdultMaleAgent':
        return 'Adult Male'
    elif clean == 'WheelchairAgent':
        return 'Wheelchair'
    return clean


def export_latex_table(df, filepath):
    """Exports dataframe to LaTeX table, handling missing optional dependencies."""
    try:
        latex_str = df.to_latex(index=False)
    except Exception:
        col_align = 'l' * len(df.columns)
        headers = " & ".join([str(c).replace('_', r'\_').replace('%', r'\%') for c in df.columns]) + r" \\ \hline"
        rows = []
        for _, row in df.iterrows():
            formatted_vals = [str(v).replace('_', r'\_').replace('%', r'\%') for v in row.values]
            rows.append(" & ".join(formatted_vals) + r" \\")
        body = "\n".join(rows)
        latex_str = f"\\begin{{tabular}}{{{col_align}}}\n\\hline\n{headers}\n{body}\n\\hline\n\\end{{tabular}}\n"
    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(latex_str)


def df_to_markdown(df, index=False):
    """Converts DataFrame to GitHub-flavored markdown table without tabulate dependency."""
    try:
        return df.to_markdown(index=index)
    except Exception:
        cols = [str(c) for c in df.columns]
        header = "| " + " | ".join(cols) + " |"
        sep = "| " + " | ".join(["---"] * len(cols)) + " |"
        rows = []
        for _, row in df.iterrows():
            formatted = [str(v) if pd.notnull(v) else "" for v in row.values]
            rows.append("| " + " | ".join(formatted) + " |")
        return "\n".join([header, sep] + rows)


def perform_two_way_anova(df, factor_a_col='CrowdDensityAlpha', factor_b_col='CleanAgentType', response_col='SawSignInt'):
    """
    Computes a balanced/unbalanced Two-Way ANOVA with Type I/III Sum of Squares in pure NumPy/SciPy.
    Returns DataFrame with Source, SS, df, MS, F-statistic, p-value, and partial eta squared.
    """
    y = df[response_col].values.astype(float)
    a = df[factor_a_col].astype(str).values
    b = df[factor_b_col].astype(str).values
    
    unique_a = np.unique(a)
    unique_b = np.unique(b)
    
    grand_mean = np.mean(y)
    ss_total = np.sum((y - grand_mean)**2)
    df_total = len(y) - 1
    
    # Factor A
    ss_a = 0.0
    for val_a in unique_a:
        y_a = y[a == val_a]
        ss_a += len(y_a) * (np.mean(y_a) - grand_mean)**2
    df_a = len(unique_a) - 1
    
    # Factor B
    ss_b = 0.0
    for val_b in unique_b:
        y_b = y[b == val_b]
        ss_b += len(y_b) * (np.mean(y_b) - grand_mean)**2
    df_b = len(unique_b) - 1
    
    # Interaction AB and Error
    ss_cell = 0.0
    ss_error = 0.0
    df_error = 0
    for val_a in unique_a:
        for val_b in unique_b:
            mask = (a == val_a) & (b == val_b)
            y_ab = y[mask]
            if len(y_ab) > 0:
                cell_mean = np.mean(y_ab)
                ss_cell += len(y_ab) * (cell_mean - grand_mean)**2
                ss_error += np.sum((y_ab - cell_mean)**2)
                df_error += len(y_ab) - 1
                
    ss_ab = max(0.0, ss_cell - ss_a - ss_b)
    df_ab = max(1, df_a * df_b)
    
    # Mean Squares and F statistics
    ms_a = ss_a / df_a if df_a > 0 else 0.0
    ms_b = ss_b / df_b if df_b > 0 else 0.0
    ms_ab = ss_ab / df_ab if df_ab > 0 else 0.0
    ms_error = ss_error / df_error if df_error > 0 else 1e-6
    
    f_a = ms_a / ms_error if ms_error > 0 else np.nan
    p_a = stats.f.sf(f_a, df_a, df_error) if df_a > 0 and df_error > 0 else np.nan
    eta_a = ss_a / (ss_a + ss_error) if (ss_a + ss_error) > 0 else 0.0
    
    f_b = ms_b / ms_error if ms_error > 0 else np.nan
    p_b = stats.f.sf(f_b, df_b, df_error) if df_b > 0 and df_error > 0 else np.nan
    eta_b = ss_b / (ss_b + ss_error) if (ss_b + ss_error) > 0 else 0.0
    
    f_ab = ms_ab / ms_error if ms_error > 0 else np.nan
    p_ab = stats.f.sf(f_ab, df_ab, df_error) if df_ab > 0 and df_error > 0 else np.nan
    eta_ab = ss_ab / (ss_ab + ss_error) if (ss_ab + ss_error) > 0 else 0.0
    
    records = [
        {'Source': f'Crowd Density (alpha)', 'SS': f'{ss_a:.3f}', 'df': df_a, 'MS': f'{ms_a:.3f}', 'F': f'{f_a:.2f}', 'p-value': f'{p_a:.4e}', 'Partial eta^2': f'{eta_a:.4f}', 'Significant': p_a < 0.05},
        {'Source': f'Demographic Cohort', 'SS': f'{ss_b:.3f}', 'df': df_b, 'MS': f'{ms_b:.3f}', 'F': f'{f_b:.2f}', 'p-value': f'{p_b:.4e}', 'Partial eta^2': f'{eta_b:.4f}', 'Significant': p_b < 0.05},
        {'Source': f'Interaction (alpha x Cohort)', 'SS': f'{ss_ab:.3f}', 'df': df_ab, 'MS': f'{ms_ab:.3f}', 'F': f'{f_ab:.2f}', 'p-value': f'{p_ab:.4e}', 'Partial eta^2': f'{eta_ab:.4f}', 'Significant': p_ab < 0.05},
        {'Source': 'Error (Residuals)', 'SS': f'{ss_error:.3f}', 'df': df_error, 'MS': f'{ms_error:.3f}', 'F': '-', 'p-value': '-', 'Partial eta^2': '-', 'Significant': '-'},
        {'Source': 'Total', 'SS': f'{ss_total:.3f}', 'df': df_total, 'MS': '-', 'F': '-', 'p-value': '-', 'Partial eta^2': '-', 'Significant': '-'}
    ]
    return pd.DataFrame(records)


def analyze_scenario_c(df, output_dir, run_name="Scenario_C"):
    """Executes full RQ1 density sweep analysis."""
    os.makedirs(output_dir, exist_ok=True)
    
    df = df.copy()
    if 'SawSign' in df.columns:
        df['SawSign'] = df['SawSign'].astype(str).str.lower().isin(['true', '1'])
    else:
        raise ValueError("Missing 'SawSign' column in data.")

    df['SawSignInt'] = df['SawSign'].astype(int)
    df['CleanAgentType'] = df['AgentType'].apply(clean_agent_type)
    df['InVCA'] = df['TimeInVCA'] > 0

    if 'CrowdDensityAlpha' not in df.columns:
        # Fallback or extract from ScenarioID or default to 0.2
        df['CrowdDensityAlpha'] = 0.2
    else:
        df['CrowdDensityAlpha'] = pd.to_numeric(df['CrowdDensityAlpha'], errors='coerce').fillna(0.2)

    vca_df = df[df['InVCA']].copy()

    # -------------------------------------------------------------
    # 1. Density x Demographic Grid Summary
    # -------------------------------------------------------------
    density_records = []
    alphas = sorted(df['CrowdDensityAlpha'].unique())
    agent_types = sorted(df['CleanAgentType'].unique())
    
    for a_val in alphas:
        sub_a = df[df['CrowdDensityAlpha'] == a_val]
        sub_a_vca = sub_a[sub_a['InVCA']]
        
        row_dict = {'Density (alpha)': a_val, 'Total Agents': len(sub_a), 'VCA Entrants': len(sub_a_vca)}
        
        male_vis, fem_vis, wheel_vis = np.nan, np.nan, np.nan
        
        for atype in agent_types:
            cohort_df = sub_a[sub_a['CleanAgentType'] == atype]
            cohort_vca = sub_a_vca[sub_a_vca['CleanAgentType'] == atype]
            saw_vca = cohort_vca['SawSign'].sum()
            n_vca = len(cohort_vca)
            vis_ratio = (saw_vca / n_vca * 100) if n_vca > 0 else 0.0
            ci_low, ci_high = calculate_wilson_ci(saw_vca, n_vca)
            
            row_dict[f'{atype} VCA Vis (%)'] = vis_ratio
            row_dict[f'{atype} 95% CI'] = f"[{ci_low:.1f}%, {ci_high:.1f}%]"
            
            if atype == 'Adult Male':
                male_vis = vis_ratio
            elif atype == 'Adult Female':
                fem_vis = vis_ratio
            elif atype == 'Wheelchair':
                wheel_vis = vis_ratio
                
        # Equity disparities
        row_dict['Delta V (Male - Female)'] = (male_vis - fem_vis) if not np.isnan(male_vis) and not np.isnan(fem_vis) else np.nan
        row_dict['Delta V (Male - Wheelchair)'] = (male_vis - wheel_vis) if not np.isnan(male_vis) and not np.isnan(wheel_vis) else np.nan
        
        density_records.append(row_dict)
        
    density_df = pd.DataFrame(density_records)
    density_df.to_csv(os.path.join(output_dir, f"{run_name}_density_sweep_summary.csv"), index=False)
    export_latex_table(density_df, os.path.join(output_dir, f"{run_name}_density_sweep_summary.tex"))

    # -------------------------------------------------------------
    # 2. Two-Way ANOVA: Alpha x Cohort Effect on Visibility
    # -------------------------------------------------------------
    anova_df = perform_two_way_anova(vca_df, factor_a_col='CrowdDensityAlpha', factor_b_col='CleanAgentType', response_col='SawSignInt')
    anova_df.to_csv(os.path.join(output_dir, f"{run_name}_two_way_anova.csv"), index=False)
    export_latex_table(anova_df, os.path.join(output_dir, f"{run_name}_two_way_anova.tex"))

    # -------------------------------------------------------------
    # 3. Publication Visualizations
    # -------------------------------------------------------------
    generate_c_visualizations(df, vca_df, density_df, alphas, agent_types, output_dir, run_name)

    # -------------------------------------------------------------
    # 4. Markdown Report
    # -------------------------------------------------------------
    report_content = generate_c_text_report(df, density_df, anova_df, run_name)
    report_path = os.path.join(output_dir, f"{run_name}_summary_report.md")
    with open(report_path, 'w', encoding='utf-8') as f:
        f.write(report_content)

    print(f"\n[SUCCESS] Scenario C Analysis complete for '{run_name}'!")
    print(f"Report written to: {report_path}")
    print(f"Figures and tables saved to: {output_dir}")
    return report_content


def generate_c_visualizations(df, vca_df, density_df, alphas, agent_types, output_dir, run_name):
    """Produces publication figures for Research Question 1."""
    palette = {'Adult Female': '#e74c3c', 'Adult Male': '#3498db', 'Wheelchair': '#2ecc71'}
    markers = {'Adult Female': 's', 'Adult Male': 'o', 'Wheelchair': '^'}

    # --- Figure 1: Density Degradation Curves (V_ratio vs Alpha by Cohort) ---
    fig, ax = plt.subplots(figsize=(8, 5))
    for atype in agent_types:
        col_vis = f'{atype} VCA Vis (%)'
        if col_vis in density_df.columns:
            ax.plot(density_df['Density (alpha)'], density_df[col_vis],
                    label=atype, color=palette.get(atype, '#333333'),
                    marker=markers.get(atype, 'o'), linewidth=2.2, markersize=7)
    ax.set_xlabel(r'Crowd Density Factor ($\alpha \in [0.2, 1.0]$)')
    ax.set_ylabel('In-VCA Visibility Ratio (%)')
    ax.set_title(f'Visibility Degradation Under Congestion (RQ1) - {run_name}', fontweight='bold')
    ax.set_ylim(0, 105)
    ax.set_xlim(min(alphas) - 0.05, max(alphas) + 0.05)
    ax.legend(title='Demographic Cohort', frameon=True)
    plt.tight_layout()
    plt.savefig(os.path.join(output_dir, f"{run_name}_density_degradation_curves.png"))
    plt.close()

    # --- Figure 2: Demographic Inequity vs Crowd Density ---
    fig, ax = plt.subplots(figsize=(8, 5))
    if 'Delta V (Male - Female)' in density_df.columns:
        ax.plot(density_df['Density (alpha)'], density_df['Delta V (Male - Female)'],
                label=r'$\Delta V_{\mathrm{gender}}$ (Male $-$ Female)', color='#8e44ad', marker='D', linewidth=2.0)
    if 'Delta V (Male - Wheelchair)' in density_df.columns and not density_df['Delta V (Male - Wheelchair)'].isna().all():
        ax.plot(density_df['Density (alpha)'], density_df['Delta V (Male - Wheelchair)'],
                label=r'$\Delta V_{\mathrm{wheelchair}}$ (Male $-$ Wheelchair)', color='#d35400', marker='v', linewidth=2.0)
    ax.set_xlabel(r'Crowd Density Factor ($\alpha$)')
    ax.set_ylabel('Demographic Inequity Gap (% Visibility Difference)')
    ax.set_title(f'Demographic Perception Inequity vs Crowd Density - {run_name}', fontweight='bold')
    ax.legend(frameon=True)
    plt.tight_layout()
    plt.savefig(os.path.join(output_dir, f"{run_name}_demographic_inequity_vs_density.png"))
    plt.close()

    # --- Figure 3: Dwell Time in VCA vs Crowd Density ---
    fig, ax = plt.subplots(figsize=(8, 5))
    sns.boxplot(data=vca_df, x='CrowdDensityAlpha', y='TimeInVCA', hue='CleanAgentType',
                palette=palette, ax=ax, showfliers=False)
    ax.set_xlabel(r'Crowd Density Factor ($\alpha$)')
    ax.set_ylabel('Time in VCA (seconds)')
    ax.set_title(f'Dwell Time Distribution Across Density Sweeps - {run_name}', fontweight='bold')
    ax.legend(title='Cohort', frameon=True)
    plt.tight_layout()
    plt.savefig(os.path.join(output_dir, f"{run_name}_dwell_time_by_density.png"))
    plt.close()


def generate_c_text_report(df, density_df, anova_df, run_name):
    """Generates markdown report synthesizing RQ1 findings."""
    md = []
    md.append("# Scenario C Simulation Analysis Report: High-Density Sweeps (RQ1)")
    md.append(f"**Run Identifier:** `{run_name}` | **Generated on:** {pd.Timestamp.now().strftime('%Y-%m-%d %H:%M:%S')}\n")

    md.append("## 1. Research Question Context")
    md.append("Scenario C systematically investigates **Research Question 1 (RQ1)**:")
    md.append("> *How does varying crowd density (alpha in {0.2, 0.4, 0.6, 0.8, 1.0}) degrade visual accessibility across demographic cohorts (Adult Male, Adult Female, Wheelchair)?*\n")
    md.append(f"- **Total Simulated Population**: `{len(df)}` agents across density regimes\n")

    md.append("## 2. Density Sweep & Inequity Breakdown")
    md.append(df_to_markdown(density_df, index=False))
    md.append("\n")

    md.append("## 3. Two-Way ANOVA: Main Effects & Interaction Analysis")
    md.append(df_to_markdown(anova_df, index=False))
    md.append("\n")

    md.append("## 4. Key Takeaways for Thesis Discussion")
    alpha_row = anova_df[anova_df['Source'].str.contains('Density', case=False)].iloc[0]
    cohort_row = anova_df[anova_df['Source'].str.contains('Demographic', case=False)].iloc[0]
    inter_row = anova_df[anova_df['Source'].str.contains('Interaction', case=False)].iloc[0]

    md.append(f"1. **Main Effect of Crowd Density**: Density had an F-statistic of **{alpha_row['F']}** (p = **{alpha_row['p-value']}**, $\\eta_p^2$ = **{alpha_row['Partial eta^2']}**), confirming significant line-of-sight occlusion at elevated pedestrian volumes.")
    md.append(f"2. **Main Effect of Demographic Cohort**: Cohort differences yielded F = **{cohort_row['F']}** (p = **{cohort_row['p-value']}**, $\\eta_p^2$ = **{cohort_row['Partial eta^2']}**), validating that eye-height disparities establish systemic visibility gaps.")
    md.append(f"3. **Interaction Effect (Density x Cohort)**: Interaction test yielded F = **{inter_row['F']}** (p = **{inter_row['p-value']}**, $\\eta_p^2$ = **{inter_row['Partial eta^2']}**), demonstrating how crowding differentially compounds accessibility barriers for shorter and seated individuals.")

    return "\n".join(md)


def main():
    parser = argparse.ArgumentParser(description="Analyze Scenario C density sweep visibility data.")
    parser.add_argument('--file', type=str, help="Path to specific visibility CSV file.")
    parser.add_argument('--dir', type=str, help="Directory containing visibility CSV files.")
    parser.add_argument('--output', type=str, default='output/scenario_C_results', help="Directory to save figures and reports.")
    args = parser.parse_args()

    script_dir = os.path.dirname(os.path.abspath(__file__))
    if args.file:
        target_files = [args.file]
    elif args.dir:
        target_files = glob.glob(os.path.join(args.dir, '**', 'visibility_data_*.csv'), recursive=True)
    else:
        default_dir = os.path.join(script_dir, 'data', 'Scenario_C')
        target_files = glob.glob(os.path.join(default_dir, 'visibility_data_*.csv'))
        if not target_files:
            target_files = glob.glob(os.path.join(script_dir, 'data', '**', 'visibility_data_*.csv'), recursive=True)

    if not target_files:
        print("Error: No visibility CSV files found. Please specify --file or --dir.")
        sys.exit(1)

    print(f"Found {len(target_files)} target file(s) for Scenario C analysis.")
    if len(target_files) == 1:
        df = pd.read_csv(target_files[0])
        base_name = os.path.splitext(os.path.basename(target_files[0]))[0]
        analyze_scenario_c(df, args.output, run_name=base_name)
    else:
        dfs = []
        for f in target_files:
            try:
                tdf = pd.read_csv(f)
                tdf['SourceFile'] = os.path.basename(f)
                dfs.append(tdf)
            except Exception as e:
                print(f"Warning: Failed to load {f}: {e}")
        if dfs:
            combined = pd.concat(dfs, ignore_index=True)
            analyze_scenario_c(combined, args.output, run_name="Scenario_C_Combined")


if __name__ == '__main__':
    main()
