-- BudgetP1ProposalSpineMasters step 8 (design §5.2 work order, step 5): trg_work_pre_ddd_one_way.
-- The marker only ever clears: an INSERT with pre_ddd = true, and an UPDATE false -> true, are refused.
CREATE FUNCTION budget.fn_work_pre_ddd_one_way() RETURNS trigger
    LANGUAGE plpgsql
    SET search_path = budget, pg_temp
AS $fn$
BEGIN
    IF (TG_OP = 'INSERT' AND NEW.pre_ddd)
       OR (TG_OP = 'UPDATE' AND NEW.pre_ddd AND NOT OLD.pre_ddd) THEN
        RAISE EXCEPTION 'PRE_DDD_ONE_WAY' USING ERRCODE = 'P0001',
            DETAIL = 'work.pre_ddd only clears (true -> false); design §5.2, §5.5 row a';
    END IF;
    RETURN NEW;
END
$fn$;

CREATE TRIGGER trg_work_pre_ddd_one_way BEFORE INSERT OR UPDATE OF pre_ddd ON budget.work
    FOR EACH ROW EXECUTE FUNCTION budget.fn_work_pre_ddd_one_way();
