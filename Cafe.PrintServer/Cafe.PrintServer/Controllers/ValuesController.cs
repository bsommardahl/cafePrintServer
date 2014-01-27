using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Reflection;
using System.Threading;
using System.Web.Http;
using AttributeRouting.Web.Http;

namespace Cafe.PrintServer.Controllers
{
    public class ValuesController : ApiController
    {
        [POST("/receipt/print")]
        public void Print([FromBody] Order order)
        {
            var printDoc = new PrintDocument
                               {
                                   DefaultPageSettings = {Landscape = true, Margins = {Left = 100}},
                                   DocumentName = "My Document Name",
                                   PrinterSettings = {PrinterName = "Microsoft XPS Document Writer"}
                               };

            printDoc.PrintPage += printDoc_PrintPage;

            printDoc.Print(); //start the print
        }

        void printDoc_PrintPage(object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            string textToPrint = ".NET Printing is easy";
            var font = new Font("Courier New", 12);
            // e.PageBounds is total page size (does not consider margins)
            // e.MarginBounds is the portion of page inside margins
            int x1 = e.MarginBounds.Left;
            int y1 = e.MarginBounds.Top;
            int w = e.MarginBounds.Width;
            int h = e.MarginBounds.Height;

            g.DrawRectangle(Pens.Red, x1, y1, w, h);
                //draw a rectangle around the margins of the page, also we can use: g.DrawRectangle(Pens.Red, e.MarginBounds)
            g.DrawString(textToPrint, font, Brushes.Black, x1, y1);

            e.HasMorePages = false; //set to true to continue printing next page
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