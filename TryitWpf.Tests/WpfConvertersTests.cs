using System;
using System.Collections;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tryit.Wpf;

namespace TryitTest.Wpf
{
    [TestClass]
    public class WpfConvertersTests
    {
        [TestMethod]
        public void BooleanConverter_ReturnsConfiguredValues()
        {
            var conv = new BooleanConverter { True = "Yes", False = "No" };
            var resultTrue = ((IValueConverter)conv).Convert(true, typeof(object), null, CultureInfo.InvariantCulture);
            var resultFalse = ((IValueConverter)conv).Convert(false, typeof(object), null, CultureInfo.InvariantCulture);

            Assert.AreEqual("Yes", resultTrue);
            Assert.AreEqual("No", resultFalse);
        }

        [TestMethod]
        public void NotNullConverter_ReturnsTrueWhenNotNull()
        {
            var conv = new NotNullConverter { True = "OK", False = "NO" };

            var yes = ((IValueConverter)conv).Convert("something", typeof(object), null, CultureInfo.InvariantCulture);
            var no = ((IValueConverter)conv).Convert(null, typeof(object), null, CultureInfo.InvariantCulture);

            Assert.AreEqual("OK", yes);
            Assert.AreEqual("NO", no);
        }

        [TestMethod]
        public void NullOrEmptyConverter_HandlesCollectionsAndNull()
        {
            var conv = new NullOrEmptyConverter { True = true, False = false };

            var empty = ((IValueConverter)conv).Convert(Array.Empty<int>(), typeof(object), null, CultureInfo.InvariantCulture);
            var nonEmpty = ((IValueConverter)conv).Convert(new[] { 1, 2 }, typeof(object), null, CultureInfo.InvariantCulture);
            var nullValue = ((IValueConverter)conv).Convert(null, typeof(object), null, CultureInfo.InvariantCulture);

            Assert.AreEqual(true, empty);
            Assert.AreEqual(false, nonEmpty);
            Assert.AreEqual(true, nullValue);
        }

        //[TestMethod]
        //public void ColorStringConverter_ParsesHexColor()
        //{
        //    var conv = new ColorStringConverter();
        //    var color = conv.ConvertFrom("#FF0000");

        //    Assert.IsInstanceOfType(color, typeof(Color));
        //    var c = (Color)color;
        //    Assert.AreEqual(255, c.R);
        //    Assert.AreEqual(0, c.G);
        //    Assert.AreEqual(0, c.B);
        //}

        [TestMethod]
        public void ValueConverterBase_InputConvert_ThrowsOnWrongType()
        {
            var conv = new BooleanConverter();

            Assert.ThrowsException<ArgumentException>(() => ((IValueConverter)conv).Convert("not-bool", typeof(object), null, CultureInfo.InvariantCulture));
        }
    }
}
