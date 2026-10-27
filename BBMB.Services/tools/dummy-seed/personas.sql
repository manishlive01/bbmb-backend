-- =====================================================================================================
-- BBMB Budget dummy seed, batch DUMMY-SEED:20261022-02 (dry-run personas)
-- Purpose: three DUMMY test identities (DDO of Division 241, SE of Circle 240, AFA Budget) so that the
-- RTM-BUD-01 developer dry run can act from Division, Circle and Finance offices with locally minted tokens.
-- Target: shared BBMB database (Dev + Staging), schema employee (shared HR). Rows are PERMANENT unless
-- neutralised (see cleanup.sql, batch -02 block, documentation only). Charges expire after 30 days by time.
--
-- DEVIATION from DESIGN.md hard rule 5 ("never create an employee or charge"): rule 5 governs batch -01.
-- Creating test identities is the explicit purpose of this batch (user instruction, 2026-10-22). Disclosed
-- in README.md section "Batch -02".
--
-- Run ONLY through run_seed.py:
--   python3 run_seed.py personas-dry-run   -> one transaction, ROLLBACK
--   python3 run_seed.py personas-real      -> one transaction, COMMIT
--   python3 run_seed.py personas-verify    -> read-only (verify_personas.sql)
--
-- Hard rules kept: no UPDATE/DELETE/TRUNCATE; no trigger or session_replication_role change; no budget.*
-- table is written; every INSERT guarded by NOT EXISTS; the 11 actor keys are set; UUIDs are never echoed.
-- =====================================================================================================
\set ON_ERROR_STOP on
\pset pager off
\if :{?dry_run}
\else
  \set dry_run 1
\endif
\if :dry_run
  \echo '*** DRY RUN (batch -02): one transaction, ROLLED BACK at the end ***'
\else
  \echo '*** REAL RUN (batch -02): one transaction, COMMIT ***'
\endif
-- Phase 0 (read-only): actor = the batch -01 actor (DESIGN §2.2 C1, then C2), IT Cell office.
WITH mig AS (
    SELECT actor_employee_id, actor_charge_id, max(occurred_at) AS last_at
    FROM budget.audit_log WHERE action_code LIKE 'MIGRATION:%' GROUP BY 1, 2),
c1 AS (
    SELECT e.employee_code, e.name_english, e.id AS emp, c.id AS chg, m.last_at AS ord_ts
    FROM mig m
    JOIN employee.employee_charge c ON c.id = m.actor_charge_id AND c.employee_id = m.actor_employee_id
    JOIN employee.employee e ON e.id = c.employee_id
    WHERE c.project_code = 'Budget' AND c.is_active AND NOT c.is_charge_expire
      AND (c.charge_expire_time IS NULL OR c.charge_expire_time > now())
      AND btrim(coalesce(e.name_english, '')) <> ''),
c2 AS (
    SELECT e.employee_code, e.name_english, e.id AS emp, c.id AS chg, c.charge_time AS ord_ts
    FROM employee.employee_charge c JOIN employee.employee e ON e.id = c.employee_id
    WHERE c.project_code = 'Budget' AND c.is_active AND NOT c.is_charge_expire
      AND (c.charge_expire_time IS NULL OR c.charge_expire_time > now())
      AND coalesce(c.ce_id, 0) = 0 AND c.cr_id IS NULL AND c.division_id IS NULL
      AND btrim(coalesce(e.name_english, '')) <> '')
SELECT employee_code AS actor_code, name_english AS actor_name, emp AS actor_emp, chg AS actor_chg
FROM (SELECT employee_code, name_english, emp, chg FROM (SELECT * FROM c1 ORDER BY ord_ts DESC LIMIT 1) a
      UNION ALL
      SELECT employee_code, name_english, emp, chg FROM (SELECT * FROM c2 ORDER BY ord_ts LIMIT 1) b) x
LIMIT 1
\gset
SELECT office_id AS itcell_office FROM budget.office WHERE office_level = 'IT_CELL' AND is_active
\gset
\echo 'Actor:' :actor_code :actor_name '; IT Cell office id' :itcell_office
-- Persona definitions (session temp table; not a budget table).
DROP TABLE IF EXISTS pg_temp.dpersona;
CREATE TEMP TABLE dpersona (code int PRIMARY KEY, uid uuid NOT NULL, name text NOT NULL, email text NOT NULL,
                            ce_code int, cr_code int, division_code int);
INSERT INTO dpersona VALUES
 (900101, '00000000-0000-4000-8000-000000900101', 'DUMMY-DDO D1 241 XEN O&M Jalandhar (dry run 20261022)',
          'dummy.ddo.d1.900101@example.invalid', 200, 240, 241),
 (900201, '00000000-0000-4000-8000-000000900201', 'DUMMY-SE Circle 240 SE/O&M Jamalpur (dry run 20261022)',
          'dummy.se.c240.900201@example.invalid', 200, 240, NULL),
 (900301, '00000000-0000-4000-8000-000000900301', 'DUMMY-AFA Budget Finance (dry run 20261022)',
          'dummy.afa.900301@example.invalid', NULL, NULL, NULL);
-- Preflight: any failure raises and nothing is written.
DO $pre$
DECLARE v_n int;
BEGIN
    IF NOT EXISTS (SELECT 1 FROM employee.employee WHERE employee_code = 900001) THEN
        RAISE EXCEPTION 'PP1: employee 900001 (batch -01 actor) is absent'; END IF;
    SELECT count(*) INTO v_n FROM budget.ce_management ce
      JOIN budget.cr_management cr ON cr.ce_id = ce.id AND cr.cr_code = 240 AND cr.status
      JOIN budget.division d ON d.cr_id = cr.id AND d.division_code = 241
     WHERE ce.ce_code = 200 AND ce.status;
    IF v_n <> 1 THEN RAISE EXCEPTION 'PP2: masters 200/240/241 do not resolve as one chain (found %)', v_n; END IF;
    SELECT count(*) INTO v_n FROM budget.office o JOIN budget.division d ON d.id = o.division_id
     WHERE o.office_level = 'DIVISION' AND o.is_active AND d.division_code = 241;
    IF v_n <> 1 THEN RAISE EXCEPTION 'PP3: expected 1 active DIVISION office for 241, found %', v_n; END IF;
    SELECT count(*) INTO v_n FROM budget.office o JOIN budget.cr_management cr ON cr.id = o.circle_id
     WHERE o.office_level = 'CIRCLE' AND o.is_active AND cr.cr_code = 240;
    IF v_n <> 1 THEN RAISE EXCEPTION 'PP4: expected 1 active CIRCLE office for 240, found %', v_n; END IF;
    SELECT count(*) INTO v_n FROM budget.office WHERE office_level = 'FINANCE' AND is_active;
    IF v_n <> 1 THEN RAISE EXCEPTION 'PP5: expected 1 active FINANCE office, found %', v_n; END IF;
    SELECT count(*) INTO v_n FROM employee.employee e JOIN dpersona p ON (e.employee_code = p.code OR e.user_id = p.uid)
     WHERE e.name_english NOT LIKE 'DUMMY-%';
    IF v_n > 0 THEN RAISE EXCEPTION 'PP6: % non-DUMMY employee rows use a persona code or user_id', v_n; END IF;
    IF EXISTS (SELECT 1 FROM employee.employee WHERE user_id = '00000000-0000-4000-8000-000000999999') THEN
        RAISE EXCEPTION 'PP7: the GHOST user_id exists; P-GHOST must be absent'; END IF;
    RAISE NOTICE 'preflight (batch -02): all checks passed';
END
$pre$;
BEGIN;
SET LOCAL lock_timeout = '10s';
SELECT set_config('budget.actor_employee_id', :'actor_emp', true),
       set_config('budget.actor_charge_id',   :'actor_chg', true),
       set_config('budget.actor_office_id',   :'itcell_office', true),
       set_config('budget.actor_role_code',   'DUMMY_SEED', true),
       set_config('budget.actor_name',        :'actor_name', true),
       set_config('budget.on_behalf_of_post', '', true),
       set_config('budget.client_ip',         '0.0.0.0', true),
       set_config('budget.request_id',        gen_random_uuid()::text, true),
       set_config('budget.action_code',       'DUMMY-SEED:20261022-02', true),
       set_config('budget.justification',     'DUMMY-SEED:20261022-02', true),
       set_config('budget.no_justification',  '', true) \gset preamble_
DO $ins$
DECLARE p record; v_emp uuid; v_ce int; v_cr int; v_div int; t record;
BEGIN
    SELECT title_id, gender_id, date_of_birth INTO t FROM employee.employee WHERE employee_code = 900001;
    FOR p IN SELECT * FROM pg_temp.dpersona ORDER BY code LOOP
        IF EXISTS (SELECT 1 FROM employee.employee e WHERE e.employee_code = p.code OR e.user_id = p.uid) THEN
            RAISE NOTICE 'skipped employee % (exists)', p.code;
        ELSE
            INSERT INTO employee.employee (id, user_id, title_id, name_english, name_hindi, gender_id, date_of_birth,
                                           phone_number, email, address, employee_code)
            SELECT gen_random_uuid(), p.uid, t.title_id, p.name, NULL, t.gender_id, t.date_of_birth,
                   '0', p.email, 'DUMMY-address, dry run 20261022', p.code
            WHERE NOT EXISTS (SELECT 1 FROM employee.employee e WHERE e.employee_code = p.code OR e.user_id = p.uid);
            RAISE NOTICE 'created employee % %', p.code, p.name;
        END IF;
        SELECT e.id INTO v_emp FROM employee.employee e WHERE e.employee_code = p.code;
        v_ce := 0; v_cr := NULL; v_div := NULL;
        IF p.ce_code IS NOT NULL THEN SELECT id INTO v_ce FROM budget.ce_management WHERE ce_code = p.ce_code AND status; END IF;
        IF p.cr_code IS NOT NULL THEN SELECT id INTO v_cr FROM budget.cr_management WHERE cr_code = p.cr_code AND status AND ce_id = v_ce; END IF;
        IF p.division_code IS NOT NULL THEN SELECT id INTO v_div FROM budget.division WHERE division_code = p.division_code AND cr_id = v_cr; END IF;
        IF EXISTS (SELECT 1 FROM employee.employee_charge c WHERE c.employee_id = v_emp AND c.project_code = 'Budget'
                     AND c.is_active AND NOT c.is_charge_expire
                     AND (c.charge_expire_time IS NULL OR c.charge_expire_time > now())) THEN
            RAISE NOTICE 'skipped charge for % (current Budget charge exists)', p.code;
        ELSE
            INSERT INTO employee.employee_charge (id, employee_id, project_code, ce_id, charge_time, charge_expire_time,
                                                  is_active, is_charge_expire, cr_id, division_id)
            SELECT gen_random_uuid(), v_emp, 'Budget', v_ce, now() - interval '1 day', now() + interval '30 days',
                   true, false, v_cr, v_div
            WHERE NOT EXISTS (SELECT 1 FROM employee.employee_charge c WHERE c.employee_id = v_emp
                                AND c.project_code = 'Budget' AND c.is_active AND NOT c.is_charge_expire
                                AND (c.charge_expire_time IS NULL OR c.charge_expire_time > now()));
            RAISE NOTICE 'created charge for % (ce_id %, cr_id %, division_id %)', p.code, v_ce, v_cr, v_div;
        END IF;
    END LOOP;
END
$ins$;
SET CONSTRAINTS ALL IMMEDIATE;
SELECT e.employee_code, e.name_english, c.project_code, c.ce_id, c.cr_id, c.division_id,
       (c.charge_expire_time > now() + interval '29 days') AS expires_in_about_30d
FROM employee.employee e JOIN employee.employee_charge c ON c.employee_id = e.id
WHERE e.employee_code IN (900101, 900201, 900301) ORDER BY 1;
\if :dry_run
ROLLBACK;
\echo '*** ROLLED BACK (dry run) ***'
\else
COMMIT;
\echo '*** COMMITTED (batch -02) ***'
\endif
