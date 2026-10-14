-- BudgetP1ReferenceStateAndAudit step 1, design §11.1 rule 5: the actor FKs reference employee.employee
-- and employee.employee_charge, which belong to EmployeeContext. Deployment applies EmployeeContext first.
DO $$
BEGIN
    IF to_regclass('employee.employee') IS NULL OR to_regclass('employee.employee_charge') IS NULL THEN
        RAISE EXCEPTION 'apply EmployeeContext migrations first';
    END IF;
END
$$;
