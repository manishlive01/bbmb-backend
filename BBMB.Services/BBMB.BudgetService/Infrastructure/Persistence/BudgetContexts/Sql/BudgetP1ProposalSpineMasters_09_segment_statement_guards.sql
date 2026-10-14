-- BudgetP1ProposalSpineMasters step 9 (design §11.2): statement-level DELETE/TRUNCATE guard on the six
-- segment masters T-01 to T-06 (DDD §3.3: no DELETE on any table; DB-13). Their row-level UPDATE guard
-- lands in P2 with F-01.
CREATE TRIGGER trg_fa_major_head_forbid_stmt BEFORE DELETE OR TRUNCATE ON budget.fa_major_head
    FOR EACH STATEMENT EXECUTE FUNCTION budget.fn_forbid_change();
CREATE TRIGGER trg_fa_sub_major_head_forbid_stmt BEFORE DELETE OR TRUNCATE ON budget.fa_sub_major_head
    FOR EACH STATEMENT EXECUTE FUNCTION budget.fn_forbid_change();
CREATE TRIGGER trg_fa_minor_head_forbid_stmt BEFORE DELETE OR TRUNCATE ON budget.fa_minor_head
    FOR EACH STATEMENT EXECUTE FUNCTION budget.fn_forbid_change();
CREATE TRIGGER trg_fa_sub_minor_head_forbid_stmt BEFORE DELETE OR TRUNCATE ON budget.fa_sub_minor_head
    FOR EACH STATEMENT EXECUTE FUNCTION budget.fn_forbid_change();
CREATE TRIGGER trg_fa_detail_head_forbid_stmt BEFORE DELETE OR TRUNCATE ON budget.fa_detail_head
    FOR EACH STATEMENT EXECUTE FUNCTION budget.fn_forbid_change();
CREATE TRIGGER trg_fa_object_head_forbid_stmt BEFORE DELETE OR TRUNCATE ON budget.fa_object_head
    FOR EACH STATEMENT EXECUTE FUNCTION budget.fn_forbid_change();
