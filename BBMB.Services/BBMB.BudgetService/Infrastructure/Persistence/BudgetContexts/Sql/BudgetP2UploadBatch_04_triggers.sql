-- BudgetP2UploadBatch step 4: trigger attachment (design §11.2). T-34 upload_batch is append-only (DB-27):
-- fn_require_actor fills the [C] actor columns, fn_audit_row writes T-51, and fn_forbid_change (row and
-- statement) refuses any UPDATE, DELETE or TRUNCATE (§11.2 forbid_change list carries T-34).
CREATE TRIGGER trg_upload_batch_require_actor BEFORE INSERT OR UPDATE ON budget.upload_batch
    FOR EACH ROW EXECUTE FUNCTION budget.fn_require_actor();
CREATE TRIGGER trg_upload_batch_audit AFTER INSERT OR UPDATE ON budget.upload_batch
    FOR EACH ROW EXECUTE FUNCTION budget.fn_audit_row();
CREATE TRIGGER trg_upload_batch_forbid_row BEFORE UPDATE OR DELETE ON budget.upload_batch
    FOR EACH ROW EXECUTE FUNCTION budget.fn_forbid_change();
CREATE TRIGGER trg_upload_batch_forbid_stmt BEFORE DELETE OR TRUNCATE ON budget.upload_batch
    FOR EACH STATEMENT EXECUTE FUNCTION budget.fn_forbid_change();
