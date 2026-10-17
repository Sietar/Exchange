using Exchange.Repository;

namespace Exchange.Models;

public class Order : IEntity
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
    public OrderStatus Status { get; private set; }

    public Order(Trader trader, Side side, decimal price, decimal size)
    {
        if (price <= 0m || size <= 0m)
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

    public void Fill(decimal amount)
    {
        if (Status == OrderStatus.Canceled)
        { 
            throw new InvalidOperationException("Cancelled");
        }
        if (amount <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be greater than zero");
        }

        if (amount > Remaining)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount cannot exceed remaining amount");
        }

        
        
        Filled += amount;
        if (Filled == Size)
        {
            Status = OrderStatus.Filled;
            return;
        }
        
        Status = OrderStatus.PartiallyFilled;
    }

    public override string ToString()
    {
        return $"ID: {Id}, Side: {Side}, Price: {Price}, Size: {Size}, Remaining: {Remaining}, Filled: {Filled}";
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Open || Status == OrderStatus.PartiallyFilled)
        {
            Status = OrderStatus.Canceled;
        }
        else
        {
            throw new InvalidOperationException("The order can't be cancelled");
        }
    }
}