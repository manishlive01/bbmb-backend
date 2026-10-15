namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines
{
    /// <summary>
    /// The works demand upload template (F-08, BUD-097). The template carries the fourteen columns of
    /// URS Table 3 (F-07) in the order the URS gives them (BUD-077), so an officer who has downloaded the
    /// template reads it as the screen (BUD-097). The upload refuses a file whose headings or order do not
    /// match this list exactly, with M-11, and does not read it (BUD-098).
    /// <para>
    /// Of the fourteen, five are entered by the DDO (SRS §4.7). The upload reads the ones that frame a works
    /// line — Account Code (col 2), Name of Work (col 5), RBE (col 11), OBE next (col 12), Justification Note
    /// (col 13) and the preceding year's AM&amp;R work (col 14). The displayed and system columns
    /// (1, 3, 4, 6, 7, 9) are present in the template for alignment with the screen but are not read; the two
    /// expenditure columns (8, 10) are keyed through F-07's expenditure action, not the demand line, and are
    /// likewise not taken from the sheet. A value in a not-read column is ignored, never an error, so the
    /// officer may paste the screen's export unchanged.
    /// </para>
    /// </summary>
    public static class UploadTemplateColumns
    {
        /// <summary>Column 2 (1-based worksheet column), Account Code.</summary>
        public const int AccountCode = 2;
        /// <summary>Column 5, Name of Work.</summary>
        public const int NameOfWork = 5;
        /// <summary>Column 11, RBE (revised estimate of the current year).</summary>
        public const int Rbe = 11;
        /// <summary>Column 12, OBE (estimate for the next financial year).</summary>
        public const int ObeNext = 12;
        /// <summary>Column 13, Justification Note.</summary>
        public const int Justification = 13;
        /// <summary>Column 14, previous year's AM&amp;R work.</summary>
        public const int PrecedingAmrWork = 14;

        /// <summary>The fourteen headings of F-07, in order (BUD-077, BUD-097). The heading row the upload checks.</summary>
        public static readonly IReadOnlyList<string> Headings =
        [
            "Sr. No.",
            "Account Code",
            "A&A sanction No. and date",
            "A&A Amount",
            "Name of Work",
            "Work code",
            "Technically sanctioned amount to date",
            "Expenditure, previous year",
            "OBE, current financial year",
            "Expenditure, up to date",
            "RBE, revised estimate of the current year",
            "OBE, estimate for the next financial year",
            "Justification Note",
            "Previous year's AM&R work",
        ];
    }
}
