import os
import time
import subprocess

def capture_screenshot(url, output_path):
    chrome_path = r"C:\Program Files\Google\Chrome\Application\chrome.exe"
    if not os.path.exists(chrome_path):
        print(f"Error: Chrome not found at {chrome_path}")
        return False
        
    cmd = [
        chrome_path,
        "--headless",
        "--disable-gpu",
        "--window-size=1280,960",
        "--hide-scrollbars",
        f"--screenshot={output_path}",
        url
    ]
    
    try:
        # Run headless chrome to take screenshot
        subprocess.run(cmd, check=True)
        print(f"Successfully captured {url} -> {output_path}")
        return True
    except Exception as e:
        print(f"Failed to capture screenshot for {url}: {e}")
        return False

def main():
    art_dir = r"C:\Users\Timeyin.egbe\.gemini\antigravity\brain\8ad9c0e5-3ba7-4bbe-beaa-f6c4249b0c4a"
    os.makedirs(art_dir, exist_ok=True)
    
    # 1. Capture Storefront (AuraStore)
    store_url = "http://localhost:5173/"
    store_output = os.path.join(art_dir, "app_aurastore_ui.png")
    
    # 2. Capture Operator Dashboard (demo telemetry active)
    dash_url = "http://localhost:5173/?tab=operator&demo=true"
    dash_output = os.path.join(art_dir, "app_operator_dashboard_ui.png")
    
    # Run captures
    print("Capturing storefront catalog...")
    capture_screenshot(store_url, store_output)
    
    # Wait briefly for React dev server to handle secondary page requests
    time.sleep(2)
    
    print("Capturing operator diagnostics dashboard...")
    capture_screenshot(dash_url, dash_output)

if __name__ == "__main__":
    main()
