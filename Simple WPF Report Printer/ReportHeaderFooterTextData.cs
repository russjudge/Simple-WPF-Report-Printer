using System.Windows;
using System.Windows.Media;

namespace RussJudge.SimpleWPFReportPrinter
{
    /// <summary>
    /// Section layout definiton for a header or footer line.
    /// </summary>
    public class ReportHeaderFooterTextData
    {
        private ReportHeaderFooterTextData()
        {
            Text = "This is dummy text that should never be used";
            Face = new("Times New");
            FontSize = 10;
        }

        /// <summary>
        /// Initializes the ReportHeaderFooterTextData layout definition, using FontFamily Courier New, Normal style, normal weight, and normal FontStretch, with font size 10.
        /// </summary>
        /// <param name="text">The text to print for the section.</param>
        public ReportHeaderFooterTextData(string text)
        {
            this.Text = text;
            Face = new(new FontFamily("Courier New"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
            FontSize = 10;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="face"></param>
        /// <param name="fontSize"></param>
        public ReportHeaderFooterTextData(string text, Typeface face, double fontSize)
        {
            Text = text;
            Face = face;
            FontSize = fontSize;
        }
        public string Text { get; private set; }
        public Typeface Face { get; private set; }
        public double FontSize { get; private set; }
    }
}
