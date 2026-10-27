-- Read-only verification of batch DUMMY-SEED:20261022-01. Run: python3 run_seed.py verify
-- (seed.sql also \i's this file inside the dry-run transaction, just before ROLLBACK.)
-- Rows that carry no DUMMY- marker (office, cycle, calendar, proposal, demand_line) are identified through
-- their INSERT row in budget.audit_log, whose action_code is the batch id.
\pset pager off
\echo '=== V1: audit_log rows written by the batch triggers, per table and operation'
SELECT table_name, operation, count(*) AS rows
FROM budget.audit_log WHERE action_code = 'DUMMY-SEED:20261022-01'
GROUP BY 1, 2 ORDER BY 1, 2;

\echo '=== V2: rows inserted by the batch, per table'
WITH ins AS (SELECT table_name, record_key FROM budget.audit_log
             WHERE action_code = 'DUMMY-SEED:20261022-01' AND operation = 'I')
SELECT 'office' AS table_name, count(*) AS dummy_rows FROM budget.office o
  WHERE EXISTS (SELECT 1 FROM ins WHERE ins.table_name = 'office' AND ins.record_key = o.office_id::text)
UNION ALL SELECT 'delegation_limit', count(*) FROM budget.delegation_limit WHERE source_ref LIKE 'DUMMY-SEED:20261022-01%'
UNION ALL SELECT 'fa_financial_years (2027, 2028 present; untagged)', count(*) FROM budget.fa_financial_years WHERE fin_year_closing IN (2027, 2028)
UNION ALL SELECT 'budget_cycle', count(*) FROM budget.budget_cycle c
  WHERE EXISTS (SELECT 1 FROM ins WHERE ins.table_name = 'budget_cycle' AND ins.record_key = c.cycle_id::text)
UNION ALL SELECT 'calendar', count(*) FROM budget.calendar k JOIN budget.budget_cycle c ON c.cycle_id = k.cycle_id
  WHERE EXISTS (SELECT 1 FROM ins WHERE ins.table_name = 'budget_cycle' AND ins.record_key = c.cycle_id::text)
UNION ALL SELECT 'attachment', count(*) FROM budget.attachment WHERE scan_engine = 'DUMMY-SEED: no file stored, not scanned'
UNION ALL SELECT 'work', count(*) FROM budget.work WHERE work_desc LIKE 'DUMMY-%' AND remarks = 'DUMMY-SEED:20261022-01'
UNION ALL SELECT 'admin_approval', count(*) FROM budget.admin_approval a JOIN budget.work w ON w.id = a.work_id
  WHERE w.remarks = 'DUMMY-SEED:20261022-01'
UNION ALL SELECT 'admin_approval_version', count(*) FROM budget.admin_approval_version WHERE aa_reference_no LIKE 'DUMMY-%'
UNION ALL SELECT 'technical_sanction', count(*) FROM budget.technical_sanction t JOIN budget.work w ON w.id = t.work_id
  WHERE w.remarks = 'DUMMY-SEED:20261022-01'
UNION ALL SELECT 'technical_sanction_version', count(*) FROM budget.technical_sanction_version WHERE ts_number LIKE 'DUMMY-%'
UNION ALL SELECT 'estimate (skipped by design: expect 0)', count(*) FROM budget.estimate WHERE estimate_code LIKE 'DUMMY-%'
UNION ALL SELECT 'budget_table type 1 (office expense)', count(*) FROM budget.budget_table WHERE budget_description LIKE 'DUMMY-%' AND budget_type = 1 AND calendar_year = 2027
UNION ALL SELECT 'budget_table type 2 (receipt)', count(*) FROM budget.budget_table WHERE budget_description LIKE 'DUMMY-%' AND budget_type = 2 AND calendar_year = 2027
UNION ALL SELECT 'proposal', count(*) FROM budget.proposal p
  WHERE EXISTS (SELECT 1 FROM ins WHERE ins.table_name = 'proposal' AND ins.record_key = p.proposal_id::text)
UNION ALL SELECT 'demand_line', count(*) FROM budget.demand_line d
  WHERE EXISTS (SELECT 1 FROM ins WHERE ins.table_name = 'demand_line' AND ins.record_key = d.demand_line_id::text)
UNION ALL SELECT 'figure_version (trigger-written)', count(*) FROM budget.figure_version WHERE justification = 'DUMMY-SEED:20261022-01';

\echo '=== V3: works by entry route'
SELECT work_class, entered_via, user_code, count(*), count(*) FILTER (WHERE work_code IS NULL) AS without_code
FROM budget.work WHERE remarks = 'DUMMY-SEED:20261022-01' GROUP BY 1, 2, 3 ORDER BY 1, 2;

\echo '=== V4: office structure (all active offices, by level)'
SELECT office_level, count(*) FROM budget.office WHERE is_active GROUP BY 1 ORDER BY 1;

\echo '=== V5: approvals: current version = highest version; revisions; delegation flag'
SELECT left(v.name_of_work, 12) AS aa, a.serial_no, a.current_version_no,
       (SELECT max(version_no) FROM budget.admin_approval_version x WHERE x.admin_approval_id = a.admin_approval_id) AS max_version,
       v.amount_sanctioned, v.competent_authority, v.exceeds_delegation
FROM budget.admin_approval a
JOIN budget.admin_approval_version v ON v.admin_approval_id = a.admin_approval_id AND v.version_no = a.current_version_no
JOIN budget.work w ON w.id = a.work_id
WHERE w.remarks = 'DUMMY-SEED:20261022-01' ORDER BY 1;

\echo '=== V6: technical sanctions as % of the current A&A (ceiling 105)'
SELECT v.ts_number, t.tab, v.ts_amount, av.amount_sanctioned AS aa_current,
       round(100.0 * v.ts_amount / nullif(av.amount_sanctioned, 0), 1) AS pct_of_aa
FROM budget.technical_sanction t
JOIN budget.technical_sanction_version v ON v.technical_sanction_id = t.technical_sanction_id AND v.version_no = t.current_version_no
LEFT JOIN budget.admin_approval a ON a.admin_approval_id = t.admin_approval_id
LEFT JOIN budget.admin_approval_version av ON av.admin_approval_id = a.admin_approval_id AND av.version_no = a.current_version_no
WHERE v.ts_number LIKE 'DUMMY-%' ORDER BY 1;

\echo '=== V7: proposals and lines'
SELECT p.proposal_id, p.category, p.state_code, (p.holder_office_id = p.origin_office_id) AS held_by_origin,
       count(d.*) AS lines, count(d.*) FILTER (WHERE d.floor_check_unchecked) AS floor_unchecked,
       (SELECT count(*) FROM budget.figure_version f JOIN budget.demand_line dd ON dd.demand_line_id = f.demand_line_id
        WHERE dd.proposal_id = p.proposal_id) AS figure_versions
FROM budget.proposal p LEFT JOIN budget.demand_line d ON d.proposal_id = p.proposal_id
WHERE EXISTS (SELECT 1 FROM budget.audit_log a WHERE a.table_name = 'proposal' AND a.operation = 'I'
              AND a.record_key = p.proposal_id::text AND a.action_code = 'DUMMY-SEED:20261022-01')
GROUP BY p.proposal_id ORDER BY p.proposal_id;

\echo '=== V8: precondition - exactly one OPEN budget_cycle'
SELECT cycle_id, current_fin_year, next_fin_year, is_first_cycle, status FROM budget.budget_cycle WHERE status = 'OPEN';
