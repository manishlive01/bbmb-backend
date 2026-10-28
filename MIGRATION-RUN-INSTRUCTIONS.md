# BBMB BudgetService — Migration Run Instructions

**Kis ke liye:** jo developer DB par migrations apply karega (machine jis par NIC DB
`10.43.18.13:5432` reachable hai).

**Repo:** `BBMB-1` · **Branch:** `feature/budget-lifecycle-api`
**Working folder:** `BBMB.Services/BBMB.BudgetService`

---

## ⚠️ Sabse pehle ye samjho — ye normal migration NAHI hai

Ye migrations ek **"migration actor" gate** ke peechhe hain (`MigrationActor.sql`,
design §11.1 rule 8). Har data-shaping step sabse pehle check karta hai ki
**executing officer ki identity connection par set hai ya nahi**.

Isliye **plain `dotnet ef database update` FAIL hoga** is error ke saath:

```
RAISE EXCEPTION 'migration actor absent or inactive'
```

(ya `migration actor set at database or role level`).

Gate do cheezein connection ke **start-up settings** mein maangta hai:

| Setting | Value |
|---|---|
| `bbmb_migration.actor_employee_id` | ek valid employee ka **UUID** |
| `bbmb_migration.actor_charge_id` | us employee ke **active charge** ka UUID |

Ye dono `employee.employee_charge` table mein **active + non-expired** hone chahiye.
Ye values connection string ke `Options=-c ...` parameter se bheji jaati hain (neeche).

---

## Prerequisites (machine par)

- `dotnet` SDK **10.x** (project `net10.0` hai)
- `dotnet-ef` tool. Agar nahi hai:
  ```bash
  dotnet tool install --global dotnet-ef
  ```
- NIC DB `10.43.18.13:5432` **reachable** ho (VPN / NIC network on).
  Test:
  ```bash
  nc -z -w5 10.43.18.13 5432   # "succeeded" aana chahiye
  ```

---

## Kaunsi connection string? (ReadMe.md se)

Project mein do connection strings hain — kahan chal rahe ho uske hisaab se chuno:

| Environment | Connection string |
|---|---|
| **Local Development (office / NIC NET)** | `Host=10.43.18.13;Port=5432;Database=BBMB;Username=postgres;Password=postgres;Ssl Mode=Disable;` |
| **Public Cloud** | **Aiven** string — `Host=bbmb-pankajjaincloud-4e30.j.aivencloud.com;Port=20575;...;Ssl Mode=Require;` (appsettings.json mein commented) |

- NIC NET par ho → NIC string (neeche ki commands isi ko use karti hain).
- Cloud par apply karna ho → Aiven string use karo, par `Options=-c bbmb_migration.actor_*`
  wala hissa waise ka waisa rehna chahiye (actor gate har jagah same hai).

---

## STEP 1 — Ek valid active actor dhoondho (DB par query)

```sql
SELECT c.employee_id, c.id AS charge_id, e.name_english
FROM employee.employee_charge c
JOIN employee.employee e ON e.id = c.employee_id
WHERE c.is_active
  AND NOT c.is_charge_expire
  AND (c.charge_expire_time IS NULL OR c.charge_expire_time > now())
LIMIT 5;
```

- Jo `employee_id` aur `charge_id` (dono UUID) mile, unhe STEP 3 ki command mein daalo.
- **Agar koi row NAHI aati** → iska matlab DB fresh hai aur abhi tak koi employee/charge
  seed nahi hua. Tab pehle **EmployeeContext** apply karna padega (STEP 4 dekho) aur
  ek active charge seed karna hoga — uske bina actor gate kabhi pass nahi hoga
  (chicken-and-egg). Is case mein Manish ko batao.

---

## STEP 2 — Confirm karo koi `bbmb_migration.*` DB/role par set nahi hai

Gate aisi persisted setting ko bhi reject karta hai (taaki galti se kisi aur run ko
us officer ke naam attribute na ho). Check:

```sql
SELECT setdatabase::regclass AS db, r.rolname AS role, s.setconfig
FROM pg_db_role_setting s
LEFT JOIN pg_roles r ON r.oid = s.setrole
WHERE EXISTS (SELECT 1 FROM unnest(s.setconfig) x WHERE x LIKE 'bbmb_migration.%');
```

- **0 rows** → theek hai, aage badho.
- **Koi row aaye** → hatao, phir dobara:
  ```sql
  ALTER DATABASE "BBMB" RESET "bbmb_migration.actor_employee_id";
  ALTER DATABASE "BBMB" RESET "bbmb_migration.actor_charge_id";
  -- (role par set ho to: ALTER ROLE postgres RESET ...)
  ```

---

## STEP 3 — Migration apply karo (BudgetContext)

Folder: `BBMB.Services/BBMB.BudgetService`

```bash
dotnet ef database update --context BudgetContext \
  --connection "Host=10.43.18.13;Port=5432;Database=BBMB;Username=postgres;Password=postgres;Ssl Mode=Disable;Options=-c bbmb_migration.actor_employee_id=<EMP_UUID> -c bbmb_migration.actor_charge_id=<CHARGE_UUID>"
```

- `<EMP_UUID>` aur `<CHARGE_UUID>` ko STEP 1 wali asli values se replace karo.
- Success par EF har applied migration ka naam print karega aur koi error nahi aayega.

---

## STEP 4 — (Agar zaroori ho) teeno contexts, sahi order mein

Project mein **3 DbContext** hain. Actor gate `employee.*` tables par depend karta hai,
isliye **EmployeeContext pehle**, phir Budget, phir Finance:

```bash
# 1) Employee pehle (actor data yahan rehta hai)
dotnet ef database update --context EmployeeContext \
  --connection "Host=10.43.18.13;Port=5432;Database=BBMB;Username=postgres;Password=postgres;Ssl Mode=Disable;Options=-c bbmb_migration.actor_employee_id=<EMP_UUID> -c bbmb_migration.actor_charge_id=<CHARGE_UUID>"

# 2) Budget
dotnet ef database update --context BudgetContext \
  --connection "Host=10.43.18.13;Port=5432;Database=BBMB;Username=postgres;Password=postgres;Ssl Mode=Disable;Options=-c bbmb_migration.actor_employee_id=<EMP_UUID> -c bbmb_migration.actor_charge_id=<CHARGE_UUID>"

# 3) Finance
dotnet ef database update --context FinanceContext \
  --connection "Host=10.43.18.13;Port=5432;Database=BBMB;Username=postgres;Password=postgres;Ssl Mode=Disable;Options=-c bbmb_migration.actor_employee_id=<EMP_UUID> -c bbmb_migration.actor_charge_id=<CHARGE_UUID>"
```

> Note: EmployeeContext/FinanceContext ke apne migrations ho sakte hain ya nahi —
> agar kisi context ke liye "No migrations were applied / already up to date" aaye to
> theek hai, wo skip ho jayega.

---

## Verify — migration apply hui ya nahi

```bash
dotnet ef migrations list --context BudgetContext \
  --connection "Host=10.43.18.13;Port=5432;Database=BBMB;Username=postgres;Password=postgres;Ssl Mode=Disable"
```

- Jis migration ke aage `(Pending)` **nahi** likha, wo DB par apply ho chuki hai.
- Ya DB par seedha:
  ```sql
  SELECT "MigrationId" FROM budget."__EFMigrationsHistory" ORDER BY "MigrationId";
  ```
  (schema naam alag ho sakta hai — agar `budget.` na mile to bina schema ke try karo.)

---

## Common errors — jaldi fix

| Error | Matlab | Fix |
|---|---|---|
| `migration actor absent or inactive` | `actor_employee_id` / `actor_charge_id` galat, ya charge active/non-expired nahi | STEP 1 se sahi active UUIDs lo |
| `migration actor set at database or role level` | `bbmb_migration.*` DB/role par persist hai | STEP 2 ka `RESET` chalao |
| `Failed to connect to 10.43.18.13:5432` | DB reachable nahi | VPN / NIC network on karo (STEP 0 ka `nc` test) |
| `No DbContext was found` / `More than one DbContext` | context specify nahi kiya | `--context BudgetContext` zaroor do |

---

## Security notes (dhyan do)

- Upar ki commands mein **plain-text DB password** hai. Chat/email par bhejte waqt
  dhyan rakho; ho sake to developer apni local connection string use kare, ya baad mein
  password rotate ho jaye.
- `appsettings.json` mein bhi DB passwords committed hain (local + Aiven). Inhe
  secrets manager / environment variable mein move karna chahiye — repo mein nahi.
