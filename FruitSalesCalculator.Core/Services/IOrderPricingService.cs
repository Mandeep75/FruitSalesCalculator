using FruitSalesCalculator.Core.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace FruitSalesCalculator.Core.Services;

public interface IOrderPricingService
{
    /// <summary>
    /// Prices every line of the order against the configured catalogue and
    /// returns the per-line breakdown and total.
    /// Throws KeyNotFoundException if a line names a fruit that isn't configured.
    /// </summary>
    OrderPricingResult CalculateOrderTotal(Order order);
}

