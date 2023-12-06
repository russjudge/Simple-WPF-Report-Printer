using System.Windows;
using System.Windows.Media;

namespace RussJudge.SimpleWPFReportPrinter
{

    /// <summary>
    /// Allows custom drawing of headers and footers
    /// </summary>
    /// <param name="context">This is the drawing context</param>
    /// <param name="definition">The report layout definition.  Use this for access to ReportTime, PageCount, and GetText().</param>
    /// <param name="bounds">The bounds of the header or footer which drawing should be within.</param>
    /// <param name="pageNumber">The zero-based page nunber</param>
    public delegate void DrawCustomHeaderOrFooter(DrawingContext context, Rect bounds, int pageNumber);

    /// <summary>
    /// Report page layout definition.
    /// </summary>
    public class ReportPageDefinition
    {
        /// <summary>
        /// The text to use that marks where the page number goes.
        /// </summary>
        public const string PageNumberSubstitution = "{PAGENUMBER}";

        /// <summary>
        /// The text for marking where the total number of pages goes.
        /// </summary>
        public const string TotalPagesSubstitution = "{PAGECOUNT}";



        ///<summary>
        /// Should table headers automatically repeat?
        ///</summary>
        public bool RepeatTableHeaders { get; set; } = true;

        /// <summary>
        /// The total pages of the report
        /// </summary>
        public int PageCount { get; internal set; } = 0;


        /// <summary>
        /// The Date/Time the report was generated.
        /// </summary>
        public DateTime ReportTime { get; private set; } = DateTime.Now;

        /// <summary>
        /// Draw custom header.  If drawing text only, do not use this.
        /// </summary>
        /// <remarks>
        /// This is useful for drawing images or other special items in the header.
        /// </remarks>
        public DrawCustomHeaderOrFooter? DrawHeader { get; set; } = null;

        /// <summary>
        /// Draw custom footer.  If drawing text only, do not use this.
        /// </summary>
        /// <remarks>
        /// This is useful for drawing images or other special items in the footer.
        /// </remarks>
        public DrawCustomHeaderOrFooter? DrawFooter { get; set; } = null;

        internal Size ContentSize { get; private set; }
        internal Point ContentOrigin { get; private set; }
        internal Size PageSize { get; set; }

        private Thickness Margins;
        private double HeaderHeight = 0;
        private double FooterHeight = 0;
        private Rect HeaderRect;
        private Rect FooterRect;

        private readonly List<ReportHeaderFooterDataLine> headerLines = [];
        private readonly List<ReportHeaderFooterDataLine> footerLines = [];
        private ReportPageDefinition() { }


        /// <summary>
        /// Initializes a Page layout definition for the report to print.
        /// </summary>
        /// <param name="pageWidth">The page width</param>
        /// <param name="pageHeight">The page height</param>
        /// <param name="left">The Left margin.  Defaults to 1 inch.</param>
        /// <param name="top">The Top Margin.  Defaults to 1 inch.</param>
        /// <param name="right">The right margin.  Defaults to 1 inch.</param>
        /// <param name="bottom">The bottom margin.  Defaults to 1 inch.</param>
        public ReportPageDefinition(
            double pageWidth, double pageHeight,
            double left = 96, double top = 96, double right = 96, double bottom = 96)
        {
            ReportTime = DateTime.Now;
            PageSize = new Size(pageWidth, pageHeight);
            Margins = new Thickness(left, top, right, bottom);

            ContentSize = new Size(PageSize.Width - (Margins.Left + Margins.Right), PageSize.Height - (Margins.Top + Margins.Bottom + HeaderHeight + FooterHeight));

            ContentOrigin = new Point(Margins.Left, Margins.Top + HeaderRect.Height);
            HeaderRect = new Rect(
                    Margins.Left, Margins.Top,
                    ContentSize.Width, HeaderHeight
                );
            FooterRect = new Rect(
                   Margins.Left, ContentOrigin.Y + ContentSize.Height,
                   ContentSize.Width, FooterHeight
               );

        }

        private void DrawText(DrawingContext context, ReportHeaderFooterDataLine line, int pageNumber)
        {
            if (line.LeftAlignText != null)
            {
                var section = GetText(line.LeftAlignText.Face, line.LeftAlignText.FontSize, line.LeftAlignText.Text, pageNumber);
                context.DrawText(section, new Point(HeaderRect.Left, line.TopOffset));
            }
            if (line.CenterAlignText != null)
            {
                var section = GetText(line.CenterAlignText.Face, line.CenterAlignText.FontSize, line.CenterAlignText.Text, pageNumber);
                double center = PageSize.Width / 2;

                double textCenter = section.Width / 2;

                double leftPoint = center - textCenter;
                context.DrawText(section, new Point(leftPoint, line.TopOffset));
            }
            if (line.RightAlignText != null)
            {
                var section = GetText(line.RightAlignText.Face, line.RightAlignText.FontSize, line.RightAlignText.Text, pageNumber);
                double leftPoint = HeaderRect.X + HeaderRect.Width - section.Width;
                context.DrawText(section, new Point(leftPoint, line.TopOffset));
            }
        }
        private DrawingVisual CreateHeaderOrFooterSection(int pageNumber, IEnumerable<ReportHeaderFooterDataLine> section, bool isForHeader)
        {
            DrawingVisual visual = new();
            using (DrawingContext context = visual.RenderOpen())
            {
                foreach (var line in section)
                {
                    DrawText(context, line, pageNumber);
                }
                if (isForHeader)
                {
                    DrawHeader?.Invoke(context, HeaderRect, pageNumber);
                }
                else
                {
                    DrawFooter?.Invoke(context, FooterRect, pageNumber);
                }
            }
            return visual;
        }


        private double GetLineHeight(ReportHeaderFooterDataLine dataLine)
        {
            double retVal = 0;
            if (dataLine.LeftAlignText != null)
            {
                var testLine = GetText(dataLine.LeftAlignText.Face, dataLine.LeftAlignText.FontSize, dataLine.LeftAlignText.Text, 0);
                retVal = testLine.LineHeight + testLine.Height;
            }
            if (dataLine.CenterAlignText != null)
            {
                var testLine = GetText(dataLine.CenterAlignText.Face, dataLine.CenterAlignText.FontSize, dataLine.CenterAlignText.Text, 0);
                var val = testLine.LineHeight + testLine.Height;
                if (val > retVal)
                {
                    retVal = val;
                }
            }
            if (dataLine.RightAlignText != null)
            {
                var testLine = GetText(dataLine.RightAlignText.Face, dataLine.RightAlignText.FontSize, dataLine.RightAlignText.Text, 0);
                var val = testLine.LineHeight + testLine.Height;
                if (val > retVal)
                {
                    retVal = val;
                }
            }
            return retVal;
        }
        private void UpdateContentSize()
        {
            ContentSize = new Size(PageSize.Width - (Margins.Left + Margins.Right), PageSize.Height - (Margins.Top + Margins.Bottom + HeaderHeight + FooterHeight));
        }
        internal Visual CreateHeader(int pageNumber)
        {
            return CreateHeaderOrFooterSection(pageNumber, headerLines, true);
        }
        internal Visual CreateFooter(int pageNumber)
        {
            return CreateHeaderOrFooterSection(pageNumber, footerLines, false);
        }

        /// <summary>
        /// Adds one line of layout specification to the header.
        /// </summary>
        /// <param name="headerLine">The header line layout definition.</param>
        public void AddHeaderLine(ReportHeaderFooterDataLine headerLine)
        {
            headerLines.Add(headerLine);
            headerLine.TopOffset = HeaderRect.Top + HeaderHeight;
            HeaderHeight += GetLineHeight(headerLine);
            UpdateContentSize();
            HeaderRect = new Rect(
                    Margins.Left, Margins.Top,
                    ContentSize.Width, HeaderHeight
                );
            ContentOrigin = new Point(Margins.Left, Margins.Top + HeaderRect.Height);
        }
        /// <summary>
        /// Adds one line of layout specification to the footer.
        /// </summary>
        /// <param name="footerLine">The footer line layout definition.</param>
        public void AddFooterLine(ReportHeaderFooterDataLine footerLine)
        {
            footerLines.Add(footerLine);
            footerLine.TopOffset = FooterRect.Top + FooterHeight;
            FooterHeight += GetLineHeight(footerLine);
            UpdateContentSize();
            FooterRect = new Rect(
                    Margins.Left, ContentOrigin.Y + ContentSize.Height,
                    ContentSize.Width, FooterHeight
                );
        }
        /// <summary>
        /// Useful for custom drawing into Header/footer, this generates FormattedText.
        /// </summary>
        /// <param name="typeface">Typeface for the text</param>
        /// <param name="fontSize">Font size for the text</param>
        /// <param name="text">The text to render</param>
        /// <param name="pageNumber">The page number being rendered.  If not supplying Page Number substitution "{PAGENUMBER}" in the text, this value can be anything.</param>
        /// <returns>The FormattedText to be drawn</returns>
        public FormattedText GetText(Typeface typeface, double fontSize, string text, int pageNumber = 0)
        {
            string txt = text
                .Replace(PageNumberSubstitution, (pageNumber + 1).ToString())
                .Replace(TotalPagesSubstitution, PageCount.ToString());

            FormattedText formattedText = new(txt,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface, fontSize, Brushes.Black,
                1);
            formattedText.SetFontWeight(typeface.Weight);
            formattedText.SetForegroundBrush(Brushes.Black);
            return formattedText;
        }
    }
}
