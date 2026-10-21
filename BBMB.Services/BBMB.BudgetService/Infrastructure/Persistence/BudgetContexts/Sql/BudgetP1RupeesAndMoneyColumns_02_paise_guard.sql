-- BudgetP1RupeesAndMoneyColumns, design §4.5 step 2 and §11.1 rule 4: guard before the type change.
-- A value carrying paise is refused, never rounded (DV-02). Rounding would change a recorded figure
-- without an audit entry, so the migration aborts and the owner corrects the rows under change
-- control (OI-09). Nothing is changed when the guard raises.
DO $$
DECLARE
    v_found text := '';
    v_count bigint;
    r record;
BEGIN
    FOR r IN
        SELECT * FROM (VALUES
            ('budget_table', 'budget_amount'),
            ('budget_table', 'revised_amount'),
            ('budget_table', 'proposed_budget'),
            ('estimate',     'estimate_amt'),
            ('work',         'estimated_cost')
        ) AS c(tbl, col)
    LOOP
        EXECUTE format('SELECT count(*) FROM budget.%I WHERE %I <> trunc(%I)', r.tbl, r.col, r.col)
            INTO v_count;
        IF v_count > 0 THEN
            v_found := v_found || format(' %s.%s: %s row(s);', r.tbl, r.col, v_count);
        END IF;
    END LOOP;

    IF v_found <> '' THEN
        RAISE EXCEPTION 'BudgetP1RupeesAndMoneyColumns aborted: amounts carrying paise found:%', v_found
            USING HINT = 'Correct the rows under change control (OI-09), then re-run the migration. No value is rounded.';
    END IF;
END
$$;
