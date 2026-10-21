-- BudgetP2AccountCodesAndAssignment step 3: trigger functions (DDD §6.3; DB-01, DB-02, DB-03). Where the rule has an
-- SRS message number the exception text is that number (M-05, M-13, M-15, M-33), so the global handler shows the
-- catalogue text (design §4.9); the others are backstops behind application pre-checks and raise the rule number.
-- ---------------------------------------------------------------------------------------------------
-- T-07 BEFORE UPDATE: trg_head_master_protect (DDD T-07 note; DB-03; ACC-008, BUD-047, BUD-268, BUD-308).
-- Refuses UPDATE of the six segments, account_code_id, head_desc, origin, eoffice_approval_ref and the creation
-- columns. not_for_use moves false -> true only, with its three companions set at that moment and fixed after;
-- the mark is refused (M-33) while a demand line refers to the code in any cycle (allocation and re-appropriation
-- references are added in P4 and P5 with their tables). party_emp_required changes only when the acting office is
-- the IT Cell (BUD-268): the office level is read rather than the role name, which is a placeholder (OI-06).
-- The legacy Financial Accounting columns are outside the Budgeting Module's DDD and are not restricted here.
-- DELETE and TRUNCATE are refused by the statement-level fn_forbid_change attached in P1.
-- ---------------------------------------------------------------------------------------------------
CREATE FUNCTION budget.fn_head_master_protect() RETURNS trigger
    LANGUAGE plpgsql
    SET search_path = budget, pg_temp
AS $fn$
DECLARE
    v_actor_office bigint := nullif(current_setting('budget.actor_office_id', true), '')::bigint;
BEGIN
    IF NEW.major_acc_code IS DISTINCT FROM OLD.major_acc_code OR NEW.sub_major_acc_code IS DISTINCT FROM OLD.sub_major_acc_code
       OR NEW.minor_acc_code IS DISTINCT FROM OLD.minor_acc_code OR NEW.sub_minor_acc_code IS DISTINCT FROM OLD.sub_minor_acc_code
       OR NEW.detail_acc_code IS DISTINCT FROM OLD.detail_acc_code OR NEW.object_acc_code IS DISTINCT FROM OLD.object_acc_code
       OR NEW.account_code_id IS DISTINCT FROM OLD.account_code_id
       OR NEW.head_desc IS DISTINCT FROM OLD.head_desc OR NEW.origin IS DISTINCT FROM OLD.origin
       OR NEW.eoffice_approval_ref IS DISTINCT FROM OLD.eoffice_approval_ref
       OR NEW.created_at IS DISTINCT FROM OLD.created_at OR NEW.created_by IS DISTINCT FROM OLD.created_by
       OR NEW.created_charge_id IS DISTINCT FROM OLD.created_charge_id THEN
        RAISE EXCEPTION 'DB-03' USING ERRCODE = 'P0001',
            DETAIL = 'an account code is neither altered nor re-described; mark it not for use and create a correct code (BUD-308)';
    END IF;
    IF OLD.not_for_use AND NOT NEW.not_for_use THEN
        RAISE EXCEPTION 'DB-03' USING ERRCODE = 'P0001', DETAIL = 'the not-for-use mark cannot be removed';
    END IF;
    IF OLD.not_for_use = NEW.not_for_use THEN
        IF NEW.not_for_use_ref IS DISTINCT FROM OLD.not_for_use_ref OR NEW.not_for_use_at IS DISTINCT FROM OLD.not_for_use_at
           OR NEW.not_for_use_by IS DISTINCT FROM OLD.not_for_use_by THEN
            RAISE EXCEPTION 'DB-03' USING ERRCODE = 'P0001', DETAIL = 'the particulars of the mark are set with it and fixed after';
        END IF;
    ELSIF EXISTS (SELECT 1 FROM budget.demand_line l WHERE l.account_code_id = NEW.account_code_id) THEN
        RAISE EXCEPTION 'M-33' USING ERRCODE = 'P0001';
    END IF;
    IF NEW.party_emp_required IS DISTINCT FROM OLD.party_emp_required AND NOT EXISTS (
        SELECT 1 FROM budget.office o WHERE o.office_id = v_actor_office AND o.office_level = 'IT_CELL'
    ) THEN
        RAISE EXCEPTION 'DB-03' USING ERRCODE = 'P0001', DETAIL = 'party_emp_required is altered under the IT Cell only (BUD-268)';
    END IF;
    RETURN NEW;
END
$fn$;
-- ---------------------------------------------------------------------------------------------------
-- T-23 BEFORE INSERT OR UPDATE OF the six segments: DB-03 reference side. A work is not classified under a code
-- marked not for use (DDD T-23 "Not a code marked not for use"). An update that leaves the segments unchanged is
-- not a new reference; a pre-DDD work's existing classification is likewise not re-checked unless changed.
-- ---------------------------------------------------------------------------------------------------
CREATE FUNCTION budget.fn_work_head_not_for_use() RETURNS trigger
    LANGUAGE plpgsql
    SET search_path = budget, pg_temp
AS $fn$
BEGIN
    IF TG_OP = 'UPDATE'
       AND NEW.major_acc_code IS NOT DISTINCT FROM OLD.major_acc_code AND NEW.sub_major_acc_code IS NOT DISTINCT FROM OLD.sub_major_acc_code
       AND NEW.minor_acc_code IS NOT DISTINCT FROM OLD.minor_acc_code AND NEW.sub_minor_acc_code IS NOT DISTINCT FROM OLD.sub_minor_acc_code
       AND NEW.detail_acc_code IS NOT DISTINCT FROM OLD.detail_acc_code AND NEW.object_acc_code IS NOT DISTINCT FROM OLD.object_acc_code THEN
        RETURN NEW;
    END IF;
    IF EXISTS (
        SELECT 1 FROM budget.budget_head_master h
        WHERE h.major_acc_code = NEW.major_acc_code AND h.sub_major_acc_code = NEW.sub_major_acc_code
          AND h.minor_acc_code = NEW.minor_acc_code AND h.sub_minor_acc_code = NEW.sub_minor_acc_code
          AND h.detail_acc_code = NEW.detail_acc_code AND h.object_acc_code = NEW.object_acc_code AND h.not_for_use
    ) THEN
        RAISE EXCEPTION 'M-05' USING ERRCODE = 'P0001';
    END IF;
    RETURN NEW;
END
$fn$;
-- ---------------------------------------------------------------------------------------------------
-- T-21 BEFORE INSERT OR UPDATE: DB-01 (the office is a Division, Circle or CE), DB-03 (no new assignment of a code
-- marked not for use), and DB-02 (removal by marking only, once, and refused while a non-deleted head-wise demand
-- line of the office's demands uses the head; the provision arm is added in P4 with T-41). The removal particulars
-- are taken from the actor context, never from the client (DDD §7.1).
-- ---------------------------------------------------------------------------------------------------
CREATE FUNCTION budget.fn_account_head_assignment_rules() RETURNS trigger
    LANGUAGE plpgsql
    SET search_path = budget, pg_temp
AS $fn$
BEGIN
    IF TG_OP = 'INSERT' THEN
        IF NOT EXISTS (SELECT 1 FROM budget.office o WHERE o.office_id = NEW.office_id
                       AND o.office_level IN ('DIVISION', 'CIRCLE', 'CE')) THEN
            RAISE EXCEPTION 'DB-01' USING ERRCODE = 'P0001', DETAIL = 'an assignment is to a Division, Circle or CE office';
        END IF;
        IF NEW.removed_at IS NOT NULL OR NEW.removed_by IS NOT NULL THEN
            RAISE EXCEPTION 'DB-02' USING ERRCODE = 'P0001', DETAIL = 'an assignment is written live';
        END IF;
        IF EXISTS (SELECT 1 FROM budget.budget_head_master h WHERE h.account_code_id = NEW.account_code_id AND h.not_for_use) THEN
            RAISE EXCEPTION 'M-05' USING ERRCODE = 'P0001';
        END IF;
        RETURN NEW;
    END IF;
    IF NEW.office_id <> OLD.office_id OR NEW.account_code_id <> OLD.account_code_id OR NEW.source <> OLD.source THEN
        RAISE EXCEPTION 'DB-02' USING ERRCODE = 'P0001', DETAIL = 'office, head and source of an assignment are fixed';
    END IF;
    IF OLD.removed_at IS NOT NULL THEN
        IF NEW.removed_at IS DISTINCT FROM OLD.removed_at OR NEW.removed_by IS DISTINCT FROM OLD.removed_by THEN
            RAISE EXCEPTION 'DB-02' USING ERRCODE = 'P0001', DETAIL = 'a removed assignment stays removed';
        END IF;
        RETURN NEW;
    END IF;
    IF NEW.removed_at IS NULL THEN
        NEW.removed_by := NULL;
        RETURN NEW;
    END IF;
    IF EXISTS (
        SELECT 1 FROM budget.demand_line l JOIN budget.proposal p ON p.proposal_id = l.proposal_id
        WHERE p.origin_office_id = NEW.office_id AND l.account_code_id = NEW.account_code_id
          AND l.line_type = 'HEAD' AND NOT l.is_deleted
    ) THEN
        RAISE EXCEPTION 'M-15' USING ERRCODE = 'P0001';
    END IF;
    NEW.removed_at := clock_timestamp();
    NEW.removed_by := nullif(current_setting('budget.actor_employee_id', true), '')::uuid;
    RETURN NEW;
END
$fn$;
-- ---------------------------------------------------------------------------------------------------
-- T-30 BEFORE INSERT OR UPDATE OF account_code_id: DB-02 demand side (VR-06, BUD-136, M-13). A head-wise line is
-- accepted only for a head assigned to the proposal's origin office and not removed. Checked on a new reference
-- only (insert, or a change of head), so removing an assignment later is governed by the T-21 side.
-- ---------------------------------------------------------------------------------------------------
CREATE FUNCTION budget.fn_demand_line_head_assigned() RETURNS trigger
    LANGUAGE plpgsql
    SET search_path = budget, pg_temp
AS $fn$
BEGIN
    IF NEW.line_type <> 'HEAD' OR (TG_OP = 'UPDATE' AND NEW.account_code_id IS NOT DISTINCT FROM OLD.account_code_id) THEN
        RETURN NEW;
    END IF;
    IF NOT EXISTS (
        SELECT 1 FROM budget.account_head_assignment a JOIN budget.proposal p ON p.origin_office_id = a.office_id
        WHERE p.proposal_id = NEW.proposal_id AND a.account_code_id = NEW.account_code_id AND a.removed_at IS NULL
    ) THEN
        RAISE EXCEPTION 'M-13' USING ERRCODE = 'P0001';
    END IF;
    RETURN NEW;
END
$fn$;
