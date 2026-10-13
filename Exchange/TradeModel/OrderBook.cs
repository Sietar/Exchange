using Exchange.Models;

namespace Exchange.TradeModel;

public class OrderBook
{
    private readonly SortedDictionary<decimal, LinkedList<Order>> _buyOrders = new(Comparer<decimal>.Create((x, y) => y.CompareTo(x)));
    private readonly SortedDictionary<decimal, LinkedList<Order>> _sellOrders = new();

    public decimal? BestBid => _buyOrders.Count > 0 ? _buyOrders.Keys.First() : null;
    public decimal? BestAsk => _sellOrders.Count > 0 ? _sellOrders.Keys.First() : null;
    
    public void AddOrder(Order order)
    {
        SortedDictionary<decimal, LinkedList<Order>>  dictionary = GetDictionary(order.Side);

        if (!dictionary.TryGetValue(order.Price, out var orderList))
        {
            orderList = new LinkedList<Order>();
            dictionary.Add(order.Price, orderList);
        }

        orderList.AddLast(order);
    }

    public IReadOnlyCollection<Order> this[Side side, decimal price] => GetDictionary(side).TryGetValue(price, out var orders) ? 
        orders :
        Array.Empty<Order>();

    private SortedDictionary<decimal, LinkedList<Order>> GetDictionary(Side side)
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
        var bestList = dictionary.Values.First();
        return bestList.First();
    }

    public void RemoveBest(Side side)
    {
        var dictionary = GetDictionary(side);
        if (dictionary.Count == 0) throw new InvalidOperationException("Empty order book side");
        var bestLevel = dictionary.First();

        bestLevel.Value.RemoveFirst();
        
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
    
    public bool RemoveOrder(Order order)
    {
        var dictionary = GetDictionary(order.Side);
        if (dictionary.TryGetValue(order.Price, out var priceLevel))
        {
            var result = priceLevel.Remove(order);
            if (priceLevel.Count == 0) dictionary.Remove(order.Price);
            return result;
        }
        return false;
    }
}