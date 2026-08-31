# Appendix C: Empirical Evaluation Trial-by-Trial Data & Raw Evidence

This appendix documents the comprehensive dataset and raw telemetry logs generated during the 70-trial staging cluster evaluation. It contains the per-trial breakdowns for all four main matrix tiers (T1–T4) and all three ablation configurations (Both, Prediction-Only, Shedding-Only). The full raw JSON telemetry files and container log traces are retained in the project repository under `AIScalingSolution/load-tests/results/final-run/`.

---

## C.1 Staging Infrastructure & Evaluation Methodology

All empirical runs were executed within a containerized staging environment deployed via Docker Compose on a single isolated host machine running Windows 11 Professional and Windows Subsystem for Linux 2 (WSL 2). To ensure reproducibility and isolate the software's performance characteristics from host hardware jitter, strict CPU and memory resource limits were applied to each service container. 

The downstream database container was seeded with a representative e-commerce data volume comprising:
* **Users**: 10,000 records
* **Products**: 50,000 records
* **Orders**: 100,000 records

During peak stress testing, database query pagination was enforced at the code level using a `.Take(10)` constraint on home-feed and catalogue queries in the repository layer to prevent relational lock deadlocks, providing a stable baseline for gateway-level mitigation comparisons.

### C.1.1 Timeout Censoring & Latency Definitions
Under extreme saturation, tail latencies naturally cluster around the client-side timeout threshold. To provide transparency, the tables in this appendix document:
1. **Aggregate P99 (ms)**: The percentile duration across *all* requests, where timed-out/failed requests are censored at the 60s limit (e.g., ~60,000 ms).
2. **Completed P99 (ms)**: The percentile duration filtering only the subset of successfully completed checkouts (`expected_response:true`). For trials where success collapsed to 0%, this is designated as `N/A`.
3. **Completed (n)**: The count of successful checkouts that completed. For trials with very low success rates, the completed P99 is computed from a tiny sample ($n$), which readers should evaluate with appropriate caution.
4. **Censored / Failed (%)**: The percentage of requests that failed to complete (due to socket dropouts or hitting the 60s timeout ceiling).

### C.1.2 Staging Container Limits (`docker-compose.yml`)
```yaml
services:
  sqlserver:
    image: mcr.microsoft.com/azure-sql-edge:latest
    deploy:
      resources:
        limits:
          cpus: "0.5"
          memory: 2G

  apigateway:
    build:
      context: .
      dockerfile: ApiGateway/Dockerfile
    deploy:
      resources:
        limits:
          cpus: "0.5"
          memory: 512M

  userservice:
    deploy:
      resources:
        limits:
          cpus: "0.2"
          memory: 128M

  productservice:
    deploy:
      resources:
        limits:
          cpus: "0.3"
          memory: 256M

  orderservice:
    deploy:
      resources:
        limits:
          cpus: "0.3"
          memory: 256M

  redis:
    image: redis:7-alpine
    deploy:
      resources:
        limits:
          cpus: "0.2"
          memory: 128M
```

---

## C.2 Main Matrix Results (T1 - T4)

### C.2.1 Tier T1: Standard Scaling (10 to 60 RPS)
| Trial | Aggregate P99 (ms) | Completed P99 (ms) | Completed (n) | Success Rate (%) | Censored / Failed (%) | Products Success (%) | Gateway CPU (%) | SQL CPU (%) | Shedded (%) |
| :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| 1 | 10629.28 ms | 10840.59 ms | 3310 | 100.00% | 0.00% | 65.02% | 36.44% | 15.71% | 17.49% |
| 2 | 2517.38 ms | 2670.39 ms | 3653 | 100.00% | 0.00% | 9.36% | 25.17% | 12.31% | 45.32% |
| 3 | 290.10 ms | 298.17 ms | 3652 | 100.00% | 0.00% | 72.51% | 26.82% | 15.51% | 13.75% |
| 4 | 252.82 ms | 408.36 ms | 3653 | 100.00% | 0.00% | 32.25% | 25.00% | 13.01% | 33.88% |
| 5 | 3782.75 ms | 3844.52 ms | 3649 | 100.00% | 0.00% | 67.22% | 25.96% | 14.92% | 16.39% |
| 6 | 30449.25 ms | 30449.87 ms | 3087 | 100.00% | 0.00% | 98.12% | 30.76% | 23.50% | 0.94% |
| 7 | 919.32 ms | 924.14 ms | 3652 | 100.00% | 0.00% | 96.71% | 29.21% | 18.65% | 1.64% |
| 8 | 44345.58 ms | 45112.10 ms | 1685 | 84.72% | 15.28% | 95.68% | 31.34% | 30.28% | 2.12% |
| 9 | 2123.62 ms | 2133.37 ms | 3651 | 100.00% | 0.00% | 68.31% | 26.42% | 17.84% | 15.84% |
| 10 | 60015.34 ms | 39252.49 ms | 590 | 46.79% | 53.21% | 100.00% | 15.69% | 12.01% | 0.00% |

### C.2.2 Tier T2: Extended Loading (10 to 110 RPS)
| Trial | Aggregate P99 (ms) | Completed P99 (ms) | Completed (n) | Success Rate (%) | Censored / Failed (%) | Products Success (%) | Gateway CPU (%) | SQL CPU (%) | Shedded (%) |
| :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| 1 | 5004.59 ms | 5209.19 ms | 6120 | 100.00% | 0.00% | 21.81% | 24.87% | 15.68% | 39.09% |
| 2 | 3650.54 ms | 3782.24 ms | 6216 | 100.00% | 0.00% | 35.49% | 24.93% | 17.81% | 32.26% |
| 3 | 32594.22 ms | 34041.57 ms | 3860 | 90.80% | 9.20% | 51.26% | 26.24% | 15.52% | 24.37% |
| 4 | 9126.27 ms | 9254.97 ms | 5537 | 100.00% | 0.00% | 4.14% | 27.04% | 13.73% | 47.93% |
| 5 | 13063.63 ms | 13337.70 ms | 5174 | 100.00% | 0.00% | 36.47% | 26.74% | 18.16% | 31.76% |
| 6 | 16575.70 ms | 17787.97 ms | 4971 | 100.00% | 0.00% | 11.25% | 24.92% | 19.73% | 44.38% |
| 7 | 60011.54 ms | 23367.39 ms | 1863 | 80.13% | 19.87% | 52.65% | 18.98% | 10.75% | 23.58% |
| 8 | 60012.69 ms | 50465.66 ms | 79 | 31.47% | 68.53% | 64.94% | 47.94% | 26.03% | 0.93% |
| 9 | 15433.02 ms | 15644.72 ms | 4928 | 100.00% | 0.00% | 13.37% | 23.13% | 13.29% | 43.31% |
| 10 | 6462.42 ms | 6630.63 ms | 5802 | 100.00% | 0.00% | 29.78% | 29.26% | 19.23% | 35.11% |

### C.2.3 Tier T3: Critical Surge (10 to 120 RPS)
| Trial | Aggregate P99 (ms) | Completed P99 (ms) | Completed (n) | Success Rate (%) | Censored / Failed (%) | Products Success (%) | Gateway CPU (%) | SQL CPU (%) | Shedded (%) |
| :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| 1 | 60005.34 ms | 29353.93 ms | 2443 | 89.98% | 10.02% | 39.82% | 19.97% | 12.21% | 30.09% |
| 2 | 60013.92 ms | 3693.24 ms | 3 | 0.19% | 99.81% | 30.63% | 21.12% | 10.53% | 34.63% |
| 3 | 60007.75 ms | N/A | 0 | 0.00% | 100.00% | 93.58% | 28.00% | 13.87% | 3.21% |
| 4 | 60013.84 ms | N/A | 0 | 0.00% | 100.00% | 66.09% | 30.68% | 9.99% | 16.93% |
| 5 | 60009.68 ms | N/A | 0 | 0.00% | 100.00% | 7.20% | 21.66% | 7.13% | 46.40% |
| 6 | 2729.30 ms | 3024.66 ms | 6844 | 100.00% | 0.00% | 8.81% | 23.16% | 17.17% | 45.59% |
| 7 | 2019.10 ms | 2035.53 ms | 6953 | 100.00% | 0.00% | 18.52% | 25.19% | 17.09% | 40.74% |
| 8 | 26504.56 ms | 26620.38 ms | 4624 | 100.00% | 0.00% | 69.68% | 24.07% | 15.45% | 15.16% |
| 9 | 2154.67 ms | 2360.15 ms | 6892 | 100.00% | 0.00% | 6.92% | 24.14% | 12.98% | 46.54% |
| 10 | 60013.57 ms | 29634.84 ms | 537 | 30.19% | 69.81% | 67.73% | 22.05% | 11.90% | 16.13% |

### C.2.4 Tier T4: Destructive Tier (10 to 180 RPS)
| Trial | Aggregate P99 (ms) | Completed P99 (ms) | Completed (n) | Success Rate (%) | Censored / Failed (%) | Products Success (%) | Gateway CPU (%) | SQL CPU (%) | Shedded (%) |
| :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| 1 | 60275.10 ms | N/A | 0 | 0.00% | 100.00% | 22.46% | 22.93% | 11.46% | 38.34% |
| 2 | 60105.08 ms | N/A | 0 | 0.00% | 100.00% | 36.58% | 35.24% | 15.13% | 18.55% |
| 3 | 60013.22 ms | N/A | 0 | 0.00% | 100.00% | 3.15% | 31.14% | 13.31% | 48.42% |
| 4 | 59992.62 ms | 18037.08 ms | 4097 | 94.40% | 5.60% | 33.00% | 25.15% | 25.18% | 32.19% |
| 5 | 60013.61 ms | 53603.26 ms | 784 | 28.90% | 71.10% | 31.44% | 26.83% | 9.81% | 34.27% |
| 6 | 60011.64 ms | 25021.06 ms | 1683 | 67.48% | 32.52% | 43.91% | 22.72% | 13.84% | 27.82% |
| 7 | 60043.89 ms | 15956.69 ms | 2190 | 65.75% | 34.25% | 64.85% | 22.85% | 16.50% | 17.22% |
| 8 | 60028.64 ms | 42895.15 ms | 501 | 56.42% | 43.58% | 62.16% | 33.33% | 19.36% | 11.43% |
| 9 | 60009.57 ms | 59351.62 ms | 147 | 3.20% | 96.80% | 3.81% | 25.87% | 8.25% | 48.09% |
| 10 | 60016.34 ms | 33272.63 ms | 1585 | 52.31% | 47.69% | 57.92% | 24.34% | 15.16% | 21.04% |

---

## C.3 Ablation Study Raw Data (T3 Surge, 120 RPS)

### C.3.1 Configuration 1: Both (SSA + Ingress Gating)
*Success rates, latencies, and CPU metrics are identical to Section C.2.3 (copied from T3 main matrix trials to preserve dataset integrity).*

### C.3.2 Configuration 2: Prediction-Only (Gating Disabled)
| Trial | Aggregate P99 (ms) | Completed P99 (ms) | Completed (n) | Success Rate (%) | Censored / Failed (%) | Products Success (%) | Gateway CPU (%) | SQL CPU (%) | Throttled (%) |
| :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| 1 | 60034.90 ms | N/A | 0 | 0.00% | 100.00% | 0.00% | 47.50% | 18.68% | 0.00% |
| 2 | 60017.23 ms | N/A | 0 | 0.00% | 100.00% | 0.00% | 48.99% | 15.06% | 0.00% |
| 3 | 60019.23 ms | 32230.45 ms | 63 | 4.28% | 95.72% | 60.69% | 36.29% | 21.97% | 19.02% |
| 4 | 60013.90 ms | N/A | 0 | 0.00% | 100.00% | 24.11% | 24.42% | 9.84% | 37.92% |
| 5 | 60014.58 ms | N/A | 0 | 0.00% | 100.00% | 16.21% | 24.15% | 8.60% | 41.90% |
| 6 | 60013.50 ms | N/A | 0 | 0.00% | 100.00% | 62.98% | 28.50% | 10.64% | 16.50% |
| 7 | 18528.37 ms | N/A | 0 | 0.00% | 100.00% | 24.27% | 21.27% | 16.28% | 33.72% |
| 8 | 60011.76 ms | N/A | 0 | 0.00% | 100.00% | 55.14% | 46.84% | 20.79% | 0.15% |
| 9 | 60002.77 ms | N/A | 0 | 0.00% | 100.00% | 79.00% | 39.89% | 22.45% | 4.27% |
| 10 | 60010.46 ms | N/A | 0 | 0.00% | 100.00% | 34.24% | 29.61% | 8.75% | 6.11% |

### C.3.3 Configuration 3: Shedding-Only (Forecasting Disabled)
| Trial | Aggregate P99 (ms) | Completed P99 (ms) | Completed (n) | Success Rate (%) | Censored / Failed (%) | Products Success (%) | Gateway CPU (%) | SQL CPU (%) | Throttled (%) |
| :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| 1 | 60054.52 ms | N/A | 0 | 0.00% | 100.00% | 31.28% | 57.15% | 36.64% | 0.00% |
| 2 | 60013.71 ms | 39351.21 ms | 31 | 16.67% | 83.33% | 94.09% | 53.49% | 21.86% | 0.00% |
| 3 | 60010.64 ms | 27981.52 ms | 1943 | 87.68% | 12.32% | 33.84% | 20.77% | 16.73% | 32.87% |
| 4 | 60014.68 ms | 23196.25 ms | 2827 | 100.00% | 0.00% | 8.81% | 20.22% | 12.76% | 29.64% |
| 5 | 60024.90 ms | 38716.34 ms | 2801 | 98.80% | 1.20% | 6.70% | 29.75% | 17.20% | 7.78% |
| 6 | 60007.79 ms | 39833.96 ms | 1902 | 79.58% | 20.42% | 48.58% | 19.98% | 13.13% | 25.71% |
| 7 | 60021.90 ms | 44245.52 ms | 566 | 32.29% | 67.71% | 100.00% | 21.88% | 9.86% | 0.00% |
| 8 | 60010.53 ms | 22682.87 ms | 14 | 0.53% | 99.47% | 92.91% | 28.37% | 14.98% | 2.45% |
| 9 | 11294.08 ms | 11594.17 ms | 6111 | 100.00% | 0.00% | 54.18% | 25.07% | 21.75% | 22.91% |
| 10 | 6169.97 ms | 6387.26 ms | 6395 | 100.00% | 0.00% | 15.39% | 24.28% | 13.37% | 42.31% |

---

## C.4 Sample Raw k6 JSON Log Excerpt
The following is a literal, unformatted excerpt from the raw `RUN_T1_1_k6.json` file demonstrating the exact metric structure generated by the k6 engine:

```json
{
  "metrics": {
    "http_reqs": {
      "count": 6620,
      "rate": 55.14957089714223
    },
    "http_req_failed": {
      "passes": 1158,
      "fails": 5462,
      "value": 0.17492447129909366
    },
    "orders_ok": {
      "passes": 3310,
      "fails": 0,
      "value": 1.0
    },
    "http_req_duration": {
      "avg": 2792.2506732477627,
      "min": 1.5159,
      "med": 2824.3112499999997,
      "max": 11695.9724,
      "p(90)": 5328.566560000001,
      "p(95)": 9600.305684999992,
      "p(99)": 10629.275041999997
    },
    "http_req_duration{expected_response:true}": {
      "p(90)": 5969.611300000011,
      "p(95)": 9888.685029999999,
      "p(99)": 10840.590818000004,
      "avg": 3351.915118161862,
      "min": 3.3335,
      "med": 3153.7384,
      "max": 11695.9724
    }
  }
}
```
