-- BudgetP5Reappropriation step 5: V-10 (design §5.4, DDD V-10, F-22; BUD-208, BUD-300). The re-appropriation
-- register of a year, with the unforeseen authorisations shown as rows of their own and marked UNFORESEEN_AUTH
-- (an unforeseen authorisation is a change of provision in the same ledger, BUD-300). Each T-46 row's office is
-- its destination provision's office, else its new work's Division office.
CREATE VIEW budget.v_reappropriation_register AS
SELECT 'REAPPROPRIATION'::varchar(16) AS entry_kind,
       r.reappropriation_id            AS entry_id,
       r.fin_year,
       r.serial_no,
       r.status,
       r.additional_amount             AS amount,
       r.eoffice_order_ref             AS eoffice_ref,
       coalesce(dp.office_id, dw.division_id_office)  AS office_id,
       r.created_at
FROM budget.reappropriation r
LEFT JOIN budget.provision dp ON dp.provision_id = r.destination_provision_id
LEFT JOIN LATERAL (
    SELECT o.office_id AS division_id_office
    FROM budget.work w
    JOIN budget.office o ON o.division_id = w.division_id AND o.office_level = 'DIVISION'
    WHERE w.id = r.destination_new_work_id
    LIMIT 1
) dw ON true
UNION ALL
SELECT 'UNFORESEEN_AUTH'::varchar(16)  AS entry_kind,
       a.unforeseen_authorisation_id   AS entry_id,
       up.fin_year,
       NULL::int                       AS serial_no,
       'AUTHORISED'::varchar(9)        AS status,
       coalesce(a.amount_current, 0) + coalesce(a.amount_next, 0) AS amount,
       NULL::varchar(100)              AS eoffice_ref,
       a.ce_office_id                  AS office_id,
       a.created_at
FROM budget.unforeseen_authorisation a
JOIN budget.unforeseen_request req ON req.unforeseen_request_id = a.unforeseen_request_id
LEFT JOIN LATERAL (
    SELECT p.fin_year FROM budget.provision p
    WHERE p.provision_id = coalesce(req.current_provision_id, req.next_provision_id)
    LIMIT 1
) up ON true;
