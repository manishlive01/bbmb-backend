-- BudgetP5Reappropriation step 2: DDD §5.8 CHECKs, the cross-schema actor and closed_by references EF does
-- not create, and the partial UK (created_by) where OPEN (one entry in hand per officer, BUD-201).

-- =====================================================================================================
-- T-46 reappropriation ([A], mutable). status enumerated and moves from OPEN once (DB-24, trigger below);
-- additional_amount > 0; exactly one destination while OPEN; destination_provision_id not null on SUBMITTED
-- and order_attachment_id not null on SUBMITTED (VR-10) are enforced by fn_reappropriation_rules.
-- =====================================================================================================
ALTER TABLE budget.reappropriation
    ADD CONSTRAINT ck_reappropriation_status CHECK (status IN ('OPEN', 'SUBMITTED', 'ABANDONED')),
    ADD CONSTRAINT ck_reappropriation_additional CHECK (additional_amount > 0),
    ADD CONSTRAINT ck_reappropriation_destination CHECK (
        -- Exactly one destination (an existing provision or a new work) while OPEN; on SUBMITTED a destination
        -- provision must exist (the new work's provision is opened at submit).
        (destination_provision_id IS NOT NULL)::int + (destination_new_work_id IS NOT NULL)::int = 1),
    ADD CONSTRAINT ck_reappropriation_submitted CHECK (
        status <> 'SUBMITTED' OR (destination_provision_id IS NOT NULL AND order_attachment_id IS NOT NULL)),
    ADD CONSTRAINT fk_reappropriation_closed_by FOREIGN KEY (closed_by) REFERENCES employee.employee (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_reappropriation_created_by FOREIGN KEY (created_by) REFERENCES employee.employee (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_reappropriation_created_charge FOREIGN KEY (created_charge_id) REFERENCES employee.employee_charge (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_reappropriation_updated_by FOREIGN KEY (updated_by) REFERENCES employee.employee (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_reappropriation_updated_charge FOREIGN KEY (updated_charge_id) REFERENCES employee.employee_charge (id) ON DELETE RESTRICT;

-- One OPEN entry per officer at a time (BUD-201): a partial unique index on created_by where OPEN. Filtered on
-- an actor column, so it is a SQL object, not an EF index.
CREATE UNIQUE INDEX ux_reappropriation_open_per_officer ON budget.reappropriation (created_by)
    WHERE status = 'OPEN';

-- =====================================================================================================
-- T-47 reappropriation_source ([A], mutable while OPEN). amount > 0; is_removed one-way (fn, below). The
-- cumulative ≤ additional (DB-24, VR-08, M-18) is a deferred commit check (fn_reappropriation_balance).
-- =====================================================================================================
ALTER TABLE budget.reappropriation_source
    ADD CONSTRAINT ck_reappropriation_source_amount CHECK (amount > 0),
    ADD CONSTRAINT fk_reappropriation_source_created_by FOREIGN KEY (created_by) REFERENCES employee.employee (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_reappropriation_source_created_charge FOREIGN KEY (created_charge_id) REFERENCES employee.employee_charge (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_reappropriation_source_updated_by FOREIGN KEY (updated_by) REFERENCES employee.employee (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_reappropriation_source_updated_charge FOREIGN KEY (updated_charge_id) REFERENCES employee.employee_charge (id) ON DELETE RESTRICT;
