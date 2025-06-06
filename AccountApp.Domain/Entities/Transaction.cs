namespace AccountApp.Domain.Entities
{
    public class Transaction
    {
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EUR";
        public string Category { get; set; } = string.Empty;
    }
}