using Exchange.Repository;

namespace Exchange.Models;

public class Trader : IEntity
{
    private static int _nextId;
    public int Id { get; init; }
    public string Name { get; init; }
    public Balance MoneyBalance { get; }
    public Balance CurrencyBalance { get; }
    
    
    
    public Trader(string name)
    {
        Id = _nextId;
        _nextId++;
        Name = name;
        MoneyBalance = new Balance();
        CurrencyBalance = new Balance();
    }

    public override string ToString()
    {
        return $": Name: {Name}, MoneyBalance: {MoneyBalance}, CurrencyBalance: {CurrencyBalance} ";
    }
}