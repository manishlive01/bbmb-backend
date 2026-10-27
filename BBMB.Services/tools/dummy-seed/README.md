# BBMB Budget dummy seed — batch `DUMMY-SEED:20261022-01`

Dummy test data loaded on 2026-10-22 into the **shared** `BBMB` database (Dev + Staging, host `10.43.18.13`). The user authorised direct writes and accepted that the rows and their audit entries are effectively permanent. The data was written by hand-written SQL through psql (dotnet and Docker were not available) and follows `DESIGN.md`, apart from the deviations listed below.

## What was loaded

All counts come from `verify.sql` after the real run (`logs/04_verify.log`).

| Table | Rows | Notes |
|---|---|---|
| `budget.office` | 16 | FINANCE, SECRETARIAT, 2 CE, 4 CIRCLE, 8 DIVISION (ids 18–33). IT Cell (id 1) already existed |
| `budget.delegation_limit` | 4 | EE ₹25,00,000 · SE ₹1,00,00,000 · CE ₹5,00,00,000 · SECRETARY ₹25,00,00,000, `effective_from` 2000-01-01. These are dummy amounts, not BBMB's delegation of powers |
| `budget.fa_financial_years` | 2 | 2027 (2026-27) and 2028 (2027-28). The table has no name column, so the rows are untagged |
| `budget.budget_cycle` | 1 | cycle 2: 2027 → 2028, first cycle, `OPEN` |
| `budget.calendar` | 5 | DIVISION 2026-11-15, CIRCLE 2026-11-30, CE 2026-12-15, SECRETARIAT 2026-12-31, FINANCE 2027-01-15 |
| `budget.attachment` | 2 | Placeholder A&A and TS copies. **No file is stored** |
| `budget.work` | 16 | 8 AA_ENTRY, 2 TS_ENTRY (AMR), 4 LEGACY_OPENING (no A&A, ≤ ₹10,000), 2 DEMAND_ENTRY. All have `work_code` NULL |
| `budget.admin_approval` | 8 | AA-02 and AA-05 are at version 2. AA-08 has `exceeds_delegation = true` |
| `budget.admin_approval_version` | 10 | 8 v1 + 2 v2 |
| `budget.technical_sanction` | 6 | 4 AA tab, 2 AMR tab. TS-03 is at 104.0 % of AA-03 (ceiling 105) |
| `budget.technical_sanction_version` | 6 | |
| `budget.estimate` | 0 | Skipped (see below) |
| `budget.budget_table` | 12 | 6 office expense (`budget_type` 1) and 6 receipt (`budget_type` 2), `calendar_year` 2027, each linked to a work |
| `budget.proposal` | 8 | WORKS, `S-01`, held by origin (ids 9–16) |
| `budget.demand_line` | 24 | 3 per proposal: AA work, no-A&A/AMR/DE work, and UNFORESEEN. All `floor_check_unchecked` |
| `budget.figure_version` | 48 | Trigger-written (RBE + OBE_NEXT per line, `change_source ENTRY`). Not inserted by the seed |
| `budget.audit_log` | 162 | Trigger-written. 154 INSERT rows + 8 UPDATE rows (the two A&A header version bumps, 4 columns each) |

## Actor

Rule C1 in DESIGN §2.2 picks the IT Cell officer whose current `'Budget'` charge ran the Budget migrations. That officer is employee **900001, "TEST Migration Officer (dev only)"**: a head-office charge (`ce_id 0`). It is the only current `'Budget'` charge on the database. Every row records role `DUMMY_SEED`, IP `0.0.0.0`, and `action_code` = `justification` = `DUMMY-SEED:20261022-01`. The office recorded is IT Cell (1). The exception is the demand-line inserts, which record the proposal's Division office (26–33), because DB-11 (M-20) requires the actor office to be the holder. Audit split: 88 rows IT_CELL, 74 rows DIVISION. UUIDs are not recorded here.

## Hierarchy, heads and wing (selection made by the seed)

| Slot | Division (code) | Circle (code) | CE (code) |
|---|---|---|---|
| D1, D2 | 241 XEN O&M Jalandhar, 242 XEN/O&M Jamalpur | 240 SE/O&M Jamalpur | 200 CE/TS Chandigarh |
| D3, D4 | 251 XEN/O&M Panipat, 254 XEN/O&M Delhi | 250 SE/O&M Panipat | 200 |
| D5, D6 | 331 RE/BPHC(Left) Gr-1, 332 RE/BPHC(Right) Gr-1 | 330 SE/BPHC Nangal | 300 CE/Generation Nangal |
| D7, D8 | 341 RE Ganguwal-Kotla, 342 XEN/O&M (Kotla&GWL) | 340 SE/CPHC Nangal | 300 |

- **Why these CEs.** No master was referenced by a current `'Budget'` charge (rank 1). CE 206 "testing" holds the 6 existing pre-DDD works (rank 2), but it has only one circle, so it cannot give the 2×2×2 shape. The shape was therefore applied as an eligibility filter, and the lowest id decided (rank 3). Consequence: the 6 existing works in Divisions 555/888/777 still have no office and stay hidden in the Works list.
- **Wing assumed.** Neither CE name contains "power" or "irrig", so both got the lowest code, `1` (Irrigation). CE/TS and CE/Generation are more likely Power (`2`). Correct this in the app with `PUT /api/budget-offices/{id}/wing` (offices 20, 21) if needed.
- **Heads (class assumed, OI-6/OI-8).**
  - Works use the three 2801 heads of the existing works (…-12-511-274, …-13-511-252, …-14-511-251), assigned round-robin. The 1001 "testing account head" is receipt-range, so it was not used for works. No 4xxx/5xxx capital head exists.
  - Office expense uses the first 6 heads in the 2000–3999 range (2801-01-205-11-517-251/252/257/258/260/261).
  - Receipts use the only 0001–1999 head, 1001-11-301-05-801-117, for all 6 rows.

## Deviations from DESIGN.md (forced by the live schema)

The live DB is at **P4/P5**: `__EFMigrationsHistory` has `BudgetP4WorkCodeConformance` and `BudgetP5Reappropriation`. DESIGN.md was written against the P1–P3 handlers in the `feature/budget-lifecycle-api` checkout. The handler logic was therefore taken from the deployed branch (`budget_v1`, worktree `.worktrees/budget-lifecycle-api`).

1. **No interim work code.** `work.work_code` is now `varchar(12)`, nullable, with `ck_work_code_format` (GENERATED `ddd-dddd-ddd` or LEGACY 7 digits). The P4 handlers insert works with `work_code = NULL` (DV-19), and a code is generated only on allocation. The seed does the same. DESIGN §8.1 "interim work code" is obsolete.
2. **Estimates skipped (DESIGN §8.7).** The P4 `CreateEstimateHandler` refuses every create with M-19. Estimate codes are recorded only by Financial Accounting against a TS. `estimate.work_code` is an `integer NOT NULL` that a code-less P4 work cannot supply, so any value would have been fabricated.
3. **`div_section_id`.** The seed stored the lowest active `div_section` of the division (DESIGN §8.8), not the 0 that the API stores (G-2). D1–D5 got sections 14/17/27/36/53. D6 has no active section, so its rows got 0.

## Skipped, with reasons

| Item | Reason |
|---|---|
| Legacy `budget.estimate` (8 planned) | Refused by the deployed handler (M-19). See deviation 2 |
| Proposal states S-02+, movements, observations, edit lease | Out of scope. A state change needs a `proposal_movement` row, which only the handlers write |
| `technical_sanction.estimate_code` | Only `budget.fn_record_estimate_code` (Financial Accounting) may write it |
| Real A&A/TS copy files | No access to the file store. Opening a copy of a `DUMMY-` approval returns `INTEGRITY` and logs "stored file … is missing" (G-4) |
| Head-wise proposals (DNA/RECEIPTS/CAPEX) | Not in scope. They need `account_head_assignment` first |
| Offices for the other CEs, circles and divisions | Subset by instruction. A later real `load-from-hierarchy` adds the rest; it is idempotent per master id |

## Evidence: commands run and results

All commands were run from this folder. The password went to psql only through `PGPASSWORD` in the child environment.

| # | Command | Result | Log |
|---|---|---|---|
| 1 | Read-only discovery (`\d` of every written table, trigger function bodies, settings, masters, charges) via `run_seed.py file <tmp>.sql` | Schema at P4/P5. Settings TS_CEILING_PCT 105, NO_AA_LIMIT 10000, AA_DEMAND_CEILING 110, AMR_INCREASE 5. 0 cycles, 0 proposals, 0 A&A, 0 expenditure rows, 1 office (IT Cell) | (temp files, not kept) |
| 2 | `python3 run_seed.py dry-run` | exit 0. 96 "created" notices. `SET CONSTRAINTS ALL IMMEDIATE` fired every deferred trigger, verify showed the counts above, then ROLLBACK. A follow-up count showed 1 office, 0 cycles and 0 batch audit rows, so nothing persisted | `logs/01_dry_run.log` |
| 3 | `python3 run_seed.py real` | exit 0. 5 phases, each `BEGIN … SET CONSTRAINTS ALL IMMEDIATE; COMMIT`. 96 created, 0 skipped | `logs/02_real_run.log` |
| 4 | `python3 run_seed.py dry-run` (re-run against the committed data) | exit 0. **0 created, 94 skipped** (idempotent), rolled back | `logs/03_rerun_dry_idempotency.log` |
| 5 | `python3 run_seed.py verify` | Counts as in the table above. Every A&A has current = max version. TS-03 is 104.0 %. 8 proposals at S-01 with 3 lines and 6 figure versions each | `logs/04_verify.log` |
| 6 | Precondition (V8 in verify.sql): `SELECT … FROM budget.budget_cycle WHERE status = 'OPEN'` | **1 row**: cycle 2, 2027 → 2028, first, OPEN | `logs/04_verify.log` |

## Files

- `seed.sql`: phased, idempotent seed. Every INSERT is guarded by `NOT EXISTS` on its natural key, and every phase sets the 11 actor keys with `set_config(…, true)`. Preflight checks P2–P11 abort before any write.
- `run_seed.py`: `dry-run` | `real` | `verify` | `file <sql>`. Reads `ConnectionStrings:DefaultConnection` from `../../BBMB.BudgetService/appsettings.json`, skipping `//` comment lines, without printing the password. Runs psql with `ON_ERROR_STOP=1`. Refuses `cleanup.sql`.
- `verify.sql`: read-only counts and checks, filtered to batch rows.
- `cleanup.sql`: **documentation only, never executed** (it `\quit`s on line 1).
- `logs/`: the run output listed above. It contains no secrets and no UUIDs.

## Re-running

`python3 run_seed.py dry-run`, then `python3 run_seed.py real`. On the current data both create nothing. Preflight P5 accepts the existing cycle only because its INSERT audit row carries this batch id. Any other first cycle or 2026-27 cycle makes the seed stop.

## Cleanup caveat

**These rows cannot be removed by DELETE.** `fn_forbid_change` blocks DELETE/TRUNCATE on every budget-process table, for the owner as well. Removal needs a superuser trigger bypass, which this tool will never do (see the header of `cleanup.sql`). `audit_log` keeps the history in any case.

Neutralise instead of delete (allowed UPDATEs on seeded rows, run only on request):
- close cycle 2 (`status = 'CLOSED'`);
- mark the 24 lines `is_deleted` while each proposal is at S-01, with `actor_office_id` set to its Division.

**Permanent side effects to be aware of (G-5):** cycle 2 uses up the `current_fin_year = 2027` and `is_first_cycle` unique slots on the shared DB, so the real first cycle 2026-27 can no longer be created on `BBMB`. Any real A&A above the dummy delegation limits will ask for a justification (M-27) until real limits are loaded with a later `effective_from`.

## Gaps raised (not worked around)

- **G-7 (new).** The `feature/budget-lifecycle-api` checkout (HEAD `1ead12c`) still has the P1–P3 `int` work-code handlers. The database is at P4/P5, so an API built from that checkout would fail on the `work_code` type and format. The deployed code is on `budget_v1`. Confirm which branch the Dev/Staging API runs.
- **G-1 / G-2 / G-4 / G-5 / G-6** from DESIGN §14 still apply. In particular, viewing the data needs a user whose current `'Budget'` charge resolves to one of these offices. None exists today except the IT Cell actor, who sees only IT Cell, so a Finance/AFA or Division DDO charge must be issued before the screens show anything.

## Batch -02 (dry-run personas), `DUMMY-SEED:20261022-02`
Added 2026-10-22 for the RTM-BUD-01 developer dry run (evidence: `BBMB-csv-validation/docs/evidence/RTM-BUD-01-dryrun-20261022/`). Script `personas.sql`, verification `verify_personas.sql`, verbs `personas-dry-run` / `personas-real` / `personas-verify` in `run_seed.py`. Logs `logs/05`–`08`.

| Code | Name | Charge (`project_code='Budget'`) | Resolves to |
|---|---|---|---|
| 900101 | `DUMMY-DDO D1 241 XEN O&M Jalandhar (dry run 20261022)` | CE 200 / circle 240 / division 241 master ids | office 26 (DIVISION) as DDO |
| 900201 | `DUMMY-SE Circle 240 SE/O&M Jamalpur (dry run 20261022)` | CE 200 / circle 240 | office 22 (CIRCLE) as SE |
| 900301 | `DUMMY-AFA Budget Finance (dry run 20261022)` | head-office form (`ce_id` 0) | office 18 (FINANCE) as AFA_BUDGET / FIN_VIEW |

- 3 rows in `employee.employee` and 3 in `employee.employee_charge`. Nothing in `budget.*` is written by this batch. `employee` has no triggers, so no audit row exists for these inserts. The `budget.*` actor keys are still set, by convention.
- Each `user_id` is a deterministic synthetic GUID that no real SSO user holds, so only a locally minted token can act as one of these personas. E-mails use `@example.invalid`.
- **Deviation from DESIGN rule 5** ("never create an employee or charge"). That rule governs batch -01. Creating test identities is the explicit purpose of batch -02, under the user instruction of 2026-10-22. The rows are in the **shared HR schema**, so other modules that list employees may show them (gap G-DR-2).
- Charges expire by time 30 days after creation (2026-11-21). To neutralise them earlier, see the batch -02 block in `cleanup.sql` (documentation only).
- The dry run also wrote business rows through the API: A&A 17, work 42, attachment 5 and demand line 49 on proposal 9, all named `DUMMY-DRYRUN-20261022 …`, plus trigger-written audit and figure rows. Opening the proposal screens took 19 edit leases on proposal 9 (`proposal` U `LEASE`). These rows are permanent (`fn_forbid_change`).
