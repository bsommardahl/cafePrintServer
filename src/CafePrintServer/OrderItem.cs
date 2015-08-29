namespace CafePrintServer
{
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