import time
import urllib.request
import urllib.error
import concurrent.futures
import random
import subprocess
import re
import os

def send_request(url):
    start = time.time()
    status = 0
    try:
        req = urllib.request.Request(url, headers={
            'User-Agent': 'Mozilla/5.0',
            'X-Correlation-Id': f'client-{random.randint(1, 100000)}'
        })
        with urllib.request.urlopen(req, timeout=3.0) as response:
            status = response.status
            response.read()
    except urllib.error.HTTPError as e:
        status = e.code
    except Exception:
        status = 500
    latency = (time.time() - start) * 1000.0  # in ms
    return status, latency

def wait_for_gateway():
    print("Waiting for API Gateway to become healthy at http://localhost:5000/products...")
    for i in range(40):
        try:
            req = urllib.request.Request("http://localhost:5000/products")
            with urllib.request.urlopen(req, timeout=1.0) as resp:
                if resp.status == 200:
                    print("API Gateway is online and healthy!")
                    return True
        except Exception:
            pass
        time.sleep(2.0)
    print("Warning: API Gateway did not respond with 200. Proceeding anyway...")
    return False

def get_latest_telemetry():
    try:
        output = subprocess.check_output(["docker", "logs", "apigateway"], stderr=subprocess.STDOUT, text=True)
        lines = output.splitlines()
        telemetry_lines = [l for l in lines if "Online accuracy telemetry" in l]
        if not telemetry_lines:
            return 0.0, 0.0
        # Parse the last line: e.g., "[ApiGateway] Online accuracy telemetry: n=46, running_MAPE=15.22%, running_RMSE=9.18 RPS"
        last_line = telemetry_lines[-1]
        mape_match = re.search(r"running_MAPE=([\d.]+)%", last_line)
        rmse_match = re.search(r"running_RMSE=([\d.]+) RPS", last_line)
        mape = float(mape_match.group(1)) if mape_match else 0.0
        rmse = float(rmse_match.group(1)) if rmse_match else 0.0
        return mape, rmse
    except Exception as e:
        print(f"Error reading gateway logs: {e}")
        return 0.0, 0.0

def run_test(test_id, name, surge_rps, duration=75):
    gateway_url = "http://localhost:5000/products"
    checkout_url = "http://localhost:5000/orders"
    
    print(f"\n========================================================")
    print(f"STARTING {test_id}: {name}")
    print(f"Workload Profile: 10s baseline (10 RPS) -> {duration-10}s surge ({surge_rps} RPS)")
    print(f"========================================================")
    
    # Warmup / Wait for telemetry to settle
    time.sleep(2.0)
    
    latencies = []
    statuses_critical = []
    statuses_non_critical = []
    
    start_time = time.time()
    
    with concurrent.futures.ThreadPoolExecutor(max_workers=surge_rps + 10) as executor:
        for sec in range(duration):
            elapsed = time.time() - start_time
            # Resolve current RPS (baseline vs surge)
            current_rps = 10 if elapsed < 10.0 else surge_rps
            
            # Divide load: 20% critical (checkout/orders), 80% non-critical (catalog/products)
            urls = []
            for _ in range(current_rps):
                is_critical = random.random() < 0.20
                urls.append((checkout_url if is_critical else gateway_url, is_critical))
            
            # Fire requests asynchronously for this second boundary
            futures = {executor.submit(send_request, url): is_critical for url, is_critical in urls}
            
            # Wait for all requests in this tick to complete
            for future in concurrent.futures.as_completed(futures):
                is_critical = futures[future]
                try:
                    status, latency = future.result()
                    latencies.append(latency)
                    if is_critical:
                        statuses_critical.append(status)
                    else:
                        statuses_non_critical.append(status)
                except Exception:
                    pass
            
            print(f"  [Time: {int(elapsed)}s] Active Load: {current_rps} RPS")
            time.sleep(1.0)
            
    print(f"Finishing {test_id}... Waiting for gateway telemetry logging...")
    time.sleep(5.0)
    
    # Compute stats
    p99_latency = sorted(latencies)[int(len(latencies) * 0.99)] if latencies else 0.0
    crit_success = (statuses_critical.count(200) / len(statuses_critical) * 100.0) if statuses_critical else 100.0
    non_crit_success = (statuses_non_critical.count(200) / len(statuses_non_critical) * 100.0) if statuses_non_critical else 100.0
    
    mape, rmse = get_latest_telemetry()
    
    result = {
        "Test ID": test_id,
        "Name": name,
        "Surge RPS": f"{surge_rps} RPS",
        "P99 Latency": f"{p99_latency:.1f} ms",
        "Critical Success": f"{crit_success:.1f}%",
        "Non-Critical Success": f"{non_crit_success:.1f}%",
        "MAPE": f"{mape:.2f}%" if mape > 0 else "N/A (No Surge Tick)",
        "RMSE": f"{rmse:.2f} RPS" if rmse > 0 else "N/A"
    }
    
    print(f"\nRESULTS FOR {test_id}:")
    for k, v in result.items():
        print(f"  {k}: {v}")
    
    return result

def main():
    if not wait_for_gateway():
        print("Error: API Gateway is not responding. Ensure Docker services are started and healthy.")
        return
        
    results = []
    
    # Run the unified experimental matrix T1-T4
    results.append(run_test("T1", "Standard Scaling & Routing", 60))
    results.append(run_test("T2", "Standard Rate-Limiting Threshold", 110))
    results.append(run_test("T3", "Critical Surge & Predictive Mitigation", 120))
    results.append(run_test("T4", "Destructive Stress Limit Test", 180))
    
    print("\n\n========================================================")
    print("CONSOLIDATED EXPERIMENTAL MATRIX RESULTS")
    print("========================================================")
    
    # Print Markdown table
    headers = ["Test ID", "Test Tier Name", "Surge Load", "P99 Latency", "Critical Route Success", "Non-Critical Route Success", "Forecast MAPE", "Forecast RMSE"]
    print("| " + " | ".join(headers) + " |")
    print("| " + " | ".join([":---:" for _ in headers]) + " |")
    
    for r in results:
        row = [
            r["Test ID"],
            r["Name"],
            r["Surge RPS"],
            r["P99 Latency"],
            r["Critical Success"],
            r["Non-Critical Success"],
            r["MAPE"],
            r["RMSE"]
        ]
        print("| " + " | ".join(row) + " |")
        
    # Write to file
    with open("scratch/matrix_run_results.md", "w") as f:
        f.write("# Consolidated Live Experimental Matrix Results\n\n")
        f.write(f"Generated on: {time.strftime('%Y-%m-%d %H:%M:%S')}\n\n")
        f.write("| " + " | ".join(headers) + " |\n")
        f.write("| " + " | ".join([":---:" for _ in headers]) + " |\n")
        for r in results:
            row = [
                r["Test ID"],
                r["Name"],
                r["Surge RPS"],
                r["P99 Latency"],
                r["Critical Success"],
                r["Non-Critical Success"],
                r["MAPE"],
                r["RMSE"]
            ]
            f.write("| " + " | ".join(row) + " |\n")
            
    print("\nResults saved to scratch/matrix_run_results.md")

if __name__ == "__main__":
    main()
