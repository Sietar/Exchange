namespace Exchange.Models;

public class Trader
{
    private static int nextId = 0;
    public int Id { get; init; }
    public string Name { get; init; }
    public decimal MoneyBalance { get; set; }
    public decimal CurrencyBalance { get; set; }
    
    public Trader(string name)
    {
        Id = nextId;
        nextId++;
        Name = name;
        MoneyBalance = 0m;
        CurrencyBalance = 0m;
    }
}