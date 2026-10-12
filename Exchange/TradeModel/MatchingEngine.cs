using Exchange.Models;

namespace Exchange.TradeModel;

public class MatchingEngine
{
    
    private readonly OrderBook _book = new();

    private static Side TakeOppositeSide(Side side)
    {
        return side == Side.Buy ? Side.Sell : Side.Buy;
    }

    private static bool PricesCross(Order incoming, Order saved) => incoming.Side == Side.Buy ? incoming.Price >= saved.Price : incoming.Price <= saved.Price;
    
    
    public void Submit(Order order)
    {
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
            }
        }

        if (order.Remaining > 0m)
        {
            _book.AddOrder(order);
        }
    }

    public override string ToString()
    {
        return _book.ToString();
    }
}