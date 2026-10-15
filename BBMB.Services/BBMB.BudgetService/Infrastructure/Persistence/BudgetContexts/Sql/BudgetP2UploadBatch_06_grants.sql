-- BudgetP2UploadBatch step 6: conditional grants (design §11.1 rule 7, OI-03; DDD §3.3).
-- Append-only: SELECT and INSERT only for bud_app, no UPDATE and no DELETE; bud_report reads only.
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'bud_app') THEN
        GRANT SELECT, INSERT ON budget.upload_batch TO bud_app;
    END IF;
    IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'bud_report') THEN
        GRANT SELECT ON budget.upload_batch TO bud_report;
    END IF;
END
$$;
