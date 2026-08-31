import os
import glob
import shutil

def main():
    art_dir = r"C:\Users\Timeyin.egbe\.gemini\antigravity\brain\8ad9c0e5-3ba7-4bbe-beaa-f6c4249b0c4a"
    
    # 1. Rename files
    store_matches = glob.glob(os.path.join(art_dir, "app_aurastore_ui_*.jpg"))
    if store_matches:
        new_store_path = os.path.join(art_dir, "app_aurastore_ui.jpg")
        shutil.copy2(store_matches[0], new_store_path)
        print("Copied store screenshot to:", new_store_path)
        
    dash_matches = glob.glob(os.path.join(art_dir, "app_operator_dashboard_ui_*.jpg"))
    if dash_matches:
        new_dash_path = os.path.join(art_dir, "app_operator_dashboard_ui.jpg")
        shutil.copy2(dash_matches[0], new_dash_path)
        print("Copied dashboard screenshot to:", new_dash_path)
        
if __name__ == "__main__":
    main()
