using Core.Persistence;
using System.Collections.Generic;

namespace Core.Display
{
    public interface IOrderWriter
    {
        void WriteOrders(IEnumerable<Order> orders);
    }
}
