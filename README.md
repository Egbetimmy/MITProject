# MIT AI-Driven Microservices Auto-Scaling & Gating Project

This repository contains a full-stack proof-of-concept e-commerce microservices platform demonstrating **proactive load forecasting and adaptive traffic gating/route shedding** using ML.NET, YARP, and React. 

The architecture is split into two primary components:
1. **[AIScalingSolution](./AIScalingSolution)**: A .NET Core 10 microservices solution containing individual domain APIs, a centralized API Gateway, a background resource monitoring service, and an ML-based regression forecaster.
2. **[frontend](./frontend)**: A standalone React + Vite client portal offering a customer storefront (**AuraStore**) and an operator diagnostics dashboard (**Operator Panel**).

---

## 🏗️ Architecture Overview

The system operates reactively to incoming load and predicted traffic accelerations:
- **Telemetry Capture:** Every HTTP request goes through custom telemetry middleware that writes lightweight ticks to **Redis** on a fire-and-forget background queue.
- **Resource Monitoring:** The `MonitoringService` polls all active APIs every 30 seconds for CPU/Memory metrics and records them into a **SQL Server** database.
- **AI Forecasting:** The `PredictionService` evaluates historical database logs using an SDCA regression model to predict the requests-per-second (RPS) load 60 seconds into the future.
- **Protective Posturing:** Based on the forecast, the system shifts its posture:
  - **Nominal:** System is fully healthy. Standard rate limits apply.
  - **Alert:** Potential spike ahead. Core API cache keys are pre-warmed.
  - **Critical:** Imminent overload. Non-critical routes (like product browsing `/products`) are shed with HTTP 429 errors. Critical routes (like checking out `/orders`) bypass constraints to guarantee transaction completions.

---

## 🚀 Getting Started

### Prerequisites
- [Docker & Docker Compose](https://www.docker.com/)
- [Node.js (v18+)](https://nodejs.org/)
- [.NET 8.0/10.0 SDK](https://dotnet.microsoft.com/download) (optional, for local C# compilation)

---

### Step 1: Start the Backend Microservices Stack

Run the stack in detached mode. This launches SqlServer, Redis, YARP Gateway, and the microservices:

```powershell
# Navigate to the backend directory
cd AIScalingSolution

# Build and start the containers
docker compose up -d --build
```

Verify that the stack is healthy by querying the API Gateway health endpoint at:
👉 **[http://localhost:5000/health](http://localhost:5000/health)**

---

### Step 2: Start the Standalone React Frontend

The React app communicates with the gateway through a local dev proxy to prevent CORS issues.

```powershell
# Open a new terminal, navigate to the frontend directory
cd frontend

# Install package dependencies (React, Chart.js, Vite)
npm install

# Start the Vite development server
npm run dev
```

Open your browser and navigate to:
👉 **[http://localhost:5173](http://localhost:5173)**

---

## 🔍 How to Test Gating and Load Shedding

1. Open the storefront at **[http://localhost:5173](http://localhost:5173)**. Select a customer user profile, browse products, and add a few items to your cart.
2. Click the **Operator Panel** tab in the top navbar.
3. In the **Simulate Traffic** card, click the **Spike (110 RPS)** preset button. This starts generating high-frequency synthetic requests from your browser.
4. Watch the **Live Traffic Acceleration Chart** update. The line graph will register the spike and show the AI model's forecast.
5. Watch the **Protective State Card** transition from **Nominal** (Green) ➔ **Alert** (Amber) ➔ **Critical** (Red pulsing).
6. Click back to the **Storefront** tab:
   - Notice the system warning banner slide down at the top.
   - Try to refresh the catalog. You will see a **"Catalog Offline (HTTP 429 Shedding)"** screen. The gateway is protecting database capacity by shedding `/products`.
   - Open your cart drawer and click **"Complete Checkout"**.
   - Notice that the checkout **succeeds immediately** and displays your order receipts. This proves that the critical `/orders` path is isolated and protected.
7. Go back to the **Operator Panel** tab and click **Stop** to halt simulator traffic. The posture will return to **Nominal** once the cooldown window finishes.

---

## 📂 Repository Structure

```
├── AIScalingSolution/          # .NET Core Microservices Solution
│   ├── ApiGateway/             # YARP Proxy with Predictive Telemetry Middleware
│   ├── UserService/            # User Accounts Domain Service
│   ├── ProductService/         # Product Inventory Catalog Domain Service
│   ├── OrderService/           # Checkout Order Processing Domain Service
│   ├── MonitoringService/      # Telemetry Database Collector Background Worker
│   ├── PredictionService/      # ML.NET SDCA Model Trainer & Forecaster
│   ├── Shared/                 # Common DTOs and Shared Libraries
│   ├── StressTest/             # Isolated Gating Validation Controller
│   └── docker-compose.yml      # Multi-container Compose Stack
│
└── frontend/                   # React + Vite Standalone Client
    ├── src/
    │   ├── App.jsx             # Unified Storefront & Diagnostics Operator UI
    │   ├── index.css           # Custom Glassmorphic Dark-Theme Styles
    │   └── main.jsx            # React Entry Point
    ├── vite.config.js          # Vite Server Proxy Rules
    └── package.json            # Node Package Configuration
```