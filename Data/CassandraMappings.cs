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
                .Column(o => o.OrderTypeEnum, cm => cm.Ignore());
            
            this.For<Transaction>()
                .TableName("transactions");
        }
    }
}
