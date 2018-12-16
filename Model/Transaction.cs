using System;

namespace Model
{
    /// <summary>
    /// Represents a purchase and sale agreement between two parties.
    /// </summary>
    public class Transaction
    {
        public static Transaction FromOrders(Order purchase, Order sale)
        {
            if (purchase.StockSymbol != sale.StockSymbol)
            {
                throw new ArgumentException($"Stock symbols for orders do not match: ${purchase.StockSymbol} and ${sale.StockSymbol}");
            }
            if (purchase.OrderType != OrderType.Purchase)
            {
                throw new ArgumentException($"Purchase order has invalid type: ${purchase.OrderType}");
            }
            if (sale.OrderType != OrderType.Sale)
            {
                throw new ArgumentException($"Sale order has invalid type: ${sale.OrderType}");
            }

            return new Transaction
            {
                BuyerId = purchase.SubmitterId,
                BuyerName = purchase.SubmitterName,
                Date = DateTime.Now,
                PricePerUnit = (purchase.PricePerUnit + sale.PricePerUnit) / 2,
                Quantity = Math.Min(purchase.Quantity, sale.Quantity),
                SellerId = sale.SubmitterId,
                SellerName = sale.SubmitterName,
                StockSymbol = purchase.StockSymbol,
                TransactionId = Guid.NewGuid(),
            };
        }

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
