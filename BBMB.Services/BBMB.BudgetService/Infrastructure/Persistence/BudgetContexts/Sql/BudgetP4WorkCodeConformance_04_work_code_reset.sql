-- BudgetP4WorkCodeConformance step 4 (design §4.6 step 4). The work_code column has already been changed to
-- varchar(12) NULL by the generated AlterColumn (int -> text keeps every value; nothing lost), and the four
-- new columns (work_code_origin/occasion/fin_year/at) have been added by the generated AddColumn ops. This
-- file runs under the migration actor (MigrationActor.sql executed immediately before it): it nulls the live
-- code (audited: one T-51 row per work, column_name = 'work_code', old = the code, new = null), then adds the
-- T-23 CHECKs, the partial unique index, and the DB-05 write-once trigger — all AFTER the reset so DB-05
-- does not refuse the migration's own write.

-- Reset the live code. The codes this build issued (D-4) are not conformant work codes and are not legacy
-- codes either; each is preserved in work_code_prev (step 3) and the live code starts empty.
UPDATE budget.work SET work_code = NULL;

-- T-23 CHECKs (copied from DDD T-23). origin null exactly when work_code null; GENERATED matches the
-- DDD-YYZZ-SSS pattern, LEGACY the seven digits of the earlier system; occasion in the five values; the
-- year of allotment present for a generated code and absent for a legacy code.
ALTER TABLE budget.work
    ADD CONSTRAINT ck_work_code_origin_pair
        CHECK ((work_code IS NULL) = (work_code_origin IS NULL)),
    ADD CONSTRAINT ck_work_code_origin_value
        CHECK (work_code_origin IS NULL OR work_code_origin IN ('GENERATED', 'LEGACY')),
    ADD CONSTRAINT ck_work_code_format
        CHECK (
            work_code IS NULL
            OR (work_code_origin = 'GENERATED' AND work_code ~ '^\d{3}-\d{4}-\d{3}$')
            OR (work_code_origin = 'LEGACY'    AND work_code ~ '^\d{7}$')
        ),
    ADD CONSTRAINT ck_work_code_occasion
        CHECK (work_code_occasion IS NULL
               OR work_code_occasion IN ('ALLOC_RBE', 'ALLOC_OBE', 'REAPPROPRIATION', 'UNFORESEEN_AUTH', 'LEGACY_OPENING')),
    ADD CONSTRAINT ck_work_code_occasion_pair
        CHECK ((work_code IS NULL) = (work_code_occasion IS NULL)),
    ADD CONSTRAINT ck_work_code_at_pair
        CHECK ((work_code IS NULL) = (work_code_at IS NULL)),
    -- A generated code carries its year of allotment (BUD-313); a legacy code does not.
    ADD CONSTRAINT ck_work_code_fin_year
        CHECK (
            (work_code_origin = 'GENERATED' AND work_code_fin_year IS NOT NULL)
            OR (work_code_origin IS DISTINCT FROM 'GENERATED' AND work_code_fin_year IS NULL)
        );

-- FK of work_code_fin_year to the financial-year master (held in schema budget, §5.5 row f) when the
-- referenced table is present. Added guarded so the migration applies in a database that does not yet carry
-- the master under this context.
DO $$
BEGIN
    IF to_regclass('budget.fa_financial_years') IS NOT NULL THEN
        ALTER TABLE budget.work
            ADD CONSTRAINT fk_work_code_fin_year FOREIGN KEY (work_code_fin_year)
                REFERENCES budget.fa_financial_years (fin_year_closing) ON DELETE RESTRICT;
    END IF;
END
$$;

-- UK on work_code (DDD T-23 "UK"): unique where present. A partial unique index allows many null codes
-- (every work before its code is generated) while forbidding two works from sharing one code.
CREATE UNIQUE INDEX ux_work_work_code ON budget.work (work_code) WHERE work_code IS NOT NULL;

-- DB-05 (write-once): once a work_code is set it is never altered (DDD DB-05, BUD-287). Attached AFTER the
-- reset so the migration's own null-write above is not refused. A row whose OLD.work_code is already set may
-- not change work_code, origin, occasion, fin_year or at; a row whose OLD.work_code is null may set them
-- (the generation path and legacy-opening path), but may not be moved back to a different non-null value.
CREATE OR REPLACE FUNCTION budget.fn_work_code_write_once() RETURNS trigger AS $$
BEGIN
    IF OLD.work_code IS NOT NULL AND (
        NEW.work_code         IS DISTINCT FROM OLD.work_code OR
        NEW.work_code_origin  IS DISTINCT FROM OLD.work_code_origin OR
        NEW.work_code_occasion IS DISTINCT FROM OLD.work_code_occasion OR
        NEW.work_code_fin_year IS DISTINCT FROM OLD.work_code_fin_year OR
        NEW.work_code_at      IS DISTINCT FROM OLD.work_code_at
    ) THEN
        RAISE EXCEPTION 'M-06' USING ERRCODE = 'P0001';  -- a work code, once set, is not altered (DB-05)
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_work_code_write_once BEFORE UPDATE ON budget.work
    FOR EACH ROW EXECUTE FUNCTION budget.fn_work_code_write_once();

-- Recreate V-08 budget.v_fa_work_sanction (dropped in step 4a before the type change) with the identical
-- definition; work_code is now varchar(12). Kept verbatim from BudgetP2AttachmentsApprovalsSanctions_05_views.
CREATE VIEW budget.v_fa_work_sanction AS
SELECT w.id                       AS work_id,
       w.work_code,
       w.division_id,
       w.work_class,
       w.work_desc,
       a.admin_approval_id,
       av.version_no              AS aa_version_no,
       av.aa_reference_no,
       av.date_of_approval,
       av.amount_sanctioned       AS aa_amount_sanctioned,
       av.account_code_id         AS aa_account_code_id,
       t.technical_sanction_id,
       t.tab,
       tv.version_no              AS ts_version_no,
       tv.ts_number,
       tv.ts_date,
       tv.ts_amount,
       t.estimate_code,
       t.estimate_code_at,
       (t.technical_sanction_id IS NOT NULL AND t.estimate_code IS NULL) AS awaiting_estimate_code
FROM budget.work w
LEFT JOIN budget.admin_approval a ON a.work_id = w.id
LEFT JOIN budget.admin_approval_version av ON av.admin_approval_id = a.admin_approval_id AND av.version_no = a.current_version_no
LEFT JOIN budget.technical_sanction t ON t.work_id = w.id
LEFT JOIN budget.technical_sanction_version tv ON tv.technical_sanction_id = t.technical_sanction_id AND tv.version_no = t.current_version_no
WHERE a.admin_approval_id IS NOT NULL OR t.technical_sanction_id IS NOT NULL;
