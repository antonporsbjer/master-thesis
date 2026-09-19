# Simulation Analysis Report: visibility_data_2026-09-12_14.48.53
**Generated on:** 2026-09-12 15:08:32

## 1. Scenario & Environment Parameters
- **Scenario ID**: `Scenario_A1_scenario`
- **Sign Location**: `(-3.29, -15.99)` at Height `3m`
- **Required Comprehension Time**: `1s`
- **Total Population Size**: `5160` agents

## 2. Key Visibility Ratios
| Metric | Value | 95% Confidence Interval |
| :--- | :--- | :--- |
| **Overall Population Visibility Ratio** | **25.25%** (1303/5160) | [24.09%, 26.46%] |
| **In-VCA Visibility Ratio (Active Sightline)** | **29.77%** (1303/4377) | [28.43%, 31.14%] |
| **VCA Penetration Rate** | **84.83%** (4377/5160) | - |

## 3. Demographic & Agent Type Disparities
| Agent Type | Total Agents | Total Saw Sign | Overall Visibility (%) | Overall 95% CI | Agents in VCA | VCA Saw Sign | VCA Visibility (%) | VCA 95% CI | Eye Height (m) | Mean Time in VCA (s) | Median Time in VCA (s) |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Adult Female | 2485 | 606 | 24.386317907444667 | [22.7%, 26.1%] | 2118 | 606 | 28.611898016997166 | [26.7%, 30.6%] | 1.4499999999999997 | 8.827521570580737 | 6.695149000000001 |
| Adult Male | 2675 | 697 | 26.05607476635514 | [24.4%, 27.8%] | 2259 | 697 | 30.854360336432052 | [29.0%, 32.8%] | 1.58 | 8.675967806029217 | 6.269865 |


### Statistical Significance of Demographic Disparities
| Comparison | VCA Diff (%) | Chi2 Stat | p-value (Chi2) | Odds Ratio | p-value (Fisher) | Significant (p < 0.05) |
| --- | --- | --- | --- | --- | --- | --- |
| Adult Female vs Adult Male | -2.2424623194348854 | 2.523018517715679 | 0.11219563595378539 | 0.8981917970440209 | 0.1052576305621562 | False |


## 4. Multi-Sign Performance & Placement Comparison
| Sign | Total Agents | Agents in VCA | VCA Penetration (%) | Overall Visibility (%) | VCA Visibility (%) | Male VCA Vis (%) | Female VCA Vis (%) | Wheelchair VCA Vis (%) | Gender Inequity (M - F) |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Sign_0 | 1032 | 985 | 95.44573643410853 | 13.85658914728682 | 14.51776649746193 | 14.901960784313726 | 14.105263157894738 | 0.0 | 0.7966976264189878 |
| Sign_A | 1032 | 946 | 91.66666666666666 | 13.565891472868216 | 14.799154334038056 | 14.663951120162933 | 14.945054945054945 | 0.0 | -0.28110382489201236 |
| Sign_B | 1032 | 687 | 66.56976744186046 | 29.26356589147287 | 43.95924308588064 | 47.277936962750715 | 40.532544378698226 | 0.0 | 6.745392584052489 |
| Sign_C | 1032 | 774 | 75.0 | 36.724806201550386 | 48.96640826873385 | 50.877192982456144 | 46.93333333333333 | 0.0 | 3.943859649122814 |
| Sign_D | 1032 | 985 | 95.44573643410853 | 32.848837209302324 | 34.41624365482234 | 35.490196078431374 | 33.26315789473684 | 0.0 | 2.227038183694532 |


## 5. Exposure & Dwell Time Analysis (Time in VCA)
| CleanAgentType | SawSign | Count | Mean | Std | Median | IQR | Min | Max |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Adult Female | False | 1512 | 5.738962744371693 | 4.32724335069219 | 4.818552 | 5.02120625 | 0.01873779 | 57.6048 |
| Adult Female | True | 606 | 16.53362874092409 | 6.587361084752436 | 16.209335 | 9.816577500000001 | 1.152735 | 40.21142 |
| Adult Male | False | 1562 | 5.4286142508450705 | 3.8443876733070583 | 4.628572 | 4.75394075 | 0.03284073 | 20.95185 |
| Adult Male | True | 697 | 15.953394281205165 | 6.845863684090195 | 16.26791 | 9.72036 | 1.37841 | 42.5281 |


## 6. Corridors & Navigation Routes
| Route | TotalInVCA | SawSignCount | VisibilityRatio | MeanTimeInVCA |
| --- | --- | --- | --- | --- |
| 7 -> 8 | 1310 | 594 | 45.343511450381676 | 11.57659490534351 |
| 4 -> 8 | 1066 | 208 | 19.51219512195122 | 5.2171636398780485 |
| 2 -> 8 | 586 | 49 | 8.361774744027302 | 9.978774384641637 |
| 3 -> 8 | 458 | 384 | 83.84279475982532 | 15.301469748253275 |
| 5 -> 8 | 329 | 12 | 3.64741641337386 | 5.640600434072948 |
| 1 -> 8 | 234 | 0 | 0.0 | 3.922593759786325 |
| 8 -> 8 | 224 | 7 | 3.125 | 2.5648527433035713 |
| 0 -> 8 | 170 | 49 | 28.823529411764703 | 8.029763311764706 |


## 7. Key Research Takeaways for Thesis
1. **Demographic Occlusion Disparity**: Adult Males achieved a **30.85%** in-VCA visibility ratio compared to **28.61%** for Adult Females (a **+2.24% difference**).
   - Eye height difference (+0.13m) contributes directly to line-of-sight occlusion in crowded conditions.
2. **Dwell Time Impact**: Agents who successfully comprehended the sign spent on average **16.22s** in the VCA, compared to **5.58s** for agents who missed it.
3. **Corridor Vulnerability**: Routes with acute approach angles or shorter dwell durations exhibited marked drops in detection rates.