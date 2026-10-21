### Master Prompt: Generate and Manage `docs/ISSUE_TRACKER.md`

Use the prompt below with your AI assistant (Cursor with `@Codebase`, VS Code Copilot with `@workspace`, Claude, or ChatGPT) to automatically generate the issue tracker file and establish the state-driven remediation workflow.

````
Role: Principal DevOps & Software Quality Lead
Context File: `docs/CODE_AUDIT_AND_HEALTH_REPORT.md`
Target Output File: `docs/ISSUE_TRACKER.md`

====================================================================
PART 1: INITIAL GENERATION TASK (CREATE & POPULATE ISSUE TRACKER)
====================================================================
Read and parse the entire audit report at `docs/CODE_AUDIT_AND_HEALTH_REPORT.md`.
Generate and save a structured remediation tracking file at:
`docs/ISSUE_TRACKER.md`

Structure requirements for `docs/ISSUE_TRACKER.md`:
1. Metadata Header:
   - System Name: iM3Helpdesk (DeskMate)
   - Baseline Health Score: 3.3 / 10 (From Audit Report)
   - Last Updated: [Current Date / Time]
   - Status Legend:
     * `[TODO]`       : Pending investigation and implementation
     * `[IN_PROGRESS]`: Currently being modified, coded, or verified
     * `[DONE]`       : Code written, reviewed, tested, and committed
     * `[BACKLOG]`    : Deferred or low-impact enhancement

2. Categorized Issue Ledger:
   Extract EVERY finding across all 8 agents from `docs/CODE_AUDIT_AND_HEALTH_REPORT.md` and assign each issue a deterministic, prefixed ID:
   - `SEC-xx`  : Security & Vulnerabilities (Agent 2)
   - `CONC-xx` : Concurrency & Scalability Risks (Agent 3)
   - `ARCH-xx` : Architecture & SOLID Violations (Agents 4 & 5)
   - `PERF-xx` : Optimization, Algorithms & Leaks (Agent 7)
   - `CLN-xx`  : Structure, Dead Code & Hygiene (Agents 1 & 6)

3. Issue Entry Format:
   For every single issue, format it as a markdown task item:
   ```markdown
   ### [TODO] {ISSUE_ID}: {Issue Title}
   - **Severity:** [CRITICAL | HIGH | MEDIUM | LOW]
   - **Agent Source:** [e.g., Agent 2: Security Auditor]
   - **Target File(s):** `{relative/path/to/file.ext}`
   - **Problem Summary:** Concise description of the defect or vulnerability.
   - **Acceptance Criteria:** What exact conditions must be met for this issue to be marked [DONE].
   - **Resolution Notes:** [Unassigned]


````

4. Summary Dashboard at Top of File: Include a dynamic tally table:

   | Category | Total | TODO | IN_PROGRESS | DONE | BACKLOG | 
   | ----- | ----- | ----- | ----- | ----- | ----- | 
   | Security (SEC) | x | x | 0 | 0 | 0 | 
   | Concurrency (CONC) | x | x | 0 | 0 | 0 | 
   | Architecture (ARCH) | x | x | 0 | 0 | 0 | 
   | Optimization (PERF) | x | x | 0 | 0 | 0 | 
   | Hygiene & Cleanup (CLN) | x | x | 0 | 0 | 0 | 
   | **Total** | **X** | **X** | **0** | **0** | **0** | 

# ==================================================================== PART 2: INTERACTIVE OPERATIONAL PROTOCOL (WORKFLOW COMMANDS)

Once `docs/ISSUE_TRACKER.md` has been generated, adhere strictly to the following commands during all subsequent interactions:

1. When I ask: "Show me pending issues" or "What is next?":

   * Inspect `docs/ISSUE_TRACKER.md`.

   * Display a concise list of all `[TODO]` items, ordered strictly by severity:

     1. CRITICAL

     2. HIGH

     3. MEDIUM

     4. LOW

   * Recommend the next highest-priority issue to address immediately.

2. When I state: "Solve " (e.g., "Solve SEC-01"):

   * Locate `<ISSUE-ID>` in `docs/ISSUE_TRACKER.md` and immediately update its tag to `[IN_PROGRESS]`.

   * Re-read the target file(s) in the workspace.

   * Provide the complete, production-grade, compilable code fix (C# or TypeScript).

   * Provide explicit, step-by-step verification instructions (e.g., test curl, unit test, or manual UI test).

   * DO NOT mark it as `[DONE]` yet. Wait for my confirmation.

3. When I state: "Mark  as done" (or confirm that the fix works):

   * Update the status tag in `docs/ISSUE_TRACKER.md` from `[IN_PROGRESS]` to `[DONE]`.

   * Append a bullet to the issue entry: `- **Resolution Notes:** Resolved on [Date] - [Brief 1-line note of what was changed].`

   * Recalculate and update the Summary Dashboard counts at the top of `docs/ISSUE_TRACKER.md`.

   * Present the next top priority pending issue ready to be solved.

4. When I state: "Move  to backlog":

   * Update its status