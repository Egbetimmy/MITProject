import os
import subprocess
import time
import threading
import json
import re

BASE_DIR = r"c:\Users\Timeyin.egbe\Documents\GitHub\MITProject"
SOLUTION_DIR = os.path.join(BASE_DIR, "AIScalingSolution")
ENV_PATH = os.path.join(SOLUTION_DIR, ".env")
K6_PATH = os.path.join(BASE_DIR, r"k6-bin\k6-v0.52.0-windows-amd64\k6.exe")
SMOKE_SCRIPT = os.path.join(BASE_DIR, r"scratch\smoke_test.js")

def write_env(config_dict):
    with open(ENV_PATH, "w") as f:
        for k, v in config_dict.items():
            f.write(f"{k}={v}\n")
    print(f"[Smoke] Configured .env with: {config_dict}")

def run_cmd(cmd, cwd=None):
    try:
        res = subprocess.run(cmd, shell=True, check=True, stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True, cwd=cwd)
        return res.stdout
    except subprocess.CalledProcessError as e:
        print(f"[Smoke] Command failed: {cmd}\nError: {e.stderr}")
        return None

def poll_cpu_worker(stop_event, samples):
    container_names = ["apigateway", "productservice", "orderservice", "userservice", "aiscaling-sqlserver"]
    cmd = f"docker stats --no-stream --format " + '"{{.Name}}: {{.CPUPerc}}"' + " " + " ".join(container_names)
    while not stop_event.is_set():
        out = run_cmd(cmd)
        if out:
            timestamp = time.time()
            for line in out.strip().split("\n"):
                if ":" in line:
                    name, cpu_str = line.split(":")
                    cpu_val = float(cpu_str.replace("%", "").strip())
                    samples.append((name.strip(), cpu_val))
        time.sleep(2)

def run_smoke_test():
    print("[Smoke] Starting Capacity Stress Smoke Test under constrained limits...")
    
    # 1. Configure gateway to be unmitigated (Prediction-Only style for non-critical route)
    env_config = {
        "PredictiveMiddleware__Engine__DisableForecasting": "false",
        "PredictiveMiddleware__NonCriticalRoutePrefixes__0": "/non-existent-route"
    }
    write_env(env_config)
    
    # 2. Restart ApiGateway
    print("[Smoke] Recreating apigateway container...")
    run_cmd("docker compose up -d --force-recreate apigateway", cwd=SOLUTION_DIR)
    time.sleep(5)
    
    # 3. Flush Redis
    print("[Smoke] Flushing Redis...")
    run_cmd("docker exec -i aiscaling-redis redis-cli flushall")
    time.sleep(1)
    
    # 4. CPU Polling Setup
    stop_cpu = threading.Event()
    cpu_samples = []
    cpu_thread = threading.Thread(target=poll_cpu_worker, args=(stop_cpu, cpu_samples))
    cpu_thread.start()
    
    # 5. Run k6
    k6_summary_file = os.path.join(BASE_DIR, r"scratch\smoke_k6_summary.json")
    k6_cmd = f'"{K6_PATH}" run --summary-export="{k6_summary_file}" "{SMOKE_SCRIPT}"'
    print(f"[Smoke] Running k6: {k6_cmd}")
    
    k6_out = run_cmd(k6_cmd, cwd=BASE_DIR)
    print("[Smoke] k6 run finished.")
    
    # 6. Stop CPU Polling
    stop_cpu.set()
    cpu_thread.join()
    
    # 7. Aggregate CPU
    cpu_by_container = {}
    for name, val in cpu_samples:
        cpu_by_container.setdefault(name, []).append(val)
        
    print("\n=== CONTAINER CPU STATISTICS (SMOKE TEST) ===")
    for name, vals in cpu_by_container.items():
        avg_val = sum(vals) / len(vals) if vals else 0.0
        max_val = max(vals) if vals else 0.0
        print(f"{name:25} | Avg CPU: {avg_val:5.2f}% | Max CPU: {max_val:5.2f}%")
        
    # 8. Parse k6 summary
    if os.path.exists(k6_summary_file):
        with open(k6_summary_file, "r") as f:
            data = json.load(f)
        metrics = data.get("metrics", {})
        
        # P99
        duration = metrics.get("http_req_duration", {})
        p99 = duration.get("p(99)", 0.0)
        if isinstance(duration.get("values"), dict):
            p99 = duration.get("values", {}).get("p(99)", p99)
            
        avg_lat = duration.get("avg", 0.0)
        if isinstance(duration.get("values"), dict):
            avg_lat = duration.get("values", {}).get("avg", avg_lat)
            
        total_reqs = metrics.get("http_reqs", {}).get("count", 0)
        if isinstance(metrics.get("http_reqs", {}).get("values"), dict):
            total_reqs = metrics.get("http_reqs", {}).get("values", {}).get("count", total_reqs)
            
        print("\n=== K6 METRICS (SMOKE TEST) ===")
        print(f"Total Requests: {total_reqs}")
        print(f"P99 Latency:    {p99:.2f} ms")
        print(f"Average Latency: {avg_lat:.2f} ms")
    else:
        print("[Smoke] Error: k6 summary file not found.")

if __name__ == "__main__":
    run_smoke_test()
