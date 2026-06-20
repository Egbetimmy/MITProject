# AIScalingSolution: .NET Core Microservices Backend

This directory houses the .NET Core microservices backend stack for the AI-scaling and adaptive traffic mitigation system.

---

## 🛠️ Service Architecture

The system consists of six primary services orchestrated via Docker:

1. **ApiGateway (Port 5000):** Built on YARP (Yet Another Reverse Proxy). It intercepts all incoming requests, runs the telemetry buffers, registers load levels, and gates non-critical paths during heavy workload spikes.
2. **UserService (Internal):** Core CRUD microservice storing user details inside a SQL Server DB.
3. **ProductService (Internal):** Catalog service maintaining product lists. Gated as a *non-critical* route during stress peaks.
4. **OrderService (Internal):** Process e-commerce order submissions. Cataloged as a *critical* route that remains open during overload.
5. **MonitoringService (Internal):** Polls all active endpoints every 30 seconds to fetch runtime resource telemetry and logs them into a SQL database.
6. **PredictionService (Internal):** Hosts the ML.NET SDCA regression model, predicting future RPS rates and outputting scaling recommendations.

---

## 🚀 Running the Stack

You can run the backend stack in two configurations:

### 1. Standard Gateway Stack (Recommended)
This runs the full microservices cluster:
```powershell
docker compose -f docker-compose.yml up -d --build
```
Endpoints:
- **API Gateway (Ingress):** `http://localhost:5000`
- **Gateway Swagger Docs:** `http://localhost:5000/swagger`

### 2. Isolated Middleware Stack
This runs a single standalone API (`StressTest.Api`) with the predictive middleware attached to validate posture transitions in isolation:
```powershell
docker compose -f docker-compose.loadtest.yml up -d --build
```

---

## 📈 Simulating Load via CLI

The folder contains pre-configured load-testing scripts to stress the APIs and trigger scaling actions:

### JMeter Simulation
Runs the JMeter `.jmx` plan to generate a workload burst:
```powershell
# Runs headless JMeter stress test and compiles HTML dashboard report
.\run-loadtest.ps1
```

### k6 Simulation
Evaluates telemetry response times and mitigation under pressure:
```powershell
# Runs k6 stress script and updates the evaluations markdown logs
.\run-evaluation.ps1 -Scenario Gateway -Tool k6
```

---

## ⚙️ Middleware Settings

Middleware behaviors (such as rate limits and non-critical pathways) are defined in [ApiGateway/appsettings.json](./ApiGateway/appsettings.json):
- `PredictiveMiddleware:NonCriticalRoutePrefixes`: Routes to shed under critical load (default: `["/products"]`).
- `PredictiveMiddleware:Engine`: Tuning variables for ML forecasting cycles.
- `PredictiveMiddleware:Mitigation`: Allowed requests-per-minute configurations across postures for authenticated vs. unauthenticated clients.
