-- BudgetP1RupeesAndMoneyColumns Down(), design §11.1 rule 6: development databases only.
-- Narrows back to numeric(12,2) only if every value fits (|v| < 10^10), else raises.
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
        EXECUTE format('SELECT count(*) FROM budget.%I WHERE abs(%I) >= 10000000000', r.tbl, r.col)
            INTO v_count;
        IF v_count > 0 THEN
            v_found := v_found || format(' %s.%s: %s row(s);', r.tbl, r.col, v_count);
        END IF;
    END LOOP;

    IF v_found <> '' THEN
        RAISE EXCEPTION 'BudgetP1RupeesAndMoneyColumns Down aborted: values do not fit numeric(12,2):%', v_found;
    END IF;
END
$$;
