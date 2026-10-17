using Exchange.Models;
using Exchange.Repository;

namespace Exchange.TradeModel;

public class MatchingEngine
{

    private readonly OrderBook _book;
    private readonly IRepository<Trader> _traders;
    
    private readonly Dictionary<int, Order> _activeOrders = new();

    public MatchingEngine(OrderBook book, IRepository<Trader>  traders)
    {
        _book = book;
        _traders = traders;
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
        
        var trader = _traders.GetById(order.TraderId);
        if (trader is null) throw new ArgumentException($"Trader with id {order.TraderId} not found");

        if (order.Side == Side.Buy)
        {
            trader.MoneyBalance.Block(order.Price * order.Size);
        }
        else
        {
            trader.CurrencyBalance.Block(order.Size);
        }
        
        
        
        while (order.Remaining > 0 && _book.PeekBest(oppositeSide) is { } oppositeOrder &&
               PricesCross(order, oppositeOrder))
        {
            var oppositeTraderId = oppositeOrder.TraderId;
            var oppositeTrader =  _traders.GetById(oppositeTraderId);
            if (oppositeTrader is null) throw new ArgumentException($"Trader with id {oppositeTraderId} not found");
            
            var amountToFill = Math.Min(oppositeOrder.Remaining, order.Remaining);
            
            order.Fill(amountToFill);
            oppositeOrder.Fill(amountToFill);
            
            var traded = oppositeOrder.Price * amountToFill;
            
            var buyerSeller = GetBuyerSeller(order,  oppositeOrder);
            
            buyerSeller.buyer.MoneyBalance.DecreaseBalance(traded);
            buyerSeller.buyer.CurrencyBalance.Deposit(amountToFill);

            var amountToUnlock = order.Price * amountToFill - traded;
            if (order.Side == Side.Buy && amountToUnlock > 0m)
            {
                buyerSeller.buyer.MoneyBalance.Unblock(amountToUnlock);
            }

            buyerSeller.seller.CurrencyBalance.DecreaseBalance(amountToFill);
            buyerSeller.seller.MoneyBalance.Deposit(traded);

            if (oppositeOrder.Status == OrderStatus.Filled)
            {
                _book.RemoveBest(oppositeOrder.Side);
                _activeOrders.Remove(oppositeOrder.Id);
            }

            var buySell = GetBuySell(order, oppositeOrder);
            var trade = new Trade(buySell.buyOrderId, buySell.sellOrderId, oppositeOrder.Price, amountToFill, DateTime.UtcNow );
            trades.Add(trade);
            TradeExecuted?.Invoke(this, trade);
        }

        if (order.Remaining > 0m)
        {
            _book.AddOrder(order);
            _activeOrders.Add(order.Id, order);
            OrderPlaced?.Invoke(this, order);
        }

        return trades;
        
        (int buyOrderId, int sellOrderId) GetBuySell(Order first, Order second) => 
            first.Side == Side.Buy ? (first.Id,second.Id) : (second.Id, first.Id);

        (Trader buyer, Trader seller) GetBuyerSeller(Order first, Order second) =>
            first.Side == Side.Buy ? (_traders.GetById(first.TraderId), _traders.GetById(second.TraderId)) : 
                (_traders.GetById(second.TraderId), _traders.GetById(first.TraderId));
    }

    public bool Cancel(int orderId)
    {
        if (!_activeOrders.TryGetValue(orderId, out var order)) return false;
        
        var trader = _traders.GetById(order.TraderId);
        if (trader is null) { throw new ArgumentException($"Trader with id {order.TraderId} not found"); }
        
        var result = _book.RemoveOrder(order) && _activeOrders.Remove(orderId);
        if (!result)
        {
            return false;
        }
        
        order.Cancel();
        
        
        if (order.Side == Side.Buy)
        {
            trader.MoneyBalance.Unblock(order.Remaining * order.Price);
        }
        else
        {
            trader.CurrencyBalance.Unblock(order.Remaining);
        }
        OrderCanceled?.Invoke(this, order);
        return true;
    }

    public override string ToString()
    {
        return _book.ToString() + _traders.ToString();
    }

    public event EventHandler<Trade>? TradeExecuted;
    public event EventHandler<Order>? OrderPlaced;
    public event EventHandler<Order>? OrderCanceled;
}