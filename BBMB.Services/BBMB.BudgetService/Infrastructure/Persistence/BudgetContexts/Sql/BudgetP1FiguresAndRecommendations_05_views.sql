-- BudgetP1FiguresAndRecommendations step 5: views of DDD §8 (design §5.4).

-- V-03 budget.v_demand_by_ce: for head-wise categories, the totals of columns 7 and 8 by cycle, CE and
-- account code, over the non-deleted HEAD lines of the CE office and of the Circles and Divisions under it
-- (BUD-302, BUD-304). Sums of numeric, no intermediate rounding.
CREATE VIEW budget.v_demand_by_ce AS
WITH line_ce AS (
    SELECT p.cycle_id,
           p.category,
           p.origin_office_id,
           l.account_code_id,
           l.rbe_amount,
           l.obe_next_amount,
           CASE o.office_level
               WHEN 'CE' THEN o.office_id
               WHEN 'CIRCLE' THEN o.parent_office_id
               WHEN 'DIVISION' THEN po.parent_office_id
           END AS ce_office_id
    FROM budget.demand_line l
    JOIN budget.proposal p ON p.proposal_id = l.proposal_id
    JOIN budget.office o ON o.office_id = p.origin_office_id
    LEFT JOIN budget.office po ON po.office_id = o.parent_office_id
    WHERE l.line_type = 'HEAD' AND NOT l.is_deleted AND p.category <> 'WORKS'
)
SELECT cycle_id,
       category,
       ce_office_id,
       account_code_id,
       sum(rbe_amount)                  AS rbe_total,
       sum(obe_next_amount)             AS obe_next_total,
       count(*)::int                    AS line_count,
       count(DISTINCT origin_office_id)::int AS office_count
FROM line_ce
WHERE ce_office_id IS NOT NULL
GROUP BY cycle_id, category, ce_office_id, account_code_id;

-- V-04 budget.v_demand_line_display, version 1 (design §5.4): the line with the work, the account code,
-- the first-cycle opening figures (T-36), expenditure (T-35; a keyed figure is never replaced by a drawn one,
-- BUD-025) and the figure at each level from T-31. P2, P3 and P4 extend it by CREATE OR REPLACE VIEW.
CREATE VIEW budget.v_demand_line_display AS
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
       coalesce(lf.level_figures, '[]'::jsonb) AS level_figures
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
) lf ON true;
