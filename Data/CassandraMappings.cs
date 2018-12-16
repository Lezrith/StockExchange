using System;
using System.Collections.Generic;
using System.Text;
using Cassandra.Mapping;
using Model;

namespace Data
{
    public class CassandraMappings : Mappings
    {
        public CassandraMappings()
        {
            this.For<Order>()
                .TableName("orders")
                .KeyspaceName("stock_exchange")
                .PartitionKey("stockSymbol")
                .ClusteringKey("orderId")
                .Column(o => o.OrderType, cm => cm.WithDbType<int>());

            this.For<Transaction>()
                .KeyspaceName("stock_exchange")
                .TableName("transactions")
                .PartitionKey("stockSymbol")
                .ClusteringKey("date");
        }
    }
}
