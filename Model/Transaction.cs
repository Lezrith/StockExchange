using System;

namespace Model
{
    /// <summary>
    /// Represents a purchase and sale agreement between two parties
    /// </summary>
    public class Transaction
    {
        public Guid TransactionId { get; set; }

        public string StockSymbol { get; set; }

        public Guid BuyerId { get; set; }

        public string BuyerName { get; set; }

        public Guid SellerId { get; set; }

        public string SellerName { get; set; }

        public int Quantity { get; set; }

        public decimal PricePerUnit { get; set; }

        public DateTimeOffset Date { get; set; }
    }
}
