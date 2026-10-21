-- BudgetP1StateTransitionSeed step 3: the T-16 seed, design §6.2 (rows T01 to T28 with the b/c variants;
-- the first pass's T25b S-11 PLACE_FB S-14 is absent). remark_required is true for RETURN, SEND_BACK,
-- REFER_BACK and REOPEN (DDD T-16). Runs after the migration-actor fragment, so each row has a T-51 row
-- whose actor carries the gate's recorded decision as justification.
INSERT INTO budget.state_transition (from_state, action_code, to_state, remark_required) VALUES
    ('S-01', 'SUBMIT', 'S-02', false),  -- T01
    ('S-01', 'SUBMIT', 'S-05', false),  -- T02
    ('S-01', 'SUBMIT', 'S-07', false),  -- T03
    ('S-02', 'RECALL', 'S-01', false),  -- T04
    ('S-02', 'RECALL', 'S-04', false),  -- T04b
    ('S-02', 'OPEN', 'S-03', false),  -- T05
    ('S-03', 'RETURN', 'S-04', true),  -- T06
    ('S-03', 'FORWARD', 'S-05', false),  -- T07
    ('S-04', 'RESUBMIT', 'S-02', false),  -- T08
    ('S-04', 'RESUBMIT', 'S-05', false),  -- T08b
    ('S-04', 'RESUBMIT', 'S-07', false),  -- T08c
    ('S-05', 'RECALL', 'S-03', false),  -- T09
    ('S-05', 'RECALL', 'S-01', false),  -- T09b
    ('S-05', 'RECALL', 'S-04', false),  -- T09c
    ('S-05', 'OPEN', 'S-06', false),  -- T10
    ('S-06', 'RETURN', 'S-03', true),  -- T11
    ('S-06', 'RETURN', 'S-04', true),  -- T11b
    ('S-06', 'FORWARD', 'S-07', false),  -- T12
    ('S-07', 'RECALL', 'S-06', false),  -- T13
    ('S-07', 'RECALL', 'S-01', false),  -- T13b
    ('S-07', 'RECALL', 'S-04', false),  -- T13c
    ('S-07', 'OPEN', 'S-08', false),  -- T14
    ('S-08', 'RETURN', 'S-06', true),  -- T15
    ('S-08', 'RETURN', 'S-04', true),  -- T15b
    ('S-08', 'FORWARD_FINANCE', 'S-09', false),  -- T16
    ('S-09', 'RECALL', 'S-08', false),  -- T17
    ('S-09', 'OPEN', 'S-10', false),  -- T18
    ('S-10', 'SEND_BACK', 'S-08', true),  -- T19
    ('S-10', 'FINALISE', 'S-11', false),  -- T20
    ('S-11', 'REOPEN', 'S-10', true),  -- T21
    ('S-11', 'PLACE_BSC', 'S-12', false),  -- T22
    ('S-12', 'ALLOCATE_RBE', 'S-13', false),  -- T23
    ('S-12', 'REFER_BACK', 'S-08', true),  -- T24
    ('S-13', 'PLACE_FB', 'S-14', false),  -- T25
    ('S-14', 'ALLOCATE_OBE', 'S-15', false),  -- T26
    ('S-14', 'REFER_BACK', 'S-08', true),  -- T27
    ('S-15', 'FREEZE', 'S-16', false);  -- T28
