using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Reflection;
using Nancy;
using Nancy.ModelBinding;
using log4net;

namespace CafePrintServer
{
    public class ReceiptController : NancyModule
    {
        static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        string _fontName;
        int _fontSize;
        int _lineHeight;

        public ReceiptController()
        {
            Post["/receipts"] = _ =>
                                    {
                                        try
                                        {
                                            Console.WriteLine("Request received.");
                                            var order = this.Bind<Order>();
                                            Console.WriteLine("Order retrieved from request.");
                                            PrintOrderReceipt(order);
                                            return Response;
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine(ex.Message);
                                            return Response.AsJson(new {message = ex.Message},
                                                                   HttpStatusCode.InternalServerError);
                                        }
                                    };
        }

        void PrintOrderReceipt(Order order)
        {
            string printerName = ConfigurationManager.AppSettings["PrinterName"];
            int leftMarginInHundredthsOfAnInch = Convert.ToInt32(ConfigurationManager.AppSettings["LeftMarginInHundredthsOfAnInch"] ?? "0");
            int topMarginInHundredthsOfAnInch = Convert.ToInt32(ConfigurationManager.AppSettings["TopMarginInHundredthsOfAnInch"] ?? "0");
            int paperWidthInHundredthsOfAnInch =
                Convert.ToInt32(ConfigurationManager.AppSettings["PaperWidthInHundredthsOfAnInch"] ?? "0");

            var printDoc = new PrintDocument
                               {
                                   DefaultPageSettings =
                                       {
                                           Landscape = false,
                                           Margins =
                                               {
                                                   Left = leftMarginInHundredthsOfAnInch,
                                                   Top = topMarginInHundredthsOfAnInch
                                               },
                                           PaperSize = new PaperSize {Width = paperWidthInHundredthsOfAnInch}
                                       },
                                   DocumentName = order._id
                               };

            if (!string.IsNullOrEmpty(printerName))
            {
                printDoc.PrinterSettings = new PrinterSettings
                                               {
                                                   PrinterName = printerName
                                               };
            }

            printDoc.PrintPage += (sender, e) =>
                                      {
                                          Console.WriteLine("Printing started...");
                                          Log.Info("Printing started...");
                                          PrintReceipt(e, order);
                                      };

            printDoc.EndPrint += (sender, args) =>
                                     {
                                         string message = string.Format("Receipt printed for {0} on {1} at {2}.",
                                                                        order.CustomerName,
                                                                        DateTime.Now.ToShortDateString(),
                                                                        DateTime.Now.ToShortTimeString());
                                         Console.WriteLine(message);
                                         Log.Info(message);
                                     };

            Console.WriteLine("Requesting print....");
            printDoc.Print(); //start the print
        }

        string FormatLempira(object val)
        {
            string num = "L " + string.Format("{0:0.00}", val);
            return string.Format("{0,10}", num);
        }

        void PrintReceipt(PrintPageEventArgs e, Order order)
        {
            Console.Write(".");
            Graphics g = e.Graphics;

            _fontSize = Convert.ToInt32(ConfigurationManager.AppSettings["FontSize"]);
            _fontName = ConfigurationManager.AppSettings["FontName"];

            Console.Write(".");
            int x1 = e.MarginBounds.Left;
            int y1 = e.MarginBounds.Top;

            _lineHeight = Convert.ToInt32(Math.Round(_fontSize*1.5));

            Console.Write(".");
            var centering = new StringFormat
                                {LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center};

            var titleFont = new Font(_fontName, Convert.ToInt32(_fontSize*1.5), FontStyle.Bold);
            var headerPrinter = new LinePrinter(g, titleFont, Brushes.Black, x1, y1, _lineHeight,
                                                centering);
            Console.Write(".");
            headerPrinter.Print("Café El Gringo");

            var mainFont = new Font(_fontName, _fontSize);
            var subHeaderPrinter = new LinePrinter(g, mainFont, Brushes.Black, x1, headerPrinter.Top, _lineHeight,
                                                   centering);

            Console.Write(".");
            subHeaderPrinter.Print("RTN: 08221886000084");
            subHeaderPrinter.Print("Barrio el Centro");
            subHeaderPrinter.Print("Santa Ana, FM");
            subHeaderPrinter.Print("Tel 504 9754-5002");
            subHeaderPrinter.Print("");
            subHeaderPrinter.Print("FACTURA");
            subHeaderPrinter.Print("");

            Console.Write(".");
            var tablePrinter = new LinePrinter(g, mainFont, Brushes.Black, x1, subHeaderPrinter.Top, _lineHeight, centering);
            tablePrinter.Print("----------------------------------------");
            tablePrinter.Print("Cant.  | Descripcion           |   Valor");
            tablePrinter.Print("----------------------------------------");
            Console.Write(".");
            IEnumerable<IGrouping<string, OrderItem>> itemGroups = order.Items.GroupBy(x => x.Name);
            foreach (var orderItem in itemGroups)
            {
                string cant = string.Format("{0}x{1}", orderItem.Count(), orderItem.First().Price);
                double valor = orderItem.First().Price*orderItem.Count();
                tablePrinter.Print("{0,-6} | {1,-21} | {2,7}", cant, orderItem.First().Name, "L " + valor);
                Console.Write(".");
            }
            tablePrinter.Print("----------------------------------------");
            tablePrinter.Print("                   Sub Total: {0}", FormatLempira(order.Items.Sum(x => x.Price)));
            tablePrinter.Print("                   Descuento: {0}", FormatLempira(0));
            tablePrinter.Print("                   ISV:       {0}", FormatLempira(order.TaxPaid));
            tablePrinter.Print("                   Total:     {0}", FormatLempira(order.AmountPaid));
            tablePrinter.Print("");
            Console.Write(".");
            tablePrinter.Print("                   Effectivo: {0}", FormatLempira(order.AmountPaid));
            tablePrinter.Print("                   Cambio:    {0}", FormatLempira(order.AmountPaid));
            tablePrinter.Print("");

            var infoPrinter = new LinePrinter(g, mainFont, Brushes.Black, x1, tablePrinter.Top, _lineHeight, centering);
            infoPrinter.Print("");
            infoPrinter.Print("Factura {0}", order._id);
            infoPrinter.Print("Condición {0}", "CONTADO");
            infoPrinter.Print("Fecha {0}/{1}/{2} - Hora {3}", order.Paid.Day, order.Paid.Month, order.Paid.Year,
                              order.Paid.ToShortTimeString());
            Console.Write(".");
            infoPrinter.Print("Cajero {0} - Caja #{1}", "Pamela M", 1);
            infoPrinter.Print("");

            var footerFont = new Font(_fontName, _fontSize, FontStyle.Italic);
            var footerPrinter = new LinePrinter(g, footerFont, Brushes.Black, x1, infoPrinter.Top, _lineHeight,
                                                centering);

            footerPrinter.Print("");
            footerPrinter.Print("Gracias por su compra.");
            Console.Write(".");
            footerPrinter.Print("La factura es benefició de todos: Exijala!");
            footerPrinter.Print("No se aceptan cambios ni devoluciones.");
            footerPrinter.Print("");
            footerPrinter.Print("www.CafeElGringo.com");

            e.HasMorePages = false; //set to true to continue printing next page
            Console.Write(".");
        }
    }
}