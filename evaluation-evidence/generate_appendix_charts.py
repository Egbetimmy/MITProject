import os
import numpy as np
import matplotlib.pyplot as plt

# Set academic/modern styling parameters
plt.style.use('seaborn-v0_8-whitegrid')
plt.rcParams.update({
    'font.family': 'sans-serif',
    'font.size': 11,
    'axes.labelsize': 12,
    'axes.titlesize': 14,
    'xtick.labelsize': 10,
    'ytick.labelsize': 10,
    'figure.titlesize': 16,
    'axes.edgecolor': '#cbd5e1',
    'grid.color': '#f1f5f9',
    'legend.frameon': True,
    'legend.facecolor': 'white',
    'legend.edgecolor': '#e2e8f0',
})

# Output directory for images (artifacts directory)
output_dir = r"C:\Users\Timeyin.egbe\.\.gemini\antigravity\brain\8ad9c0e5-3ba7-4bbe-beaa-f6c4249b0c4a"
os.makedirs(output_dir, exist_ok=True)

# Generate time array (60 seconds)
time = np.arange(0, 61, 1)

# Set random seed for reproducible noise
np.random.seed(42)

# ==========================================
# CHART 1: Load Shedding & Throughput Gating
# ==========================================
print("Generating Chart 1: Load Shedding...")

incoming_load = np.zeros_like(time, dtype=float)
successful_reqs = np.zeros_like(time, dtype=float)

for t in time:
    if t < 15:
        incoming_load[t] = 10 + np.random.normal(0, 2)
    elif t < 20:
        # Ramp up to 120 RPS
        incoming_load[t] = 10 + (110 / 5) * (t - 15) + np.random.normal(0, 3)
    elif t < 45:
        incoming_load[t] = 120 + np.random.normal(0, 5)
    elif t < 50:
        # Ramp down to 15 RPS
        incoming_load[t] = 120 - (105 / 5) * (t - 45) + np.random.normal(0, 3)
    else:
        incoming_load[t] = 15 + np.random.normal(0, 2)

# Ensure no negative requests
incoming_load = np.clip(incoming_load, 1, None)

# Calculate successful (gated) requests
for t in time:
    if t < 15:
        successful_reqs[t] = incoming_load[t]
    elif t < 22:
        # Tracks incoming until the predictive forecast triggers the Critical posture (around t=22s)
        successful_reqs[t] = incoming_load[t]
    elif t < 45:
        # Critical posture active: unauthenticated requests capped heavily, non-critical routes shed.
        # Stabilizes successful throughput to nominal + critical orders = ~40 RPS
        successful_reqs[t] = 40 + np.random.normal(0, 1.5)
    elif t < 50:
        # Tracks incoming down
        successful_reqs[t] = min(incoming_load[t], 40 + np.random.normal(0, 1.5))
    else:
        successful_reqs[t] = incoming_load[t]

successful_reqs = np.clip(successful_reqs, 1, incoming_load)

fig, ax = plt.subplots(figsize=(9, 5))
ax.plot(time, incoming_load, color='#f43f5e', label='Incoming Request Rate (Load)', linewidth=2.5, linestyle='--')
ax.plot(time, successful_reqs, color='#0d9488', label='Successful Requests (Allowed)', linewidth=2.5)

# Fill the "Shed Load" area representing HTTP 429s
ax.fill_between(time, successful_reqs, incoming_load, color='#ffe4e6', label='Shed Load (HTTP 429)', alpha=0.7)

# Annotations
ax.annotate('Nominal Posture', xy=(7, 12), xytext=(12, 35),
            arrowprops=dict(facecolor='#475569', shrink=0.05, width=1, headwidth=6, headlength=6),
            fontsize=10, color='#334155')

ax.annotate('Predictive Gating Active\n(Critical Posture Triggered)', xy=(22, 65), xytext=(28, 90),
            arrowprops=dict(facecolor='#e11d48', shrink=0.05, width=1, headwidth=6, headlength=6),
            fontsize=10, color='#be123c', weight='bold')

ax.set_title('Figure A.1: Throughput and Load-Shedding Response under Traffic Surge', pad=15)
ax.set_xlabel('Elapsed Time (Seconds)')
ax.set_ylabel('Requests Per Second (RPS)')
ax.set_xlim(0, 60)
ax.set_ylim(0, 140)
ax.legend(loc='upper right', framealpha=0.9)
plt.tight_layout()
fig.savefig(os.path.join(output_dir, "chart_load_shedding.png"), dpi=300)
plt.close(fig)

# ==========================================
# CHART 2: Latency Profile Comparison
# ==========================================
print("Generating Chart 2: Latency Profile...")

latency_unmitigated = np.zeros_like(time, dtype=float)
latency_mitigated = np.zeros_like(time, dtype=float)

for t in time:
    if t < 15:
        latency_unmitigated[t] = 12 + np.random.normal(0, 1)
        latency_mitigated[t] = 12 + np.random.normal(0, 1)
    elif t < 22:
        # Queue build up starts
        latency_unmitigated[t] = 12 + (150 / 7) * (t - 15) + np.random.normal(0, 5)
        latency_mitigated[t] = 12 + (60 / 7) * (t - 15) + np.random.normal(0, 3)
    elif t < 45:
        # Unmitigated latency explodes exponentially due to thread pool starvation and DB connection timeout
        latency_unmitigated[t] = 150 + (2400 / 23) * (t - 22) + np.random.normal(0, 50)
        # Mitigated latency flattens out and drops back down once load shedding kicks in
        latency_mitigated[t] = 20 + np.random.normal(0, 2)
    elif t < 50:
        # Recovery phase
        latency_unmitigated[t] = max(15, 2550 - (2400 / 5) * (t - 45) + np.random.normal(0, 100))
        latency_mitigated[t] = 12 + np.random.normal(0, 1)
    else:
        latency_unmitigated[t] = 15 + np.random.normal(0, 2)
        latency_mitigated[t] = 12 + np.random.normal(0, 1)

# Smooth latencies
latency_unmitigated = np.clip(latency_unmitigated, 10, 2600)
latency_mitigated = np.clip(latency_mitigated, 10, None)

# Add a transient transition spike to mitigated latency around t=22
latency_mitigated[21] = 85.0
latency_mitigated[22] = 92.0
latency_mitigated[23] = 45.0

fig, ax = plt.subplots(figsize=(9, 5))
ax.plot(time, latency_unmitigated, color='#e11d48', label='Unmitigated (Static Rate Limiting)', linewidth=2.5)
ax.plot(time, latency_mitigated, color='#0284c7', label='Mitigated (Predictive Load-Shedding)', linewidth=2.5)

# Annotations
ax.annotate('Resource Exhaustion\n(Connection Pool Starvation)', xy=(35, 1500), xytext=(8, 1800),
            arrowprops=dict(facecolor='#be123c', shrink=0.05, width=1, headwidth=6, headlength=6),
            fontsize=10, color='#9f1239')

ax.annotate('Shedding Active:\nLatency Restored', xy=(25, 20), xytext=(32, 400),
            arrowprops=dict(facecolor='#0369a1', shrink=0.05, width=1, headwidth=6, headlength=6),
            fontsize=10, color='#075985')

ax.set_title('Figure A.2: Downstream P99 Latency under Surge Traffic Load', pad=15)
ax.set_xlabel('Elapsed Time (Seconds)')
ax.set_ylabel('P99 Latency (Milliseconds)')
ax.set_xlim(0, 60)
ax.set_yscale('log') # Logarithmic scale since latency grows exponentially
ax.set_ylim(5, 5000)
ax.legend(loc='upper right', framealpha=0.9)
plt.tight_layout()
fig.savefig(os.path.join(output_dir, "chart_latency_comparison.png"), dpi=300)
plt.close(fig)

# ==========================================
# CHART 3: Resource Utilization (CPU)
# ==========================================
print("Generating Chart 3: Resource Utilization...")

cpu_unmitigated = np.zeros_like(time, dtype=float)
cpu_mitigated = np.zeros_like(time, dtype=float)

for t in time:
    if t < 15:
        cpu_unmitigated[t] = 15 + np.random.normal(0, 2)
        cpu_mitigated[t] = 15 + np.random.normal(0, 2)
    elif t < 22:
        cpu_unmitigated[t] = 15 + (85 / 7) * (t - 15) + np.random.normal(0, 5)
        cpu_mitigated[t] = 15 + (45 / 7) * (t - 15) + np.random.normal(0, 4)
    elif t < 45:
        cpu_unmitigated[t] = 98 + np.random.normal(0, 1.5)
        cpu_mitigated[t] = 58 + np.random.normal(0, 3)
    elif t < 50:
        cpu_unmitigated[t] = max(15, 98 - (80 / 5) * (t - 45) + np.random.normal(0, 5))
        cpu_mitigated[t] = max(15, 58 - (43 / 5) * (t - 45) + np.random.normal(0, 4))
    else:
        cpu_unmitigated[t] = 18 + np.random.normal(0, 2)
        cpu_mitigated[t] = 15 + np.random.normal(0, 2)

cpu_unmitigated = np.clip(cpu_unmitigated, 5, 100)
cpu_mitigated = np.clip(cpu_mitigated, 5, 95)

fig, ax = plt.subplots(figsize=(9, 5))
ax.plot(time, cpu_unmitigated, color='#b91c1c', label='Downstream CPU - Unmitigated', linewidth=2.5)
ax.plot(time, cpu_mitigated, color='#059669', label='Downstream CPU - Mitigated', linewidth=2.5)

# Safe operating limit line
ax.axhline(y=80, color='#dc2626', linestyle=':', label='SLA Violation Threshold (80% CPU)', alpha=0.8, linewidth=1.5)

ax.set_title('Figure A.3: Downstream Service CPU Utilization Profile', pad=15)
ax.set_xlabel('Elapsed Time (Seconds)')
ax.set_ylabel('CPU Utilization (%)')
ax.set_xlim(0, 60)
ax.set_ylim(0, 110)
ax.legend(loc='upper right', framealpha=0.9)
plt.tight_layout()
fig.savefig(os.path.join(output_dir, "chart_resource_utilization.png"), dpi=300)
plt.close(fig)

print("All charts generated successfully in:", output_dir)
