namespace BBMB.BudgetService.Features.BudgetProcess.Proposals
{
    /// <summary>T-29 <c>category</c> values (DDD T-29 CHECK, BUD-236).</summary>
    public static class ProposalCategory
    {
        public const string Works = "WORKS";
        public const string Dna = "DNA";
        public const string Receipts = "RECEIPTS";
        public const string Capex = "CAPEX";

        public static readonly IReadOnlyList<string> All = [Works, Dna, Receipts, Capex];

        /// <summary>The three head-wise categories (F-11, F-15, F-16).</summary>
        public static readonly IReadOnlyList<string> HeadWise = [Dna, Receipts, Capex];
    }

    /// <summary>The sixteen states of SRS §5 / DDD T-15.</summary>
    public static class ProposalStateCode
    {
        public const string S01 = "S-01";
        public const string S02 = "S-02";
        public const string S03 = "S-03";
        public const string S04 = "S-04";
        public const string S05 = "S-05";
        public const string S06 = "S-06";
        public const string S07 = "S-07";
        public const string S08 = "S-08";
        public const string S09 = "S-09";
        public const string S10 = "S-10";
        public const string S11 = "S-11";
        public const string S12 = "S-12";
        public const string S13 = "S-13";
        public const string S14 = "S-14";
        public const string S15 = "S-15";
        public const string S16 = "S-16";
    }

    /// <summary>The sixteen actions of DDD T-16 (CHECK).</summary>
    public static class ProposalAction
    {
        public const string Submit = "SUBMIT";
        public const string Recall = "RECALL";
        public const string Open = "OPEN";
        public const string Return = "RETURN";
        public const string Resubmit = "RESUBMIT";
        public const string Forward = "FORWARD";
        public const string ForwardFinance = "FORWARD_FINANCE";
        public const string SendBack = "SEND_BACK";
        public const string Finalise = "FINALISE";
        public const string Reopen = "REOPEN";
        public const string PlaceBsc = "PLACE_BSC";
        public const string AllocateRbe = "ALLOCATE_RBE";
        public const string ReferBack = "REFER_BACK";
        public const string PlaceFb = "PLACE_FB";
        public const string AllocateObe = "ALLOCATE_OBE";
        public const string Freeze = "FREEZE";

        public static readonly IReadOnlyList<string> All =
        [
            Submit, Recall, Open, Return, Resubmit, Forward, ForwardFinance, SendBack, Finalise, Reopen,
            PlaceBsc, AllocateRbe, ReferBack, PlaceFb, AllocateObe, Freeze,
        ];
    }

    /// <summary>T-30 <c>line_type</c> values (CHECK; DB-01).</summary>
    public static class DemandLineType
    {
        public const string Work = "WORK";
        public const string Head = "HEAD";
        public const string Unforeseen = "UNFORESEEN";
    }

    /// <summary>T-31 <c>figure</c> and <c>change_source</c> values (CHECK).</summary>
    public static class FigureKind
    {
        public const string Rbe = "RBE";
        public const string ObeNext = "OBE_NEXT";

        public const string SourceEntry = "ENTRY";
        public const string SourceUpload = "UPLOAD";
        public const string SourceAlteration = "ALTERATION";
    }
}
