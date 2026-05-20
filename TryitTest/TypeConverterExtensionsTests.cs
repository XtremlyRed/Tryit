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
        int result = TypeConverterExtensions.ConvertTo<int>(source);

        // Assert
        Assert.AreEqual(source, result, "Should return original value when types match");
    }

    [TestMethod]
    public void ConvertTo_StringToInt_ConvertsSuccessfully()
    {
        // Arrange
        string source = "123";

        // Act
        int result = TypeConverterExtensions.ConvertTo<int>(source);

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
        TypeConverterExtensions.ConvertTo<int>(source);
    }

    [TestMethod]
    public void ConvertTo_WithCustomConverter_UsesRegisteredConverter()
    {
        // Arrange
        var source = new SourceType { Value = 42 };
        TypeConverterExtensions.ConvertRegister<SourceType, TargetType>(s => new TargetType { Value = s.Value });

        // Act
        var result = TypeConverterExtensions.ConvertTo<TargetType>(source);

        // Assert
        Assert.AreEqual(source.Value, result.Value, "Custom converter should properly convert values");
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ConvertTo_NullValue_ThrowsArgumentNullException()
    {
        // Arrange
        object? source = null;

        // Act
        TypeConverterExtensions.ConvertTo<string>(source);
    }

    [TestMethod]
    public void AppendConverter_RegisterMultipleConverters_WorksCorrectly()
    {
        // Arrange
        TypeConverterExtensions.ConvertRegister<int, string>(i => i.ToString());
        TypeConverterExtensions.ConvertRegister<string, double>(s => double.Parse(s));

        // Act
        string stringResult = TypeConverterExtensions.ConvertTo<string>(42);
        double doubleResult = TypeConverterExtensions.ConvertTo<double>(stringResult);

        // Assert
        Assert.AreEqual("42", stringResult, "First conversion should work");
        Assert.AreEqual(42.0, doubleResult, "Second conversion should work");
    }

    [TestMethod]
    public void ConvertTo_ConcurrentAccess_HandlesMultipleThreads()
    {
        // Arrange
        TypeConverterExtensions.ConvertRegister<int, string>(i => i.ToString());
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
                    string result = TypeConverterExtensions.ConvertTo<string>(index);
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
        CollectionAssert.DoesNotContain(results, false, "All conversions should succeed in concurrent scenario");
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
