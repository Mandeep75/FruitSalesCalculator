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

  ## Pricing strategies (Strategy pattern)

`IPricingStrategy` defines one interchangeable way of pricing a line:
given a base unit price and a quantity, return the line total.

- **`PerWeightPricingStrategy`** — price × weight; fractional quantities valid.
- **`PerItemPricingStrategy`** — price × count; rejects fractional quantities,
  since you can't buy 2.5 bananas.

The strategies take two numbers rather than domain objects, keeping them
decoupled from the domain model and freely composable (the discount decorator,
added next, wraps any strategy through the same interface).

## Bulk discounts (Decorator pattern)

`BulkDiscountDecorator` wraps any `IPricingStrategy` and applies a percentage
discount when quantity exceeds a threshold. Cherry ("$5.00/kg, 10% off over
2kg") is per-weight pricing _composed with_ a discount - not a third pricing
type. The same decorator works unchanged over per-item pricing, and decorators
can stack (e.g. a future seasonal discount wrapping a bulk discount).

Boundary decision: the spec says "more than 2kg", so exactly 2kg pays full
price - pinned by a dedicated test.

Two interpretation decisions on the Cherry rule, both pinned by tests:

- "More than 2kg" is exclusive - exactly 2kg pays full price.
- The brief's phrasing ("10% off for more than 2kg") is ambiguous between
  discounting the whole line or only the excess. Both are implemented as
  decorators - `BulkDiscountDecorator` (whole line, the default, matching
  retail promotion language) and `TieredDiscountDecorator` (marginal,
  tariff-style). Which applies is data on the fruit's `DiscountRule`
  (`DiscountKind`), so different fruits can use different schemes in the
  same shop.

  ## Strategy composition (Factory pattern)

`PricingStrategyFactory` is the single place that decides which pricing code a
fruit needs: pick the base strategy from `PricingType`, then wrap it in the
decorator matching the discount rule's `DiscountKind`, if a rule is present.

Consumers never construct strategies or inspect pricing types - they hand the
factory a fruit and receive a composed `IPricingStrategy`. Adding a new pricing
type or discount kind is one new class plus one new case in this factory;
nothing else in the system changes.

## Status

Work in progress — being built incrementally, commit by commit. Design decisions,
patterns used, and extension notes will be documented here as the implementation
progresses.
