-- #####################################################################################################
-- ##                                                                                                 ##
-- ##   WARNING: DOCUMENTATION ONLY. DO NOT EXECUTE. run_seed.py refuses to run this file.            ##
-- ##                                                                                                 ##
-- ##   Batch DUMMY-SEED:20261022-01 on the SHARED BBMB database (Dev + Staging).                      ##
-- ##                                                                                                 ##
-- ##   Every budget-process table carries a BEFORE DELETE OR TRUNCATE statement trigger              ##
-- ##   (budget.fn_forbid_change), and the version/attachment/limit tables also forbid UPDATE per row. ##
-- ##   The DELETEs below therefore FAIL as written. Removing these rows needs a superuser to bypass   ##
-- ##   the triggers (e.g. ALTER TABLE ... DISABLE TRIGGER or session_replication_role = replica),     ##
-- ##   which this project's rules forbid for the seed. That bypass is a deliberate, approved DBA      ##
-- ##   change outside this tool, with its own change record.                                         ##
-- ##                                                                                                 ##
-- ##   Even then, budget.audit_log keeps the INSERT history of every row (it is append-only by        ##
-- ##   design and is NOT listed for deletion here). Deleting the audited rows while keeping their     ##
-- ##   audit rows leaves audit entries that point at missing records. Decide that explicitly.         ##
-- ##                                                                                                 ##
-- ##   Alternative without a bypass (README "Neutralise instead of delete"): close the cycle, mark    ##
-- ##   the demand lines deleted. Everything else stays.                                               ##
-- ##                                                                                                 ##
-- #####################################################################################################
\echo 'cleanup.sql is documentation only and must not be executed'
\quit 1

-- Keys identifying the batch rows (from the 2026-10-22 real run; re-derive with verify.sql before use):
--   office_id 18-33 (FINANCE 18, SECRETARIAT 19, CE 20-21, CIRCLE 22-25, DIVISION 26-33)
--   budget_cycle 2; calendar (cycle 2, 5 levels); delegation_limit source_ref 'DUMMY-SEED:20261022-01%'
--   proposal 9-16; work remarks = 'DUMMY-SEED:20261022-01'; attachment scan_engine 'DUMMY-SEED: no file stored, not scanned'
--   fa_financial_years 2027 and 2028 are REAL calendar facts and are NOT listed for removal.

-- FK-reverse order (children first).
-- 1. figure_version (trigger-written, child of demand_line)
DELETE FROM budget.figure_version f USING budget.demand_line d
 WHERE f.demand_line_id = d.demand_line_id AND d.proposal_id BETWEEN 9 AND 16;
-- 2. demand_line
DELETE FROM budget.demand_line WHERE proposal_id BETWEEN 9 AND 16;
-- 3. proposal
DELETE FROM budget.proposal WHERE proposal_id BETWEEN 9 AND 16 AND cycle_id = 2;
-- 4. budget_table (legacy; no forbid trigger, but references work)
DELETE FROM budget.budget_table WHERE budget_description LIKE 'DUMMY-%' AND calendar_year = 2027 AND user_code = 'DUMMY';
-- 5. technical_sanction_version, technical_sanction
DELETE FROM budget.technical_sanction_version WHERE ts_number LIKE 'DUMMY-TS/2026-27/TS-0%';
DELETE FROM budget.technical_sanction t USING budget.work w
 WHERE w.id = t.work_id AND w.remarks = 'DUMMY-SEED:20261022-01';
-- 6. admin_approval_version, admin_approval
DELETE FROM budget.admin_approval_version WHERE aa_reference_no LIKE 'DUMMY-AA/2026-27/AA-0%';
DELETE FROM budget.admin_approval a USING budget.work w
 WHERE w.id = a.work_id AND w.remarks = 'DUMMY-SEED:20261022-01';
-- 7. work
DELETE FROM budget.work WHERE remarks = 'DUMMY-SEED:20261022-01' AND work_desc LIKE 'DUMMY-%';
-- 8. attachment (placeholders; no stored file exists)
DELETE FROM budget.attachment WHERE scan_engine = 'DUMMY-SEED: no file stored, not scanned';
-- 9. calendar, budget_cycle
DELETE FROM budget.calendar WHERE cycle_id = 2;
DELETE FROM budget.budget_cycle WHERE cycle_id = 2 AND current_fin_year = 2027;
-- 10. delegation_limit
DELETE FROM budget.delegation_limit WHERE source_ref LIKE 'DUMMY-SEED:20261022-01%';
-- 11. office (DIVISION -> CIRCLE -> CE -> SECRETARIAT -> FINANCE). audit_log.actor_office_id references
--     offices 26-33 (and IT Cell 1), so these DELETEs also fail on FK unless the audit rows go first.
--     These are real structure rows a later load-from-hierarchy would recreate; keeping them is recommended.
DELETE FROM budget.office WHERE office_id BETWEEN 26 AND 33 AND office_level = 'DIVISION';
DELETE FROM budget.office WHERE office_id BETWEEN 22 AND 25 AND office_level = 'CIRCLE';
DELETE FROM budget.office WHERE office_id IN (20, 21) AND office_level = 'CE';
DELETE FROM budget.office WHERE office_id = 19 AND office_level = 'SECRETARIAT';
DELETE FROM budget.office WHERE office_id = 18 AND office_level = 'FINANCE';
-- =====================================================================================================
-- Batch DUMMY-SEED:20261022-02 (dry-run personas). DOCUMENTATION ONLY - never executed by run_seed.py.
-- Neutralise (preferred over delete): the three persona charges stop resolving at once.
-- UPDATE employee.employee_charge SET is_active = false
--  WHERE employee_id IN (SELECT id FROM employee.employee WHERE employee_code IN (900101, 900201, 900301)
--                          AND name_english LIKE 'DUMMY-%');
-- The employee rows have no budget FK of their own, but audit_log/figure_version rows written by the
-- dry run carry 900101's id as actor, so deleting employee 900101 would orphan that attribution. Keep them.
-- =====================================================================================================
