using System.Text.RegularExpressions;
using Npgsql;

namespace BBMB.BudgetService.Shared.Messages
{
    /// <summary>
    /// The message catalogue of SRS BBMB-SRS-BUD-01 §7 (BUD-012). Text is the SRS wording verbatim; a
    /// change of wording is a catalogue change and is made here only. Design §2.1, §4.9.
    /// </summary>
    public static partial class BudgetMessages
    {
        public const string M01 = "M-01";
        public const string M02 = "M-02";
        public const string M03 = "M-03";
        public const string M04 = "M-04";
        public const string M05 = "M-05";
        public const string M06 = "M-06";
        public const string M07 = "M-07";
        public const string M08 = "M-08";
        public const string M09 = "M-09";
        public const string M10 = "M-10";
        public const string M11 = "M-11";
        public const string M12 = "M-12";
        public const string M13 = "M-13";
        public const string M14 = "M-14";
        public const string M15 = "M-15";
        public const string M16 = "M-16";
        public const string M17 = "M-17";
        public const string M18 = "M-18";
        public const string M19 = "M-19";
        public const string M20 = "M-20";
        public const string M21 = "M-21";
        public const string M22 = "M-22";
        public const string M23 = "M-23";
        public const string M24 = "M-24";
        public const string M25 = "M-25";
        public const string M26 = "M-26";
        public const string M27 = "M-27";
        public const string M28 = "M-28";
        public const string M29 = "M-29";
        public const string M30 = "M-30";
        public const string M31 = "M-31";
        public const string M32 = "M-32";
        public const string M33 = "M-33";
        public const string M34 = "M-34";
        public const string M35 = "M-35";
        public const string M36 = "M-36";

        /// <summary>Record absent or outside the actor's visible offices (design §4.9). Not an SRS number.</summary>
        public const string NotFound = "NOT_FOUND";

        /// <summary>BUD-122 advice to record a justification. Interim id, number to be assigned (OI-13).</summary>
        public const string Adv122 = "ADV-122";

        /// <summary>Stored attachment digest mismatch (design §4.8, §10). Interim id (OI-13).</summary>
        public const string Integrity = "INTEGRITY";

        private static readonly IReadOnlyDictionary<string, string> Catalogue = new Dictionary<string, string>
        {
            [M01] = "The code entered already exists under the same parent head. Enter a code not already used at this level.",
            [M02] = "The complete Account Code formed by these six segments already exists in the Manual.",
            [M03] = "The e-Office reference of the approval of the Group Head is required before the code can be created.",
            [M04] = "The scanned copy of the administrative approval has to be attached before the entry can be submitted.",
            [M05] = "The Account Code selected does not exist in the Manual of Accounts Codes.",
            [M06] = "The technical sanctions against this approval would exceed 105 per cent of the approved amount. The administrative approval has to be revised before a larger sanction can be recorded. On the AM&R tab, a second sanction against the same work is not accepted.",
            [M07] = "The scanned copy of the technical sanction has to be attached before the entry can be submitted.",
            [M08] = "Provision against a work having no administrative approval cannot exceed Rs. 10,000 under either estimate unless the certificate of the Chief Engineer concerned, that the work is urgent and that the budget will be utilised as demanded, is attached.",
            [M09] = "The revised estimate and the estimate for the coming year, added together, exceed 110 per cent of the amount approved under the administrative approval.",
            [M10] = "The figure demanded is more than 5 per cent above the budget approved for the AM&R work this work continues. A justification and the approval of the Chief Engineer concerned are required.",
            [M11] = "The file uploaded is not in the template the module issues. Download the template and enter the demand in it.",
            [M12] = "A remark is required before the proposal can be returned or an observation recorded.",
            [M13] = "This account head is not assigned to your office. Demand can be made only against the heads assigned.",
            [M14] = "The total distributed against this account code exceeds the amount approved for your office against the same code.",
            [M15] = "This head cannot be removed from the office. A demand or an allocation exists against it.",
            [M16] = "The minutes of the Budget Sub-Committee or of the Full Board have to be uploaded before an allocation can be made.",
            [M17] = "The budget for this cycle has been frozen. A change can now be made only by re-appropriation.",
            [M18] = "The amount surrendered would exceed the additional amount required. Reduce the amount or remove a surrender line.",
            [M19] = "This action is not available at the present stage of the proposal.",
            [M20] = "You are not authorised to perform this action.",
            [M21] = "The named field is required.",
            [M22] = "The value entered in the named field is not of the required form. The expected form is stated.",
            [M23] = "The proposal is open at your office with another officer and can be seen but not changed.",
            [M24] = "The file is of a format or a size the module does not accept. The permitted formats and the limit are stated.",
            [M25] = "This work is already present in the demand and has not been entered again.",
            [M26] = "The last date prescribed in the budget calendar for action at this level has passed.",
            [M27] = "The amount sanctioned is above the powers delegated to the authority recorded. Record a justification to proceed.",
            [M28] = "The total allocated to the estimates of this work exceeds the budget allocated to the work. Reduce the amount or allocate less against another estimate.",
            [M29] = "The amount exceeds the balance available under unforeseen work for the office concerned.",
            [M30] = "The distribution of one or more account codes no longer agrees with the revised figure. The distribution has to be corrected before the budget can be frozen.",
            [M31] = "The revised technically sanctioned amount is below the expenditure already booked against the estimate code of this sanction.",
            [M32] = "The figure this check is measured against is nil or negative, so the percentage check has not been applied to this line.",
            [M33] = "This account code cannot be marked not for use. A demand, an allocation, a re-appropriation or an expenditure exists against it.",
            [M34] = "The basis on which provision was allocated against this work or head has changed. The allocated figure exceeds what the changed basis supports by the amount shown, and is to be corrected by re-appropriation.",
            [M35] = "This account head already carries a line in this demand. Amend the line already present rather than adding a second.",
            [M36] = "The revised estimate is below the expenditure already incurred, shown against it. The revised estimate cannot be less than what has already been spent.",

            // Interim, not SRS-numbered (OI-13). Wording follows the requirement each serves.
            [NotFound] = "The record requested does not exist or is not visible to your office.",
            [Adv122] = "The figure has been altered without a justification. Record a justification, or select Continue without justification.",
            [Integrity] = "The stored file does not match the digest recorded when it was uploaded and has not been served.",
        };

        /// <summary>
        /// Unique-constraint name → message id for 23505 (design §4.9). Filled as the constraints that
        /// carry a uniqueness message arrive in each phase's migration.
        /// </summary>
        private static readonly Dictionary<string, string> UniqueConstraintMessages = new(StringComparer.Ordinal)
        {
            // T-30 partial UKs (BudgetContext.UxDemandLineWork / UxDemandLineHead).
            ["ux_demand_line_work"] = M25, // a work once in a demand (BUD-104)
            ["ux_demand_line_head"] = M35, // one head once in a head-wise demand (VR-14, BUD-310)
            // Backstops behind the handler pre-checks (concurrent requests): M-19 as the cycle choice under OI-13.
            ["IX_budget_cycle_current_fin_year"] = M19, // T-28 one cycle per financial year
            ["IX_budget_cycle_is_first_cycle"] = M19,   // T-28 one first cycle
            ["IX_setting_setting_key_effective_from"] = M19, // T-17 one row per key and instant
            ["IX_proposal_cycle_id_category_origin_office_id"] = M19, // T-29 one proposal per cycle, category, origin
            // T-07: the six segments, and the complete code they form, exist once in the Manual (VR-01, BUD-042).
            ["PK_budget_head_master"] = M02,
            ["IX_budget_head_master_complete_code"] = M02,
            // T-27 partial UK: one sanction only against an AM&R work (TSN-007, BUD-274; M-06 names it).
            ["ux_technical_sanction_amr_work"] = M06,
            // T-25 UKs, backstops behind the handler (one approval per work; serial allotted in the Division).
            ["ux_admin_approval_work"] = M19,
            ["ux_admin_approval_division_serial"] = M19,
        };

        /// <summary>The uniqueness message id registered for <paramref name="constraintName"/>, or null.</summary>
        public static string? UniqueConstraintMessageFor(string constraintName) =>
            UniqueConstraintMessages.TryGetValue(constraintName, out var id) ? id : null;
        /// <summary>Every catalogued id with its text.</summary>
        public static IReadOnlyDictionary<string, string> All => Catalogue;

        public static bool IsKnown(string id) => Catalogue.ContainsKey(id);

        /// <summary>The catalogue text of <paramref name="id"/>. Throws for an unknown id: a coding defect.</summary>
        public static string Text(string id) =>
            Catalogue.TryGetValue(id, out var text)
                ? text
                : throw new ArgumentOutOfRangeException(nameof(id), id, "Message id is not in the catalogue.");

        /// <summary>The form carried in <c>Response.Message</c>: <c>"M-06: &lt;text&gt;"</c>.</summary>
        public static string Format(string id) => $"{id}: {Text(id)}";

        /// <summary>Registers the uniqueness message of a constraint. Called once per constraint at type load.</summary>
        internal static void MapUniqueConstraint(string constraintName, string messageId)
        {
            if (!IsKnown(messageId))
                throw new ArgumentOutOfRangeException(nameof(messageId), messageId, "Message id is not in the catalogue.");
            UniqueConstraintMessages[constraintName] = messageId;
        }

        [GeneratedRegex(@"^M-\d{2}$")]
        private static partial Regex RaisedMessageId();

        /// <summary>
        /// Maps a PostgreSQL error raised by a database rule to the catalogue (design §4.9, §9.3):
        /// <list type="bullet">
        /// <item>P0001 whose text is exactly an M-xx id → that message;</item>
        /// <item>23503 (foreign key) → M-05;</item>
        /// <item>23505 (unique) → the message registered for the constraint;</item>
        /// <item>anything else → null (the caller rethrows to the global handler).</item>
        /// </list>
        /// The result never contains a table, column or constraint name (BUD-012, BUD-261).
        /// </summary>
        public static (string MessageId, string Text)? FromPostgres(PostgresException ex)
        {
            ArgumentNullException.ThrowIfNull(ex);
            string? id = ex.SqlState switch
            {
                PostgresErrorCodes.RaiseException when ex.MessageText is { } m
                    && RaisedMessageId().IsMatch(m) && IsKnown(m) => m,
                PostgresErrorCodes.ForeignKeyViolation => M05,
                PostgresErrorCodes.UniqueViolation when ex.ConstraintName is { } c
                    && UniqueConstraintMessages.TryGetValue(c, out var mapped) => mapped,
                _ => null,
            };
            return id is null ? null : (id, Text(id));
        }
    }
}
