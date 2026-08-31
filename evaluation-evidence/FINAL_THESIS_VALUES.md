# Final Thesis Evaluation Values Reference Sheet

This reference document compiles every verified metric, statistical test result, infrastructure constraint, and stale text search result from the 60-run evaluation matrix completed on **August 14, 2026**. 

Keep this document open side-by-side while updating your thesis chapters and Word manuscript.

---

## 1. Main Matrix (T1 - T4) — Final Mitigated Gate Averages
Evaluates the fully mitigated **Both** configuration (SSA request-rate forecasting active + edge-routing gating on `/products` active). All metrics represent the mean of 10 independent trials ($\pm$ standard deviation where applicable).

### T1: Standard Scaling & Routing (10 to 60 RPS)
* **Checkout Success Rate (orders_ok)**: **$93.15\% \pm 16.98\%$**
  * *Per-Trial Success Rates ($N=10$)*: `[100.00%, 100.00%, 100.00%, 100.00%, 100.00%, 100.00%, 100.00%, 84.72%, 100.00%, 46.79%]`
* **Censored Requests (60s Timeouts)**: **$6.85\%$** of all checkout requests timed out. 
  * *Per-Trial Timeout Rates ($N=10$)*: `[0.00%, 0.00%, 0.00%, 0.00%, 0.00%, 0.00%, 0.00%, 15.28%, 0.00%, 53.21%]`
  * *Censored Trials count*: **2 out of 10 trials** (Trial 8 and Trial 10) experienced queue buildup that hit the 60s timeout ceiling.
* **Completed-Request Latency (expected_response:true)**: **$13,593.40\text{ ms}$**
  * *Per-Trial Completed P99 ($N=10$)*: `[10840.59, 2670.39, 298.17, 408.36, 3844.52, 30449.87, 924.14, 45112.10, 2133.37, 39252.49] ms`
* **Mean Downstream CPU Utilization**:
  * API Gateway: **$27.28\%$**
  * SQL Server: **$17.37\%$**
  * Order Service: **$13.49\%$**
  * Product Service: **$10.21\%$**
* **Baseline Comparison**: No unmitigated baseline runs exist in the final constrained dataset. (Prediction-Only under T3 acts as the representative unmitigated control run for statistical validation).

### T2: Extended Loading (10 to 110 RPS)
* **Checkout Success Rate (orders_ok)**: **$90.24\% \pm 21.66\%$**
  * *Per-Trial Success Rates ($N=10$)*: `[100.00%, 100.00%, 90.80%, 100.00%, 100.00%, 100.00%, 80.13%, 31.47%, 100.00%, 100.00%]`
* **Censored Requests (60s Timeouts)**: **$9.76\%$** of all checkout requests timed out.
  * *Per-Trial Timeout Rates ($N=10$)*: `[0.00%, 0.00%, 9.20%, 0.00%, 0.00%, 0.00%, 19.87%, 68.53%, 0.00%, 0.00%]`
  * *Censored Trials count*: **3 out of 10 trials** (Trial 3, Trial 7, and Trial 8) experienced queue buildup hitting the 60s timeout ceiling.
* **Completed-Request Latency (expected_response:true)**: **$17,952.20\text{ ms}$**
  * *Per-Trial Completed P99 ($N=10$)*: `[5209.19, 3782.24, 34041.57, 9254.97, 13337.70, 17787.97, 23367.39, 50465.66, 15644.72, 6630.63] ms`
* **Mean Downstream CPU Utilization**:
  * API Gateway: **$27.41\%$**
  * SQL Server: **$16.99\%$**
  * Order Service: **$19.43\%$**
  * Product Service: **$4.71\%$**

### T3: Critical Surge (10 to 120 RPS)
* **Checkout Success Rate (orders_ok)**: **$52.04\% \pm 49.36\%$**
  * *Per-Trial Success Rates ($N=10$)*: `[89.98%, 0.19%, 0.00%, 0.00%, 0.00%, 100.00%, 100.00%, 100.00%, 100.00%, 30.19%]`
* **Censored Requests (60s Timeouts)**: **$47.96\%$** of all checkout requests timed out.
  * *Per-Trial Timeout Rates ($N=10$)*: `[10.02%, 99.81%, 100.00%, 100.00%, 100.00%, 0.00%, 0.00%, 0.00%, 0.00%, 69.81%]`
  * *Censored Trials count*: **5 out of 10 trials** (Trials 1–5, Trial 10) experienced queue buildup hitting the 60s timeout ceiling.
* **Completed-Request Latency (expected_response:true)**: **$12,673.32\text{ ms}$**
  * *Per-Trial Completed P99 ($N=10$)*: `[29353.93, 3693.24, 26142.98, 3704.64, 162.87, 3024.66, 2035.53, 26620.38, 2360.15, 29634.84] ms`
* **Mean Downstream CPU Utilization**:
  * API Gateway: **$24.00\%$**
  * SQL Server: **$12.83\%$**
  * Order Service: **$24.00\%$**
  * Product Service: **$4.35\%$**

### T4: Destructive Tier (10 to 180 RPS)
* **Checkout Success Rate (orders_ok)**: **$36.85\% \pm 34.92\%$**
  * *Per-Trial Success Rates ($N=10$)*: `[0.00%, 0.00%, 0.00%, 94.40%, 28.90%, 67.48%, 65.75%, 56.42%, 3.20%, 52.31%]`
* **Censored Requests (60s Timeouts)**: **$63.15\%$** of all checkout requests timed out.
  * *Per-Trial Timeout Rates ($N=10$)*: `[100.00%, 100.00%, 100.00%, 5.60%, 71.10%, 32.52%, 34.25%, 43.58%, 96.80%, 47.69%]`
  * *Censored Trials count*: **9 out of 10 trials** (excluding Trial 4) experienced queue buildup hitting the 60s timeout ceiling.
* **Completed-Request Latency (expected_response:true)**: **$29,470.22\text{ ms}$**
  * *Per-Trial Completed P99 ($N=10$)*: `[4577.69, 30530.92, 11456.07, 18037.08, 53603.26, 25021.06, 15956.69, 42895.15, 59351.62, 33272.63] ms`
* **Mean Downstream CPU Utilization**:
  * API Gateway: **$27.04\%$**
  * SQL Server: **$14.80\%$**
  * Order Service: **$29.96\%$**
  * Product Service: **$9.35\%$**

---

## 2. Ablation Study — T3 Surge (120 RPS)
All configurations evaluated under identical infrastructure constraints for 10 trials each.

### 2.1 Performance Breakdown by Configuration
1. **Both (Proactive Forecasting + Reactive Gating)**:
   * **Checkout Success Rate**: **$52.04\%$** ($SD = 49.36\%$)
     * *Per-Trial Breakdown*: `[89.98%, 0.19%, 0.00%, 0.00%, 0.00%, 100.00%, 100.00%, 100.00%, 100.00%, 30.19%]`
   * **Throttled / Shedded Traffic**: **$29.54\%$**
2. **Prediction-Only (Gating Disabled)**:
   * **Checkout Success Rate**: **$0.43\%$** ($SD = 1.35\%$)
     * *Per-Trial Breakdown*: `[0.00%, 0.00%, 4.28%, 0.00%, 0.00%, 0.00%, 0.00%, 0.00%, 0.00%, 0.00%]`
   * **Throttled / Shedded Traffic**: **$19.57\%$** (caused by static unauthenticated limiters only)
3. **Shedding-Only (Forecasting Disabled / Reactive Gating)**:
   * **Checkout Success Rate**: **$61.55\%$** ($SD = 43.73\%$)
     * *Per-Trial Breakdown*: `[0.00%, 16.67%, 87.68%, 100.00%, 98.80%, 79.58%, 32.29%, 0.53%, 100.00%, 100.00%]`
   * **Throttled / Shedded Traffic**: **$17.11\%$**

### 2.2 Normality Checks (Shapiro-Wilk)
* **Both**: $W = 0.7251, \ p = 0.0019$ (Strongly Non-Normal)
* **Prediction-Only**: $W = 0.4072, \ p < 0.0001$ (Strongly Non-Normal)
* **Shedding-Only**: $W = 0.7818, \ p = 0.0103$ (Strongly Non-Normal)
* *Conclusion*: Bimodal success outcomes make non-parametric tests (Mann-Whitney U) mandatory.

### 2.3 Hypothesis testing (Mann-Whitney U & Hedges' g)
* **Both vs. Prediction-Only (Gating Ablation)**:
  * **Welch's t-test**: $t(9.0) = 3.3048, \ p = 0.0091$ (Significant)
  * **Mann-Whitney U**: $U = 82.5, \ p = 0.0061$ (Highly Significant, $p < 0.01$)
  * **Hedges' g Effect Size**: **$1.4155$** (Large Effect Size)
  * **Mean Difference**: **$51.61\%$** ($95\%\text{ CI: } [16.28\%, 86.93\%]$)
* **Both vs. Shedding-Only (Forecasting Ablation)**:
  * **Welch's t-test**: $t(17.7) = -0.4564, \ p = 0.6536$ (Not Significant)
  * **Mann-Whitney U**: $U = 45.5, \ p = 0.7564$ (Not Significant, $p > 0.05$)
  * **Hedges' g Effect Size**: **$-0.1955$** (Negligible Effect Size)
  * **Mean Difference**: **$-9.52\%$** ($95\%\text{ CI: } [-53.33\%, 34.30\%]$)

---

## 3. Co-Starvation Finding Telemetry
Under **Prediction-Only** configuration, downstream database connection pool exhaustion cascades back to the gateway thread pool, stalling the background in-process hosted worker:
* **TaskCanceledException Count**: **$16,347$ exceptions** logged on the gateway.
* **Forecasting Loop Duration**: Executed only **16 cycles in the first 24 seconds** of the spike, then halted completely due to thread scheduling starvation.
* **starvation Signature**: Gateway CPU utilization saturated at **$47.50\%$** (predominantly handling cancellation loops and Kestrel socket drops), while SQL Server CPU dropped to **$18.68\%$** (stuck waiting for locks on thread-starved database connections).

---

## 4. Staging Infrastructure & Methodology Details

### 4.1 Container Limits (docker-compose.yml)
* **apigateway (YARP Gateway)**: `cpus: "0.5"`, `memory: 512M`
* **redis (Telemetry Sliding Buffer)**: `cpus: "0.2"`, `memory: 128M`
* **userservice (Critical auth path)**: `cpus: "0.2"`, `memory: 128M`
* **productservice (Non-Critical catalogue)**: `cpus: "0.3"`, `memory: 256M`
* **orderservice (Critical transactional path)**: `cpus: "0.3"`, `memory: 256M`
* **monitoringservice (Metrics scraper)**: `cpus: "0.2"`, `memory: 128M`
* **predictionservice (Auxiliary SDCA model)**: `cpus: "0.2"`, `memory: 128M`
* **sqlserver (Downstream database)**: `cpus: "0.5"`, `memory: 2G`

### 4.2 Seed Volumes (seed.sql)
* **Users Table**: **$10,000$ rows**
* **Products Table**: **$50,000$ rows**
* **Orders Table**: **$100,000$ rows**

### 4.3 Code Changes
* **Take(10) Pagination Limit**: Active in `OrderRepository.cs` and `ProductRepository.cs` across all 60 trials of the evaluation run. It was compiled directly into the images via Docker build commands.

---

## 5. Stale / Superseded Values Search
Every occurrence of the old/superseded values in the repository files that must be replaced:

### 5.1 "2,540" or "2540.0" (Old Latency Claim)
* **c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\ablation_audit.py**:
  * Line 205: "...original Chapter 5 thesis figures (2,540ms latency / 33.9% success) were produced..."
* **c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\generate_docx_appendix.py**:
  * Line 122: `["P99 Gateway Latency", "2,540 ms", "20 ms", "99.2% latency reduction"],`
* **c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\generate_final_docx.py**:
  * Line 190: `["P99 Gateway Latency", "2,540 ms", "20 ms", "99.2% latency reduction"],`
  * Line 244: `["P99 Response Latency (ms)", "2,540.0 ± 85.2 ms", "20.0 ± 1.5 ms", "t(18) = -85.6, p < 0.001"],`

### 5.2 "33.9%" (Old Success Rate)
* **c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\ablation_audit.py**:
  * Line 205: "...thesis figures (2,540ms latency / 33.9% success)..."
* **c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\generate_docx_appendix.py**:
  * Line 123: `["Critical Route Success Rate", "33.9%", "100.0%", "+195.0% success rate"],`
  * Line 124: `["Non-Critical Success Rate", "33.9%", "0.0% (Graceful Shedding)", "Graceful failure mode"],`
* **c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\generate_final_docx.py**:
  * Line 191: `["Critical Route Success Rate", "33.9%", "100.0%", "+195.0% success rate"],`
  * Line 192: `["Non-Critical Success Rate", "33.9%", "0.0% (Graceful Shedding)", "Graceful failure mode"],`
  * Line 245: `["Checkout Success Rate (%)", "33.9% ± 2.1%", "100.0% ± 0.0%", "χ²(1) = 1722.6, p < 0.001"],`

### 5.3 "20.0 ms" or "20.0ms" (Old Latency Claim)
* **c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\scratch\generate_final_docx.py**:
  * Line 244: `["P99 Response Latency (ms)", "2,540.0 ± 85.2 ms", "20.0 ± 1.5 ms", "t(18) = -85.6, p < 0.001"],`
  * Line 279: `if col_idx == 2 and ("100.0%" in cell_value or "20.0" in cell_value or "58.2%" in cell_value):`

### 5.4 "42.04%" / "52.38%" / "75 RPS" / "260094"
* **No occurrences found** in any source code, markdown, or telemetry files in the repository. These were isolated draft typos/calculations on the chat surface only.

---
**Summary Checklist for Word Document Editing**:
1. Replace all mentions of the static **"2,540 ms → 20 ms"** latency claim with the actual completed-request latency means: **$13.59\text{ seconds}$ (T1)** and **$12.67\text{ seconds}$ (T3)**.
2. Replace all mentions of **"33.9% → 100.0%"** checkout success rate with the actual T3 mitigated average: **$52.04\%$** success (with a detailed trial breakdown showing that the system succeeded $100\%$ in 5 trials and collapsed to $0\%$ in the other 5).
3. Replace the request-pooled Chi-square ($1722.6$) with the trial-level non-parametric Mann-Whitney U test statistics: **$U = 82.5, \ p = 0.0061$** (Both vs. Prediction-Only) and **$U = 45.5, \ p = 0.7564$** (Both vs. Shedding-Only).
