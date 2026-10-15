-- BudgetP6Reporting step 5 (design §5.4, §8.8 F-26): the P4/P5-INDEPENDENT reporting surface. The full V-06
-- v_consolidation reads allocation/provision (T-40/T-41/T-42) and re-appropriation (T-46), which do not exist
-- until P4/P5; it is carried as the deferred P6-01b and is NOT created here. This view is the queryable fact
-- table for the query builder (F-26) over the tables that exist now (P1/P2/P3): one row per non-deleted demand
-- line with its proposal, cycle, framing office, wing/CE/Circle/Division keys and head, the demanded figures
-- (columns 9/10) and the distributed share for the framing office. No allocation, freeze or re-appropriation
-- column appears; where such a figure would belong the query builder exposes a documented 'not yet available'
-- field rather than a fabricated number (P6-03/P6-04 scope note, user time-box 2026-10-15).
CREATE VIEW budget.v_report_demand AS
SELECT l.demand_line_id,
       p.proposal_id,
       p.cycle_id,
       c.current_fin_year,
       p.category,
       p.state_code,
       p.origin_office_id,
       o.office_level           AS origin_office_level,
       o.wing_code,
       o.ce_id,
       o.circle_id,
       o.division_id,
       l.line_type,
       l.work_id,
       l.account_code_id,
       h.complete_code,
       h.head_desc,
       l.rbe_amount             AS demanded_rbe,
       l.obe_next_amount        AS demanded_obe_next,
       dist.rbe_amount          AS distributed_rbe,
       dist.obe_next_amount     AS distributed_obe_next
FROM budget.demand_line l
JOIN budget.proposal p ON p.proposal_id = l.proposal_id
JOIN budget.budget_cycle c ON c.cycle_id = p.cycle_id
JOIN budget.office o ON o.office_id = p.origin_office_id
LEFT JOIN budget.budget_head_master h ON h.account_code_id = l.account_code_id
LEFT JOIN LATERAL (
    SELECT d.rbe_amount, d.obe_next_amount
    FROM budget.ce_recommendation r
    JOIN budget.distribution d ON d.recommendation_id = r.recommendation_id AND d.office_id = p.origin_office_id
    WHERE r.cycle_id = p.cycle_id AND r.category = p.category AND r.account_code_id = l.account_code_id
    LIMIT 1
) dist ON l.account_code_id IS NOT NULL
WHERE NOT l.is_deleted;
