namespace RussJudge.SimpleWPFReportPrinter
{
    /// <summary>
    /// Basic header or footer line layout definition.
    /// </summary>
    public class ReportHeaderFooterDataLine
    {
        private ReportHeaderFooterDataLine() { }
        /// <summary>
        /// Initializes the ReportHeaderFooterDataLine structure, with no left or right-aligned text.
        /// </summary>
        /// <param name="centerAlign">The center-aligned layout definition.</param>
        public ReportHeaderFooterDataLine(ReportHeaderFooterTextData centerAlign)
        {
            CenterAlignText = centerAlign;
        }
        /// <summary>
        /// Initializes the ReportHeaderFooterDataLine structure
        /// </summary>
        /// <param name="leftAlign">The left-aligned layout definition.</param>
        /// <param name="centerAlign">The center-aligned layout definition.</param>
        /// <param name="rightAlign">The right-aligned layout definition.</param>
        public ReportHeaderFooterDataLine(ReportHeaderFooterTextData? leftAlign, ReportHeaderFooterTextData? centerAlign, ReportHeaderFooterTextData? rightAlign)
        {
            LeftAlignText = leftAlign;
            CenterAlignText = centerAlign;
            RightAlignText = rightAlign;
        }
        /// <summary>
        /// Gets the Left-aligned layout definition
        /// </summary>
        public ReportHeaderFooterTextData? LeftAlignText { get; private set; }
        /// <summary>
        /// Gets the center-aligned layout definition
        /// </summary>
        public ReportHeaderFooterTextData? CenterAlignText { get; private set; }
        /// <summary>
        /// Gets the right-aligned layout definition
        /// </summary>
        public ReportHeaderFooterTextData? RightAlignText { get; private set; }

        internal double TopOffset { get; set; }

    }
}
