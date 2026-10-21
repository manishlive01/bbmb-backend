-- BudgetP1StateTransitionSeed step 2: T-16 CHECK (DDD T-16) and trigger attachment (design §11.2): the
-- actor trigger only checks the context (no actor columns), audit, append-only row and statement guards.
ALTER TABLE budget.state_transition
    ADD CONSTRAINT ck_state_transition_action CHECK (action_code IN ('SUBMIT', 'RECALL', 'OPEN', 'RETURN', 'RESUBMIT',
        'FORWARD', 'FORWARD_FINANCE', 'SEND_BACK', 'FINALISE', 'REOPEN', 'PLACE_BSC', 'ALLOCATE_RBE', 'REFER_BACK',
        'PLACE_FB', 'ALLOCATE_OBE', 'FREEZE'));

CREATE TRIGGER trg_state_transition_require_actor BEFORE INSERT OR UPDATE ON budget.state_transition
    FOR EACH ROW EXECUTE FUNCTION budget.fn_require_actor();
CREATE TRIGGER trg_state_transition_audit AFTER INSERT OR UPDATE ON budget.state_transition
    FOR EACH ROW EXECUTE FUNCTION budget.fn_audit_row();
CREATE TRIGGER trg_state_transition_forbid_row BEFORE UPDATE OR DELETE ON budget.state_transition
    FOR EACH ROW EXECUTE FUNCTION budget.fn_forbid_change();
CREATE TRIGGER trg_state_transition_forbid_stmt BEFORE DELETE OR TRUNCATE ON budget.state_transition
    FOR EACH STATEMENT EXECUTE FUNCTION budget.fn_forbid_change();
