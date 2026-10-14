-- BudgetP1ProposalSpine step 4: trigger attachment (design §11.2) and the rule triggers of step 3.

-- T-29 proposal (mutable).
CREATE TRIGGER trg_proposal_require_actor BEFORE INSERT OR UPDATE ON budget.proposal
    FOR EACH ROW EXECUTE FUNCTION budget.fn_require_actor();
CREATE TRIGGER trg_proposal_audit AFTER INSERT OR UPDATE ON budget.proposal
    FOR EACH ROW EXECUTE FUNCTION budget.fn_audit_row();
CREATE TRIGGER trg_proposal_forbid_stmt BEFORE DELETE OR TRUNCATE ON budget.proposal
    FOR EACH STATEMENT EXECUTE FUNCTION budget.fn_forbid_change();
CREATE TRIGGER trg_proposal_insert_rules BEFORE INSERT ON budget.proposal
    FOR EACH ROW EXECUTE FUNCTION budget.fn_proposal_insert_rules();
CREATE TRIGGER trg_proposal_fixed_columns BEFORE UPDATE ON budget.proposal
    FOR EACH ROW EXECUTE FUNCTION budget.fn_proposal_fixed_columns();
CREATE CONSTRAINT TRIGGER trg_proposal_state_change AFTER UPDATE OF state_code, holder_office_id ON budget.proposal
    DEFERRABLE INITIALLY DEFERRED
    FOR EACH ROW EXECUTE FUNCTION budget.fn_proposal_state_change();

-- T-30 demand_line (mutable; DB-14 capture after insert/update of the two figures).
CREATE TRIGGER trg_demand_line_require_actor BEFORE INSERT OR UPDATE ON budget.demand_line
    FOR EACH ROW EXECUTE FUNCTION budget.fn_require_actor();
CREATE TRIGGER trg_demand_line_audit AFTER INSERT OR UPDATE ON budget.demand_line
    FOR EACH ROW EXECUTE FUNCTION budget.fn_audit_row();
CREATE TRIGGER trg_demand_line_forbid_stmt BEFORE DELETE OR TRUNCATE ON budget.demand_line
    FOR EACH STATEMENT EXECUTE FUNCTION budget.fn_forbid_change();
CREATE TRIGGER trg_demand_line_rules BEFORE INSERT OR UPDATE ON budget.demand_line
    FOR EACH ROW EXECUTE FUNCTION budget.fn_demand_line_rules();
CREATE TRIGGER trg_demand_line_capture_figure_version AFTER INSERT OR UPDATE OF rbe_amount, obe_next_amount ON budget.demand_line
    FOR EACH ROW EXECUTE FUNCTION budget.fn_capture_figure_version();

-- T-31 figure_version (append-only, written by fn_capture_figure_version only).
CREATE TRIGGER trg_figure_version_require_actor BEFORE INSERT OR UPDATE ON budget.figure_version
    FOR EACH ROW EXECUTE FUNCTION budget.fn_require_actor();
CREATE TRIGGER trg_figure_version_audit AFTER INSERT OR UPDATE ON budget.figure_version
    FOR EACH ROW EXECUTE FUNCTION budget.fn_audit_row();
CREATE TRIGGER trg_figure_version_forbid_row BEFORE UPDATE OR DELETE ON budget.figure_version
    FOR EACH ROW EXECUTE FUNCTION budget.fn_forbid_change();
CREATE TRIGGER trg_figure_version_forbid_stmt BEFORE DELETE OR TRUNCATE ON budget.figure_version
    FOR EACH STATEMENT EXECUTE FUNCTION budget.fn_forbid_change();

-- T-32 observation (DB-15 protect, §5.5 row d).
CREATE TRIGGER trg_observation_require_actor BEFORE INSERT OR UPDATE ON budget.observation
    FOR EACH ROW EXECUTE FUNCTION budget.fn_require_actor();
CREATE TRIGGER trg_observation_audit AFTER INSERT OR UPDATE ON budget.observation
    FOR EACH ROW EXECUTE FUNCTION budget.fn_audit_row();
CREATE TRIGGER trg_observation_forbid_stmt BEFORE DELETE OR TRUNCATE ON budget.observation
    FOR EACH STATEMENT EXECUTE FUNCTION budget.fn_forbid_change();
CREATE TRIGGER trg_observation_protect BEFORE INSERT OR UPDATE ON budget.observation
    FOR EACH ROW EXECUTE FUNCTION budget.fn_observation_protect();

-- T-33 proposal_movement (append-only; DB-12).
CREATE TRIGGER trg_proposal_movement_require_actor BEFORE INSERT OR UPDATE ON budget.proposal_movement
    FOR EACH ROW EXECUTE FUNCTION budget.fn_require_actor();
CREATE TRIGGER trg_proposal_movement_audit AFTER INSERT OR UPDATE ON budget.proposal_movement
    FOR EACH ROW EXECUTE FUNCTION budget.fn_audit_row();
CREATE TRIGGER trg_proposal_movement_forbid_row BEFORE UPDATE OR DELETE ON budget.proposal_movement
    FOR EACH ROW EXECUTE FUNCTION budget.fn_forbid_change();
CREATE TRIGGER trg_proposal_movement_forbid_stmt BEFORE DELETE OR TRUNCATE ON budget.proposal_movement
    FOR EACH STATEMENT EXECUTE FUNCTION budget.fn_forbid_change();
CREATE TRIGGER trg_proposal_movement_rules BEFORE INSERT ON budget.proposal_movement
    FOR EACH ROW EXECUTE FUNCTION budget.fn_movement_rules();
