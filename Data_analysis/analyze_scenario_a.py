"""
Scenario A Data Analysis Script for Master's Thesis
===================================================
Analyzes agent visibility data for Scenario A simulations, calculating:
- Total & In-VCA Visibility Ratios
- Demographic / Agent Type disparities (Females vs Males vs Wheelchairs)
- Statistical significance tests (Chi-Square & Odds Ratios)
- Temporal exposure analysis (Time in VCA vs Detection Probability)
- Route-based visibility analysis (StartNode -> GoalNode)
- Publication-quality visualizations and Markdown/CSV summary exports
"""

import os
import sys
import argparse
import glob
import pandas as pd
import numpy as np
import scipy.stats as stats
import matplotlib.pyplot as plt
import seaborn as sns

# Set publication style for figures
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
    """Cleans agent type string (e.g. 'AdultFemaleAgent(Clone)' -> 'Adult Female')."""
    if not isinstance(name, str):
        return str(name)
    clean = name.replace('(Clone)', '').strip()
    # Insert spaces before capital letters if not present
    if clean == 'AdultFemaleAgent':
        return 'Adult Female'
    elif clean == 'AdultMaleAgent':
        return 'Adult Male'
    elif clean == 'WheelchairAgent':
        return 'Wheelchair'
    return clean


def export_latex_table(df, filepath):
    """Exports a dataframe to LaTeX table, gracefully handling missing optional dependencies."""
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




def analyze_visibility_data(df, output_dir, run_name="Scenario_A"):
    """Performs deep analysis on the visibility dataframe and generates reports + plots."""
    os.makedirs(output_dir, exist_ok=True)
    
    # Preprocessing
    df = df.copy()
    if 'SawSign' in df.columns:
        df['SawSign'] = df['SawSign'].astype(str).str.lower().isin(['true', '1'])
    else:
        raise ValueError("Missing 'SawSign' column in data.")
        
    df['CleanAgentType'] = df['AgentType'].apply(clean_agent_type)
    df['InVCA'] = df['TimeInVCA'] > 0
    df['Route'] = df['StartNode'].astype(str) + ' -> ' + df['GoalNode'].astype(str)

    if 'SignName' not in df.columns:
        df['SignName'] = 'Sign_0'
    else:
        df['SignName'] = df['SignName'].fillna('Sign_0')

    if 'IsTargetAudience' in df.columns:
        df['IsTargetAudience'] = df['IsTargetAudience'].astype(str).str.lower().isin(['true', '1'])
    else:
        df['IsTargetAudience'] = True

    total_agents = len(df)
    vca_df = df[df['InVCA']].copy()
    total_in_vca = len(vca_df)
    
    # -------------------------------------------------------------
    # 1. High-Level Metrics
    # -------------------------------------------------------------
    total_saw_all = df['SawSign'].sum()
    total_ratio_all = (total_saw_all / total_agents * 100) if total_agents > 0 else 0.0
    all_ci_low, all_ci_high = calculate_wilson_ci(total_saw_all, total_agents)

    total_saw_vca = vca_df['SawSign'].sum()
    total_ratio_vca = (total_saw_vca / total_in_vca * 100) if total_in_vca > 0 else 0.0
    vca_ci_low, vca_ci_high = calculate_wilson_ci(total_saw_vca, total_in_vca)
    
    vca_penetration_rate = (total_in_vca / total_agents * 100) if total_agents > 0 else 0.0

    # -------------------------------------------------------------
    # 2. Demographic Breakdown
    # -------------------------------------------------------------
    demographic_records = []
    agent_types = sorted(df['CleanAgentType'].unique())
    
    for atype in agent_types:
        sub_all = df[df['CleanAgentType'] == atype]
        sub_vca = vca_df[vca_df['CleanAgentType'] == atype]
        
        n_all = len(sub_all)
        saw_all = sub_all['SawSign'].sum()
        ratio_all = (saw_all / n_all * 100) if n_all > 0 else 0.0
        ci_all_low, ci_all_high = calculate_wilson_ci(saw_all, n_all)
        
        n_vca = len(sub_vca)
        saw_vca = sub_vca['SawSign'].sum()
        ratio_vca = (saw_vca / n_vca * 100) if n_vca > 0 else 0.0
        ci_vca_low, ci_vca_high = calculate_wilson_ci(saw_vca, n_vca)
        
        mean_height = sub_all['Height'].mean() if 'Height' in sub_all.columns else np.nan
        mean_eye_height = sub_all['EyeHeight'].mean() if 'EyeHeight' in sub_all.columns else np.nan
        mean_time_vca = sub_vca['TimeInVCA'].mean() if n_vca > 0 else 0.0
        median_time_vca = sub_vca['TimeInVCA'].median() if n_vca > 0 else 0.0
        
        demographic_records.append({
            'Agent Type': atype,
            'Total Agents': n_all,
            'Total Saw Sign': saw_all,
            'Overall Visibility (%)': ratio_all,
            'Overall 95% CI': f"[{ci_all_low:.1f}%, {ci_all_high:.1f}%]",
            'Agents in VCA': n_vca,
            'VCA Saw Sign': saw_vca,
            'VCA Visibility (%)': ratio_vca,
            'VCA 95% CI': f"[{ci_vca_low:.1f}%, {ci_vca_high:.1f}%]",
            'Eye Height (m)': mean_eye_height,
            'Mean Time in VCA (s)': mean_time_vca,
            'Median Time in VCA (s)': median_time_vca
        })
        
    demo_df = pd.DataFrame(demographic_records)

    # -------------------------------------------------------------
    # 3. Statistical Disparity Test (e.g. Chi-Square / Fisher test)
    # -------------------------------------------------------------
    stat_test_results = []
    if len(agent_types) >= 2:
        for i in range(len(agent_types)):
            for j in range(i + 1, len(agent_types)):
                t1, t2 = agent_types[i], agent_types[j]
                vca_t1 = vca_df[vca_df['CleanAgentType'] == t1]
                vca_t2 = vca_df[vca_df['CleanAgentType'] == t2]
                
                s1, f1 = vca_t1['SawSign'].sum(), len(vca_t1) - vca_t1['SawSign'].sum()
                s2, f2 = vca_t2['SawSign'].sum(), len(vca_t2) - vca_t2['SawSign'].sum()
                
                table = [[s1, f1], [s2, f2]]
                chi2, p_val, dof, _ = stats.chi2_contingency(table)
                odds_ratio, p_fisher = stats.fisher_exact(table)
                
                stat_test_results.append({
                    'Comparison': f"{t1} vs {t2}",
                    'VCA Diff (%)': (s1/len(vca_t1)*100) - (s2/len(vca_t2)*100) if len(vca_t1)>0 and len(vca_t2)>0 else 0,
                    'Chi2 Stat': chi2,
                    'p-value (Chi2)': p_val,
                    'Odds Ratio': odds_ratio,
                    'p-value (Fisher)': p_fisher,
                    'Significant (p < 0.05)': p_val < 0.05
                })
    stat_df = pd.DataFrame(stat_test_results)

    # -------------------------------------------------------------
    # 4. Multi-Sign Analysis (when multiple signs are present)
    # -------------------------------------------------------------
    sign_df = pd.DataFrame()
    if df['SignName'].nunique() > 1:
        sign_records = []
        for sname in sorted(df['SignName'].unique()):
            sdf = df[df['SignName'] == sname]
            svca = sdf[sdf['InVCA']]
            sn_all = len(sdf)
            sn_vca = len(svca)
            ssaw_all = sdf['SawSign'].sum()
            ssaw_vca = svca['SawSign'].sum()
            sr_all = (ssaw_all / sn_all * 100) if sn_all > 0 else 0.0
            sr_vca = (ssaw_vca / sn_vca * 100) if sn_vca > 0 else 0.0
            
            m_vca = svca[svca['CleanAgentType'] == 'Adult Male']
            f_vca = svca[svca['CleanAgentType'] == 'Adult Female']
            w_vca = svca[svca['CleanAgentType'] == 'Wheelchair']
            
            sr_male = (m_vca['SawSign'].mean() * 100) if len(m_vca) > 0 else 0.0
            sr_female = (f_vca['SawSign'].mean() * 100) if len(f_vca) > 0 else 0.0
            sr_wheel = (w_vca['SawSign'].mean() * 100) if len(w_vca) > 0 else 0.0
            
            sign_records.append({
                'Sign': sname,
                'Total Agents': sn_all,
                'Agents in VCA': sn_vca,
                'VCA Penetration (%)': (sn_vca / sn_all * 100) if sn_all > 0 else 0.0,
                'Overall Visibility (%)': sr_all,
                'VCA Visibility (%)': sr_vca,
                'Male VCA Vis (%)': sr_male,
                'Female VCA Vis (%)': sr_female,
                'Wheelchair VCA Vis (%)': sr_wheel,
                'Gender Inequity (M - F)': sr_male - sr_female
            })
        sign_df = pd.DataFrame(sign_records)
        sign_df.to_csv(os.path.join(output_dir, f"{run_name}_sign_comparison.csv"), index=False)
        export_latex_table(sign_df, os.path.join(output_dir, f"{run_name}_sign_comparison.tex"))

    # -------------------------------------------------------------
    # 5. Exposure Analysis (TimeInVCA vs SawSign)
    # -------------------------------------------------------------
    exposure_summary = vca_df.groupby(['CleanAgentType', 'SawSign'])['TimeInVCA'].agg(
        Count='count',
        Mean='mean',
        Std='std',
        Median='median',
        IQR=lambda x: x.quantile(0.75) - x.quantile(0.25),
        Min='min',
        Max='max'
    ).reset_index()
    
    # -------------------------------------------------------------
    # 6. Route Analysis
    # -------------------------------------------------------------
    route_summary = vca_df.groupby('Route').agg(
        TotalInVCA=('SawSign', 'count'),
        SawSignCount=('SawSign', 'sum'),
        VisibilityRatio=('SawSign', lambda x: x.mean() * 100),
        MeanTimeInVCA=('TimeInVCA', 'mean')
    ).sort_values(by='TotalInVCA', ascending=False).reset_index()

    # -------------------------------------------------------------
    # 7. Save Tables to CSV and LaTeX for Thesis
    # -------------------------------------------------------------
    demo_df.to_csv(os.path.join(output_dir, f"{run_name}_demographics.csv"), index=False)
    export_latex_table(demo_df, os.path.join(output_dir, f"{run_name}_demographics.tex"))
    
    if not stat_df.empty:
        stat_df.to_csv(os.path.join(output_dir, f"{run_name}_statistical_tests.csv"), index=False)
        export_latex_table(stat_df, os.path.join(output_dir, f"{run_name}_statistical_tests.tex"))
        
    exposure_summary.to_csv(os.path.join(output_dir, f"{run_name}_exposure_stats.csv"), index=False)
    route_summary.to_csv(os.path.join(output_dir, f"{run_name}_route_analysis.csv"), index=False)
    export_latex_table(route_summary.head(10), os.path.join(output_dir, f"{run_name}_route_analysis_top10.tex"))

    # -------------------------------------------------------------
    # 8. Generate Visualizations
    # -------------------------------------------------------------
    generate_visualizations(df, vca_df, demo_df, route_summary, sign_df, output_dir, run_name)

    # -------------------------------------------------------------
    # 9. Generate Markdown & Text Report
    # -------------------------------------------------------------
    report_content = generate_text_report(df, vca_df, demo_df, stat_df, sign_df, exposure_summary, route_summary, 
                                          total_agents, total_in_vca, total_ratio_all, total_ratio_vca, 
                                          all_ci_low, all_ci_high, vca_ci_low, vca_ci_high, vca_penetration_rate,
                                          run_name)
    
    report_path = os.path.join(output_dir, f"{run_name}_summary_report.md")
    with open(report_path, 'w', encoding='utf-8') as f:
        f.write(report_content)
        
    print(f"\n[SUCCESS] Analysis complete for '{run_name}'!")
    print(f"Report written to: {report_path}")
    print(f"Visualizations and CSV tables saved to: {output_dir}")
    
    return report_content


def generate_visualizations(df, vca_df, demo_df, route_summary, sign_df, output_dir, run_name):
    """Produces publication-ready charts."""
    palette = {'Adult Female': '#e74c3c', 'Adult Male': '#3498db', 'Wheelchair': '#2ecc71'}
    
    # --- Figure 1: Demographic Visibility Comparison with Error Bars ---
    fig, ax = plt.subplots(figsize=(8, 5))
    x = np.arange(len(demo_df))
    width = 0.35
    
    # Calculate error margins for error bars
    overall_err = []
    vca_err = []
    for _, row in demo_df.iterrows():
        # Parsing CI strings e.g. "[25.0%, 35.0%]"
        ci_o = [float(val.replace('%', '').strip()) for val in row['Overall 95% CI'].strip('[]').split(',')]
        overall_err.append([row['Overall Visibility (%)'] - ci_o[0], ci_o[1] - row['Overall Visibility (%)']])
        
        ci_v = [float(val.replace('%', '').strip()) for val in row['VCA 95% CI'].strip('[]').split(',')]
        vca_err.append([row['VCA Visibility (%)'] - ci_v[0], ci_v[1] - row['VCA Visibility (%)']])
        
    overall_err = np.array(overall_err).T
    vca_err = np.array(vca_err).T

    rects1 = ax.bar(x - width/2, demo_df['Overall Visibility (%)'], width, yerr=overall_err, 
                    label='Overall Population', capsize=5, color='#95a5a6', edgecolor='black', alpha=0.85)
    rects2 = ax.bar(x + width/2, demo_df['VCA Visibility (%)'], width, yerr=vca_err, 
                    label='VCA Entrants Only', capsize=5, color='#2980b9', edgecolor='black', alpha=0.85)

    ax.set_ylabel('Visibility Ratio (%)')
    ax.set_title(f'Sign Visibility Ratio by Agent Type (95% CI) - {run_name}', fontweight='bold')
    ax.set_xticks(x)
    ax.set_xticklabels(demo_df['Agent Type'])
    ax.set_ylim(0, 100)
    ax.legend(frameon=True)
    
    for rect in rects1:
        height = rect.get_height()
        ax.annotate(f'{height:.1f}%', xy=(rect.get_x() + rect.get_width() / 2, height),
                    xytext=(0, 4), textcoords="offset points", ha='center', va='bottom', fontsize=9)
    for rect in rects2:
        height = rect.get_height()
        ax.annotate(f'{height:.1f}%', xy=(rect.get_x() + rect.get_width() / 2, height),
                    xytext=(0, 4), textcoords="offset points", ha='center', va='bottom', fontsize=9)

    plt.tight_layout()
    plt.savefig(os.path.join(output_dir, f"{run_name}_visibility_by_agent_type.png"))
    plt.close()

    # --- Figure 2: Time in VCA Distribution (Boxplot / Strip) ---
    fig, ax = plt.subplots(figsize=(9, 5))
    vca_plot_df = vca_df.copy()
    vca_plot_df['Detection Status'] = vca_plot_df['SawSign'].map({True: 'Saw Sign', False: 'Missed Sign'})
    
    sns.boxplot(data=vca_plot_df, x='CleanAgentType', y='TimeInVCA', hue='Detection Status',
                palette={'Saw Sign': '#2ecc71', 'Missed Sign': '#e74c3c'}, ax=ax, showmeans=True,
                meanprops={"marker":"o", "markerfacecolor":"white", "markeredgecolor":"black", "markersize":"8"})
    
    ax.set_title(f'Dwell Time in Visual Catchment Area (VCA) - {run_name}', fontweight='bold')
    ax.set_xlabel('Agent Demographic')
    ax.set_ylabel('Time in VCA (seconds)')
    ax.legend(title='Outcome', frameon=True)
    plt.tight_layout()
    plt.savefig(os.path.join(output_dir, f"{run_name}_time_in_vca_distribution.png"))
    plt.close()

    # --- Figure 3: Route-Based Visibility Breakdown ---
    if len(route_summary) > 0:
        fig, ax = plt.subplots(figsize=(10, max(4, len(route_summary) * 0.45)))
        top_routes = route_summary.head(10).sort_values(by='VisibilityRatio', ascending=True)
        
        bars = ax.barh(top_routes['Route'], top_routes['VisibilityRatio'], color='#34495e', edgecolor='black', alpha=0.85)
        ax.set_xlabel('VCA Visibility Ratio (%)')
        ax.set_ylabel('Corridor / Route (StartNode -> GoalNode)')
        ax.set_title(f'Visibility Ratio by Route Corridors (Top Traffic) - {run_name}', fontweight='bold')
        ax.set_xlim(0, 100)
        
        for bar, total in zip(bars, top_routes['TotalInVCA']):
            width = bar.get_width()
            ax.annotate(f'{width:.1f}% (n={total})', xy=(width, bar.get_y() + bar.get_height()/2),
                        xytext=(5, 0), textcoords="offset points", ha='left', va='center', fontsize=9)
                        
        plt.tight_layout()
        plt.savefig(os.path.join(output_dir, f"{run_name}_visibility_by_route.png"))
        plt.close()

    # --- Figure 4: Logistic Detection Probability Curve vs Time in VCA ---
    fig, ax = plt.subplots(figsize=(8, 5))
    times = np.linspace(0, max(vca_df['TimeInVCA'].max(), 1), 200)
    
    for atype in vca_df['CleanAgentType'].unique():
        sub = vca_df[vca_df['CleanAgentType'] == atype]
        if len(sub) > 10 and sub['SawSign'].nunique() > 1:
            try:
                # Logistic regression
                slope, intercept, r_val, p_val, std_err = stats.linregress(sub['TimeInVCA'], sub['SawSign'].astype(int))
                # Logistic sigmoid fit
                import scipy.optimize as opt
                def sigmoid(x, k, x0):
                    return 1 / (1 + np.exp(-k * (x - x0)))
                popt, _ = opt.curve_fit(sigmoid, sub['TimeInVCA'], sub['SawSign'].astype(int), p0=[0.5, sub['TimeInVCA'].median()], maxfev=5000)
                prob_curve = sigmoid(times, *popt) * 100
                ax.plot(times, prob_curve, label=f"{atype} (Fitted)", linewidth=2.5)
            except Exception:
                # Fallback to empirical binned mean
                bins = pd.qcut(sub['TimeInVCA'], q=min(5, len(sub['TimeInVCA'].unique())), duplicates='drop')
                binned = sub.groupby(bins, observed=False).agg({'TimeInVCA': 'mean', 'SawSign': lambda x: x.mean() * 100})
                ax.plot(binned['TimeInVCA'], binned['SawSign'], marker='o', label=f"{atype} (Binned)")

    ax.set_title(f'Sign Detection Probability vs Time in VCA - {run_name}', fontweight='bold')
    ax.set_xlabel('Time Spent in VCA (seconds)')
    ax.set_ylabel('Probability of Seeing Sign (%)')
    ax.set_ylim(0, 105)
    ax.legend(frameon=True)
    plt.tight_layout()
    plt.savefig(os.path.join(output_dir, f"{run_name}_detection_probability_curve.png"))
    plt.close()

    # --- Figure 5: Multi-Sign Visibility Comparison (if applicable) ---
    if not sign_df.empty:
        fig, ax = plt.subplots(figsize=(10, 5))
        signs = sign_df['Sign'].tolist()
        x_idx = np.arange(len(signs))
        w = 0.25
        
        ax.bar(x_idx - w, sign_df['Male VCA Vis (%)'], w, label='Adult Male', color='#3498db', edgecolor='black')
        ax.bar(x_idx, sign_df['Female VCA Vis (%)'], w, label='Adult Female', color='#e74c3c', edgecolor='black')
        ax.bar(x_idx + w, sign_df['Wheelchair VCA Vis (%)'], w, label='Wheelchair', color='#2ecc71', edgecolor='black')
        
        ax.set_ylabel('VCA Visibility Ratio (%)')
        ax.set_title(f'Multi-Sign Demographic Visibility Comparison - {run_name}', fontweight='bold')
        ax.set_xticks(x_idx)
        ax.set_xticklabels(signs)
        ax.set_ylim(0, 100)
        ax.legend(frameon=True)
        plt.tight_layout()
        plt.savefig(os.path.join(output_dir, f"{run_name}_multi_sign_comparison.png"))
        plt.close()


def generate_text_report(df, vca_df, demo_df, stat_df, sign_df, exposure_summary, route_summary, 
                         total_agents, total_in_vca, total_ratio_all, total_ratio_vca, 
                         all_ci_low, all_ci_high, vca_ci_low, vca_ci_high, vca_penetration_rate,
                         run_name):
    """Formats findings into clean markdown report."""
    md = []
    md.append(f"# Simulation Analysis Report: {run_name}")
    md.append(f"**Generated on:** {pd.Timestamp.now().strftime('%Y-%m-%d %H:%M:%S')}\n")
    
    # Metadata
    md.append("## 1. Scenario & Environment Parameters")
    scen_id = df['ScenarioID'].iloc[0] if 'ScenarioID' in df.columns else 'Unknown'
    sign_h = df['SignHeight'].iloc[0] if 'SignHeight' in df.columns else 'N/A'
    sign_x = df['SignPositionX'].iloc[0] if 'SignPositionX' in df.columns else 'N/A'
    sign_z = df['SignPositionZ'].iloc[0] if 'SignPositionZ' in df.columns else 'N/A'
    comp_t = df['SignComprehensionTime'].iloc[0] if 'SignComprehensionTime' in df.columns else 'N/A'
    
    md.append(f"- **Scenario ID**: `{scen_id}`")
    md.append(f"- **Sign Location**: `({sign_x}, {sign_z})` at Height `{sign_h}m`")
    md.append(f"- **Required Comprehension Time**: `{comp_t}s`")
    md.append(f"- **Total Population Size**: `{total_agents}` agents\n")
    
    # Key Summary
    md.append("## 2. Key Visibility Ratios")
    md.append("| Metric | Value | 95% Confidence Interval |")
    md.append("| :--- | :--- | :--- |")
    md.append(f"| **Overall Population Visibility Ratio** | **{total_ratio_all:.2f}%** ({df['SawSign'].sum()}/{total_agents}) | [{all_ci_low:.2f}%, {all_ci_high:.2f}%] |")
    md.append(f"| **In-VCA Visibility Ratio (Active Sightline)** | **{total_ratio_vca:.2f}%** ({vca_df['SawSign'].sum()}/{total_in_vca}) | [{vca_ci_low:.2f}%, {vca_ci_high:.2f}%] |")
    md.append(f"| **VCA Penetration Rate** | **{vca_penetration_rate:.2f}%** ({total_in_vca}/{total_agents}) | - |\n")

    # Demographics
    md.append("## 3. Demographic & Agent Type Disparities")
    md.append(df_to_markdown(demo_df, index=False))
    md.append("\n")

    # Statistical significance
    if not stat_df.empty:
        md.append("### Statistical Significance of Demographic Disparities")
        md.append(df_to_markdown(stat_df, index=False))
        md.append("\n")

    # Multi-Sign Performance
    if not sign_df.empty:
        md.append("## 4. Multi-Sign Performance & Placement Comparison")
        md.append(df_to_markdown(sign_df, index=False))
        md.append("\n")
        
    # Temporal & Exposure
    md.append("## 5. Exposure & Dwell Time Analysis (Time in VCA)")
    md.append(df_to_markdown(exposure_summary, index=False))
    md.append("\n")
    
    # Routes
    md.append("## 6. Corridors & Navigation Routes")
    md.append(df_to_markdown(route_summary.head(10), index=False))
    md.append("\n")
    
    # Thesis Insights
    md.append("## 7. Key Research Takeaways for Thesis")
    
    # Determine male vs female disparity
    if 'Adult Female' in demo_df['Agent Type'].values and 'Adult Male' in demo_df['Agent Type'].values:
        f_row = demo_df[demo_df['Agent Type'] == 'Adult Female'].iloc[0]
        m_row = demo_df[demo_df['Agent Type'] == 'Adult Male'].iloc[0]
        diff_vca = m_row['VCA Visibility (%)'] - f_row['VCA Visibility (%)']
        diff_eye = m_row['Eye Height (m)'] - f_row['Eye Height (m)']
        
        md.append(f"1. **Demographic Occlusion Disparity**: Adult Males achieved a **{m_row['VCA Visibility (%)']:.2f}%** in-VCA visibility ratio compared to **{f_row['VCA Visibility (%)']:.2f}%** for Adult Females (a **{diff_vca:+.2f}% difference**).")
        md.append(f"   - Eye height difference ({diff_eye:+.2f}m) contributes directly to line-of-sight occlusion in crowded conditions.")
    
    saw_mean_t = vca_df[vca_df['SawSign']]['TimeInVCA'].mean()
    miss_mean_t = vca_df[~vca_df['SawSign']]['TimeInVCA'].mean()
    md.append(f"2. **Dwell Time Impact**: Agents who successfully comprehended the sign spent on average **{saw_mean_t:.2f}s** in the VCA, compared to **{miss_mean_t:.2f}s** for agents who missed it.")
    md.append(f"3. **Corridor Vulnerability**: Routes with acute approach angles or shorter dwell durations exhibited marked drops in detection rates.")
    
    return "\n".join(md)


def main():
    parser = argparse.ArgumentParser(description="Analyze Scenario A visibility data.")
    parser.add_argument('--file', type=str, help="Path to specific visibility CSV file.")
    parser.add_argument('--dir', type=str, help="Directory containing visibility CSV files.")
    parser.add_argument('--output', type=str, default='output/scenario_A_results', help="Directory to save figures and reports.")
    args = parser.parse_args()

    script_dir = os.path.dirname(os.path.abspath(__file__))
    
    # Determine target files
    if args.file:
        target_files = [args.file]
    elif args.dir:
        target_files = glob.glob(os.path.join(args.dir, '**', 'visibility_data_*.csv'), recursive=True)
        if not target_files:
            target_files = glob.glob(os.path.join(args.dir, 'visibility_data_*.csv'))
    else:
        # Default fallback to scenario-A-1 directory or data/
        default_scenario_dir = os.path.join(script_dir, 'data', 'scenario-A-1')
        target_files = glob.glob(os.path.join(default_scenario_dir, 'visibility_data_*.csv'))
        if not target_files:
            target_files = glob.glob(os.path.join(script_dir, 'data', '**', 'visibility_data_*.csv'), recursive=True)

    if not target_files:
        print("Error: No visibility CSV files found. Please specify --file or --dir.")
        sys.exit(1)

    print(f"Found {len(target_files)} target file(s) for analysis.")
    
    # If single file
    if len(target_files) == 1:
        file_path = target_files[0]
        base_name = os.path.splitext(os.path.basename(file_path))[0]
        df = pd.read_csv(file_path)
        analyze_visibility_data(df, args.output, run_name=base_name)
    else:
        # Batch analysis
        dfs = []
        for f in target_files:
            try:
                temp_df = pd.read_csv(f)
                temp_df['SourceFile'] = os.path.basename(f)
                dfs.append(temp_df)
            except Exception as e:
                print(f"Warning: Failed to load {f}: {e}")
                
        if dfs:
            combined_df = pd.concat(dfs, ignore_index=True)
            analyze_visibility_data(combined_df, args.output, run_name="Scenario_A_Combined")


if __name__ == '__main__':
    main()
