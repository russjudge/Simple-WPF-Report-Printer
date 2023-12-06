using RussJudge.SimpleWPFReportPrinter;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Example
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnPrint(object sender, RoutedEventArgs e)
        {
            GenerateReport();
        }
        private List<string> GenerateList()
        {
            List<string> lines = [];
            var words = Properties.Resources.Text.Split(' ');
            for (int j = 0; j < 10; j++)
            {
                for (int i = 0; i < words.Length - 1; i += 2)
                {
                    lines.Add(words[i] + " " + words[i + 1]);
                }
            }
            return lines;
        }

        private FlowDocument CreateFlowDocument()
        {
            var lines = GenerateList();
            FlowDocument doc = new();

            Table table = new()
            {
                TextAlignment = TextAlignment.Justify
            };
            doc.Blocks.Add(table);

            table.Columns.Add(new());
            table.Columns.Add(new());

            table.RowGroups.Add(GetGroupHeader());
            table.RowGroups.Add(GetGroupBody(lines));
            table.RowGroups.Add(GetGroupSummary(lines.Count));
            return doc;
        }
        private static TableRowGroup GetGroupHeader()
        {
            // The background from the rowGroupHeader will only show on the first page
            // and does not carry through to subsequent pages.  I tried to fix this
            // in the code, and had it working for one report I generated, but it caused
            // other more serious problems in this sample in that it simply repeated
            // the first page.  Therefore the best way to ensure the background is set
            // for all pages, set it on the individual cells.

            // If you do not need to repeat the row headers on each page, then you can
            // ignore the above and set "RepeatTableHeaders" in the ReportPageDefinition
            // to False.

            // The example below is to demonstrate setting the background
            // in the TableRowGroup, but it only showing on the first page and not
            // carrying forward to subsequent pages (therefore something to NOT do).

            // Cause of this issue is 

            TableRowGroup rowGroupHeader = new()
            {
                Background = Brushes.Maroon,  //DON'T set the background here.
                FontWeight = FontWeights.Bold,
                FontSize = 12
            };

            TableRow row = new();
            rowGroupHeader.Rows.Add(row);

            Paragraph para = new()
            {
                Background = Brushes.LightGray
            };

            para.Inlines.Add("Row Number");

            TableCell cell = new(para);
            row.Cells.Add(cell);

            para = new()
            {
                Background = Brushes.LightGray
            };
            para.Inlines.Add("Some Text");

            cell = new(para);

            row.Cells.Add(cell);

            return rowGroupHeader;
        }
        private static TableRowGroup GetGroupBody(List<string> lines)
        {
            TableRowGroup rowGroupBody = new()
            {
                FontSize = 10
            };
            int i = 0;

            foreach (var line in lines)
            {
                i++;
                TableRow row = new();
                rowGroupBody.Rows.Add(row);

                Paragraph para = new();
                para.Inlines.Add(i.ToString());

                TableCell cell = new(para);
                row.Cells.Add(cell);

                para = new();
                para.Inlines.Add(line);

                cell = new(para);
                row.Cells.Add(cell);

            }
            return rowGroupBody;
        }
        private static TableRowGroup GetGroupSummary(int count)
        {
            TableRowGroup rowGroupSummary = new()
            {
                FontSize = 12,
                FontStyle = FontStyles.Italic
            };
            TableRow row = new();
            rowGroupSummary.Rows.Add(row);

            Paragraph para = new();
            para.Inlines.Add(new Run("Total Rows:"));
            para.Inlines.Add(new Run(count.ToString()));

            TableCell cell = new(para);

            row.Cells.Add(cell);
            cell.ColumnSpan = 2;

            return rowGroupSummary;
        }
        private void GenerateReport()
        {
            // Call PrintDocument method to send document to printer
            PrintDialog printDlg = new();
            if (printDlg.ShowDialog().GetValueOrDefault())
            {

                //Build the typefaces that will be used in the Header and Footer
                Typeface tpBold = new(new("Times New"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);
                Typeface tpNormal = new(new("Times New"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

                ReportPageDefinition def = new(printDlg.PrintableAreaWidth, printDlg.PrintableAreaHeight);

                def.AddHeaderLine(new(
                    new(DateTime.Now.ToString(), tpNormal, 10),
                    null,
                    new(string.Format("Page {0}", ReportPageDefinition.PageNumberSubstitution), tpNormal, 10)));


                def.AddHeaderLine(new(new("Sample Simple WPF Report Printer", tpBold, 18)));

                def.AddFooterLine(new(
                    null,
                    null,
                    new(string.Format("Page {0} of {1}", ReportPageDefinition.PageNumberSubstitution, ReportPageDefinition.TotalPagesSubstitution), tpNormal, 10))
                    );

                var myLogo = new BitmapImage(new Uri("pack://application:,,,/Resources/rjicon.png", UriKind.RelativeOrAbsolute));

                //Use of "DrawHeader" is completely optional.  It is useful for drawing custom things like images or ellipses or whatever.
                //   if you are doing text only, "DrawHeader" is pointless.
                def.DrawHeader = (context, bounds, pageNumber) =>
                {
                    context.DrawImage(myLogo, new(bounds.Left + 110, bounds.Top, 24, 24));
                };


                //Use of "DrawFooter" is completely optional.  It is useful for drawing custom things like images or ellipses or whatever.
                //   if you are doing text only, "DrawFooter" is pointless.
                def.DrawFooter = (context, bounds, pageNumber) =>
                {
                    context.DrawImage(myLogo, new(bounds.Left, bounds.Top, 24, 24));
                };

                printDlg.PrintDocument(
                     new ReportPaginator(CreateFlowDocument(), def),
                    "Sample Print-Simple WPF Report Printer");
                MessageBox.Show("Printing complete.");
            }
        }
    }
}