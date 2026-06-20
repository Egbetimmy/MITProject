const { useState, useEffect, useCallback } = React;

function App() {
    const [users, setUsers] = useState([]);
    const [currentUser, setCurrentUser] = useState(null);
    const [products, setProducts] = useState([]);
    const [cart, setCart] = useState([]);
    const [isCartOpen, setIsCartOpen] = useState(false);
    
    // System posture telemetry
    const [systemPosture, setSystemPosture] = useState('Nominal');
    const [isCatalogOffline, setIsCatalogOffline] = useState(false);
    const [checkoutSuccess, setCheckoutSuccess] = useState(null);
    const [isCheckingOut, setIsCheckingOut] = useState(false);

    // Fetch active users from UserService
    const fetchUsers = async () => {
        try {
            const res = await fetch('/users');
            const data = await res.json();
            const fetchedUsers = data.data || [];
            setUsers(fetchedUsers);
            if (fetchedUsers.length > 0 && !currentUser) {
                setCurrentUser(fetchedUsers[0]);
            }
        } catch (err) {
            console.error('Failed to load users', err);
        }
    };

    // Fetch catalog products from ProductService
    const fetchProducts = async () => {
        try {
            const res = await fetch('/products');
            
            // Check for rate-limiting route-shedding (HTTP 429)
            if (res.status === 429) {
                setIsCatalogOffline(true);
                return;
            }

            const data = await res.json();
            setProducts(data.data || []);
            setIsCatalogOffline(false);
        } catch (err) {
            console.error('Failed to load products', err);
            // If failed due to CORS or network error under heavy load
            if (systemPosture === 'Critical') {
                setIsCatalogOffline(true);
            }
        }
    };

    // Poll live Diagnostics for posture
    const pollDiagnostics = async () => {
        try {
            const res = await fetch('/api/diagnostics');
            if (res.ok) {
                const data = await res.json();
                setSystemPosture(data.posture || 'Nominal');
                
                // If posture is Critical, we preemptively know the catalog is offline
                if (data.posture === 'Critical') {
                    setIsCatalogOffline(true);
                } else if (isCatalogOffline && data.posture !== 'Critical') {
                    // Refetch catalog once posture drops back down
                    fetchProducts();
                }
            }
        } catch (err) {
            console.error('Failed to poll diagnostics', err);
        }
    };

    // Load initial data
    useEffect(() => {
        fetchUsers();
        fetchProducts();
        
        // Setup polling
        const interval = setInterval(() => {
            pollDiagnostics();
        }, 1000);
        
        return () => clearInterval(interval);
    }, []);

    // Refresh products catalog when current user changes or manual refresh
    useEffect(() => {
        if (systemPosture !== 'Critical') {
            fetchProducts();
        }
    }, [currentUser]);

    // Add item to cart
    const addToCart = (product) => {
        setCart(prevCart => {
            const existing = prevCart.find(item => item.product.id === product.id);
            if (existing) {
                return prevCart.map(item => 
                    item.product.id === product.id 
                        ? { ...item, quantity: item.quantity + 1 }
                        : item
                );
            }
            return [...prevCart, { product, quantity: 1 }];
        });
        setIsCartOpen(true);
    };

    // Update cart item quantity
    const updateQuantity = (productId, delta) => {
        setCart(prevCart => {
            return prevCart.map(item => {
                if (item.product.id === productId) {
                    const newQty = item.quantity + delta;
                    return newQty > 0 ? { ...item, quantity: newQty } : null;
                }
                return item;
            }).filter(Boolean);
        });
    };

    // Submit Checkout Orders (Critical Route)
    const handleCheckout = async () => {
        if (cart.length === 0 || !currentUser) return;
        
        setIsCheckingOut(true);
        const orderResults = [];
        let hasError = false;

        try {
            // Process checkout for each cart item sequentially
            for (const item of cart) {
                const res = await fetch('/orders', {
                    method: 'POST',
                    headers: { 
                        'Content-Type': 'application/json',
                        'X-Correlation-Id': `store-checkout-${Math.floor(Math.random() * 100000)}`
                    },
                    body: JSON.stringify({
                        userId: currentUser.id,
                        productId: item.product.id,
                        quantity: item.quantity
                    })
                });

                if (!res.ok) {
                    throw new Error(`Checkout failed with status ${res.status}`);
                }

                const json = await res.json();
                orderResults.push(json.data);
            }
            
            // Set success modal details
            setCheckoutSuccess({
                transactions: orderResults,
                user: currentUser
            });
            
            // Reset cart
            setCart([]);
            setIsCartOpen(false);
        } catch (err) {
            alert(`Checkout Error: ${err.message}. Please try again.`);
            hasError = true;
        } finally {
            setIsCheckingOut(false);
        }
    };

    // Cart summary metrics
    const cartTotal = cart.reduce((total, item) => total + (item.product.price * item.quantity), 0);
    const cartCount = cart.reduce((count, item) => count + item.quantity, 0);

    return (
        <div className="store-container">
            {/* Top Navbar */}
            <nav className="navbar">
                <div className="nav-logo" onClick={() => fetchProducts()}>
                    🛍️ AuraShop
                </div>
                
                <div className="nav-actions">
                    {/* User profile switcher */}
                    <div className="user-selector">
                        <span>Browsing as:</span>
                        <select 
                            className="select-input"
                            value={currentUser ? currentUser.id : ''} 
                            onChange={(e) => {
                                const selected = users.find(u => u.id === parseInt(e.target.value));
                                if (selected) setCurrentUser(selected);
                            }}
                        >
                            {users.length === 0 ? (
                                <option value="">No Users Seeded</option>
                            ) : (
                                users.map(u => (
                                    <option key={u.id} value={u.id}>{u.name} (ID: {u.id})</option>
                                ))
                            )}
                        </select>
                    </div>

                    {/* Telemetry Health Badge */}
                    <div className="health-badge">
                        <span className={`health-dot ${systemPosture.toLowerCase()}`}></span>
                        <span>Gateway: {systemPosture}</span>
                    </div>

                    {/* Cart Trigger Button */}
                    <button className="cart-trigger" onClick={() => setIsCartOpen(true)}>
                        🛒 Cart <span className="cart-count">{cartCount}</span>
                    </button>
                </div>
            </nav>

            {/* Posture Banners */}
            {systemPosture === 'Alert' && (
                <div className="status-banner alert-banner">
                    <span>⚠️ Proactive cache pre-warming active: API Gateway is preparing for an incoming workload acceleration.</span>
                </div>
            )}
            {systemPosture === 'Critical' && (
                <div className="status-banner critical-banner">
                    <span>🚨 CRITICAL LOAD MITIGATION ACTIVE: Non-essential browsing features are throttled to ensure 100% Checkout reliability.</span>
                </div>
            )}

            {/* Hero / Landing Section */}
            <div className="hero">
                <div className="hero-card">
                    <span className="hero-tag">AURA NEXT-GEN CATALOG</span>
                    <h2>Premium Technology, Intelligently Gated.</h2>
                    <p className="hero-desc">
                        Experience automated scaling resilience. When simulator loads peak, our AI middleware sheds non-critical services (like this product listing catalog) while keeping the checkout cart active and responsive.
                    </p>
                </div>
            </div>

            {/* Product Catalog Grid */}
            <section className="catalog-section">
                <div className="section-header">
                    <h3>Explore Tech Products</h3>
                    <button className="btn-add" style={{backgroundColor: 'rgba(255,255,255,0.05)', color: 'var(--text-primary)'}} onClick={() => fetchProducts()}>
                        🔄 Refresh Catalog
                    </button>
                </div>

                <div className="product-grid">
                    {isCatalogOffline ? (
                        <div className="catalog-offline-card">
                            <span className="offline-icon">🛡️</span>
                            <h4>Catalog Browsing Suspended (HTTP 429 Shedding)</h4>
                            <p>
                                The product catalog service is temporarily offline because the system has entered a <strong>CRITICAL</strong> protective posture. 
                                Telemetry writes are restricted to conserve resource allocation.
                            </p>
                            <p style={{color: 'var(--color-nominal)', fontWeight: 600}}>
                                ✅ Good News: Checkout remains open! You can successfully complete the checkout of any items currently in your cart.
                            </p>
                        </div>
                    ) : products.length === 0 ? (
                        <div style={{gridColumn: '1/-1', textAlign: 'center', color: 'var(--text-secondary)'}}>
                            Loading catalog items from ProductService...
                        </div>
                    ) : (
                        products.map(p => (
                            <div className="product-card" key={p.id}>
                                <div className="product-img-placeholder">
                                    {p.sku.includes('QUANT') ? '💻' : p.sku.includes('CORE') ? '⚡' : '💾'}
                                </div>
                                <div className="product-info">
                                    <span className="product-sku">{p.sku}</span>
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

            {/* Shopping Cart Drawer */}
            <div className={`cart-drawer-overlay ${isCartOpen ? 'active' : ''}`} onClick={() => setIsCartOpen(false)}></div>
            <div className={`cart-drawer ${isCartOpen ? 'active' : ''}`}>
                <div className="cart-header">
                    <h3>Your Cart</h3>
                    <button className="btn-close" onClick={() => setIsCartOpen(false)}>✕</button>
                </div>

                <div className="cart-items">
                    {cart.length === 0 ? (
                        <div style={{textAlign: 'center', marginTop: '40px', color: 'var(--text-secondary)'}}>
                            Your cart is empty. Add products from the catalog.
                        </div>
                    ) : (
                        cart.map(item => (
                            <div className="cart-item" key={item.product.id}>
                                <div className="item-info">
                                    <h5>{item.product.name}</h5>
                                    <span className="item-price">${item.product.price.toFixed(2)}</span>
                                </div>
                                <div className="item-qty-actions">
                                    <button className="qty-btn" onClick={() => updateQuantity(item.product.id, -1)}>-</button>
                                    <span className="qty-count">{item.quantity}</span>
                                    <button className="qty-btn" onClick={() => updateQuantity(item.product.id, 1)}>+</button>
                                </div>
                            </div>
                        ))
                    )}
                </div>

                <div className="cart-footer">
                    <div className="cart-total-row">
                        <span>Grand Total:</span>
                        <span className="price">${cartTotal.toFixed(2)}</span>
                    </div>
                    
                    <button 
                        className="btn-checkout" 
                        disabled={cart.length === 0 || !currentUser || isCheckingOut}
                        onClick={handleCheckout}
                    >
                        {isCheckingOut ? 'Securing Transaction...' : 'Complete Secure Checkout 🔒'}
                    </button>
                    
                    {systemPosture === 'Critical' && (
                        <p style={{fontSize: '11px', color: 'var(--color-nominal)', textAlign: 'center', lineHeight: '1.4'}}>
                            🛡️ AI Gateway is prioritizing this checkout route under Critical Load.
                        </p>
                    )}
                </div>
            </div>

            {/* Success Modal */}
            {checkoutSuccess && (
                <div className="modal-overlay">
                    <div className="modal-content">
                        <span className="success-icon">🎉</span>
                        <h4>Checkout Successful!</h4>
                        <p>Your order has been compiled and processed by the <strong>OrderService</strong>. The API Gateway successfully routed this transaction.</p>
                        
                        <div style={{width: '100%', textAlign: 'left'}}>
                            <p style={{fontSize: '12px', fontWeight: 'bold', marginBottom: '8px'}}>Transaction Records:</p>
                            {checkoutSuccess.transactions.map((tx, idx) => (
                                <div className="tx-id" key={idx}>
                                    <div>Order #{tx.id}</div>
                                    <div>Total Price: ${tx.totalPrice.toFixed(2)}</div>
                                </div>
                            ))}
                        </div>

                        <button className="btn-modal-close" onClick={() => setCheckoutSuccess(null)}>
                            Back to Store
                        </button>
                    </div>
                </div>
            )}
        </div>
    );
}

// Render React App
const root = document.getElementById('root');
ReactDOM.createRoot(root).render(<App />);
