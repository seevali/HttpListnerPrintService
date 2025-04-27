using System;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Printing;
using System.Runtime.InteropServices;

namespace HttpListenerPrintService
{
    public class PrinterHelper
    {
        public static string GetDefaultPrinter()
        {
            PrinterSettings settings = new PrinterSettings();
            return settings.PrinterName;
        }

        public static void PrintPdf(string filePath)
        {
            string defaultPrinter = GetDefaultPrinter();

            if (IsFilePrinter(defaultPrinter))
            {
                throw new InvalidOperationException("File printers are not allowed.");
            }

            PrintDocument printDocument = new PrintDocument();
            printDocument.PrinterSettings.PrinterName = defaultPrinter;
            printDocument.PrintPage += (sender, e) =>
            {
                using (var pdfDocument = PdfiumViewer.PdfDocument.Load(filePath))
                {
                    var page = pdfDocument.Render(0, (int)e.PageBounds.Width, (int)e.PageBounds.Height, true);
                    e.Graphics.DrawImage(page, e.PageBounds);
                }
            };
            printDocument.Print();
        }

        private static bool IsFilePrinter(string printerName)
        {
            string[] filePrinters = { "Microsoft Print to PDF", "Microsoft XPS Document Writer", "Fax" };
            return filePrinters.Contains(printerName);
        }
    }
}
