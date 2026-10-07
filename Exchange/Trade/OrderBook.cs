using Exchange.Models;

namespace Exchange.OrderBook;

public class OrderBook
{
    private readonly SortedDictionary<decimal, Queue<Order>> _buyOrders = new(Comparer<decimal>.Create((x, y) => y.CompareTo(x)));
    private readonly SortedDictionary<decimal, Queue<Order>> _sellOrders = new();

    public decimal? BestBid => _buyOrders.Count > 0 ? _buyOrders.Keys.First() : null;
    public decimal? BestAsk => _sellOrders.Count > 0 ? _sellOrders.Keys.First() : null;
    
    public void Add(Order order)
    {
        SortedDictionary<decimal, Queue<Order>>  dictionary = GetDictionary(order.Side);

        if (!dictionary.TryGetValue(order.Price, out var orderQueue))
        {
            orderQueue = new Queue<Order>();
            dictionary.Add(order.Price, orderQueue);
        }

        orderQueue.Enqueue(order);
    }
    
    public Queue<Order> this[Side side, Decimal price] => side == Side.Buy ? _buyOrders[price] : _sellOrders[price];

    private SortedDictionary<decimal, Queue<Order>> GetDictionary(Side side)
    {
        if (side == Side.Buy)
        {
            return _buyOrders;
        }
        else
        {
            return _sellOrders;
        }
    }
}