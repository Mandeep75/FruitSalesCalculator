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
  any measurement basis (see 'Bulk discounts' below).
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

## Fruit catalogue (Repository pattern)

`IFruitRepository` abstracts catalogue storage; services depend only on the
interface. The default `InMemoryFruitRepository` uses a `ConcurrentDictionary`
with a case-insensitive comparer:

- Safe under concurrent reads and writes (covered by a 100-parallel-writes
  test asserting no lost entries) - and since `Fruit` is immutable, shared
  instances are safe to read from concurrently priced orders without locks.
- `GetByName` returns null for unknown fruit (normal outcome, handled by the
  pricing service); `Add` throws on duplicates (caller error, fails loudly).

## Order pricing (the service layer)

`OrderPricingService` orchestrates a price calculation: resolve each order
line's fruit from the repository, obtain its composed strategy from the
factory, price the line, and sum. It knows nothing about storage or pricing
rules - both are behind interfaces - so it never changes when pricing models
are added.

`CalculateOrderTotal` returns an `OrderPricingResult` (per-line breakdown +
total) rather than a bare number, so receipts can itemise without re-pricing.
An order line naming an unconfigured fruit fails with a clear
`KeyNotFoundException` identifying the fruit.

## Testing approach

Testing is layered deliberately, mirrored by the test project's folder
structure (`Unit/` and `Integration/`):

- **Unit** - each class exercised in isolation:
  - Pure-logic tests for strategies and decorators with real inputs and
    expected outputs, including boundary decisions (exactly 2kg pays full
    price; whole-line vs tiered discounting compared side by side).
  - Moq-isolated tests for `OrderPricingService` - the repository and factory
    are mocked, so these verify orchestration only: the right arguments reach
    the strategy, line totals sum correctly, unknown fruit and null orders
    are rejected. They stay green even if a concrete strategy has a bug -
    that failure belongs to the strategy's own tests.
- **Integration** - the real components composed, no mocks:
  - The brief's own worked example (Apple 1.5kg + Banana x4 + Cherry 3kg =
    $17.70), pinning the headline scenario end to end.
  - 100 orders priced concurrently against one shared catalogue - exercising
    the thread-safety design (ConcurrentDictionary + immutable domain objects
    objects + stateless strategies = no locks needed on the pricing path).

Each layer fails for exactly one kind of reason: a red unit test points at a
class, a red integration test points at the wiring between them.

## Status

Work in progress — being built incrementally, commit by commit. Design decisions,
patterns used, and extension notes will be documented here as the implementation
progresses.
