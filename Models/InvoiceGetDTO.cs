namespace PartyProductWebApi.Models
{
    public class InvoiceGetDTO
    {
        public int InvoiceId { get; set; }
        public string PartyName { get; set; }
        public string ProductName { get; set; }
        public int CurrentRate { get; set; }
        public int Quantity { get; set; }
        public string Date { get; set; }
    }

}
