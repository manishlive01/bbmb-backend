-- BudgetP4AllocationProvisionFreeze step 5: V-02, V-09 and V-04 version 4 (design §5.4, DDD §8).

-- ===================================================================================================
-- V-02 budget.v_provision_balance (DDD V-02): each live provision (not superseded) with the sum of its T-42
-- movements as the amount available, and whether it is operative for its (year, holder). The operative
-- provision of a year is the RBE provision where one is live and the OBE provision otherwise. This is the
-- single source of "the budget allotted" and is read by the DB-23 and DB-26 checks and by every balance read.
-- "Live" = superseded_by_provision_id IS NULL (a superseded provision keeps its history but is not the
-- current one). The holder key is (fin_year, holder_type, work_id, office_id, account_code_id).
-- ===================================================================================================
CREATE VIEW budget.v_provision_balance AS
SELECT p.provision_id,
       p.fin_year,
       p.basis,
       p.category,
       p.holder_type,
       p.work_id,
       p.office_id,
       p.account_code_id,
       p.demand_line_id,
       p.effective_from,
       p.budgetary_check,
       coalesce(mv.available, 0) AS available,
       -- Operative: for the holder in the year, the live RBE provision if one exists, else the live OBE one.
       (p.basis = 'RBE'
        OR NOT EXISTS (
            SELECT 1 FROM budget.provision r
            WHERE r.superseded_by_provision_id IS NULL
              AND r.basis = 'RBE'
              AND r.fin_year = p.fin_year
              AND r.holder_type = p.holder_type
              AND r.work_id IS NOT DISTINCT FROM p.work_id
              AND r.office_id = p.office_id
              AND r.account_code_id IS NOT DISTINCT FROM p.account_code_id
        )) AS is_operative
FROM budget.provision p
LEFT JOIN LATERAL (
    SELECT sum(m.amount) AS available
    FROM budget.provision_movement m WHERE m.provision_id = p.provision_id
) mv ON true
WHERE p.superseded_by_provision_id IS NULL;

-- ===================================================================================================
-- V-09 budget.v_fa_provision (DDD V-09, ALC-007, BUD-184, BUD-290, BUD-307): the operative provision read by
-- Financial Accounting, work-wise, estimate-wise and head-wise, with effective_from and budgetary_check. The
-- estimate-wise rows come from T-43 against a work provision; the work-wise and head-wise rows are the
-- operative provision itself. Granted to bud_fa in step 6.
-- ===================================================================================================
CREATE VIEW budget.v_fa_provision AS
-- Work-wise and head-wise: the operative provision and its available balance.
SELECT vb.provision_id,
       vb.holder_type,
       vb.fin_year,
       vb.basis,
       vb.category,
       vb.work_id,
       vb.office_id,
       vb.account_code_id,
       NULL::bigint        AS technical_sanction_id,
       NULL::varchar(30)   AS estimate_code,
       vb.available        AS amount,
       vb.effective_from,
       vb.budgetary_check
FROM budget.v_provision_balance vb
WHERE vb.is_operative
UNION ALL
-- Estimate-wise: each estimate allocation of an operative WORK provision, carrying the sanction's estimate code.
SELECT vb.provision_id,
       'ESTIMATE'          AS holder_type,
       vb.fin_year,
       vb.basis,
       vb.category,
       vb.work_id,
       vb.office_id,
       vb.account_code_id,
       ea.technical_sanction_id,
       t.estimate_code,
       ea.amount,
       vb.effective_from,
       vb.budgetary_check
FROM budget.v_provision_balance vb
JOIN budget.estimate_allocation ea ON ea.provision_id = vb.provision_id
JOIN budget.technical_sanction t ON t.technical_sanction_id = ea.technical_sanction_id
WHERE vb.is_operative AND vb.holder_type = 'WORK';

-- ===================================================================================================
-- V-04 budget.v_demand_line_display, version 4 (design §5.4): version 3 plus the carry-forward from V-02 —
-- the budget allotted to the line in the previous cycle, read from the operative provision of the line's work
-- for the current financial year (BUD-184). CREATE OR REPLACE appends columns only: every version-3 column
-- keeps its name, type and position, so the existing mapping is unaffected. The two new columns are null for
-- a line whose work has no operative provision yet.
-- ===================================================================================================
CREATE OR REPLACE VIEW budget.v_demand_line_display AS
SELECT l.demand_line_id,
       l.proposal_id,
       p.cycle_id,
       p.category,
       p.origin_office_id,
       l.serial_no,
       l.line_type,
       l.work_id,
       w.work_desc,
       w.work_class,
       w.preceding_amr_work_id,
       l.account_code_id,
       h.complete_code,
       h.head_desc,
       l.unforeseen_name,
       l.rbe_amount,
       l.obe_next_amount,
       l.justification_text,
       l.pct_check_not_applied,
       l.floor_check_unchecked,
       l.added_on_return,
       l.is_deleted,
       ob.amount AS opening_obe_current,
       rp.amount AS opening_rbe_previous,
       ex.amount AS expenditure_to_date,
       ey.amount AS expenditure_previous_year,
       coalesce(lf.level_figures, '[]'::jsonb) AS level_figures,
       aa.admin_approval_id,
       aa.name_of_work         AS aa_name_of_work,
       aa.aa_reference_no,
       aa.date_of_approval     AS aa_date_of_approval,
       aa.amount_sanctioned    AS aa_amount_sanctioned,
       aa.current_version_no   AS aa_current_version_no,
       ts.sanction_count       AS ts_count,
       ts.ts_total             AS ts_amount_to_date,
       l.justification_attachment_id,
       l.ce_certificate_attachment_id,
       l.ce_amr_approval_attachment_id,
       col9.rbe_amount         AS recommended_rbe_for_office,
       col9.obe_next_amount    AS recommended_obe_for_office,
       cf.provision_id         AS carry_forward_provision_id,
       cf.available            AS carry_forward_available
FROM budget.demand_line l
JOIN budget.proposal p ON p.proposal_id = l.proposal_id
JOIN budget.budget_cycle c ON c.cycle_id = p.cycle_id
LEFT JOIN budget.work w ON w.id = l.work_id
LEFT JOIN budget.budget_head_master h ON h.account_code_id = l.account_code_id
LEFT JOIN LATERAL (
    SELECT o.amount FROM budget.opening_figure o
    WHERE o.cycle_id = p.cycle_id AND o.figure_kind = 'OBE_CURRENT'
      AND ((l.line_type = 'WORK' AND o.subject_type = 'WORK' AND o.work_id = l.work_id)
        OR (l.line_type = 'HEAD' AND o.subject_type = 'HEAD' AND o.office_id = p.origin_office_id
            AND o.account_code_id = l.account_code_id))
) ob ON true
LEFT JOIN LATERAL (
    SELECT o.amount FROM budget.opening_figure o
    WHERE o.cycle_id = p.cycle_id AND o.figure_kind = 'RBE_PREVIOUS'
      AND ((l.line_type = 'WORK' AND o.subject_type = 'WORK' AND o.work_id = l.work_id)
        OR (l.line_type = 'HEAD' AND o.subject_type = 'HEAD' AND o.office_id = p.origin_office_id
            AND o.account_code_id = l.account_code_id))
) rp ON true
LEFT JOIN LATERAL (
    SELECT e.amount FROM budget.expenditure_figure e
    WHERE e.fin_year = c.current_fin_year AND e.period IN ('UP_TO_DATE', 'UP_TO_MONTH')
      AND e.nature = CASE WHEN p.category = 'RECEIPTS' THEN 'RECEIPT' ELSE 'EXPENDITURE' END
      AND ((l.line_type = 'WORK' AND e.subject_type = 'WORK' AND e.work_id = l.work_id)
        OR (l.line_type = 'HEAD' AND e.subject_type = 'HEAD' AND e.office_id = p.origin_office_id
            AND e.account_code_id = l.account_code_id))
    ORDER BY (e.source = 'KEYED') DESC, e.as_at DESC, e.expenditure_figure_id DESC
    LIMIT 1
) ex ON true
LEFT JOIN LATERAL (
    SELECT e.amount FROM budget.expenditure_figure e
    WHERE e.fin_year = c.current_fin_year - 1 AND e.period = 'FULL_YEAR'
      AND e.nature = CASE WHEN p.category = 'RECEIPTS' THEN 'RECEIPT' ELSE 'EXPENDITURE' END
      AND ((l.line_type = 'WORK' AND e.subject_type = 'WORK' AND e.work_id = l.work_id)
        OR (l.line_type = 'HEAD' AND e.subject_type = 'HEAD' AND e.office_id = p.origin_office_id
            AND e.account_code_id = l.account_code_id))
    ORDER BY (e.source = 'KEYED') DESC, e.as_at DESC, e.expenditure_figure_id DESC
    LIMIT 1
) ey ON true
LEFT JOIN LATERAL (
    SELECT jsonb_agg(jsonb_build_object('level_office_id', x.level_office_id, 'figure', x.figure, 'value', x.value)
                     ORDER BY x.first_at, x.figure) AS level_figures
    FROM (
        SELECT DISTINCT ON (f.level_office_id, f.figure)
               f.level_office_id, f.figure, f.value,
               min(f.created_at) OVER (PARTITION BY f.level_office_id) AS first_at
        FROM budget.figure_version f
        WHERE f.demand_line_id = l.demand_line_id
        ORDER BY f.level_office_id, f.figure, f.created_at DESC, f.figure_version_id DESC
    ) x
) lf ON true
LEFT JOIN LATERAL (
    SELECT a.admin_approval_id, a.current_version_no, v.name_of_work, v.aa_reference_no, v.date_of_approval,
           v.amount_sanctioned
    FROM budget.admin_approval a
    JOIN budget.admin_approval_version v ON v.admin_approval_id = a.admin_approval_id AND v.version_no = a.current_version_no
    WHERE a.work_id = l.work_id
) aa ON l.line_type = 'WORK'
LEFT JOIN LATERAL (
    SELECT count(*)::int AS sanction_count, sum(v.ts_amount) AS ts_total
    FROM budget.technical_sanction t
    JOIN budget.technical_sanction_version v ON v.technical_sanction_id = t.technical_sanction_id AND v.version_no = t.current_version_no
    WHERE t.work_id = l.work_id
) ts ON l.line_type = 'WORK'
LEFT JOIN LATERAL (
    SELECT d.rbe_amount, d.obe_next_amount
    FROM budget.ce_recommendation r
    JOIN budget.distribution d ON d.recommendation_id = r.recommendation_id AND d.office_id = p.origin_office_id
    WHERE r.cycle_id = p.cycle_id AND r.category = p.category AND r.account_code_id = l.account_code_id
    LIMIT 1
) col9 ON l.account_code_id IS NOT NULL
LEFT JOIN LATERAL (
    -- V-02 carry-forward: the operative provision of the line's work for the cycle's current year (BUD-184).
    SELECT vb.provision_id, vb.available
    FROM budget.v_provision_balance vb
    WHERE vb.is_operative AND vb.holder_type = 'WORK'
      AND vb.work_id = l.work_id AND vb.fin_year = c.current_fin_year
    LIMIT 1
) cf ON l.line_type = 'WORK';
