# Dummy seed batch DUMMY-SEED:20261022-01 on the shared BBMB database

The batch loads a 2 CE × 2 circle × 2 division office subset, dummy delegation limits, the first OPEN cycle 2026-27 → 2027-28 with its calendar, 16 works with A&A/TS/no-A&A/demand-entry routes, 12 legacy budget entries, and 8 WORKS proposals at S-01 with 24 demand lines. It does this through hand-written psql (`seed.sql`) driven by `run_seed.py`, phase by phase, each phase committing on its own. Every write goes through the normal triggers, so audit_log and figure_version were written by the database, not the seed. The run is already committed and cannot be removed by DELETE, so this review checks the scripts and the recorded evidence, not a re-run. No hard rule was broken, and the recorded counts reconcile with the verify output.

Watch for: two `\quit <n>` guards that psql exits with status 0, so a STOP in `seed.sql` would look like success (likely); `run_seed.py` downgrades `verify-ca`/`verify-full` SSL modes to `prefer` (confirmed); `file` and `verify` modes are not enforced read-only (confirmed); `cleanup.sql` is guarded only by psql meta-commands, so a GUI client would execute its DELETEs (confirmed).

**Verdict**: APPROVED

## High-level view

The safety rules hold. `seed.sql` contains no trigger disable, no `session_replication_role`, no DELETE or TRUNCATE, and no INSERT into audit_log, figure_version, recommendation_version or proposal_movement. Its only UPDATE is `admin_approval.current_version_no = 2` on two approvals this batch created. P6 ensures any `DUMMY-` work the UPDATE could reach was written by this batch. Each write phase is its own `BEGIN … SET CONSTRAINTS ALL IMMEDIATE; COMMIT`, with the 11 actor GUCs set through `set_config(…, true)`.

Idempotency is demonstrated, not just claimed. Every INSERT has a NOT EXISTS guard on the natural key named in DESIGN.md. The real run created 96 rows and the dry re-run against committed data created 0 and skipped 94. The gap of 2 is the two DEMAND_ENTRY works, which have no separate skip notice. They are found by lookup before their line is skipped.

The recorded evidence reconciles. V1 in `logs/04_verify.log` sums to 162 audit rows (154 I + 8 U = 2 version bumps × 4 columns). The 74 rows attributed to DIVISION are exactly 24 lines + 48 figure versions + 2 DE works. Every amount is a whole rupee, AA-08 is the only `exceeds_delegation`, and TS-03 is at 104.0 % of a 105 % ceiling. The demand figures also pass VR-03 and VR-04 when recomputed by hand. The actor is identified as employee 900001 "TEST Migration Officer (dev only)", selected by rule C1. No password, UUID or connection string appears in the files or logs.

The remaining issues are in the tooling, not the committed data. Two `\quit` guards cannot set an exit status. SSL modes are mapped loosely. The read-only modes are read-only by convention only. `cleanup.sql` is protected by psql meta-commands that other clients ignore. The README has a few small inaccuracies, and one traceability gap is not stated: `fa_financial_years` and `budget_table` have no audit trigger.

<details>
<summary>Issues (8)</summary>

1. **`\quit 3` / `\quit 1` exit 0** — psql's `\quit` takes no exit status. The STOP paths in `seed.sql` (hier_ok, heads_ok) and the `cleanup.sql` header end psql with status 0 and a warning that the extra argument was ignored. Replace each with a statement that fails under ON_ERROR_STOP, e.g. `SELECT 1/0 AS stop;` or a `DO` block that raises. (likely)
2. **SSL mode downgrade** — `psql_env` maps only `disable`/`require`. `VerifyCA`/`VerifyFull` become `prefer`, and so does an absent setting, which allows plaintext. Pass `verify-ca`/`verify-full` through unchanged, and default to `require`. (confirmed)
3. **"Read-only" modes not enforced** — `verify` and `file` run with full write rights on the shared DB. Set `PGOPTIONS=-c default_transaction_read_only=on` for both. The `cleanup.sql` refusal is a basename match that a copy defeats. (confirmed)
4. **`cleanup.sql` executable outside psql** — in pgAdmin or DBeaver, `\echo`/`\quit` are not honoured. The `budget_table` DELETE, which has no forbid trigger, would then remove 12 rows that have no audit trail. Wrap the DELETEs in a `/* … */` block, or make the first statement a raising `DO` block. (confirmed)
5. **Unaudited seeded rows not disclosed** — V1 shows no audit rows for `fa_financial_years` (2) or `budget_table` (12). The README marks the fa rows as "untagged", but it never says that only `logs/02_real_run.log` proves they were created by this batch. Add one sentence and keep the log with the batch record. (confirmed)
6. **README inaccuracies** — it says preflight covers "P2–P11", but `seed.sql` implements P2–P7, P10 and P11. P8 (schema) was done manually in discovery step 1, and P9 is moot because estimates were skipped. It says `cleanup.sql` "`\quit`s on line 1", but the quit is at line 22. The swap of DESIGN §3.2/§4.1 `selection.psql` for hard-coded master codes in `dseed_slot` is not listed as a deviation. The README also publishes the internal DB host IP, which it does not need. Correct all four. (confirmed)
7. **Phase-0 actor pick relies on UNION ALL order** — C1-before-C2 precedence depends on unguaranteed row order before the outer `LIMIT 1`. It had no effect here, because only one `'Budget'` charge exists. Add `ORDER BY pref` for re-use. (confirmed)
8. **CE wing assumed Irrigation** — offices 20 and 21 (CE/TS, CE/Generation) got wing `1`, but they are likely Power. Have the business confirm the wing, then correct it with `PUT /api/budget-offices/{id}/wing` before anyone tests wing-dependent screens. No corrective INSERT is needed. (likely)

</details>

<details>
<summary>Details</summary>

### Hard rules against the committed run

The bypass keywords appear only in the `cleanup.sql` warning comment. The one UPDATE (seed.sql line 555) can reach only a batch row: it is constrained to `current_version_no = 1` on an approval found by `(division_id, work_desc)` from `dseed_aa`, and P6 aborts if any `DUMMY-` work exists that this batch did not write. Demand lines record the holder Division as `actor_office_id` (reset to IT Cell after each proposal). That attribution is how DB-11 is satisfied without a bypass, and README and DESIGN §1.1 disclose it.

### Committed side effects on the shared DB (disclosed, accepted by the user)

Cycle 2 permanently uses up the `current_fin_year = 2027` and `is_first_cycle` unique slots. The dummy delegation limits at `effective_from 2000-01-01` will flag real A&As until real limits are loaded. The CE wing was assumed to be `1` (Irrigation) for CE/TS and CE/Generation, which are more likely Power. The placeholder attachments return `INTEGRITY` when opened. README states all four, with remediation routes (`PUT /api/budget-offices/{id}/wing`, closing the cycle), and none needs an additive correcting INSERT. The wing correction is an UPDATE through the API on seeded rows and should be done before anyone tests wing-dependent screens.

### Exit status of the STOP guards

`\quit` ends the script, but psql's exit status follows the last error, not the argument. The hierarchy and head guards sit before the first `BEGIN`, so a STOP writes nothing. It does make `run_seed.py real` return 0 with no rows created, and that can be misread as "already seeded". This did not happen in the recorded runs: all four logs reach their completion banner and contain no warnings.

### Connection handling in run_seed.py

The password is read from `appsettings.json` and reaches psql only through `PGPASSWORD` in the child environment. It is never printed, and the logs contain no connection details beyond the actor. The SSL mapping is the weak point. The script never reads or records which mode the run actually used, so whether the recorded run negotiated TLS cannot be confirmed from the evidence. Separately, the README publishes the internal host IP. That is not a secret, but it is unnecessary in a document that may circulate.

Pre-existing and out of scope for this diff: the DB password lives in `BBMB.BudgetService/appsettings.json` in the repo. The seed avoids copying it, but the long-term fix is a user-secrets or environment source.

### Test coverage of the seed itself

What is covered: the dry run rolled back with a follow-up count proving nothing persisted, the real run, the idempotent re-run, and the read-only verify. Not tested: the STOP paths (hier_ok/heads_ok/preflight failures) and their exit status, and a partial-failure resume (a failure in phase 4 after phases 1–3 committed). By inspection, the guards make a resume safe.

</details>

<details>
<summary>File map</summary>

- `seed.sql`: preflight, temp selection tables, 5 committed phases, dry-run ROLLBACK path.
- `run_seed.py`: psql wrapper with dry-run/real/verify/file modes and env-only password.
- `verify.sql`: read-only batch counts V1–V8, batch rows identified via audit_log.
- `cleanup.sql`: documentation-only DELETE list behind a psql `\quit` header.
- `README.md`: counts, actor, deviations, skipped items, evidence table, side effects.
- `DESIGN.md`: seed design, hard rules, preflight table, gaps G-1–G-7.
- `logs/01–04`: dry run, real run, idempotent re-run, verify output.

Full content: the files in `/Users/manishkumar/BBMB-1/BBMB.Services/tools/dummy-seed/`. The tool is not under a PR diff.

</details>
