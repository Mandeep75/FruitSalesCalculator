# Fruit Sales Calculator

A small pricing system for a fruit shop that calculates the total price of fruit
orders, where different fruits can use different pricing models (per kg, per item,
or with discount rules).

Built for the Origin Energy 2nd round coding assessment. .NET 10 / C#.

## Solution structure

| Project                           | Purpose                                                                                                                                                                                                |
| --------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| `FruitSalesCalculator.Core`       | Class library holding all domain models and business logic — fruit definitions, pricing strategies, and order price calculation. Deliberately host-agnostic: it has no dependency on how it's invoked. |
| `FruitSalesCalculator.ConsoleApp` | Console client that demonstrates the system end-to-end: configures the fruit catalogue, builds a sample order, and prints a priced receipt.                                                            |
| `FruitSalesCalculator.Tests`      | xUnit test project covering the core logic, using arrange/act/assert notation.                                                                                                                         |

## Domain model

The core domain lives in `FruitSalesCalculator.Core/Domain`:

- **`Fruit`** — a fruit the shop sells: a name, a base price, a `PricingType`
  (per kg or per item), and an optional `DiscountRule`.
- **`DiscountRule`** — an optional modifier on a fruit (e.g. "10% off over 2kg"),
  expressed as a threshold quantity and a discount fraction. Modelled as a
  modifier rather than a third pricing type, so discount logic can compose with
  any measurement basis (see design notes, coming with the pricing strategies).
- **`Order` / `OrderLine`** — a customer order. An order line holds a fruit
  _name_ and a quantity; resolution to the configured fruit happens at pricing
  time. `Quantity` is deliberately generic — kilograms for per-weight fruit,
  item count for per-item fruit.

Design notes:

- **Validation lives in constructors** — domain objects can never exist in an
  invalid state (no negative prices, no 150% discounts, no empty orders).
- **All domain types are immutable** — safe to share across concurrently
  priced orders without locking.
- **Discounts are data, not code** — a fruit carries an optional rule; no new
  classes are needed to configure a discounted fruit.

## Status

Work in progress — being built incrementally, commit by commit. Design decisions,
patterns used, and extension notes will be documented here as the implementation
progresses.
