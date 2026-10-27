-- =====================================================================================================
-- BBMB Budget dummy seed, batch DUMMY-SEED:20261022-01
-- Target: shared BBMB database (Dev + Staging). Rows written here are PERMANENT: every budget-process
-- table forbids DELETE/TRUNCATE by trigger (fn_forbid_change). See README.md and cleanup.sql.
--
-- Run ONLY through run_seed.py:
--   python3 run_seed.py dry-run   -> the whole seed in ONE transaction, deferred checks forced, ROLLBACK
--   python3 run_seed.py real      -> one transaction per phase, each ending SET CONSTRAINTS ALL IMMEDIATE; COMMIT
--
-- Design: DESIGN.md in this folder. Deviations forced by the live schema (P4 already applied) are listed
-- in README.md section "Deviations from DESIGN.md".
--
-- Hard rules kept: no trigger is disabled, session_replication_role is never touched, audit_log /
-- figure_version / recommendation_version / proposal_movement are never written directly, no existing row
-- is updated or deleted (the only UPDATE is admin_approval.current_version_no on two approvals this seed
-- created), every INSERT is guarded by NOT EXISTS on its natural key, every phase sets the 11 actor keys.
-- =====================================================================================================
\set ON_ERROR_STOP on
\pset pager off
\if :{?dry_run}
\else
  \set dry_run 1
\endif
\if :dry_run
  \echo '*** DRY RUN: everything below runs in ONE transaction and is ROLLED BACK at the end ***'
\else
  \echo '*** REAL RUN: each phase COMMITs ***'
\endif

-- -----------------------------------------------------------------------------------------------------
-- Phase 0 (read-only): actor (DESIGN §2.2, C1 then C2) and IT Cell office. UUIDs stay in psql variables.
-- -----------------------------------------------------------------------------------------------------
WITH mig AS (
    SELECT actor_employee_id, actor_charge_id, max(occurred_at) AS last_at
    FROM budget.audit_log WHERE action_code LIKE 'MIGRATION:%' GROUP BY 1, 2),
c1 AS (
    SELECT e.employee_code, e.name_english, e.id AS emp, c.id AS chg, 1 AS pref, m.last_at AS ord_ts, '' AS ord_code
    FROM mig m
    JOIN employee.employee_charge c ON c.id = m.actor_charge_id AND c.employee_id = m.actor_employee_id
    JOIN employee.employee e ON e.id = c.employee_id
    WHERE c.project_code = 'Budget' AND c.is_active AND NOT c.is_charge_expire
      AND (c.charge_expire_time IS NULL OR c.charge_expire_time > now())
      AND btrim(coalesce(e.name_english, '')) <> ''),
c2 AS (
    SELECT e.employee_code, e.name_english, e.id AS emp, c.id AS chg, 2 AS pref, c.charge_time AS ord_ts,
           e.employee_code::text AS ord_code
    FROM employee.employee_charge c JOIN employee.employee e ON e.id = c.employee_id
    WHERE c.project_code = 'Budget' AND c.is_active AND NOT c.is_charge_expire
      AND (c.charge_expire_time IS NULL OR c.charge_expire_time > now())
      AND coalesce(c.ce_id, 0) = 0 AND c.cr_id IS NULL AND c.division_id IS NULL
      AND btrim(coalesce(e.name_english, '')) <> '')
SELECT employee_code AS actor_code, name_english AS actor_name, emp AS actor_emp, chg AS actor_chg,
       CASE pref WHEN 1 THEN 'C1 (migration actor)' ELSE 'C2 (head-office charge)' END AS actor_rule
FROM (SELECT * FROM c1 ORDER BY ord_ts DESC LIMIT 1) a
UNION ALL
SELECT * FROM (SELECT employee_code, name_english, emp, chg, 'C2 (head-office charge)'
               FROM c2 ORDER BY ord_ts, ord_code LIMIT 1) b
LIMIT 1
\gset
SELECT office_id AS itcell_office FROM budget.office WHERE office_level = 'IT_CELL' AND is_active
\gset
\echo 'Actor:' :actor_code :actor_name 'selected by rule' :actor_rule '; IT Cell office id' :itcell_office

-- Preflight checks (P2 P3 P4 P5 P6 P7 P10 P11). Any failure raises and, with ON_ERROR_STOP, nothing is written.
DO $pre$
DECLARE
    v_batch constant text := 'DUMMY-SEED:20261022-01';
    v_n int;
    v_first record;
BEGIN
    SELECT count(*) INTO v_n FROM budget.office WHERE office_level = 'IT_CELL' AND is_active;
    IF v_n <> 1 THEN RAISE EXCEPTION 'P2: expected exactly 1 active IT_CELL office, found %', v_n; END IF;

    -- P3: the four settings the phase-4/5 amounts were designed against
    PERFORM 1 FROM (VALUES ('TS_CEILING_PCT', 105), ('NO_AA_LIMIT_RUPEES', 10000),
                           ('AA_DEMAND_CEILING_PCT', 110), ('AMR_INCREASE_PCT', 5)) AS x(k, v)
    WHERE (SELECT s.numeric_value FROM budget.setting s WHERE s.setting_key = x.k AND s.effective_from <= now()
           ORDER BY s.effective_from DESC LIMIT 1) IS DISTINCT FROM x.v;
    IF FOUND THEN RAISE EXCEPTION 'P3: live settings differ from DESIGN §4 P3; re-check amounts before seeding'; END IF;

    SELECT count(*) INTO v_n FROM budget.state;
    IF v_n <> 16 OR NOT EXISTS (SELECT 1 FROM budget.state WHERE state_code = 'S-01' AND figures_open) THEN
        RAISE EXCEPTION 'P4: state table is not the expected 16 rows with S-01 figures_open';
    END IF;

    -- P5: no foreign first cycle and no foreign 2026-27 cycle (this batch's own cycle is accepted on re-run)
    FOR v_first IN SELECT c.cycle_id, c.current_fin_year, c.is_first_cycle FROM budget.budget_cycle c
                   WHERE c.is_first_cycle OR c.current_fin_year = 2027 LOOP
        IF NOT (v_first.is_first_cycle AND v_first.current_fin_year = 2027 AND EXISTS (
                SELECT 1 FROM budget.audit_log a WHERE a.table_name = 'budget_cycle' AND a.operation = 'I'
                  AND a.record_key = v_first.cycle_id::text AND a.action_code = v_batch)) THEN
            RAISE EXCEPTION 'P5: cycle % (current_fin_year %, first %) exists and was not created by this batch',
                v_first.cycle_id, v_first.current_fin_year, v_first.is_first_cycle;
        END IF;
    END LOOP;

    -- P6: a DUMMY- work not written by this batch would collide with the natural keys used here
    SELECT count(*) INTO v_n FROM budget.work w WHERE w.work_desc LIKE 'DUMMY-%'
      AND NOT EXISTS (SELECT 1 FROM budget.audit_log a WHERE a.table_name = 'work' AND a.operation = 'I'
                        AND a.record_key = w.id::text AND a.action_code = v_batch);
    IF v_n > 0 THEN RAISE EXCEPTION 'P6: % DUMMY- work rows exist that this batch did not write', v_n; END IF;

    IF (now() AT TIME ZONE 'Asia/Kolkata')::date < DATE '2026-09-10' THEN
        RAISE EXCEPTION 'P7: IST date is before the latest dated item (2026-09-10)';
    END IF;

    -- P10: no expenditure figures, so every demand line is floor_check_unchecked (VR-15)
    SELECT count(*) INTO v_n FROM budget.expenditure_figure;
    IF v_n > 0 THEN RAISE EXCEPTION 'P10: expenditure_figure has % rows; floor_check_unchecked must be re-derived', v_n; END IF;

    PERFORM encode(sha256(convert_to('x', 'UTF8')), 'hex'), gen_random_uuid();  -- P11
    RAISE NOTICE 'preflight: all checks passed';
END
$pre$;

-- -----------------------------------------------------------------------------------------------------
-- Session-scoped selection and data (temp tables, not budget tables; created outside any phase).
-- Hierarchy (DESIGN §4.1): 2 CE x 2 circles x 2 divisions. Rank 1 (a current 'Budget' charge on the
-- master) matched nothing; rank 2 (CE 206 'testing', holding the 6 pre-DDD works) has a single circle and
-- cannot give the 2x2x2 shape, so the shape is applied as an eligibility filter and rank 3 (lowest id)
-- decides: CE 200 and CE 300. Keyed by master CODE so a re-run resolves the same rows.
-- -----------------------------------------------------------------------------------------------------
DROP TABLE IF EXISTS pg_temp.dseed_slot;
CREATE TEMP TABLE dseed_slot (slot text PRIMARY KEY, n int NOT NULL, ce_code int NOT NULL, cr_code int NOT NULL,
                              division_code int NOT NULL);
INSERT INTO dseed_slot VALUES
    ('D1', 1, 200, 240, 241), ('D2', 2, 200, 240, 242), ('D3', 3, 200, 250, 251), ('D4', 4, 200, 250, 254),
    ('D5', 5, 300, 330, 331), ('D6', 6, 300, 330, 332), ('D7', 7, 300, 340, 341), ('D8', 8, 300, 340, 342);

DROP TABLE IF EXISTS pg_temp.dseed_div;
CREATE TEMP TABLE dseed_div AS
SELECT s.slot, s.n, ce.id AS ce_id, cr.id AS cr_id, d.id AS division_id, d.division_code
FROM dseed_slot s
JOIN budget.ce_management ce ON ce.ce_code = s.ce_code AND ce.status
JOIN budget.cr_management cr ON cr.cr_code = s.cr_code AND cr.status AND cr.ce_id = ce.id
JOIN budget.division d ON d.division_code = s.division_code AND d.cr_id = cr.id;
SELECT (count(*) = 8)::int AS hier_ok FROM dseed_div \gset
\if :hier_ok
\else
  \echo 'STOP: expected 8 resolved division slots'
  \quit 3
\endif

-- Wing per CE (DESIGN §4.2): name ILIKE power/irrig, else the lowest irr_pwr_code and "wing assumed".
DROP TABLE IF EXISTS pg_temp.dseed_ce;
CREATE TEMP TABLE dseed_ce AS
SELECT ce.id AS ce_id, ce.ce_code, ce.name,
       coalesce(CASE WHEN ce.name ILIKE '%power%' THEN (SELECT irr_pwr_code FROM budget.irr_pwr WHERE irr_pwr_desc ILIKE '%power%' ORDER BY 1 LIMIT 1)
                     WHEN ce.name ILIKE '%irrig%' THEN (SELECT irr_pwr_code FROM budget.irr_pwr WHERE irr_pwr_desc ILIKE '%irrig%' ORDER BY 1 LIMIT 1) END,
                (SELECT min(irr_pwr_code) FROM budget.irr_pwr)) AS wing_code,
       NOT (ce.name ILIKE '%power%' OR ce.name ILIKE '%irrig%') AS wing_assumed
FROM budget.ce_management ce WHERE ce.id IN (SELECT ce_id FROM dseed_div);

-- Heads (DESIGN §4.3). WORK: heads of the existing works outside the receipt range (the 1001 'testing
-- account head' is a receipt-range head and is not used for works); no 4xxx/5xxx head exists, so the
-- class is assumed. OE: major 2000-3999. RC: major 0001-1999 (only one exists; reused round-robin).
DROP TABLE IF EXISTS pg_temp.dseed_head;
CREATE TEMP TABLE dseed_head AS
SELECT 'WORK'::text AS cls, row_number() OVER (ORDER BY h.complete_code)::int AS idx, h.*
FROM budget.budget_head_master h
WHERE NOT h.not_for_use AND h.major_acc_code >= '2000'
  AND EXISTS (SELECT 1 FROM budget.work w WHERE w.major_acc_code = h.major_acc_code AND w.sub_major_acc_code = h.sub_major_acc_code
              AND w.minor_acc_code = h.minor_acc_code AND w.sub_minor_acc_code = h.sub_minor_acc_code
              AND w.detail_acc_code = h.detail_acc_code AND w.object_acc_code = h.object_acc_code AND w.pre_ddd);
INSERT INTO dseed_head
SELECT 'OE', row_number() OVER (ORDER BY h.complete_code)::int, h.*
FROM (SELECT * FROM budget.budget_head_master WHERE NOT not_for_use AND major_acc_code BETWEEN '2000' AND '3999'
      ORDER BY complete_code LIMIT 6) h;
INSERT INTO dseed_head
SELECT 'RC', row_number() OVER (ORDER BY h.complete_code)::int, h.*
FROM (SELECT * FROM budget.budget_head_master WHERE NOT not_for_use AND major_acc_code BETWEEN '0001' AND '1999'
      ORDER BY complete_code LIMIT 6) h;
SELECT (count(DISTINCT cls) = 3)::int AS heads_ok FROM dseed_head \gset
\if :heads_ok
\else
  \echo 'STOP: a head class resolved to no head'
  \quit 3
\endif
SELECT cls, idx, account_code_id, complete_code, head_desc FROM dseed_head ORDER BY cls, idx;

-- Phase-4/5 data (DESIGN §8, §9). Whole rupees only.
DROP TABLE IF EXISTS pg_temp.dseed_aa;
CREATE TEMP TABLE dseed_aa (slot text PRIMARY KEY, div text, work_desc text, amount numeric, authority text,
                            approved date, ref text, eoffice text, expect_exceeds boolean, justification text);
INSERT INTO dseed_aa VALUES
 ('AA-01','D1','DUMMY-AA-01 Strengthening of inspection road along canal, km 2 to 4',1850000,'EE','2026-05-12','DUMMY-AA/2026-27/AA-01','DUMMY-EOFF/AA-01',false,NULL),
 ('AA-02','D2','DUMMY-AA-02 Replacement of gate hoist motors at regulator',4200000,'SE','2026-05-20','DUMMY-AA/2026-27/AA-02','DUMMY-EOFF/AA-02',false,NULL),
 ('AA-03','D3','DUMMY-AA-03 Repair of staff quarters block B',1200000,'EE','2026-06-03','DUMMY-AA/2026-27/AA-03','DUMMY-EOFF/AA-03',false,NULL),
 ('AA-04','D4','DUMMY-AA-04 Desilting of feeder channel, reach 3',8500000,'SE','2026-06-18','DUMMY-AA/2026-27/AA-04','DUMMY-EOFF/AA-04',false,NULL),
 ('AA-05','D5','DUMMY-AA-05 Rewinding of generator unit stator',16000000,'CE','2026-07-01','DUMMY-AA/2026-27/AA-05','DUMMY-EOFF/AA-05',false,NULL),
 ('AA-06','D6','DUMMY-AA-06 CCTV surveillance at control room',975000,'EE','2026-07-15','DUMMY-AA/2026-27/AA-06','DUMMY-EOFF/AA-06',false,NULL),
 ('AA-07','D7','DUMMY-AA-07 Protection wall downstream of spillway',24000000,'CE','2026-08-01','DUMMY-AA/2026-27/AA-07','DUMMY-EOFF/AA-07',false,NULL),
 ('AA-08','D8','DUMMY-AA-08 Upgradation of 66 kV switchyard protection relays',3850000,'EE','2026-08-20','DUMMY-AA/2026-27/AA-08','DUMMY-EOFF/AA-08',true,
  'DUMMY-Justification: approved above EE delegated power for test of BUD-055');

DROP TABLE IF EXISTS pg_temp.dseed_aa_rev;
CREATE TEMP TABLE dseed_aa_rev (slot text PRIMARY KEY, amount numeric, authority text, approved date, ref text);
INSERT INTO dseed_aa_rev VALUES
 ('AA-02',4750000,'SE','2026-08-15','DUMMY-AA/2026-27/AA-02/R1'),
 ('AA-05',14500000,'CE','2026-09-10','DUMMY-AA/2026-27/AA-05/R1');

DROP TABLE IF EXISTS pg_temp.dseed_ts;
CREATE TEMP TABLE dseed_ts (slot text PRIMARY KEY, tab text, aa_slot text, div text, amr_work_desc text,
                            estimate_name text, ts_number text, ts_date date, amount numeric);
INSERT INTO dseed_ts VALUES
 ('TS-01','AA','AA-01',NULL,NULL,'DUMMY-Estimate: inspection road km 2-4','DUMMY-TS/2026-27/TS-01','2026-07-10',1790000),
 ('TS-02','AA','AA-02',NULL,NULL,'DUMMY-Estimate: gate hoist motors','DUMMY-TS/2026-27/TS-02','2026-09-01',4300000),
 ('TS-03','AA','AA-03',NULL,NULL,'DUMMY-Estimate: staff quarters block B','DUMMY-TS/2026-27/TS-03','2026-07-20',1248000),
 ('TS-04','AA','AA-04',NULL,NULL,'DUMMY-Estimate: desilting reach 3','DUMMY-TS/2026-27/TS-04','2026-08-05',8000000),
 ('TS-05','AMR',NULL,'D5','DUMMY-AMR-01 Annual maintenance of colony roads 2026-27','DUMMY-Estimate: AM&R colony roads','DUMMY-TS/2026-27/TS-05','2026-06-25',640000),
 ('TS-06','AMR',NULL,'D6','DUMMY-AMR-02 Annual repair of drainage gallery pumps 2026-27','DUMMY-Estimate: AM&R gallery pumps','DUMMY-TS/2026-27/TS-06','2026-07-30',325000);

DROP TABLE IF EXISTS pg_temp.dseed_noaa;
CREATE TEMP TABLE dseed_noaa (slot text PRIMARY KEY, div text, work_desc text, estimated_cost numeric);
INSERT INTO dseed_noaa VALUES
 ('NOAA-01','D1','DUMMY-NOAA-01 Minor repair of office roof leakage',9500),
 ('NOAA-02','D2','DUMMY-NOAA-02 Replacement of damaged door locks, store',6800),
 ('NOAA-03','D3','DUMMY-NOAA-03 Fixing of name boards at site',9800),
 ('NOAA-04','D4','DUMMY-NOAA-04 Repair of hand pump, colony',10000);

DROP TABLE IF EXISTS pg_temp.dseed_bt;
CREATE TEMP TABLE dseed_bt (slot text PRIMARY KEY, budget_type int, div text, head_idx int, description text,
                            budget_amount numeric, revised_amount numeric, proposed_budget numeric);
INSERT INTO dseed_bt VALUES
 ('OE-01',1,'D1',1,'DUMMY-OE-01 Office contingencies',450000,480000,510000),
 ('OE-02',1,'D2',2,'DUMMY-OE-02 Telephone and internet charges',120000,135000,140000),
 ('OE-03',1,'D3',3,'DUMMY-OE-03 Electricity charges, office',600000,640000,675000),
 ('OE-04',1,'D4',4,'DUMMY-OE-04 Stationery and printing',85000,90000,95000),
 ('OE-05',1,'D5',5,'DUMMY-OE-05 POL for departmental vehicles',360000,390000,410000),
 ('OE-06',1,'D6',6,'DUMMY-OE-06 Minor office repairs',200000,225000,240000),
 ('RC-01',2,'D1',1,'DUMMY-RC-01 Rent recovered from staff quarters',240000,255000,270000),
 ('RC-02',2,'D2',2,'DUMMY-RC-02 Sale of tender documents',60000,65000,70000),
 ('RC-03',2,'D3',3,'DUMMY-RC-03 Water charges recovered',180000,190000,200000),
 ('RC-04',2,'D4',4,'DUMMY-RC-04 Sale of scrap',350000,320000,300000),
 ('RC-05',2,'D5',5,'DUMMY-RC-05 Licence fee, colony shops',120000,125000,130000),
 ('RC-06',2,'D6',6,'DUMMY-RC-06 Penalty recovered from contractors',75000,80000,85000);

-- Demand lines (DESIGN §9.3). kind WORK = existing seeded work by description; DE = new DEMAND_ENTRY work.
DROP TABLE IF EXISTS pg_temp.dseed_line;
CREATE TEMP TABLE dseed_line (div text, ord int, kind text, ref text, rbe numeric, obe numeric, justification text,
                              PRIMARY KEY (div, ord));
INSERT INTO dseed_line VALUES
 ('D1',1,'WORK','DUMMY-AA-01 Strengthening of inspection road along canal, km 2 to 4',600000,950000,NULL),
 ('D1',2,'WORK','DUMMY-NOAA-01 Minor repair of office roof leakage',8000,9500,NULL),
 ('D1',3,'UNF','DUMMY-UNF-D1 Unforeseen works',50000,75000,NULL),
 ('D2',1,'WORK','DUMMY-AA-02 Replacement of gate hoist motors at regulator',1500000,2200000,NULL),
 ('D2',2,'WORK','DUMMY-NOAA-02 Replacement of damaged door locks, store',6500,9000,NULL),
 ('D2',3,'UNF','DUMMY-UNF-D2 Unforeseen works',50000,75000,NULL),
 ('D3',1,'WORK','DUMMY-AA-03 Repair of staff quarters block B',400000,650000,NULL),
 ('D3',2,'WORK','DUMMY-NOAA-03 Fixing of name boards at site',9000,9800,NULL),
 ('D3',3,'UNF','DUMMY-UNF-D3 Unforeseen works',50000,75000,NULL),
 ('D4',1,'WORK','DUMMY-AA-04 Desilting of feeder channel, reach 3',2500000,4500000,NULL),
 ('D4',2,'WORK','DUMMY-NOAA-04 Repair of hand pump, colony',7200,10000,NULL),
 ('D4',3,'UNF','DUMMY-UNF-D4 Unforeseen works',50000,75000,NULL),
 ('D5',1,'WORK','DUMMY-AA-05 Rewinding of generator unit stator',4000000,7000000,NULL),
 ('D5',2,'WORK','DUMMY-AMR-01 Annual maintenance of colony roads 2026-27',300000,340000,
  'DUMMY-Justification: recurring annual maintenance; no preceding AM&R work recorded'),
 ('D5',3,'UNF','DUMMY-UNF-D5 Unforeseen works',50000,75000,NULL),
 ('D6',1,'WORK','DUMMY-AA-06 CCTV surveillance at control room',350000,500000,NULL),
 ('D6',2,'WORK','DUMMY-AMR-02 Annual repair of drainage gallery pumps 2026-27',150000,175000,
  'DUMMY-Justification: recurring annual maintenance; no preceding AM&R work recorded'),
 ('D6',3,'UNF','DUMMY-UNF-D6 Unforeseen works',50000,75000,NULL),
 ('D7',1,'WORK','DUMMY-AA-07 Protection wall downstream of spillway',6000000,12000000,NULL),
 ('D7',2,'DE','DUMMY-DE-01 Replacement of broken window panes, site office',6000,8500,NULL),
 ('D7',3,'UNF','DUMMY-UNF-D7 Unforeseen works',50000,75000,NULL),
 ('D8',1,'WORK','DUMMY-AA-08 Upgradation of 66 kV switchyard protection relays',1200000,2000000,NULL),
 ('D8',2,'DE','DUMMY-DE-02 Painting of gauge posts',5500,7000,NULL),
 ('D8',3,'UNF','DUMMY-UNF-D8 Unforeseen works',50000,75000,NULL);

-- Helper view: slot -> division id, its DIVISION office (once phase 1 has run) and its works head.
CREATE OR REPLACE TEMP VIEW dseed_slot_v AS
SELECT d.slot, d.n, d.division_id, d.division_code, o.office_id AS division_office_id, h.account_code_id AS work_head_id
FROM dseed_div d
LEFT JOIN budget.office o ON o.office_level = 'DIVISION' AND o.division_id = d.division_id
JOIN dseed_head h ON h.cls = 'WORK' AND h.idx = ((d.n - 1) % (SELECT count(*) FROM dseed_head WHERE cls = 'WORK')) + 1;

\if :dry_run
BEGIN;
\endif

-- =====================================================================================================
-- Phase 1: budget.office subset (LoadBudgetOfficesHandler; DESIGN §5)
-- =====================================================================================================
\echo '--- Phase 1: offices'
\if :dry_run
\else
BEGIN;
\endif
SET LOCAL lock_timeout = '10s';
SELECT set_config('budget.actor_employee_id', :'actor_emp', true),
       set_config('budget.actor_charge_id',   :'actor_chg', true),
       set_config('budget.actor_office_id',   :'itcell_office', true),
       set_config('budget.actor_role_code',   'DUMMY_SEED', true),
       set_config('budget.actor_name',        :'actor_name', true),
       set_config('budget.on_behalf_of_post', '', true),
       set_config('budget.client_ip',         '0.0.0.0', true),
       set_config('budget.request_id',        gen_random_uuid()::text, true),
       set_config('budget.action_code',       'DUMMY-SEED:20261022-01', true),
       set_config('budget.justification',     'DUMMY-SEED:20261022-01', true),
       set_config('budget.no_justification',  '', true) \gset dummy_
DO $p1$
DECLARE
    v_fin bigint; v_sec bigint; r record; v_n int;
BEGIN
    LOCK TABLE budget.office IN SHARE ROW EXCLUSIVE MODE;
    SELECT office_id INTO v_fin FROM budget.office WHERE office_level = 'FINANCE' ORDER BY office_id LIMIT 1;
    IF v_fin IS NULL THEN
        INSERT INTO budget.office (office_level, office_name) VALUES ('FINANCE', 'Finance') RETURNING office_id INTO v_fin;
        RAISE NOTICE 'created office FINANCE %', v_fin;
    ELSE RAISE NOTICE 'skipped office FINANCE % (exists)', v_fin; END IF;

    SELECT office_id INTO v_sec FROM budget.office WHERE office_level = 'SECRETARIAT' ORDER BY office_id LIMIT 1;
    IF v_sec IS NULL THEN
        INSERT INTO budget.office (office_level, office_name, parent_office_id) VALUES ('SECRETARIAT', 'Secretariat', v_fin)
        RETURNING office_id INTO v_sec;
        RAISE NOTICE 'created office SECRETARIAT %', v_sec;
    ELSE RAISE NOTICE 'skipped office SECRETARIAT % (exists)', v_sec; END IF;

    FOR r IN SELECT * FROM dseed_ce ORDER BY ce_id LOOP
        IF NOT EXISTS (SELECT 1 FROM budget.office WHERE ce_id = r.ce_id) THEN
            INSERT INTO budget.office (office_level, ce_id, wing_code, parent_office_id) VALUES ('CE', r.ce_id, r.wing_code, v_sec);
            RAISE NOTICE 'created office CE ce_code % wing % (assumed: %)', r.ce_code, r.wing_code, r.wing_assumed;
        ELSE RAISE NOTICE 'skipped office CE ce_code % (exists)', r.ce_code; END IF;
    END LOOP;

    FOR r IN SELECT DISTINCT d.cr_id, d.ce_id FROM dseed_div d ORDER BY d.cr_id LOOP
        IF NOT EXISTS (SELECT 1 FROM budget.office WHERE circle_id = r.cr_id) THEN
            INSERT INTO budget.office (office_level, circle_id, parent_office_id)
            SELECT 'CIRCLE', r.cr_id, o.office_id FROM budget.office o WHERE o.ce_id = r.ce_id;
            GET DIAGNOSTICS v_n = ROW_COUNT;
            IF v_n <> 1 THEN RAISE EXCEPTION 'circle %: parent CE office not found', r.cr_id; END IF;
            RAISE NOTICE 'created office CIRCLE cr_id %', r.cr_id;
        ELSE RAISE NOTICE 'skipped office CIRCLE cr_id % (exists)', r.cr_id; END IF;
    END LOOP;

    FOR r IN SELECT * FROM dseed_div ORDER BY n LOOP
        IF NOT EXISTS (SELECT 1 FROM budget.office WHERE division_id = r.division_id) THEN
            INSERT INTO budget.office (office_level, division_id, parent_office_id)
            SELECT 'DIVISION', r.division_id, o.office_id FROM budget.office o WHERE o.circle_id = r.cr_id;
            GET DIAGNOSTICS v_n = ROW_COUNT;
            IF v_n <> 1 THEN RAISE EXCEPTION 'division %: parent CIRCLE office not found', r.division_id; END IF;
            RAISE NOTICE 'created office DIVISION % (division_code %)', r.slot, r.division_code;
        ELSE RAISE NOTICE 'skipped office DIVISION % (exists)', r.slot; END IF;
    END LOOP;
END
$p1$;
\if :dry_run
\else
SET CONSTRAINTS ALL IMMEDIATE;
COMMIT;
\endif

-- =====================================================================================================
-- Phase 2: budget.delegation_limit (CreateDelegationLimitHandler; DESIGN §6). Dummy amounts, 2000-01-01.
-- =====================================================================================================
\echo '--- Phase 2: delegation limits'
\if :dry_run
\else
BEGIN;
\endif
SET LOCAL lock_timeout = '10s';
SELECT set_config('budget.actor_employee_id', :'actor_emp', true),
       set_config('budget.actor_charge_id',   :'actor_chg', true),
       set_config('budget.actor_office_id',   :'itcell_office', true),
       set_config('budget.actor_role_code',   'DUMMY_SEED', true),
       set_config('budget.actor_name',        :'actor_name', true),
       set_config('budget.on_behalf_of_post', '', true),
       set_config('budget.client_ip',         '0.0.0.0', true),
       set_config('budget.request_id',        gen_random_uuid()::text, true),
       set_config('budget.action_code',       'DUMMY-SEED:20261022-01', true),
       set_config('budget.justification',     'DUMMY-SEED:20261022-01', true),
       set_config('budget.no_justification',  '', true) \gset dummy_
DO $p2$
DECLARE r record;
BEGIN
    FOR r IN SELECT * FROM (VALUES ('EE', 2500000), ('SE', 10000000), ('CE', 50000000), ('SECRETARY', 250000000)) x(lvl, amt) LOOP
        IF NOT EXISTS (SELECT 1 FROM budget.delegation_limit WHERE authority_level = r.lvl AND effective_from = DATE '2000-01-01') THEN
            INSERT INTO budget.delegation_limit (authority_level, max_amount, effective_from, source_ref)
            VALUES (r.lvl, r.amt, DATE '2000-01-01', 'DUMMY-SEED:20261022-01 not an order of delegation');
            RAISE NOTICE 'created delegation_limit % %', r.lvl, r.amt;
        ELSE RAISE NOTICE 'skipped delegation_limit % (exists)', r.lvl; END IF;
    END LOOP;
END
$p2$;
\if :dry_run
\else
SET CONSTRAINTS ALL IMMEDIATE;
COMMIT;
\endif

-- =====================================================================================================
-- Phase 3: financial years, first OPEN cycle 2026-27 -> 2027-28, calendar (DESIGN §7)
-- =====================================================================================================
\echo '--- Phase 3: financial years, cycle, calendar'
\if :dry_run
\else
BEGIN;
\endif
SET LOCAL lock_timeout = '10s';
SELECT set_config('budget.actor_employee_id', :'actor_emp', true),
       set_config('budget.actor_charge_id',   :'actor_chg', true),
       set_config('budget.actor_office_id',   :'itcell_office', true),
       set_config('budget.actor_role_code',   'DUMMY_SEED', true),
       set_config('budget.actor_name',        :'actor_name', true),
       set_config('budget.on_behalf_of_post', '', true),
       set_config('budget.client_ip',         '0.0.0.0', true),
       set_config('budget.request_id',        gen_random_uuid()::text, true),
       set_config('budget.action_code',       'DUMMY-SEED:20261022-01', true),
       set_config('budget.justification',     'DUMMY-SEED:20261022-01', true),
       set_config('budget.no_justification',  '', true) \gset dummy_
DO $p3$
DECLARE v_fy int; v_cycle bigint; r record;
BEGIN
    -- fa_financial_years has no name/description column to mark; the rows are real calendar facts.
    FOREACH v_fy IN ARRAY ARRAY[2027, 2028] LOOP
        IF NOT EXISTS (SELECT 1 FROM budget.fa_financial_years WHERE fin_year_closing = v_fy) THEN
            INSERT INTO budget.fa_financial_years (fin_year_closing, start_date, end_date)
            VALUES (v_fy, make_date(v_fy - 1, 4, 1), make_date(v_fy, 3, 31));
            RAISE NOTICE 'created fa_financial_years %', v_fy;
        ELSE RAISE NOTICE 'skipped fa_financial_years % (exists)', v_fy; END IF;
    END LOOP;

    SELECT cycle_id INTO v_cycle FROM budget.budget_cycle WHERE current_fin_year = 2027;
    IF v_cycle IS NULL THEN
        INSERT INTO budget.budget_cycle (current_fin_year, next_fin_year, is_first_cycle, status)
        VALUES (2027, 2028, true, 'OPEN') RETURNING cycle_id INTO v_cycle;
        RAISE NOTICE 'created budget_cycle % (2027 -> 2028, first, OPEN)', v_cycle;
    ELSE RAISE NOTICE 'skipped budget_cycle % (exists)', v_cycle; END IF;

    FOR r IN SELECT * FROM (VALUES ('DIVISION', DATE '2026-11-15'), ('CIRCLE', DATE '2026-11-30'), ('CE', DATE '2026-12-15'),
                                   ('SECRETARIAT', DATE '2026-12-31'), ('FINANCE', DATE '2027-01-15')) x(lvl, d) LOOP
        IF NOT EXISTS (SELECT 1 FROM budget.calendar WHERE cycle_id = v_cycle AND office_level = r.lvl) THEN
            INSERT INTO budget.calendar (cycle_id, office_level, last_date) VALUES (v_cycle, r.lvl, r.d);
            RAISE NOTICE 'created calendar % %', r.lvl, r.d;
        ELSE RAISE NOTICE 'skipped calendar % (exists)', r.lvl; END IF;
    END LOOP;
END
$p3$;
\if :dry_run
\else
SET CONSTRAINTS ALL IMMEDIATE;
COMMIT;
\endif

-- =====================================================================================================
-- Phase 4: attachments, works + A&A (+2 revisions), TS, works without A&A, legacy budget entries (DESIGN §8)
-- Live schema is at P4 (BudgetP4WorkCodeConformance): a work is entered WITHOUT a code (work_code NULL,
-- DV-19) exactly as the deployed CreateWork/CreateAdminApproval/CreateTechnicalSanction handlers do.
-- Legacy estimates are NOT written: the P4 CreateEstimateHandler refuses every create with M-19.
-- =====================================================================================================
\echo '--- Phase 4: works, A&A, TS, budget entries'
\if :dry_run
\else
BEGIN;
\endif
SET LOCAL lock_timeout = '10s';
SELECT set_config('budget.actor_employee_id', :'actor_emp', true),
       set_config('budget.actor_charge_id',   :'actor_chg', true),
       set_config('budget.actor_office_id',   :'itcell_office', true),
       set_config('budget.actor_role_code',   'DUMMY_SEED', true),
       set_config('budget.actor_name',        :'actor_name', true),
       set_config('budget.on_behalf_of_post', '', true),
       set_config('budget.client_ip',         '0.0.0.0', true),
       set_config('budget.request_id',        gen_random_uuid()::text, true),
       set_config('budget.action_code',       'DUMMY-SEED:20261022-01', true),
       set_config('budget.justification',     'DUMMY-SEED:20261022-01', true),
       set_config('budget.no_justification',  '', true) \gset dummy_
DO $p4$
DECLARE
    c_batch  constant text := 'DUMMY-SEED:20261022-01';
    c_engine constant text := 'DUMMY-SEED: no file stored, not scanned';
    v_aa_att bigint; v_ts_att bigint;
    r record; s record; h record;
    v_work int; v_aa bigint; v_serial int; v_lim numeric; v_exceeds boolean; v_cur numeric; v_sum numeric;
    v_pct numeric; v_ts bigint; v_v1 record; v_cycle record; v_seq int; v_sect int;
BEGIN
    -- 4a: placeholder evidence attachments (no stored file; DESIGN §1.1, §10, G-4)
    SELECT attachment_id INTO v_aa_att FROM budget.attachment
    WHERE original_file_name = 'DUMMY-SEED-AA-COPY-PLACEHOLDER.pdf' AND scan_engine = c_engine;
    IF v_aa_att IS NULL THEN
        INSERT INTO budget.attachment (storage_key, original_file_name, file_extension, content_type, evidence_class,
                                       size_bytes, sha256_digest, scan_engine, scanned_at)
        VALUES (gen_random_uuid(), 'DUMMY-SEED-AA-COPY-PLACEHOLDER.pdf', 'pdf', 'application/pdf', 'EVIDENCE', 1,
                encode(sha256(convert_to(c_batch || ':AA', 'UTF8')), 'hex'), c_engine, now())
        RETURNING attachment_id INTO v_aa_att;
        RAISE NOTICE 'created attachment AA placeholder %', v_aa_att;
    ELSE RAISE NOTICE 'skipped attachment AA placeholder % (exists)', v_aa_att; END IF;
    SELECT attachment_id INTO v_ts_att FROM budget.attachment
    WHERE original_file_name = 'DUMMY-SEED-TS-COPY-PLACEHOLDER.pdf' AND scan_engine = c_engine;
    IF v_ts_att IS NULL THEN
        INSERT INTO budget.attachment (storage_key, original_file_name, file_extension, content_type, evidence_class,
                                       size_bytes, sha256_digest, scan_engine, scanned_at)
        VALUES (gen_random_uuid(), 'DUMMY-SEED-TS-COPY-PLACEHOLDER.pdf', 'pdf', 'application/pdf', 'EVIDENCE', 1,
                encode(sha256(convert_to(c_batch || ':TS', 'UTF8')), 'hex'), c_engine, now())
        RETURNING attachment_id INTO v_ts_att;
        RAISE NOTICE 'created attachment TS placeholder %', v_ts_att;
    ELSE RAISE NOTICE 'skipped attachment TS placeholder % (exists)', v_ts_att; END IF;

    -- 4b: new work + A&A header + version 1 (CreateAdminApprovalHandler, "new work" path)
    FOR r IN SELECT * FROM dseed_aa ORDER BY slot LOOP
        SELECT * INTO s FROM dseed_slot_v WHERE slot = r.div;
        IF s.division_office_id IS NULL THEN RAISE EXCEPTION '%: DIVISION office missing', r.slot; END IF;
        SELECT id INTO v_work FROM budget.work WHERE division_id = s.division_id AND work_desc = r.work_desc;
        IF v_work IS NOT NULL THEN
            IF NOT EXISTS (SELECT 1 FROM budget.admin_approval WHERE work_id = v_work) THEN
                RAISE EXCEPTION '%: work % exists without an approval; not repaired', r.slot, v_work;
            END IF;
            RAISE NOTICE 'skipped % (exists, work %)', r.slot, v_work; CONTINUE;
        END IF;
        SELECT * INTO h FROM budget.budget_head_master WHERE account_code_id = s.work_head_id;
        INSERT INTO budget.work (division_id, work_desc, work_class, entered_via, user_code, pre_ddd,
                                 major_acc_code, sub_major_acc_code, minor_acc_code, sub_minor_acc_code, detail_acc_code, object_acc_code,
                                 original_budget, revised_budget, proposed_budget, remarks)
        VALUES (s.division_id, r.work_desc, 'AA', 'AA_ENTRY', 'BUDAA', false,
                h.major_acc_code, h.sub_major_acc_code, h.minor_acc_code, h.sub_minor_acc_code, h.detail_acc_code, h.object_acc_code,
                '', '', '', c_batch)
        RETURNING id INTO v_work;
        PERFORM pg_advisory_xact_lock(hashtext('budget.aa_serial'), (s.division_office_id % 2147483647)::int);
        SELECT coalesce(max(serial_no), 0) + 1 INTO v_serial FROM budget.admin_approval WHERE division_office_id = s.division_office_id;
        SELECT max_amount INTO v_lim FROM budget.delegation_limit
        WHERE authority_level = r.authority AND effective_from <= r.approved ORDER BY effective_from DESC LIMIT 1;
        v_exceeds := v_lim IS NOT NULL AND r.amount > v_lim;
        IF v_exceeds <> r.expect_exceeds THEN
            RAISE EXCEPTION '%: exceeds_delegation computed % but designed %; live limits differ', r.slot, v_exceeds, r.expect_exceeds;
        END IF;
        INSERT INTO budget.admin_approval (work_id, division_office_id, serial_no, current_version_no)
        VALUES (v_work, s.division_office_id, v_serial, 1) RETURNING admin_approval_id INTO v_aa;
        INSERT INTO budget.admin_approval_version (admin_approval_id, version_no, name_of_work, aa_reference_no, date_of_approval,
                                                   account_code_id, amount_sanctioned, aa_copy_attachment_id, eoffice_ref,
                                                   competent_authority, exceeds_delegation, delegation_justification)
        VALUES (v_aa, 1, r.work_desc, r.ref, r.approved, h.account_code_id, r.amount, v_aa_att, r.eoffice,
                r.authority, v_exceeds, CASE WHEN v_exceeds THEN r.justification END);
        RAISE NOTICE 'created % work % approval % serial % amount % exceeds %', r.slot, v_work, v_aa, v_serial, r.amount, v_exceeds;
    END LOOP;

    -- 4c: revisions to version 2 (ReviseAdminApprovalHandler). No live demand line exists, so no correction flag.
    FOR r IN SELECT rv.*, a.work_desc, a.div, a.eoffice FROM dseed_aa_rev rv JOIN dseed_aa a USING (slot) ORDER BY slot LOOP
        SELECT * INTO s FROM dseed_slot_v WHERE slot = r.div;
        SELECT a.admin_approval_id INTO v_aa FROM budget.admin_approval a JOIN budget.work w ON w.id = a.work_id
        WHERE w.division_id = s.division_id AND w.work_desc = r.work_desc FOR UPDATE OF a;
        IF v_aa IS NULL THEN RAISE EXCEPTION '% revision: approval not found', r.slot; END IF;
        IF EXISTS (SELECT 1 FROM budget.admin_approval_version WHERE admin_approval_id = v_aa AND version_no = 2) THEN
            RAISE NOTICE 'skipped % revision (v2 exists)', r.slot; CONTINUE;
        END IF;
        SELECT * INTO v_v1 FROM budget.admin_approval_version WHERE admin_approval_id = v_aa AND version_no = 1;
        SELECT max_amount INTO v_lim FROM budget.delegation_limit
        WHERE authority_level = r.authority AND effective_from <= r.approved ORDER BY effective_from DESC LIMIT 1;
        v_exceeds := v_lim IS NOT NULL AND r.amount > v_lim;
        IF v_exceeds THEN RAISE EXCEPTION '% revision unexpectedly exceeds delegation', r.slot; END IF;
        INSERT INTO budget.admin_approval_version (admin_approval_id, version_no, name_of_work, aa_reference_no, date_of_approval,
                                                   account_code_id, amount_sanctioned, aa_copy_attachment_id, eoffice_ref,
                                                   competent_authority, exceeds_delegation, delegation_justification)
        VALUES (v_aa, 2, v_v1.name_of_work, r.ref, r.approved, v_v1.account_code_id, r.amount, v_aa_att, v_v1.eoffice_ref,
                r.authority, false, NULL);
        UPDATE budget.admin_approval SET current_version_no = 2 WHERE admin_approval_id = v_aa AND current_version_no = 1;
        RAISE NOTICE 'created % revision v2 amount %', r.slot, r.amount;
    END LOOP;

    -- 4d: technical sanctions (CreateTechnicalSanctionHandler). AA tab on an approval; AMR tab with a new AMR work.
    SELECT s2.numeric_value INTO v_pct FROM budget.setting s2 WHERE s2.setting_key = 'TS_CEILING_PCT' AND s2.effective_from <= now()
    ORDER BY s2.effective_from DESC LIMIT 1;
    FOR r IN SELECT * FROM dseed_ts ORDER BY slot LOOP
        IF r.tab = 'AA' THEN
            SELECT * INTO s FROM dseed_slot_v WHERE slot = (SELECT div FROM dseed_aa WHERE slot = r.aa_slot);
            SELECT a.admin_approval_id, a.work_id INTO v_aa, v_work FROM budget.admin_approval a JOIN budget.work w ON w.id = a.work_id
            WHERE w.division_id = s.division_id AND w.work_desc = (SELECT work_desc FROM dseed_aa WHERE slot = r.aa_slot);
            IF v_aa IS NULL THEN RAISE EXCEPTION '%: approval % not found', r.slot, r.aa_slot; END IF;
            IF EXISTS (SELECT 1 FROM budget.technical_sanction t JOIN budget.technical_sanction_version v
                         ON v.technical_sanction_id = t.technical_sanction_id
                       WHERE t.admin_approval_id = v_aa AND v.ts_number = r.ts_number) THEN
                RAISE NOTICE 'skipped % (exists)', r.slot; CONTINUE;
            END IF;
            PERFORM pg_advisory_xact_lock(hashtext('budget.ts_ceiling'), (v_aa % 2147483647)::int);
            SELECT v.amount_sanctioned INTO v_cur FROM budget.admin_approval a
            JOIN budget.admin_approval_version v ON v.admin_approval_id = a.admin_approval_id AND v.version_no = a.current_version_no
            WHERE a.admin_approval_id = v_aa;
            SELECT coalesce(sum(v.ts_amount), 0) INTO v_sum FROM budget.technical_sanction t
            JOIN budget.technical_sanction_version v ON v.technical_sanction_id = t.technical_sanction_id AND v.version_no = t.current_version_no
            WHERE t.admin_approval_id = v_aa AND t.tab = 'AA';
            IF (v_sum + r.amount) * 100 > v_pct * v_cur THEN
                RAISE EXCEPTION '%: TS total % would exceed % %% of A&A %', r.slot, v_sum + r.amount, v_pct, v_cur;
            END IF;
        ELSE
            SELECT * INTO s FROM dseed_slot_v WHERE slot = r.div;
            v_aa := NULL;
            SELECT id INTO v_work FROM budget.work WHERE division_id = s.division_id AND work_desc = r.amr_work_desc;
            IF v_work IS NOT NULL THEN
                IF NOT EXISTS (SELECT 1 FROM budget.technical_sanction WHERE work_id = v_work AND tab = 'AMR') THEN
                    RAISE EXCEPTION '%: AMR work % exists without its sanction; not repaired', r.slot, v_work;
                END IF;
                RAISE NOTICE 'skipped % (exists, work %)', r.slot, v_work; CONTINUE;
            END IF;
            SELECT * INTO h FROM budget.budget_head_master WHERE account_code_id = s.work_head_id;
            INSERT INTO budget.work (division_id, work_desc, work_class, entered_via, user_code, pre_ddd,
                                     major_acc_code, sub_major_acc_code, minor_acc_code, sub_minor_acc_code, detail_acc_code, object_acc_code,
                                     original_budget, revised_budget, proposed_budget, remarks)
            VALUES (s.division_id, r.amr_work_desc, 'AMR', 'TS_ENTRY', 'BUDTS', false,
                    h.major_acc_code, h.sub_major_acc_code, h.minor_acc_code, h.sub_minor_acc_code, h.detail_acc_code, h.object_acc_code,
                    '', '', '', c_batch)
            RETURNING id INTO v_work;
        END IF;
        PERFORM pg_advisory_xact_lock(hashtext('budget.ts_serial'), s.division_id);
        SELECT coalesce(max(t.serial_no), 0) + 1 INTO v_serial FROM budget.technical_sanction t
        JOIN budget.work w ON w.id = t.work_id WHERE w.division_id = s.division_id;
        INSERT INTO budget.technical_sanction (work_id, tab, admin_approval_id, serial_no, current_version_no)
        VALUES (v_work, r.tab, v_aa, v_serial, 1) RETURNING technical_sanction_id INTO v_ts;
        INSERT INTO budget.technical_sanction_version (technical_sanction_id, version_no, name_of_estimate, ts_number, ts_date,
                                                       ts_amount, ts_copy_attachment_id)
        VALUES (v_ts, 1, r.estimate_name, r.ts_number, r.ts_date, r.amount, v_ts_att);
        RAISE NOTICE 'created % tab % sanction % serial % amount %', r.slot, r.tab, v_ts, v_serial, r.amount;
    END LOOP;

    -- 4e: works without A&A (CreateWorkHandler: LEGACY_OPENING, first cycle OPEN, WORKS proposal not yet submitted)
    SELECT c.cycle_id, c.status INTO v_cycle FROM budget.budget_cycle c WHERE c.is_first_cycle;
    IF v_cycle.cycle_id IS NULL OR v_cycle.status <> 'OPEN' THEN RAISE EXCEPTION '4e: first cycle is not OPEN'; END IF;
    FOR r IN SELECT * FROM dseed_noaa ORDER BY slot LOOP
        SELECT * INTO s FROM dseed_slot_v WHERE slot = r.div;
        IF EXISTS (SELECT 1 FROM budget.work WHERE division_id = s.division_id AND work_desc = r.work_desc) THEN
            RAISE NOTICE 'skipped % (exists)', r.slot; CONTINUE;
        END IF;
        IF EXISTS (SELECT 1 FROM budget.proposal p WHERE p.cycle_id = v_cycle.cycle_id AND p.category = 'WORKS'
                   AND p.origin_office_id = s.division_office_id AND p.first_submitted_at IS NOT NULL) THEN
            RAISE EXCEPTION '%: WORKS proposal already submitted (WorkRules.CheckFirstCycleAsync)', r.slot;
        END IF;
        IF r.estimated_cost > (SELECT numeric_value FROM budget.setting WHERE setting_key = 'NO_AA_LIMIT_RUPEES'
                               AND effective_from <= now() ORDER BY effective_from DESC LIMIT 1) THEN
            RAISE EXCEPTION '%: estimated cost above NO_AA_LIMIT_RUPEES', r.slot;
        END IF;
        SELECT * INTO h FROM budget.budget_head_master WHERE account_code_id = s.work_head_id;
        INSERT INTO budget.work (division_id, work_desc, work_class, entered_via, user_code, pre_ddd, estimated_cost,
                                 major_acc_code, sub_major_acc_code, minor_acc_code, sub_minor_acc_code, detail_acc_code, object_acc_code,
                                 original_budget, revised_budget, proposed_budget, remarks)
        VALUES (s.division_id, r.work_desc, 'AA', 'LEGACY_OPENING', 'DUMMY', false, r.estimated_cost,
                h.major_acc_code, h.sub_major_acc_code, h.minor_acc_code, h.sub_minor_acc_code, h.detail_acc_code, h.object_acc_code,
                '', '', '', c_batch)
        RETURNING id INTO v_work;
        RAISE NOTICE 'created % work % estimated_cost %', r.slot, v_work, r.estimated_cost;
    END LOOP;

    -- 4g: legacy budget_table entries (CreateBudgetEntryHandler), each linked to the slot's AA work (G-1)
    FOR r IN SELECT * FROM dseed_bt ORDER BY slot LOOP
        SELECT * INTO s FROM dseed_slot_v WHERE slot = r.div;
        IF EXISTS (SELECT 1 FROM budget.budget_table WHERE division_id = s.division_id AND calendar_year = 2027
                   AND budget_description = r.description) THEN
            RAISE NOTICE 'skipped % (exists)', r.slot; CONTINUE;
        END IF;
        SELECT * INTO h FROM dseed_head
        WHERE cls = CASE r.budget_type WHEN 1 THEN 'OE' ELSE 'RC' END
          AND idx = ((r.head_idx - 1) % (SELECT count(*) FROM dseed_head WHERE cls = CASE r.budget_type WHEN 1 THEN 'OE' ELSE 'RC' END)) + 1;
        SELECT w.id INTO v_work FROM budget.work w JOIN budget.admin_approval a ON a.work_id = w.id
        WHERE w.division_id = s.division_id AND w.work_desc LIKE 'DUMMY-AA-%' ORDER BY w.id LIMIT 1;
        IF v_work IS NULL THEN RAISE EXCEPTION '%: no seeded AA work in the division', r.slot; END IF;
        SELECT coalesce(max(sequence_no), 0) + 1 INTO v_seq FROM budget.budget_table WHERE division_id = s.division_id AND calendar_year = 2027;
        SELECT coalesce(min(ds.id), 0) INTO v_sect FROM budget.div_section ds WHERE ds.division_id = s.division_id AND ds.status;
        INSERT INTO budget.budget_table (division_id, div_section_id, calendar_year, sequence_no,
                                         major_acc_code, sub_major_acc_code, minor_acc_code, sub_minor_acc_code, detail_acc_code, object_acc_code,
                                         budget_description, budget_amount, revised_amount, proposed_budget, budget_type, user_code, work_id)
        VALUES (s.division_id, v_sect, 2027, v_seq,
                h.major_acc_code, h.sub_major_acc_code, h.minor_acc_code, h.sub_minor_acc_code, h.detail_acc_code, h.object_acc_code,
                r.description, r.budget_amount, r.revised_amount, r.proposed_budget, r.budget_type, 'DUMMY', v_work);
        RAISE NOTICE 'created % budget_type % seq % head %', r.slot, r.budget_type, v_seq, h.complete_code;
    END LOOP;
END
$p4$;
\if :dry_run
\else
SET CONSTRAINTS ALL IMMEDIATE;
COMMIT;
\endif

-- =====================================================================================================
-- Phase 5: WORKS proposals at S-01 for D1-D8 with 3 demand lines each (OpenProposals, AddDemandLine; DESIGN §9)
-- No state change, no movement, no lease. While lines are written actor_office_id = the Division office
-- (DB-11 / M-20), disclosed in README.
-- =====================================================================================================
\echo '--- Phase 5: proposals and demand lines'
\if :dry_run
\else
BEGIN;
\endif
SET LOCAL lock_timeout = '10s';
SELECT set_config('budget.actor_employee_id', :'actor_emp', true),
       set_config('budget.actor_charge_id',   :'actor_chg', true),
       set_config('budget.actor_office_id',   :'itcell_office', true),
       set_config('budget.actor_role_code',   'DUMMY_SEED', true),
       set_config('budget.actor_name',        :'actor_name', true),
       set_config('budget.on_behalf_of_post', '', true),
       set_config('budget.client_ip',         '0.0.0.0', true),
       set_config('budget.request_id',        gen_random_uuid()::text, true),
       set_config('budget.action_code',       'DUMMY-SEED:20261022-01', true),
       set_config('budget.justification',     'DUMMY-SEED:20261022-01', true),
       set_config('budget.no_justification',  '', true) \gset dummy_
DO $p5$
DECLARE
    c_batch constant text := 'DUMMY-SEED:20261022-01';
    v_itcell text := current_setting('budget.actor_office_id');
    v_cycle bigint; s record; l record; h record; p record;
    v_prop bigint; v_work int; v_head bigint; v_serial int; v_line bigint;
BEGIN
    SELECT cycle_id INTO v_cycle FROM budget.budget_cycle WHERE current_fin_year = 2027 AND status = 'OPEN';
    IF v_cycle IS NULL THEN RAISE EXCEPTION 'phase 5: OPEN cycle 2027 not found'; END IF;
    LOCK TABLE budget.proposal IN SHARE ROW EXCLUSIVE MODE;

    FOR s IN SELECT * FROM dseed_slot_v ORDER BY n LOOP
        IF NOT EXISTS (SELECT 1 FROM budget.proposal WHERE cycle_id = v_cycle AND category = 'WORKS' AND origin_office_id = s.division_office_id) THEN
            INSERT INTO budget.proposal (cycle_id, category, origin_office_id, holder_office_id, state_code)
            VALUES (v_cycle, 'WORKS', s.division_office_id, s.division_office_id, 'S-01');
            RAISE NOTICE 'created WORKS proposal %', s.slot;
        ELSE RAISE NOTICE 'skipped WORKS proposal % (exists)', s.slot; END IF;
    END LOOP;
    -- Other active DIVISION offices (OpenProposalsHandler would open these too); none expected.
    FOR p IN SELECT o.office_id FROM budget.office o WHERE o.is_active AND o.office_level = 'DIVISION'
             AND o.office_id NOT IN (SELECT division_office_id FROM dseed_slot_v) LOOP
        RAISE NOTICE 'NOT opened: DIVISION office % outside the seeded slots', p.office_id;
    END LOOP;

    FOR s IN SELECT * FROM dseed_slot_v ORDER BY n LOOP
        SELECT pr.* INTO p FROM budget.proposal pr
        WHERE pr.cycle_id = v_cycle AND pr.category = 'WORKS' AND pr.origin_office_id = s.division_office_id FOR UPDATE;
        IF p.state_code <> 'S-01' OR p.holder_office_id <> p.origin_office_id THEN
            RAISE NOTICE 'skipped lines of % (proposal % not at S-01 with its origin)', s.slot, p.proposal_id; CONTINUE;
        END IF;
        PERFORM set_config('budget.actor_office_id', s.division_office_id::text, true);
        FOR l IN SELECT * FROM dseed_line WHERE div = s.slot ORDER BY ord LOOP
            IF l.kind = 'UNF' THEN
                IF EXISTS (SELECT 1 FROM budget.demand_line WHERE proposal_id = p.proposal_id AND line_type = 'UNFORESEEN'
                           AND unforeseen_name = l.ref) THEN
                    RAISE NOTICE 'skipped line % % (exists)', s.slot, l.ord; CONTINUE;
                END IF;
                v_work := NULL; v_head := NULL;
            ELSE
                SELECT id INTO v_work FROM budget.work WHERE division_id = s.division_id AND work_desc = l.ref;
                IF v_work IS NULL AND l.kind = 'DE' THEN
                    -- AddDemandLineHandler DEMAND_ENTRY path: new AA-class work with no approval, no code
                    SELECT * INTO h FROM budget.budget_head_master WHERE account_code_id = s.work_head_id;
                    INSERT INTO budget.work (division_id, work_desc, work_class, entered_via, user_code, pre_ddd,
                                             major_acc_code, sub_major_acc_code, minor_acc_code, sub_minor_acc_code, detail_acc_code, object_acc_code,
                                             original_budget, revised_budget, proposed_budget, remarks)
                    VALUES (s.division_id, l.ref, 'AA', 'DEMAND_ENTRY', 'BUDDE', false,
                            h.major_acc_code, h.sub_major_acc_code, h.minor_acc_code, h.sub_minor_acc_code, h.detail_acc_code, h.object_acc_code,
                            '', '', '', c_batch)
                    RETURNING id INTO v_work;
                    RAISE NOTICE 'created DEMAND_ENTRY work % in %', v_work, s.slot;
                ELSIF v_work IS NULL THEN
                    RAISE EXCEPTION 'line % %: work "%" not found', s.slot, l.ord, l.ref;
                END IF;
                IF EXISTS (SELECT 1 FROM budget.demand_line WHERE proposal_id = p.proposal_id AND work_id = v_work AND NOT is_deleted) THEN
                    RAISE NOTICE 'skipped line % % (exists)', s.slot, l.ord; CONTINUE;
                END IF;
                -- design §5.5 row i: the line's head is the head the work's six segments resolve to
                SELECT hm.account_code_id INTO v_head FROM budget.work w JOIN budget.budget_head_master hm
                  ON hm.major_acc_code = w.major_acc_code AND hm.sub_major_acc_code = w.sub_major_acc_code
                 AND hm.minor_acc_code = w.minor_acc_code AND hm.sub_minor_acc_code = w.sub_minor_acc_code
                 AND hm.detail_acc_code = w.detail_acc_code AND hm.object_acc_code = w.object_acc_code
                WHERE w.id = v_work;
            END IF;
            SELECT coalesce(max(serial_no), 0) + 1 INTO v_serial FROM budget.demand_line WHERE proposal_id = p.proposal_id;
            INSERT INTO budget.demand_line (proposal_id, line_type, serial_no, work_id, account_code_id, unforeseen_name,
                                            rbe_amount, obe_next_amount, justification_text,
                                            pct_check_not_applied, floor_check_unchecked, added_on_return, is_deleted)
            VALUES (p.proposal_id, CASE WHEN l.kind = 'UNF' THEN 'UNFORESEEN' ELSE 'WORK' END, v_serial, v_work, v_head,
                    CASE WHEN l.kind = 'UNF' THEN l.ref END, l.rbe, l.obe, l.justification,
                    false, true, false, false)
            RETURNING demand_line_id INTO v_line;
            RAISE NOTICE 'created line % serial % (%)', s.slot, v_serial, l.kind;
        END LOOP;
        PERFORM set_config('budget.actor_office_id', v_itcell, true);
    END LOOP;
END
$p5$;
\if :dry_run
\else
SET CONSTRAINTS ALL IMMEDIATE;
COMMIT;
\endif

\if :dry_run
\echo '--- DRY RUN: firing every deferred constraint trigger, then verify, then ROLLBACK'
SET CONSTRAINTS ALL IMMEDIATE;
\i verify.sql
ROLLBACK;
\echo '*** DRY RUN complete: ROLLED BACK, nothing written ***'
\else
\echo '*** REAL RUN complete ***'
\endif
