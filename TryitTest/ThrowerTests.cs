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
            // ��Ӧ�׳��쳣
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

            // Assert - ��ExpectedException����
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void IsNullOrEmpty_WithEmptyString_ThrowsArgumentException()
        {
            // Arrange
            string emptyString = string.Empty;

            // Act
            Thrower.IsNullOrEmpty(emptyString);

            // Assert - ��ExpectedException����
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
                Assert.Fail("Ӧ�׳�ArgumentException");
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
            // ��Ӧ�׳��쳣
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

            // Assert - ��ExpectedException����
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void IsNullOrWhiteSpace_WithEmptyString_ThrowsArgumentException()
        {
            // Arrange
            string emptyString = string.Empty;

            // Act
            Thrower.IsNullOrWhiteSpace(emptyString);

            // Assert - ��ExpectedException����
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void IsNullOrWhiteSpace_WithWhiteSpaceString_ThrowsArgumentException()
        {
            // Arrange
            string whiteSpaceString = "   ";

            // Act
            Thrower.IsNullOrWhiteSpace(whiteSpaceString);

            // Assert - ��ExpectedException����
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
                Assert.Fail("Ӧ�׳�ArgumentException");
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
            // ������IsNull�����ƺ��뷽�����Ʋ��������ڶ���Ϊnullʱ�ŷ��أ���nullʱ���쳣
            // �����������ĺ�ע�ͣ�����ΪӦ���Ƕ���Ϊnullʱ���쳣
            // ��˴˴����Է�null����Ӧ�ò����쳣
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

            // Assert - ��ExpectedException����
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
                Assert.Fail("Ӧ�׳�ArgumentException");
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
