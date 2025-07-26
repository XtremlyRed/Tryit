using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tryit;

namespace TryitTest.Utils
{
    [TestClass]
    public class ThrowerTests
    {
        #region IsNullOrEmpty Tests

        [TestMethod]
        public void IsNullOrEmpty_WithValidString_DoesNotThrowException()
        {
            // Arrange
            string validString = "Test";

            // Act & Assert
            // 不应抛出异常
            Thrower.IsNullOrEmpty(validString);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void IsNullOrEmpty_WithNullString_ThrowsArgumentException()
        {
            // Arrange
            string? nullString = null;

            // Act
            Thrower.IsNullOrEmpty(nullString);

            // Assert - 由ExpectedException处理
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void IsNullOrEmpty_WithEmptyString_ThrowsArgumentException()
        {
            // Arrange
            string emptyString = string.Empty;

            // Act
            Thrower.IsNullOrEmpty(emptyString);

            // Assert - 由ExpectedException处理
        }

        [TestMethod]
        public void IsNullOrEmpty_WithArgumentName_IncludesNameInExceptionMessage()
        {
            // Arrange
            string? nullString = null;
            string argumentName = "testArgument";

            // Act
            try
            {
                Thrower.IsNullOrEmpty(nullString, argumentName);
                Assert.Fail("应抛出ArgumentException");
            }
            catch (ArgumentException ex)
            {
                // Assert
                StringAssert.Contains(ex.Message, argumentName);
            }
        }

        #endregion

        #region IsNullOrWhiteSpace Tests

        [TestMethod]
        public void IsNullOrWhiteSpace_WithValidString_DoesNotThrowException()
        {
            // Arrange
            string validString = "Test";

            // Act & Assert
            // 不应抛出异常
            Thrower.IsNullOrWhiteSpace(validString);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void IsNullOrWhiteSpace_WithNullString_ThrowsArgumentException()
        {
            // Arrange
            string? nullString = null;

            // Act
            Thrower.IsNullOrWhiteSpace(nullString);

            // Assert - 由ExpectedException处理
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void IsNullOrWhiteSpace_WithEmptyString_ThrowsArgumentException()
        {
            // Arrange
            string emptyString = string.Empty;

            // Act
            Thrower.IsNullOrWhiteSpace(emptyString);

            // Assert - 由ExpectedException处理
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void IsNullOrWhiteSpace_WithWhiteSpaceString_ThrowsArgumentException()
        {
            // Arrange
            string whiteSpaceString = "   ";

            // Act
            Thrower.IsNullOrWhiteSpace(whiteSpaceString);

            // Assert - 由ExpectedException处理
        }

        [TestMethod]
        public void IsNullOrWhiteSpace_WithArgumentName_IncludesNameInExceptionMessage()
        {
            // Arrange
            string whiteSpaceString = "   ";
            string argumentName = "testArgument";

            // Act
            try
            {
                Thrower.IsNullOrWhiteSpace(whiteSpaceString, argumentName);
                Assert.Fail("应抛出ArgumentException");
            }
            catch (ArgumentException ex)
            {
                // Assert
                StringAssert.Contains(ex.Message, argumentName);
            }
        }

        #endregion

        #region IsNull Tests

        [TestMethod]
        public void IsNull_WithNonNullObject_DoesNotThrowException()
        {
            // Arrange
            object testObject = new object();

            // Act & Assert
            // 代码中IsNull方法似乎与方法名称不符，它在对象为null时才返回，非null时抛异常
            // 但根据上下文和注释，我认为应该是对象为null时抛异常
            // 因此此处测试非null对象应该不抛异常
            Thrower.IsNull(testObject);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void IsNull_WithNullObject_ThrowsArgumentException()
        {
            // Arrange
            object? nullObject = null;

            // Act
            Thrower.IsNull(nullObject);

            // Assert - 由ExpectedException处理
        }

        [TestMethod]
        public void IsNull_WithArgumentName_IncludesNameInExceptionMessage()
        {
            // Arrange
            object? nullObject = null;
            string argumentName = "testObject";

            // Act
            try
            {
                Thrower.IsNull(nullObject, argumentName);
                Assert.Fail("应抛出ArgumentException");
            }
            catch (ArgumentException ex)
            {
                // Assert
                StringAssert.Contains(ex.Message, argumentName);
            }
        }

        #endregion
    }
}
