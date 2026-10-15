-- BudgetP3CalendarDistributionSources step 2: DDD T-20, T-39, T-18 CHECKs and the cross-schema actor
-- references EF does not create. Not gated: P3 carries no DDD deviation awaiting a decision (§11.4). No data
-- is written, so the migration actor is not needed (§11.1 rule 8).
-- The office_level CHECK of T-13 (DDD §3.3), save IT_CELL for T-20 (a calendar date is not set for the IT Cell).
-- T-20 budget.calendar (mutable: last_date; [A]).
ALTER TABLE budget.calendar
    ADD CONSTRAINT ck_calendar_office_level CHECK (office_level IN ('DIVISION', 'CIRCLE', 'CE', 'SECRETARIAT', 'FINANCE')),
    ADD CONSTRAINT fk_calendar_created_by FOREIGN KEY (created_by) REFERENCES employee.employee (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_calendar_created_charge FOREIGN KEY (created_charge_id) REFERENCES employee.employee_charge (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_calendar_updated_by FOREIGN KEY (updated_by) REFERENCES employee.employee (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_calendar_updated_charge FOREIGN KEY (updated_charge_id) REFERENCES employee.employee_charge (id) ON DELETE RESTRICT;
-- T-39 budget.distribution (mutable: the shares; [A]). Amounts >= 0 (whole rupees by the domain).
ALTER TABLE budget.distribution
    ADD CONSTRAINT ck_distribution_rbe CHECK (rbe_amount >= 0),
    ADD CONSTRAINT ck_distribution_obe CHECK (obe_next_amount >= 0),
    ADD CONSTRAINT fk_distribution_created_by FOREIGN KEY (created_by) REFERENCES employee.employee (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_distribution_created_charge FOREIGN KEY (created_charge_id) REFERENCES employee.employee_charge (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_distribution_updated_by FOREIGN KEY (updated_by) REFERENCES employee.employee (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_distribution_updated_charge FOREIGN KEY (updated_charge_id) REFERENCES employee.employee_charge (id) ON DELETE RESTRICT;
-- T-18 budget.expenditure_source_setting (append-only; [C]).
ALTER TABLE budget.expenditure_source_setting
    ADD CONSTRAINT ck_expenditure_source_mode CHECK (mode IN ('KEYED', 'DRAWN')),
    ADD CONSTRAINT fk_expenditure_source_created_by FOREIGN KEY (created_by) REFERENCES employee.employee (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_expenditure_source_created_charge FOREIGN KEY (created_charge_id) REFERENCES employee.employee_charge (id) ON DELETE RESTRICT;
