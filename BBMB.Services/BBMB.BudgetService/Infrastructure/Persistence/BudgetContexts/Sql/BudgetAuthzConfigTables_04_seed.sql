-- BudgetAuthzConfigTables step 4 (design §1, §7): seed the seven tables from the CURRENT appsettings.json
-- (BudgetProcess.ProjectCode / EditLeaseMinutes, BudgetRights.ProjectName / RoleOfficeLevels / Rights /
-- QueryFieldRoles / AdminRoles) so behaviour is unchanged on deploy. The ONE exception is admin roles: they
-- seed as a PENDING admin assignment with no document, so they lose admin access until approved (requirement
-- 4; disclosed in the design). The migration-actor fragment has run just before this step, so every insert
-- here is audited into budget.audit_log by the triggers attached in step 2.
--
-- The project-code "1" and the project name "Budget" are seeded as DATA here; they are never hard-coded in C#.
-- sso_project_id is unknown today (OQ-2) → seeded NULL; TokenClaimsRoleSource falls back to the name match.

DO $seed$
DECLARE
    -- role_name -> office_level (BudgetRights.RoleOfficeLevels, verbatim).
    role_names  text[] := ARRAY['DDO','SE','CE','SECRETARY','AFA_BUDGET','FIN_VIEW','IT_CELL','ProjectAdmin'];
    role_levels text[] := ARRAY['DIVISION','CIRCLE','CE','SECRETARIAT','FINANCE','FINANCE','IT_CELL','FINANCE'];
    -- Admin roles (BudgetRights.AdminRoles, verbatim).
    admin_roles text[] := ARRAY['ProjectAdmin'];
    -- Every right key (keys of BudgetRights.Rights) plus Settings.Admin (design §6: gates the new config/admin
    -- endpoints; also added to appsettings so BudgetEndpointPolicyTests' key-set assertion stays green).
    right_keys  text[] := ARRAY[
        'F01.Create','F01.MarkNotForUse','F17.Assign','F10.Act','F13.Act','F19.Allocate','F20.Freeze',
        'F21.Enter','F23.Set','Cycle.Manage','Settings.Manage','Finance.Open','F01.FlagPartyEmp',
        'Settings.ExpenditureSource','Offices.Load','F03.Enter','F06.Enter','F07.Enter','F07.Submit',
        'F29.Allocate','F30.Submit','Works.Enter','F11.Enter','F11.Submit','Review.Examine',
        'Review.ForwardFinance','F14.Distribute','F30.Authorise','F30.Examine','Proposal.Recall',
        'Proposal.Reply','Budget.View','Legacy.BudgetEntry','Attachment.Upload','Settings.Admin'];
    -- Query fields (keys of BudgetRights.QueryFieldRoles, verbatim).
    field_names text[] := ARRAY[
        'category','state','finYear','wing','ce','circle','division','lineType','accountCode',
        'demandedRbe','demandedObeNext','distributedRbe','distributedObeNext'];
    -- role -> rights (BudgetRights.Rights, each key's array), as "right=role" pairs.
    rr_pairs text[] := ARRAY[
        'F01.Create=AFA_BUDGET','F01.MarkNotForUse=AFA_BUDGET','F17.Assign=AFA_BUDGET','F10.Act=AFA_BUDGET',
        'F13.Act=AFA_BUDGET','F19.Allocate=AFA_BUDGET','F20.Freeze=AFA_BUDGET','F21.Enter=AFA_BUDGET',
        'F23.Set=AFA_BUDGET','Cycle.Manage=AFA_BUDGET','Settings.Manage=AFA_BUDGET','Finance.Open=AFA_BUDGET',
        'F01.FlagPartyEmp=IT_CELL','Settings.ExpenditureSource=IT_CELL','Offices.Load=IT_CELL',
        'F03.Enter=DDO','F06.Enter=DDO','F07.Enter=DDO','F07.Submit=DDO','F29.Allocate=DDO','F30.Submit=DDO',
        'Works.Enter=DDO','F11.Enter=DDO','F11.Enter=SE','F11.Enter=CE','F11.Submit=DDO','F11.Submit=SE',
        'F11.Submit=CE','Review.Examine=SE','Review.Examine=CE','Review.Examine=SECRETARY',
        'Review.ForwardFinance=SECRETARY','F14.Distribute=CE','F30.Authorise=CE','F30.Examine=SE',
        'Proposal.Recall=DDO','Proposal.Recall=SE','Proposal.Recall=CE','Proposal.Recall=SECRETARY',
        'Proposal.Reply=DDO','Proposal.Reply=SE','Proposal.Reply=CE',
        'Budget.View=DDO','Budget.View=SE','Budget.View=CE','Budget.View=SECRETARY','Budget.View=AFA_BUDGET',
        'Budget.View=FIN_VIEW','Budget.View=IT_CELL',
        'Legacy.BudgetEntry=DDO','Legacy.BudgetEntry=AFA_BUDGET',
        'Attachment.Upload=DDO','Attachment.Upload=SE','Attachment.Upload=CE','Attachment.Upload=SECRETARY',
        'Attachment.Upload=AFA_BUDGET',
        -- Settings.Admin: the new config/admin right (design §6), granted to the admin role and IT_CELL.
        'Settings.Admin=ProjectAdmin','Settings.Admin=IT_CELL'];
    -- Every QueryFieldRoles field is granted to the same eight roles (appsettings default).
    qf_roles text[] := ARRAY['DDO','SE','CE','SECRETARY','AFA_BUDGET','FIN_VIEW','IT_CELL','ProjectAdmin'];
    i int;
    pair text;
    fkey text;
    frole text;
    v_role_id bigint;
    v_right_id bigint;
    v_field_id bigint;
    fname text;
    arole text;
BEGIN
    -- budget_project_settings: one active row (BudgetProcess + BudgetRights.ProjectName).
    INSERT INTO budget.budget_project_settings
        (sso_project_id, sso_project_name, employee_charge_project_code, edit_lease_minutes, is_active)
        VALUES (NULL, 'Budget', '1', 20, true);

    -- budget_role (RoleOfficeLevels + is_admin from AdminRoles).
    FOR i IN array_lower(role_names, 1) .. array_upper(role_names, 1) LOOP
        INSERT INTO budget.budget_role (role_name, office_level, is_admin, is_active)
            VALUES (role_names[i], role_levels[i], role_names[i] = ANY (admin_roles), true);
    END LOOP;

    -- budget_right (keys of Rights + Settings.Admin).
    FOR i IN array_lower(right_keys, 1) .. array_upper(right_keys, 1) LOOP
        INSERT INTO budget.budget_right (right_key, is_active) VALUES (right_keys[i], true);
    END LOOP;

    -- budget_query_field (keys of QueryFieldRoles).
    FOR i IN array_lower(field_names, 1) .. array_upper(field_names, 1) LOOP
        INSERT INTO budget.budget_query_field (field_name, is_active) VALUES (field_names[i], true);
    END LOOP;

    -- budget_role_right (Rights arrays).
    FOREACH pair IN ARRAY rr_pairs LOOP
        fkey  := split_part(pair, '=', 1);
        frole := split_part(pair, '=', 2);
        SELECT id INTO v_role_id  FROM budget.budget_role  WHERE role_name = frole;
        SELECT id INTO v_right_id FROM budget.budget_right WHERE right_key = fkey;
        INSERT INTO budget.budget_role_right (role_id, right_id) VALUES (v_role_id, v_right_id);
    END LOOP;

    -- budget_role_query_field (every field to the eight default roles).
    FOREACH fname IN ARRAY field_names LOOP
        SELECT id INTO v_field_id FROM budget.budget_query_field WHERE field_name = fname;
        FOREACH frole IN ARRAY qf_roles LOOP
            SELECT id INTO v_role_id FROM budget.budget_role WHERE role_name = frole;
            INSERT INTO budget.budget_role_query_field (role_id, query_field_id) VALUES (v_role_id, v_field_id);
        END LOOP;
    END LOOP;

    -- budget_admin_role_assignment: one PENDING row per AdminRoles value, no document (requirement 4). The
    -- role is NOT an effective admin until approved, so it loses the blanket admin access it had under
    -- appsettings — the single deliberate behaviour change on deploy.
    FOREACH arole IN ARRAY admin_roles LOOP
        SELECT id INTO v_role_id FROM budget.budget_role WHERE role_name = arole;
        INSERT INTO budget.budget_admin_role_assignment (role_id, status) VALUES (v_role_id, 'PENDING');
    END LOOP;
END
$seed$;
