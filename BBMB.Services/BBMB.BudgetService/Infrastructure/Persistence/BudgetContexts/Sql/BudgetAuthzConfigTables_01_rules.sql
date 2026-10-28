-- BudgetAuthzConfigTables step 1 (design §1, §7): partial UKs, CHECKs and the cross-schema employee FKs the
-- EF CreateTable cannot express. Hand SQL, same split as every other budget table.

-- budget_project_settings: exactly one active row; a positive edit lease.
CREATE UNIQUE INDEX ux_project_settings_active
    ON budget.budget_project_settings (is_active) WHERE is_active;
ALTER TABLE budget.budget_project_settings
    ADD CONSTRAINT ck_edit_lease_minutes CHECK (edit_lease_minutes > 0);

-- budget_role: office level reuses the T-13 office-level domain.
ALTER TABLE budget.budget_role
    ADD CONSTRAINT ck_budget_role_office_level
    CHECK (office_level IN ('DIVISION','CIRCLE','CE','SECRETARIAT','FINANCE','IT_CELL'));

-- budget_admin_role_assignment:
--  * status domain;
--  * one live (non-revoked) assignment per role;
--  * APPROVED implies a document, an uploader and an approver (the DB owns this invariant, design §1);
--  * uploaded_by / approved_by reference employee.employee(id) (cross-schema FK, RESTRICT).
ALTER TABLE budget.budget_admin_role_assignment
    ADD CONSTRAINT ck_admin_status CHECK (status IN ('PENDING','APPROVED','REVOKED'));
CREATE UNIQUE INDEX ux_admin_role_active
    ON budget.budget_admin_role_assignment (role_id) WHERE status <> 'REVOKED';
ALTER TABLE budget.budget_admin_role_assignment
    ADD CONSTRAINT ck_admin_approved_has_doc
    CHECK (status <> 'APPROVED'
           OR (document_ref IS NOT NULL AND approved_by IS NOT NULL AND uploaded_by IS NOT NULL));
ALTER TABLE budget.budget_admin_role_assignment
    ADD CONSTRAINT fk_admin_uploaded_by FOREIGN KEY (uploaded_by)
        REFERENCES employee.employee (id) ON DELETE RESTRICT;
ALTER TABLE budget.budget_admin_role_assignment
    ADD CONSTRAINT fk_admin_approved_by FOREIGN KEY (approved_by)
        REFERENCES employee.employee (id) ON DELETE RESTRICT;
