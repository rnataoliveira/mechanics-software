"""
Parses `dotnet list package --vulnerable --include-transitive` output
and generates a structured HTML security report with findings table
and prioritized action plan.
"""
import re
import html
import datetime
import pathlib
import sys

RAW_PATH = pathlib.Path("vuln-output.txt")
OUT_PATH  = pathlib.Path("coverage-html/security/index.html")

SEVERITY_ORDER = {"Critical": 0, "High": 1, "Moderate": 2, "Low": 3}
SEVERITY_COLOR = {
    "Critical": "#d73a49",
    "High":     "#e36209",
    "Moderate": "#b08800",
    "Low":      "#0969da",
}
SEVERITY_BG = {
    "Critical": "#fff0f0",
    "High":     "#fff4ec",
    "Moderate": "#fffbdd",
    "Low":      "#f1f8ff",
}


def parse_findings(raw: str) -> list[dict]:
    findings = []
    current_project = None
    current_type = None

    for line in raw.splitlines():
        m = re.match(r"Project `(.+?)` has the following vulnerable packages", line.strip())
        if m:
            current_project = m.group(1)
            current_type = None
            continue

        if "Top-level Package" in line:
            current_type = "Direct"
            continue
        if "Transitive Package" in line:
            current_type = "Transitive"
            continue

        m = re.match(r"\s*>\s+(\S+)\s+(.+)", line)
        if m and current_project and current_type:
            pkg = m.group(1)
            parts = m.group(2).split()
            if len(parts) >= 2:
                advisory = parts[-1]
                severity = parts[-2]
                if severity in SEVERITY_ORDER:
                    version = parts[-3] if len(parts) >= 3 else "?"
                    findings.append({
                        "project":  current_project,
                        "package":  pkg,
                        "type":     current_type,
                        "version":  version,
                        "severity": severity,
                        "advisory": advisory,
                    })

    findings.sort(key=lambda f: SEVERITY_ORDER.get(f["severity"], 99))
    return findings


def sev_badge(sev: str, n: int) -> str:
    if n == 0:
        return f'<span style="color:#6a737d;font-weight:700">{n}</span>'
    color = SEVERITY_COLOR.get(sev, "#6a737d")
    return f'<span style="background:{color};color:#fff;padding:2px 10px;border-radius:10px;font-size:.8rem;font-weight:600">{n}</span>'


def type_chip(t: str) -> str:
    if t == "Direct":
        return '<span style="background:#d4e8ff;color:#0969da;padding:1px 7px;border-radius:8px;font-size:.75rem;font-weight:600">Direct</span>'
    return '<span style="background:#f0f0f0;color:#555;padding:1px 7px;border-radius:8px;font-size:.75rem">Transitive</span>'


def finding_row(f: dict) -> str:
    sev  = f["severity"]
    bg   = SEVERITY_BG.get(sev, "#fff")
    color = SEVERITY_COLOR.get(sev, "#333")
    host = re.sub(r"https?://([^/]+).*", r"\1", f["advisory"])
    return (
        f'<tr style="background:{bg}">'
        f'<td><strong>{html.escape(f["package"])}</strong></td>'
        f'<td style="color:#555">{html.escape(f["project"])}</td>'
        f'<td>{type_chip(f["type"])}</td>'
        f'<td><code>{html.escape(f["version"])}</code></td>'
        f'<td><span style="color:{color};font-weight:700">{html.escape(sev)}</span></td>'
        f'<td><a href="{html.escape(f["advisory"])}" target="_blank" rel="noopener">'
        f'{html.escape(host)} ↗</a></td>'
        f'</tr>\n'
    )


def action_plan(findings: list[dict]) -> str:
    if not findings:
        return '<p style="color:#2ea44f;font-weight:600">No vulnerabilities detected. No action required.</p>'

    direct_critical = [f for f in findings if f["type"] == "Direct"     and f["severity"] in ("Critical", "High")]
    transitive_high = [f for f in findings if f["type"] == "Transitive" and f["severity"] in ("Critical", "High")]
    moderate        = [f for f in findings if f["severity"] == "Moderate"]
    low             = [f for f in findings if f["severity"] == "Low"]

    items = []

    if direct_critical:
        pkgs = ", ".join(f'<code>{html.escape(f["package"])}</code>' for f in direct_critical)
        items.append(
            f'<li><strong style="color:#d73a49">Immediate action —</strong> '
            f'Upgrade direct dependenc{"y" if len(direct_critical)==1 else "ies"} {pkgs} '
            f'to a patched version. Check the advisory for the minimum safe version. '
            f'Run <code>dotnet add package &lt;name&gt; --version &lt;fixed&gt;</code> for each.</li>'
        )

    if transitive_high:
        pkgs = ", ".join(f'<code>{html.escape(f["package"])}</code>' for f in transitive_high)
        items.append(
            f'<li><strong style="color:#e36209">High priority —</strong> '
            f'Transitive package{"s" if len(transitive_high)>1 else ""} {pkgs} '
            f'{"are" if len(transitive_high)>1 else "is"} flagged. '
            f'Run <code>dotnet list package --include-transitive</code> to trace which direct '
            f'dependency pulls {"them" if len(transitive_high)>1 else "it"} in, then upgrade '
            f'that dependency to a version that brings in a fixed transitive version.</li>'
        )

    if moderate:
        pkgs = ", ".join(f'<code>{html.escape(f["package"])}</code>' for f in moderate)
        items.append(
            f'<li><strong style="color:#b08800">Next sprint —</strong> '
            f'Schedule upgrade of {pkgs} (Moderate severity). '
            f'No immediate risk, but should not be deferred beyond the next maintenance cycle.</li>'
        )

    if low:
        pkgs = ", ".join(f'<code>{html.escape(f["package"])}</code>' for f in low)
        items.append(
            f'<li><strong style="color:#0969da">Maintenance window —</strong> '
            f'Include {pkgs} (Low severity) in the next periodic dependency update.</li>'
        )

    return '<ul style="line-height:2.2;padding-left:1.5rem;margin:0">' + "".join(items) + "</ul>"


def build_page(findings: list[dict]) -> str:
    counts = {s: 0 for s in SEVERITY_ORDER}
    for f in findings:
        if f["severity"] in counts:
            counts[f["severity"]] += 1

    total    = sum(counts.values())
    has_high = counts["Critical"] + counts["High"] > 0
    banner_bg   = "#d73a49" if has_high else ("#bf8700" if total > 0 else "#2ea44f")
    banner_text = (f'{total} vulnerabilit{"y" if total == 1 else "ies"} found' if total > 0 else "No vulnerabilities")
    generated = datetime.datetime.utcnow().strftime("%Y-%m-%d %H:%M UTC")

    findings_rows = (
        "".join(finding_row(f) for f in findings)
        if findings else
        '<tr><td colspan="6" style="text-align:center;color:#6a737d;padding:2rem">'
        'No vulnerable packages detected.</td></tr>'
    )

    return f"""<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width,initial-scale=1">
  <title>MechanicsSoftware — Vulnerability Report</title>
  <style>
    * {{ box-sizing: border-box }}
    body {{ font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif;
           margin: 0; background: #f6f8fa; color: #24292e }}
    header {{ background: #24292e; color: #fff; padding: 1rem 2rem; display: flex;
              align-items: center; gap: 1.5rem }}
    header a {{ color: #58a6ff; text-decoration: none; font-size: .9rem }}
    .banner {{ background: {banner_bg}; color: #fff; padding: .75rem 2rem;
               font-size: 1.1rem; font-weight: 700 }}
    main {{ max-width: 1020px; margin: 2rem auto; padding: 0 1rem }}
    h2 {{ font-size: 1rem; font-weight: 700; margin: 1.5rem 0 .5rem; color: #0d1117;
          border-bottom: 2px solid #e1e4e8; padding-bottom: 4px }}
    .summary-grid {{ display: flex; gap: 1rem; margin: 1rem 0; flex-wrap: wrap }}
    .scard {{ background: #fff; border: 1px solid #e1e4e8; border-radius: 8px;
              padding: .75rem 1.25rem; text-align: center; min-width: 85px }}
    .scard .num {{ font-size: 2rem; font-weight: 700; line-height: 1.2 }}
    .scard .lbl {{ font-size: .8rem; color: #6a737d }}
    .action-box {{ background: #fff; border: 1px solid #e1e4e8; border-radius: 6px;
                   padding: 1rem 1.5rem; margin: .5rem 0 }}
    table {{ width: 100%; border-collapse: collapse; background: #fff;
             border: 1px solid #e1e4e8; border-radius: 6px; overflow: hidden; margin: .5rem 0 }}
    th {{ background: #f6f8fa; text-align: left; padding: .5rem 1rem;
          border-bottom: 1px solid #e1e4e8; font-size: .85rem }}
    td {{ padding: .45rem 1rem; border-bottom: 1px solid #e1e4e8; font-size: .85rem }}
    tr:last-child td {{ border-bottom: none }}
    footer {{ text-align: center; font-size: .8rem; color: #6a737d; padding: 2rem }}
    code {{ background: #f3f4f6; padding: 1px 5px; border-radius: 3px; font-size: .85rem }}
    a {{ color: #0969da }}
  </style>
</head>
<body>
  <header>
    <strong>MechanicsSoftware</strong>
    <a href="../index.html">← Coverage Report</a>
  </header>
  <div class="banner">{banner_text}</div>
  <main>
    <p style="color:#6a737d;font-size:.85rem;margin:.5rem 0 1.5rem">
      Generated: {generated} &nbsp;·&nbsp;
      <code>dotnet list package --vulnerable --include-transitive</code>
    </p>

    <h2>Summary</h2>
    <div class="summary-grid">
      <div class="scard"><div class="num" style="color:#d73a49">{counts["Critical"]}</div><div class="lbl">Critical</div></div>
      <div class="scard"><div class="num" style="color:#e36209">{counts["High"]}</div><div class="lbl">High</div></div>
      <div class="scard"><div class="num" style="color:#b08800">{counts["Moderate"]}</div><div class="lbl">Moderate</div></div>
      <div class="scard"><div class="num" style="color:#0969da">{counts["Low"]}</div><div class="lbl">Low</div></div>
      <div class="scard"><div class="num">{total}</div><div class="lbl">Total</div></div>
    </div>

    <h2>Action Plan</h2>
    <div class="action-box">{action_plan(findings)}</div>

    <h2>Findings</h2>
    <table>
      <thead>
        <tr>
          <th>Package</th>
          <th>Project</th>
          <th>Type</th>
          <th>Resolved Version</th>
          <th>Severity</th>
          <th>Advisory</th>
        </tr>
      </thead>
      <tbody>{findings_rows}</tbody>
    </table>
  </main>
  <footer>MechanicsSoftware · FIAP POS Tech 15SOAT · Fase 2</footer>
</body>
</html>"""


if __name__ == "__main__":
    if not RAW_PATH.exists():
        print(f"ERROR: {RAW_PATH} not found", file=sys.stderr)
        sys.exit(1)

    OUT_PATH.parent.mkdir(parents=True, exist_ok=True)
    findings = parse_findings(RAW_PATH.read_text())
    OUT_PATH.write_text(build_page(findings))
    total = len(findings)
    print(f"Security report written: {total} finding{'s' if total != 1 else ''}.")
