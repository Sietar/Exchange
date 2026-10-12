using Exchange.Models;

namespace Exchange.TradeModel;

public class OrderBook
{
    private readonly SortedDictionary<decimal, Queue<Order>> _buyOrders = new(Comparer<decimal>.Create((x, y) => y.CompareTo(x)));
    private readonly SortedDictionary<decimal, Queue<Order>> _sellOrders = new();

    public decimal? BestBid => _buyOrders.Count > 0 ? _buyOrders.Keys.First() : null;
    public decimal? BestAsk => _sellOrders.Count > 0 ? _sellOrders.Keys.First() : null;
    
    public void AddOrder(Order order)
    {
        SortedDictionary<decimal, Queue<Order>>  dictionary = GetDictionary(order.Side);

        if (!dictionary.TryGetValue(order.Price, out var orderQueue))
        {
            orderQueue = new Queue<Order>();
            dictionary.Add(order.Price, orderQueue);
        }

        orderQueue.Enqueue(order);
    }

    public IReadOnlyCollection<Order> this[Side side, decimal price] => GetDictionary(side).TryGetValue(price, out var orders) ? 
        orders :
        Array.Empty<Order>();

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

    public Order? PeekBest(Side side)
    {
        var dictionary = GetDictionary(side);
        if (dictionary.Count == 0) return null;
        var bestQueue = dictionary.Values.First();
        return bestQueue.Peek();
    }

    public void RemoveBest(Side side)
    {
        var dictionary = GetDictionary(side);
        if (dictionary.Count == 0) throw new InvalidOperationException("Empty order book side");
        var bestLevel = dictionary.First();

        bestLevel.Value.Dequeue();
        
        if (bestLevel.Value.Count == 0) dictionary.Remove(bestLevel.Key);

    }
    public override string ToString()
    {
        string result = "";

        foreach (var pos in _sellOrders.Reverse())
        {
            foreach (var order in pos.Value)
            {
                result += order.ToString() + "\n";
            }
        }

        result += "\n-------------\n";

        foreach (var pos in _buyOrders)
        {
            foreach (var order in pos.Value)
            {
                result += order.ToString() + "\n";
            }
        }
        return result;
    }
}