-- Down of BudgetP2TsCeilingLock: restores the BudgetP2AttachmentsApprovalsSanctions body (no lock).
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
