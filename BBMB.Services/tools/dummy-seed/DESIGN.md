# BBMB Budget: dummy seed design (hand-written SQL via psql, shared `BBMB` DB)

Batch tag: `DUMMY-SEED:20261022-01` (written below as **BATCH**).
Target: host `10.43.18.13`, database `BBMB` (shared by Dev and Staging). The user has authorised direct writes and accepts that the rows and their audit entries are permanent.
Status: design only. Nothing in this step wrote code or ran SQL.

This design replaces the "dedicated DB + EF seeder" recommendation in `financial-services/.agents/tasks/budget-dummy-data-plan/report.md`, because dotnet and Docker are not available. Everything else in that report (tables, triggers, screen needs) still applies as reference.

Sources read: `BudgetP1*`/`BudgetP2*`/`BudgetP3*` SQL under `BBMB.BudgetService/Infrastructure/Persistence/BudgetContexts/Sql/`, `BudgetContext*.cs`, `BudgetContextModelSnapshot.cs`, `Migrations/20260320104422_heirarchyKeysRefactor.cs`, and these handlers: LoadBudgetOffices, CreateBudgetCycle, SetCalendar, CreateDelegationLimit, OpenProposals, CreateWork, WorkRules, CreateAdminApproval, ReviseAdminApproval, AdminApprovalRules, CreateTechnicalSanction, TechnicalSanctionRules, AddDemandLine, DemandLineWrites, DemandLineRuleEvaluator, DemandLineRules, UploadAttachment, DownloadAttachment, CreateEstimate, CreateBudgetEntry, GetBudgetEntryList, plus the test helpers `BudgetDatabaseFixture`, `DbActor` and `TestActor`.

---

## 1. Hard rules (the coder must not break any of these)

1. **Never** disable triggers. That means no `ALTER TABLE … DISABLE TRIGGER`, no `session_replication_role` and no `SET CONSTRAINTS … DEFERRED` tricks to dodge a check.
2. **Never** INSERT into `budget.audit_log`, `figure_version`, `recommendation_version` or `proposal_movement`. Triggers write the first two. The phases here need no movement or recommendation rows.
3. **Never** DELETE, TRUNCATE or UPDATE a row this seed did not create. The only UPDATE in this design is `admin_approval.current_version_no` on two seeded approvals (step 4c).
4. **Every** write transaction starts with the actor preamble (§2), which sets all 11 keys with `set_config(…, true)`. It uses one **existing** current `employee_charge` with `project_code = 'Budget'`. Both `action_code` and `justification` are BATCH.
5. Reuse the existing masters (division, CE, circle, heads, irr_pwr). **Never** create a division, CE, circle, head, employee or charge. The only master rows you may add are the missing financial years, closing 2027 and 2028.
6. Every name, description, remark, reference and code that the seed creates and that can carry a prefix starts with `DUMMY-`. The exceptions are listed in §1.1.
7. Amounts are whole rupees. The `budget.rupees` domain enforces `VALUE = trunc(VALUE)`.
8. **Idempotent:** every insert is guarded by `NOT EXISTS` on the natural key given per step. Running the scripts twice must create nothing the second time.
9. **Never** set `budget.estimate_code_writer`. It is an internal 12th key that only `budget.fn_record_estimate_code` may set.
10. Read the DB credentials from the environment (`PGHOST`/`PGUSER`/`PGPASSWORD`, or a `.pgpass` outside the repo). Never put a password in a script, the report or git. The `appsettings*.json` passwords must not be copied anywhere.

### 1.1 Deliberate exceptions, and why

| Exception | Reason |
|---|---|
| `budget.office` rows carry no `DUMMY-` prefix. CE, CIRCLE and DIVISION rows have `office_name` NULL. FINANCE is `'Finance'` and SECRETARIAT is `'Secretariat'`. | `ck_office_name` makes the name NULL for mapped levels. FINANCE and SECRETARIAT are singletons that LoadFromHierarchy creates with exactly these names and **never updates**, and no rename endpoint exists. A `DUMMY-Finance` row would stay as the real Finance office for good. These rows are real structure, the same as the handler would produce, not dummy data. |
| `actor_role_code = 'DUMMY_SEED'` (not `DDO`, `AFA_BUDGET` and so on) | One officer's charge writes every row. Recording a role that officer does not hold would put a false attribution in the audit trail and in `figure_version.role_code`. The key must be non-blank, because `audit_log.actor_role_code` and `figure_version.role_code` are NOT NULL. |
| `actor_office_id` = the IT Cell office, except during the phase-5 demand-line inserts | DB-11 (`fn_demand_line_rules`) refuses a line unless `actor_office_id` equals the proposal's holder office. The handler gets there by acting as the Division's DDO. Here the seed officer is recorded as acting from that Division office. This is disclosed in §10. |
| Two placeholder `budget.attachment` rows with no stored file | `admin_approval_version.aa_copy_attachment_id` and `technical_sanction_version.ts_copy_attachment_id` are NOT NULL FKs to an EVIDENCE attachment (VR-10, DB-08). Without them, no A&A and no TS can exist. The consequences are in §10 and G-4. |

---

## 2. Actor context

### 2.1 The 11 keys (verbatim from `BudgetActionScope.SetActorSql` and `MigrationActor.sql`)

| # | Key | Value set by the seed | Why |
|---|---|---|---|
| 1 | `budget.actor_employee_id` | chosen employee `id` | `fn_require_actor` needs a UUID that matches the charge |
| 2 | `budget.actor_charge_id` | chosen charge `id` | It must be current: `is_active`, `NOT is_charge_expire`, and `charge_expire_time` NULL or in the future |
| 3 | `budget.actor_office_id` | IT Cell `office_id`. In phase 5, while lines are inserted, the Division office of the proposal (§7) | `audit_log.actor_office_id` is NOT NULL with an FK to `office`. DB-11 also reads this key |
| 4 | `budget.actor_role_code` | `'DUMMY_SEED'` | NOT NULL in audit_log and figure_version (§1.1) |
| 5 | `budget.actor_name` | that employee's `name_english` (must not be blank) | `audit_log.actor_name` is NOT NULL, and `fn_audit_row` turns `''` into NULL |
| 6 | `budget.on_behalf_of_post` | `''` | Not acting on behalf of anyone |
| 7 | `budget.client_ip` | `'0.0.0.0'` | Same as `MigrationActor.sql`: marks a non-HTTP origin |
| 8 | `budget.request_id` | `gen_random_uuid()::text`, one per transaction | |
| 9 | `budget.action_code` | BATCH | Not `UPLOAD%` (that would change `figure_version.change_source`) and not `RETURN` (an observation trigger keys on it) |
| 10 | `budget.justification` | BATCH | Copied into `figure_version.justification` |
| 11 | `budget.no_justification` | `''` | |

Preamble (the first statement of every transaction, before any other statement). psql variables come from §3.2:

```sql
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
       set_config('budget.no_justification',  '', true);
```

### 2.2 Which charge (decided)

Use the **IT Cell officer who ran the Budget migrations on this database**. Design §11.1 rule 8 makes the migration actor a named IT Cell officer, and their charge is recorded on every `MIGRATION:%` audit row. That makes it the only charge on record as acting for the IT Cell in Budget.

```sql
-- C1 (preferred)
WITH mig AS (
  SELECT actor_employee_id, actor_charge_id, max(occurred_at) AS last_at
  FROM budget.audit_log WHERE action_code LIKE 'MIGRATION:%' GROUP BY 1, 2)
SELECT e.employee_code, e.name_english, e.id AS actor_emp, c.id AS actor_chg
FROM mig m
JOIN employee.employee_charge c ON c.id = m.actor_charge_id AND c.employee_id = m.actor_employee_id
JOIN employee.employee e ON e.id = c.employee_id
WHERE c.project_code = 'Budget' AND c.is_active AND NOT c.is_charge_expire
  AND (c.charge_expire_time IS NULL OR c.charge_expire_time > now())
  AND btrim(coalesce(e.name_english, '')) <> ''
ORDER BY m.last_at DESC LIMIT 1;
```

Fallback C2, used only if C1 returns no row: a current `'Budget'` charge with `coalesce(ce_id,0) = 0 AND cr_id IS NULL AND division_id IS NULL` (a head-office charge, as an IT Cell or AFA officer holds), ordered by `charge_time` then `employee_code`, first row.
If both return nothing: **stop**, write no rows, and report.

The report names the actor by **employee code and name only**. Keep the UUIDs in psql variables and do not print them in the report.

---

## 3. Execution model

### 3.1 Files (the coder creates them in `tools/dummy-seed/`)

`00_preflight.sql` (read-only) · `10_offices.sql` · `20_delegation.sql` · `30_cycle.sql` · `40_works.sql` · `50_proposals.sql` · `90_verify.sql` (read-only) · `run.sql` (driver) · `REPORT.md` (written after the run).

The phase files contain **no** `BEGIN`/`COMMIT`. `run.sql` wraps them. Use `psql -X -v ON_ERROR_STOP=1 -v dry_run=1|0 -f run.sql`.

### 3.2 Session variables

`00_preflight.sql` resolves the following with `\gset`. Everything else is looked up by natural key inside each phase.

- `actor_emp`, `actor_chg`, `actor_name`, `itcell_office`
- `ce1`, `ce2`, `cr1`…`cr4`, `div1`…`div8` (master ids), `wing_ce1`, `wing_ce2`

Persist the selection to `selection.psql` as `\set` lines so that a re-run uses the same masters. It contains no secrets: master ids are not secret, while the actor UUIDs are re-resolved each run and never written out.

### 3.3 Dry run, then real run

- **Dry run (mandatory first):** run `BEGIN`, then the preamble, then phases 1–5, then `SET CONSTRAINTS ALL IMMEDIATE;` then `ROLLBACK;`. `SET CONSTRAINTS ALL IMMEDIATE` is essential. The deferred constraint triggers (DB-06 `*_version_check`, DB-09 `*_ts_ceiling`, DB-10 `trg_proposal_state_change`) only fire at commit. Without it, a ROLLBACK dry run would never exercise them. Identity sequences still advance on rollback. The id gaps this leaves are harmless and expected.
- **Real run:** one transaction per phase, each `BEGIN; <preamble>; \i phase; SET CONSTRAINTS ALL IMMEDIATE; COMMIT;`. A phase is all-or-nothing. If a phase fails, fix the cause and re-run. The guards skip whatever already exists.
- Each phase prints `RAISE NOTICE` lines `created`/`skipped` per item. The coder copies them into `REPORT.md`.

### 3.4 Locks taken (as the handlers do)

| Lock | Where | Source |
|---|---|---|
| `LOCK TABLE budget.office IN SHARE ROW EXCLUSIVE MODE` | phase 1 | LoadBudgetOfficesHandler |
| `pg_advisory_xact_lock(hashtext('budget.aa_serial'), (office_id % 2147483647)::int)` | before each A&A serial | AdminApprovalRules.NextSerialAsync |
| `pg_advisory_xact_lock(hashtext('budget.ts_serial'), division_id)` | before each TS serial | TechnicalSanctionRules.NextSerialAsync |
| `pg_advisory_xact_lock(hashtext('budget.ts_ceiling'), (admin_approval_id % 2147483647)::int)` | before each AA-tab TS | TechnicalSanctionRules.CheckAaCeilingAsync |
| `LOCK TABLE budget.proposal IN SHARE ROW EXCLUSIVE MODE` | phase 5 opening | OpenProposalsHandler |
| `SELECT 1 FROM budget.proposal WHERE proposal_id = … FOR UPDATE` | before each proposal's lines | DemandLineWrites.LockProposalAsync |

---

## 4. Phase 0: preflight (read-only; stop on any failure)

| # | Check | Expected | On mismatch |
|---|---|---|---|
| P1 | Actor resolved (§2.2) | 1 row | stop |
| P2 | `SELECT office_id FROM budget.office WHERE office_level='IT_CELL' AND is_active` | exactly 1 | stop |
| P3 | Settings in force (latest `effective_from <= now()` per key) | TS_CEILING_PCT 105, NO_AA_LIMIT_RUPEES 10000, AA_DEMAND_CEILING_PCT 110, AMR_INCREASE_PCT 5 | Use the live values in the §6 rules, then re-check the amounts in §6.2/§7.2 against them. Report |
| P4 | `budget.state` 16 rows. `S-01.figures_open = true` | yes | stop phase 5 |
| P5 | `budget.budget_cycle` has no row with `current_fin_year = 2027` and none with `is_first_cycle` | 0 / 0 | stop phases 3–5 and report. Do not create a non-first cycle silently |
| P6 | Natural-key collisions: any row whose description, reference or name starts with `DUMMY-` and was not written by this batch | 0 | report, and treat as already-seeded only if it matches exactly |
| P7 | IST date: `(now() AT TIME ZONE 'Asia/Kolkata')::date >= '2026-09-10'` (the latest dated item) | true | stop phase 4 (`ck_*_version_date`) |
| P8 | Live schema matches the columns used: `\d budget.<table>` for every table written | all columns present, same nullability | stop that phase and report the difference. Do not guess |
| P9 | `budget.estimate` FK target (OI-2) | `estimate.work_code → work(id)` | see §6.6 |
| P10 | Expenditure rows: `budget.expenditure_figure` for the dummy works | 0 (new works) | n/a (sets `floor_check_unchecked`, §7.2) |
| P11 | `gen_random_uuid()` and `sha256(bytea)` available | yes (PG ≥ 13) | stop phase 4a |

### 4.1 Hierarchy selection (deterministic; persisted to `selection.psql`)

Target shape: 2 active CEs (`ce_management.status`), 2 active circles each (`cr_management.status`, `cr.ce_id = ce`), 2 divisions per circle (`division.cr_id = cr`). That gives slots **D1–D8**: D1/D2 in cr1 and D3/D4 in cr2 under ce1, D5/D6 in cr3 and D7/D8 in cr4 under ce2.

Ranking, applied in order at every level (CE, then circle, then division):
1. Referenced by a current `'Budget'` `employee_charge` (`ce_id` / `cr_id` / `division_id`). Then an existing Budget user can see the data from their own office.
2. Contains the divisions of the 6 existing `budget.work` rows. Those works also become visible: GetWorkList shows only works of Divisions that have an office.
3. Lowest `id`.

If the masters cannot give 2×2×2, take up to 8 divisions from the two chosen CEs across whatever active circles they have, with at least 1 division per chosen circle. Report the shape actually used. Fewer than 4 divisions: stop and ask.

### 4.2 Wing per CE

`irr_pwr` holds the allowed codes. If the CE name matches `ILIKE '%power%'`, use the code whose `irr_pwr_desc ILIKE '%power%'`. If it matches `'%irrig%'`, use the irrigation code. Otherwise use the lowest `irr_pwr_code`, and list it in the report as "wing assumed". A wrong wing can be corrected in the app (`PUT /api/budget-offices/{id}/wing`, SetOfficeWingHandler).

### 4.3 Account heads (from `budget_head_master`, `not_for_use = false`, `sub_major_acc_code` and `sub_minor_acc_code` NOT NULL)

| Use | Rule | Count |
|---|---|---|
| Works (A&A, TS, no-A&A, demand-entry) | The heads of the 6 existing works, matched on all six segments (they already passed the `fk_work_account_code` guard). If there are fewer than 3, add heads with `major_acc_code LIKE '4%' OR LIKE '5%'` (capital outlay), by `complete_code` | 3–6 distinct; assign round-robin by slot |
| Office expense (`budget_type` 1) | `major_acc_code BETWEEN '2000' AND '3999'`, by `complete_code` | up to 6 distinct |
| Receipts (`budget_type` 2) | `major_acc_code BETWEEN '0001' AND '1999'`, by `complete_code` | up to 6 distinct |

If a category has no match, reuse the works heads and record "head class assumed" (OI-8). These ranges are the usual Government of India major-head ranges (receipts 0xxx–1xxx, revenue expenditure 2xxx–3xxx, capital 4xxx–5xxx). They have not been checked against BBMB's Manual of Accounts.

---

## 5. Phase 1: `budget.office` (replicates LoadBudgetOfficesHandler for the subset)

Insert in this order, without supplying `office_id` (GENERATED ALWAYS). `fn_require_actor` fills `created_*`. `is_active` takes its default of `true`.

| Row | office_level | division_id | circle_id | ce_id | office_name | wing_code | parent_office_id | Natural-key guard |
|---|---|---|---|---|---|---|---|---|
| FINANCE | `FINANCE` | NULL | NULL | NULL | `'Finance'` | NULL | NULL | `NOT EXISTS (office_level='FINANCE')` |
| SECRETARIAT | `SECRETARIAT` | NULL | NULL | NULL | `'Secretariat'` | NULL | FINANCE id | `NOT EXISTS (office_level='SECRETARIAT')` |
| IT_CELL | (exists, bootstrap) | | | | | | | not inserted. P2 checks it |
| CE ×2 | `CE` | NULL | NULL | `:ce1`/`:ce2` | NULL | `:wing_ce1`/`:wing_ce2` | SECRETARIAT id | `NOT EXISTS (ce_id = x)` (UK) |
| CIRCLE ×4 | `CIRCLE` | NULL | `cr_management.id` | NULL | NULL | NULL | office of `cr.ce_id` | `NOT EXISTS (circle_id = x)` (UK) |
| DIVISION ×8 | `DIVISION` | `division.id` | NULL | NULL | NULL | NULL | office of `division.cr_id` | `NOT EXISTS (division_id = x)` (UK) |

Like the handler, an existing row is **never updated** (the wing included). If an existing FINANCE row exists, use it as the parent.
Subset note: the real handler loads **every** active CE that has a wing, every active circle and every division. Here only the selected subset is loaded, because the task scopes it to ~8 divisions. A later real `load-from-hierarchy` adds the rest (it is idempotent per master id). It reports the remaining CEs as `cesWithoutWing` unless wings are supplied.

---

## 6. Phase 2: `budget.delegation_limit` (append-only; CreateDelegationLimitHandler)

These rows are not required to insert A&As (no row means no limit). They are needed so that one A&A can be consistently `exceeds_delegation = true` (BUD-055), and so the Settings screen lists limits.

| authority_level | max_amount (₹) | effective_from | source_ref | Guard (UK) |
|---|---|---|---|---|
| EE | 25,00,000 | 2000-01-01 | `DUMMY-SEED:20261022-01 not an order of delegation` | `NOT EXISTS (authority_level, effective_from)` |
| SE | 1,00,00,000 | 2000-01-01 | same | same |
| CE | 5,00,00,000 | 2000-01-01 | same | same |
| SECRETARY | 25,00,00,000 | 2000-01-01 | same | same |

**`effective_from = 2000-01-01` is deliberate.** The limit in force is the row with the latest `effective_from <= date` (DelegationLimitReader). With the earliest possible date, any real order loaded later with its own date overrides the dummy row from that date on. The amounts are dummy values, not BBMB's delegation of powers.
Side effect (disclosed): until real limits are loaded, any A&A that a real tester enters above these amounts asks for a justification (M-27). It is a warning only; nothing is refused once a justification is given.

---

## 7. Phase 3: financial years, cycle, calendar

| Table | Row | Guard | Rule replicated |
|---|---|---|---|
| `budget.fa_financial_years` | `(2027, '2026-04-01', '2027-03-31', closed NULL)` and `(2028, '2027-04-01', '2028-03-31', NULL)`, each only if missing | `NOT EXISTS (fin_year_closing = n)` | `ck_fa_financial_years_start/end` (1 April to 31 March). No actor trigger exists in the code (confirm in P8). Set the preamble anyway |
| `budget.budget_cycle` | `current_fin_year 2027, next_fin_year 2028, is_first_cycle true, status 'OPEN'` | `NOT EXISTS (current_fin_year = 2027)`. P5 already refused if any first cycle exists | CreateBudgetCycleHandler: both years exist, next = current + 1 (CHECK), one per current year (UK), one first cycle (partial UK) |
| `budget.calendar` (PK `cycle_id, office_level`) | DIVISION 2026-11-15 · CIRCLE 2026-11-30 · CE 2026-12-15 · SECRETARIAT 2026-12-31 · FINANCE 2027-01-15 | `NOT EXISTS (cycle_id, office_level)` | SetCalendarHandler/Validator: the five levels except IT_CELL (`ck_calendar_office_level`). The dates are dummy values, set in the future so no M-26 "lapsed" warning appears yet |

**Permanence warning (raise to the user):** this row permanently uses up the `current_fin_year = 2027` slot and the single `is_first_cycle` slot on the shared DB. The real 2026-27 / first cycle can then never be created on `BBMB`. It can later be set to `status = 'CLOSED'` (an UPDATE of a seeded row, which is allowed), but the UK slots stay taken. See G-5.

---

## 8. Phase 4: works, A&A, TS, legacy estimates and entries

Run as one transaction, in the sub-step order below (4a → 4g).

### 8.1 Rules replicated by hand

**Interim work code (WorkRules.NextInterimWorkCodeAsync; also CreateWorkHandler).** `work_code = int("{division.id}{n:D5}")`. Here `n = suffix + 1` when the Division's `max(work_code)` as text starts with `division.id` as text, is longer than it, and the suffix parses as an int. Otherwise `n = 1`. Recompute after each insert in the same Division.

```sql
-- p = division.id; :D5 means "at least 5 digits", NOT lpad-truncate
SELECT (p::text || CASE WHEN length(n::text) >= 5 THEN n::text ELSE lpad(n::text, 5, '0') END)::int
FROM (SELECT CASE WHEN last IS NOT NULL
                   AND left(last::text, length(p::text)) = p::text
                   AND length(last::text) > length(p::text)
                   AND substr(last::text, length(p::text) + 1) ~ '^[0-9]+$'
                   AND substr(last::text, length(p::text) + 1)::numeric <= 2147483647
              THEN substr(last::text, length(p::text) + 1)::int + 1 ELSE 1 END AS n
      FROM (SELECT max(work_code) AS last FROM budget.work WHERE division_id = p) s) t;
```

`work_code` has a UK across all Divisions. A collision raises an error and aborts. Report it; do not tweak the code.

**A&A serial (BUD-053):** `coalesce(max(serial_no),0)+1 FROM admin_approval WHERE division_office_id = office`, taken after the advisory lock in §3.4.
**TS serial:** `coalesce(max(t.serial_no),0)+1 FROM technical_sanction t JOIN work w ON w.id = t.work_id WHERE w.division_id = d`, taken after the lock.
**TS ceiling (VR-02 / DB-09):** for each AA-tab TS, pre-check `(Σ current-version ts_amount of the approval's AA-tab TS + new) × 100 <= TS_CEILING_PCT × current-version amount_sanctioned`. The deferred trigger re-checks at commit. Exactly 105 % passes.
**Delegation (BUD-055):** `exceeds_delegation = (limit in force for competent_authority on date_of_approval is set AND amount_sanctioned > limit)`. When true, `delegation_justification` must be non-blank (CHECK).
**Dates:** `date_of_approval` and `ts_date` must be on or before the IST date of `created_at` (CHECK; P7).
**Work columns the handlers leave at their default, set the same way here:** `pre_ddd false` (an INSERT with `true` is refused by `trg_work_pre_ddd_one_way`), `div_section_id NULL`, `original_budget/revised_budget/proposed_budget ''` (NOT NULL text), all EF shadow columns (`MajorHeadMajorAccCode` …) NULL, `estimated_cost NULL` unless stated. `remarks = BATCH` (NOT NULL text; marks the row for later identification). The six segments are copied from the chosen head (WorkRules.ApplySegments), and `trg_work_head_not_for_use` re-checks NFU.

### 8.2 Step 4a: two placeholder attachments (append-only)

| original_file_name | file_extension | content_type | evidence_class | size_bytes | sha256_digest | scan_engine | scanned_at | storage_key |
|---|---|---|---|---|---|---|---|---|
| `DUMMY-SEED-AA-COPY-PLACEHOLDER.pdf` | `pdf` | `application/pdf` | `EVIDENCE` | 1 | `encode(sha256(convert_to('DUMMY-SEED:20261022-01:AA','UTF8')),'hex')` | `DUMMY-SEED: no file stored, not scanned` | `now()` | `gen_random_uuid()` |
| `DUMMY-SEED-TS-COPY-PLACEHOLDER.pdf` | same | same | same | 1 | same, with suffix `:TS` | same | same | same |

Guard: `NOT EXISTS (original_file_name = x AND scan_engine = 'DUMMY-SEED: no file stored, not scanned')`. All A&A versions share the first row and all TS versions share the second. The FK index is not unique, so sharing is allowed. VR-10 "uploaded by the actor" holds because `created_by` is the seed actor.

### 8.3 Step 4b: works with A&A, A&A header and version 1 (CreateAdminApprovalHandler, "new work" path)

One item = one work + one `admin_approval` + one `admin_approval_version` (v1).
Work: `work_class 'AA'`, `entered_via 'AA_ENTRY'`, `user_code 'BUDAA'`, interim code.
Header: `division_office_id` = the slot's DIVISION office, serial as in §8.1, `current_version_no 1`.
Version: `version_no 1`, `name_of_work = work_desc`, `account_code_id` = the work's head, `aa_copy_attachment_id` = the AA placeholder.
Guard: `NOT EXISTS (work WHERE division_id = d AND work_desc = x)`. If the work exists but has no approval, raise an exception and report; do not repair.
BUD-059 (an auto WORK line when the WORKS proposal is at S-01/S-04) does **not** fire, because no proposal exists yet (phase 5). That matches the handler in this order.

| Slot | Div | work_desc | Amount ₹ | Authority | date_of_approval | aa_reference_no | eoffice_ref | exceeds |
|---|---|---|---|---|---|---|---|---|
| AA-01 | D1 | `DUMMY-AA-01 Strengthening of inspection road along canal, km 2 to 4` | 18,50,000 | EE | 2026-05-12 | `DUMMY-AA/2026-27/AA-01` | `DUMMY-EOFF/AA-01` | false |
| AA-02 | D2 | `DUMMY-AA-02 Replacement of gate hoist motors at regulator` | 42,00,000 | SE | 2026-05-20 | `DUMMY-AA/2026-27/AA-02` | `DUMMY-EOFF/AA-02` | false |
| AA-03 | D3 | `DUMMY-AA-03 Repair of staff quarters block B` | 12,00,000 | EE | 2026-06-03 | `DUMMY-AA/2026-27/AA-03` | `DUMMY-EOFF/AA-03` | false |
| AA-04 | D4 | `DUMMY-AA-04 Desilting of feeder channel, reach 3` | 85,00,000 | SE | 2026-06-18 | `DUMMY-AA/2026-27/AA-04` | `DUMMY-EOFF/AA-04` | false |
| AA-05 | D5 | `DUMMY-AA-05 Rewinding of generator unit stator` | 1,60,00,000 | CE | 2026-07-01 | `DUMMY-AA/2026-27/AA-05` | `DUMMY-EOFF/AA-05` | false |
| AA-06 | D6 | `DUMMY-AA-06 CCTV surveillance at control room` | 9,75,000 | EE | 2026-07-15 | `DUMMY-AA/2026-27/AA-06` | `DUMMY-EOFF/AA-06` | false |
| AA-07 | D7 | `DUMMY-AA-07 Protection wall downstream of spillway` | 2,40,00,000 | CE | 2026-08-01 | `DUMMY-AA/2026-27/AA-07` | `DUMMY-EOFF/AA-07` | false |
| AA-08 | D8 | `DUMMY-AA-08 Upgradation of 66 kV switchyard protection relays` | 38,50,000 | EE | 2026-08-20 | `DUMMY-AA/2026-27/AA-08` | `DUMMY-EOFF/AA-08` | **true**, justification `DUMMY-Justification: approved above EE delegated power for test of BUD-055` |

Each `exceeds` value must equal the §8.1 formula computed live. A mismatch means the live limits differ from phase 2: raise an exception and report.

### 8.4 Step 4c: two revisions (ReviseAdminApprovalHandler)

Each revision: insert `admin_approval_version` with `version_no = current + 1` (DB-06 refuses gaps), then `UPDATE budget.admin_approval SET current_version_no = 2 WHERE admin_approval_id = … AND current_version_no = 1` (header row locked `FOR UPDATE` first, as the handler does). The deferred DB-06 check confirms that header = max version at commit.
Guard: `NOT EXISTS (admin_approval_version WHERE admin_approval_id = a AND version_no = 2)`.
No correction flags arise. The handler raises AA_REDUCED / AA_HEAD_CHANGED only when a live demand line of the work exists, and none exists before phase 5.

| Slot | v2 amount ₹ | Authority | date_of_approval | aa_reference_no | Head | exceeds |
|---|---|---|---|---|---|---|
| AA-02 | 47,50,000 (upward) | SE | 2026-08-15 | `DUMMY-AA/2026-27/AA-02/R1` | unchanged | false |
| AA-05 | 1,45,00,000 (downward) | CE | 2026-09-10 | `DUMMY-AA/2026-27/AA-05/R1` | unchanged | false |

The other v2 columns: `name_of_work` and `eoffice_ref` are copied from v1, and `aa_copy_attachment_id` is the placeholder.

### 8.5 Step 4d: technical sanctions (CreateTechnicalSanctionHandler)

AA tab: header `tab 'AA'`, `admin_approval_id`, `work_id` = the approval's work, serial as in §8.1, `current_version_no 1`, `estimate_code NULL` and `estimate_code_at NULL` (DB-05 refuses anything else). Version v1: `ts_copy_attachment_id` = the TS placeholder.
AMR tab: in the same item, a new work with `work_class 'AMR'`, `entered_via 'TS_ENTRY'`, `user_code 'BUDTS'`, interim code; then a header with `tab 'AMR'` and `admin_approval_id NULL`. `ux_technical_sanction_amr_work` allows one TS per AMR work.
Guard: AA tab `NOT EXISTS (tsv.ts_number = x JOIN ts ON admin_approval_id = a)`. AMR tab: the work natural key `(division_id, work_desc)`, plus its TS.

| Slot | Tab | On | name_of_estimate | ts_number | ts_date | ts_amount ₹ | % of current A&A |
|---|---|---|---|---|---|---|---|
| TS-01 | AA | AA-01 | `DUMMY-Estimate: inspection road km 2-4` | `DUMMY-TS/2026-27/TS-01` | 2026-07-10 | 17,90,000 | 96.8 |
| TS-02 | AA | AA-02 | `DUMMY-Estimate: gate hoist motors` | `DUMMY-TS/2026-27/TS-02` | 2026-09-01 | 43,00,000 | 90.5 of v2 (102.4 of v1) |
| TS-03 | AA | AA-03 | `DUMMY-Estimate: staff quarters block B` | `DUMMY-TS/2026-27/TS-03` | 2026-07-20 | **12,48,000** | **104.0** (just under the 105 ceiling) |
| TS-04 | AA | AA-04 | `DUMMY-Estimate: desilting reach 3` | `DUMMY-TS/2026-27/TS-04` | 2026-08-05 | 80,00,000 | 94.1 |
| TS-05 | AMR | new work in D5: `DUMMY-AMR-01 Annual maintenance of colony roads 2026-27` | `DUMMY-Estimate: AM&R colony roads` | `DUMMY-TS/2026-27/TS-05` | 2026-06-25 | 6,40,000 | n/a |
| TS-06 | AMR | new work in D6: `DUMMY-AMR-02 Annual repair of drainage gallery pumps 2026-27` | `DUMMY-Estimate: AM&R gallery pumps` | `DUMMY-TS/2026-27/TS-06` | 2026-07-30 | 3,25,000 | n/a |

No TS revision is seeded, because the task does not ask for one.

### 8.6 Step 4e: works without A&A (CreateWorkHandler / Works master path)

`work_class 'AA'`, `entered_via 'LEGACY_OPENING'` (set server-side by the handler), `user_code 'DUMMY'` (the handler takes this from the client), interim code, `estimated_cost` ≤ NO_AA_LIMIT_RUPEES.
Handler precondition (WorkRules.CheckFirstCycleAsync): the first cycle exists and is OPEN, and the Division's WORKS proposal is absent or has `first_submitted_at` NULL. Phase 3 and P5 make this true.
Guard: `(division_id, work_desc)`.

| Slot | Div | work_desc | estimated_cost ₹ |
|---|---|---|---|
| NOAA-01 | D1 | `DUMMY-NOAA-01 Minor repair of office roof leakage` | 9,500 |
| NOAA-02 | D2 | `DUMMY-NOAA-02 Replacement of damaged door locks, store` | 6,800 |
| NOAA-03 | D3 | `DUMMY-NOAA-03 Fixing of name boards at site` | 9,800 |
| NOAA-04 | D4 | `DUMMY-NOAA-04 Repair of hand pump, colony` | 10,000 (exactly at the limit; VR-03 refuses only above it) |

### 8.7 Step 4f: `budget.estimate` (legacy, CreateEstimateHandler: a duplicate-key check only)

| estimate_code | Work | estimate_amt ₹ | estimate_desc / estimate_narr |
|---|---|---|---|
| `DUMMY-E01` | AA-01 | 17,90,000 | `DUMMY-Estimate for AA-01` / `DUMMY-Narration: test data, BATCH` |
| `DUMMY-E02` | AA-02 | 43,00,000 | same pattern |
| `DUMMY-E03` | AA-03 | 12,48,000 | |
| `DUMMY-E04` | AA-04 | 80,00,000 | |
| `DUMMY-E05` | AMR-01 | 6,40,000 | |
| `DUMMY-E06` | AMR-02 | 3,25,000 | |
| `DUMMY-E07` | NOAA-01 | 9,500 | |
| `DUMMY-E08` | NOAA-02 | 6,800 | |

`fin_year_end 2027`. `division_code` = `division.division_code` (the **code**, not the id: GetEstimateList joins on `DivisionCode`). `estimate_code` is varchar(10), and `DUMMY-E01` is 9 characters.
**`estimate.work_code` holds `work.id`, not `work.work_code`.** `heirarchyKeysRefactor` re-pointed `FK_estimate_work_work_code` to `work(id)` (P9 / OI-2). If the live FK still targets `work(work_code)`, store `work.work_code` instead and report which one was used.
Guard: PK `(work_code, estimate_code)`.

### 8.8 Step 4g: `budget.budget_table` (legacy, CreateBudgetEntryHandler)

`calendar_year 2027` and `user_code 'DUMMY'`. The six segments come from §4.3, matched by type.
`sequence_no` = `coalesce(max(sequence_no),0)+1` per `(division_id, calendar_year)`. The handler takes it from the client; this rule keeps the UK `(division_id, calendar_year, sequence_no)` free.
`div_section_id` = the lowest `div_section.id` of that division where `status` is true, else 0. The column has no FK. The handler always stores 0 (G-2).
**`work_id` = the slot Division's AA work, never NULL.** GetBudgetEntryListHandler dereferences `b.Work.WorkCode` in memory, so a row with no work makes the whole list return 500 (G-1).
Guard: `(division_id, calendar_year, budget_description)`.

| # | Type | Div | budget_description | budget_amount ₹ | revised_amount ₹ | proposed_budget ₹ |
|---|---|---|---|---|---|---|
| OE-01 | 1 | D1 | `DUMMY-OE-01 Office contingencies` | 4,50,000 | 4,80,000 | 5,10,000 |
| OE-02 | 1 | D2 | `DUMMY-OE-02 Telephone and internet charges` | 1,20,000 | 1,35,000 | 1,40,000 |
| OE-03 | 1 | D3 | `DUMMY-OE-03 Electricity charges, office` | 6,00,000 | 6,40,000 | 6,75,000 |
| OE-04 | 1 | D4 | `DUMMY-OE-04 Stationery and printing` | 85,000 | 90,000 | 95,000 |
| OE-05 | 1 | D5 | `DUMMY-OE-05 POL for departmental vehicles` | 3,60,000 | 3,90,000 | 4,10,000 |
| OE-06 | 1 | D6 | `DUMMY-OE-06 Minor office repairs` | 2,00,000 | 2,25,000 | 2,40,000 |
| RC-01 | 2 | D1 | `DUMMY-RC-01 Rent recovered from staff quarters` | 2,40,000 | 2,55,000 | 2,70,000 |
| RC-02 | 2 | D2 | `DUMMY-RC-02 Sale of tender documents` | 60,000 | 65,000 | 70,000 |
| RC-03 | 2 | D3 | `DUMMY-RC-03 Water charges recovered` | 1,80,000 | 1,90,000 | 2,00,000 |
| RC-04 | 2 | D4 | `DUMMY-RC-04 Sale of scrap` | 3,50,000 | 3,20,000 | 3,00,000 |
| RC-05 | 2 | D5 | `DUMMY-RC-05 Licence fee, colony shops` | 1,20,000 | 1,25,000 | 1,30,000 |
| RC-06 | 2 | D6 | `DUMMY-RC-06 Penalty recovered from contractors` | 75,000 | 80,000 | 85,000 |

`budget_type` is the `BudgetType` enum: 0 Work, **1 Expensive (office expense)**, **2 Receipt**.

---

## 9. Phase 5: WORKS proposals at S-01 with demand lines

### 9.1 Is it consistent? Yes, within the limits below

- **Opening:** `fn_proposal_insert_rules` (DB-01, DB-10) accepts a proposal created at `S-01` with `holder = origin`, for a WORKS proposal at a DIVISION office. Opening writes **no movement** and is not a state transition. OpenProposalsHandler opens WORKS for **every active DIVISION office**. After phase 1 that is exactly D1–D8, so this matches the handler. If more DIVISION offices exist by run time, open for the 8 slots only and report the others.
- **Lines:** DB-11 needs the cycle OPEN, the state `figures_open` (S-01 is), and `actor_office_id = holder_office_id`. DB-01, DB-03 and DB-04 hold by construction. `fn_capture_figure_version` writes the RBE and OBE_NEXT versions automatically (`change_source 'ENTRY'`, because holder = origin).
- **Not done:** S-02 and later states, movements, observations, the edit lease (`edit_lease_by`/`expires_at`). The lease is transient (20 min) and would only lock out a real user, so it is left NULL. `first_submitted_at` stays NULL.

### 9.2 Inserts

1. `LOCK TABLE budget.proposal IN SHARE ROW EXCLUSIVE MODE;` then, per slot: `INSERT proposal (cycle_id, category 'WORKS', origin_office_id = holder_office_id = DIVISION office, state_code 'S-01')`. Guard: the UK `(cycle_id, category, origin_office_id)`. `actor_office_id` is IT Cell here.
2. Per proposal: `SELECT set_config('budget.actor_office_id', <that DIVISION office_id>, true);`, `SELECT … FOR UPDATE`, and a re-check that the state is `S-01` and holder = origin (if not, skip that proposal and report). Then insert the lines. After the last proposal, set `actor_office_id` back to IT Cell.
3. Phase-5 new works (DEMAND_ENTRY path of AddDemandLineHandler) for D7 and D8. Insert the work immediately before its line: `work_class 'AA'` with no A&A, `entered_via 'DEMAND_ENTRY'`, `user_code 'BUDDE'`, interim code, guard `(division_id, work_desc)`.

Line columns: `serial_no = coalesce(max,0)+1` per proposal (BUD-079). For WORK lines, `account_code_id` = the head the work's six segments resolve to (§5.5 row i; M-22 otherwise). For UNFORESEEN lines, `account_code_id NULL`, `work_id NULL` and `unforeseen_name` set (CHECKs). Also `added_on_return false`, `is_deleted false`, `pct_check_not_applied false` (every percentage base is positive).
**`floor_check_unchecked = true` on every line.** VR-15: the category is not RECEIPTS, `rbe` is set, and no UP_TO_DATE/UP_TO_MONTH expenditure row exists for these works or heads (P10), so the handler flags it as unchecked.
Line guards: WORK `NOT EXISTS (proposal_id, work_id, NOT is_deleted)` (also the partial UK `ux_demand_line_work`). UNFORESEEN `NOT EXISTS (proposal_id, line_type 'UNFORESEEN', unforeseen_name)`.

### 9.3 Lines (24 in total, 3 per proposal). Figures in whole ₹

| Div | Line | Work / name | rbe | obe_next | Rule kept clear |
|---|---|---|---|---|---|
| D1 | 1 | AA-01 | 6,00,000 | 9,50,000 | VR-04: 15.5 L ≤ 110 % × 18.5 L |
| D1 | 2 | NOAA-01 | 8,000 | 9,500 | VR-03: each ≤ 10,000 |
| D1 | 3 | UNF `DUMMY-UNF-D1 Unforeseen works` | 50,000 | 75,000 | none applies |
| D2 | 1 | AA-02 | 15,00,000 | 22,00,000 | 37 L ≤ 110 % × 47.5 L |
| D2 | 2 | NOAA-02 | 6,500 | 9,000 | VR-03 |
| D2 | 3 | UNF `DUMMY-UNF-D2 Unforeseen works` | 50,000 | 75,000 | |
| D3 | 1 | AA-03 | 4,00,000 | 6,50,000 | 10.5 L ≤ 13.2 L |
| D3 | 2 | NOAA-03 | 9,000 | 9,800 | VR-03 |
| D3 | 3 | UNF `DUMMY-UNF-D3 Unforeseen works` | 50,000 | 75,000 | |
| D4 | 1 | AA-04 | 25,00,000 | 45,00,000 | 70 L ≤ 93.5 L |
| D4 | 2 | NOAA-04 | 7,200 | 10,000 | VR-03 (equal to the limit passes) |
| D4 | 3 | UNF `DUMMY-UNF-D4 Unforeseen works` | 50,000 | 75,000 | |
| D5 | 1 | AA-05 | 40,00,000 | 70,00,000 | 1.10 Cr ≤ 110 % × 1.45 Cr (v2) |
| D5 | 2 | AMR-01 | 3,00,000 | 3,40,000 | VR-05 "neither" branch: `justification_text = 'DUMMY-Justification: recurring annual maintenance; no preceding AM&R work recorded'` (required at Submit, set now) |
| D5 | 3 | UNF `DUMMY-UNF-D5 Unforeseen works` | 50,000 | 75,000 | |
| D6 | 1 | AA-06 | 3,50,000 | 5,00,000 | 8.5 L ≤ 10.725 L |
| D6 | 2 | AMR-02 | 1,50,000 | 1,75,000 | same justification as D5-2 |
| D6 | 3 | UNF `DUMMY-UNF-D6 Unforeseen works` | 50,000 | 75,000 | |
| D7 | 1 | AA-07 | 60,00,000 | 1,20,00,000 | 1.8 Cr ≤ 2.64 Cr |
| D7 | 2 | DE-01 (new) `DUMMY-DE-01 Replacement of broken window panes, site office` | 6,000 | 8,500 | VR-03 |
| D7 | 3 | UNF `DUMMY-UNF-D7 Unforeseen works` | 50,000 | 75,000 | |
| D8 | 1 | AA-08 | 12,00,000 | 20,00,000 | 32 L ≤ 42.35 L |
| D8 | 2 | DE-02 (new) `DUMMY-DE-02 Painting of gauge posts` | 5,500 | 7,000 | VR-03 |
| D8 | 3 | UNF `DUMMY-UNF-D8 Unforeseen works` | 50,000 | 75,000 | |

All lines save with no warning under the Save-moment rule set (DemandLineRuleEvaluator). Re-evaluate them against the P3 live settings if those differ.

---

## 10. Not done in SQL, or done with a disclosed compromise

| Item | Status | Reason |
|---|---|---|
| States S-02…S-16, `proposal_movement`, observations, returns | **Skipped** | Out of scope by instruction. A state change needs a movement row in the same transaction (DB-10, composite FK to `state_transition`). S-12…S-16 have no endpoints at all |
| `audit_log`, `figure_version` rows | Trigger-written only | Hard rule 2 |
| `technical_sanction.estimate_code` | **Left NULL** | Only `budget.fn_record_estimate_code`, called by Financial Accounting (`bud_fa`, TSN-009), may write it. It is not a Budget-app action |
| Real A&A / TS copy files | **Placeholder rows only** | No access to the API's file store. Downloading a copy answers `INTEGRITY` and logs at Error level "stored file … is missing" (DownloadAttachmentHandler). Tell testers not to open copies of `DUMMY-` approvals |
| Attribution | **Single seed officer, role `DUMMY_SEED`** | The handlers would write A&A/TS/works/lines as the Division DDO, the cycle/limits as AFA_BUDGET, and offices as IT_CELL. No new personas may be created, so the audit trail correctly names one officer. During the line inserts it shows that officer acting from each Division office (DB-11) |
| Edit lease on proposals | Not set | Transient; it would block real users |
| Head-wise proposals (DNA/RECEIPTS/CAPEX), account-head assignments, opening/expenditure figures, CE recommendations, distributions | Not in scope | The task covers WORKS only. Head-wise proposals need `account_head_assignment` first (VR-06) |
| `pre_ddd = true` works | Impossible | `trg_work_pre_ddd_one_way` refuses an INSERT with `true` |
| New divisions, CEs, employees, charges, SSO users | Not created | Hard rule 5. Viewing the data needs an existing user (see §11.2) |
| **Removal later** ("baad mein remove kar denge") | **Not possible by DELETE** | `fn_forbid_change` blocks DELETE/TRUNCATE on every budget-process table, for the owner too. See §12 for what can be neutralised |

---

## 11. Verification

### 11.1 `90_verify.sql` (read-only; record the results in `REPORT.md`)

- Counts created by this batch: `budget.audit_log WHERE action_code = 'DUMMY-SEED:20261022-01' GROUP BY table_name, operation`. Expect roughly 230 rows. Offices 16, delegation 4, cycle 1, calendar 5, attachments 2, works 16, A&A 8 + versions 10 + header updates (≈4 column rows each), TS 6 + versions 6, proposals 8, lines 24, figure_version 48. FY, estimate and budget_table have no audit trigger.
- `budget.office`: FINANCE 1, SECRETARIAT 1, IT_CELL 1, CE 2, CIRCLE 4, DIVISION 8; each parent chain resolves to FINANCE.
- Every seeded A&A has `current_version_no = max(version_no)`. AA-02 and AA-05 are at v2. AA-08 is `exceeds_delegation`.
- TS-03 is at 104.0 % of AA-03. The Σ check holds for every AA-tab approval.
- 8 proposals at S-01 with 3 lines each, and 2 `figure_version` rows per line.
- `budget_table`: 6 rows with `budget_type = 1` and 6 with `budget_type = 2` for `calendar_year 2027`, all with `work_id` set. `estimate`: 8 rows.
- Re-run the real run once more. It must report "skipped" for every item and create 0 rows; `audit_log` count unchanged.

### 11.2 What the screens will show (not verifiable from SQL)

Every Budget GET resolves an actor and fails closed without one. Data appears only for a logged-in user whose token has a Budget role and who has a current `'Budget'` charge that resolves to an office (BudgetOfficeScope).
- AFA_BUDGET or FIN_VIEW (FINANCE office) and SECRETARY (SECRETARIAT) see everything.
- A DDO sees only their own Division, which works only if their charge's `division_id` is one of D1–D8. §4.1 rank 1 favours that.
- An IT_CELL user sees only the IT Cell office, so they see no proposals, A&As or works.

Also from the plan report, not re-checked here: Office Expenses and Receipts need the FE route `/api/budget-entries` → `/api/budgets`, and the Works screen's `ApiEndPoints.AA` was undefined.

---

## 12. Permanence and later neutralisation (not removal)

These steps would use allowed UPDATEs on seeded rows only, and only if the user later asks. They are not part of this run.
- Cycle: `status = 'CLOSED'`. The current-cycle query then ignores it, but the 2027 and first-cycle UK slots stay used.
- Demand lines: `is_deleted = true` (one-way, DB-13), and only while the proposal is at S-01/S-04 with the matching actor office.
- Offices: `is_active = false` is technically possible, but these are real structure rows. Keep them.
- A&A, TS, attachments, delegation limits, works, `audit_log`: these cannot be changed or removed (append-only or forbidden).
- `budget_table`/`estimate`: the code has no forbid trigger on them (confirm in P8). A DBA could delete these by WHERE `user_code = 'DUMMY'` / `estimate_code LIKE 'DUMMY-%'`, but this seed never does.

---

## 13. Open items for the coder (confirm against the live schema during the dry run)

| ID | Item | Owner |
|---|---|---|
| OI-1 | Actor chosen by C1 or C2: employee code and name, project `Budget`, current | coder → report |
| OI-2 | `FK_estimate_work_work_code` target column (`id` expected) | coder (P9) |
| OI-3 | Triggers on `fa_financial_years`, `estimate`, `budget_table` (none expected) | coder (P8) |
| OI-4 | Live setting values (P3). Adjust nothing silently | coder |
| OI-5 | Hierarchy shape and selection (§4.1), and the wing per CE (§4.2) | coder → report |
| OI-6 | Head choice per class (§4.3), including any "head class assumed" | coder → report |
| OI-7 | Column list and nullability of every written table (`\d`) against §5–§9 | coder (P8) |
| OI-8 | BBMB's real head classes and delegation amounts. These are dummy here, and real values come from BBMB Finance (plan OD-4) | user |

---

## 14. Gaps found while designing (raise; do not work around silently)

- **G-1** `GetBudgetEntryListHandler` builds `WorkCode = b.Work.WorkCode` in memory. Any `budget_table` row with `work_id` NULL makes the list throw a NullReferenceException (500). The seed links a work to every entry. Backend fix: `b.Work?.WorkCode`.
- **G-2** `CreateBudgetEntryDto.DivSection` does not map to `BudgetEntry.DivSectionId` (Mapster name mismatch), so the API always stores `div_section_id = 0`.
- **G-3** `estimate.work_code` is an FK to `work.id`, despite its name. A client that posts a real work code gets an FK error or links the wrong work.
- **G-4** The placeholder attachments produce `INTEGRITY` errors in the logs if anyone opens them. Keep this in mind if any log alerting watches for `INTEGRITY`.
- **G-5** Dev and Staging share `BBMB`. This seed permanently uses up the first-cycle and 2026-27 cycle slots there, so UAT of cycle creation needs a separate database (plan OD-1).
- **G-6** In this checkout, `appsettings.json` still holds the placeholder `BudgetProcess:ProjectCode`. The DB's charges use `'Budget'`. An API built from this checkout answers M-20 and shows no data until the deployed setting is `Budget`.

---

## 15. Summary

- **Actor:** the IT Cell officer whose current `'Budget'` charge ran the Budget migrations on `BBMB`, taken from the `MIGRATION:%` audit rows. The fallback is the earliest current head-office `'Budget'` charge. Role code `DUMMY_SEED`, office IT Cell (the Division office only while demand lines are written). The coder reports it by employee code and name.
- **Phases:** all five can be done consistently in SQL.
  - Phase 1 loads 16 offices (Finance, Secretariat, 2 CE, 4 circles, 8 divisions).
  - Phase 2 adds 4 delegation limits.
  - Phase 3 adds FY 2027/2028 if missing, 1 OPEN first cycle 2026-27 → 2027-28, and 5 calendar rows.
  - Phase 4 adds 8 A&As (2 revised, 1 above delegation), 6 TS (4 AA-tab including one at 104 %, 2 AMR), 4 works without A&A, 8 estimates and 6+6 legacy budget entries. It depends on two placeholder attachments.
  - Phase 5 opens 8 WORKS proposals at S-01 with 3 lines each.
- **Expected to be skipped:** nothing is skipped as a whole phase. Within phases, `estimate_code` on TS, real copy files, the edit lease, and everything beyond S-01 are left out (§10).
- **Stop conditions:** no current `'Budget'` charge, an existing 2027 or first cycle, or a live schema that differs from §5–§9. In any of these cases the coder stops and reports instead of adapting.
