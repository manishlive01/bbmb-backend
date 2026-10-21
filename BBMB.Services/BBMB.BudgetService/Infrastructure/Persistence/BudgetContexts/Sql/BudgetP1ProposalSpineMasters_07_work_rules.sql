-- BudgetP1ProposalSpineMasters step 7 (design §5.2 work order, step 4; DDD T-23).
ALTER TABLE budget.work
    -- work_class: CHECK in AA, AMR.
    ADD CONSTRAINT ck_work_class CHECK (work_class IS NULL OR work_class IN ('AA', 'AMR')),
    -- entered_via: CHECK in the five DDD values.
    ADD CONSTRAINT ck_work_entered_via CHECK (entered_via IS NULL OR entered_via IN
        ('AA_ENTRY', 'DEMAND_ENTRY', 'TS_ENTRY', 'REAPPROP_NEW', 'LEGACY_OPENING')),
    -- §5.5 row a: work_class and entered_via NOT NULL in effect for every row with pre_ddd = false.
    ADD CONSTRAINT ck_work_pre_ddd CHECK (pre_ddd OR (work_class IS NOT NULL AND entered_via IS NOT NULL)),
    -- preceding_amr_work_id: only for AMR.
    ADD CONSTRAINT ck_work_preceding_amr CHECK (preceding_amr_work_id IS NULL OR work_class = 'AMR'),
    -- The three creation columns are all-or-none (§5.2).
    ADD CONSTRAINT ck_work_created_all_or_none CHECK (
        (created_at IS NULL) = (created_by IS NULL) AND (created_by IS NULL) = (created_charge_id IS NULL)),
    ADD CONSTRAINT fk_work_created_by FOREIGN KEY (created_by)
        REFERENCES employee.employee (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_work_created_charge FOREIGN KEY (created_charge_id)
        REFERENCES employee.employee_charge (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_work_updated_by FOREIGN KEY (updated_by)
        REFERENCES employee.employee (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_work_updated_charge FOREIGN KEY (updated_charge_id)
        REFERENCES employee.employee_charge (id) ON DELETE RESTRICT;

-- DDD T-23 "FK (six columns) T-07" (design §5.2, §5.5 row g). Guard first: list every work whose six
-- segments match no budget_head_master row and abort (§11.1 rule 4). The owner corrects them (OI-41).
DO $$
DECLARE
    v_list text;
BEGIN
    SELECT string_agg(format('work %s (%s-%s-%s-%s-%s-%s)', w.id, w.major_acc_code, w.sub_major_acc_code,
                             w.minor_acc_code, w.sub_minor_acc_code, w.detail_acc_code, w.object_acc_code),
                      '; ' ORDER BY w.id)
    INTO v_list
    FROM budget.work w
    WHERE NOT EXISTS (
        SELECT 1 FROM budget.budget_head_master h
        WHERE h.major_acc_code = w.major_acc_code
          AND h.sub_major_acc_code = w.sub_major_acc_code
          AND h.minor_acc_code = w.minor_acc_code
          AND h.sub_minor_acc_code = w.sub_minor_acc_code
          AND h.detail_acc_code = w.detail_acc_code
          AND h.object_acc_code = w.object_acc_code);
    IF v_list IS NOT NULL THEN
        RAISE EXCEPTION 'BudgetP1ProposalSpineMasters aborted: works whose six segments match no account code: %', v_list;
    END IF;
END
$$;

ALTER TABLE budget.work
    ADD CONSTRAINT fk_work_account_code
        FOREIGN KEY (major_acc_code, sub_major_acc_code, minor_acc_code, sub_minor_acc_code, detail_acc_code, object_acc_code)
        REFERENCES budget.budget_head_master
            (major_acc_code, sub_major_acc_code, minor_acc_code, sub_minor_acc_code, detail_acc_code, object_acc_code)
        ON DELETE RESTRICT;
