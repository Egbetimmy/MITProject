# AuraShop Standalone React + Vite Frontend

This folder contains the standalone client application for the MIT AI-Scaling microservices project. It is built as a unified Single Page Application (SPA) using **React (v19)**, **Vite**, and **Chart.js** for real-time telemetry rendering.

The application serves two user roles:
1. **AuraStore (Customer View):** Browse products from the catalog, add items to the cart drawer, and complete order checkouts. Shows status banners and catalog suspension fallbacks during high system load.
2. **Operator Panel (Admin View):** View live system postures, monitor real-time RPS graphs, trigger simulated load spikes, review historical metrics database logs, and command the AI model trainer.

---

## 🛠️ Configuration & Proxy Setup

During local development, relative fetch URLs (like `/users`, `/products`, `/orders`) must be routed to the C# API Gateway on `http://localhost:5000`.

To handle this cleanly without CORS errors, [vite.config.js](./vite.config.js) is configured with server proxies:

```javascript
server: {
  port: 5173,
  proxy: {
    '/users': 'http://localhost:5000',
    '/products': 'http://localhost:5000',
    '/orders': 'http://localhost:5000',
    '/monitoring': 'http://localhost:5000',
    '/prediction': 'http://localhost:5000',
    '/api': 'http://localhost:5000'
  }
}
```

---

## 🚀 Running Locally

### 1. Install Dependencies
Run from this folder (`/frontend/`) to restore dependencies (React, ReactDOM, Chart.js, ESLint):
```bash
npm install
```

### 2. Start Dev Server
Spin up the Vite development server:
```bash
npm run dev
```
Open the output URL in your browser:
👉 **[http://localhost:5173](http://localhost:5173)**

---

## 📦 Available Scripts

In the project directory, you can run:

- `npm run dev`: Starts the local development server with HMR.
- `npm run build`: Compiles and bundles the application assets into `/dist` for production deployment.
- `npm run preview`: Locally previews the compiled production build.
- `npm run lint`: Audits the codebase for React Hooks and syntax standards.

---

## 📂 Source Code Structure

- **[index.html](./index.html)**: Main HTML structure, loads Google Fonts (Outfit & Space Grotesk) and sets up the canvas container background elements.
- **[src/main.jsx](./src/main.jsx)**: React root compiler attachment.
- **[src/App.jsx](./src/App.jsx)**: Consolidated state manager and UI components. Holds simulated traffic threads, user cart contexts, and fetches telemetries.
- **[src/index.css](./src/index.css)**: CSS style system implementing premium dark-theme layouts, glassmorphic widgets, and glowing animations.
