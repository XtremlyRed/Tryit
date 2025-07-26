using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TryitTest.Extensions;

[TestClass]
public class MathExtensionsTests
{
    [TestMethod]
    public void IsCoerceZore_Double_ExactZero_ReturnsTrue()
    {
        // Arrange
        double value = 0.0;

        // Act
        bool result = value.IsCoerceZore();

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void IsCoerceZore_Double_NearZero_ReturnsTrue()
    {
        // Arrange
        double value = 1e-7;

        // Act
        bool result = value.IsCoerceZore();

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void IsCoerceZore_Double_NotNearZero_ReturnsFalse()
    {
        // Arrange
        double value = 0.1;

        // Act
        bool result = value.IsCoerceZore();

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void IsCoerceZore_Double_InvalidDigits_ThrowsException()
    {
        // Arrange
        double value = 0.1;

        // Act
        value.IsCoerceZore(0);
    }

    [TestMethod]
    public void CoerceEquals_Double_ExactEquals_ReturnsTrue()
    {
        // Arrange
        double value1 = 1.23456;
        double value2 = 1.23456;

        // Act
        bool result = value1.CoerceEquals(value2);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void CoerceEquals_Double_NearEquals_ReturnsTrue()
    {
        // Arrange
        double value1 = 1.234567;
        double value2 = 1.234568;

        // Act
        bool result = value1.CoerceEquals(value2);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void CoerceEquals_Double_NotEquals_ReturnsFalse()
    {
        // Arrange
        double value1 = 1.23456;
        double value2 = 1.23466;

        // Act
        bool result = value1.CoerceEquals(value2);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void CoerceIn_Int_ValueInRange_ReturnsValue()
    {
        // Arrange
        int value = 5;

        // Act
        int result = value.CoerceIn(0, 10);

        // Assert
        Assert.AreEqual(value, result);
    }

    [TestMethod]
    public void CoerceIn_Int_ValueBelowRange_ReturnsMinValue()
    {
        // Arrange
        int value = -5;

        // Act
        int result = value.CoerceIn(0, 10);

        // Assert
        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public void CoerceIn_Int_ValueAboveRange_ReturnsMaxValue()
    {
        // Arrange
        int value = 15;

        // Act
        int result = value.CoerceIn(0, 10);

        // Assert
        Assert.AreEqual(10, result);
    }

    [TestMethod]
    public void CoerceAtLeast_Double_ValueAboveMin_ReturnsValue()
    {
        // Arrange
        double value = 5.5;

        // Act
        double result = value.CoerceAtLeast(3.0);

        // Assert
        Assert.AreEqual(value, result);
    }

    [TestMethod]
    public void CoerceAtLeast_Double_ValueBelowMin_ReturnsMinValue()
    {
        // Arrange
        double value = 2.5;
        double minValue = 3.0;

        // Act
        double result = value.CoerceAtLeast(minValue);

        // Assert
        Assert.AreEqual(minValue, result);
    }

    [TestMethod]
    public void CoerceAtMost_Decimal_ValueBelowMax_ReturnsValue()
    {
        // Arrange
        decimal value = 7.5m;

        // Act
        decimal result = value.CoerceAtMost(10.0m);

        // Assert
        Assert.AreEqual(value, result);
    }

    [TestMethod]
    public void CoerceAtMost_Decimal_ValueAboveMax_ReturnsMaxValue()
    {
        // Arrange
        decimal value = 12.5m;
        decimal maxValue = 10.0m;

        // Act
        decimal result = value.CoerceAtMost(maxValue);

        // Assert
        Assert.AreEqual(maxValue, result);
    }

    [TestMethod]
    public void VerifyIn_Int_ValueInRange_IncludeEquals_ReturnsTrue()
    {
        // Arrange
        int value = 5;

        // Act
        bool result = value.VerifyIn(0, 10);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void VerifyIn_Int_ValueAtBoundary_IncludeEquals_ReturnsTrue()
    {
        // Arrange
        int value = 10;

        // Act
        bool result = value.VerifyIn(0, 10);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void VerifyIn_Int_ValueAtBoundary_ExcludeEquals_ReturnsFalse()
    {
        // Arrange
        int value = 10;

        // Act
        bool result = value.VerifyIn(0, 10, false);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void VerifyIn_Int_ValueOutsideRange_ReturnsFalse()
    {
        // Arrange
        int value = 15;

        // Act
        bool result = value.VerifyIn(0, 10);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void BaseConversion_ValidInput_ReturnsCorrectString()
    {
        // Arrange & Act
        string binary = MathExtensions.BaseConversion(42, 2);
        string hex = MathExtensions.BaseConversion(42, 16);
        string base36 = MathExtensions.BaseConversion(42, 36);

        // Assert
        Assert.AreEqual("101010", binary);
        Assert.AreEqual("2A", hex);
        Assert.AreEqual("16", base36);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void BaseConversion_InvalidBase_ThrowsArgumentException()
    {
        // Arrange & Act
        MathExtensions.BaseConversion(42, 37);
    }
}
