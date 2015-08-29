using System;
using System.Collections.Generic;

namespace CafePrintServer
{
    public class Order
    {
        public double TaxPaid { get; set; }
        public string LocationId { get; set; }
        public DateTime Paid { get; set; }
        public DateTime Created { get; set; }
        public double AmountPaid { get; set; }
        public string CustomerName { get; set; }
        public bool AllDelivered { get; set; }
        public OrderItem[] Items { get; set; }
        public string _id { get; set; }
    }
}