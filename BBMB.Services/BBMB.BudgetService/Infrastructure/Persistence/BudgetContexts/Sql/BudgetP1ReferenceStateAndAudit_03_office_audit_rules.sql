-- BudgetP1ReferenceStateAndAudit step 3: T-13 and T-51 rules EF cannot express (DDD T-13, T-51, §1.7).

-- T-13 budget.office (DDD T-13).
ALTER TABLE budget.office
    ADD CONSTRAINT ck_office_level
        CHECK (office_level IN ('DIVISION', 'CIRCLE', 'CE', 'SECRETARIAT', 'FINANCE', 'IT_CELL')),
    -- division_id / circle_id / ce_id: set if and only if the level is DIVISION / CIRCLE / CE.
    ADD CONSTRAINT ck_office_division CHECK ((division_id IS NOT NULL) = (office_level = 'DIVISION')),
    ADD CONSTRAINT ck_office_circle   CHECK ((circle_id IS NOT NULL) = (office_level = 'CIRCLE')),
    ADD CONSTRAINT ck_office_ce       CHECK ((ce_id IS NOT NULL) = (office_level = 'CE')),
    -- office_name: mandatory for SECRETARIAT, FINANCE and IT_CELL, null otherwise.
    ADD CONSTRAINT ck_office_name
        CHECK ((office_name IS NOT NULL) = (office_level IN ('SECRETARIAT', 'FINANCE', 'IT_CELL'))),
    -- wing_code: mandatory for CE.
    ADD CONSTRAINT ck_office_wing CHECK (office_level <> 'CE' OR wing_code IS NOT NULL),
    -- parent_office_id: null for FINANCE and IT_CELL only.
    ADD CONSTRAINT ck_office_parent
        CHECK ((parent_office_id IS NULL) = (office_level IN ('FINANCE', 'IT_CELL'))),
    -- [A] actor columns → employee schema (design §4.1, OI-02: added by SQL, the tables belong to EmployeeContext).
    ADD CONSTRAINT fk_office_created_by FOREIGN KEY (created_by)
        REFERENCES employee.employee (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_office_created_charge FOREIGN KEY (created_charge_id)
        REFERENCES employee.employee_charge (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_office_updated_by FOREIGN KEY (updated_by)
        REFERENCES employee.employee (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_office_updated_charge FOREIGN KEY (updated_charge_id)
        REFERENCES employee.employee_charge (id) ON DELETE RESTRICT;

-- T-51 budget.audit_log (DDD T-51).
ALTER TABLE budget.audit_log
    ADD CONSTRAINT ck_audit_log_operation CHECK (operation IN ('I', 'U')),
    ADD CONSTRAINT fk_audit_log_actor_employee FOREIGN KEY (actor_employee_id)
        REFERENCES employee.employee (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_audit_log_actor_charge FOREIGN KEY (actor_charge_id)
        REFERENCES employee.employee_charge (id) ON DELETE RESTRICT;
