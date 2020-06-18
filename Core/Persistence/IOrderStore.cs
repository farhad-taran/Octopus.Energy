using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Persistence
{
    public interface IOrderStore
    {
        List<Order> GetOrders();
    }
}
