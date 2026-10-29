# BBMB Budget — admin-actor authorization fix

**Service:** `BBMB.Services` / `BBMB.BudgetService`
**Worktree:** `/Users/manishkumar/BBMB-1/.worktrees/budget-db-authz`
**Branch:** `feature/budget-db-authz`
**Range:** `222ea1e..HEAD` (single commit `bf7b10a` — `fix(authz): resolve an effective-admin actor without an operational charge`)

## 1. Problem

`BudgetRightHandler` already grants an **effective-admin acting role** every configured right *without* a charge. An effective-admin role is one that is in `BudgetConfigSnapshot.AdminRoles`: `is_admin` is true **and** it carries an APPROVED, document-backed admin assignment (read from the DB-backed authz config, not a hard-coded role name).

But actor resolution in `HttpBudgetActorSource` still demanded an operational charge of the acting role's office level. A pure-admin user with no operational `employee_charge` therefore resolved to no actor — reason `NO_CHARGE` — which the endpoint maps to **403 M-20** (`BudgetResults`: `M-20 → 403`). The observed symptom was an admin-only token hitting `POST /api/technical-sanctions/list` and getting 403 M-20, blocked from every endpoint that resolves an actor.

The handler authorised the admin; the actor source then refused to produce the actor the handler had just authorised. That is the bug this change fixes.

## 2. The rule chosen for admin actor resolution

Resolution now branches on the **same `AdminRoles` signal the handler authorises on**, read from `IBudgetConfigProvider` (never a hard-coded `"ProjectAdmin"`). In `ResolveAsync`, after the acting role is known:

```
snapshot = configProvider.GetAsync(ct)
if snapshot.AdminRoles.Contains(role, OrdinalIgnoreCase)  -> ResolveAdminActorAsync   (new admin branch)
else                                                      -> ResolveOperationalActorAsync (today's path, verbatim)
```

### 2.1 How an effective-admin resolves to a `BudgetActor` without an operational charge

`ResolveAdminActorAsync` builds the actor from role-level and token facts, not from an office-level-matched charge:

| Field | Source | Failure reason if it cannot resolve |
|---|---|---|
| Employee id / name | token `user_id` → employee lookup (exists without any charge; all employee rows for the user are read, as `ChargeResolver` does) | `NO_USER_ID` (bad token), `NO_EMPLOYEE` (no employee row), `NO_EMPLOYEE_NAME` (blank name) |
| Office | the single active office of the admin role's configured level, via `IBudgetOfficeScope.ResolveActorOfficeAsync` | `NO_ADMIN_OFFICE` |
| Charge (audit stamp only) | a current active charge of the employee — see §3 | `NO_ADMIN_CHARGE` |

The `BudgetActor` record shape is unchanged; the admin branch simply fills it from different sources.

### 2.2 How the admin office is resolved

The admin office is the single active office of the admin role's configured level. `FINANCE / SECRETARIAT / IT_CELL` are **role-level** offices (not charge-dependent), so an empty charge context is sufficient — the branch calls `ResolveActorOfficeAsync(new UserChargeContext { EmployeeId = employeeIds[0] }, role, ct)`. If the role has no office level, or no active office of that level exists, `ResolveActorOfficeAsync` returns null and resolution fails with **`NO_ADMIN_OFFICE`** — a specific reason an operator can act on, not a misleading M-20 that implies a missing charge.

## 3. The charge-for-audit decision

The audit path is driven entirely by `budget.fn_require_actor` (a `BEFORE INSERT/UPDATE` trigger) and `budget.fn_audit_row` (the `AFTER` writer into `budget.audit_log`). In the migration `20261014140505_BudgetP1ReferenceStateAndAudit`, `budget.audit_log` declares `actor_employee_id`, `actor_charge_id` and `actor_office_id` as **NOT NULL**. `fn_require_actor` additionally re-checks that `actor_charge_id` is a *current active charge of the actor employee*:

```
EXISTS ( employee.employee_charge c
         WHERE c.id = actor_charge_id
           AND c.employee_id = actor_employee_id
           AND c.is_active
           AND NOT c.is_charge_expire
           AND (c.charge_expire_time IS NULL OR c.charge_expire_time > now()) )
```

Two consequences shaped the design:

- **A sentinel / fake charge id cannot be used.** The trigger would reject it, so the admin actor must stamp a *real* current active charge — which is why the fix needs no schema or migration change.
- **The stamp charge need not match the admin office level.** The trigger does not restrict the stamp charge by project or by office level; it only requires that it be a current active charge of the employee.

### 3.1 How `actor_charge_id` is stamped for an admin

`ResolveAdminStampChargeAsync` selects a charge whose predicate is exactly `fn_require_actor`'s acceptance predicate:

```
c.IsActive && !c.IsChargeExpire && (c.ChargeExpireTime == null || c.ChargeExpireTime > now)
```

Selection order:

1. **Prefer a budget-project charge** (`ProjectCode == snapshot.ProjectCode`), ordered `ChargeTime` then `Id` for determinism — so the stamp stays within the Budget project where one exists.
2. **Else any current active charge** of the employee, same deterministic order.

The chosen `(EmployeeId, ChargeId)` becomes `BudgetActor.EmployeeId` / `BudgetActor.ChargeId`. Because `BudgetActionScope` writes these through `set_config` from **non-nullable value types** (`Guid` / `long`), the admin path can never set a NULL `actor_charge_id` or `actor_office_id`, and the stamp charge passes `fn_require_actor`'s re-check. The NOT-NULL audit columns are therefore always satisfied.

A blank `ProjectCode` means no active settings row; the snapshot is then `Empty`, `AdminRoles` is empty, and the admin branch is unreachable — the helper fails closed (returns null) rather than matching a blank-project charge.

### 3.2 What happens if an admin has no charge at all

If the employee holds no current active charge, `ResolveAdminStampChargeAsync` returns null and resolution fails with **`NO_ADMIN_CHARGE`** — a reason that tells the operator to attach a charge, rather than a misleading `NO_CHARGE → M-20`. The admin is not silently let through without a valid audit actor (which the DB trigger would reject anyway); the correct operational fix (attach a charge) is surfaced by name.

## 4. Files changed

| File | Change |
|---|---|
| `BBMB.Services/BBMB.BudgetService/Shared/Services/BudgetActors/HttpBudgetActorSource.cs` | Admin/non-admin branch; new `ResolveAdminActorAsync`, `ResolveOperationalActorAsync`, `ResolveAdminStampChargeAsync`, `RequestId` helper and `EmployeeStampCharge` record; new `IBudgetConfigProvider` constructor parameter; `BudgetActingRole`/`AuthzConfig` using directives. |
| `BBMB.Services/BBMB.BudgetService.Tests/Shared/Services/HttpBudgetActorSourceAdminTests.cs` | New in-memory unit suite (4 tests + stubs) covering the admin branch and the unchanged non-admin path. |
| `BBMB.Services/BBMB.BudgetService.Tests/Exit/P1ExitTests.cs` | Constructor call updated to pass the config provider to the new `HttpBudgetActorSource` signature (compile only). |

No production files other than `HttpBudgetActorSource.cs`. No migration, SQL, entity, or DI-registration files changed.

## 5. Why non-admin behaviour is unchanged

A non-admin acting role never enters the admin branch (its role is not in `AdminRoles`). It flows through `ResolveOperationalActorAsync`, which is today's logic moved verbatim:

| Case | Behaviour (unchanged) |
|---|---|
| No current Budget charge | `BudgetChargeOutcome.None` → `NO_CHARGE` → **M-20 (403)** |
| Several current Budget charges, none named | `BudgetChargeOutcome.Ambiguous` → `AMBIGUOUS_CHARGE` (with the charge ids) |
| Cross-project roles | still grant nothing — the charge resolver only yields Budget-project charges |

The only mechanical edits inside this path are cosmetic: the request-id block was extracted into the shared `RequestId(http)` helper (identical logic — reuse the item if present, else create and store one). The resolution outcomes are byte-for-byte the same.

## 6. Build and test results

All commands were run in the worktree `BBMB.Services` directory on this machine.

| Command | Outcome |
|---|---|
| `dotnet build BBMB.BudgetService/BBMB.BudgetService.csproj` | **0 Error(s)** (112 pre-existing warnings) |
| `dotnet build BBMB.BudgetService.Tests/BBMB.BudgetService.Tests.csproj` | **0 Error(s)** (26 warnings) |
| `dotnet test --no-build --filter "FullyQualifiedName~RemovedRoutesTests\|...BudgetEndpointPolicyTests\|...PostReadRoutesTests\|...HttpBudgetActorSourceAdminTests"` | **Passed 99, Failed 0** (incl. the 4 new admin-branch unit tests) |
| `dotnet test --no-build --filter "FullyQualifiedName~SubMinorHead\|FullyQualifiedName~MinorHead"` (InMemory) | **Passed 27, Failed 0** |

**Docker is unavailable on this machine** (`docker info` fails). The Testcontainers/PostgreSQL suites — `ChargeResolverTests`, `P1ExitTests`, and the migration tests — therefore **did not run**. They compile (both projects build with 0 errors), but the DB-backed trigger behaviour (`fn_require_actor` rejecting a non-current charge, the NOT-NULL audit columns, `fn_audit_row` row writes) was **not exercised in this environment**. The new admin-branch tests deliberately cover the resolution logic in-memory; the trigger re-check itself is left to the Testcontainers suites, which must be run where Docker is present before relying on this change end to end.

## 7. DB / migration impact

**None.** The fix is confined to application code. It makes no schema change precisely *because* `fn_require_actor` requires a real current charge — a sentinel charge id was considered and rejected for that reason. `BudgetActor`'s record shape is unchanged, the `set_config` keys in `BudgetActionScope` are unchanged, and the admin path stamps an existing `employee_charge` row rather than inventing one. No migration was generated or run; the NIC NET DB (`10.43.18.13`) and all live DBs are untouched.

## 8. Remaining manual test

In an environment with Docker, run the Testcontainers suites to exercise the trigger re-check against a real PostgreSQL instance.

The decisive end-to-end check on a deployed build: an **admin-only token (pure admin, no operational charge)** calling an endpoint that resolves an actor — e.g. `POST /api/technical-sanctions/list` — should now **succeed** instead of returning **403 M-20**, provided the admin employee holds at least one current active charge for the audit stamp. If the admin holds no charge at all, expect the specific `NO_ADMIN_CHARGE` reason (attach a charge), not a bare M-20; if the admin role's office level has no active office, expect `NO_ADMIN_OFFICE`.
