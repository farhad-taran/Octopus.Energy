using Core.Display;
using Core.Persistence;
using Core.Services;
using FluentAssertions;
using Moq;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Core.Tests.Services
{
    public class OrdersServiceTests
    {
        private readonly OrdersService sut;
        private readonly Mock<IOrderStore> orderStoreMock;
        private readonly Mock<IOrderWriter> orderWriterMock;

        public OrdersServiceTests()
        {
            this.orderStoreMock = new Mock<IOrderStore>();
            this.orderWriterMock = new Mock<IOrderWriter>();

            this.sut = new OrdersService(this.orderStoreMock.Object, this.orderWriterMock.Object);
        }

        [Fact]
        public void WriteOutSmallOrders_WhenNoSmallOrdersExists_WritesNothing()
        {
            //Arrange
            var orders = new List<Order>
            {
                new Order { Size = 8},
                new Order { Size = 9},
                new Order { Size = 10}
            };

            this.orderStoreMock.Setup(x => x.GetOrders()).Returns(orders);

            IEnumerable<Order> writtenOrders = null;
            this.orderWriterMock.Setup(x => x.WriteOrders(It.IsAny<IEnumerable<Order>>()))
                .Callback<IEnumerable<Order>>(x => writtenOrders = x);

            //Act
            this.sut.WriteOutSmallOrders();

            //Assert
            writtenOrders.Should().NotBeNull();
            writtenOrders.Should().BeEmpty();
        }

        [Fact]
        public void WriteOutSmallOrders_WhenSomeSmallOrdersExists_WritesSmallOrdersOrderedByPrice()
        {
            //Arrange
            var orders = new List<Order>
            {
                new Order { Size = 10},
                new Order { Size = 12, Price = 12, Symbol = "12"},
                new Order { Size = 11, Price = 11, Symbol = "11"}
            };

            this.orderStoreMock.Setup(orderStore => orderStore.GetOrders()).Returns(orders);

            IEnumerable<Order> writtenOrders = null;
            this.orderWriterMock.Setup(orderWriter => orderWriter.WriteOrders(It.IsAny<IEnumerable<Order>>()))
                .Callback<IEnumerable<Order>>(orders => writtenOrders = orders);

            //Act
            this.sut.WriteOutSmallOrders();

            //Assert
            writtenOrders.Count().Should().Be(2);
            writtenOrders.First().Size.Should().Be(11);
            writtenOrders.First().Price.Should().Be(11);
            writtenOrders.First().Symbol.Should().Be("11");
            writtenOrders.Last().Size.Should().Be(12);
            writtenOrders.Last().Price.Should().Be(12);
            writtenOrders.Last().Symbol.Should().Be("12");
        }

        [Fact]
        public void WriteOutLargeOrders_WhenNoLargeOrdersExists_WritesNothing()
        {
            //Arrange
            var orders = new List<Order>
            {
                new Order { Size = 98},
                new Order { Size = 99},
                new Order { Size = 100}
            };

            this.orderStoreMock.Setup(orderStore => orderStore.GetOrders()).Returns(orders);

            IEnumerable<Order> writtenOrders = null;
            this.orderWriterMock.Setup(orderWriter => orderWriter.WriteOrders(It.IsAny<IEnumerable<Order>>()))
                .Callback<IEnumerable<Order>>(orders => writtenOrders = orders);

            //Act
            this.sut.WriteOutLargeOrders();

            //Assert
            writtenOrders.Should().NotBeNull();
            writtenOrders.Should().BeEmpty();
        }

        [Fact]
        public void WriteOutLargeOrders_WhenSomeLargeOrdersExists_WritesLargeOrdersOrderedByPrice()
        {
            //Arrange
            var orders = new List<Order>
            {
                new Order { Size = 100},
                new Order { Size = 102, Price = 102, Symbol = "102"},
                new Order { Size = 101, Price = 101, Symbol = "101"},
            };

            this.orderStoreMock.Setup(orderStore => orderStore.GetOrders()).Returns(orders);

            IEnumerable<Order> writtenOrders = null;
            this.orderWriterMock.Setup(orderWriter => orderWriter.WriteOrders(It.IsAny<IEnumerable<Order>>()))
                .Callback<IEnumerable<Order>>(orders => writtenOrders = orders);

            //Act
            this.sut.WriteOutLargeOrders();

            //Assert
            writtenOrders.Count().Should().Be(2);
            writtenOrders.First().Size.Should().Be(101);
            writtenOrders.First().Price.Should().Be(101);
            writtenOrders.First().Symbol.Should().Be("101");
            writtenOrders.Last().Size.Should().Be(102);
            writtenOrders.Last().Price.Should().Be(102);
            writtenOrders.Last().Symbol.Should().Be("102");
        }
    }
}
