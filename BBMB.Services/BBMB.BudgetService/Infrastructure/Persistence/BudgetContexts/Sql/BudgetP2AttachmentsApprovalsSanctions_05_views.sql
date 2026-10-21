-- BudgetP2AttachmentsApprovalsSanctions step 5: views of DDD §8 (design §5.4).
-- V-04 budget.v_demand_line_display, version 2: version 1 plus the approval's current reference, date and
-- amount and the sanctions to date (DDD V-04; joined at display, never copied onto the line, DDD §1.7).
-- CREATE OR REPLACE VIEW only appends columns, so every version-1 column keeps its name, type and position.
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
       l.ce_amr_approval_attachment_id
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
) ts ON l.line_type = 'WORK';
-- V-08 budget.v_fa_work_sanction (DDD §8.2, TSN-009, BUD-270, BUD-284, BUD-287): works with their codes,
-- the current version of each approval and sanction, and whether the sanction awaits an estimate code.
-- One row per sanction; a work with an approval and no sanction appears once with null sanction columns.
CREATE VIEW budget.v_fa_work_sanction AS
SELECT w.id                       AS work_id,
       w.work_code,
       w.division_id,
       w.work_class,
       w.work_desc,
       a.admin_approval_id,
       av.version_no              AS aa_version_no,
       av.aa_reference_no,
       av.date_of_approval,
       av.amount_sanctioned       AS aa_amount_sanctioned,
       av.account_code_id         AS aa_account_code_id,
       t.technical_sanction_id,
       t.tab,
       tv.version_no              AS ts_version_no,
       tv.ts_number,
       tv.ts_date,
       tv.ts_amount,
       t.estimate_code,
       t.estimate_code_at,
       (t.technical_sanction_id IS NOT NULL AND t.estimate_code IS NULL) AS awaiting_estimate_code
FROM budget.work w
LEFT JOIN budget.admin_approval a ON a.work_id = w.id
LEFT JOIN budget.admin_approval_version av ON av.admin_approval_id = a.admin_approval_id AND av.version_no = a.current_version_no
LEFT JOIN budget.technical_sanction t ON t.work_id = w.id
LEFT JOIN budget.technical_sanction_version tv ON tv.technical_sanction_id = t.technical_sanction_id AND tv.version_no = t.current_version_no
WHERE a.admin_approval_id IS NOT NULL OR t.technical_sanction_id IS NOT NULL;
