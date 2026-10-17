namespace Exchange.Models;

public class Balance
{
    public decimal Total { get; private set; }
    public decimal BlockedAmount { get; private set; }
    public decimal AvailableAmount => Total - BlockedAmount;
    
    public void Block(decimal amount)
    {
        CheckPositive(amount);

        CheckAvailability(amount);
        
        BlockedAmount +=  amount;
    }
    
    
    public void Unblock(decimal amount)
    {
        CheckPositive(amount);

        if (amount > BlockedAmount)
        {
            throw new InvalidOperationException("Amount cannot exceed BlockedAmount");
        }
        
        BlockedAmount -=  amount;
    }

    public void Deposit(decimal amount)
    {
        CheckPositive(amount);
        Total += amount;
    }
    
    public void Withdraw(decimal amount)
    {
        CheckPositive(amount);
        CheckAvailability(amount);
        Total -= amount;
    }

    public void DecreaseBalance(decimal amount)
    {
        CheckPositive(amount);
        Unblock(amount);
        Withdraw(amount);
    }

    private void CheckPositive(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be greater than zero");
        }
    }

    private void CheckAvailability(decimal amount)
    {
        if (amount > AvailableAmount)
        {
            throw new InsufficientFundsException(AvailableAmount, amount);
        }
    }

    public override string ToString()
    {
        return $"Total: {Total}, BlockedAmount: {BlockedAmount}, AvailableAmount: {AvailableAmount}";
    }
}