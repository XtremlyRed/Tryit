using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tryit;

namespace TryitTest;

[TestClass]
public class TypeConverterExtensionsTests
{
    [TestMethod]
    public void ConvertTo_WhenSourceTypeMatchesTargetType_ReturnsOriginalValue()
    {
        // Arrange
        int source = 42;

        // Act
        int result = source.ConvertTo<int>();

        // Assert
        Assert.AreEqual(source, result, "Should return original value when types match");
    }

    [TestMethod]
    public void ConvertTo_StringToInt_ConvertsSuccessfully()
    {
        // Arrange
        string source = "123";

        // Act
        int result = source.ConvertTo<int>();

        // Assert
        Assert.AreEqual(123, result, "Should convert string to int correctly");
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidCastException))]
    public void ConvertTo_InvalidConversion_ThrowsInvalidCastException()
    {
        // Arrange
        string source = "not a number";

        // Act
        source.ConvertTo<int>();
    }

    [TestMethod]
    public void ConvertTo_WithCustomConverter_UsesRegisteredConverter()
    {
        // Arrange
        var source = new SourceType { Value = 42 };
        TypeConverterExtensions.AppendConverter<SourceType, TargetType>(s => new TargetType
        {
            Value = s.Value,
        });

        // Act
        var result = source.ConvertTo<TargetType>();

        // Assert
        Assert.AreEqual(
            source.Value,
            result.Value,
            "Custom converter should properly convert values"
        );
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ConvertTo_NullValue_ThrowsArgumentNullException()
    {
        // Arrange
        object? source = null;

        // Act
        source.ConvertTo<string>();
    }

    [TestMethod]
    public void AppendConverter_RegisterMultipleConverters_WorksCorrectly()
    {
        // Arrange
        TypeConverterExtensions.AppendConverter<int, string>(i => i.ToString());
        TypeConverterExtensions.AppendConverter<string, double>(s => double.Parse(s));

        // Act
        string stringResult = 42.ConvertTo<string>();
        double doubleResult = stringResult.ConvertTo<double>();

        // Assert
        Assert.AreEqual("42", stringResult, "First conversion should work");
        Assert.AreEqual(42.0, doubleResult, "Second conversion should work");
    }

    [TestMethod]
    public void ConvertTo_ConcurrentAccess_HandlesMultipleThreads()
    {
        // Arrange
        TypeConverterExtensions.AppendConverter<int, string>(i => i.ToString());
        const int threadCount = 10;
        var tasks = new Task[threadCount];
        var results = new bool[threadCount];

        // Act
        for (int i = 0; i < threadCount; i++)
        {
            var index = i;
            tasks[i] = Task.Run(() =>
            {
                try
                {
                    string result = index.ConvertTo<string>();
                    results[index] = result == index.ToString();
                }
                catch
                {
                    results[index] = false;
                }
            });
        }
        Task.WaitAll(tasks);

        // Assert
        CollectionAssert.DoesNotContain(
            results,
            false,
            "All conversions should succeed in concurrent scenario"
        );
    }

    // Helper classes for testing custom conversion
    private class SourceType
    {
        public int Value { get; set; }
    }

    private class TargetType
    {
        public int Value { get; set; }
    }
}
