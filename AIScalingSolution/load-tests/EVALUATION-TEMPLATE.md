# Predictive Middleware - Load Test Evaluation Record

> **Auto-generated** by `run-evaluation.ps1`. Artifacts for this run are linked below.
> Complete any remaining `_(manual)_` fields and write section 10 for your thesis.

**Artifacts**

| Artifact | Path |
|----------|------|
| This record | `{{EVALUATION_FILE}}` |
| k6 JSON summary | `{{K6_SUMMARY_FILE}}` |
| Diagnostics log | `{{DIAGNOSTICS_LOG_FILE}}` |
| JMeter results | `{{JMETER_RESULTS_FILE}}` |
| JMeter HTML report | `{{JMETER_REPORT_DIR}}` |

---

## 1. Test metadata

| Field | Value |
|-------|-------|
| **Evaluation ID** | `{{EVALUATION_ID}}` |
| **Date / time (UTC)** | {{DATETIME_UTC}} |
| **Evaluator** | {{EVALUATOR}} |
| **Thesis chapter reference** | Ch. 5 - Evaluation |
| **Scenario** | {{SCENARIO}} |
| **Tool** | {{TOOL}} |
| **Script / plan** | `{{SCRIPT_PATH}}` |
| **Git commit** | `{{GIT_COMMIT}}` |

---

## 2. Environment

| Component | Version / image | Endpoint |
|-----------|-----------------|----------|
| Host OS | {{HOST_OS}} | |
| .NET runtime | {{DOTNET_VERSION}} | |
| Redis | `redis:7-alpine` | `localhost:6379` |
| ApiGateway | docker / local | `http://localhost:5000` |
| StressTest.Api | docker / local | `http://localhost:5000` _(docker)_ / `5080` _(dotnet run)_ |
| SQL Server | optional | gateway scenario only |
| k6 / JMeter | {{TOOL_VERSION}} | |

**Middleware configuration highlights**

| Setting | Value |
|---------|-------|
| Redis key prefix | `gateway:telemetry:v1` / `api:telemetry:v1` |
| SSA horizon (seconds) | 60 |
| Baseline RPS per bucket | 80 |
| Critical route(s) | `/orders`, `/users`, `/api/v1/payment/checkout` |
| Non-critical route(s) | `/products`, `/api/v1/promotions/ads` |
| Diagnostics interval | 2s |

---

## 3. Traffic profile

| Phase | Duration | Target RPS | Endpoints exercised |
|-------|----------|------------|---------------------|
| 1 - Baseline | 30s | 10 | critical + non-critical |
| 2 - Spike | 15s | 800 | critical + non-critical |
| 3 - Cool-down | 45s | 5 | critical + non-critical |

**Hypothesis under test**

1. SSA forecasting elevates posture **before** reactive infrastructure scaling would respond.
2. Critical routes remain available (HTTP 200) during Critical posture.
3. Non-critical routes are shed (HTTP 429) during Critical posture.
4. Posture returns toward Nominal during cool-down.
5. Middleware telemetry overhead stays sub-millisecond at P99.

---

## 4. Gateway / middleware diagnostics (console log)

Paste representative log lines from each phase (auto-captured from container logs; verify and edit if needed):

```text
{{DIAGNOSTICS_SNIPPET}}
```

| Observation | Baseline | Spike | Cool-down |
|-------------|----------|-------|-----------|
| Dominant posture | {{DOMINANT_POSTURE_BASELINE}} | {{DOMINANT_POSTURE_SPIKE}} | {{DOMINANT_POSTURE_COOLDOWN}} |
| Peak current RPS | {{PEAK_RPS_BASELINE}} | {{PEAK_RPS_SPIKE}} | {{PEAK_RPS_COOLDOWN}} |
| Peak forecasted RPS (60s) | {{PEAK_FORECAST_BASELINE}} | {{PEAK_FORECAST_SPIKE}} | {{PEAK_FORECAST_COOLDOWN}} |
| Cumulative throttled requests | {{THROTTLED_BASELINE}} | {{THROTTLED_SPIKE}} | {{THROTTLED_COOLDOWN}} |
| P99 internal overhead (ms) | {{P99_OVERHEAD_BASELINE}} | {{P99_OVERHEAD_SPIKE}} | {{P99_OVERHEAD_COOLDOWN}} |
| Time to first Critical posture (s from spike start) | n/a | {{TIME_TO_CRITICAL}} | n/a |
| Time to return to Nominal (s from cool-down start) | n/a | n/a | {{TIME_TO_NOMINAL}} |

---

## 5. k6 results

### 5.1 Aggregate metrics

| Metric | Value | Pass threshold |
|--------|-------|----------------|
| Total requests | {{K6_TOTAL_REQUESTS}} | — |
| `http_req_failed` rate | {{K6_HTTP_REQ_FAILED_RATE}} | < 50% |
| `http_req_duration` p(50) | {{K6_P50_MS}} ms | — |
| `http_req_duration` p(95) | {{K6_P95_MS}} ms | — |
| `http_req_duration` p(99) | {{K6_P99_MS}} ms | — |
| `throttled_429` count | {{K6_THROTTLED_429}} | — |
| Critical route success rate (`{{K6_CRITICAL_METRIC}}`) | {{K6_CRITICAL_OK_RATE}} | > 90% |
| Non-critical success rate (`{{K6_NONCRITICAL_METRIC}}`) | {{K6_NONCRITICAL_OK_RATE}} | — |

### 5.2 Per-phase breakdown

| Phase | Critical route 200 | Critical route 429 | Non-critical 200 | Non-critical 429 | Avg latency (ms) |
|-------|-------------------|--------------------|------------------|------------------|------------------|
| Baseline | | | | | |
| Spike | | | | | |
| Cool-down | | | | | |

**k6 export command (optional JSON summary)**

```bash
k6 run -e BASE_URL=http://localhost:5000 \
  --summary-export=load-tests/results/k6-summary.json \
  load-tests/k6/gateway-microservices-stress.js
```

---

## 6. JMeter results

| Field | Value |
|-------|-------|
| Results file | `results.jtl` |
| HTML report | `ReportOutput/index.html` |
| Total samples | |
| Error % | |
| Throughput (req/s) | |
| Avg response time (ms) | |
| P99 response time (ms) | |

### Response code distribution (spike phase)

| Route | 200 | 429 | 5xx | Other |
|-------|-----|-----|-----|-------|
| Critical | | | | |
| Non-critical | | | | |

---

## 7. Hypothesis validation checklist

| # | Hypothesis | Result (Pass / Fail / Partial) | Evidence |
|---|------------|-------------------------------|----------|
| H1 | Posture escalates during spike | {{H1_RESULT}} | {{H1_EVIDENCE}} |
| H2 | Critical routes stay available | {{H2_RESULT}} | {{H2_EVIDENCE}} |
| H3 | Non-critical routes shed under Critical | {{H3_RESULT}} | {{H3_EVIDENCE}} |
| H4 | Posture recovers on cool-down | {{H4_RESULT}} | {{H4_EVIDENCE}} |
| H5 | Low middleware overhead | {{H5_RESULT}} | {{H5_EVIDENCE}} |

---

## 8. Comparison (optional - with vs without middleware)

| Metric | Without middleware | With predictive middleware | Delta |
|--------|-------------------|---------------------------|-------|
| P99 latency during spike | | | |
| Error rate during spike | | | |
| Critical route availability | | | |
| Backend CPU (if monitored) | | | |

---

## 9. Observations and limitations

**What worked**

-

**Unexpected behaviour**

-

**Limitations of this run**

- Single-node Docker deployment (no real K8s HPA)
- Synthetic traffic only
-

---

## 10. Conclusion (1–2 paragraphs for thesis)

_Summarise whether the AI-driven predictive middleware met its load-management objectives in this run. Reference specific numbers (posture transition time, 429 shedding rate, critical route availability)._

---

## Appendix — quick commands

```powershell
# Recommended: scaffold record, run test, auto-fill metrics
./run-evaluation.ps1 -Scenario Gateway -Tool k6
./run-evaluation.ps1 -Scenario StressTest -Tool k6
./run-evaluation.ps1 -Scenario StressTest -Tool JMeter

# Manual runs
docker compose up -d --build
k6 run -e BASE_URL=http://localhost:5000 load-tests/k6/gateway-microservices-stress.js
./run-loadtest.ps1
```
