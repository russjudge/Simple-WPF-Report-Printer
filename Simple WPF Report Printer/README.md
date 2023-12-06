# Simple WPF Report Printer

This library provides a mechanism for printing a report with repeating headers through WPF from a FlowDocument.

I needed to print a simple report from my WPF application, but all my searches could only find instructions and examples that 
assumed a single page document with no need for headers that repeat on each page printed.  I could only find one set of code
that would print repeating headers, but it provided no sample application to demonstrate how to use the code, so was nearly
worthless.

This library solves that problem.

In addition to providing a simplified way of generating headers and footers to a report printing through WPF, the library
assumes you are printing a table and detects the column headers and prints these headers on all pages the table prints on.

## How to use

An example to use this library can be found at on the project's repository page at https://github.com/russjudge/Simple-WPF-Report-Printer

Start with creating your report as a FlowDocument.  Create a Table, define your columns, add RowGroups with the first containing your table headers.

Use the System.Windows.Controls.PrintDialog object to allow the user to select the printer.

Create a ReportPageDefinition, add header and footer lines.

Use PrintDocument from the PrintDialog, passing a new ReportPaginator to print the report.

## License

[MIT](https://choosealicense.com/licenses/mit/)


