-- BudgetP2UploadBatch step 2: DDD T-34 CHECK and the cross-schema actor references EF does not create
-- (employee schema). Not gated: P2 carries no DDD deviation awaiting a decision (design §11.4). The migration
-- writes no data, so the migration actor is not needed (design §11.1 rule 8).
-- T-34 budget.upload_batch (append-only; the uploading officer and time are the [C] columns).
ALTER TABLE budget.upload_batch
    ADD CONSTRAINT ck_upload_batch_lines_committed CHECK (lines_committed > 0),
    ADD CONSTRAINT fk_upload_batch_created_by FOREIGN KEY (created_by) REFERENCES employee.employee (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_upload_batch_created_charge FOREIGN KEY (created_charge_id) REFERENCES employee.employee_charge (id) ON DELETE RESTRICT;
