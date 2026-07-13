import React, { useState, useEffect, useRef, useCallback } from 'react';
import { Chart, registerables } from 'chart.js';

// Register Chart.js modules
Chart.register(...registerables);

// Automatically bypass ngrok browser warning for all fetch requests
const originalFetch = window.fetch;
window.fetch = async (input, init) => {
  init = init || {};
  init.headers = init.headers || {};
  if (init.headers instanceof Headers) {
    init.headers.set('ngrok-skip-browser-warning', 'true');
  } else if (Array.isArray(init.headers)) {
    init.headers.push(['ngrok-skip-browser-warning', 'true']);
  } else {
    init.headers['ngrok-skip-browser-warning'] = 'true';
  }
  return originalFetch(input, init);
};

const GATEWAY_URL = 'https://6bf1-102-88-55-18.ngrok-free.app';

export default function App() {
  // Navigation
  const [activeTab, setActiveTab] = useState('store'); // 'store' or 'operator'
  const [activeExplorerTab, setActiveExplorerTab] = useState('users'); // 'users', 'products', 'orders', 'stress'

  // Toast notifications state
  const [toasts, setToasts] = useState([]);
  const showToast = useCallback((message, type = 'info') => {
    const id = Date.now() + Math.random();
    setToasts(prev => [...prev, { id, message, type }]);
    setTimeout(() => {
      setToasts(prev => prev.filter(t => t.id !== id));
    }, 4000);
  }, []);

  // Global Sync States
  const [users, setUsers] = useState([]);
  const [currentUser, setCurrentUser] = useState(null);
  const [products, setProducts] = useState([]);
  const [orders, setOrders] = useState([]);
  const [cart, setCart] = useState([]);
  const [isCartOpen, setIsCartOpen] = useState(false);

  // System Posture Telemetry
  const [systemPosture, setSystemPosture] = useState('Nominal');
  const [isCatalogOffline, setIsCatalogOffline] = useState(false);
  const [checkoutSuccess, setCheckoutSuccess] = useState(null);
  const [isCheckingOut, setIsCheckingOut] = useState(false);
  const [postureChangedTime, setPostureChangedTime] = useState(null);
  const [elapsedSinceChange, setElapsedSinceChange] = useState('0s ago');

  // Live Metrics
  const [currentRps, setCurrentRps] = useState(0.0);
  const [forecastedRps, setForecastedRps] = useState(0.0);
  const [throttledCount, setThrottledCount] = useState(0);
  const [p99Overhead, setP99Overhead] = useState(0.00);

  // Traffic Simulator State
  const [simIntensityRps, setSimIntensityRps] = useState(0);
  const [simDurationSeconds, setSimDurationSeconds] = useState(60);
  const [simTargetRps, setSimTargetRps] = useState(110);
  const [isSimulatorRunning, setIsSimulatorRunning] = useState(false);
  const [simTimeLeft, setSimTimeLeft] = useState(0);
  const [simRouteMix, setSimRouteMix] = useState('balanced');
  const [simClientAuth, setSimClientAuth] = useState('unauth');
  const [predictiveEngineEnabled, setPredictiveEngineEnabled] = useState(true);
  const [simStats, setSimStats] = useState({ sent: 0, success: 0, throttled: 0 });
  const [terminalLogs, setTerminalLogs] = useState([{ time: new Date().toLocaleTimeString(), message: '[SYSTEM] Console initialized. Standing by for traffic simulator...', type: 'system' }]);
  const [mockThrottledCounter, setMockThrottledCounter] = useState(0);
  const [simSummary, setSimSummary] = useState(null);
  const simStartTimeRef = useRef(null);

  // Historical Telemetry & ML State
  const [historicalMetrics, setHistoricalMetrics] = useState([]);
  const [metricsLoading, setMetricsLoading] = useState(false);
  const [mlConsoleLogs, setMlConsoleLogs] = useState([{ time: new Date().toLocaleTimeString(), message: '[SYSTEM] Ready to trigger model compilation...', type: 'system' }]);
  const [isTraining, setIsTraining] = useState(false);

  // Chart Ref
  const chartRef = useRef(null);
  const chartInstanceRef = useRef(null);
  const chartDataRef = useRef({ rps: Array(30).fill(0), forecast: Array(30).fill(0) });

  const simulatorTerminalRef = useRef(null);
  const trainingTerminalRef = useRef(null);
  const countdownIntervalRef = useRef(null);
  const simStatsRef = useRef(simStats);
  const mockThrottledCounterRef = useRef(mockThrottledCounter);

  useEffect(() => {
    simStatsRef.current = simStats;
  }, [simStats]);

  useEffect(() => {
    mockThrottledCounterRef.current = mockThrottledCounter;
  }, [mockThrottledCounter]);

  // Auto-scroll simulator terminal logs
  useEffect(() => {
    if (simulatorTerminalRef.current) {
      simulatorTerminalRef.current.scrollTop = simulatorTerminalRef.current.scrollHeight;
    }
  }, [terminalLogs]);

  // Auto-scroll ML training terminal logs
  useEffect(() => {
    if (trainingTerminalRef.current) {
      trainingTerminalRef.current.scrollTop = trainingTerminalRef.current.scrollHeight;
    }
  }, [mlConsoleLogs]);

  // Refs for simulator interval
  const simIntensityRef = useRef(simIntensityRps);
  const simRouteMixRef = useRef(simRouteMix);
  const simClientAuthRef = useRef(simClientAuth);
  const productsRef = useRef(products);
  const usersRef = useRef(users);
  const currentUserRef = useRef(currentUser);

  // Sync refs to prevent stale closures in async interval
  useEffect(() => { simIntensityRef.current = simIntensityRps; }, [simIntensityRps]);
  useEffect(() => { simRouteMixRef.current = simRouteMix; }, [simRouteMix]);
  useEffect(() => { simClientAuthRef.current = simClientAuth; }, [simClientAuth]);
  useEffect(() => { productsRef.current = products; }, [products]);
  useEffect(() => { usersRef.current = users; }, [users]);
  useEffect(() => { currentUserRef.current = currentUser; }, [currentUser]);

  // Append items to terminal console helper
  const logTerminal = useCallback((message, type = 'success') => {
    setTerminalLogs(prev => {
      const updated = [...prev, { time: new Date().toLocaleTimeString(), message, type }];
      return updated.slice(-100); // Cap logs at 100
    });
  }, []);

  // Fetch lists for CRUD Explorer
  const loadCrudData = useCallback(async () => {
    try {
      // Users
      const resUsers = await fetch(`${GATEWAY_URL}/users`);
      if (resUsers.ok) {
        const data = await resUsers.json();
        const fetchedUsers = data.data || [];
        setUsers(fetchedUsers);
        if (fetchedUsers.length > 0 && !currentUserRef.current) {
          setCurrentUser(fetchedUsers[0]);
        }
      }
      
      // Products
      const resProds = await fetch(`${GATEWAY_URL}/products`);
      if (resProds.ok) {
        const data = await resProds.json();
        setProducts(data.data || []);
      }
      
      // Orders
      const resOrders = await fetch(`${GATEWAY_URL}/orders`);
      if (resOrders.ok) {
        const data = await resOrders.json();
        setOrders(data.data || []);
      }
    } catch (err) {
      console.error('Failed to load explorer CRUD data', err);
    }
  }, []);

  // Fetch products catalog specifically (supports shedding check)
  const fetchProductsCatalog = useCallback(async () => {
    try {
      const res = await fetch(`${GATEWAY_URL}/products`);
      if (res.status === 429) {
        setIsCatalogOffline(true);
        return;
      }
      if (res.ok) {
        const data = await res.json();
        setProducts(data.data || []);
        setIsCatalogOffline(false);
      }
    } catch (err) {
      console.error('Failed to fetch product catalog', err);
      if (systemPosture === 'Critical') {
        setIsCatalogOffline(true);
      }
    }
  }, [systemPosture]);

  // Poll diagnostics endpoint
  const pollDiagnostics = useCallback(async () => {
    try {
      const res = await fetch(`${GATEWAY_URL}/api/diagnostics`);
      if (res.ok) {
        const data = await res.json();
        
        // Posture changes
        if (data.posture !== systemPosture) {
          setSystemPosture(data.posture || 'Nominal');
          setPostureChangedTime(new Date(data.postureChangedAt || Date.now()));
        }
        
        // Telemetry stats
        setCurrentRps(data.currentRps || 0);
        setForecastedRps(data.forecastedRps || 0);
        setThrottledCount(data.throttledRequests || 0);
        setP99Overhead(data.p99OverheadMs || 0);
        if (data.predictiveEngineEnabled !== undefined) {
          setPredictiveEngineEnabled(data.predictiveEngineEnabled);
        }
        
        // Critical status triggers catalog offline view
        if (data.posture === 'Critical') {
          setIsCatalogOffline(true);
        } else if (isCatalogOffline && data.posture !== 'Critical') {
          fetchProductsCatalog();
        }

        // Add to graph buffer
        if (chartInstanceRef.current) {
          const rpsBuffer = chartDataRef.current.rps;
          const forecastBuffer = chartDataRef.current.forecast;
          
          rpsBuffer.push(data.currentRps || 0);
          forecastBuffer.push(data.forecastedRps || 0);
          
          rpsBuffer.shift();
          forecastBuffer.shift();
          
          chartInstanceRef.current.update('none');
        }
      }
    } catch (err) {
      // Mock diagnostics fallback if gateway offline (for standalone testing)
      simulateStandaloneDiagnostics();
    }
  }, [systemPosture, isCatalogOffline, fetchProductsCatalog]);

  // Mock telemetry for offline development fallback
  const simulateStandaloneDiagnostics = () => {
    const intensity = simIntensityRef.current;
    const mockRps = intensity + (Math.random() - 0.5) * 1.5;
    const mockForecast = intensity > 0 ? (intensity * 1.2) : 0;
    
    let posture = 'Nominal';
    if (mockRps > 70 || mockForecast > 85) {
      posture = 'Critical';
    } else if (mockRps > 30 || mockForecast > 45) {
      posture = 'Alert';
    }

    setSystemPosture(posture);
    setCurrentRps(Math.max(0, mockRps));
    setForecastedRps(Math.max(0, mockForecast));
    setP99Overhead(0.12 + Math.random() * 0.04);
    
    if (posture === 'Critical') {
      setIsCatalogOffline(true);
    } else {
      setIsCatalogOffline(false);
    }

    if (chartInstanceRef.current) {
      const rpsBuffer = chartDataRef.current.rps;
      const forecastBuffer = chartDataRef.current.forecast;
      rpsBuffer.push(Math.max(0, mockRps));
      forecastBuffer.push(Math.max(0, mockForecast));
      rpsBuffer.shift();
      forecastBuffer.shift();
      chartInstanceRef.current.update('none');
    }
  };

  // Keep track of posture changed time relative label
  useEffect(() => {
    if (!postureChangedTime) return;
    const interval = setInterval(() => {
      const diff = Math.max(0, Math.floor((Date.now() - postureChangedTime.getTime()) / 1000));
      setElapsedSinceChange(`${diff}s ago`);
    }, 1000);
    return () => clearInterval(interval);
  }, [postureChangedTime]);

  // Polling setup
  useEffect(() => {
    loadCrudData();
    pollDiagnostics();
    const interval = setInterval(pollDiagnostics, 1000);
    return () => clearInterval(interval);
  }, [loadCrudData, pollDiagnostics]);

  // Chart.js initialization
  useEffect(() => {
    if (activeTab !== 'operator' || !chartRef.current) return;
    
    const ctx = chartRef.current.getContext('2d');
    const labels = Array(30).fill(0).map((_, i) => `${29 - i}s ago`);
    
    chartInstanceRef.current = new Chart(ctx, {
      type: 'line',
      data: {
        labels: labels,
        datasets: [
          {
            label: 'Current RPS',
            data: chartDataRef.current.rps,
            borderColor: '#6366f1',
            backgroundColor: 'rgba(99, 102, 241, 0.04)',
            borderWidth: 2.5,
            tension: 0.3,
            fill: true,
            pointRadius: 0
          },
          {
            label: 'Forecasted RPS',
            data: chartDataRef.current.forecast,
            borderColor: '#ec4899',
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
        plugins: { legend: { display: false } },
        scales: {
          x: {
            grid: { display: false },
            ticks: { color: '#475569', font: { size: 9 } }
          },
          y: {
            min: 0,
            suggestedMax: 120,
            grid: { color: 'rgba(0, 0, 0, 0.04)' },
            ticks: { color: '#475569', font: { size: 9 } }
          }
        }
      }
    });

    return () => {
      if (chartInstanceRef.current) {
        chartInstanceRef.current.destroy();
        chartInstanceRef.current = null;
      }
    };
  }, [activeTab]);

  // Simulator Call Loop
  useEffect(() => {
    if (simIntensityRps === 0) return;
    
    const intervalMs = 1000 / simIntensityRps;
    
    const fireRequest = async () => {
      const mix = simRouteMixRef.current;
      const clientAuth = simClientAuthRef.current;
      
      let path = '';
      let method = 'GET';
      let body = null;
      
      // Determine request details
      const isCritical = mix === 'critical' || (mix === 'balanced' && Math.random() > 0.6);
      if (isCritical) {
        method = 'POST';
        path = '/orders';
        const userList = usersRef.current;
        const prodList = productsRef.current;
        
        const userId = userList.length > 0 ? userList[Math.floor(Math.random() * userList.length)].id : 1;
        const productId = prodList.length > 0 ? prodList[Math.floor(Math.random() * prodList.length)].id : 1;
        body = JSON.stringify({ userId, productId, quantity: 1 });
      } else {
        path = '/products';
      }

      setSimStats(prev => ({ ...prev, sent: prev.sent + 1 }));
      
      const clientIndex = Math.floor(Math.random() * 5);
      const headers = {
        'Content-Type': 'application/json',
        'X-Correlation-Id': `react-sim-client-${clientIndex}`
      };
      if (clientAuth === 'auth') {
        headers['Authorization'] = 'Bearer simulated-react-user';
      }

      const start = performance.now();
      try {
        const url = method === 'GET' ? `${GATEWAY_URL}${path}?_cb=${Date.now()}` : `${GATEWAY_URL}${path}`;
        const res = await fetch(url, { method, headers, body });
        const elapsed = Math.round(performance.now() - start);
        const status = res.status;

        if (status === 200 || status === 201 || status === 204) {
          setSimStats(prev => ({ ...prev, success: prev.success + 1 }));
          logTerminal(`[OK] ${method} ${path} -> ${status} (${elapsed}ms)`, 'success');
        } else if (status === 429) {
          setSimStats(prev => ({ ...prev, throttled: prev.throttled + 1 }));
          logTerminal(`[SHED] ${method} ${path} -> 429 Too Many Requests (${elapsed}ms)`, 'error');
          setMockThrottledCounter(prev => prev + 1);
        } else {
          logTerminal(`[FAIL] ${method} ${path} -> ${status} (${elapsed}ms)`, 'error');
        }
      } catch (err) {
        const elapsed = Math.round(performance.now() - start);
        logTerminal(`[ERR] ${method} ${path} failed: ${err.message} (${elapsed}ms)`, 'error');
      }
    };

    const timer = setInterval(fireRequest, intervalMs);
    return () => clearInterval(timer);
  }, [simIntensityRps, logTerminal]);

  const startSimulation = () => {
    setSimStats({ sent: 0, success: 0, throttled: 0 });
    setMockThrottledCounter(0);
    simStartTimeRef.current = Date.now();
    setSimIntensityRps(simTargetRps);
    setIsSimulatorRunning(true);
    setSimTimeLeft(simDurationSeconds);
    logTerminal(`[SYSTEM] Starting custom simulation: ${simTargetRps} RPS for ${simDurationSeconds} seconds.`, 'system');

    if (countdownIntervalRef.current) clearInterval(countdownIntervalRef.current);
    let secondsLeft = simDurationSeconds;
    countdownIntervalRef.current = setInterval(() => {
      secondsLeft -= 1;
      setSimTimeLeft(secondsLeft);
      if (secondsLeft <= 0) {
        stopSimulation(true);
      }
    }, 1000);
  };

  const stopSimulation = (reachedEnd = false) => {
    if (countdownIntervalRef.current) {
      clearInterval(countdownIntervalRef.current);
      countdownIntervalRef.current = null;
    }
    setSimIntensityRps(0);
    setIsSimulatorRunning(false);
    setSimTimeLeft(0);
    
    const elapsed = Math.max(1, Math.round((Date.now() - simStartTimeRef.current) / 1000));
    logTerminal(`[SYSTEM] Simulation stopped. Duration: ${elapsed}s.`, 'system');

    setSimSummary({
      sent: simStatsRef.current.sent,
      success: simStatsRef.current.success,
      throttled: simStatsRef.current.throttled,
      durationSeconds: elapsed,
      routeMix: simRouteMix,
      clientAuth: simClientAuth
    });
  };

  const downloadChart = () => {
    if (!chartRef.current) return;
    // Force white background for downloaded image if transparent, or just export raw PNG
    const url = chartRef.current.toDataURL('image/png');
    const a = document.createElement('a');
    a.href = url;
    a.download = `traffic-acceleration-chart-${new Date().toISOString().slice(0, 19).replace(/:/g, '-')}.png`;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    showToast('Chart image downloaded!', 'success');
  };

  const copySummaryReport = () => {
    if (!simSummary) return;
    const reportText = `Simulation Summary Report
---------------------------------
Duration: ${simSummary.durationSeconds} seconds
Total Requests Sent: ${simSummary.sent}
Successful Requests: ${simSummary.success} (${simSummary.sent > 0 ? ((simSummary.success / simSummary.sent) * 100).toFixed(1) : 0}%)
Throttled (HTTP 429): ${simSummary.throttled} (${simSummary.sent > 0 ? ((simSummary.throttled / simSummary.sent) * 100).toFixed(1) : 0}%)
Route Mix Configuration: ${simSummary.routeMix}
Client Credentials: ${simSummary.clientAuth === 'auth' ? 'Authenticated' : 'Unauthenticated'}
`;
    navigator.clipboard.writeText(reportText)
      .then(() => showToast('Report copied to clipboard!', 'success'))
      .catch(() => showToast('Failed to copy report.', 'error'));
  };

  const saveSummaryReport = () => {
    if (!simSummary) return;
    const reportText = `Simulation Summary Report
---------------------------------
Duration: ${simSummary.durationSeconds} seconds
Total Requests Sent: ${simSummary.sent}
Successful Requests: ${simSummary.success} (${simSummary.sent > 0 ? ((simSummary.success / simSummary.sent) * 100).toFixed(1) : 0}%)
Throttled (HTTP 429): ${simSummary.throttled} (${simSummary.sent > 0 ? ((simSummary.throttled / simSummary.sent) * 100).toFixed(1) : 0}%)
Route Mix Configuration: ${simSummary.routeMix}
Client Credentials: ${simSummary.clientAuth === 'auth' ? 'Authenticated' : 'Unauthenticated'}
`;
    const blob = new Blob([reportText], { type: 'text/plain;charset=utf-8' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `simulation-report-${new Date().toISOString().slice(0, 19).replace(/:/g, '-')}.txt`;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
    showToast('Report saved to file!', 'success');
  };

  const handleTogglePredictiveMode = async (enabled) => {
    try {
      const res = await fetch(`${GATEWAY_URL}/api/config`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ predictiveEngineEnabled: enabled })
      });
      if (res.ok) {
        setPredictiveEngineEnabled(enabled);
        showToast(
          enabled 
            ? 'Predictive Gating enabled. Downstream posture shifts active.' 
            : 'Static Rate Limiting active. Gateway locked to Nominal posture.', 
          'info'
        );
      } else {
        showToast('Failed to toggle mitigation mode.', 'error');
      }
    } catch (err) {
      showToast('Network error while toggling mode.', 'error');
    }
  };

  // Add Item to Store Cart
  const addToCart = (product) => {
    setCart(prev => {
      const existing = prev.find(item => item.product.id === product.id);
      if (existing) {
        return prev.map(item => item.product.id === product.id ? { ...item, quantity: item.quantity + 1 } : item);
      }
      return [...prev, { product, quantity: 1 }];
    });
    setIsCartOpen(true);
    showToast(`Added ${product.name} to cart!`, 'success');
  };

  // Modify cart item quantity
  const updateCartQty = (productId, delta) => {
    setCart(prev => {
      return prev.map(item => {
        if (item.product.id === productId) {
          const newQty = item.quantity + delta;
          return newQty > 0 ? { ...item, quantity: newQty } : null;
        }
        return item;
      }).filter(Boolean);
    });
  };

  // Cart Metrics
  const cartTotal = cart.reduce((tot, it) => tot + (it.product.price * it.quantity), 0);
  const cartCount = cart.reduce((count, it) => count + it.quantity, 0);

  // Cart Checkout
  const handleStoreCheckout = async () => {
    if (cart.length === 0 || !currentUser) return;
    setIsCheckingOut(true);
    const orderResults = [];

    try {
      for (const item of cart) {
        const res = await fetch(`${GATEWAY_URL}/orders`, {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            'X-Correlation-Id': `react-checkout-${Math.floor(Math.random() * 100000)}`
          },
          body: JSON.stringify({
            userId: currentUser.id,
            productId: item.product.id,
            quantity: item.quantity
          })
        });

        if (!res.ok) throw new Error(`Status ${res.status}`);
        const data = await res.json();
        orderResults.push(data.data);
      }

      setCheckoutSuccess({ transactions: orderResults, user: currentUser });
      setCart([]);
      setIsCartOpen(false);
      loadCrudData(); // Reload orders database logs
    } catch (err) {
      showToast(`Checkout failed: ${err.message}. Orders are gated by database capacity.`, 'error');
    } finally {
      setIsCheckingOut(false);
    }
  };

  // CRUD actions from Operator Test Bench
  const handleCreateUser = async (e) => {
    e.preventDefault();
    const name = e.target.userName.value;
    const email = e.target.userEmail.value;
    try {
      const res = await fetch(`${GATEWAY_URL}/users`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ name, email })
      });
      if (res.ok) {
        showToast('User created successfully!', 'success');
        e.target.reset();
        loadCrudData();
      } else {
        showToast('Failed to add user', 'error');
      }
    } catch (err) {
      showToast(err.message, 'error');
    }
  };

  const handleCreateProduct = async (e) => {
    e.preventDefault();
    const name = e.target.prodName.value;
    const sku = e.target.prodSku.value;
    const price = parseFloat(e.target.prodPrice.value);
    
    try {
      const res = await fetch(`${GATEWAY_URL}/products`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ name, sku, price })
      });
      
      if (res.status === 429) {
        showToast('Request Shedded (429)! Cannot add products while system posture is CRITICAL.', 'error');
        return;
      }
      
      if (res.ok) {
        showToast('Product created successfully!', 'success');
        e.target.reset();
        loadCrudData();
      } else {
        showToast('Failed to add product', 'error');
      }
    } catch (err) {
      showToast(err.message, 'error');
    }
  };

  const handleCreateOrder = async (e) => {
    e.preventDefault();
    const userId = parseInt(e.target.orderUserId.value);
    const productId = parseInt(e.target.orderProductId.value);
    const quantity = parseInt(e.target.orderQuantity.value);

    try {
      const res = await fetch(`${GATEWAY_URL}/orders`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ userId, productId, quantity })
      });
      if (res.ok) {
        showToast('Order created successfully!', 'success');
        e.target.reset();
        loadCrudData();
      } else {
        showToast('Failed to process order', 'error');
      }
    } catch (err) {
      showToast(err.message, 'error');
    }
  };

  const handleDeleteUser = async (id) => {
    if (!confirm(`Delete user ${id}?`)) return;
    try {
      await fetch(`${GATEWAY_URL}/users/${id}`, { method: 'DELETE' });
      showToast(`User ${id} deleted successfully!`, 'success');
      loadCrudData();
    } catch (err) {
      showToast(err.message, 'error');
    }
  };

  const handleDeleteProduct = async (id) => {
    if (!confirm(`Delete product ${id}?`)) return;
    try {
      const res = await fetch(`${GATEWAY_URL}/products/${id}`, { method: 'DELETE' });
      if (res.status === 429) {
        showToast('Request Shedded (429)! Cannot delete products while system posture is CRITICAL.', 'error');
        return;
      }
      showToast(`Product ${id} deleted successfully!`, 'success');
      loadCrudData();
    } catch (err) {
      showToast(err.message, 'error');
    }
  };

  // Load Historical telemetry database logs
  const loadHistoricalMetrics = async () => {
    setMetricsLoading(true);
    try {
      const res = await fetch(`${GATEWAY_URL}/monitoring/metrics`);
      const json = await res.json();
      setHistoricalMetrics(json.data || []);
    } catch (err) {
      console.error('Failed to load metrics', err);
    } finally {
      setMetricsLoading(false);
    }
  };

  // Model training
  const triggerModelTraining = async () => {
    setIsTraining(true);
    setMlConsoleLogs(prev => [...prev, { time: new Date().toLocaleTimeString(), message: '[SYSTEM] Compiling latest metrics and refitting regression tree...', type: 'system' }]);
    
    try {
      const res = await fetch(`${GATEWAY_URL}/prediction/predictions/train`, { method: 'POST' });
      const json = await res.json();
      if (res.ok) {
        setMlConsoleLogs(prev => [
          ...prev,
          { time: new Date().toLocaleTimeString(), message: `[OK] ${json.data}`, type: 'success' },
          { time: new Date().toLocaleTimeString(), message: '[SYSTEM] Model weight refitted and saved successfully to shared volume (model.zip).', type: 'success' }
        ]);
      } else {
        setMlConsoleLogs(prev => [...prev, { time: new Date().toLocaleTimeString(), message: `[FAIL] Training failed: ${json.error || res.statusText}`, type: 'error' }]);
      }
    } catch (err) {
      setMlConsoleLogs(prev => [...prev, { time: new Date().toLocaleTimeString(), message: `[ERR] Connection error: ${err.message}`, type: 'error' }]);
    } finally {
      setIsTraining(false);
    }
  };
  return (
    <div className="store-container">
      {/* Background Ambient Orbs */}
      <div className="glow-container">
        <div className="glow-orb orb-1"></div>
        <div className="glow-orb orb-2"></div>
      </div>

      {/* Top Navigation Navbar */}
      <nav className="navbar">
        <div className="nav-logo" onClick={() => { setActiveTab('store'); fetchProductsCatalog(); }}>
          🛍️ AuraShop
        </div>

        <div className="nav-actions">
          {/* Tab Selector */}
          <div style={{ display: 'flex', gap: '8px', borderRight: '1px solid rgba(255,255,255,0.06)', paddingRight: '16px', marginRight: '16px' }}>
            <button 
              className={`cart-trigger ${activeTab === 'store' ? 'active-tab' : ''}`}
              style={{ backgroundColor: activeTab === 'store' ? 'var(--color-accent)' : 'transparent' }}
              onClick={() => setActiveTab('store')}
            >
              🛍️ Storefront
            </button>
            <button 
              className={`cart-trigger ${activeTab === 'operator' ? 'active-tab' : ''}`}
              style={{ backgroundColor: activeTab === 'operator' ? 'var(--color-accent)' : 'transparent' }}
              onClick={() => { setActiveTab('operator'); loadHistoricalMetrics(); }}
            >
              📊 Operator Panel
            </button>
          </div>

          {/* Active Profile selector */}
          <div className="user-selector">
            <span>Customer:</span>
            <select 
              className="select-input"
              value={currentUser ? currentUser.id : ''} 
              onChange={(e) => {
                const selected = users.find(u => u.id === parseInt(e.target.value));
                if (selected) setCurrentUser(selected);
              }}
            >
              {users.map(u => (
                <option key={u.id} value={u.id}>{u.name} (ID: {u.id})</option>
              ))}
            </select>
          </div>

          {/* Telemetry Health dot */}
          <div className="health-badge">
            <span className={`health-dot ${systemPosture.toLowerCase()}`}></span>
            <span>{systemPosture} Posture</span>
          </div>

          {/* Cart Icon */}
          <button className="cart-trigger" onClick={() => setIsCartOpen(true)}>
            🛒 Cart <span className="cart-count">{cartCount}</span>
          </button>
        </div>
      </nav>

      {/* Posture Banners */}
      {systemPosture === 'Alert' && (
        <div className="status-banner alert-banner">
          <span>⚠️ Warning posture active. Cache pre-warming triggered due to forecasted traffic rate increases.</span>
        </div>
      )}
      {systemPosture === 'Critical' && (
        <div className="status-banner critical-banner">
          <span>🚨 Critical load protection active. Catalog browsing suspended to isolate database CPU capacity for checkout orders.</span>
        </div>
      )}

      {/* Main Container tabs */}
      {activeTab === 'store' ? (
        // STOREFRONT VIEW
        <>
          <div className="hero">
            <div className="hero-card">
              <span className="hero-tag">Aura E-Commerce Catalog</span>
              <h2>Premium Gear. Automated Gating.</h2>
              <p className="hero-desc">
                Experience automated scaling resilience in a C# microservice. Use the <strong>Operator Panel</strong> tab to simulate high traffic, and watch how AuraShop automatically pauses its product lists to protect checkout transactions.
              </p>
            </div>
          </div>

          <section className="catalog-section">
            <div className="section-header">
              <h3>Explore Products Catalog</h3>
              <button 
                className="btn-add" 
                style={{ backgroundColor: 'rgba(255,255,255,0.04)', color: 'var(--text-primary)' }} 
                onClick={fetchProductsCatalog}
              >
                🔄 Refresh
              </button>
            </div>

            <div className="product-grid">
              {isCatalogOffline ? (
                <div className="catalog-offline-card">
                  <span className="offline-icon">🛡️</span>
                  <h4>Catalog Offline (HTTP 429 Shedding)</h4>
                  <p>
                    Product browsing has been suspended temporarily because the system is experiencing a <strong>CRITICAL</strong> load.
                    This prevents non-essential catalog reads from exhausting SQL Server connection pools.
                  </p>
                  <p style={{ color: 'var(--color-nominal)', fontWeight: 600 }}>
                    ✅ Checkout remains open! You can successfully submit and complete any items currently in your cart drawer.
                  </p>
                </div>
              ) : products.length === 0 ? (
                <div style={{ gridColumn: '1/-1', textAlign: 'center', color: 'var(--text-secondary)', padding: '40px' }}>
                  Loading catalog items... Seed products in Operator tab if missing.
                </div>
              ) : (
                products.map(p => (
                  <div className="product-card" key={p.id}>
                    <div className="product-img-container">
                      {p.name?.toLowerCase().includes('laptop') ? (
                        <img src="/laptop.png" alt="Laptop" className="product-img" />
                      ) : p.name?.toLowerCase().includes('mouse') ? (
                        <img src="/mouse.png" alt="Mouse" className="product-img" />
                      ) : p.name?.toLowerCase().includes('keyboard') ? (
                        <img src="/keyboard.png" alt="Keyboard" className="product-img" />
                      ) : (
                        <div className="product-img-placeholder" style={{ fontSize: '36px' }}>💾</div>
                      )}
                    </div>
                    <div className="product-info">
                      <span className="product-sku">{p.sku || `SKU-${p.name?.toUpperCase().substring(0, 5).replace(/\s+/g, '')}-0${p.id}`}</span>
                      <h4 className="product-name">{p.name}</h4>
                    </div>
                    <div className="product-footer">
                      <span className="product-price">${p.price.toFixed(2)}</span>
                      <button className="btn-add" onClick={() => addToCart(p)}>
                        + Add to Cart
                      </button>
                    </div>
                  </div>
                ))
              )}
            </div>
          </section>
        </>
      ) : (
        // OPERATOR VIEW
        <main className="main-content" style={{ margin: '0 auto', width: '100%', maxWidth: '1200px', padding: '40px' }}>
          {/* Posture Card and Gauges */}
          <div className="dashboard-grid">
            {/* Posture Box */}
            <div className="card posture-card" style={{ borderLeftColor: systemPosture === 'Nominal' ? 'var(--color-nominal)' : systemPosture === 'Alert' ? 'var(--color-alert)' : 'var(--color-critical)', boxShadow: `0 8px 32px 0 ${systemPosture === 'Nominal' ? 'var(--glow-nominal)' : systemPosture === 'Alert' ? 'var(--glow-alert)' : 'var(--glow-critical)'}` }}>
              <div className="card-header">
                <h3>PROTECTIVE STATE</h3>
              </div>
              <div className="posture-display">
                <div className="posture-badge" style={{ color: systemPosture === 'Nominal' ? 'var(--color-nominal)' : systemPosture === 'Alert' ? 'var(--color-alert)' : 'var(--color-critical)' }}>{systemPosture}</div>
                <p className="posture-desc">
                  {systemPosture === 'Nominal' && 'System is running within normal thresholds. Non-critical routes are fully available.'}
                  {systemPosture === 'Alert' && 'Proactive warning posture activated. AI model forecasts high incoming load. Core services cached.'}
                  {systemPosture === 'Critical' && 'System protective action is active! Non-critical routes (e.g. Products / Ads) are shed with HTTP 429.'}
                </p>
              </div>
              <div className="posture-meta">
                <span>Last changed: <strong>{elapsedSinceChange}</strong></span>
              </div>
            </div>

            {/* Metrics */}
            <div className="card telemetry-card">
              <div className="card-header">
                <h3>REAL-TIME METRICS</h3>
              </div>
              <div className="stats-grid">
                <div className="stat-item">
                  <span className="stat-label">Current Load</span>
                  <span className="stat-value">{currentRps.toFixed(1)} <span className="unit">RPS</span></span>
                </div>
                <div className="stat-item">
                  <span className="stat-label">60s Forecast</span>
                  <span className="stat-value">{forecastedRps.toFixed(1)} <span className="unit">RPS</span></span>
                </div>
                <div className="stat-item">
                  <span className="stat-label">Throttled (429)</span>
                  <span className="stat-value alert-text">{throttledCount + mockThrottledCounter}</span>
                </div>
                <div className="stat-item">
                  <span className="stat-label">P99 Overhead</span>
                  <span className="stat-value">{p99Overhead.toFixed(2)} <span className="unit">ms</span></span>
                </div>
              </div>
            </div>
          </div>

          {/* Graph Card */}
          <div className="card chart-card">
            <div className="card-header" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <h3>Live Traffic Acceleration (Chart)</h3>
              <button className="btn-add" style={{ padding: '4px 12px', fontSize: '12px' }} onClick={downloadChart}>
                📥 Download Chart
              </button>
            </div>
            <div className="chart-container" style={{ height: '220px' }}>
              <canvas ref={chartRef}></canvas>
            </div>
          </div>

          {/* Simulator and Terminal logs */}
          <div className="dashboard-grid simulator-grid">
            <div className="card simulator-card">
              <div className="card-header">
                <h3>Simulate Traffic</h3>
              </div>
              <div className="card-body">
                <p className="instructions">Configure target load intensity and run time to simulate traffic conditions.</p>
                
                <div className="form-row">
                  <div className="form-group">
                    <label>Target RPS ({simTargetRps} RPS)</label>
                    <input 
                      type="range" 
                      min="5" 
                      max="150" 
                      step="5"
                      value={simTargetRps} 
                      disabled={isSimulatorRunning}
                      onChange={(e) => setSimTargetRps(parseInt(e.target.value))}
                      className="form-range"
                      style={{ width: '100%', accentColor: 'var(--color-accent)' }}
                    />
                  </div>
                  <div className="form-group">
                    <label>Duration (Seconds)</label>
                    <select 
                      className="form-select" 
                      value={simDurationSeconds} 
                      disabled={isSimulatorRunning}
                      onChange={(e) => setSimDurationSeconds(parseInt(e.target.value))}
                    >
                      <option value={10}>10 Seconds (Quick)</option>
                      <option value={30}>30 Seconds</option>
                      <option value={60}>60 Seconds (1 Min)</option>
                      <option value={120}>120 Seconds (2 Min)</option>
                      <option value={300}>300 Seconds (5 Min)</option>
                    </select>
                  </div>
                </div>

                <div className="form-row">
                  <div className="form-group">
                    <label>Endpoints Target</label>
                    <select className="form-select" disabled={isSimulatorRunning} value={simRouteMix} onChange={(e) => setSimRouteMix(e.target.value)}>
                      <option value="balanced">Balanced Mix</option>
                      <option value="critical">Critical Only (/orders)</option>
                      <option value="noncritical">Non-Critical Only (/products)</option>
                    </select>
                  </div>
                  <div className="form-group">
                    <label>Client Auth</label>
                    <select className="form-select" disabled={isSimulatorRunning} value={simClientAuth} onChange={(e) => setSimClientAuth(e.target.value)}>
                      <option value="unauth">Unauthenticated</option>
                      <option value="auth">Authenticated</option>
                    </select>
                  </div>
                </div>

                <div className="form-group" style={{ marginBottom: '18px' }}>
                  <label>Mitigation Strategy</label>
                  <select 
                    className="form-select" 
                    disabled={isSimulatorRunning} 
                    value={predictiveEngineEnabled ? 'predictive' : 'static'} 
                    onChange={(e) => handleTogglePredictiveMode(e.target.value === 'predictive')}
                  >
                    <option value="predictive">🎯 Predictive Gating (Dynamic Postures)</option>
                    <option value="static">🔒 Static Rate Limiting (Nominal Locked)</option>
                  </select>
                </div>

                {isSimulatorRunning ? (
                  <div style={{ marginTop: '16px' }}>
                    <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '12px', marginBottom: '6px' }}>
                      <span style={{ color: 'var(--color-accent)', fontWeight: 600 }}>⚡ Custom Load Active...</span>
                      <span><strong>{simTimeLeft}s</strong> remaining</span>
                    </div>
                    <div className="progress-bar-bg" style={{ width: '100%', height: '6px', backgroundColor: 'rgba(255,255,255,0.05)', borderRadius: '3px', overflow: 'hidden' }}>
                      <div className="progress-bar-fill" style={{ width: `${(simTimeLeft / simDurationSeconds) * 100}%`, height: '100%', backgroundColor: 'var(--color-accent)', boxShadow: '0 0 8px var(--color-accent)', transition: 'width 1s linear' }}></div>
                    </div>
                    <button 
                      className="btn" 
                      style={{ width: '100%', marginTop: '15px', backgroundColor: 'var(--color-critical)', color: 'white' }}
                      onClick={() => stopSimulation(false)}
                    >
                      ⏹️ Stop Simulation
                    </button>
                  </div>
                ) : (
                  <button 
                    className="btn btn-primary" 
                    style={{ width: '100%', marginTop: '15px' }}
                    onClick={startSimulation}
                  >
                    🚀 Start Simulation Run
                  </button>
                )}

                <div className="simulator-status-bar" style={{ marginTop: '20px' }}>
                  <div className="sim-metric">Sent: <span>{simStats.sent}</span></div>
                  <div className="sim-metric success">Success: <span>{simStats.success}</span></div>
                  <div className="sim-metric throttled">Throttled: <span>{simStats.throttled}</span></div>
                </div>
              </div>
            </div>

            {/* Terminal logs */}
            <div className="card terminal-card" style={{ display: 'flex', flexDirection: 'column' }}>
              <div className="card-header" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <h3>Simulator Console Output</h3>
                <button className="btn-add" style={{ padding: '2px 8px', fontSize: '10px' }} onClick={() => setTerminalLogs([])}>Clear</button>
              </div>
              <div ref={simulatorTerminalRef} className="terminal-body" style={{ flexGrow: 1, minHeight: '180px' }}>
                {terminalLogs.map((log, idx) => (
                  <div key={idx} className={`term-line ${log.type === 'error' ? 'error-line' : log.type === 'system' ? 'system-line' : 'success-line'}`}>
                    [{log.time}] {log.message}
                  </div>
                ))}
              </div>
            </div>
          </div>

          {/* Sub tabs nav */}
          <div className="explorer-container" style={{ marginTop: '24px' }}>
            <div className="explorer-nav">
              {['users', 'products', 'orders'].map(tab => (
                <button 
                  key={tab}
                  className={`exp-tab ${activeExplorerTab === tab ? 'active' : ''}`}
                  onClick={() => setActiveExplorerTab(tab)}
                >
                  {tab.toUpperCase()} CRUD
                </button>
              ))}
              <button 
                className={`exp-tab ${activeExplorerTab === 'metrics' ? 'active' : ''}`}
                onClick={() => { setActiveExplorerTab('metrics'); loadHistoricalMetrics(); }}
              >
                DB TELEMETRY HISTORICAL
              </button>
              <button 
                className={`exp-tab ${activeExplorerTab === 'ml' ? 'active' : ''}`}
                onClick={() => setActiveExplorerTab('ml')}
              >
                AI MODEL TRAINER
              </button>
            </div>

            {/* Sub tab panels */}
            {activeExplorerTab === 'users' && (
              <div className="explorer-panel active">
                <div className="panel-layout">
                  <div className="form-panel">
                    <h3>Create New User</h3>
                    <form onSubmit={handleCreateUser}>
                      <div className="form-group">
                        <label>Name</label>
                        <input name="userName" required placeholder="Name..." className="form-input" />
                      </div>
                      <div className="form-group">
                        <label>Email</label>
                        <input name="userEmail" type="email" required placeholder="Email..." className="form-input" />
                      </div>
                      <button type="submit" className="btn btn-primary">Add User</button>
                    </form>
                  </div>
                  <div className="table-panel">
                    <h3>Users List</h3>
                    <div className="table-container">
                      <table>
                        <thead>
                          <tr><th>ID</th><th>Name</th><th>Email</th><th>Actions</th></tr>
                        </thead>
                        <tbody>
                          {users.map(u => (
                            <tr key={u.id}>
                              <td>{u.id}</td>
                              <td><strong>{u.name}</strong></td>
                              <td>{u.email}</td>
                              <td><button className="btn-add" style={{ backgroundColor: 'var(--color-critical)' }} onClick={() => handleDeleteUser(u.id)}>Delete</button></td>
                            </tr>
                          ))}
                        </tbody>
                      </table>
                    </div>
                  </div>
                </div>
              </div>
            )}

            {activeExplorerTab === 'products' && (
              <div className="explorer-panel active">
                <div className="panel-layout">
                  <div className="form-panel">
                    <h3>Create Product</h3>
                    <form onSubmit={handleCreateProduct}>
                      <div className="form-group">
                        <label>Name</label>
                        <input name="prodName" required placeholder="Name..." className="form-input" />
                      </div>
                      <div className="form-group">
                        <label>SKU</label>
                        <input name="prodSku" required placeholder="e.g. SKU-QUANT-10..." className="form-input" />
                      </div>
                      <div className="form-group">
                        <label>Price</label>
                        <input name="prodPrice" type="number" step="0.01" required placeholder="Price..." className="form-input" />
                      </div>
                      <button type="submit" className="btn btn-primary">Add Product</button>
                    </form>
                  </div>
                  <div className="table-panel">
                    <h3>Catalog</h3>
                    <div className="table-container">
                      <table>
                        <thead>
                          <tr><th>ID</th><th>Name</th><th>SKU</th><th>Price</th><th>Actions</th></tr>
                        </thead>
                        <tbody>
                          {products.map(p => (
                            <tr key={p.id}>
                              <td>{p.id}</td>
                              <td><strong>{p.name}</strong></td>
                              <td><code>{p.sku || `SKU-${p.name?.toUpperCase().substring(0, 5).replace(/\s+/g, '')}-0${p.id}`}</code></td>
                              <td>${p.price.toFixed(2)}</td>
                              <td><button className="btn-add" style={{ backgroundColor: 'var(--color-critical)' }} onClick={() => handleDeleteProduct(p.id)}>Delete</button></td>
                            </tr>
                          ))}
                        </tbody>
                      </table>
                    </div>
                  </div>
                </div>
              </div>
            )}

            {activeExplorerTab === 'orders' && (
              <div className="explorer-panel active">
                <div className="panel-layout">
                  <div className="form-panel">
                    <h3>Process Order</h3>
                    <form onSubmit={handleCreateOrder}>
                      <div className="form-group">
                        <label>User</label>
                        <select name="orderUserId" required className="form-select">
                          {users.map(u => <option key={u.id} value={u.id}>{u.name}</option>)}
                        </select>
                      </div>
                      <div className="form-group">
                        <label>Product</label>
                        <select name="orderProductId" required className="form-select">
                          {products.map(p => <option key={p.id} value={p.id}>{p.name}</option>)}
                        </select>
                      </div>
                      <div className="form-group">
                        <label>Quantity</label>
                        <input name="orderQuantity" type="number" min="1" defaultValue="1" className="form-input" />
                      </div>
                      <button type="submit" className="btn btn-primary">Add Order</button>
                    </form>
                  </div>
                  <div className="table-panel">
                    <h3>Orders Database Logs</h3>
                    <div className="table-container">
                      <table>
                        <thead>
                          <tr><th>ID</th><th>User ID</th><th>Product ID</th><th>Qty</th><th>Total</th><th>Date</th></tr>
                        </thead>
                        <tbody>
                          {orders.map(o => (
                            <tr key={o.id}>
                              <td>{o.id}</td>
                              <td>User #{o.userId}</td>
                              <td>Product #{o.productId}</td>
                              <td>{o.quantity}</td>
                              <td><strong>${(o.totalPrice || 0).toFixed(2)}</strong></td>
                              <td>{new Date(o.createdAt || Date.now()).toLocaleTimeString()}</td>
                            </tr>
                          ))}
                        </tbody>
                      </table>
                    </div>
                  </div>
                </div>
              </div>
            )}

            {activeExplorerTab === 'metrics' && (
              <div className="explorer-panel active">
                <div className="card">
                  <div className="card-header" style={{ display: 'flex', justifyContent: 'space-between' }}>
                    <h3>Collected Logs in Database</h3>
                    <button className="btn-add" onClick={loadHistoricalMetrics}>Refresh Logs</button>
                  </div>
                  <div className="table-container">
                    <table>
                      <thead>
                        <tr><th>Timestamp</th><th>Service</th><th>CPU (%)</th><th>Memory (MB)</th><th>Request Count</th><th>Response (ms)</th></tr>
                      </thead>
                      <tbody>
                        {metricsLoading ? (
                          <tr><td colSpan="6" className="text-center">Fetching DB data...</td></tr>
                        ) : historicalMetrics.length === 0 ? (
                          <tr><td colSpan="6" className="text-center">No telemetry logs saved. Poll loops running...</td></tr>
                        ) : (
                          historicalMetrics.slice(0, 50).map((m, idx) => (
                            <tr key={idx}>
                              <td><code>{new Date(m.timestamp).toLocaleTimeString()}</code></td>
                              <td><strong>{m.serviceName}</strong></td>
                              <td>{m.cpuUsage.toFixed(1)}%</td>
                              <td>{m.memoryUsage.toFixed(1)} MB</td>
                              <td>{m.requestCount}</td>
                              <td>{m.responseTime.toFixed(2)} ms</td>
                            </tr>
                          ))
                        )}
                      </tbody>
                    </table>
                  </div>
                </div>
              </div>
            )}

            {activeExplorerTab === 'ml' && (
              <div className="explorer-panel active">
                <div className="dashboard-grid ml-grid">
                  <div className="card ml-card">
                    <div className="card-header"><h3>ML.NET sdca model training</h3></div>
                    <div className="card-body">
                      <p>Train and refit the ML.NET SDCA regression model using metric records logged in the SQL server database.</p>
                      <button className="btn btn-primary" disabled={isTraining} onClick={triggerModelTraining}>
                        {isTraining ? <><span className="spinner"></span>Training Model...</> : '⚡ Run Model Training'}
                      </button>
                    </div>
                  </div>
                  <div className="card ml-console-card">
                    <div className="card-header"><h3>stdout training log</h3></div>
                    <div ref={trainingTerminalRef} className="terminal-body">
                      {mlConsoleLogs.map((log, idx) => (
                        <div key={idx} className={`term-line ${log.type === 'error' ? 'error-line' : 'success-line'}`}>
                          [{log.time}] {log.message}
                        </div>
                      ))}
                    </div>
                  </div>
                </div>
              </div>
            )}
          </div>
        </main>
      )}

      {/* Shopping Cart Drawer */}
      <div className={`cart-drawer-overlay ${isCartOpen ? 'active' : ''}`} onClick={() => setIsCartOpen(false)}></div>
      <div className={`cart-drawer ${isCartOpen ? 'active' : ''}`}>
        <div className="cart-header">
          <h3>Your Shopping Cart</h3>
          <button className="btn-close" onClick={() => setIsCartOpen(false)}>✕</button>
        </div>
        <div className="cart-items">
          {cart.length === 0 ? (
            <div style={{ textAlign: 'center', marginTop: '40px', color: 'var(--text-secondary)' }}>
              Cart is empty.
            </div>
          ) : (
            cart.map(item => (
              <div className="cart-item" key={item.product.id}>
                <div className="item-info">
                  <h5>{item.product.name}</h5>
                  <span className="item-price">${item.product.price.toFixed(2)}</span>
                </div>
                <div className="item-qty-actions">
                  <button className="qty-btn" onClick={() => updateCartQty(item.product.id, -1)}>-</button>
                  <span className="qty-count">{item.quantity}</span>
                  <button className="qty-btn" onClick={() => updateCartQty(item.product.id, 1)}>+</button>
                </div>
              </div>
            ))
          )}
        </div>
        <div className="cart-footer">
          <div className="cart-total-row">
            <span>Total:</span>
            <span className="price">${cartTotal.toFixed(2)}</span>
          </div>
          <button 
            className="btn-checkout" 
            disabled={cart.length === 0 || !currentUser || isCheckingOut}
            onClick={handleStoreCheckout}
          >
            {isCheckingOut ? 'Securing Transaction...' : 'Complete Checkout 🔒'}
          </button>
        </div>
      </div>

      {/* Success Modal */}
      {checkoutSuccess && (
        <div className="modal-overlay">
          <div className="modal-content">
            <span className="success-icon">🎉</span>
            <h4>Order Completed!</h4>
            <p>Your e-commerce transaction was processed. The gateway bypassed rating blocks for this route.</p>
            <div style={{ width: '100%', textAlign: 'left' }}>
              {checkoutSuccess.transactions.map((tx, idx) => (
                <div className="tx-id" key={idx}>
                  <div>Order #{tx.id}</div>
                  <div>Total: ${tx.totalPrice.toFixed(2)}</div>
                </div>
              ))}
            </div>
            <button className="btn-modal-close" onClick={() => setCheckoutSuccess(null)}>
              Back to Catalog
            </button>
          </div>
        </div>
      )}

      {/* Simulator Summary Modal */}
      {simSummary && (
        <div className="modal-overlay">
          <div className="modal-content" style={{ maxWidth: '450px' }}>
            <span className="success-icon" style={{ fontSize: '32px' }}>📊</span>
            <h4 style={{ fontFamily: 'var(--font-heading)', fontWeight: 800, fontSize: '20px', marginBottom: '8px' }}>Simulation Summary Report</h4>
            <p style={{ color: 'var(--text-secondary)', fontSize: '13px', marginBottom: '16px' }}>
              Synthetic traffic simulation has terminated. Below is the diagnostic footprint captured at the client side.
            </p>
            
            <div className="summary-metrics-list" style={{ width: '100%', display: 'flex', flexDirection: 'column', gap: '8px', marginBottom: '20px' }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', padding: '8px 12px', backgroundColor: 'rgba(255,255,255,0.02)', borderRadius: '8px', border: '1px solid var(--border-color)', fontSize: '13px' }}>
                <span style={{ color: 'var(--text-secondary)' }}>Duration:</span>
                <span style={{ fontWeight: 600 }}>{simSummary.durationSeconds} seconds</span>
              </div>
              <div style={{ display: 'flex', justifyContent: 'space-between', padding: '8px 12px', backgroundColor: 'rgba(255,255,255,0.02)', borderRadius: '8px', border: '1px solid var(--border-color)', fontSize: '13px' }}>
                <span style={{ color: 'var(--text-secondary)' }}>Total Requests Sent:</span>
                <span style={{ fontWeight: 600 }}>{simSummary.sent}</span>
              </div>
              <div style={{ display: 'flex', justifyContent: 'space-between', padding: '8px 12px', backgroundColor: 'rgba(16, 185, 129, 0.04)', borderRadius: '8px', border: '1px solid rgba(16, 185, 129, 0.15)', fontSize: '13px' }}>
                <span style={{ color: 'var(--color-nominal)' }}>Successful Requests:</span>
                <span style={{ fontWeight: 600, color: 'var(--color-nominal)' }}>{simSummary.success} ({simSummary.sent > 0 ? ((simSummary.success / simSummary.sent) * 100).toFixed(1) : 0}%)</span>
              </div>
              <div style={{ display: 'flex', justifyContent: 'space-between', padding: '8px 12px', backgroundColor: 'rgba(239, 68, 68, 0.04)', borderRadius: '8px', border: '1px solid rgba(239, 68, 68, 0.15)', fontSize: '13px' }}>
                <span style={{ color: 'var(--color-critical)' }}>Throttled (HTTP 429):</span>
                <span style={{ fontWeight: 600, color: 'var(--color-critical)' }}>{simSummary.throttled} ({simSummary.sent > 0 ? ((simSummary.throttled / simSummary.sent) * 100).toFixed(1) : 0}%)</span>
              </div>
              <div style={{ display: 'flex', justifyContent: 'space-between', padding: '8px 12px', backgroundColor: 'rgba(255,255,255,0.02)', borderRadius: '8px', border: '1px solid var(--border-color)', fontSize: '13px' }}>
                <span style={{ color: 'var(--text-secondary)' }}>Route Mix Configuration:</span>
                <span style={{ fontWeight: 600, textTransform: 'capitalize' }}>{simSummary.routeMix}</span>
              </div>
              <div style={{ display: 'flex', justifyContent: 'space-between', padding: '8px 12px', backgroundColor: 'rgba(255,255,255,0.02)', borderRadius: '8px', border: '1px solid var(--border-color)', fontSize: '13px' }}>
                <span style={{ color: 'var(--text-secondary)' }}>Client Credentials:</span>
                <span style={{ fontWeight: 600 }}>{simSummary.clientAuth === 'auth' ? 'Authenticated 🔑' : 'Unauthenticated 🌐'}</span>
              </div>
            </div>

            <div style={{ display: 'flex', gap: '10px', marginTop: '16px' }}>
              <button className="btn" onClick={copySummaryReport} style={{ flexGrow: 1, padding: '10px', display: 'flex', alignItems: 'center', justifyContent: 'center', gap: '6px', fontSize: '13px' }}>
                📋 Copy Report
              </button>
              <button className="btn" onClick={saveSummaryReport} style={{ flexGrow: 1, padding: '10px', display: 'flex', alignItems: 'center', justifyContent: 'center', gap: '6px', fontSize: '13px' }}>
                💾 Save Report
              </button>
            </div>

            <button className="btn-modal-close" onClick={() => setSimSummary(null)} style={{ width: '100%', padding: '10px', marginTop: '10px' }}>
              Acknowledge & Close
            </button>
          </div>
        </div>
      )}
      {/* Toast Notifications */}
      <div className="toast-container">
        {toasts.map(toast => (
          <div key={toast.id} className={`toast-message toast-${toast.type}`}>
            <span className="toast-icon">
              {toast.type === 'success' ? '✅' : toast.type === 'error' ? '❌' : 'ℹ️'}
            </span>
            <span className="toast-text">{toast.message}</span>
          </div>
        ))}
      </div>
    </div>
  );
}
