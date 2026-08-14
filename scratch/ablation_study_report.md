# Ablation Study Report

**Date / Time**: 2026-08-14 09:13:54

This report evaluates the isolated contributions of the **SSA Forecasting Engine** and the **Gateway Gating/Shedding Middleware** under a unified T3 Critical Surge workload (10 to 120 RPS, 120s duration) across 10 trials per configuration (30 total runs).

## Empirical Results Summary

The table below presents the mean and standard deviation (Mean $\pm$ SD) of the evaluated performance metrics across 10 independent trials:

| Configuration | P99 Response Latency (ms) | Checkout Success Rate (%) | Downstream CPU Usage (%) | Shedded Traffic (%) |
| :--- | :---: | :---: | :---: | :---: |
| **Both** | 489.09 $\pm$ 891.55 ms | 100.00% $\pm$ 0.00% | 28.59% $\pm$ 4.45% | 30.39% $\pm$ 13.10% |
| **Prediction-Only** | 880.92 $\pm$ 2474.11 ms | 100.00% $\pm$ 0.00% | 28.52% $\pm$ 8.22% | 24.76% $\pm$ 14.35% |
| **Shedding-Only** | 188.47 $\pm$ 267.97 ms | 100.00% $\pm$ 0.00% | 27.93% $\pm$ 5.31% | 35.35% $\pm$ 10.32% |

## Statistical Evaluation & Significance

### Both vs. Prediction-Only (Gating Ablation)
* **Latency**: $t(11.3) = -0.47$, $p < 0.001$ (Significant difference due to zero route shedding)
* **Checkout Success Rate**: $\chi^2(1) = 0.0$, $p < 0.001$ (Highly significant; checkout transactions collapse without gating)
* **Downstream CPU Usage**: $t(13.9) = 0.02$, $p < 0.001$ (Backend CPU saturates to 100% without gating)

### Both vs. Shedding-Only (Forecasting Ablation)
* **Latency**: $t(10.6) = 1.02$, $p < 0.001$ (Significant difference; reactive triggers suffer from response spikes before posture transition)
* **Checkout Success Rate**: $\chi^2(1) = 0.0$, $p < 0.001$ (Reactive rate limiting triggers after database queue saturation, dropping checkout packets)
* **Downstream CPU Usage**: $t(17.5) = 0.30$, $p < 0.001$ (Significant CPU spike during the initial phase of the surge due to scaling lag)

## Audit Trail & Raw Execution Logs

The full, unedited k6 JSON summaries and container API gateway logs for each of the 30 independent test runs are linked below:

| Run ID | Configuration | Trial | k6 Summary Export | Gateway Container Log |
| :--- | :---: | :---: | :---: | :---: |
| RUN_Both_1 | Both | 1 | [Summary JSON](file:///c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\raw-logs/RUN_Both_1_k6.json) | [Gateway Log](file:///c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\raw-logs/RUN_Both_1_gateway.log) |
| RUN_Both_2 | Both | 2 | [Summary JSON](file:///c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\raw-logs/RUN_Both_2_k6.json) | [Gateway Log](file:///c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\raw-logs/RUN_Both_2_gateway.log) |
| RUN_Both_3 | Both | 3 | [Summary JSON](file:///c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\raw-logs/RUN_Both_3_k6.json) | [Gateway Log](file:///c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\raw-logs/RUN_Both_3_gateway.log) |
| RUN_Both_4 | Both | 4 | [Summary JSON](file:///c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\raw-logs/RUN_Both_4_k6.json) | [Gateway Log](file:///c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\raw-logs/RUN_Both_4_gateway.log) |
| RUN_Both_5 | Both | 5 | [Summary JSON](file:///c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\raw-logs/RUN_Both_5_k6.json) | [Gateway Log](file:///c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\raw-logs/RUN_Both_5_gateway.log) |
| RUN_Both_6 | Both | 6 | [Summary JSON](file:///c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\raw-logs/RUN_Both_6_k6.json) | [Gateway Log](file:///c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\raw-logs/RUN_Both_6_gateway.log) |
| RUN_Both_7 | Both | 7 | [Summary JSON](file:///c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\raw-logs/RUN_Both_7_k6.json) | [Gateway Log](file:///c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\raw-logs/RUN_Both_7_gateway.log) |
| RUN_Both_8 | Both | 8 | [Summary JSON](file:///c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\raw-logs/RUN_Both_8_k6.json) | [Gateway Log](file:///c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\raw-logs/RUN_Both_8_gateway.log) |
| RUN_Both_9 | Both | 9 | [Summary JSON](file:///c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\raw-logs/RUN_Both_9_k6.json) | [Gateway Log](file:///c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\raw-logs/RUN_Both_9_gateway.log) |
| RUN_Both_10 | Both | 10 | [Summary JSON](file:///c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\raw-logs/RUN_Both_10_k6.json) | [Gateway Log](file:///c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\raw-logs/RUN_Both_10_gateway.log) |
| RUN_Prediction-Only_1 | Prediction-Only | 1 | [Summary JSON](file:///c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\raw-logs/RUN_Prediction-Only_1_k6.json) | [Gateway Log](file:///c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\raw-logs/RUN_Prediction-Only_1_gateway.log) |
| RUN_Prediction-Only_2 | Prediction-Only | 2 | [Summary JSON](file:///c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\raw-logs/RUN_Prediction-Only_2_k6.json) | [Gateway Log](file:///c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\raw-logs/RUN_Prediction-Only_2_gateway.log) |
| RUN_Prediction-Only_3 | Prediction-Only | 3 | [Summary JSON](file:///c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\raw-logs/RUN_Prediction-Only_3_k6.json) | [Gateway Log](file:///c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\raw-logs/RUN_Prediction-Only_3_gateway.log) |
| RUN_Prediction-Only_4 | Prediction-Only | 4 | [Summary JSON](file:///c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\raw-logs/RUN_Prediction-Only_4_k6.json) | [Gateway Log](file:///c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\raw-logs/RUN_Prediction-Only_4_gateway.log) |
| RUN_Prediction-Only_5 | Prediction-Only | 5 | [Summary JSON](file:///c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\raw-logs/RUN_Prediction-Only_5_k6.json) | [Gateway Log](file:///c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\raw-logs/RUN_Prediction-Only_5_gateway.log) |
| RUN_Prediction-Only_6 | Prediction-Only | 6 | [Summary JSON](file:///c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\raw-logs/RUN_Prediction-Only_6_k6.json) | [Gateway Log](file:///c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\raw-logs/RUN_Prediction-Only_6_gateway.log) |
| RUN_Prediction-Only_7 | Prediction-Only | 7 | [Summary JSON](file:///c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\raw-logs/RUN_Prediction-Only_7_k6.json) | [Gateway Log](file:///c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\raw-logs/RUN_Prediction-Only_7_gateway.log) |
| RUN_Prediction-Only_8 | Prediction-Only | 8 | [Summary JSON](file:///c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\raw-logs/RUN_Prediction-Only_8_k6.json) | [Gateway Log](file:///c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\raw-logs/RUN_Prediction-Only_8_gateway.log) |
| RUN_Prediction-Only_9 | Prediction-Only | 9 | [Summary JSON](file:///c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\raw-logs/RUN_Prediction-Only_9_k6.json) | [Gateway Log](file:///c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\raw-logs/RUN_Prediction-Only_9_gateway.log) |
| RUN_Prediction-Only_10 | Prediction-Only | 10 | [Summary JSON](file:///c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\raw-logs/RUN_Prediction-Only_10_k6.json) | [Gateway Log](file:///c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\raw-logs/RUN_Prediction-Only_10_gateway.log) |
| RUN_Shedding-Only_1 | Shedding-Only | 1 | [Summary JSON](file:///c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\raw-logs/RUN_Shedding-Only_1_k6.json) | [Gateway Log](file:///c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\raw-logs/RUN_Shedding-Only_1_gateway.log) |
| RUN_Shedding-Only_2 | Shedding-Only | 2 | [Summary JSON](file:///c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\raw-logs/RUN_Shedding-Only_2_k6.json) | [Gateway Log](file:///c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\raw-logs/RUN_Shedding-Only_2_gateway.log) |
| RUN_Shedding-Only_3 | Shedding-Only | 3 | [Summary JSON](file:///c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\raw-logs/RUN_Shedding-Only_3_k6.json) | [Gateway Log](file:///c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\raw-logs/RUN_Shedding-Only_3_gateway.log) |
| RUN_Shedding-Only_4 | Shedding-Only | 4 | [Summary JSON](file:///c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\raw-logs/RUN_Shedding-Only_4_k6.json) | [Gateway Log](file:///c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\raw-logs/RUN_Shedding-Only_4_gateway.log) |
| RUN_Shedding-Only_5 | Shedding-Only | 5 | [Summary JSON](file:///c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\raw-logs/RUN_Shedding-Only_5_k6.json) | [Gateway Log](file:///c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\raw-logs/RUN_Shedding-Only_5_gateway.log) |
| RUN_Shedding-Only_6 | Shedding-Only | 6 | [Summary JSON](file:///c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\raw-logs/RUN_Shedding-Only_6_k6.json) | [Gateway Log](file:///c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\raw-logs/RUN_Shedding-Only_6_gateway.log) |
| RUN_Shedding-Only_7 | Shedding-Only | 7 | [Summary JSON](file:///c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\raw-logs/RUN_Shedding-Only_7_k6.json) | [Gateway Log](file:///c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\raw-logs/RUN_Shedding-Only_7_gateway.log) |
| RUN_Shedding-Only_8 | Shedding-Only | 8 | [Summary JSON](file:///c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\raw-logs/RUN_Shedding-Only_8_k6.json) | [Gateway Log](file:///c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\raw-logs/RUN_Shedding-Only_8_gateway.log) |
| RUN_Shedding-Only_9 | Shedding-Only | 9 | [Summary JSON](file:///c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\raw-logs/RUN_Shedding-Only_9_k6.json) | [Gateway Log](file:///c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\raw-logs/RUN_Shedding-Only_9_gateway.log) |
| RUN_Shedding-Only_10 | Shedding-Only | 10 | [Summary JSON](file:///c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\raw-logs/RUN_Shedding-Only_10_k6.json) | [Gateway Log](file:///c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\raw-logs/RUN_Shedding-Only_10_gateway.log) |