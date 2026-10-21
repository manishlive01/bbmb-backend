-- BudgetP1FiguresAndRecommendations step 4: trigger attachment (design §11.2) and the rule triggers.

-- T-35 expenditure_figure (append-only).
CREATE TRIGGER trg_expenditure_figure_require_actor BEFORE INSERT OR UPDATE ON budget.expenditure_figure
    FOR EACH ROW EXECUTE FUNCTION budget.fn_require_actor();
CREATE TRIGGER trg_expenditure_figure_audit AFTER INSERT OR UPDATE ON budget.expenditure_figure
    FOR EACH ROW EXECUTE FUNCTION budget.fn_audit_row();
CREATE TRIGGER trg_expenditure_figure_forbid_row BEFORE UPDATE OR DELETE ON budget.expenditure_figure
    FOR EACH ROW EXECUTE FUNCTION budget.fn_forbid_change();
CREATE TRIGGER trg_expenditure_figure_forbid_stmt BEFORE DELETE OR TRUNCATE ON budget.expenditure_figure
    FOR EACH STATEMENT EXECUTE FUNCTION budget.fn_forbid_change();

-- T-36 opening_figure (mutable until the origin proposal is first submitted, DB-16).
CREATE TRIGGER trg_opening_figure_require_actor BEFORE INSERT OR UPDATE ON budget.opening_figure
    FOR EACH ROW EXECUTE FUNCTION budget.fn_require_actor();
CREATE TRIGGER trg_opening_figure_audit AFTER INSERT OR UPDATE ON budget.opening_figure
    FOR EACH ROW EXECUTE FUNCTION budget.fn_audit_row();
CREATE TRIGGER trg_opening_figure_forbid_stmt BEFORE DELETE OR TRUNCATE ON budget.opening_figure
    FOR EACH STATEMENT EXECUTE FUNCTION budget.fn_forbid_change();
CREATE TRIGGER trg_opening_figure_rules BEFORE INSERT OR UPDATE ON budget.opening_figure
    FOR EACH ROW EXECUTE FUNCTION budget.fn_opening_figure_rules();

-- T-37 ce_recommendation (mutable; DB-14 capture into T-38).
CREATE TRIGGER trg_ce_recommendation_require_actor BEFORE INSERT OR UPDATE ON budget.ce_recommendation
    FOR EACH ROW EXECUTE FUNCTION budget.fn_require_actor();
CREATE TRIGGER trg_ce_recommendation_audit AFTER INSERT OR UPDATE ON budget.ce_recommendation
    FOR EACH ROW EXECUTE FUNCTION budget.fn_audit_row();
CREATE TRIGGER trg_ce_recommendation_forbid_stmt BEFORE DELETE OR TRUNCATE ON budget.ce_recommendation
    FOR EACH STATEMENT EXECUTE FUNCTION budget.fn_forbid_change();
CREATE TRIGGER trg_ce_recommendation_rules BEFORE INSERT OR UPDATE ON budget.ce_recommendation
    FOR EACH ROW EXECUTE FUNCTION budget.fn_recommendation_rules();
CREATE TRIGGER trg_ce_recommendation_capture_version AFTER INSERT OR UPDATE ON budget.ce_recommendation
    FOR EACH ROW EXECUTE FUNCTION budget.fn_capture_recommendation_version();

-- T-38 recommendation_version (append-only, written by fn_capture_recommendation_version only).
CREATE TRIGGER trg_recommendation_version_require_actor BEFORE INSERT OR UPDATE ON budget.recommendation_version
    FOR EACH ROW EXECUTE FUNCTION budget.fn_require_actor();
CREATE TRIGGER trg_recommendation_version_audit AFTER INSERT OR UPDATE ON budget.recommendation_version
    FOR EACH ROW EXECUTE FUNCTION budget.fn_audit_row();
CREATE TRIGGER trg_recommendation_version_forbid_row BEFORE UPDATE OR DELETE ON budget.recommendation_version
    FOR EACH ROW EXECUTE FUNCTION budget.fn_forbid_change();
CREATE TRIGGER trg_recommendation_version_forbid_stmt BEFORE DELETE OR TRUNCATE ON budget.recommendation_version
    FOR EACH STATEMENT EXECUTE FUNCTION budget.fn_forbid_change();
