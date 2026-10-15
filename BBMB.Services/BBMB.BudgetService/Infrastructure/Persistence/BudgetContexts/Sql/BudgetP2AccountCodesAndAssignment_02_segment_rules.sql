-- BudgetP2AccountCodesAndAssignment step 2 (after the guard and the description widening): DDD T-01 to T-06 code
-- CHECKs, the two composite T-07 FKs the DDD names and the database lacked, and the row-level UPDATE guard that
-- makes the segment masters insert-only (design §4.7, §11.2; DDD §3.3 grants bud_app no UPDATE on T-01 to T-06).
-- The statement-level DELETE / TRUNCATE guard has been attached since P1.
ALTER TABLE budget.fa_major_head     ADD CONSTRAINT ck_fa_major_head_code     CHECK (major_acc_code ~ '^\d{4}$');
ALTER TABLE budget.fa_sub_major_head ADD CONSTRAINT ck_fa_sub_major_head_code CHECK (sub_major_acc_code ~ '^\d{2}$');
ALTER TABLE budget.fa_minor_head     ADD CONSTRAINT ck_fa_minor_head_code     CHECK (minor_acc_code ~ '^\d{3}$');
ALTER TABLE budget.fa_sub_minor_head ADD CONSTRAINT ck_fa_sub_minor_head_code CHECK (sub_minor_acc_code ~ '^\d{2}$');
ALTER TABLE budget.fa_detail_head    ADD CONSTRAINT ck_fa_detail_head_code    CHECK (detail_acc_code ~ '^\d{3}$');
ALTER TABLE budget.fa_object_head    ADD CONSTRAINT ck_fa_object_head_code    CHECK (object_acc_code ~ '^\d{3}$');

-- T-07 sub_major_acc_code: FK (major, sub-major) T-02; sub_minor_acc_code: FK (minor, sub-minor) T-04 (RESTRICT).
ALTER TABLE budget.budget_head_master
    ADD CONSTRAINT fk_budget_head_master_sub_major FOREIGN KEY (major_acc_code, sub_major_acc_code)
        REFERENCES budget.fa_sub_major_head (major_acc_code, sub_major_acc_code) ON DELETE RESTRICT ON UPDATE RESTRICT,
    ADD CONSTRAINT fk_budget_head_master_sub_minor FOREIGN KEY (minor_acc_code, sub_minor_acc_code)
        REFERENCES budget.fa_sub_minor_head (minor_acc_code, sub_minor_acc_code) ON DELETE RESTRICT ON UPDATE RESTRICT;

CREATE TRIGGER trg_fa_major_head_forbid_row BEFORE UPDATE ON budget.fa_major_head
    FOR EACH ROW EXECUTE FUNCTION budget.fn_forbid_change();
CREATE TRIGGER trg_fa_sub_major_head_forbid_row BEFORE UPDATE ON budget.fa_sub_major_head
    FOR EACH ROW EXECUTE FUNCTION budget.fn_forbid_change();
CREATE TRIGGER trg_fa_minor_head_forbid_row BEFORE UPDATE ON budget.fa_minor_head
    FOR EACH ROW EXECUTE FUNCTION budget.fn_forbid_change();
CREATE TRIGGER trg_fa_sub_minor_head_forbid_row BEFORE UPDATE ON budget.fa_sub_minor_head
    FOR EACH ROW EXECUTE FUNCTION budget.fn_forbid_change();
CREATE TRIGGER trg_fa_detail_head_forbid_row BEFORE UPDATE ON budget.fa_detail_head
    FOR EACH ROW EXECUTE FUNCTION budget.fn_forbid_change();
CREATE TRIGGER trg_fa_object_head_forbid_row BEFORE UPDATE ON budget.fa_object_head
    FOR EACH ROW EXECUTE FUNCTION budget.fn_forbid_change();
