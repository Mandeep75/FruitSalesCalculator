using FruitSalesCalculator.Core.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace FruitSalesCalculator.Tests.Unit
{
    public class DiscountRuleTests
    {
        [Fact]
        public void Constructor_KindNotSpecified_DefaultsToWholeLine()
        {
            // Arrange & Act
            var rule = new DiscountRule(2m, 0.10m);

            // Assert
            Assert.Equal(DiscountKind.WholeLine, rule.Kind);
        }
    }
}
