using Exchange.Models;

namespace Exchange.TradeModel;

public class MatchingEngine
{

    private readonly OrderBook _book;
    
    private readonly Dictionary<int, Order> _activeOrders = new();

    public MatchingEngine(OrderBook book)
    {
        _book = book;
    }
    private static Side TakeOppositeSide(Side side)
    {
        return side == Side.Buy ? Side.Sell : Side.Buy;
    }

    private static bool PricesCross(Order incoming, Order saved) => incoming.Side == Side.Buy ? incoming.Price >= saved.Price : incoming.Price <= saved.Price;
    
    
    public List<Trade> Submit(Order order)
    {
        List<Trade> trades = [];
        
        var oppositeSide = TakeOppositeSide(order.Side);
        
        while (order.Remaining > 0 && _book.PeekBest(oppositeSide) is { } oppositeOrder &&
               PricesCross(order, oppositeOrder))
        {
            var amountToFill = Math.Min(oppositeOrder.Remaining, order.Remaining);
            
            order.Fill(amountToFill);
            oppositeOrder.Fill(amountToFill);

            if (oppositeOrder.Status == OrderStatus.Filled)
            {
                _book.RemoveBest(oppositeOrder.Side);
                _activeOrders.Remove(oppositeOrder.Id);
            }

            var buySell = GetBuySell(order, oppositeOrder);
            trades.Add(new Trade(buySell.buyOrderId, buySell.sellOrderId, oppositeOrder.Price, amountToFill, DateTime.UtcNow ));
        }

        if (order.Remaining > 0m)
        {
            _book.AddOrder(order);
            _activeOrders.Add(order.Id, order);
        }

        return trades;
        
        (int buyOrderId, int sellOrderId) GetBuySell(Order first, Order second) => first.Side == Side.Buy ? (first.Id,second.Id) :
                                  (second.Id, first.Id);
    }

    public bool Cancel(int orderId)
    {
        if (!_activeOrders.TryGetValue(orderId, out var order)) return false;
        
        var result = _book.RemoveOrder(order) && _activeOrders.Remove(orderId);
        if (result)
        {
            order.Cancel();
        }
        return result;
    }

    public override string ToString()
    {
        return _book.ToString();
    }
}