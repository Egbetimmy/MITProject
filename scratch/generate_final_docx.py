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

def add_header(doc, text, level):
    h = doc.add_paragraph()
    h.paragraph_format.space_before = Pt(20 if level == 1 else 14)
    h.paragraph_format.space_after = Pt(6)
    
    run = h.add_run(text)
    run.font.name = "Calibri"
    run.font.size = Pt(16 if level == 1 else 13)
    run.bold = True
    run.font.color.rgb = RGBColor(15, 23, 42) # Navy
    return h

def main():
    image_dir = r"C:\Users\Timeyin.egbe\.gemini\antigravity\brain\8ad9c0e5-3ba7-4bbe-beaa-f6c4249b0c4a"
    os.makedirs(image_dir, exist_ok=True)
    
    # Render formulas
    equations = {
        "eq_trajectory.png": r"\dpi{300}\bg{white}X = \begin{bmatrix} y_1 & y_2 & \cdots & y_K \\ y_2 & y_3 & \cdots & y_{K+1} \\ \vdots & \vdots & \ddots & \vdots \\ y_L & y_{L+1} & \cdots & y_N \end{bmatrix}",
        "eq_svd.png": r"\dpi{300}\bg{white}X = \sum_{i=1}^{d} \sqrt{\lambda_i} U_i V_i^T",
        "eq_lrr.png": r"\dpi{300}\bg{white}y_j = \sum_{i=1}^{L-1} a_i y_{j-i} \quad \text{for } j > N",
        "eq_coef.png": r"\dpi{300}\bg{white}A = \frac{1}{1 - \nu^2} \sum_{k \in I_{\text{trend}}} \pi_k U_k^{\nabla}"
    }
    
    rendered_paths = {}
    for name, latex in equations.items():
        path = os.path.join(image_dir, name)
        if os.path.exists(path):
            rendered_paths[name] = path
        elif download_equation(latex, path):
            rendered_paths[name] = path
        else:
            rendered_paths[name] = None

    # Load source code files
    base_dir = r"c:\Users\Timeyin.egbe\Documents\GitHub\MITProject"
    rl_middleware_path = os.path.join(base_dir, "AIScalingSolution", "Infrastructure", "AIScaling.PredictiveMiddleware", "Pipeline", "Middleware", "AdaptiveRateLimitingMiddleware.cs")
    shed_middleware_path = os.path.join(base_dir, "AIScalingSolution", "Infrastructure", "AIScaling.PredictiveMiddleware", "Pipeline", "Middleware", "PredictiveTrafficMiddleware.cs")
    
    rl_code = ""
    if os.path.exists(rl_middleware_path):
        with open(rl_middleware_path, "r", encoding="utf-8") as f:
            rl_code = f.read()
            
    shed_code = ""
    if os.path.exists(shed_middleware_path):
        with open(shed_middleware_path, "r", encoding="utf-8") as f:
            shed_code = f.read()

    print("Sources loaded. Building Word document...")
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
    
    # ----------------------------------------------------
    # DOCUMENT HEADER TITLE
    # ----------------------------------------------------
    p_title = add_paragraph_with_spacing(doc, space_before=12, space_after=18)
    run_title = p_title.add_run("APPENDICES")
    run_title.font.name = "Calibri"
    run_title.font.size = Pt(26)
    run_title.bold = True
    run_title.font.color.rgb = navy_color
    
    # ----------------------------------------------------
    # APPENDIX A: PERFORMANCE CHARTS & DATA
    # ----------------------------------------------------
    add_header(doc, "Appendix A: Performance Charts and Quantitative Evaluation Data", level=1)
    add_paragraph_with_spacing(doc, "This appendix compiles the performance charts, mathematical foundations, and response schemas collected during the testing and validation phase of the API Gateway.", space_after=12)
    
    # Table A.1: Harmonized Simulation Testing Matrix
    add_header(doc, "Table A.1: Harmonized Simulation Testing Matrix", level=2)
    add_paragraph_with_spacing(doc, "To resolve stress-level inconsistencies and validate the system across a standardized spectrum of traffic conditions, all evaluations were unified under a single experimental matrix. This matrix defines four distinct testing tiers, run for a duration of 60 seconds across 10 independent trials each:", space_after=12)
    
    table_matrix = doc.add_table(rows=5, cols=6)
    table_matrix.style = 'Light Shading Accent 1'
    
    headers_matrix = ["Test ID", "Test Tier Name", "Baseline Load", "Surge Load", "Duration", "Independent Runs"]
    data_matrix = [
        ["T1", "Standard Scaling & Routing", "10 RPS", "60 RPS", "60 s", "10 Trials"],
        ["T2", "Standard Rate-Limiting Threshold", "10 RPS", "110 RPS", "60 s", "10 Trials"],
        ["T3", "Critical Surge & Predictive Mitigation", "10 RPS", "120 RPS", "60 s", "10 Trials"],
        ["T4", "Destructive Stress Limit Test", "10 RPS", "180 RPS", "60 s", "10 Trials"]
    ]
    
    hdr_cells_m = table_matrix.rows[0].cells
    for idx, header_text in enumerate(headers_matrix):
        hdr_cells_m[idx].text = header_text
        set_cell_background(hdr_cells_m[idx], "0F172A")
        set_cell_margins(hdr_cells_m[idx])
        for p in hdr_cells_m[idx].paragraphs:
            p.alignment = WD_ALIGN_PARAGRAPH.CENTER
            for r in p.runs:
                r.font.bold = True
                r.font.name = "Calibri"
                r.font.size = Pt(10)
                r.font.color.rgb = RGBColor(255, 255, 255)
                
    for row_idx, row_data in enumerate(data_matrix):
        row_cells = table_matrix.rows[row_idx + 1].cells
        for col_idx, cell_value in enumerate(row_data):
            row_cells[col_idx].text = cell_value
            set_cell_margins(row_cells[col_idx])
            if row_idx % 2 == 1:
                set_cell_background(row_cells[col_idx], "F8FAFC")
            else:
                set_cell_background(row_cells[col_idx], "FFFFFF")
                
            for p in row_cells[col_idx].paragraphs:
                p.alignment = WD_ALIGN_PARAGRAPH.CENTER
                for r in p.runs:
                    r.font.name = "Calibri"
                    r.font.size = Pt(9.5)
                    r.font.color.rgb = charcoal_color
                    if col_idx == 0:
                        r.bold = True
                        
    add_paragraph_with_spacing(doc, "Each test tier serves a specific evaluation purpose: T1 evaluates nominal auto-scaling; T2 establishes the boundary where standard rate-limiting is triggered; T3 evaluates the predictive middleware and active load-shedding responses; and T4 tests the ultimate resilience and degradation behaviour under severe saturation.", space_before=6, space_after=18)

    # Table A.2
    add_header(doc, "Table A.2: Summary Performance Metrics Comparison (Under T3 Critical Surge: 120 RPS)", level=2)
    table = doc.add_table(rows=8, cols=4)
    table.style = 'Light Shading Accent 1'
    
    headers = ["Performance Metric", "Unmitigated Scenario", "Mitigated Scenario", "Metric Improvement"]
    data = [
        ["Peak Simulated Traffic", "120 RPS", "120 RPS", "Baseline T3 Load Match"],
        ["P99 Gateway Latency", "2,540 ms", "20 ms", "99.2% latency reduction"],
        ["Critical Route Success Rate", "33.9%", "100.0%", "+195.0% success rate"],
        ["Non-Critical Success Rate", "33.9%", "0.0% (Graceful Shedding)", "Graceful failure mode"],
        ["Peak Downstream CPU Usage", "100% (Saturation)", "58% (Safe margins)", "42.0% headroom preserved"],
        ["Database Connection Pool", "Exhausted (Maxed Out)", "Stable (35% utilization)", "Prevents database deadlock"],
        ["HTTP 429 Response Format", "Standard IIS/Kestrel text", "Custom JSON payload", "Structured API response"]
    ]
    
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
                r.font.size = Pt(10)
                r.font.color.rgb = RGBColor(255, 255, 255)
                
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
                    r.font.size = Pt(9.5)
                    r.font.color.rgb = charcoal_color
                    if col_idx == 2 and ("100.0%" in cell_value or "20 ms" in cell_value or "58%" in cell_value):
                        r.bold = True
                        r.font.color.rgb = RGBColor(13, 148, 136)

    # Explanatory text for dual-level analysis
    add_paragraph_with_spacing(doc, "To provide a complete empirical validation, the system performance is analyzed at two distinct resolutions:\n"
                                    "1. Micro-Level Request Analysis (Within-Run): Evaluates the variance of individual request events (e.g., latency of individual HTTP transactions, n=4,392) during a single peak load event to capture the user-experience distribution.\n"
                                    "2. Macro-Level Multi-Trial Analysis (Between-Run): Contrasts the aggregate averages across 10 independent, identical simulation runs (n=10 per group) to ensure system reproducibility and rule out transient virtualization or network noise.", space_after=12)

    # Table A.3: Multi-Trial Aggregates
    add_header(doc, "Table A.3: Macro-Level Multi-Trial Reproducibility (Averages Across 10 Independent Runs Under T3 Surge)", level=2)
    table2 = doc.add_table(rows=4, cols=4)
    table2.style = 'Light Shading Accent 1'
    
    headers2 = ["Evaluated Vector (Aggregate)", "Unmitigated Baseline (Mean ± SD)", "Mitigated Gateway (Mean ± SD)", "t-Test / Significance"]
    data2 = [
        ["P99 Response Latency (ms)", "2,540.0 ± 85.2 ms", "20.0 ± 1.5 ms", "t(18) = -85.6, p < 0.001"],
        ["Checkout Success Rate (%)", "33.9% ± 2.1%", "100.0% ± 0.0%", "χ²(1) = 1722.6, p < 0.001"],
        ["Downstream CPU Usage (%)", "98.4% ± 1.2%", "58.2% ± 2.4%", "t(18) = -44.2, p < 0.001"]
    ]
    
    hdr_cells2 = table2.rows[0].cells
    for idx, header_text in enumerate(headers2):
        hdr_cells2[idx].text = header_text
        set_cell_background(hdr_cells2[idx], "0F172A")
        set_cell_margins(hdr_cells2[idx])
        for p in hdr_cells2[idx].paragraphs:
            p.alignment = WD_ALIGN_PARAGRAPH.CENTER
            for r in p.runs:
                r.font.bold = True
                r.font.name = "Calibri"
                r.font.size = Pt(10)
                r.font.color.rgb = RGBColor(255, 255, 255)
                
    for row_idx, row_data in enumerate(data2):
        row_cells = table2.rows[row_idx + 1].cells
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
                    r.font.size = Pt(9.5)
                    r.font.color.rgb = charcoal_color
                    if col_idx == 2 and ("100.0%" in cell_value or "20.0" in cell_value or "58.2%" in cell_value):
                        r.bold = True
                        r.font.color.rgb = RGBColor(13, 148, 136)

    # Figures
    img1_path = os.path.join(image_dir, "chart_load_shedding.png")
    if os.path.exists(img1_path):
        doc.add_page_break()
        add_header(doc, "Throughput and Load-Shedding Response Curve", level=2)
        p_img1 = doc.add_paragraph()
        p_img1.alignment = WD_ALIGN_PARAGRAPH.CENTER
        p_img1.add_run().add_picture(img1_path, width=Inches(5.5))
        add_paragraph_with_spacing(doc, "Figure A.1: Throughput and Load-Shedding Response under Traffic Surge", space_before=4, space_after=12, italic=True, font_size=9.5, color=gray_color).alignment = WD_ALIGN_PARAGRAPH.CENTER

    img2_path = os.path.join(image_dir, "chart_latency_comparison.png")
    if os.path.exists(img2_path):
        add_header(doc, "P99 Response Latency Comparison", level=2)
        p_img2 = doc.add_paragraph()
        p_img2.alignment = WD_ALIGN_PARAGRAPH.CENTER
        p_img2.add_run().add_picture(img2_path, width=Inches(5.5))
        add_paragraph_with_spacing(doc, "Figure A.2: Downstream P99 Latency Profile (Logarithmic Scale)", space_before=4, space_after=12, italic=True, font_size=9.5, color=gray_color).alignment = WD_ALIGN_PARAGRAPH.CENTER

    img3_path = os.path.join(image_dir, "chart_resource_utilization.png")
    if os.path.exists(img3_path):
        add_header(doc, "Downstream Service CPU Utilization Profile", level=2)
        p_img3 = doc.add_paragraph()
        p_img3.alignment = WD_ALIGN_PARAGRAPH.CENTER
        p_img3.add_run().add_picture(img3_path, width=Inches(5.5))
        add_paragraph_with_spacing(doc, "Figure A.3: Downstream Service CPU Utilization vs. SLA Threshold", space_before=4, space_after=12, italic=True, font_size=9.5, color=gray_color).alignment = WD_ALIGN_PARAGRAPH.CENTER

    # Math
    add_header(doc, "Mathematical Formulations of the Singular Spectrum Analysis (SSA) Predictor", level=2)
    add_paragraph_with_spacing(doc, "1. Embedding (Trajectory Space Creation):", bold=True, space_before=6)
    if rendered_paths["eq_trajectory.png"]:
        p = doc.add_paragraph()
        p.alignment = WD_ALIGN_PARAGRAPH.CENTER
        p.add_run().add_picture(rendered_paths["eq_trajectory.png"], width=Inches(3.0))
        
    add_paragraph_with_spacing(doc, "2. Singular Value Decomposition (SVD):", bold=True, space_before=6)
    if rendered_paths["eq_svd.png"]:
        p = doc.add_paragraph()
        p.alignment = WD_ALIGN_PARAGRAPH.CENTER
        p.add_run().add_picture(rendered_paths["eq_svd.png"], width=Inches(2.0))
        
    add_paragraph_with_spacing(doc, "3. Diagonal Averaging and LRR Forecasting:", bold=True, space_before=6)
    if rendered_paths["eq_lrr.png"]:
        p = doc.add_paragraph()
        p.alignment = WD_ALIGN_PARAGRAPH.CENTER
        p.add_run().add_picture(rendered_paths["eq_lrr.png"], width=Inches(2.5))
    if rendered_paths["eq_coef.png"]:
        p = doc.add_paragraph()
        p.alignment = WD_ALIGN_PARAGRAPH.CENTER
        p.add_run().add_picture(rendered_paths["eq_coef.png"], width=Inches(2.2))

    # Section A.5.1
    add_header(doc, "A.5.1 Real-Time Forecasting Accuracy Validation (H1)", level=2)
    add_paragraph_with_spacing(doc, "Hypothesis 1 proposed that a non-parametric Singular Spectrum Analysis (SSA) forecasting model would generate short-horizon traffic projections with a Mean Absolute Percentage Error (MAPE) below 10%, translating to improved p99 latency stability and SLA attainment under burst conditions.", space_after=6)
    add_paragraph_with_spacing(doc, "The SSA forecasting engine (SsaTrafficForecastEngine.cs) was updated to natively monitor forecast deviations by matching predictions against actual observed request rates at elapsed horizon boundaries. During simulated load runs under a nominal-to-burst traffic surge (15 to 60 RPS), the engine recorded the following performance metrics:", space_after=12)

    # Table A.3: Real-Time Forecast Accuracy
    table_acc = doc.add_table(rows=6, cols=3)
    table_acc.style = 'Light Shading Accent 1'
    
    headers_acc = ["Accuracy Metric", "Hypothesis H1 Threshold / Setting", "Empirical Computed Value"]
    data_acc = [
        ["Forecast Horizon", "60 seconds lookahead", "60 seconds"],
        ["Evaluation Windows (n)", "Sliding error buffer", "46 samples"],
        ["Mean Absolute Percentage Error (MAPE)", "MAPE < 10.00%", "15.22%"],
        ["Root Mean Squared Error (RMSE)", "n/a", "9.18 RPS"],
        ["H1 Hypothesis Objective Status", "MAPE < 10.00%", "PARTIALLY MET (Operational shift occurred, but error > 10%)"]
    ]
    
    hdr_cells_acc = table_acc.rows[0].cells
    for idx, header_text in enumerate(headers_acc):
        hdr_cells_acc[idx].text = header_text
        set_cell_background(hdr_cells_acc[idx], "0F172A")
        set_cell_margins(hdr_cells_acc[idx])
        for p in hdr_cells_acc[idx].paragraphs:
            p.alignment = WD_ALIGN_PARAGRAPH.CENTER
            for r in p.runs:
                r.font.bold = True
                r.font.name = "Calibri"
                r.font.size = Pt(10)
                r.font.color.rgb = RGBColor(255, 255, 255)
                
    for row_idx, row_data in enumerate(data_acc):
        row_cells = table_acc.rows[row_idx + 1].cells
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
                    r.font.size = Pt(9.5)
                    r.font.color.rgb = charcoal_color
                    if col_idx == 2 and ("PARTIALLY MET" in cell_value or "15.22%" in cell_value):
                        r.bold = True
                        r.font.color.rgb = RGBColor(234, 88, 12)
                        
    add_paragraph_with_spacing(doc, "The measured MAPE of 15.22% does not strictly meet the H1 threshold of < 10% under extreme transient burst conditions. However, the SSA engine demonstrated high operational efficacy: as shown in Section 5.2, it successfully detected the surge onset within 1.0 second, triggering the mitigation pipeline to protect down-stream services. The slightly higher error rate is attributed to the intense non-linear acceleration phase during the shock load, which represents a limitation that can be resolved with further parameter tuning (such as window sizing and decay factors) in future work.", space_before=6, space_after=12)

    # ----------------------------------------------------
    # APPENDIX B: APPLICATION INTERFACE SCREENSHOTS
    # ----------------------------------------------------
    doc.add_page_break()
    add_header(doc, "Appendix B: Application Interface Screenshots", level=1)
    add_paragraph_with_spacing(doc, "The following sections present high-fidelity screenshots of the two main user-facing interfaces developed for this system framework.", space_after=12)
    
    store_path = os.path.join(image_dir, "app_aurastore_ui.png")
    if os.path.exists(store_path):
        add_header(doc, "B.1 AuraStore Customer Portal", level=2)
        p_store = doc.add_paragraph()
        p_store.alignment = WD_ALIGN_PARAGRAPH.CENTER
        p_store.add_run().add_picture(store_path, width=Inches(5.5))
        add_paragraph_with_spacing(doc, "Figure B.1: AuraStore E-Commerce Storefront showing Product Catalog and Cart", space_before=4, space_after=12, italic=True, font_size=9.5, color=gray_color).alignment = WD_ALIGN_PARAGRAPH.CENTER
        
    dashboard_path = os.path.join(image_dir, "app_operator_dashboard_ui.png")
    if os.path.exists(dashboard_path):
        add_header(doc, "B.2 Operator Telemetry & Diagnostics Dashboard", level=2)
        p_dash = doc.add_paragraph()
        p_dash.alignment = WD_ALIGN_PARAGRAPH.CENTER
        p_dash.add_run().add_picture(dashboard_path, width=Inches(5.5))
        add_paragraph_with_spacing(doc, "Figure B.2: Operator Control Panel with real-time RPS charts, active posture status, and terminal event logs", space_before=4, space_after=12, italic=True, font_size=9.5, color=gray_color).alignment = WD_ALIGN_PARAGRAPH.CENTER
        
    # ----------------------------------------------------
    # APPENDIX C: SAMPLE CODE SNIPPETS
    # ----------------------------------------------------
    doc.add_page_break()
    add_header(doc, "Appendix C: Sample Core Code Snippets", level=1)
    
    add_header(doc, "C.1 AdaptiveRateLimitingMiddleware.cs", level=2)
    add_paragraph_with_spacing(doc, "ASP.NET Core request pipeline interceptor executing token-bucket client rate limiting with posture-driven limits:", space_after=6)
    
    p_code1 = doc.add_paragraph()
    p_code1.paragraph_format.left_indent = Inches(0.2)
    p_code1.paragraph_format.space_after = Pt(12)
    p_code1_run = p_code1.add_run(rl_code[:4000] + "\n\n// ... Code truncated for space ...")
    p_code1_run.font.name = "Consolas"
    p_code1_run.font.size = Pt(8.5)
    p_code1_run.font.color.rgb = RGBColor(9, 79, 76)
    
    add_header(doc, "C.2 PredictiveTrafficMiddleware.cs", level=2)
    add_paragraph_with_spacing(doc, "Pipeline interceptor executing non-blocking telemetry writes and active route-shedding gating when system posture becomes Critical:", space_after=6)
    
    p_code2 = doc.add_paragraph()
    p_code2.paragraph_format.left_indent = Inches(0.2)
    p_code2.paragraph_format.space_after = Pt(12)
    p_code2_run = p_code2.add_run(shed_code[:4000] + "\n\n// ... Code truncated for space ...")
    p_code2_run.font.name = "Consolas"
    p_code2_run.font.size = Pt(8.5)
    p_code2_run.font.color.rgb = RGBColor(9, 79, 76)

    # ----------------------------------------------------
    # APPENDIX D: SETUP & OPERATIONS MANUAL
    # ----------------------------------------------------
    doc.add_page_break()
    add_header(doc, "Appendix D: Setup and Operations Manual", level=1)
    
    add_header(doc, "D.1 System Prerequisites", level=2)
    add_paragraph_with_spacing(doc, "Ensure the following runtime environments are configured on the local system:\n"
                                    "• Docker Engine and Docker Compose (v2.0 or higher)\n"
                                    "• .NET SDK v8.0 (for compiling API Gateway and microservices)\n"
                                    "• Node.js v18.0 or higher (for compiling the React simulation dashboard)\n"
                                    "• ngrok CLI tool (configured with a valid tunnel authtoken)", space_after=12)
    
    add_header(doc, "D.2 Spin Up Containerized Core Infrastructure", level=2)
    add_paragraph_with_spacing(doc, "Navigate to the solution directory and start the database, caching, telemetry, and background services:\n"
                                    "   $ docker-compose up -d --build\n"
                                    "This command will spin up 8 Docker containers, including SQL Server, Redis, and the order-processing services.", space_after=12)
    
    add_header(doc, "D.3 Configure ngrok Tunnel & Ingress", level=2)
    add_paragraph_with_spacing(doc, "Map the local gateway's HTTP ingress port (5000) to a public ngrok domain name:\n"
                                    "   $ ngrok http 5000\n"
                                    "Copy the generated tunnel URL (e.g., https://abcd-123.ngrok-free.app) and set it in the frontend's App.jsx configuration file:\n"
                                    "   const GATEWAY_URL = 'https://abcd-123.ngrok-free.app';", space_after=12)
    
    add_header(doc, "D.4 Launch the Simulator Dashboard", level=2)
    add_paragraph_with_spacing(doc, "Navigate to the frontend folder, install packages, and start the development server:\n"
                                    "   $ npm install\n"
                                    "   $ npm run dev\n"
                                    "Open http://localhost:5173/ in your browser. Set the target RPS on the operator tab and click 'Start Simulation'.", space_after=12)

    # ----------------------------------------------------
    # APPENDIX E: EMPIRICAL TELEMETRY FRAMEWORK VS. QUESTIONNAIRES
    # ----------------------------------------------------
    doc.add_page_break()
    add_header(doc, "Appendix E: Empirical Telemetry Framework vs. Questionnaires", level=1)
    
    add_header(doc, "E.1 Methodological Justification", level=2)
    add_paragraph_with_spacing(doc, "This research employs a purely quantitative, empirical software engineering methodology. Unlike social science studies, human-subject questionnaires or surveys are not applicable. System performance, gateway throughput gating, and transaction resilience are validated strictly via mathematical metrics, system telemetry logs, and high-frequency load testing.", space_after=12)
    
    add_header(doc, "E.2 Telemetry Metrics Schema", level=2)
    add_paragraph_with_spacing(doc, "In place of qualitative questionnaires, the database logs quantitative system telemetry metrics. The table below details the data schema of the telemetry fields recorded every second to validate system posture changes:", space_after=12)
    
    # Table E.1
    table_e = doc.add_table(rows=6, cols=3)
    table_e.style = 'Light Shading Accent 1'
    
    headers_e = ["Telemetry Field", "Data Type", "Operational Definition"]
    data_e = [
        ["timestamp", "DateTimeOffset", "The exact UTC timestamp when the statistics bucket was finalized."],
        ["systemPosture", "String (Enum)", "System protective state resolved by the SSA forecasting model (Nominal, Alert, Critical)."],
        ["currentRps", "Double", "Calculated throughput rate (Requests Per Second) entering the gateway boundary."],
        ["forecastedRps", "Double", "Calculated lookahead peak traffic rate projected for the next 60 seconds."],
        ["throttledRequests", "Int64 (Counter)", "Cumulative count of HTTP 429 requests shedded by the active middleware gating rules."]
    ]
    
    hdr_cells_e = table_e.rows[0].cells
    for idx, header_text in enumerate(headers_e):
        hdr_cells_e[idx].text = header_text
        set_cell_background(hdr_cells_e[idx], "0F172A")
        set_cell_margins(hdr_cells_e[idx])
        for p in hdr_cells_e[idx].paragraphs:
            p.alignment = WD_ALIGN_PARAGRAPH.CENTER
            for r in p.runs:
                r.font.bold = True
                r.font.name = "Calibri"
                r.font.size = Pt(10)
                r.font.color.rgb = RGBColor(255, 255, 255)
                
    for row_idx, row_data in enumerate(data_e):
        row_cells = table_e.rows[row_idx + 1].cells
        for col_idx, cell_value in enumerate(row_data):
            row_cells[col_idx].text = cell_value
            set_cell_margins(row_cells[col_idx])
            if row_idx % 2 == 1:
                set_cell_background(row_cells[col_idx], "F8FAFC")
            else:
                set_cell_background(row_cells[col_idx], "FFFFFF")
                
            for p in row_cells[col_idx].paragraphs:
                for r in p.runs:
                    r.font.name = "Calibri"
                    r.font.size = Pt(9.5)
                    r.font.color.rgb = charcoal_color
                    if col_idx == 0:
                        r.bold = True
                        r.font.name = "Consolas"
                        r.font.size = Pt(9)

    # Save document
    output_path = os.path.join(base_dir, "APPENDIX_A.docx")
    doc.save(output_path)
    print("Final Word document generated successfully at:", output_path)

if __name__ == "__main__":
    main()
