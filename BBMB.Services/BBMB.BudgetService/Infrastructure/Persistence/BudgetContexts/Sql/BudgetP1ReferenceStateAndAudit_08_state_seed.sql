-- BudgetP1ReferenceStateAndAudit step 8: T-15 seed, DDD §9 L-02 (sixteen states). Runs after the
-- migration-actor fragment, so every row has a T-51 insert row attributed to the executing officer.
-- state_name: SRS §5 wording. holder_level: the T-13 level of SRS §5 "Held by" (S-01 and S-04 are held
-- by the framing office; DIVISION is the nominal level, the actual holder is proposal.holder_office_id).
-- figures_open: S-01, S-03, S-04, S-06, S-08, S-10 (S-10 for WORKS only, enforced by DB-11).
INSERT INTO budget.state (state_code, state_name, holder_level, figures_open) VALUES
    ('S-01', 'Under preparation',                 'DIVISION',    true),
    ('S-02', 'Submitted, awaiting Circle',        'CIRCLE',      false),
    ('S-03', 'Under examination at Circle',       'CIRCLE',      true),
    ('S-04', 'Returned to Division',              'DIVISION',    true),
    ('S-05', 'Forwarded, awaiting CE',            'CE',          false),
    ('S-06', 'Under examination at CE',           'CE',          true),
    ('S-07', 'Forwarded, awaiting Secretary',     'SECRETARIAT', false),
    ('S-08', 'Under examination at Secretary',    'SECRETARIAT', true),
    ('S-09', 'Awaiting scrutiny at Finance',      'FINANCE',     false),
    ('S-10', 'Under scrutiny at Finance',         'FINANCE',     true),
    ('S-11', 'Finalised at Finance',              'FINANCE',     false),
    ('S-12', 'Before the Budget Sub-Committee',   'FINANCE',     false),
    ('S-13', 'Revised estimate allocated',        'FINANCE',     false),
    ('S-14', 'Before the Full Board',             'FINANCE',     false),
    ('S-15', 'Coming year''s estimate allocated', 'FINANCE',     false),
    ('S-16', 'Frozen',                            'FINANCE',     false);
