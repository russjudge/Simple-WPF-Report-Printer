using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace RussJudge.SimpleWPFReportPrinter
{
    /// <summary>
    /// Document paginator for reports.
    /// </summary>
    public class ReportPaginator : DocumentPaginator
    {

        private readonly DocumentPaginator Paginator;
        private readonly ReportPageDefinition Definition;

        private ContainerVisual? ColumnHeadersVisual = null;


        /// <summary>
        /// gets a value indicating whether the PageCount is the total number of pages.
        /// </summary>
        public override bool IsPageCountValid
        {
            get { return Paginator.IsPageCountValid; }
        }
        /// <summary>
        /// gets a count of the total number of pages formatted.
        /// </summary>
        public override int PageCount
        {
            get { return Paginator.PageCount; }
        }
        /// <summary>
        /// Gets or sets the width and height of each page.
        /// </summary>
        public override Size PageSize
        {
            get
            {
                return Paginator.PageSize;
            }
            set
            {
                Paginator.PageSize = value;
            }
        }
        /// <summary>
        /// Returns the element being paginated.
        /// </summary>
        public override IDocumentPaginatorSource Source
        {
            get { return Paginator.Source; }
        }
        /// <summary>
        /// Initializes a new ReportPaginator object.
        /// </summary>
        /// <param name="document">The item source to print.</param>
        /// <param name="definition">Page layout definition.</param>
        public ReportPaginator(FlowDocument document, ReportPageDefinition definition)
        {
            // Create a copy of the flow document,
            // so we can modify it without modifying
            // the original.
            using MemoryStream stream = new();
            TextRange sourceDocument = new(document.ContentStart, document.ContentEnd);
            sourceDocument.Save(stream, DataFormats.Xaml);
            FlowDocument copy = new();
            TextRange copyDocumentRange = new(copy.ContentStart, copy.ContentEnd);
            copyDocumentRange.Load(stream, DataFormats.Xaml);
            this.Paginator = ((IDocumentPaginatorSource)copy).DocumentPaginator;
            this.Definition = definition;
            Paginator.PageSize = definition.ContentSize;

            // Change page size of the document to
            // the size of the content area
            copy.ColumnWidth = double.MaxValue; // Prevent columns
            copy.PageWidth = Definition.ContentSize.Width;
            copy.PageHeight = Definition.ContentSize.Height;
            copy.PagePadding = new Thickness(0);
        }

        /// <summary>
        /// Gets the DocumentPage for the specified pageNumber.
        /// </summary>
        /// <param name="pageNumber">The zero-based page number of the documate page that is needed.</param>
        /// <returns></returns>
        public override DocumentPage GetPage(int pageNumber)
        {
            if (Definition.PageCount == 0)
            {
                Definition.PageCount = Paginator.PageCount;
            }
            DocumentPage retVal;
            // Use default paginator to handle pagination
            Visual originalPage = Paginator.GetPage(pageNumber).Visual;

            ContainerVisual visual = new();
            ContainerVisual pageVisual = new()
            {
                Transform = new TranslateTransform(
                    Definition.ContentOrigin.X,
                    Definition.ContentOrigin.Y
                )
            };
            pageVisual.Children.Add(originalPage);

            visual.Children.Add(pageVisual);

            // Create headers and footers
            var head = Definition.CreateHeader(pageNumber);

            visual.Children.Add(head);
            visual.Children.Add(Definition.CreateFooter(pageNumber));

            // Check for repeating table headers
            if (Definition.RepeatTableHeaders)
            {
                // Find table header
                if (PageStartsWithTable(originalPage, out ContainerVisual? table) && ColumnHeadersVisual != null)
                {
                    // The page starts with a table and a table header was
                    // found on the previous page. Presumably this table 
                    // was started on the previous page, so we'll repeat the
                    // table header.
                    Rect headerBounds = VisualTreeHelper.GetDescendantBounds(ColumnHeadersVisual);

                    ContainerVisual tableHeaderVisual = new()
                    {
                        // Translate the header to be at the top of the page
                        // instead of its previous position
                        Transform = new TranslateTransform(Definition.ContentOrigin.X, Definition.ContentOrigin.Y - headerBounds.Top)
                    };

                    // Since we've placed the repeated table header on top of the
                    // content area, we'll need to scale down the rest of the content
                    // to accomodate this. Since the table header is relatively small,
                    // this probably is barely noticeable.
                    double yScale = (Definition.ContentSize.Height - headerBounds.Height) / Definition.ContentSize.Height;
                    TransformGroup group = new();
                    group.Children.Add(new ScaleTransform(1.0, yScale));
                    group.Children.Add(new TranslateTransform(
                        Definition.ContentOrigin.X,
                        Definition.ContentOrigin.Y + headerBounds.Height
                    ));
                    pageVisual.Transform = group;

                    if (VisualTreeHelper.GetParent(ColumnHeadersVisual) is ContainerVisual cp)
                    {
                        cp.Children.Remove(ColumnHeadersVisual);
                    }
                    tableHeaderVisual.Children.Add(ColumnHeadersVisual);
                    visual.Children.Add(tableHeaderVisual);
                }

                // Check if there is a table on the bottom of the page.
                // If it's there, its header should be repeated
                if (PageEndsWithTable(originalPage, out ContainerVisual? newTable, out ContainerVisual? newHeader))
                {
                    if (newTable == table && ColumnHeadersVisual != null)
                    {
                        // Still the same table so don't change the repeating header
                    }
                    else
                    {
                        // We've found a new table. Repeat the header on the next page
                        ColumnHeadersVisual = newHeader;
                    }
                }
                else
                {
                    // There was no table at the end of the page
                    ColumnHeadersVisual = null;
                }

            }

            retVal = new(
                visual,
                Definition.PageSize,
                new Rect(new Point(), Definition.PageSize),
                new Rect(Definition.ContentOrigin, Definition.ContentSize)
            );

            return retVal;
        }


        /// <summary>
        /// Checks if the page ends with a table.
        /// </summary>
        /// <remarks>
        /// There is no such thing as a 'TableVisual'. There is a RowVisual, which
        /// is contained in a ParagraphVisual if it's part of a table. For our
        /// purposes, we'll consider this the table Visual
        /// 
        /// You'd think that if the last element on the page was a table row, 
        /// this would also be the last element in the visual tree, but this is not true
        /// The page ends with a ContainerVisual which is aparrently empty.
        /// Therefore, this method will only check the last child of an element
        /// unless this is a ContainerVisual
        /// </remarks>
        /// <param name="element"></param>
        /// <param name="tableVisual"></param>
        /// <param name="headerVisual"></param>
        /// <returns></returns>
        private static bool PageEndsWithTable(DependencyObject element, out ContainerVisual? tableVisual, out ContainerVisual? headerVisual)
        {

            if (element.GetType().Name == "RowVisual")
            {
                tableVisual = (ContainerVisual)VisualTreeHelper.GetParent(element);
                headerVisual = (ContainerVisual)VisualTreeHelper.GetChild(tableVisual, 0);
                return true;
            }
            int children = VisualTreeHelper.GetChildrenCount(element);
            if (element.GetType() == typeof(ContainerVisual))
            {
                for (int c = children - 1; c >= 0; c--)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(element, c);
                    if (PageEndsWithTable(child, out tableVisual, out headerVisual))
                    {
                        return true;
                    }
                }
            }
            else if (children > 0)
            {
                DependencyObject child = VisualTreeHelper.GetChild(element, children - 1);
                if (PageEndsWithTable(child, out tableVisual, out headerVisual))
                {
                    return true;
                }
            }
            tableVisual = null;
            headerVisual = null;
            return false;
        }


        private static bool PageStartsWithTable(DependencyObject element, out ContainerVisual? tableVisual)
        {
            if (element.GetType().Name == "RowVisual")
            {
                tableVisual = (ContainerVisual)VisualTreeHelper.GetParent(element);
                return true;
            }
            if (VisualTreeHelper.GetChildrenCount(element) > 0)
            {
                DependencyObject child = VisualTreeHelper.GetChild(element, 0);
                if (PageStartsWithTable(child, out tableVisual))
                {
                    return true;
                }
            }
            tableVisual = null;
            return false;
        }

    }
}
