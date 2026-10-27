-- Read-only verification of batch DUMMY-SEED:20261022-02 (dry-run personas). No UUIDs are printed.
\pset pager off
BEGIN READ ONLY;
\echo '== DUMMY persona employees and their current Budget charges'
SELECT e.employee_code, e.name_english, e.email,
       count(c.*) FILTER (WHERE c.project_code = 'Budget' AND c.is_active AND NOT c.is_charge_expire
                            AND (c.charge_expire_time IS NULL OR c.charge_expire_time > now())) AS current_budget_charges,
       max(c.ce_id) AS ce_id, max(c.cr_id) AS cr_id, max(c.division_id) AS division_id,
       max(c.charge_expire_time)::date AS charge_expires
FROM employee.employee e LEFT JOIN employee.employee_charge c ON c.employee_id = e.id
WHERE e.employee_code IN (900101, 900201, 900301)
GROUP BY 1, 2, 3 ORDER BY 1;
\echo '== charges not belonging to batch -02 personas (must be unchanged: 7 before the batch)'
SELECT count(*) AS other_charges FROM employee.employee_charge c JOIN employee.employee e ON e.id = c.employee_id
WHERE e.employee_code NOT IN (900101, 900201, 900301);
\echo '== GHOST user_id must be absent'
SELECT count(*) AS ghost_rows FROM employee.employee WHERE user_id = '00000000-0000-4000-8000-000000999999';
ROLLBACK;
