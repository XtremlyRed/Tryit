using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tryit;

namespace TryitTest
{
    [TestClass]
    public class ContextTests
    {
        private class TestData
        {
            public string Name { get; set; } = "Test";
            public int Value { get; set; } = 123;
        }

        [TestMethod]
        public void SetValue_GetValue_ByType_Success()
        {
            IContext context = new Context();
            var testData = new TestData();
            context.SetValue(testData);

            var result = context.GetValue<TestData>();

            Assert.AreSame(testData, result);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetValue_ByType_NotSet_ThrowsException()
        {
            IContext context = new Context();
            context.GetValue<TestData>();
        }

        [TestMethod]
        public void TryGetValue_ByType_Success()
        {
            IContext context = new Context();
            var testData = new TestData();
            context.SetValue(testData);

            var success = context.TryGetValue<TestData>(out var result);

            Assert.IsTrue(success);
            Assert.AreSame(testData, result);
        }

        [TestMethod]
        public void TryGetValue_ByType_NotSet_ReturnsFalse()
        {
            IContext context = new Context();
            var success = context.TryGetValue<TestData>(out var result);

            Assert.IsFalse(success);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void SetValue_GetValue_ByKey_Success()
        {
            IContext context = new Context();
            var testData = new TestData();
            var key = "my_key";
            context.SetValue(key, testData);

            var result = context.GetValue<TestData>(key);

            Assert.AreSame(testData, result);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetValue_ByKey_NotSet_ThrowsException()
        {
            IContext context = new Context();
            context.GetValue<TestData>("non_existent_key");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetValue_ByKey_WrongType_ThrowsException()
        {
            IContext context = new Context();
            context.SetValue("my_key", "a string value");
            context.GetValue<TestData>("my_key");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SetValue_ByKey_NullKey_ThrowsException()
        {
            IContext context = new Context();
            context.SetValue<string>(null!, "some value");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetValue_ByKey_EmptyKey_ThrowsException()
        {
            IContext context = new Context();
            context.GetValue<string>("");
        }

        [TestMethod]
        public void TryGetValue_ByKey_Success()
        {
            IContext context = new Context();
            var testData = new TestData();
            var key = "my_key";
            context.SetValue(key, testData);

            var success = context.TryGetValue(key, out TestData? result);

            Assert.IsTrue(success);
            Assert.AreSame(testData, result);
        }

        [TestMethod]
        public void TryGetValue_ByKey_NotSet_ReturnsFalse()
        {
            IContext context = new Context();
            var success = context.TryGetValue("non_existent_key", out TestData? result);

            Assert.IsFalse(success);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void TryGetValue_ByKey_WrongType_ReturnsFalse()
        {
            IContext context = new Context();
            context.SetValue("my_key", "a string value");
            var success = context.TryGetValue("my_key", out TestData? result);

            Assert.IsFalse(success);
            Assert.IsNull(result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TryGetValue_ByKey_WhitespaceKey_ThrowsException()
        {
            IContext context = new Context();
            context.TryGetValue(" ", out string? _);
        }

        [TestMethod]
        public void SetValue_OverwritesExistingValue_ByType()
        {
            IContext context = new Context();
            var first = "first_value";
            var second = "second_value";

            context.SetValue(first);
            var result1 = context.GetValue<string>();
            Assert.AreEqual(first, result1);

            context.SetValue(second);
            var result2 = context.GetValue<string>();
            Assert.AreEqual(second, result2);
        }

        [TestMethod]
        public void SetValue_OverwritesExistingValue_ByKey()
        {
            IContext context = new Context();
            var key = "my_key";
            var first = "first_value";
            var second = "second_value";

            context.SetValue(key, first);
            var result1 = context.GetValue<string>(key);
            Assert.AreEqual(first, result1);

            context.SetValue(key, second);
            var result2 = context.GetValue<string>(key);
            Assert.AreEqual(second, result2);
        }
    }
}
