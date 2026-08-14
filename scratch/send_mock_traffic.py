import time
import urllib.request
import concurrent.futures
import random

def send_request(url):
    try:
        # Simple HTTP request to gateway. 
        # Even if downstream is offline, the gateway records the traffic event before proxying.
        req = urllib.request.Request(url, headers={'User-Agent': 'Mozilla/5.0', 'X-Correlation-Id': f'client-{random.randint(1, 5)}'})
        with urllib.request.urlopen(req, timeout=1.0) as response:
            response.read()
    except Exception:
        pass

def main():
    gateway_url = "http://localhost:5000/products"
    checkout_url = "http://localhost:5000/orders"
    
    print("Starting mock traffic generator (running for 85 seconds)...")
    start_time = time.time()
    
    # We will simulate traffic:
    # Seconds 0-30: 15 RPS (Nominal)
    # Seconds 30-45: Ramp up to 60 RPS (Surge)
    # Seconds 45-85: Sustain at 60 RPS
    with concurrent.futures.ThreadPoolExecutor(max_workers=50) as executor:
        while True:
            elapsed = time.time() - start_time
            if elapsed > 85.0:
                break
                
            # Resolve current target RPS
            if elapsed < 30.0:
                rps = 15
            elif elapsed < 45.0:
                rps = int(15 + (45 / 15) * (elapsed - 30))
            else:
                rps = 60
                
            # Send the batch for this second
            urls = [checkout_url if random.random() > 0.6 else gateway_url for _ in range(rps)]
            futures = [executor.submit(send_request, url) for url in urls]
            
            # Sleep until next second boundary
            time.sleep(1.0)
            
    print("Traffic generator complete.")

if __name__ == "__main__":
    main()
