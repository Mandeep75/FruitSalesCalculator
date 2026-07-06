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

## Status

Work in progress — being built incrementally, commit by commit. Design decisions,
patterns used, and extension notes will be documented here as the implementation
progresses.
