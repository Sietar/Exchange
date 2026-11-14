# Exchange

An in-memory order matching engine written in C#: the core of a trading exchange, without a web layer.

Traders place buy and sell limit orders for one asset. The engine matches them against an order book, executes trades, updates traders' balances and notifies subscribers about what happened.

> **Status:** learning project. The domain core works; there is no UI, API or persistence yet — `Program.cs` is a placeholder.

## Features

- **Order book with price-time priority.** The best price is matched first; at the same price, the earlier order wins.
- **Limit orders with partial fills.** An incoming order trades against the opposite side while prices cross; the unfilled remainder rests in the book.
- **Trades at the resting order's price.** A buyer who bid higher than the matched ask gets the difference back.
- **Balance reservation.** Placing an order blocks the money (for a buy) or the asset (for a sell); it is released on fill or on cancel. Orders a trader cannot afford are rejected.
- **Order cancellation** for open and partially filled orders.
- **Events** for placed, executed and canceled orders (`OrderPlaced`, `TradeExecuted`, `OrderCanceled`).

## How it works

```
Exchange/
├── Models/        Order, Trade, Trader, Balance, Side, OrderStatus
├── TradeModel/
│   ├── OrderBook.cs       bids and asks
│   └── MatchingEngine.cs  Submit() and Cancel()
└── Repository/    generic IRepository<T> with an in-memory implementation
```

- **`OrderBook`** keeps each side in a `SortedDictionary<decimal, LinkedList<Order>>`: price levels sorted best-first (bids descending, asks ascending), and a FIFO queue of orders inside each level. Taking the best order is cheap, and time priority comes from the queue order.
- **`MatchingEngine.Submit`** reserves funds, then loops while the incoming order has quantity left and the best opposite order's price crosses it. Each step fills both orders by the smaller remaining size, moves money and asset between the two traders, and records a `Trade`. Filled orders leave the book.
- **`Balance`** tracks `Total` and `Blocked`, so `Available = Total − Blocked` is what a trader can still commit. All amounts are `decimal`.

## Tech stack

C#, .NET 10, console application. No external packages.

## Running

```bash
dotnet build
dotnet run --project Exchange
```

The engine is used from code; a console scenario is not written yet.

## Next steps

- [ ] Unit tests for matching, partial fills, cancellation and balances
- [ ] Console demo scenario
- [ ] ASP.NET Core Web API on top of the engine
- [ ] Market orders
