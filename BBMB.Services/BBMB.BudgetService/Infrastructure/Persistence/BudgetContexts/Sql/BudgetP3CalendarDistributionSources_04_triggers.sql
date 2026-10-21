-- BudgetP3CalendarDistributionSources step 4: trigger attachment (design §11.2). Every new table carries
-- fn_require_actor, fn_audit_row and the statement-level DELETE/TRUNCATE guard. T-18 is append-only (DB-27)
-- and also carries the row-level fn_forbid_change. T-39 carries its placement rule and the DB-18 deferred
-- ceiling check; T-37 gains the redistribution-flag trigger.
-- T-20 calendar (mutable: last_date).
CREATE TRIGGER trg_calendar_require_actor BEFORE INSERT OR UPDATE ON budget.calendar
    FOR EACH ROW EXECUTE FUNCTION budget.fn_require_actor();
CREATE TRIGGER trg_calendar_audit AFTER INSERT OR UPDATE ON budget.calendar
    FOR EACH ROW EXECUTE FUNCTION budget.fn_audit_row();
CREATE TRIGGER trg_calendar_forbid_stmt BEFORE DELETE OR TRUNCATE ON budget.calendar
    FOR EACH STATEMENT EXECUTE FUNCTION budget.fn_forbid_change();
-- T-39 distribution (mutable: the shares; DB-01 placement; DB-18 at commit).
CREATE TRIGGER trg_distribution_require_actor BEFORE INSERT OR UPDATE ON budget.distribution
    FOR EACH ROW EXECUTE FUNCTION budget.fn_require_actor();
CREATE TRIGGER trg_distribution_audit AFTER INSERT OR UPDATE ON budget.distribution
    FOR EACH ROW EXECUTE FUNCTION budget.fn_audit_row();
CREATE TRIGGER trg_distribution_forbid_stmt BEFORE DELETE OR TRUNCATE ON budget.distribution
    FOR EACH STATEMENT EXECUTE FUNCTION budget.fn_forbid_change();
CREATE TRIGGER trg_distribution_rules BEFORE INSERT OR UPDATE ON budget.distribution
    FOR EACH ROW EXECUTE FUNCTION budget.fn_distribution_rules();
CREATE CONSTRAINT TRIGGER trg_distribution_ceiling AFTER INSERT OR UPDATE ON budget.distribution
    DEFERRABLE INITIALLY DEFERRED
    FOR EACH ROW EXECUTE FUNCTION budget.fn_distribution_ceiling_check();
-- T-18 expenditure_source_setting (append-only).
CREATE TRIGGER trg_expenditure_source_require_actor BEFORE INSERT OR UPDATE ON budget.expenditure_source_setting
    FOR EACH ROW EXECUTE FUNCTION budget.fn_require_actor();
CREATE TRIGGER trg_expenditure_source_audit AFTER INSERT OR UPDATE ON budget.expenditure_source_setting
    FOR EACH ROW EXECUTE FUNCTION budget.fn_audit_row();
CREATE TRIGGER trg_expenditure_source_forbid_row BEFORE UPDATE OR DELETE ON budget.expenditure_source_setting
    FOR EACH ROW EXECUTE FUNCTION budget.fn_forbid_change();
CREATE TRIGGER trg_expenditure_source_forbid_stmt BEFORE DELETE OR TRUNCATE ON budget.expenditure_source_setting
    FOR EACH STATEMENT EXECUTE FUNCTION budget.fn_forbid_change();
-- T-37 recommendation: the redistribution flag (BUD-279), after a recommended-figure change.
CREATE TRIGGER trg_recommendation_redistribution AFTER UPDATE ON budget.ce_recommendation
    FOR EACH ROW EXECUTE FUNCTION budget.fn_recommendation_redistribution_flag();
