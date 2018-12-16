using System;
using System.Collections.Generic;

namespace Model
{
    /// <summary>
    /// Represents order of sell/purchase of certain amount of shares at fixed max/min price per unit.
    /// </summary>
    public class Order
    {
        public Guid OrderId { get; set; }

        public string StockSymbol { get; set; }

        public Guid SubmitterId { get; set; }

        public string SubmitterName { get; set; }

        public int Quantity { get; set; }

        public OrderType OrderType { get; set; }

        public decimal PricePerUnit { get; set; }

        public DateTimeOffset Date { get; set; }

        public IEnumerable<Guid> LockedBy { get; set; }
    }
}
