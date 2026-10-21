-- BudgetP1FiguresAndRecommendations step 2: DDD T-35 to T-38 CHECKs and references EF does not create.

-- T-35 budget.expenditure_figure (DDD T-35).
ALTER TABLE budget.expenditure_figure
    -- subject: WORK (work_id) or HEAD (office_id + account_code_id), exactly.
    ADD CONSTRAINT ck_expenditure_figure_subject_type CHECK (subject_type IN ('WORK', 'HEAD')),
    ADD CONSTRAINT ck_expenditure_figure_work CHECK ((work_id IS NOT NULL) = (subject_type = 'WORK')),
    ADD CONSTRAINT ck_expenditure_figure_office CHECK ((office_id IS NOT NULL) = (subject_type = 'HEAD')),
    ADD CONSTRAINT ck_expenditure_figure_account_code CHECK ((account_code_id IS NOT NULL) = (subject_type = 'HEAD')),
    ADD CONSTRAINT fk_expenditure_figure_account_code FOREIGN KEY (account_code_id)
        REFERENCES budget.budget_head_master (account_code_id) ON DELETE RESTRICT,
    ADD CONSTRAINT ck_expenditure_figure_nature CHECK (nature IN ('EXPENDITURE', 'RECEIPT')),
    ADD CONSTRAINT ck_expenditure_figure_period CHECK (period IN ('FULL_YEAR', 'UP_TO_MONTH', 'UP_TO_DATE')),
    -- up_to_month: first day of the month; not null exactly for UP_TO_MONTH (BUD-141).
    ADD CONSTRAINT ck_expenditure_figure_up_to_month CHECK (
        (up_to_month IS NOT NULL) = (period = 'UP_TO_MONTH') AND (up_to_month IS NULL OR extract(day FROM up_to_month) = 1)),
    ADD CONSTRAINT ck_expenditure_figure_source CHECK (source IN ('KEYED', 'DRAWN')),
    -- Whole rupees where source is KEYED (BUD-013); rupees and paise where DRAWN (BUD-266).
    ADD CONSTRAINT ck_expenditure_figure_keyed_whole CHECK (source <> 'KEYED' OR amount = trunc(amount)),
    ADD CONSTRAINT fk_expenditure_figure_created_by FOREIGN KEY (created_by) REFERENCES employee.employee (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_expenditure_figure_created_charge FOREIGN KEY (created_charge_id) REFERENCES employee.employee_charge (id) ON DELETE RESTRICT;

-- T-36 budget.opening_figure (DDD T-36).
ALTER TABLE budget.opening_figure
    -- subject: WORK (work_id) or HEAD (office_id + account_code_id), exactly.
    ADD CONSTRAINT ck_opening_figure_subject_type CHECK (subject_type IN ('WORK', 'HEAD')),
    ADD CONSTRAINT ck_opening_figure_work CHECK ((work_id IS NOT NULL) = (subject_type = 'WORK')),
    ADD CONSTRAINT ck_opening_figure_office CHECK ((office_id IS NOT NULL) = (subject_type = 'HEAD')),
    ADD CONSTRAINT ck_opening_figure_account_code CHECK ((account_code_id IS NOT NULL) = (subject_type = 'HEAD')),
    ADD CONSTRAINT fk_opening_figure_account_code FOREIGN KEY (account_code_id)
        REFERENCES budget.budget_head_master (account_code_id) ON DELETE RESTRICT,
    ADD CONSTRAINT ck_opening_figure_kind CHECK (figure_kind IN ('OBE_CURRENT', 'RBE_PREVIOUS')),
    ADD CONSTRAINT fk_opening_figure_created_by FOREIGN KEY (created_by) REFERENCES employee.employee (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_opening_figure_created_charge FOREIGN KEY (created_charge_id) REFERENCES employee.employee_charge (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_opening_figure_updated_by FOREIGN KEY (updated_by) REFERENCES employee.employee (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_opening_figure_updated_charge FOREIGN KEY (updated_charge_id) REFERENCES employee.employee_charge (id) ON DELETE RESTRICT;

-- T-37 budget.ce_recommendation (DDD T-37).
ALTER TABLE budget.ce_recommendation
    ADD CONSTRAINT ck_ce_recommendation_category CHECK (category IN ('DNA', 'RECEIPTS', 'CAPEX')),
    -- concurrence_ref: not null where either figure is set (BUD-153, BUD-154).
    ADD CONSTRAINT ck_ce_recommendation_concurrence CHECK (
        (rbe_recommended IS NULL AND obe_next_recommended IS NULL) OR concurrence_ref IS NOT NULL),
    ADD CONSTRAINT fk_ce_recommendation_account_code FOREIGN KEY (account_code_id)
        REFERENCES budget.budget_head_master (account_code_id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_ce_recommendation_confirmed_by FOREIGN KEY (distribution_confirmed_by)
        REFERENCES employee.employee (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_ce_recommendation_created_by FOREIGN KEY (created_by) REFERENCES employee.employee (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_ce_recommendation_created_charge FOREIGN KEY (created_charge_id) REFERENCES employee.employee_charge (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_ce_recommendation_updated_by FOREIGN KEY (updated_by) REFERENCES employee.employee (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_ce_recommendation_updated_charge FOREIGN KEY (updated_charge_id) REFERENCES employee.employee_charge (id) ON DELETE RESTRICT;

-- T-38 budget.recommendation_version (DDD T-38).
ALTER TABLE budget.recommendation_version
    ADD CONSTRAINT fk_recommendation_version_created_by FOREIGN KEY (created_by) REFERENCES employee.employee (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_recommendation_version_created_charge FOREIGN KEY (created_charge_id) REFERENCES employee.employee_charge (id) ON DELETE RESTRICT;
