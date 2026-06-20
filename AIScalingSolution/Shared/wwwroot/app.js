// Global App State
const state = {
    mode: 'detecting', // 'gateway' or 'stress-test'
    apiBase: '', // empty means same origin
    liveChart: null,
    intensityRps: 0,
    routeMix: 'balanced',
    clientAuth: 'unauth',
    simStats: { sent: 0, success: 0, throttled: 0 },
    simTimer: null,
    chartData: {
        labels: [],
        rps: [],
        forecast: []
    },
    // Cache for CRUD dropdowns
    users: [],
    products: []
};

// DOM Elements
const elements = {
    connectionBadge: document.getElementById('connectionBadge'),
    detectedMode: document.getElementById('detectedMode'),
    navExplorer: document.getElementById('navExplorer'),
    navMlModel: document.getElementById('navMlModel'),
    pageTitle: document.getElementById('pageTitle'),
    
    // Posture & Stats
    postureCard: document.getElementById('postureCard'),
    postureBadge: document.getElementById('postureBadge'),
    postureDesc: document.getElementById('postureDesc'),
    postureChanged: document.getElementById('postureChanged'),
    statRps: document.getElementById('statRps'),
    statForecast: document.getElementById('statForecast'),
    statThrottled: document.getElementById('statThrottled'),
    statOverhead: document.getElementById('statOverhead'),
    
    // Simulator & Console
    routeMix: document.getElementById('routeMix'),
    clientAuth: document.getElementById('clientAuth'),
    simCount: document.getElementById('simCount'),
    simSuccess: document.getElementById('simSuccess'),
    simThrottled: document.getElementById('simThrottled'),
    terminalConsole: document.getElementById('terminalConsole'),
    clearTerminalBtn: document.getElementById('clearTerminalBtn'),
    
    // Explorer Tabs and Panels
    explorerTabs: document.getElementById('explorerTabs'),
    tabUsersBtn: document.getElementById('tabUsersBtn'),
    tabProductsBtn: document.getElementById('tabProductsBtn'),
    tabOrdersBtn: document.getElementById('tabOrdersBtn'),
    tabStressBtn: document.getElementById('tabStressBtn'),
    panelUsers: document.getElementById('panel-users'),
    panelProducts: document.getElementById('panel-products'),
    panelOrders: document.getElementById('panel-orders'),
    panelStress: document.getElementById('panel-stress'),
    
    // Explorer Forms & Tables
    createUserForm: document.getElementById('createUserForm'),
    userName: document.getElementById('userName'),
    userEmail: document.getElementById('userEmail'),
    usersTable: document.getElementById('usersTable'),
    
    createProductForm: document.getElementById('createProductForm'),
    prodName: document.getElementById('prodName'),
    prodSku: document.getElementById('prodSku'),
    prodPrice: document.getElementById('prodPrice'),
    productsTable: document.getElementById('productsTable'),
    
    createOrderForm: document.getElementById('createOrderForm'),
    orderUserId: document.getElementById('orderUserId'),
    orderProductId: document.getElementById('orderProductId'),
    orderQuantity: document.getElementById('orderQuantity'),
    ordersTable: document.getElementById('ordersTable'),
    
    btnCheckoutFlow: document.getElementById('btnCheckoutFlow'),
    btnAdsFlow: document.getElementById('btnAdsFlow'),
    
    // Metrics
    metricsTable: document.getElementById('metricsTable'),
    refreshMetricsBtn: document.getElementById('refreshMetricsBtn'),
    
    // ML Model
    modelReadinessIndicator: document.getElementById('modelReadinessIndicator'),
    modelStateText: document.getElementById('modelStateText'),
    modelSource: document.getElementById('modelSource'),
    btnTrainModel: document.getElementById('btnTrainModel'),
    mlConsoleLog: document.getElementById('mlConsoleLog'),
    
    refreshBtn: document.getElementById('refreshBtn')
};

// Initialize Application
document.addEventListener('DOMContentLoaded', async () => {
    initTabs();
    initChart();
    await detectStack();
    
    // Set up polling for diagnostics
    setInterval(pollDiagnostics, 1000);
    pollDiagnostics(); // Initial poll
    
    // Bind Event Listeners
    elements.refreshBtn.addEventListener('click', pollDiagnostics);
    elements.clearTerminalBtn.addEventListener('click', clearConsole);
    
    // Traffic simulator intensity selection
    document.querySelectorAll('.intensity-btn').forEach(btn => {
        btn.addEventListener('click', (e) => {
            document.querySelectorAll('.intensity-btn').forEach(b => b.classList.remove('active'));
            btn.classList.add('active');
            const rps = parseInt(btn.dataset.rps);
            setSimulatorIntensity(rps);
        });
    });
    
    // Route mix & Auth selector bindings
    elements.routeMix.addEventListener('change', (e) => state.routeMix = e.target.value);
    elements.clientAuth.addEventListener('change', (e) => state.clientAuth = e.target.value);
    
    // Forms submissions
    elements.createUserForm.addEventListener('submit', handleAddUser);
    elements.createProductForm.addEventListener('submit', handleAddProduct);
    elements.createOrderForm.addEventListener('submit', handleAddOrder);
    
    // Stress endpoints manual triggers
    elements.btnCheckoutFlow.addEventListener('click', () => triggerStressEndpoint('payment/checkout', 'critical'));
    elements.btnAdsFlow.addEventListener('click', () => triggerStressEndpoint('promotions/ads', 'noncritical'));
    
    // Refresh buttons
    elements.refreshMetricsBtn.addEventListener('click', loadHistoricalMetrics);
    elements.btnTrainModel.addEventListener('click', triggerModelTraining);
});

// Setup Tab Navigation
function initTabs() {
    document.querySelectorAll('.nav-item').forEach(btn => {
        btn.addEventListener('click', () => {
            document.querySelectorAll('.nav-item').forEach(b => b.classList.remove('active'));
            btn.classList.add('active');
            
            const targetTab = btn.dataset.tab;
            document.querySelectorAll('.tab-content').forEach(panel => {
                panel.classList.remove('active');
            });
            document.getElementById(`tab-${targetTab}`).classList.add('active');
            
            // Tab specific triggers
            if (targetTab === 'metrics') {
                loadHistoricalMetrics();
            }
        });
    });

    // Sub-navigation in Explorer tab
    document.querySelectorAll('.exp-tab').forEach(tabBtn => {
        tabBtn.addEventListener('click', () => {
            document.querySelectorAll('.exp-tab').forEach(tb => tb.classList.remove('active'));
            tabBtn.classList.add('active');
            
            const panelId = tabBtn.dataset.exp;
            document.querySelectorAll('.explorer-panel').forEach(panel => {
                panel.classList.remove('active');
            });
            document.getElementById(`panel-${panelId}`).classList.add('active');
        });
    });
}

// Detect which stack is running
async function detectStack() {
    logConsole('System', 'Detecting stack configuration at root /health...');
    try {
        const response = await fetch(`${state.apiBase}/health`);
        const data = await response.json();
        
        if (data.service === 'ApiGateway') {
            state.mode = 'gateway';
            elements.detectedMode.textContent = 'API Gateway Mode';
            elements.connectionBadge.className = 'connection-status success-status';
            
            // Show gateway components
            elements.tabUsersBtn.style.display = 'block';
            elements.tabProductsBtn.style.display = 'block';
            elements.tabOrdersBtn.style.display = 'block';
            elements.tabStressBtn.style.display = 'none';
            
            elements.navExplorer.style.display = 'flex';
            elements.navMlModel.style.display = 'flex';
            elements.pageTitle.textContent = 'System Telemetry Control Room';
            
            logConsole('System', 'Connected to API Gateway. Microservices stack active.');
            
            // Load CRUD data
            loadCrudData();
        } else if (data.service === 'StressTest.Api') {
            state.mode = 'stress-test';
            elements.detectedMode.textContent = 'Stress Test Mode';
            elements.connectionBadge.className = 'connection-status warning-status';
            
            // Show isolated stress endpoints panel
            elements.tabUsersBtn.style.display = 'none';
            elements.tabProductsBtn.style.display = 'none';
            elements.tabOrdersBtn.style.display = 'none';
            elements.tabStressBtn.style.display = 'block';
            elements.tabStressBtn.click(); // Select the stress tab
            
            elements.navMlModel.style.display = 'none'; // ML service isn't local to stress test
            elements.pageTitle.textContent = 'Predictive Middleware Stress Panel';
            
            logConsole('System', 'Connected to Isolated StressTest API. Part 4 Gating validation active.');
        } else {
            throw new Error('Unknown service type');
        }
    } catch (err) {
        state.mode = 'unknown';
        elements.detectedMode.textContent = 'Offline / Standalone';
        elements.connectionBadge.className = 'connection-status error-status';
        logConsole('System', 'Error detecting running backend. Check docker containers. Running in simulation fallback.', true);
    }
}

// Polling live Diagnostics
let lastStateFetched = null;
async function pollDiagnostics() {
    try {
        const response = await fetch(`${state.apiBase}/api/diagnostics`);
        if (!response.ok) throw new Error('Failed to fetch diagnostics');
        
        const data = await response.json();
        updateDiagnosticsUI(data);
    } catch (err) {
        // Fallback mock simulation values in standalone mode
        if (state.mode === 'unknown') {
            simulateStandaloneDiagnostics();
        }
    }
}

// Update Diagnostics UI
function updateDiagnosticsUI(data) {
    // Posture mapping
    const posture = data.posture || 'Nominal';
    elements.postureBadge.textContent = posture;
    
    // Posture changes / details
    if (posture === 'Nominal') {
        elements.postureCard.style.borderLeftColor = 'var(--color-nominal)';
        elements.postureCard.style.boxShadow = '0 8px 32px 0 var(--glow-nominal)';
        elements.postureBadge.style.color = 'var(--color-nominal)';
        elements.postureDesc.textContent = 'System is running within normal thresholds. Non-critical routes are fully available.';
    } else if (posture === 'Alert') {
        elements.postureCard.style.borderLeftColor = 'var(--color-alert)';
        elements.postureCard.style.boxShadow = '0 8px 32px 0 var(--glow-alert)';
        elements.postureBadge.style.color = 'var(--color-alert)';
        elements.postureDesc.textContent = 'Proactive warning posture activated. AI model forecasts high incoming load. Core services cached.';
    } else if (posture === 'Critical') {
        elements.postureCard.style.borderLeftColor = 'var(--color-critical)';
        elements.postureCard.style.boxShadow = '0 8px 32px 0 var(--glow-critical)';
        elements.postureBadge.style.color = 'var(--color-critical)';
        elements.postureDesc.textContent = 'System protective action is active! Non-critical routes (e.g. Products / Ads) are shed with HTTP 429.';
    }
    
    // Format timestamp differences
    if (data.postureChangedAt) {
        const diff = Math.max(0, Math.floor((Date.now() - new Date(data.postureChangedAt).getTime()) / 1000));
        elements.postureChanged.textContent = `${diff}s ago`;
    }
    
    // Numeric stats
    elements.statRps.innerHTML = `${(data.currentRps || 0).toFixed(1)} <span class="unit">RPS</span>`;
    elements.statForecast.innerHTML = `${(data.forecastedRps || 0).toFixed(1)} <span class="unit">RPS</span>`;
    elements.statThrottled.textContent = data.throttledRequests || 0;
    elements.statOverhead.innerHTML = `${(data.p99OverheadMs || 0).toFixed(2)} <span class="unit">ms</span>`;
    
    // ML Readiness
    if (state.mode === 'gateway') {
        const isReady = (data.forecastedRps !== undefined);
        elements.modelReadinessIndicator.textContent = isReady ? 'Model Active' : 'Model Training Required';
        elements.modelReadinessIndicator.className = isReady ? 'status-indicator ready' : 'status-indicator alert-text';
        elements.modelStateText.textContent = isReady ? 'Ready & Serving' : 'Awaiting Training';
    }
    
    // Update live graph
    updateChart(data.currentRps || 0, data.forecastedRps || 0);
}

// Standalone Simulator Fallback
let mockRpsHistory = 0;
let mockThrottled = 0;
function simulateStandaloneDiagnostics() {
    const data = {
        posture: 'Nominal',
        postureChangedAt: new Date(),
        currentRps: state.intensityRps + (Math.random() - 0.5) * 2,
        forecastedRps: state.intensityRps > 0 ? (state.intensityRps * 1.25) : 0,
        throttledRequests: mockThrottled,
        p99OverheadMs: 0.12 + Math.random() * 0.05
    };
    
    // Simulate posture logic
    if (data.currentRps > 70 || data.forecastedRps > 80) {
        data.posture = 'Critical';
    } else if (data.currentRps > 30 || data.forecastedRps > 40) {
        data.posture = 'Alert';
    }
    
    if (data.currentRps < 0) data.currentRps = 0;
    
    updateDiagnosticsUI(data);
}

// Set up Live Chart using Chart.js
function initChart() {
    const ctx = document.getElementById('liveChart').getContext('2d');
    
    // Initialize empty labels/data
    for (let i = 29; i >= 0; i--) {
        state.chartData.labels.push(`${i}s ago`);
        state.chartData.rps.push(0);
        state.chartData.forecast.push(0);
    }
    
    state.liveChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: state.chartData.labels,
            datasets: [
                {
                    label: 'Current RPS',
                    data: state.chartData.rps,
                    borderColor: '#3b82f6',
                    backgroundColor: 'rgba(59, 130, 246, 0.05)',
                    borderWidth: 2,
                    tension: 0.3,
                    fill: true,
                    pointRadius: 0
                },
                {
                    label: 'Forecasted RPS',
                    data: state.chartData.forecast,
                    borderColor: '#a855f7',
                    backgroundColor: 'transparent',
                    borderWidth: 2,
                    borderDash: [5, 5],
                    tension: 0.3,
                    pointRadius: 0
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { display: false }
            },
            scales: {
                x: {
                    grid: { display: false },
                    ticks: { color: '#64748b', font: { size: 10 } }
                },
                y: {
                    min: 0,
                    suggestedMax: 120,
                    grid: { color: 'rgba(255,255,255,0.03)' },
                    ticks: { color: '#64748b', font: { size: 10 } }
                }
            }
        }
    });
}

function updateChart(rps, forecast) {
    if (!state.liveChart) return;
    
    state.chartData.rps.push(rps);
    state.chartData.forecast.push(forecast);
    
    state.chartData.rps.shift();
    state.chartData.forecast.shift();
    
    state.liveChart.update('none'); // Update without animation for performance
}

// Traffic Simulator Engine
function setSimulatorIntensity(rps) {
    state.intensityRps = rps;
    
    if (state.simTimer) {
        clearInterval(state.simTimer);
        state.simTimer = null;
    }
    
    if (rps === 0) {
        logConsole('Simulator', 'Traffic simulator stopped.');
        return;
    }
    
    logConsole('Simulator', `Traffic simulator started at intensity ${rps} RPS (Mix: ${state.routeMix}, Auth: ${state.clientAuth}).`);
    
    // Interval calculated in ms
    const interval = 1000 / rps;
    state.simTimer = setInterval(fireSimulatedRequest, interval);
}

async function fireSimulatedRequest() {
    // Determine path
    let path = '';
    let method = 'GET';
    let body = null;
    
    if (state.mode === 'gateway') {
        const isCritical = state.routeMix === 'critical' || (state.routeMix === 'balanced' && Math.random() > 0.6);
        if (isCritical) {
            method = 'POST';
            path = '/orders';
            
            // Pick a random user/product or default to ID 1
            const userId = state.users.length > 0 ? state.users[Math.floor(Math.random() * state.users.length)].id : 1;
            const productId = state.products.length > 0 ? state.products[Math.floor(Math.random() * state.products.length)].id : 1;
            body = JSON.stringify({ userId, productId, quantity: 1 });
        } else {
            // Non-critical endpoint
            path = '/products';
        }
    } else {
        // Stress test mode
        const isCritical = state.routeMix === 'critical' || (state.routeMix === 'balanced' && Math.random() > 0.5);
        path = isCritical ? '/api/v1/payment/checkout' : '/api/v1/promotions/ads';
    }
    
    state.simStats.sent++;
    elements.simCount.textContent = state.simStats.sent;
    
    const started = performance.now();
    const headers = {
        'Content-Type': 'application/json',
        'X-Correlation-Id': `browser-sim-${Math.floor(Math.random() * 100000)}`
    };
    
    if (state.clientAuth === 'auth') {
        headers['Authorization'] = 'Bearer simulated-dashboard-user-token';
    }

    try {
        const url = `${state.apiBase}${path}`;
        const response = await fetch(url, {
            method,
            headers,
            body
        });
        
        const elapsed = Math.round(performance.now() - started);
        const status = response.status;
        
        if (status === 200 || status === 201 || status === 204) {
            state.simStats.success++;
            elements.simSuccess.textContent = state.simStats.success;
            logConsole('Traffic', `[OK] ${method} ${path} -> ${status} (${elapsed}ms)`);
        } else if (status === 429) {
            state.simStats.throttled++;
            elements.simThrottled.textContent = state.simStats.throttled;
            logConsole('Traffic', `[SHED] ${method} ${path} -> 429 Too Many Requests (${elapsed}ms)`, true);
            mockThrottled++; // Increment standalone diagnostic mock throttle
        } else {
            logConsole('Traffic', `[FAIL] ${method} ${path} -> ${status} (${elapsed}ms)`, true);
        }
    } catch (err) {
        const elapsed = Math.round(performance.now() - started);
        logConsole('Traffic', `[ERR] ${method} ${path} failed: ${err.message} (${elapsed}ms)`, true);
    }
}

// Console Logging Helper
function logConsole(source, message, isError = false) {
    const time = new Date().toLocaleTimeString();
    const line = document.createElement('div');
    line.className = `term-line ${isError ? 'error-line' : source === 'System' || source === 'Simulator' ? 'system-line' : 'success-line'}`;
    line.textContent = `[${time}] [${source}] ${message}`;
    
    elements.terminalConsole.appendChild(line);
    
    // Auto scroll to bottom
    elements.terminalConsole.scrollTop = elements.terminalConsole.scrollHeight;
    
    // Cap log items in DOM
    while (elements.terminalConsole.children.length > 100) {
        elements.terminalConsole.removeChild(elements.terminalConsole.firstChild);
    }
}

function clearConsole() {
    elements.terminalConsole.innerHTML = '<div class="term-line system-line">[SYSTEM] Console cleared.</div>';
    state.simStats = { sent: 0, success: 0, throttled: 0 };
    elements.simCount.textContent = 0;
    elements.simSuccess.textContent = 0;
    elements.simThrottled.textContent = 0;
    mockThrottled = 0;
}

// Load CRUD lists for API Explorer
async function loadCrudData() {
    try {
        // Users
        const resUsers = await fetch(`${state.apiBase}/users`);
        const jsonUsers = await resUsers.json();
        state.users = jsonUsers.data || [];
        populateUsersTable(state.users);
        
        // Products
        const resProds = await fetch(`${state.apiBase}/products`);
        const jsonProds = await resProds.json();
        state.products = jsonProds.data || [];
        populateProductsTable(state.products);
        
        // Orders
        const resOrders = await fetch(`${state.apiBase}/orders`);
        const jsonOrders = await resOrders.json();
        populateOrdersTable(jsonOrders.data || []);
        
        // Populate select lists
        updateDropdowns();
    } catch (err) {
        console.error('Failed to load explorer lists', err);
    }
}

function populateUsersTable(users) {
    const tbody = elements.usersTable.querySelector('tbody');
    tbody.innerHTML = '';
    if (users.length === 0) {
        tbody.innerHTML = '<tr><td colspan="4" class="text-center">No active users.</td></tr>';
        return;
    }
    
    users.forEach(u => {
        const tr = document.createElement('tr');
        tr.innerHTML = `
            <td>${u.id}</td>
            <td><strong>${escapeHtml(u.name)}</strong></td>
            <td>${escapeHtml(u.email)}</td>
            <td><button class="btn btn-sm btn-secondary" onclick="deleteUser(${u.id})">❌ Delete</button></td>
        `;
        tbody.appendChild(tr);
    });
}

function populateProductsTable(products) {
    const tbody = elements.productsTable.querySelector('tbody');
    tbody.innerHTML = '';
    if (products.length === 0) {
        tbody.innerHTML = '<tr><td colspan="5" class="text-center">No products in catalog.</td></tr>';
        return;
    }
    
    products.forEach(p => {
        const tr = document.createElement('tr');
        tr.innerHTML = `
            <td>${p.id}</td>
            <td><strong>${escapeHtml(p.name)}</strong></td>
            <td><code>${escapeHtml(p.sku)}</code></td>
            <td>$${p.price.toFixed(2)}</td>
            <td><button class="btn btn-sm btn-secondary" onclick="deleteProduct(${p.id})">❌ Delete</button></td>
        `;
        tbody.appendChild(tr);
    });
}

function populateOrdersTable(orders) {
    const tbody = elements.ordersTable.querySelector('tbody');
    tbody.innerHTML = '';
    if (orders.length === 0) {
        tbody.innerHTML = '<tr><td colspan="6" class="text-center">No order records processed.</td></tr>';
        return;
    }
    
    orders.forEach(o => {
        const tr = document.createElement('tr');
        tr.innerHTML = `
            <td>${o.id}</td>
            <td>User #${o.userId}</td>
            <td>Product #${o.productId}</td>
            <td>${o.quantity}</td>
            <td><strong>$${(o.totalPrice || 0).toFixed(2)}</strong></td>
            <td>${new Date(o.createdAt || Date.now()).toLocaleTimeString()}</td>
        `;
        tbody.appendChild(tr);
    });
}

function updateDropdowns() {
    elements.orderUserId.innerHTML = '<option value="">Select a user...</option>';
    state.users.forEach(u => {
        elements.orderUserId.innerHTML += `<option value="${u.id}">${escapeHtml(u.name)} (ID: ${u.id})</option>`;
    });
    
    elements.orderProductId.innerHTML = '<option value="">Select a product...</option>';
    state.products.forEach(p => {
        elements.orderProductId.innerHTML += `<option value="${p.id}">${escapeHtml(p.name)} ($${p.price.toFixed(2)})</option>`;
    });
}

// CRUD Operations
async function handleAddUser(e) {
    e.preventDefault();
    const payload = { name: elements.userName.value, email: elements.userEmail.value };
    try {
        const res = await fetch(`${state.apiBase}/users`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });
        if (res.ok) {
            elements.userName.value = '';
            elements.userEmail.value = '';
            loadCrudData();
        } else {
            alert('Failed to add user: ' + res.statusText);
        }
    } catch (err) {
        alert('Error: ' + err.message);
    }
}

async function handleAddProduct(e) {
    e.preventDefault();
    const payload = { 
        name: elements.prodName.value, 
        sku: elements.prodSku.value, 
        price: parseFloat(elements.prodPrice.value) 
    };
    try {
        const res = await fetch(`${state.apiBase}/products`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });
        if (res.status === 429) {
            alert('❌ Request Shedded (429)! Cannot add products while system posture is CRITICAL.');
            return;
        }
        if (res.ok) {
            elements.prodName.value = '';
            elements.prodSku.value = '';
            elements.prodPrice.value = '';
            loadCrudData();
        } else {
            alert('Failed to add product: ' + res.statusText);
        }
    } catch (err) {
        alert('Error: ' + err.message);
    }
}

async function handleAddOrder(e) {
    e.preventDefault();
    const payload = {
        userId: parseInt(elements.orderUserId.value),
        productId: parseInt(elements.orderProductId.value),
        quantity: parseInt(elements.orderQuantity.value)
    };
    try {
        const res = await fetch(`${state.apiBase}/orders`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });
        if (res.ok) {
            elements.orderQuantity.value = '1';
            loadCrudData();
        } else {
            alert('Failed to process order: ' + res.statusText);
        }
    } catch (err) {
        alert('Error: ' + err.message);
    }
}

// Global functions for inline delete buttons
window.deleteUser = async function(id) {
    if (!confirm(`Delete user ${id}?`)) return;
    try {
        await fetch(`${state.apiBase}/users/${id}`, { method: 'DELETE' });
        loadCrudData();
    } catch (err) {
        alert(err.message);
    }
}

window.deleteProduct = async function(id) {
    if (!confirm(`Delete product ${id}?`)) return;
    try {
        const res = await fetch(`${state.apiBase}/products/${id}`, { method: 'DELETE' });
        if (res.status === 429) {
            alert('❌ Request Shedded (429)! Cannot delete products while system posture is CRITICAL.');
            return;
        }
        loadCrudData();
    } catch (err) {
        alert(err.message);
    }
}

// Isolated Stress Mode Triggers
async function triggerStressEndpoint(subpath, classification) {
    logConsole('Stress', `Triggering manual request to /api/v1/${subpath}...`);
    try {
        const start = performance.now();
        const res = await fetch(`${state.apiBase}/api/v1/${subpath}`);
        const elapsed = Math.round(performance.now() - start);
        const data = await res.json();
        
        if (res.ok) {
            logConsole('Stress', `[OK] /api/v1/${subpath} -> 200 OK (${elapsed}ms). Payload: ${JSON.stringify(data)}`);
        } else {
            logConsole('Stress', `[SHED] /api/v1/${subpath} -> ${res.status} (${elapsed}ms). Payload: ${JSON.stringify(data)}`, true);
        }
    } catch (err) {
        logConsole('Stress', `[FAIL] /api/v1/${subpath} -> Failed: ${err.message}`, true);
    }
}

// Historical Telemetry Database loader
async function loadHistoricalMetrics() {
    const tbody = elements.metricsTable.querySelector('tbody');
    tbody.innerHTML = '<tr><td colspan="6" class="text-center">Loading database metric history...</td></tr>';
    
    try {
        const res = await fetch(`${state.apiBase}/monitoring/metrics`);
        const json = await res.json();
        const metrics = json.data || [];
        
        tbody.innerHTML = '';
        if (metrics.length === 0) {
            tbody.innerHTML = '<tr><td colspan="6" class="text-center">No telemetry logs collected in SQL Server database. Poll loop running...</td></tr>';
            return;
        }
        
        // Take latest 50 and populate
        metrics.slice(0, 50).forEach(m => {
            const tr = document.createElement('tr');
            tr.innerHTML = `
                <td><code>${new Date(m.timestamp).toLocaleTimeString()}</code></td>
                <td><strong>${escapeHtml(m.serviceName)}</strong></td>
                <td>${m.cpuUsage.toFixed(1)}%</td>
                <td>${m.memoryUsage.toFixed(1)} MB</td>
                <td>${m.requestCount}</td>
                <td>${m.responseTime.toFixed(2)} ms</td>
            `;
            tbody.appendChild(tr);
        });
    } catch (err) {
        tbody.innerHTML = `<tr><td colspan="6" class="text-center alert-text">Failed to fetch metrics: ${err.message}</td></tr>`;
    }
}

// AI Model Training triggers
async function triggerModelTraining() {
    const logDiv = elements.mlConsoleLog;
    logDiv.innerHTML = '<div class="term-line system-line">[SYSTEM] Sending ML.NET compile command to /prediction/predictions/train...</div>';
    
    try {
        const res = await fetch(`${state.apiBase}/prediction/predictions/train`, { method: 'POST' });
        const json = await res.json();
        
        if (res.ok) {
            logDiv.innerHTML += `<div class="term-line success-line">[OK] ${json.data}</div>`;
            logDiv.innerHTML += '<div class="term-line success-line">[SYSTEM] Model refitted successfully! Future gateway ingress will use the updated model weight.</div>';
            pollDiagnostics(); // Update indicators
        } else {
            logDiv.innerHTML += `<div class="term-line error-line">[FAIL] Training aborted: ${json.error || res.statusText}</div>`;
        }
    } catch (err) {
        logDiv.innerHTML += `<div class="term-line error-line">[ERR] Connection failed: ${err.message}</div>`;
    }
    logDiv.scrollTop = logDiv.scrollHeight;
}

// HTML Escaper
function escapeHtml(str) {
    return str.replace(/&/g, "&amp;")
              .replace(/</g, "&lt;")
              .replace(/>/g, "&gt;")
              .replace(/"/g, "&quot;")
              .replace(/'/g, "&#039;");
}
