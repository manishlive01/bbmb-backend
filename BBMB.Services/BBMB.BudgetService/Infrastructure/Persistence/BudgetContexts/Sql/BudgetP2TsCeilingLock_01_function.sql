-- ---------------------------------------------------------------------------------------------------
-- DB-09 at commit (VR-02, M-06), serialised per approval.
-- Replaces the BudgetP2AttachmentsApprovalsSanctions body (published; changed only by this later
-- migration). The original took no lock: under READ COMMITTED two transactions whose commit triggers run
-- concurrently do not see each other's sanction, so each passes alone and together they exceed the
-- ceiling (write-skew). The check now takes a transaction-level advisory lock keyed on the approval
-- after resolving it and before the SUM. A concurrent writer for the same approval waits until the
-- first commits; its SUM statement then takes a fresh snapshot (READ COMMITTED, volatile function) and
-- sees the first sanction. Application pre-checks (TechnicalSanctionRules.CheckAaCeilingAsync) take the
-- same key: pg_advisory_xact_lock(hashtext('budget.ts_ceiling'), (admin_approval_id % 2147483647)::int).
-- A key collision between two approvals only serialises them; it never admits an excess.
-- ---------------------------------------------------------------------------------------------------
CREATE OR REPLACE FUNCTION budget.fn_ts_ceiling_check() RETURNS trigger
    LANGUAGE plpgsql
    SET search_path = budget, pg_temp
AS $fn$
DECLARE
    v_approval bigint;
    v_amount   numeric;
    v_total    numeric;
    v_pct      numeric;
BEGIN
    SELECT t.admin_approval_id INTO v_approval
    FROM budget.technical_sanction t
    WHERE t.technical_sanction_id = NEW.technical_sanction_id AND t.tab = 'AA';
    IF v_approval IS NULL THEN
        RETURN NULL;
    END IF;
    PERFORM pg_advisory_xact_lock(hashtext('budget.ts_ceiling'), (v_approval % 2147483647)::int);
    SELECT v.amount_sanctioned INTO v_amount
    FROM budget.admin_approval a
    JOIN budget.admin_approval_version v ON v.admin_approval_id = a.admin_approval_id AND v.version_no = a.current_version_no
    WHERE a.admin_approval_id = v_approval;
    SELECT coalesce(sum(v.ts_amount), 0) INTO v_total
    FROM budget.technical_sanction t
    JOIN budget.technical_sanction_version v ON v.technical_sanction_id = t.technical_sanction_id AND v.version_no = t.current_version_no
    WHERE t.admin_approval_id = v_approval AND t.tab = 'AA';
    SELECT s.numeric_value INTO v_pct
    FROM budget.setting s
    WHERE s.setting_key = 'TS_CEILING_PCT' AND s.effective_from <= now()
    ORDER BY s.effective_from DESC
    LIMIT 1;
    IF v_pct IS NULL THEN
        RAISE EXCEPTION 'DB-09' USING ERRCODE = 'P0001', DETAIL = 'no TS_CEILING_PCT is in force';
    END IF;
    IF v_amount IS NULL OR v_total * 100 > v_pct * v_amount THEN
        RAISE EXCEPTION 'M-06' USING ERRCODE = 'P0001';
    END IF;
    RETURN NULL;
END
$fn$;
