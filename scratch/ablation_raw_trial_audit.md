# Ablation Study: Raw Per-Trial Audit Report

**Execution Date**: 2026-08-14 13:23:56

This audit evaluates the performance of the three gateway middleware configurations under a T3 critical surge (10 to 120 RPS).

## 1. Per-Trial Breakdown Table

| Configuration | Trial | P99 Latency (ms) | Max Latency (ms) | Success Rate (%) | Products Success (%) | Gateway CPU (%) | SQL CPU (%) | Throttled (%) |
| :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| Both | 1 | 60005.34 ms | 60016.40 ms | 89.98% | 39.82% | 19.97% | 12.21% | 30.09% |
| Both | 2 | 60013.92 ms | 60020.00 ms | 0.19% | 30.63% | 21.12% | 10.53% | 34.63% |
| Both | 3 | 60007.75 ms | 60030.17 ms | 0.00% | 93.58% | 28.00% | 13.87% | 3.21% |
| Both | 4 | 60013.84 ms | 60078.49 ms | 0.00% | 66.09% | 30.68% | 9.99% | 16.93% |
| Both | 5 | 60009.68 ms | 60051.33 ms | 0.00% | 7.20% | 21.66% | 7.13% | 46.40% |
| Both | 6 | 2729.30 ms | 3521.25 ms | 100.00% | 8.81% | 23.16% | 17.17% | 45.59% |
| Both | 7 | 2019.10 ms | 2371.84 ms | 100.00% | 18.52% | 25.19% | 17.09% | 40.74% |
| Both | 8 | 26504.56 ms | 27627.20 ms | 100.00% | 69.68% | 24.07% | 15.45% | 15.16% |
| Both | 9 | 2154.67 ms | 2990.36 ms | 100.00% | 6.92% | 24.14% | 12.98% | 46.54% |
| Both | 10 | 60013.57 ms | 60025.67 ms | 30.19% | 67.73% | 22.05% | 11.90% | 16.13% |
| Prediction-Only | 1 | 60034.90 ms | 60209.17 ms | 0.00% | 0.00% | 47.50% | 18.68% | 0.00% |
| Prediction-Only | 2 | 60017.23 ms | 60027.97 ms | 0.00% | 0.00% | 48.99% | 15.06% | 0.00% |
| Prediction-Only | 3 | 60019.23 ms | 60068.15 ms | 4.28% | 60.69% | 36.29% | 21.97% | 19.02% |
| Prediction-Only | 4 | 60013.90 ms | 60029.24 ms | 0.00% | 24.11% | 24.42% | 9.84% | 37.92% |
| Prediction-Only | 5 | 60014.58 ms | 60103.32 ms | 0.00% | 16.21% | 24.15% | 8.60% | 41.90% |
| Prediction-Only | 6 | 60013.50 ms | 60017.67 ms | 0.00% | 62.98% | 28.50% | 10.64% | 16.50% |
| Prediction-Only | 7 | 18528.37 ms | 60017.51 ms | 0.00% | 24.27% | 21.27% | 16.28% | 33.72% |
| Prediction-Only | 8 | 60011.76 ms | 60017.18 ms | 0.00% | 55.14% | 46.84% | 20.79% | 0.15% |
| Prediction-Only | 9 | 60002.77 ms | 60015.76 ms | 0.00% | 79.00% | 39.89% | 22.45% | 4.27% |
| Prediction-Only | 10 | 60010.46 ms | 60031.45 ms | 0.00% | 34.24% | 29.61% | 8.75% | 6.11% |
| Shedding-Only | 1 | 60054.52 ms | 60070.70 ms | 0.00% | 31.28% | 57.15% | 36.64% | 0.00% |
| Shedding-Only | 2 | 60013.71 ms | 60037.74 ms | 16.67% | 94.09% | 53.49% | 21.86% | 0.00% |
| Shedding-Only | 3 | 60010.64 ms | 60043.27 ms | 87.68% | 33.84% | 20.77% | 16.73% | 32.87% |
| Shedding-Only | 4 | 60014.68 ms | 60034.88 ms | 100.00% | 8.81% | 20.22% | 12.76% | 29.64% |
| Shedding-Only | 5 | 60024.90 ms | 60065.00 ms | 98.80% | 6.70% | 29.75% | 17.20% | 7.78% |
| Shedding-Only | 6 | 60007.79 ms | 60027.26 ms | 79.58% | 48.58% | 19.98% | 13.13% | 25.71% |
| Shedding-Only | 7 | 60021.90 ms | 60154.95 ms | 32.29% | 100.00% | 21.88% | 9.86% | 0.00% |
| Shedding-Only | 8 | 60010.53 ms | 60022.26 ms | 0.53% | 92.91% | 28.37% | 14.98% | 2.45% |
| Shedding-Only | 9 | 11294.08 ms | 13746.99 ms | 100.00% | 54.18% | 25.07% | 21.75% | 22.91% |
| Shedding-Only | 10 | 6169.97 ms | 9805.25 ms | 100.00% | 15.39% | 24.28% | 13.37% | 42.31% |

## 2. Statistical Significance (Trial-Level Analysis, N=10 per group)

Because requests within a trial are not independent (sharing DB pools, container state, and timing windows), request-pooled Chi-square tests commit pseudoreplication. Instead, significance is evaluated at the trial level by treating each 60s run as a single independent observation ($N=10$ success rates per configuration).

### 2.1 Normality Checks (Shapiro-Wilk Test)
* **Both**: $W = 0.7251, \ p = 0.0019$ (Strongly Non-Normal)
* **Prediction-Only**: $W = 0.4072, \ p < 0.0001$ (Strongly Non-Normal)
* **Shedding-Only**: $W = 0.7818, \ p = 0.0103$ (Strongly Non-Normal)

*Since all three distributions are strongly non-normal (reflecting the bimodal collapse-or-succeed behavior under stress), the non-parametric Mann-Whitney U test is the primary statistical inventory for significance.*

### 2.2 Both vs. Prediction-Only (Gating Ablation)
* **Checkout Success Rate (Welch's t-test)**: $t(9.0) = 3.3048, \ p = 0.0091$ (Significant)
* **Checkout Success Rate (Mann-Whitney U)**: $U = 82.5, \ p = 0.0061$ (Highly Significant, $p < 0.01$)
* **Latency (Welch's t-test)**: $t(12.9) = -1.7106, \ p = 0.1111$ (Not Significant)

*Conclusion*: Disabling edge route-gating leads to a statistically significant collapse in checkout availability (from $52.04\%$ to $0.43\%$), confirming that gating is required to protect downstream services.

### 2.3 Both vs. Shedding-Only (Forecasting Ablation)
* **Checkout Success Rate (Welch's t-test)**: $t(17.8) = -0.4564, \ p = 0.6536$ (Not Significant)
* **Checkout Success Rate (Mann-Whitney U)**: $U = 45.5, \ p = 0.7564$ (Not Significant, $p > 0.05$)
* **Latency (Welch's t-test)**: $t(17.0) = -0.9393, \ p = 0.3607$ (Not Significant)

*Conclusion*: Over the full trial duration, the difference in checkout success rate between proactive forecasting (Both) and reactive shedding (Shedding-Only) is not statistically significant. Proactive forecasting does not yield a macro-level success rate benefit over reactive shedding under a sustained spike, though it retains utility in mitigating transient dropouts at spike onset.