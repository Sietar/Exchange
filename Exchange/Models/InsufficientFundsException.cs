namespace Exchange.Models;

public class InsufficientFundsException(decimal balance, decimal needed)
    : Exception($"Insufficient funds. Need : {needed}, Current : {balance}");