using System.Runtime.CompilerServices;

namespace Exchange.Models;

public class Order
{
    private static int nextId = 0;
    public int Id { get; init; }
    public int TraderId { get; init; }
    public Side Side { get; init; }
    public decimal Price { get; init; }
    public decimal Size { get; init; }
    public decimal Filled { get; private set; }
    public decimal Remaining => Size - Filled;
    public DateTime CreatedAt { get; init; }
    public OrderStatus Status { get; init; }

    public Order(Trader trader, Side side, decimal price, decimal size)
    {
        if (price <= 0 || size <= 0)
        {
            throw new ArgumentException("Price or size must be greater than zero");
        }
        Id = nextId;
        nextId++;
        TraderId = trader.Id;
        Side = side;
        Price = price;
        Size = size;
        Filled = 0m;
        CreatedAt = DateTime.UtcNow;
        Status = OrderStatus.Open;
    }

}