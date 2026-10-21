-- BudgetP1ReferenceStateAndAudit step 10: T-08 fa_financial_years (DDD T-08, design §5.2).
-- start_date / end_date timestamptz → date, and the DDD CHECKs. Guarded (design §11.1 rule 4): the
-- migration aborts, changing nothing, where a value cannot be converted without loss or would fail a CHECK.
-- The conversion reads the stored instant in UTC, the zone the service writes in (Npgsql timestamptz from
-- DateTime UTC). A value whose time of day in UTC is not 00:00 is refused rather than truncated.
DO $$
DECLARE
    v_list text;
BEGIN
    SELECT string_agg(format('%s (start %s, end %s)', fin_year_closing, start_date, end_date), '; '
                      ORDER BY fin_year_closing)
    INTO v_list
    FROM budget.fa_financial_years
    WHERE (start_date AT TIME ZONE 'UTC')::time <> '00:00'
       OR (end_date AT TIME ZONE 'UTC')::time <> '00:00';
    IF v_list IS NOT NULL THEN
        RAISE EXCEPTION 'BudgetP1ReferenceStateAndAudit aborted: financial years with a time part, not convertible to date: %', v_list;
    END IF;

    SELECT string_agg(format('%s (start %s, end %s)', fin_year_closing,
                             (start_date AT TIME ZONE 'UTC')::date, (end_date AT TIME ZONE 'UTC')::date), '; '
                      ORDER BY fin_year_closing)
    INTO v_list
    FROM budget.fa_financial_years
    WHERE fin_year_closing NOT BETWEEN 2000 AND 2099
       OR (start_date AT TIME ZONE 'UTC')::date <> make_date(fin_year_closing - 1, 4, 1)
       OR (end_date AT TIME ZONE 'UTC')::date <> make_date(fin_year_closing, 3, 31);
    IF v_list IS NOT NULL THEN
        RAISE EXCEPTION 'BudgetP1ReferenceStateAndAudit aborted: financial years not conforming to DDD T-08 (1 April to 31 March, closing year 2000-2099): %', v_list;
    END IF;
END
$$;

ALTER TABLE budget.fa_financial_years
    ALTER COLUMN start_date TYPE date USING (start_date AT TIME ZONE 'UTC')::date,
    ALTER COLUMN end_date   TYPE date USING (end_date AT TIME ZONE 'UTC')::date;

ALTER TABLE budget.fa_financial_years
    -- CHECK between 2000 and 2099.
    ADD CONSTRAINT ck_fa_financial_years_closing CHECK (fin_year_closing BETWEEN 2000 AND 2099),
    -- CHECK equal to 1 April of the preceding year.
    ADD CONSTRAINT ck_fa_financial_years_start CHECK (start_date = make_date(fin_year_closing - 1, 4, 1)),
    -- CHECK equal to 31 March of fin_year_closing.
    ADD CONSTRAINT ck_fa_financial_years_end CHECK (end_date = make_date(fin_year_closing, 3, 31));
