-- BudgetP4WorkCodeConformance step 5 (design §4.6 step 5, DDD T-24). The work_code_serial table has been
-- created by the generated CreateTable op (PK (division_code, fin_year)); this file adds the DDD CHECKs, the
-- fin_year FK to the financial-year master, and budget.fn_generate_work_code, the only writer of a work code.

-- T-24 CHECKs (DDD T-24): division_code 1..999 (as T-12); last_serial 1..999 — a serial beyond 999 fails the
-- CHECK and no code is generated (BUD-314).
ALTER TABLE budget.work_code_serial
    ADD CONSTRAINT ck_work_code_serial_division CHECK (division_code BETWEEN 1 AND 999),
    ADD CONSTRAINT ck_work_code_serial_last     CHECK (last_serial BETWEEN 1 AND 999);

DO $$
BEGIN
    IF to_regclass('budget.fa_financial_years') IS NOT NULL THEN
        ALTER TABLE budget.work_code_serial
            ADD CONSTRAINT fk_work_code_serial_fin_year FOREIGN KEY (fin_year)
                REFERENCES budget.fa_financial_years (fin_year_closing) ON DELETE RESTRICT;
    END IF;
END
$$;

-- budget.fn_generate_work_code(p_work_id, p_occasion) (DDD T-24, BUD-231/232/313/314/315). Assigns and
-- returns a conformant DDD-YYZZ-SSS code for a work that has none, on one of the four occasions. It:
--   * reads the work's Division (BUD-315) and resolves the Division code (1..999) from master.division;
--   * resolves the allotment year's closing from the occasion's cycle year (passed as the second element of
--     the composite argument is avoided: the caller sets the work's cycle via the provision; here the year
--     is taken from the current budget cycle's financial year — see the ALLOC/REAPPROP/UNFORESEEN callers in
--     P4-08/P4-12, which pass p_fin_year_closing);
--   * takes the next serial per (division_code, fin_year) with INSERT … ON CONFLICT DO UPDATE … RETURNING,
--     which holds the T-24 row lock until commit, so two concurrent allocations for one Division and year
--     queue on the row and cannot compute the same serial (DV-03). A rolled-back allocation releases its
--     serial with the transaction, leaving no gap.
--   * composes the code as division(3) - YYZZ - serial(3) and writes work_code, origin GENERATED, the
--     occasion, the year and the timestamp onto the work in one UPDATE. DB-05 permits this because the
--     work's code was null; a second call for a work that already has a code is refused (the code is
--     unchanged — the function returns the existing code without a write).
CREATE OR REPLACE FUNCTION budget.fn_generate_work_code(
    p_work_id int,
    p_occasion varchar,
    p_fin_year_closing int
) RETURNS varchar AS $$
DECLARE
    v_division_id   int;
    v_division_code int;
    v_existing      varchar(12);
    v_serial        smallint;
    v_yyzz          text;
    v_code          varchar(12);
BEGIN
    IF p_occasion NOT IN ('ALLOC_RBE', 'ALLOC_OBE', 'REAPPROPRIATION', 'UNFORESEEN_AUTH') THEN
        RAISE EXCEPTION 'fn_generate_work_code: occasion % is not one of the four generation occasions (BUD-232)', p_occasion;
    END IF;

    SELECT work_code, division_id INTO v_existing, v_division_id FROM budget.work WHERE id = p_work_id;
    IF NOT FOUND THEN
        RAISE EXCEPTION 'fn_generate_work_code: work % does not exist', p_work_id;
    END IF;
    -- Idempotent for a coded work: a second allocation of the same work generates no second code (BUD-233).
    IF v_existing IS NOT NULL THEN
        RETURN v_existing;
    END IF;

    -- Division master is in schema budget (design §4.1 deviation (b), not DDD schema master).
    SELECT division_code INTO v_division_code FROM budget.division WHERE id = v_division_id;
    IF v_division_code IS NULL OR v_division_code < 1 OR v_division_code > 999 THEN
        RAISE EXCEPTION 'fn_generate_work_code: Division % has no valid division_code (1..999) for work %', v_division_id, p_work_id;
    END IF;

    -- Next serial for this Division and year, serialised by the row lock.
    INSERT INTO budget.work_code_serial (division_code, fin_year, last_serial)
    VALUES (v_division_code, p_fin_year_closing, 1)
    ON CONFLICT (division_code, fin_year)
        DO UPDATE SET last_serial = budget.work_code_serial.last_serial + 1
    RETURNING last_serial INTO v_serial;

    -- YYZZ of the allotment year (closing year 2027 -> 2627): preceding two digits then closing two digits.
    v_yyzz := lpad(((p_fin_year_closing - 1) % 100)::text, 2, '0') || lpad((p_fin_year_closing % 100)::text, 2, '0');
    v_code := lpad(v_division_code::text, 3, '0') || '-' || v_yyzz || '-' || lpad(v_serial::text, 3, '0');

    UPDATE budget.work
    SET work_code = v_code,
        work_code_origin = 'GENERATED',
        work_code_occasion = p_occasion,
        work_code_fin_year = p_fin_year_closing,
        work_code_at = now()
    WHERE id = p_work_id;

    RETURN v_code;
END;
$$ LANGUAGE plpgsql;
