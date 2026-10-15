-- BudgetP4AllocationProvisionFreeze Down (dev only, design §11.1 rule 6): undo the P4 database objects before
-- EF drops the tables and columns. The DB-17 trigger on ce_recommendation and the standalone functions are
-- dropped here (DropTable does not drop them). V-09 and V-02 are dropped (V-09 depends on provision/
-- estimate_allocation, V-02 on provision, so they must go before DropTable). V-04 is restored to its P3
-- version 3 (which does not reference the P4 provision balance) so DropTable does not fail on a dependent view.
DROP TRIGGER IF EXISTS trg_recommendation_cites_minutes ON budget.ce_recommendation;
DROP VIEW IF EXISTS budget.v_fa_provision;

-- Restore V-04 to version 3 (P3): drop the P4 carry-forward columns by replacing the view with the v3 body.
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
    SELECT d.rbe_amount, d.obe_next_amount
    FROM budget.ce_recommendation r
    JOIN budget.distribution d ON d.recommendation_id = r.recommendation_id AND d.office_id = p.origin_office_id
    WHERE r.cycle_id = p.cycle_id AND r.category = p.category AND r.account_code_id = l.account_code_id
    LIMIT 1
) col9 ON l.account_code_id IS NOT NULL;

DROP VIEW IF EXISTS budget.v_provision_balance;

-- Restore the P2 placeholder subject CHECK before the provision_id column is dropped by EF.
ALTER TABLE budget.correction_flag
    DROP CONSTRAINT ck_correction_flag_subject,
    ADD CONSTRAINT ck_correction_flag_subject CHECK (work_id IS NOT NULL);

DROP FUNCTION IF EXISTS budget.fn_freeze_set_cycle_frozen();
DROP FUNCTION IF EXISTS budget.fn_freeze_preconditions();
DROP FUNCTION IF EXISTS budget.fn_recommendation_cites_minutes();
DROP FUNCTION IF EXISTS budget.fn_unforeseen_authorisation_balance();
DROP FUNCTION IF EXISTS budget.fn_unforeseen_request_rules();
DROP FUNCTION IF EXISTS budget.fn_estimate_allocation_ceiling();
DROP FUNCTION IF EXISTS budget.fn_provision_movement_balance();
DROP FUNCTION IF EXISTS budget.fn_provision_movement_rules();
DROP FUNCTION IF EXISTS budget.fn_provision_write_once();
