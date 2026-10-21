-- BudgetP1ProposalSpine step 2: DDD T-29 to T-33 CHECKs and the references EF does not create (employee
-- schema, T-07 account_code_id). T-33's composite FK to T-16 is added by BudgetP1StateTransitionSeed.

-- T-29 budget.proposal.
ALTER TABLE budget.proposal
    ADD CONSTRAINT ck_proposal_category CHECK (category IN ('WORKS', 'DNA', 'RECEIPTS', 'CAPEX')),
    ADD CONSTRAINT fk_proposal_edit_lease_by FOREIGN KEY (edit_lease_by) REFERENCES employee.employee (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_proposal_created_by FOREIGN KEY (created_by) REFERENCES employee.employee (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_proposal_created_charge FOREIGN KEY (created_charge_id) REFERENCES employee.employee_charge (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_proposal_updated_by FOREIGN KEY (updated_by) REFERENCES employee.employee (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_proposal_updated_charge FOREIGN KEY (updated_charge_id) REFERENCES employee.employee_charge (id) ON DELETE RESTRICT;

-- T-30 budget.demand_line.
ALTER TABLE budget.demand_line
    ADD CONSTRAINT ck_demand_line_type CHECK (line_type IN ('WORK', 'HEAD', 'UNFORESEEN')),
    -- work_id: not null exactly for WORK.
    ADD CONSTRAINT ck_demand_line_work CHECK ((work_id IS NOT NULL) = (line_type = 'WORK')),
    -- account_code_id: not null for WORK and HEAD, null for UNFORESEEN (BUD-277).
    ADD CONSTRAINT ck_demand_line_account_code CHECK ((account_code_id IS NULL) = (line_type = 'UNFORESEEN')),
    -- unforeseen_name: not null exactly for UNFORESEEN.
    ADD CONSTRAINT ck_demand_line_unforeseen_name CHECK ((unforeseen_name IS NOT NULL) = (line_type = 'UNFORESEEN')),
    -- deleted_at, deleted_by: set with is_deleted.
    ADD CONSTRAINT ck_demand_line_deleted CHECK (
        is_deleted = (deleted_at IS NOT NULL) AND is_deleted = (deleted_by IS NOT NULL)),
    ADD CONSTRAINT fk_demand_line_account_code FOREIGN KEY (account_code_id)
        REFERENCES budget.budget_head_master (account_code_id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_demand_line_deleted_by FOREIGN KEY (deleted_by) REFERENCES employee.employee (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_demand_line_created_by FOREIGN KEY (created_by) REFERENCES employee.employee (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_demand_line_created_charge FOREIGN KEY (created_charge_id) REFERENCES employee.employee_charge (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_demand_line_updated_by FOREIGN KEY (updated_by) REFERENCES employee.employee (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_demand_line_updated_charge FOREIGN KEY (updated_charge_id) REFERENCES employee.employee_charge (id) ON DELETE RESTRICT;

-- T-31 budget.figure_version.
ALTER TABLE budget.figure_version
    ADD CONSTRAINT ck_figure_version_figure CHECK (figure IN ('RBE', 'OBE_NEXT')),
    ADD CONSTRAINT ck_figure_version_source CHECK (change_source IN ('ENTRY', 'UPLOAD', 'ALTERATION')),
    ADD CONSTRAINT fk_figure_version_created_by FOREIGN KEY (created_by) REFERENCES employee.employee (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_figure_version_created_charge FOREIGN KEY (created_charge_id) REFERENCES employee.employee_charge (id) ON DELETE RESTRICT;

-- T-32 budget.observation.
ALTER TABLE budget.observation
    -- observation_text: CHECK not blank (BUD-120, M-12).
    ADD CONSTRAINT ck_observation_text CHECK (btrim(observation_text) <> ''),
    ADD CONSTRAINT fk_observation_replied_by FOREIGN KEY (replied_by) REFERENCES employee.employee (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_observation_created_by FOREIGN KEY (created_by) REFERENCES employee.employee (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_observation_created_charge FOREIGN KEY (created_charge_id) REFERENCES employee.employee_charge (id) ON DELETE RESTRICT;

-- T-33 budget.proposal_movement.
ALTER TABLE budget.proposal_movement
    ADD CONSTRAINT fk_proposal_movement_created_by FOREIGN KEY (created_by) REFERENCES employee.employee (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_proposal_movement_created_charge FOREIGN KEY (created_charge_id) REFERENCES employee.employee_charge (id) ON DELETE RESTRICT;
