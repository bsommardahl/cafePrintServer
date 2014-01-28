using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Web.Http;

namespace CafePrintServer
{
    public class ReceiptController : ApiController
    {
        string _fontName;
        int _fontSize;
        int _lineHeight;
        string _printerName;

        public void Post([FromBody] Order order)
        {
            _printerName = ConfigurationManager.AppSettings["PrinterName"];

            var printDoc = new PrintDocument
                               {
                                   DefaultPageSettings =
                                       {
                                           Landscape = false,
                                           Margins =
                                               {
                                                   Left = 50+(312/2),
                                                   Top = 50
                                               },
                                           PaperSize = new PaperSize {Width = 312}
                                       },
                                   DocumentName = order._id,
                                   PrinterSettings = {PrinterName = _printerName}
                               };

            printDoc.PrintPage += (sender, e) => PrintReceipt(e, order);

            printDoc.Print(); //start the print

            Console.WriteLine("Receipt printed for {0} on {1} at {2}.", order.CustomerName,
                              DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString());
        }

        string FormatLempira(object val)
        {
            string num = "L " + string.Format("{0:0.00}", val);
            return string.Format("{0,10}", num);
        }

        void PrintReceipt(PrintPageEventArgs e, Order order)
        {
            Graphics g = e.Graphics;

            _fontSize = Convert.ToInt32((string) ConfigurationManager.AppSettings["FontSize"]);
            _fontName = ConfigurationManager.AppSettings["FontName"];
            
            int x1 = e.MarginBounds.Left;
            int y1 = e.MarginBounds.Top;
            
            _lineHeight = 15;

            var centering = new StringFormat
                                {LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center};

            var titleFont = new Font(_fontName, Convert.ToInt32(_fontSize*1.5), FontStyle.Bold);
            var headerPrinter = new LinePrinter(g, titleFont, Brushes.Black, x1, y1, Convert.ToInt32(_lineHeight*1.5),
                                                centering);
            headerPrinter.Print("Café El Gringo");

            var mainFont = new Font(_fontName, _fontSize);
            var subHeaderPrinter = new LinePrinter(g, mainFont, Brushes.Black, x1, headerPrinter.Top, _lineHeight, centering);

            subHeaderPrinter.Print("RTN: 08221886000084");
            subHeaderPrinter.Print("Barrio el Centro");
            subHeaderPrinter.Print("Santa Ana, FM");
            subHeaderPrinter.Print("Tel 504 9754-5002");
            subHeaderPrinter.Print("");
            subHeaderPrinter.Print("FACTURA");
            subHeaderPrinter.Print("");
            
            var tablePrinter = new LinePrinter(g, mainFont, Brushes.Black, x1, subHeaderPrinter.Top, 13, centering);
            tablePrinter.Print("----------------------------------------");
            tablePrinter.Print("Cant.  | Descripcion           |   Valor");
            tablePrinter.Print("----------------------------------------");
            IEnumerable<IGrouping<string, OrderItem>> itemGroups = order.Items.GroupBy(x => x.Name);
            foreach (var orderItem in itemGroups)
            {
                string cant = string.Format("{0}x{1}", orderItem.Count(), orderItem.First().Price);
                double valor = orderItem.First().Price*orderItem.Count();
                tablePrinter.Print("{0,-6} | {1,-21} | {2,7}", cant, orderItem.First().Name, "L " + valor);
            }
            tablePrinter.Print("----------------------------------------");
            tablePrinter.Print("                   Sub Total: {0}", FormatLempira(order.Items.Sum(x => x.Price)));
            tablePrinter.Print("                   Descuento: {0}", FormatLempira(0));
            tablePrinter.Print("                   ISV:       {0}", FormatLempira(order.TaxPaid));
            tablePrinter.Print("                   Total:     {0}", FormatLempira(order.AmountPaid));
            tablePrinter.Print("");
            tablePrinter.Print("                   Effectivo: {0}", FormatLempira(order.AmountPaid));
            tablePrinter.Print("                   Cambio:    {0}", FormatLempira(order.AmountPaid));
            tablePrinter.Print("");

            var infoPrinter = new LinePrinter(g, mainFont, Brushes.Black, x1, tablePrinter.Top, _lineHeight, centering);
            infoPrinter.Print("");
            infoPrinter.Print("Factura {0}", order._id);
            infoPrinter.Print("Condición {0}", "CONTADO");
            infoPrinter.Print("Fecha {0}/{1}/{2} - Hora {3}", order.Paid.Day, order.Paid.Month, order.Paid.Year,
                              order.Paid.ToShortTimeString());
            infoPrinter.Print("Cajero {0} - Caja #{1}", "Pamela M", 1);
            infoPrinter.Print("");

            var footerFont = new Font(_fontName, _fontSize, FontStyle.Italic);
            var footerPrinter = new LinePrinter(g, footerFont, Brushes.Black, x1, infoPrinter.Top, _lineHeight,
                                                centering);

            footerPrinter.Print("");
            footerPrinter.Print("Gracias por su compra.");
            footerPrinter.Print("La factura es benefició de todos: Exijala!");
            footerPrinter.Print("No se aceptan cambios ni devoluciones.");
            footerPrinter.Print("");
            footerPrinter.Print("www.CafeElGringo.com");

            e.HasMorePages = false; //set to true to continue printing next page
        }
    }

    public class LinePrinter
    {
        readonly Brush _brush;
        readonly Font _font;
        readonly StringFormat _format;
        readonly Graphics _graphics;
        readonly int _left;
        readonly int _lineHeight;
        public int Top;

        public LinePrinter(Graphics graphics, Font font, Brush brush, int left, int top, int lineHeight,
                           StringFormat format)
        {
            _graphics = graphics;
            _font = font;
            _brush = brush;
            Top = top;
            _lineHeight = lineHeight;
            _format = format;
            _left = left;
        }

        public void Print(string text, params object[] args)
        {
            if (args.Length > 0)
                text = string.Format(text, args);

            _graphics.DrawString(text, _font, _brush, _left, Top, _format);
            Top += _lineHeight;
        }
    }

    public class Order
    {
        public double TaxPaid { get; set; }
        public string LocationId { get; set; }
        public DateTime Paid { get; set; }
        public DateTime Created { get; set; }
        public double AmountPaid { get; set; }
        public string CustomerName { get; set; }
        public bool AllDelivered { get; set; }
        public List<OrderItem> Items { get; set; }
        public string _id { get; set; }
    }

    public class OrderItem
    {
        public string _id { get; set; }
        public string Tag { get; set; }
        public double Price { get; set; }
        public double TaxRate { get; set; }
        public bool Delivered { get; set; }
        public string Name { get; set; }
    }
}