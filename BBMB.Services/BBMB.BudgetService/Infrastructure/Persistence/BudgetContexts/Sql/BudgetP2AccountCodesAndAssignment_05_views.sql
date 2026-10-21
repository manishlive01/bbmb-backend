-- BudgetP2AccountCodesAndAssignment step 5: views (DDD §8).
-- V-01: every head of account as at the moment of the query, with the six codes, the six nomenclatures from the
-- segment masters, the complete code, the ACC-009 flag, origin, creation particulars and the not-for-use mark
-- (F-02, BUD-048 to BUD-050). Not a stored document: the Manual is T-07 as it stands when read.
CREATE VIEW budget.v_manual_of_accounts_codes AS
SELECT h.account_code_id,
       h.major_acc_code,     ma.major_acc_desc,
       h.sub_major_acc_code, sma.sub_major_acc_desc,
       h.minor_acc_code,     mi.minor_acc_desc,
       h.sub_minor_acc_code, smi.sub_minor_acc_desc,
       h.detail_acc_code,    d.detail_acc_desc,
       h.object_acc_code,    o.object_acc_desc,
       h.complete_code,
       h.head_desc,
       h.party_emp_required,
       h.origin,
       h.eoffice_approval_ref,
       h.created_at,
       h.created_by,
       e.name_english AS created_by_name,
       h.not_for_use,
       h.not_for_use_ref,
       h.not_for_use_at
FROM budget.budget_head_master h
LEFT JOIN budget.fa_major_head     ma  ON ma.major_acc_code = h.major_acc_code
LEFT JOIN budget.fa_sub_major_head sma ON sma.major_acc_code = h.major_acc_code AND sma.sub_major_acc_code = h.sub_major_acc_code
LEFT JOIN budget.fa_minor_head     mi  ON mi.minor_acc_code = h.minor_acc_code
LEFT JOIN budget.fa_sub_minor_head smi ON smi.minor_acc_code = h.minor_acc_code AND smi.sub_minor_acc_code = h.sub_minor_acc_code
LEFT JOIN budget.fa_detail_head    d   ON d.detail_acc_code = h.detail_acc_code
LEFT JOIN budget.fa_object_head    o   ON o.object_acc_code = h.object_acc_code
LEFT JOIN employee.employee        e   ON e.id = h.created_by;

-- V-07: the Manual of Accounts Codes with the ACC-009 flag and the not-for-use mark, for Financial Accounting
-- (ACC-005, INT-002). bud_fa reads it; it holds no privilege on T-07 itself (DDD §8.3).
CREATE VIEW budget.v_fa_account_code AS
SELECT h.account_code_id,
       h.major_acc_code, h.sub_major_acc_code, h.minor_acc_code, h.sub_minor_acc_code, h.detail_acc_code, h.object_acc_code,
       h.complete_code,
       h.head_desc,
       h.party_emp_required,
       h.not_for_use,
       h.not_for_use_at
FROM budget.budget_head_master h;
