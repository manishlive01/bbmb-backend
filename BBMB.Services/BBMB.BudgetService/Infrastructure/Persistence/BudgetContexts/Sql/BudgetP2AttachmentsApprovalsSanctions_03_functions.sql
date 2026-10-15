-- BudgetP2AttachmentsApprovalsSanctions step 3: the database rules of approvals and sanctions (DDD §6.1
-- DB-01, DB-03 reference side on T-26, DB-04, DB-05, DB-06, DB-07, DB-08, DB-09) and fn_record_estimate_code
-- (DDD §6.3). Where the rule has an SRS message number the exception text is that number (M-05, M-06), so
-- the global handler shows the catalogue text (design §4.9); the others are backstops behind application
-- pre-checks and raise the rule number.
-- ---------------------------------------------------------------------------------------------------
-- Helper for DB-08: the attachment is of evidence class. Raises DB-08 otherwise.
-- ---------------------------------------------------------------------------------------------------
CREATE FUNCTION budget.fn_require_evidence(p_attachment_id bigint, p_column text) RETURNS void
    LANGUAGE plpgsql
    SET search_path = budget, pg_temp
AS $fn$
BEGIN
    IF p_attachment_id IS NOT NULL AND NOT EXISTS (
        SELECT 1 FROM budget.attachment a WHERE a.attachment_id = p_attachment_id AND a.evidence_class = 'EVIDENCE'
    ) THEN
        RAISE EXCEPTION 'DB-08' USING ERRCODE = 'P0001',
            DETAIL = format('%s must refer to an attachment of evidence class', p_column);
    END IF;
END
$fn$;
-- ---------------------------------------------------------------------------------------------------
-- T-25 BEFORE INSERT OR UPDATE: DB-01 (an approval is at a Division office) and DB-04 (the office is the
-- Division of the work, AAP-003). work, office and serial are fixed once written. The serial is unique in
-- the Division by the UK; current_version_no is checked against T-26 at commit (DB-06).
-- ---------------------------------------------------------------------------------------------------
CREATE FUNCTION budget.fn_admin_approval_rules() RETURNS trigger
    LANGUAGE plpgsql
    SET search_path = budget, pg_temp
AS $fn$
BEGIN
    IF TG_OP = 'UPDATE' THEN
        IF NEW.work_id <> OLD.work_id OR NEW.division_office_id <> OLD.division_office_id OR NEW.serial_no <> OLD.serial_no THEN
            RAISE EXCEPTION 'DB-04' USING ERRCODE = 'P0001', DETAIL = 'work, Division and serial of an approval are fixed';
        END IF;
        IF NEW.current_version_no < OLD.current_version_no THEN
            RAISE EXCEPTION 'DB-06' USING ERRCODE = 'P0001', DETAIL = 'the current version of an approval never goes back';
        END IF;
        RETURN NEW;
    END IF;
    IF NOT EXISTS (
        SELECT 1 FROM budget.office o JOIN budget.work w ON w.division_id = o.division_id
        WHERE o.office_id = NEW.division_office_id AND o.office_level = 'DIVISION' AND w.id = NEW.work_id
    ) THEN
        RAISE EXCEPTION 'DB-04' USING ERRCODE = 'P0001',
            DETAIL = 'an approval is held by the Division office of its work (DB-01, DB-04)';
    END IF;
    RETURN NEW;
END
$fn$;
-- ---------------------------------------------------------------------------------------------------
-- T-26 BEFORE INSERT: DB-06 (the next version number, from 1 without gap), DB-07 (the A&A reference is not
-- used by another approval of the same Division; a revision of the same approval may repeat its own),
-- DB-08 (the copy is of evidence class), DB-03 (a code marked not for use is refused as a new reference:
-- version 1, or a revision that changes the code; a revision keeping its code is not a new reference).
-- The Division is serialised by a transaction-level advisory lock so two concurrent approvals cannot both
-- pass the DB-07 test.
-- ---------------------------------------------------------------------------------------------------
CREATE FUNCTION budget.fn_admin_approval_version_rules() RETURNS trigger
    LANGUAGE plpgsql
    SET search_path = budget, pg_temp
AS $fn$
DECLARE
    v_division bigint;
    v_max      smallint;
    v_prev     bigint;
BEGIN
    SELECT a.division_office_id INTO v_division FROM budget.admin_approval a WHERE a.admin_approval_id = NEW.admin_approval_id;
    PERFORM pg_advisory_xact_lock(hashtext('budget.aa_reference'), v_division::int);
    SELECT max(v.version_no) INTO v_max FROM budget.admin_approval_version v WHERE v.admin_approval_id = NEW.admin_approval_id;
    IF NEW.version_no <> coalesce(v_max, 0) + 1 THEN
        RAISE EXCEPTION 'DB-06' USING ERRCODE = 'P0001',
            DETAIL = format('approval %s: version %s written where %s is next', NEW.admin_approval_id, NEW.version_no, coalesce(v_max, 0) + 1);
    END IF;
    IF EXISTS (
        SELECT 1 FROM budget.admin_approval_version v
        JOIN budget.admin_approval a ON a.admin_approval_id = v.admin_approval_id
        WHERE a.division_office_id = v_division
          AND v.admin_approval_id <> NEW.admin_approval_id
          AND v.aa_reference_no = NEW.aa_reference_no
    ) THEN
        RAISE EXCEPTION 'DB-07' USING ERRCODE = 'P0001',
            DETAIL = 'the A&A reference is used by another approval of the Division';
    END IF;
    PERFORM budget.fn_require_evidence(NEW.aa_copy_attachment_id, 'aa_copy_attachment_id');
    IF v_max IS NOT NULL THEN
        SELECT v.account_code_id INTO v_prev FROM budget.admin_approval_version v
        WHERE v.admin_approval_id = NEW.admin_approval_id AND v.version_no = v_max;
    END IF;
    IF NEW.account_code_id IS DISTINCT FROM v_prev AND EXISTS (
        SELECT 1 FROM budget.budget_head_master h WHERE h.account_code_id = NEW.account_code_id AND h.not_for_use
    ) THEN
        RAISE EXCEPTION 'M-05' USING ERRCODE = 'P0001';
    END IF;
    RETURN NEW;
END
$fn$;
-- ---------------------------------------------------------------------------------------------------
-- T-25 / T-26 at commit: DB-06. The header's current version equals the highest version in T-26, and the
-- header has its version 1. Deferred, so the header and its version are written in either order.
-- ---------------------------------------------------------------------------------------------------
CREATE FUNCTION budget.fn_admin_approval_version_check() RETURNS trigger
    LANGUAGE plpgsql
    SET search_path = budget, pg_temp
AS $fn$
DECLARE
    v_id      bigint := NEW.admin_approval_id;
    v_current smallint;
    v_max     smallint;
BEGIN
    SELECT a.current_version_no INTO v_current FROM budget.admin_approval a WHERE a.admin_approval_id = v_id;
    SELECT max(v.version_no) INTO v_max FROM budget.admin_approval_version v WHERE v.admin_approval_id = v_id;
    IF v_max IS NULL OR v_current IS DISTINCT FROM v_max THEN
        RAISE EXCEPTION 'DB-06' USING ERRCODE = 'P0001',
            DETAIL = format('approval %s: current version %s, highest version %s', v_id, v_current, v_max);
    END IF;
    RETURN NULL;
END
$fn$;
-- ---------------------------------------------------------------------------------------------------
-- T-27 BEFORE INSERT OR UPDATE: DB-04 (the tab is the work's class, BUD-068; an AA-tab sanction is of the
-- work's own approval), the serial unique within the work's Division (DDD T-27 UK (work's Division,
-- serial), which spans T-23 and so is a trigger, serialised by an advisory lock), the fixed columns, and
-- DB-05: estimate_code is null on insert and is written once, only inside budget.fn_record_estimate_code.
-- ---------------------------------------------------------------------------------------------------
CREATE FUNCTION budget.fn_technical_sanction_rules() RETURNS trigger
    LANGUAGE plpgsql
    SET search_path = budget, pg_temp
AS $fn$
DECLARE
    v_division int;
    v_class    text;
    v_writer   boolean := coalesce(current_setting('budget.estimate_code_writer', true), '') = 'fn_record_estimate_code';
BEGIN
    IF TG_OP = 'UPDATE' THEN
        IF NEW.work_id <> OLD.work_id OR NEW.tab <> OLD.tab OR NEW.serial_no <> OLD.serial_no
           OR NEW.admin_approval_id IS DISTINCT FROM OLD.admin_approval_id THEN
            RAISE EXCEPTION 'DB-04' USING ERRCODE = 'P0001', DETAIL = 'work, tab, approval and serial of a sanction are fixed';
        END IF;
        IF NEW.current_version_no < OLD.current_version_no THEN
            RAISE EXCEPTION 'DB-06' USING ERRCODE = 'P0001', DETAIL = 'the current version of a sanction never goes back';
        END IF;
        IF NEW.estimate_code IS DISTINCT FROM OLD.estimate_code OR NEW.estimate_code_at IS DISTINCT FROM OLD.estimate_code_at THEN
            IF OLD.estimate_code IS NOT NULL OR NOT v_writer THEN
                RAISE EXCEPTION 'DB-05' USING ERRCODE = 'P0001',
                    DETAIL = 'an estimate code is written once, by budget.fn_record_estimate_code';
            END IF;
        END IF;
        RETURN NEW;
    END IF;
    IF NEW.estimate_code IS NOT NULL OR NEW.estimate_code_at IS NOT NULL THEN
        RAISE EXCEPTION 'DB-05' USING ERRCODE = 'P0001', DETAIL = 'a sanction is written without an estimate code';
    END IF;
    SELECT w.division_id, w.work_class INTO v_division, v_class FROM budget.work w WHERE w.id = NEW.work_id;
    IF v_class IS DISTINCT FROM NEW.tab THEN
        RAISE EXCEPTION 'DB-04' USING ERRCODE = 'P0001', DETAIL = 'the tab of a sanction is the class of its work';
    END IF;
    IF NEW.tab = 'AA' AND NOT EXISTS (
        SELECT 1 FROM budget.admin_approval a WHERE a.admin_approval_id = NEW.admin_approval_id AND a.work_id = NEW.work_id
    ) THEN
        RAISE EXCEPTION 'DB-04' USING ERRCODE = 'P0001', DETAIL = 'an AA-tab sanction is of the approval of its work';
    END IF;
    PERFORM pg_advisory_xact_lock(hashtext('budget.ts_serial'), v_division);
    IF EXISTS (
        SELECT 1 FROM budget.technical_sanction t JOIN budget.work w ON w.id = t.work_id
        WHERE w.division_id = v_division AND t.serial_no = NEW.serial_no
    ) THEN
        RAISE EXCEPTION 'DB-04' USING ERRCODE = 'P0001', DETAIL = 'the serial is used by another sanction of the Division';
    END IF;
    RETURN NEW;
END
$fn$;
-- ---------------------------------------------------------------------------------------------------
-- T-28 BEFORE INSERT: DB-06 (next version number), DB-07 (the TS number is not used by another sanction of
-- the same Division; a revision of the same sanction may repeat its own), DB-08 (the copy is of evidence
-- class).
-- ---------------------------------------------------------------------------------------------------
CREATE FUNCTION budget.fn_technical_sanction_version_rules() RETURNS trigger
    LANGUAGE plpgsql
    SET search_path = budget, pg_temp
AS $fn$
DECLARE
    v_division int;
    v_max      smallint;
BEGIN
    SELECT w.division_id INTO v_division
    FROM budget.technical_sanction t JOIN budget.work w ON w.id = t.work_id
    WHERE t.technical_sanction_id = NEW.technical_sanction_id;
    PERFORM pg_advisory_xact_lock(hashtext('budget.ts_number'), v_division);
    SELECT max(v.version_no) INTO v_max FROM budget.technical_sanction_version v
    WHERE v.technical_sanction_id = NEW.technical_sanction_id;
    IF NEW.version_no <> coalesce(v_max, 0) + 1 THEN
        RAISE EXCEPTION 'DB-06' USING ERRCODE = 'P0001',
            DETAIL = format('sanction %s: version %s written where %s is next', NEW.technical_sanction_id, NEW.version_no, coalesce(v_max, 0) + 1);
    END IF;
    IF EXISTS (
        SELECT 1 FROM budget.technical_sanction_version v
        JOIN budget.technical_sanction t ON t.technical_sanction_id = v.technical_sanction_id
        JOIN budget.work w ON w.id = t.work_id
        WHERE w.division_id = v_division
          AND v.technical_sanction_id <> NEW.technical_sanction_id
          AND v.ts_number = NEW.ts_number
    ) THEN
        RAISE EXCEPTION 'DB-07' USING ERRCODE = 'P0001',
            DETAIL = 'the technical sanction number is used by another sanction of the Division';
    END IF;
    PERFORM budget.fn_require_evidence(NEW.ts_copy_attachment_id, 'ts_copy_attachment_id');
    RETURN NEW;
END
$fn$;
-- ---------------------------------------------------------------------------------------------------
-- T-27 / T-28 at commit: DB-06 for sanctions, as for approvals.
-- ---------------------------------------------------------------------------------------------------
CREATE FUNCTION budget.fn_technical_sanction_version_check() RETURNS trigger
    LANGUAGE plpgsql
    SET search_path = budget, pg_temp
AS $fn$
DECLARE
    v_id      bigint := NEW.technical_sanction_id;
    v_current smallint;
    v_max     smallint;
BEGIN
    SELECT t.current_version_no INTO v_current FROM budget.technical_sanction t WHERE t.technical_sanction_id = v_id;
    SELECT max(v.version_no) INTO v_max FROM budget.technical_sanction_version v WHERE v.technical_sanction_id = v_id;
    IF v_max IS NULL OR v_current IS DISTINCT FROM v_max THEN
        RAISE EXCEPTION 'DB-06' USING ERRCODE = 'P0001',
            DETAIL = format('sanction %s: current version %s, highest version %s', v_id, v_current, v_max);
    END IF;
    RETURN NULL;
END
$fn$;
-- ---------------------------------------------------------------------------------------------------
-- DB-09 at commit (VR-02, BUD-072 to BUD-074, M-06): on the AA tab, the current versions of all sanctions
-- of an approval total no more than TS_CEILING_PCT (T-17, in force now) of the approval's current amount.
-- No override exists. Fired by a sanction write (T-28 insert, T-27 current-version change); the limit is
-- recomputed on the approval's current version, so an upward revision re-admits a refused sanction
-- (BUD-074). A downward revision of the approval is not refused here: BUD-065 accepts it and raises a
-- correction flag instead (design §8.4 revisions row).
-- Compared as sum x 100 <= pct x amount, so exactly 105 per cent is accepted without rounding.
-- ---------------------------------------------------------------------------------------------------
CREATE FUNCTION budget.fn_ts_ceiling_check() RETURNS trigger
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
-- ---------------------------------------------------------------------------------------------------
-- T-45 BEFORE INSERT OR UPDATE: DB-05. A flag is written uncleared; the clearance is set once, from the
-- actor context, and nothing else of a flag changes.
-- ---------------------------------------------------------------------------------------------------
CREATE FUNCTION budget.fn_correction_flag_rules() RETURNS trigger
    LANGUAGE plpgsql
    SET search_path = budget, pg_temp
AS $fn$
DECLARE
    c_open constant text[] := ARRAY['cleared_at', 'cleared_by'];
BEGIN
    IF TG_OP = 'INSERT' THEN
        IF NEW.cleared_at IS NOT NULL OR NEW.cleared_by IS NOT NULL THEN
            RAISE EXCEPTION 'DB-05' USING ERRCODE = 'P0001', DETAIL = 'a correction flag is written uncleared';
        END IF;
        RETURN NEW;
    END IF;
    IF (to_jsonb(NEW) - c_open) IS DISTINCT FROM (to_jsonb(OLD) - c_open) THEN
        RAISE EXCEPTION 'DB-05' USING ERRCODE = 'P0001', DETAIL = 'a correction flag is fixed save for its clearance';
    END IF;
    IF NEW.cleared_at IS DISTINCT FROM OLD.cleared_at OR NEW.cleared_by IS DISTINCT FROM OLD.cleared_by THEN
        IF OLD.cleared_at IS NOT NULL OR NEW.cleared_at IS NULL THEN
            RAISE EXCEPTION 'DB-05' USING ERRCODE = 'P0001', DETAIL = 'the clearance of a flag is set once';
        END IF;
        NEW.cleared_at := clock_timestamp();
        NEW.cleared_by := nullif(current_setting('budget.actor_employee_id', true), '')::uuid;
    END IF;
    RETURN NEW;
END
$fn$;
-- ---------------------------------------------------------------------------------------------------
-- T-30 BEFORE INSERT OR UPDATE: DB-08 for the two Chief Engineer files (VR-03 certificate, BUD-091; VR-05
-- approval, BUD-278). The justification attachment is of supporting purpose and is not restricted by class.
-- ---------------------------------------------------------------------------------------------------
CREATE FUNCTION budget.fn_demand_line_attachment_rules() RETURNS trigger
    LANGUAGE plpgsql
    SET search_path = budget, pg_temp
AS $fn$
BEGIN
    IF TG_OP = 'INSERT' OR NEW.ce_certificate_attachment_id IS DISTINCT FROM OLD.ce_certificate_attachment_id THEN
        PERFORM budget.fn_require_evidence(NEW.ce_certificate_attachment_id, 'ce_certificate_attachment_id');
    END IF;
    IF TG_OP = 'INSERT' OR NEW.ce_amr_approval_attachment_id IS DISTINCT FROM OLD.ce_amr_approval_attachment_id THEN
        PERFORM budget.fn_require_evidence(NEW.ce_amr_approval_attachment_id, 'ce_amr_approval_attachment_id');
    END IF;
    RETURN NEW;
END
$fn$;
-- ---------------------------------------------------------------------------------------------------
-- budget.fn_record_estimate_code (DDD §6.3, §8.3; DB-05; TSN-009, BUD-271): the only writer of an estimate
-- code. Executable by bud_fa. Refuses where a code is already recorded. SECURITY DEFINER so that bud_fa
-- needs no privilege on T-27; the T-27 triggers still run, so the call must be made inside an actor
-- context (DB-28) like every other write.
-- ---------------------------------------------------------------------------------------------------
CREATE FUNCTION budget.fn_record_estimate_code(p_technical_sanction_id bigint, p_estimate_code varchar)
    RETURNS void
    LANGUAGE plpgsql
    SECURITY DEFINER
    SET search_path = budget, pg_temp
AS $fn$
DECLARE
    v_existing text;
BEGIN
    IF btrim(coalesce(p_estimate_code, '')) = '' OR length(p_estimate_code) > 30 THEN
        RAISE EXCEPTION 'DB-05' USING ERRCODE = 'P0001', DETAIL = 'an estimate code of 1 to 30 characters is required';
    END IF;
    SELECT t.estimate_code INTO v_existing FROM budget.technical_sanction t
    WHERE t.technical_sanction_id = p_technical_sanction_id
    FOR UPDATE;
    IF NOT FOUND THEN
        RAISE EXCEPTION 'DB-05' USING ERRCODE = 'P0001', DETAIL = format('no technical sanction %s', p_technical_sanction_id);
    END IF;
    IF v_existing IS NOT NULL THEN
        RAISE EXCEPTION 'DB-05' USING ERRCODE = 'P0001', DETAIL = 'an estimate code is already recorded on this sanction';
    END IF;
    PERFORM set_config('budget.estimate_code_writer', 'fn_record_estimate_code', true);
    UPDATE budget.technical_sanction
       SET estimate_code = p_estimate_code, estimate_code_at = clock_timestamp()
     WHERE technical_sanction_id = p_technical_sanction_id;
    PERFORM set_config('budget.estimate_code_writer', '', true);
END
$fn$;
