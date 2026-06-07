# Predictive Middleware Load Tests

Two evaluation scenarios are supported:

| Scenario | Host | k6 script | Use case |
|----------|------|-----------|----------|
| **Isolated middleware** | `StressTest.Api` :5080 | `k6/predictive-middleware-stress.js` | Framework validation (Parts 2–4) |
| **Microservices ingress** | `ApiGateway` :5000 | `k6/gateway-microservices-stress.js` | Full thesis demo with real services |

## Recommended workflow

Use **`run-evaluation.ps1`** from the `AIScalingSolution` folder. It will:

1. Create a dated record in `evaluations/RUN-YYYY-MM-DD-##.md`
2. Start Docker (unless `-SkipDocker`)
3. Run k6 or JMeter
4. Capture gateway diagnostics and auto-fill metrics + hypothesis checklist
5. Open the completed record for you to review

```powershell
# Full thesis demo — ApiGateway + microservices
./run-evaluation.ps1 -Scenario Gateway -Tool k6

# Isolated middleware validation
./run-evaluation.ps1 -Scenario StressTest -Tool k6

# JMeter (uses run-loadtest.ps1 under the hood)
./run-evaluation.ps1 -Scenario StressTest -Tool JMeter

# Scaffold only — no Docker or load test
./run-evaluation.ps1 -Scenario Gateway -TemplateOnly
```

Manual template: [`EVALUATION-TEMPLATE.md`](./EVALUATION-TEMPLATE.md)

---

## Scenario A — Isolated middleware (StressTest.Api)

### Prerequisites

1. Redis on `localhost:6379`
2. `StressTest.Api` on port `5080` (or set `BASE_URL`)

### Start the test host

```bash
dotnet run --project StressTest/Api/StressTest.Api.csproj --urls http://localhost:5080
```

Or via Docker:

```bash
docker compose -f docker-compose.loadtest.yml up -d --build
```

Watch console diagnostics every 2 seconds:

```text
[TIMESTAMP] Posture: CRITICAL | Current RPS: 780 | Forecasted 60s RPS: 1200 | Throttled Requests: 342 | P99 Internal Overhead: 0.15ms
```

### Run k6

```bash
k6 run -e BASE_URL=http://localhost:5080 load-tests/k6/predictive-middleware-stress.js
```

| Route class | Endpoint |
|-------------|----------|
| Critical | `GET /api/v1/payment/checkout` |
| Non-critical | `GET /api/v1/promotions/ads` |

---

## Scenario B — ApiGateway + microservices (thesis demo)

### Prerequisites

1. Full stack running (includes Redis + predictive middleware on gateway):

```bash
docker compose up -d --build
```

2. Gateway healthy at `http://localhost:5000/health`

### Run k6

```bash
k6 run -e BASE_URL=http://localhost:5000 load-tests/k6/gateway-microservices-stress.js
```

| Route class | Gateway path | Backend |
|-------------|--------------|---------|
| Critical | `GET /orders` | OrderService |
| Critical | `GET /users` | UserService |
| Non-critical | `GET /products` | ProductService (shed under Critical posture) |

Watch `apigateway` container logs for posture diagnostics:

```bash
docker compose logs -f apigateway
```

---

## Traffic phases (both scenarios)

| Phase | Duration | Shape | Rate |
|-------|----------|-------|------|
| 1 Baseline | 30s | Nominal steady | 10 RPS |
| 2 Spike | 15s | Cliff burst | 800 RPS |
| 3 Cool-down | 45s | Recovery | 5 RPS |

---

## Apache JMeter

```powershell
./run-loadtest.ps1
```

Uses `LoadTestPlan.jmx` against port `5000` (StressTest.Api via `docker-compose.loadtest.yml`).

1. **Thread groups** — 3 phases matching the table above
2. **HTTP samplers** (parallel):
   - `GET ${BASE}/api/v1/payment/checkout` (critical)
   - `GET ${BASE}/api/v1/promotions/ads` (non-critical)
3. **Headers**: `User-Agent: Apache-HttpClient/JMeter`, `X-Correlation-Id: ${__threadNum}-${__time}`
4. **Output**: `results.jtl`, HTML report in `ReportOutput/`

---

## Evaluation

Copy [`EVALUATION-TEMPLATE.md`](./EVALUATION-TEMPLATE.md) for each run and fill in diagnostics, k6/JMeter metrics, and hypothesis validation.

Optional k6 JSON export:

```bash
k6 run -e BASE_URL=http://localhost:5000 \
  --summary-export=load-tests/results/k6-summary.json \
  load-tests/k6/gateway-microservices-stress.js
```
