# Experiment Plan

To rigorously address your research questions, the experimental design must systematically isolate variables across predefined scenarios while employing robust statistical evaluation. By recreating the established methodologies of Motamedi et al. alongside custom high-density simulations governed by the Unilateral Incompressibility Constraint (UIC), you can quantitatively map the degradation of line-of-sight against emergent crowd turbulence. To establish the mathematical significance of these interactions—specifically correlating the UIC packing parameter ($\alpha$), precise signage coordinates, and distinct anthropometric profiles—deploying a Two-Way Analysis of Variance (ANOVA) and multivariate regression models on the extracted spatiotemporal datasets will be essential. The following experimental plan details the specific configurations and parameters required to execute these simulations.

1. Recreation of Motamedi et al. Scenarios

    * Scenario A (Demographic Variance): Instantiate 1,600 agents and execute two demographic distributions: first, 50% male (eye-level 1.58m) and 50% female (eye-level 1.45m); second, 75% adults and 25% children or wheelchair users (eye-level 1.17m). Position the target sign at a mounting height of 3.0m, defining the maximum viewing distance ($d$) as 15m, the maximum viewing angle ($h$) as $90^\circ$, and setting the comprehension time to 1.0s.

    * Scenario B (Complex Environment Traffic): Simulate 1,128 agents navigating a subway station geometry based on empirical field traffic rates. Evaluate a "Main Sign" configured with a $60^\circ$ viewing angle and a 2.0s comprehension time, alongside a "Hotel Sign" configured with a $90^\circ$ angle and a 2.0s comprehension time.

    * Scenario C (Signage Optimization): Execute an optimization matrix placing signs at a fixed vertical height of 2.4m across 1,681 dense discrete grid points and 10,000 random coordinates to calculate the maximum visibility ratio across horizontal and vertical orientations.

2. Custom Density and Sign Placement Experiments (Addressing RQ1 & RQ2)

    * Density Thresholds (RQ1): Systematically modulate the UIC parameter ($\alpha$) to simulate varying degrees of crowd pressure, scaling from a normal density ($\alpha = 1.0$) down to extreme high-density states ($\alpha = 0.4$ and $\alpha = 0.2$). By logging the Effective Route Detection Ratio ($RD_{effective}$) and continuous visual exposure times at these distinct intervals, you can mathematically quantify the precise macroscopic density thresholds where signage visibility degrades due to crowd occlusion.
    
    * Placement Configurations (RQ2): Utilize your Building Information Modeling (BIM) coordinates to manipulate physical sign variables. Test extreme mounting height variations—such as comparing low wall-mounted signs at 0.6m against high signs at 2.6m or 3.0m—and modulate the virtual Field of View (FOV) angular constraints (e.g., $30^\circ$ versus $60^\circ$) to determine which architectural configuration best withstands the high-density occlusions generated in RQ1.

3. Anthropometric and Body Size Effects (Addressing RQ3)
Microscopic Perception Analysis: Extract the raycast detection logs and filter the dataset exclusively by the distinct anthropometric eye-levels defined in Phase 1 (1.58m, 1.45m, and 1.17m). By applying a Two-Way ANOVA to evaluate the interaction between these specific body sizes and the high-density states ($\alpha$), you can statistically quantify the spatial and demographic inequities experienced by smaller agents or wheelchair users during wayfinding.
