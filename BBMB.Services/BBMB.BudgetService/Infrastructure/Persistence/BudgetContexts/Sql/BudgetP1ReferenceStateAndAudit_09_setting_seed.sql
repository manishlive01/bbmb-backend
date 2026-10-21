-- BudgetP1ReferenceStateAndAudit step 9: T-17 seed, DDD §9 L-02 and the DDD T-17 note (design §5.3).
-- Runs after the migration-actor fragment. UNFORESEEN_ACCOUNT_CODE is not seeded: null means no head is
-- set and unforeseen provision is shown as a line of its own (BUD-277).
INSERT INTO budget.setting (setting_key, numeric_value, account_code_id, effective_from, authority_ref) VALUES
    ('TS_CEILING_PCT',        105,   NULL, now(), 'DDD L-02 seed'),  -- URS TSN-005, VR-02
    ('NO_AA_LIMIT_RUPEES',    10000, NULL, now(), 'DDD L-02 seed'),  -- WKB-007, VR-03
    ('AA_DEMAND_CEILING_PCT', 110,   NULL, now(), 'DDD L-02 seed'),  -- WKB-010, VR-04
    ('AMR_INCREASE_PCT',      5,     NULL, now(), 'DDD L-02 seed'),  -- WKB-010A, VR-05
    ('UPLOAD_MAX_LINES',      5000,  NULL, now(), 'DDD L-02 seed');  -- NIC design decision at BUD-103
