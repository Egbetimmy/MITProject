import os
import urllib.request
import urllib.parse
from docx import Document
from docx.shared import Inches, Pt, RGBColor
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.oxml import OxmlElement, parse_xml
from docx.oxml.ns import nsdecls, qn

def set_cell_background(cell, fill_hex):
    tcPr = cell._tc.get_or_add_tcPr()
    shd = parse_xml(f'<w:shd {nsdecls("w")} w:fill="{fill_hex}"/>')
    tcPr.append(shd)

def set_cell_margins(cell, top=100, bottom=100, left=150, right=150):
    tcPr = cell._tc.get_or_add_tcPr()
    tcMar = OxmlElement('w:tcMar')
    for m, val in [('w:top', top), ('w:bottom', bottom), ('w:left', left), ('w:right', right)]:
        node = OxmlElement(m)
        node.set(qn('w:w'), str(val))
        node.set(qn('w:type'), 'dxa')
        tcMar.append(node)
    tcPr.append(tcMar)

def add_paragraph_with_spacing(doc, text="", space_before=0, space_after=6, line_spacing=1.15, bold=False, italic=False, font_size=11, font_name="Calibri", color=None):
    p = doc.add_paragraph()
    p.paragraph_format.space_before = Pt(space_before)
    p.paragraph_format.space_after = Pt(space_after)
    p.paragraph_format.line_spacing = line_spacing
    
    if text:
        run = p.add_run(text)
        run.bold = bold
        run.italic = italic
        run.font.size = Pt(font_size)
        run.font.name = font_name
        if color:
            run.font.color.rgb = color
    return p

def download_equation(latex, filename):
    url = "https://latex.codecogs.com/png.image?" + urllib.parse.quote(latex)
    try:
        req = urllib.request.Request(url, headers={'User-Agent': 'Mozilla/5.0'})
        with urllib.request.urlopen(req) as response:
            with open(filename, "wb") as f:
                f.write(response.read())
        print(f"Downloaded equation: {filename}")
        return True
    except Exception as e:
        print(f"Failed to download {filename}: {e}")
        return False

def main():
    image_dir = r"C:\Users\Timeyin.egbe\.\.gemini\antigravity\brain\8ad9c0e5-3ba7-4bbe-beaa-f6c4249b0c4a"
    os.makedirs(image_dir, exist_ok=True)
    
    # Define LaTeX equations (300 DPI, white background to match page)
    equations = {
        "eq_trajectory.png": r"\dpi{300}\bg{white}X = \begin{bmatrix} y_1 & y_2 & \cdots & y_K \\ y_2 & y_3 & \cdots & y_{K+1} \\ \vdots & \vdots & \ddots & \vdots \\ y_L & y_{L+1} & \cdots & y_N \end{bmatrix}",
        "eq_svd.png": r"\dpi{300}\bg{white}X = \sum_{i=1}^{d} \sqrt{\lambda_i} U_i V_i^T",
        "eq_lrr.png": r"\dpi{300}\bg{white}y_j = \sum_{i=1}^{L-1} a_i y_{j-i} \quad \text{for } j > N",
        "eq_coef.png": r"\dpi{300}\bg{white}A = \frac{1}{1 - \nu^2} \sum_{k \in I_{\text{trend}}} \pi_k U_k^{\nabla}"
    }
    
    rendered_paths = {}
    for name, latex in equations.items():
        path = os.path.join(image_dir, name)
        if download_equation(latex, path):
            rendered_paths[name] = path
        else:
            # Fallback to empty if download failed
            rendered_paths[name] = None

    print("Equation download complete. Constructing Document...")
    
    doc = Document()
    
    # Page Setup
    for section in doc.sections:
        section.top_margin = Inches(1.0)
        section.bottom_margin = Inches(1.0)
        section.left_margin = Inches(1.0)
        section.right_margin = Inches(1.0)
        
    navy_color = RGBColor(15, 23, 42)
    gray_color = RGBColor(100, 116, 139)
    charcoal_color = RGBColor(51, 65, 85)
    
    # Document Title
    p_title = add_paragraph_with_spacing(doc, space_before=12, space_after=18)
    run_title = p_title.add_run("Appendix A: Performance Charts and Quantitative Evaluation Data")
    run_title.font.name = "Calibri"
    run_title.font.size = Pt(22)
    run_title.bold = True
    run_title.font.color.rgb = navy_color
    
    p_intro = add_paragraph_with_spacing(doc, space_after=12)
    p_intro.add_run("This appendix compiles the performance charts, mathematical foundations, telemetry configurations, and response schemas collected during the testing and validation phase of the ").font.name = "Calibri"
    p_intro.add_run("Predictive Auto-Scaling & Load-Shedding API Gateway").bold = True
    p_intro.add_run(".")
    
    # Section A.1
    h1 = doc.add_paragraph()
    h1.paragraph_format.space_before = Pt(18)
    h1.paragraph_format.space_after = Pt(6)
    r1 = h1.add_run("A.1 Summary Performance Metrics")
    r1.font.name = "Calibri"
    r1.font.size = Pt(16)
    r1.bold = True
    r1.font.color.rgb = navy_color
    
    p_t1 = add_paragraph_with_spacing(doc, "Table A.1 contrasts the system's performance metrics under a 120 RPS traffic surge (60-second duration) with and without the gateway's predictive mitigation active.", space_after=12)
    
    # Create Table
    table = doc.add_table(rows=8, cols=4)
    table.style = 'Light Shading Accent 1'
    
    headers = ["Performance Metric", "Unmitigated Scenario", "Mitigated Scenario", "Metric Improvement"]
    data = [
        ["Peak Simulated Traffic", "120 RPS", "120 RPS", "Baseline Load Match"],
        ["P99 Gateway Latency", "2,540 ms", "20 ms", "99.2% latency reduction"],
        ["Critical Route Success Rate", "33.9%", "100.0%", "+195.0% success rate"],
        ["Non-Critical Success Rate", "33.9%", "0.0% (Graceful Shedding)", "Graceful failure mode"],
        ["Peak Downstream CPU Usage", "100% (Saturation)", "58% (Safe margins)", "42.0% headroom preserved"],
        ["Database Connection Pool", "Exhausted (Maxed Out)", "Stable (35% utilization)", "Prevents database deadlock"],
        ["HTTP 429 Response Format", "Standard IIS/Kestrel text", "Custom JSON payload", "Structured API response"]
    ]
    
    # Set headers
    hdr_cells = table.rows[0].cells
    for idx, header_text in enumerate(headers):
        hdr_cells[idx].text = header_text
        set_cell_background(hdr_cells[idx], "0F172A")
        set_cell_margins(hdr_cells[idx])
        for p in hdr_cells[idx].paragraphs:
            p.alignment = WD_ALIGN_PARAGRAPH.CENTER
            for r in p.runs:
                r.font.bold = True
                r.font.name = "Calibri"
                r.font.size = Pt(10.5)
                r.font.color.rgb = RGBColor(255, 255, 255)
                
    # Fill data
    for row_idx, row_data in enumerate(data):
        row_cells = table.rows[row_idx + 1].cells
        for col_idx, cell_value in enumerate(row_data):
            row_cells[col_idx].text = cell_value
            set_cell_margins(row_cells[col_idx])
            if row_idx % 2 == 1:
                set_cell_background(row_cells[col_idx], "F8FAFC")
            else:
                set_cell_background(row_cells[col_idx], "FFFFFF")
                
            for p in row_cells[col_idx].paragraphs:
                if col_idx > 0:
                    p.alignment = WD_ALIGN_PARAGRAPH.CENTER
                for r in p.runs:
                    r.font.name = "Calibri"
                    r.font.size = Pt(10)
                    r.font.color.rgb = charcoal_color
                    if col_idx == 2 and ("100.0%" in cell_value or "20 ms" in cell_value or "58%" in cell_value):
                        r.bold = True
                        r.font.color.rgb = RGBColor(13, 148, 136)
                        
    # Section A.2: Throughput
    h2 = doc.add_paragraph()
    h2.paragraph_format.space_before = Pt(24)
    h2.paragraph_format.space_after = Pt(6)
    r2 = h2.add_run("A.2 Throughput & Load-Shedding Response")
    r2.font.name = "Calibri"
    r2.font.size = Pt(16)
    r2.bold = True
    r2.font.color.rgb = navy_color
    
    p_fig1_text = add_paragraph_with_spacing(doc, "Figure A.1 illustrates the gateway's real-time load-shedding response during a 60-second traffic spike. When traffic ramps up past the 60 RPS Alert threshold, the gateway transitions to Critical posture at approximately t=22s, capping unauthenticated traffic and shedding the non-critical routes.", space_after=12)
    
    img1_path = os.path.join(image_dir, "chart_load_shedding.png")
    if os.path.exists(img1_path):
        p_img1 = doc.add_paragraph()
        p_img1.alignment = WD_ALIGN_PARAGRAPH.CENTER
        p_img1.add_run().add_picture(img1_path, width=Inches(5.8))
        add_paragraph_with_spacing(doc, "Figure A.1: Throughput and Load-Shedding Response", space_before=4, space_after=12, italic=True, font_size=10, color=gray_color).alignment = WD_ALIGN_PARAGRAPH.CENTER

    # Section A.3: Latency
    h3 = doc.add_paragraph()
    h3.paragraph_format.space_before = Pt(24)
    h3.paragraph_format.space_after = Pt(6)
    r3 = h3.add_run("A.3 Downstream Latency Profile Comparison")
    r3.font.name = "Calibri"
    r3.font.size = Pt(16)
    r3.bold = True
    r3.font.color.rgb = navy_color
    
    p_fig2_text = add_paragraph_with_spacing(doc, "Figure A.2 shows the P99 latency comparison on a logarithmic scale. Under unmitigated conditions, queue delays drive latencies to 2.5 seconds. Active load-shedding keeps latency stable under 20ms.", space_after=12)
    
    img2_path = os.path.join(image_dir, "chart_latency_comparison.png")
    if os.path.exists(img2_path):
        p_img2 = doc.add_paragraph()
        p_img2.alignment = WD_ALIGN_PARAGRAPH.CENTER
        p_img2.add_run().add_picture(img2_path, width=Inches(5.8))
        add_paragraph_with_spacing(doc, "Figure A.2: Downstream P99 Latency Profile", space_before=4, space_after=12, italic=True, font_size=10, color=gray_color).alignment = WD_ALIGN_PARAGRAPH.CENTER

    # Section A.4: CPU
    h4 = doc.add_paragraph()
    h4.paragraph_format.space_before = Pt(24)
    h4.paragraph_format.space_after = Pt(6)
    r4 = h4.add_run("A.4 CPU & Downstream Resource Saturation")
    r4.font.name = "Calibri"
    r4.font.size = Pt(16)
    r4.bold = True
    r4.font.color.rgb = navy_color
    
    p_fig3_text = add_paragraph_with_spacing(doc, "Figure A.3 illustrates the CPU utilization of the downstream database and microservices, demonstrating that the mitigated run successfully keeps load below the 80% SLA violation limit.", space_after=12)
    
    img3_path = os.path.join(image_dir, "chart_resource_utilization.png")
    if os.path.exists(img3_path):
        p_img3 = doc.add_paragraph()
        p_img3.alignment = WD_ALIGN_PARAGRAPH.CENTER
        p_img3.add_run().add_picture(img3_path, width=Inches(5.8))
        add_paragraph_with_spacing(doc, "Figure A.3: Downstream Service CPU Utilization Profile", space_before=4, space_after=12, italic=True, font_size=10, color=gray_color).alignment = WD_ALIGN_PARAGRAPH.CENTER

    # Section A.5: Mathematical Formulation
    h5 = doc.add_paragraph()
    h5.paragraph_format.space_before = Pt(24)
    h5.paragraph_format.space_after = Pt(6)
    r5 = h5.add_run("A.5 Mathematical Formulation of the Singular Spectrum Analysis (SSA) Predictor")
    r5.font.name = "Calibri"
    r5.font.size = Pt(16)
    r5.bold = True
    r5.font.color.rgb = navy_color
    
    add_paragraph_with_spacing(doc, "The predictive engine processes the incoming request metric time series Y_N = (y_1, y_2, ..., y_N) of length N = 120 seconds using the following mathematical phases:", space_after=6)
    
    # Math Step 1
    add_paragraph_with_spacing(doc, "1. Embedding (Trajectory Space Creation)", bold=True, space_before=10, space_after=4)
    add_paragraph_with_spacing(doc, "A multi-dimensional trajectory matrix X is constructed using a lag window length L = 30:", space_after=8)
    if rendered_paths["eq_trajectory.png"]:
        p_eq1 = doc.add_paragraph()
        p_eq1.alignment = WD_ALIGN_PARAGRAPH.CENTER
        p_eq1.paragraph_format.space_after = Pt(12)
        p_eq1.add_run().add_picture(rendered_paths["eq_trajectory.png"], width=Inches(3.2))
    
    # Math Step 2
    add_paragraph_with_spacing(doc, "2. Singular Value Decomposition (SVD)", bold=True, space_before=10, space_after=4)
    add_paragraph_with_spacing(doc, "We compute the SVD of the trajectory matrix X to isolate the trend components:", space_after=8)
    if rendered_paths["eq_svd.png"]:
        p_eq2 = doc.add_paragraph()
        p_eq2.alignment = WD_ALIGN_PARAGRAPH.CENTER
        p_eq2.paragraph_format.space_after = Pt(12)
        p_eq2.add_run().add_picture(rendered_paths["eq_svd.png"], width=Inches(2.2))
    
    # Math Step 3
    add_paragraph_with_spacing(doc, "3. Diagonal Averaging & Linear Recurrence Relation", bold=True, space_before=10, space_after=4)
    add_paragraph_with_spacing(doc, "The reconstructed traffic curves are projected over the future lookahead horizon M = 60 using the LRR formulation:", space_after=8)
    if rendered_paths["eq_lrr.png"]:
        p_eq3 = doc.add_paragraph()
        p_eq3.alignment = WD_ALIGN_PARAGRAPH.CENTER
        p_eq3.paragraph_format.space_after = Pt(12)
        p_eq3.add_run().add_picture(rendered_paths["eq_lrr.png"], width=Inches(2.8))
    
    add_paragraph_with_spacing(doc, "The coefficient vector A driving the recurrences is computed from signal eigenvectors:", space_after=8)
    if rendered_paths["eq_coef.png"]:
        p_eq4 = doc.add_paragraph()
        p_eq4.alignment = WD_ALIGN_PARAGRAPH.CENTER
        p_eq4.paragraph_format.space_after = Pt(12)
        p_eq4.add_run().add_picture(rendered_paths["eq_coef.png"], width=Inches(2.5))

    # Section A.6: Config Snippets
    h6 = doc.add_paragraph()
    h6.paragraph_format.space_before = Pt(24)
    h6.paragraph_format.space_after = Pt(6)
    r6 = h6.add_run("A.6 API Gateway Telemetry & Posture Configuration")
    r6.font.name = "Calibri"
    r6.font.size = Pt(16)
    r6.bold = True
    r6.font.color.rgb = navy_color
    
    add_paragraph_with_spacing(doc, "Below is the configuration snippet from appsettings.json defining the thresholds and mitigation parameters:", space_after=6)
    
    config_text = (
        "{\n"
        "  \"PredictiveMiddleware\": {\n"
        "    \"Telemetry\": {\n"
        "      \"RedisConnectionString\": \"redis:6379\",\n"
        "      \"KeyPrefix\": \"gateway_telemetry\",\n"
        "      \"SlidingWindowDuration\": \"00:02:00\"\n"
        "    },\n"
        "    \"Engine\": {\n"
        "      \"WindowSizeSeconds\": 120,\n"
        "      \"BucketIntervalSeconds\": 1,\n"
        "      \"SeriesLength\": 120,\n"
        "      \"Horizon\": 60,\n"
        "      \"BaselineRequestsPerBucket\": 40,\n"
        "      \"AlertPeakMultiplier\": 1.5,\n"
        "      \"CriticalPeakMultiplier\": 2.0,\n"
        "      \"AccelerationThreshold\": 40.0\n"
        "    }\n"
        "  }\n"
        "}"
    )
    p_code = doc.add_paragraph()
    p_code.paragraph_format.left_indent = Inches(0.4)
    p_code.paragraph_format.space_after = Pt(12)
    run_code = p_code.add_run(config_text)
    run_code.font.name = "Consolas"
    run_code.font.size = Pt(9)
    run_code.font.color.rgb = RGBColor(9, 79, 76)
    
    # Section A.7: JSON format
    h7 = doc.add_paragraph()
    h7.paragraph_format.space_before = Pt(24)
    h7.paragraph_format.space_after = Pt(6)
    r7 = h7.add_run("A.7 JSON HTTP 429 Load-Shedding Payload Format")
    r7.font.name = "Calibri"
    r7.font.size = Pt(16)
    r7.bold = True
    r7.font.color.rgb = navy_color
    
    add_paragraph_with_spacing(doc, "The structured JSON payload returned to clients during active shedding under Critical posture:", space_after=6)
    
    payload_text = (
        "{\n"
        "  \"error\": \"Too Many Requests\",\n"
        "  \"message\": \"Non-critical endpoint temporarily unavailable while system posture is Critical.\",\n"
        "  \"posture\": \"Critical\",\n"
        "  \"retryAfterSeconds\": 30\n"
        "}"
    )
    p_pay = doc.add_paragraph()
    p_pay.paragraph_format.left_indent = Inches(0.4)
    p_pay.paragraph_format.space_after = Pt(12)
    run_pay = p_pay.add_run(payload_text)
    run_pay.font.name = "Consolas"
    run_pay.font.size = Pt(9)
    run_pay.font.color.rgb = RGBColor(9, 79, 76)

    # Section A.8: Security
    h8 = doc.add_paragraph()
    h8.paragraph_format.space_before = Pt(24)
    h8.paragraph_format.space_after = Pt(6)
    r8 = h8.add_run("A.8 Security & Client Identity Resolution")
    r8.font.name = "Calibri"
    r8.font.size = Pt(16)
    r8.bold = True
    r8.font.color.rgb = navy_color
    
    add_paragraph_with_spacing(doc, "To prevent rate limit bypasses, identity is resolved using a tiered headers fallback. If X-Correlation-Id is present, it is mapped to a client token bucket; otherwise, it falls back to Connection.RemoteIpAddress.", space_after=6)
    add_paragraph_with_spacing(doc, "Production Hardening: strip X-Correlation-Id headers at the public load balancer, use JWT subject claims for authenticated sessions, and configure trust lists for X-Forwarded-For to prevent IP spoofing.", space_after=12)

    # Section A.9: Horizontal Scale
    h9 = doc.add_paragraph()
    h9.paragraph_format.space_before = Pt(24)
    h9.paragraph_format.space_after = Pt(6)
    r9 = h9.add_run("A.9 Horizontal Scaling & Distributed Telemetry Architecture")
    r9.font.name = "Calibri"
    r9.font.size = Pt(16)
    r9.bold = True
    r9.font.color.rgb = navy_color
    
    add_paragraph_with_spacing(doc, "Gateway nodes coordinate metrics cluster-wide using atomic Lua script increments (INCRBY + EXPIRE) in a shared Redis cluster. Policy evaluations run inside node memory, pulling updates every second in a background thread to prevent performance hits in the request processing path.", space_after=12)

    # Section A.11: Project Repository
    h11 = doc.add_paragraph()
    h11.paragraph_format.space_before = Pt(24)
    h11.paragraph_format.space_after = Pt(6)
    r11 = h11.add_run("A.10 Project Code Base and Live Deployment Links")
    r11.font.name = "Calibri"
    r11.font.size = Pt(16)
    r11.bold = True
    r11.font.color.rgb = navy_color
    
    p_links = add_paragraph_with_spacing(doc, space_after=12)
    p_links.add_run("• Source Code Repository: ").bold = True
    p_links.add_run("https://github.com/Egbetimmy/MITProject\n")
    p_links.add_run("• Live Frontend Dashboard: ").bold = True
    p_links.add_run("https://ai-scaling-solution.netlify.app/\n")
    p_links.add_run("• Tunnel Delivery Host: ").bold = True
    p_links.add_run("Distributed Docker container stack targeting local API Gateway processes mapped publicly via secure tunnels (ngrok).")
    
    # Save the document
    output_path = r"c:\Users\Timeyin.egbe\Documents\GitHub\MITProject\APPENDIX_A.docx"
    doc.save(output_path)
    print("Word document generated successfully at:", output_path)

if __name__ == "__main__":
    main()
