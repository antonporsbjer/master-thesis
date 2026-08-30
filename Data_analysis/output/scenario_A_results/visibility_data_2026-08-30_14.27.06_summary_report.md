# Simulation Analysis Report: visibility_data_2026-08-30_14.27.06
**Generated on:** 2026-08-30 14:40:46

## 1. Scenario & Environment Parameters
- **Scenario ID**: `scenario-A-1_scenario`
- **Sign Location**: `(-3.29, -12.53)` at Height `3m`
- **Required Comprehension Time**: `1s`
- **Total Population Size**: `1084` agents

## 2. Key Visibility Ratios
| Metric | Value | 95% Confidence Interval |
| :--- | :--- | :--- |
| **Overall Population Visibility Ratio** | **29.89%** (324/1084) | [27.24%, 32.68%] |
| **In-VCA Visibility Ratio (Active Sightline)** | **42.80%** (324/757) | [39.32%, 46.35%] |
| **VCA Penetration Rate** | **69.83%** (757/1084) | - |

## 3. Demographic & Agent Type Disparities
| Agent Type   |   Total Agents |   Total Saw Sign |   Overall Visibility (%) | Overall 95% CI   |   Agents in VCA |   VCA Saw Sign |   VCA Visibility (%) | VCA 95% CI     |   Eye Height (m) |   Mean Time in VCA (s) |   Median Time in VCA (s) |
|:-------------|---------------:|-----------------:|-------------------------:|:-----------------|----------------:|---------------:|---------------------:|:---------------|-----------------:|-----------------------:|-------------------------:|
| Adult Female |            548 |              137 |                  25      | [21.6%, 28.8%]   |             373 |            137 |              36.7292 | [32.0%, 41.7%] |             1.45 |                10.2491 |                  8.88191 |
| Adult Male   |            536 |              187 |                  34.8881 | [31.0%, 39.0%]   |             384 |            187 |              48.6979 | [43.7%, 53.7%] |             1.58 |                10.5168 |                  9.37654 |


### Statistical Significance of Demographic Disparities
| Comparison                 |   VCA Diff (%) |   Chi2 Stat |   p-value (Chi2) |   Odds Ratio |   p-value (Fisher) | Significant (p < 0.05)   |
|:---------------------------|---------------:|------------:|-----------------:|-------------:|-------------------:|:-------------------------|
| Adult Female vs Adult Male |       -11.9687 |     10.5878 |       0.00113839 |     0.611552 |        0.000948305 | True                     |


## 4. Exposure & Dwell Time Analysis (Time in VCA)
| CleanAgentType   | SawSign   |   Count |     Mean |     Std |   Median |     IQR |       Min |     Max |
|:-----------------|:----------|--------:|---------:|--------:|---------:|--------:|----------:|--------:|
| Adult Female     | False     |     236 |  6.87244 | 4.17625 |  5.71527 | 4.96597 | 0.139764  | 18.4579 |
| Adult Female     | True      |     137 | 16.0658  | 4.88341 | 16.1696  | 7.80961 | 4.09294   | 25.0721 |
| Adult Male       | False     |     197 |  6.34855 | 3.71575 |  5.9895  | 3.99086 | 0.0789321 | 17.9941 |
| Adult Male       | True      |     187 | 14.9079  | 4.61772 | 15.3412  | 7.36597 | 2.30537   | 25.836  |


## 5. Corridors & Navigation Routes
| Route    |   TotalInVCA |   SawSignCount |   VisibilityRatio |   MeanTimeInVCA |
|:---------|-------------:|---------------:|------------------:|----------------:|
| 4 -> 12  |           59 |             43 |          72.8814  |        13.7014  |
| 0 -> 10  |           47 |             35 |          74.4681  |        14.6335  |
| 0 -> 11  |           47 |              0 |           0       |         5.19128 |
| 2 -> 11  |           43 |             34 |          79.0698  |        15.428   |
| 4 -> 11  |           41 |              2 |           4.87805 |         5.04782 |
| 5 -> 11  |           41 |              0 |           0       |         8.73488 |
| 2 -> 12  |           38 |             14 |          36.8421  |        12.1268  |
| 2 -> 10  |           36 |              3 |           8.33333 |        10.3344  |
| 5 -> 10  |           34 |             25 |          73.5294  |        12.3238  |
| 12 -> 10 |           32 |             19 |          59.375   |        10.6404  |


## 6. Key Research Takeaways for Thesis
1. **Demographic Occlusion Disparity**: Adult Males achieved a **48.70%** in-VCA visibility ratio compared to **36.73%** for Adult Females (a **+11.97% difference**).
   - Eye height difference (+0.13m) contributes directly to line-of-sight occlusion in crowded conditions.
2. **Dwell Time Impact**: Agents who successfully comprehended the sign spent on average **15.40s** in the VCA, compared to **6.63s** for agents who missed it.
3. **Corridor Vulnerability**: Routes with acute approach angles or shorter dwell durations exhibited marked drops in detection rates.