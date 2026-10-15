-- BudgetP5Reappropriation Down (dev only, design §11.1 rule 6): drop V-10 (it depends on reappropriation and
-- unforeseen_authorisation) and the standalone functions before EF drops the tables and the T-45 column. The
-- triggers on T-46/T-47 are dropped with their tables by DropTable; the partial UK is dropped with the table.
DROP VIEW IF EXISTS budget.v_reappropriation_register;

DROP FUNCTION IF EXISTS budget.fn_reappropriation_submit_balance();
DROP FUNCTION IF EXISTS budget.fn_reappropriation_balance();
DROP FUNCTION IF EXISTS budget.fn_reappropriation_source_rules();
DROP FUNCTION IF EXISTS budget.fn_reappropriation_status_once();
