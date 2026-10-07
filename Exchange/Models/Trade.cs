namespace Exchange.Models;

public record Trade(int BuyOrderId, int SellOrderId, decimal Price, decimal Size, DateTime CreatedAt);