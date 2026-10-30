-- Read-only drift check between budget_role.office_level_id (source of truth) and the legacy office_level string
-- (review R2-2, expand/contract window of migration BudgetRoleSyncAndLevelFk). It writes nothing.
-- Render it through BudgetRoleLevelDriftCheck.Sql(): the alias VALUES list below is substituted from
-- BudgetRoleLevel.Aliases, the single level mapping, so it cannot disagree with the application.
--
-- The previous release writes only office_level, and the migration backfill ran once, only WHERE
-- office_level_id IS NULL. Run this check (a) before this release takes over from the previous one, and (b)
-- immediately before the CONTRACT migration that drops office_level. Every row it returns has to be resolved,
-- through roles/configure (which dual-writes both columns), before that step proceeds. Drift kinds:
--   FK_MISSING       office_level set, office_level_id NULL: a role created by the previous release, or one whose
--                    level found no levels row. The new release denies it until it is configured.
--   LEGACY_STALE     both set, but office_level is not the code office_level_id maps to: the previous release
--                    changed the level after the backfill. The new release acts on the FK.
--   LEGACY_MISSING   office_level_id set, office_level NULL. Not written by any release; the previous release
--                    cannot read the row.
--   LEVEL_NOT_BUDGET office_level_id points at a levels row whose name is not a Budget alias.
--   BOTH_NULL        a role inserted by the role sync and not yet configured. Expected once the previous release
--                    is gone; while it still serves the database the previous release cannot read the row
--                    (review R2-1, which is why the sync is off by default).
SELECT br.id,
       br.role_name,
       br.office_level,
       br.office_level_id,
       l.name AS level_name,
       a.code AS level_code,
       CASE
           WHEN br.office_level_id IS NULL AND br.office_level IS NULL THEN 'BOTH_NULL'
           WHEN br.office_level_id IS NULL THEN 'FK_MISSING'
           WHEN a.code IS NULL THEN 'LEVEL_NOT_BUDGET'
           WHEN br.office_level IS NULL THEN 'LEGACY_MISSING'
           ELSE 'LEGACY_STALE'
       END AS drift
FROM budget.budget_role br
LEFT JOIN budget.levels l ON l.id = br.office_level_id
LEFT JOIN ({{LEVEL_ALIASES}}) AS a(alias, code) ON a.alias = upper(btrim(l.name))
WHERE br.office_level_id IS NULL
   OR br.office_level IS NULL
   OR a.code IS NULL
   OR a.code <> upper(btrim(br.office_level))
ORDER BY br.id;
