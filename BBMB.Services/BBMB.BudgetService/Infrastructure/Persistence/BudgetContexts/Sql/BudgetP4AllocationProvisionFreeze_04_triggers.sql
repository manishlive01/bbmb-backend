-- BudgetP4AllocationProvisionFreeze step 4: trigger attachment (design §11.2). Every new table T-40..T-44,
-- T-48..T-50 carries fn_require_actor, fn_audit_row and the statement-level DELETE/TRUNCATE guard. The
-- append-only tables (T-40 allocation, T-42 provision_movement, T-44 freeze, T-49 movement, T-50
-- authorisation) also carry the row-level fn_forbid_change (BEFORE UPDATE OR DELETE). T-41 provision and
-- T-43 estimate_allocation are mutable (supersession marker / the amount), so they take no row-level forbid;
-- T-41's write-once rule is fn_provision_write_once. The DB-rule triggers of step 3 are attached last.

-- ===================== T-40 allocation (append-only) =====================
CREATE TRIGGER trg_allocation_require_actor BEFORE INSERT OR UPDATE ON budget.allocation
    FOR EACH ROW EXECUTE FUNCTION budget.fn_require_actor();
CREATE TRIGGER trg_allocation_audit AFTER INSERT OR UPDATE ON budget.allocation
    FOR EACH ROW EXECUTE FUNCTION budget.fn_audit_row();
CREATE TRIGGER trg_allocation_forbid_row BEFORE UPDATE OR DELETE ON budget.allocation
    FOR EACH ROW EXECUTE FUNCTION budget.fn_forbid_change();
CREATE TRIGGER trg_allocation_forbid_stmt BEFORE DELETE OR TRUNCATE ON budget.allocation
    FOR EACH STATEMENT EXECUTE FUNCTION budget.fn_forbid_change();

-- ===================== T-41 provision (write-once except supersession) =====================
CREATE TRIGGER trg_provision_require_actor BEFORE INSERT OR UPDATE ON budget.provision
    FOR EACH ROW EXECUTE FUNCTION budget.fn_require_actor();
CREATE TRIGGER trg_provision_audit AFTER INSERT OR UPDATE ON budget.provision
    FOR EACH ROW EXECUTE FUNCTION budget.fn_audit_row();
CREATE TRIGGER trg_provision_write_once BEFORE UPDATE ON budget.provision
    FOR EACH ROW EXECUTE FUNCTION budget.fn_provision_write_once();
CREATE TRIGGER trg_provision_forbid_stmt BEFORE DELETE OR TRUNCATE ON budget.provision
    FOR EACH STATEMENT EXECUTE FUNCTION budget.fn_forbid_change();
-- At most one live provision per holder key and basis, at commit (deferred so supersession is atomic).
CREATE CONSTRAINT TRIGGER trg_provision_live_unique AFTER INSERT OR UPDATE ON budget.provision
    DEFERRABLE INITIALLY DEFERRED
    FOR EACH ROW EXECUTE FUNCTION budget.fn_provision_live_unique();

-- ===================== T-42 provision_movement (ledger, append-only) =====================
CREATE TRIGGER trg_provision_movement_require_actor BEFORE INSERT OR UPDATE ON budget.provision_movement
    FOR EACH ROW EXECUTE FUNCTION budget.fn_require_actor();
CREATE TRIGGER trg_provision_movement_audit AFTER INSERT OR UPDATE ON budget.provision_movement
    FOR EACH ROW EXECUTE FUNCTION budget.fn_audit_row();
CREATE TRIGGER trg_provision_movement_forbid_row BEFORE UPDATE OR DELETE ON budget.provision_movement
    FOR EACH ROW EXECUTE FUNCTION budget.fn_forbid_change();
CREATE TRIGGER trg_provision_movement_forbid_stmt BEFORE DELETE OR TRUNCATE ON budget.provision_movement
    FOR EACH STATEMENT EXECUTE FUNCTION budget.fn_forbid_change();
-- DB-19 and DB-22 when the movement is written.
CREATE TRIGGER trg_provision_movement_rules BEFORE INSERT ON budget.provision_movement
    FOR EACH ROW EXECUTE FUNCTION budget.fn_provision_movement_rules();
-- DB-21 at commit: one authorisation's (or re-appropriation's) movements sum to nil.
CREATE CONSTRAINT TRIGGER trg_provision_movement_balance AFTER INSERT ON budget.provision_movement
    DEFERRABLE INITIALLY DEFERRED
    FOR EACH ROW EXECUTE FUNCTION budget.fn_provision_movement_balance();

-- ===================== T-43 estimate_allocation (mutable) =====================
CREATE TRIGGER trg_estimate_allocation_require_actor BEFORE INSERT OR UPDATE ON budget.estimate_allocation
    FOR EACH ROW EXECUTE FUNCTION budget.fn_require_actor();
CREATE TRIGGER trg_estimate_allocation_audit AFTER INSERT OR UPDATE ON budget.estimate_allocation
    FOR EACH ROW EXECUTE FUNCTION budget.fn_audit_row();
CREATE TRIGGER trg_estimate_allocation_forbid_stmt BEFORE DELETE OR TRUNCATE ON budget.estimate_allocation
    FOR EACH STATEMENT EXECUTE FUNCTION budget.fn_forbid_change();
-- DB-23, VR-12 (M-28) at commit: total per provision not above the available.
CREATE CONSTRAINT TRIGGER trg_estimate_allocation_ceiling AFTER INSERT OR UPDATE ON budget.estimate_allocation
    DEFERRABLE INITIALLY DEFERRED
    FOR EACH ROW EXECUTE FUNCTION budget.fn_estimate_allocation_ceiling();

-- ===================== T-44 freeze (append-only) =====================
CREATE TRIGGER trg_freeze_require_actor BEFORE INSERT OR UPDATE ON budget.freeze
    FOR EACH ROW EXECUTE FUNCTION budget.fn_require_actor();
CREATE TRIGGER trg_freeze_audit AFTER INSERT OR UPDATE ON budget.freeze
    FOR EACH ROW EXECUTE FUNCTION budget.fn_audit_row();
CREATE TRIGGER trg_freeze_forbid_row BEFORE UPDATE OR DELETE ON budget.freeze
    FOR EACH ROW EXECUTE FUNCTION budget.fn_forbid_change();
CREATE TRIGGER trg_freeze_forbid_stmt BEFORE DELETE OR TRUNCATE ON budget.freeze
    FOR EACH STATEMENT EXECUTE FUNCTION budget.fn_forbid_change();
-- DB-19: the precondition check before, and the cycle-FROZEN effect after.
CREATE TRIGGER trg_freeze_preconditions BEFORE INSERT ON budget.freeze
    FOR EACH ROW EXECUTE FUNCTION budget.fn_freeze_preconditions();
CREATE TRIGGER trg_freeze_set_cycle_frozen AFTER INSERT ON budget.freeze
    FOR EACH ROW EXECUTE FUNCTION budget.fn_freeze_set_cycle_frozen();

-- ===================== T-48 unforeseen_request (mutable) =====================
CREATE TRIGGER trg_unforeseen_request_require_actor BEFORE INSERT OR UPDATE ON budget.unforeseen_request
    FOR EACH ROW EXECUTE FUNCTION budget.fn_require_actor();
CREATE TRIGGER trg_unforeseen_request_audit AFTER INSERT OR UPDATE ON budget.unforeseen_request
    FOR EACH ROW EXECUTE FUNCTION budget.fn_audit_row();
CREATE TRIGGER trg_unforeseen_request_forbid_stmt BEFORE DELETE OR TRUNCATE ON budget.unforeseen_request
    FOR EACH STATEMENT EXECUTE FUNCTION budget.fn_forbid_change();
-- DB-25: work has a recorded approval and no work code.
CREATE TRIGGER trg_unforeseen_request_rules BEFORE INSERT OR UPDATE ON budget.unforeseen_request
    FOR EACH ROW EXECUTE FUNCTION budget.fn_unforeseen_request_rules();

-- ===================== T-49 unforeseen_request_movement (append-only) =====================
CREATE TRIGGER trg_unforeseen_movement_require_actor BEFORE INSERT OR UPDATE ON budget.unforeseen_request_movement
    FOR EACH ROW EXECUTE FUNCTION budget.fn_require_actor();
CREATE TRIGGER trg_unforeseen_movement_audit AFTER INSERT OR UPDATE ON budget.unforeseen_request_movement
    FOR EACH ROW EXECUTE FUNCTION budget.fn_audit_row();
CREATE TRIGGER trg_unforeseen_movement_forbid_row BEFORE UPDATE OR DELETE ON budget.unforeseen_request_movement
    FOR EACH ROW EXECUTE FUNCTION budget.fn_forbid_change();
CREATE TRIGGER trg_unforeseen_movement_forbid_stmt BEFORE DELETE OR TRUNCATE ON budget.unforeseen_request_movement
    FOR EACH STATEMENT EXECUTE FUNCTION budget.fn_forbid_change();

-- ===================== T-50 unforeseen_authorisation (append-only) =====================
CREATE TRIGGER trg_unforeseen_authorisation_require_actor BEFORE INSERT OR UPDATE ON budget.unforeseen_authorisation
    FOR EACH ROW EXECUTE FUNCTION budget.fn_require_actor();
CREATE TRIGGER trg_unforeseen_authorisation_audit AFTER INSERT OR UPDATE ON budget.unforeseen_authorisation
    FOR EACH ROW EXECUTE FUNCTION budget.fn_audit_row();
CREATE TRIGGER trg_unforeseen_authorisation_forbid_row BEFORE UPDATE OR DELETE ON budget.unforeseen_authorisation
    FOR EACH ROW EXECUTE FUNCTION budget.fn_forbid_change();
CREATE TRIGGER trg_unforeseen_authorisation_forbid_stmt BEFORE DELETE OR TRUNCATE ON budget.unforeseen_authorisation
    FOR EACH STATEMENT EXECUTE FUNCTION budget.fn_forbid_change();
-- DB-26, VR-13 (M-29): the authorised amounts are not above the provision balance drawn upon.
CREATE TRIGGER trg_unforeseen_authorisation_balance BEFORE INSERT ON budget.unforeseen_authorisation
    FOR EACH ROW EXECUTE FUNCTION budget.fn_unforeseen_authorisation_balance();

-- ===================== T-37 ce_recommendation: DB-17 (minutes citation, P4) =====================
CREATE TRIGGER trg_recommendation_cites_minutes BEFORE UPDATE ON budget.ce_recommendation
    FOR EACH ROW EXECUTE FUNCTION budget.fn_recommendation_cites_minutes();
