using Core.Display;
using Core.Persistence;
using System;
using System.Linq;

namespace Core.Services
{
    public class OrdersService
    {
        private readonly IOrderStore orderStore;
        private readonly IOrderWriter orderWriter;

        public OrdersService(IOrderStore orderStore, IOrderWriter orderWriter)
        {
            this.orderStore = orderStore ?? throw new ArgumentException(nameof(orderStore));
            this.orderWriter = orderWriter ?? throw new ArgumentException(nameof(orderWriter));
        }

        public void WriteOutSmallOrders()
        {
            var orders = this.orderStore.GetOrders();
            var filteredOrders = orders.Where(order => order.Size > 10).OrderBy(order => order.Price);
            this.orderWriter.WriteOrders(filteredOrders);
        }

        public void WriteOutLargeOrders()
        {
            var orders = this.orderStore.GetOrders();
            var filteredOrders = orders.Where(order => order.Size > 100).OrderBy(order => order.Price);
            this.orderWriter.WriteOrders(filteredOrders);
        }
    }
}
