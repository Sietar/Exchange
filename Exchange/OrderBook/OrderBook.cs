using Exchange.Models;

namespace Exchange.OrderBook;

public class OrderBook
{
    SortedDictionary<decimal, Queue<Order>> BuyOrders = new SortedDictionary<decimal, Queue<Order>>();
    
}