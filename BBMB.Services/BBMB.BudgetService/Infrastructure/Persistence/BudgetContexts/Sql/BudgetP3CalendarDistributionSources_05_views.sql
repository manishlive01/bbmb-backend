-- BudgetP3CalendarDistributionSources step 5: V-05 and V-04 version 3 (design §5.4).
-- V-05 budget.v_distribution_balance (DDD V-05, BUD-158, BUD-160): for each recommendation, the figure
-- recommended, the total distributed and the balance retained at the CE office, column by column. The balance
-- is computed, never stored.
CREATE VIEW budget.v_distribution_balance AS
SELECT r.recommendation_id,
       r.cycle_id,
       r.category,
       r.ce_office_id,
       r.account_code_id,
       coalesce(r.rbe_recommended, 0)                         AS rbe_recommended,
       coalesce(r.obe_next_recommended, 0)                    AS obe_next_recommended,
       coalesce(d.rbe_distributed, 0)                         AS rbe_distributed,
       coalesce(d.obe_distributed, 0)                         AS obe_distributed,
       coalesce(r.rbe_recommended, 0) - coalesce(d.rbe_distributed, 0)      AS rbe_balance_retained,
       coalesce(r.obe_next_recommended, 0) - coalesce(d.obe_distributed, 0) AS obe_balance_retained,
       r.redistribution_required
FROM budget.ce_recommendation r
LEFT JOIN LATERAL (
    SELECT sum(x.rbe_amount) AS rbe_distributed, sum(x.obe_next_amount) AS obe_distributed
    FROM budget.distribution x WHERE x.recommendation_id = r.recommendation_id
) d ON true;

-- V-04 budget.v_demand_line_display, version 3: version 2 plus columns 9 and 10 of URS Table 3/4 for the
-- office that frames the demand (DDD V-04 "columns 9 and 10 from T-37 and T-39 for the viewing office").
-- Column 9/10 for a line are the share distributed to the proposal's origin office under the recommendation
-- for the line's head and category in the cycle; where the origin office is the CE office itself, the balance
-- retained (V-05). Null where no recommendation or distribution yet exists (shown as not available, never nil).
-- CREATE OR REPLACE VIEW appends columns only, so every version-2 column keeps its name, type and position.
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
       col9.obe_next_amount    AS recommended_obe_for_office
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
    -- Columns 9/10 for the framing office: its distributed share, or (where it is the CE office) the balance
    -- retained. Keyed on the line's head and the proposal's category and cycle. Null where none exists yet.
    SELECT d.rbe_amount, d.obe_next_amount
    FROM budget.ce_recommendation r
    JOIN budget.distribution d ON d.recommendation_id = r.recommendation_id AND d.office_id = p.origin_office_id
    WHERE r.cycle_id = p.cycle_id AND r.category = p.category AND r.account_code_id = l.account_code_id
    LIMIT 1
) col9 ON l.account_code_id IS NOT NULL;
