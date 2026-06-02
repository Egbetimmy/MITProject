# Predictive Middleware Load Tests (Part 4)

## Prerequisites

1. Redis running on `localhost:6379`
2. StressTest.Api running on port `5080` (or set `BASE_URL`)

## Start the test host

```bash
dotnet run --project StressTest/Api/StressTest.Api.csproj --urls http://localhost:5080
```

Watch the console for diagnostics lines every 2 seconds:

```text
[TIMESTAMP] Posture: CRITICAL | Current RPS: 780 | Forecasted 60s RPS: 1200 | Throttled Requests: 342 | P99 Internal Overhead: 0.15ms
```

## Run K6 (recommended)

```bash
k6 run -e BASE_URL=http://localhost:5080 load-tests/k6/predictive-middleware-stress.js
```

### Traffic phases

| Phase | Duration | Shape | Rate |
|-------|----------|-------|------|
| 1 Baseline | 30s | Nominal steady | 10 RPS |
| 2 Spike | 15s | Cliff burst | 800 RPS |
| 3 Cool-down | 45s | Recovery | 5 RPS |

## Apache JMeter outline

1. **Thread Group** → Ultimate Thread Group with 3 stages:
   - 0–30s: 10 threads, 10 loops/s
   - 30–45s: 800 threads ramped instantly, constant throughput 800/s
   - 45–90s: 5 threads, 5 loops/s
2. **HTTP Samplers** (parallel):
   - `GET ${BASE}/api/v1/payment/checkout`
   - `GET ${BASE}/api/v1/promotions/ads`
3. **Headers**: `User-Agent: Apache-HttpClient/JMeter`, `X-Correlation-Id: ${__threadNum}-${__time}`
4. **Listeners**: Aggregate Report + Response Codes per second
