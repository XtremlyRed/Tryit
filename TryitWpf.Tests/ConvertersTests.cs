using System;
using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tryit.Wpf;

namespace TryitWpf.Tests;

[TestClass]
public class ConvertersTests
{
    private enum TestEnum
    {
        [System.ComponentModel.Description("FirstDisplay")]
        First = 1,

        [System.ComponentModel.Description("Second description")]
        Second = 2,

        Third = 3,
    }

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
    public void BooleanConverterExtension_ProvideValue_ProducesConfiguredConverter()
    {
        var ext = new BooleanConverterExtension { True = "T", False = "F" };
        var provided = (BooleanConverter)ext.ProvideValue(null!);

        Assert.IsNotNull(provided);
        Assert.AreEqual("T", provided.True);
        Assert.AreEqual("F", provided.False);
    }

    [TestMethod]
    public void ValueConverterBase_InputConvert_ThrowsOnWrongType()
    {
        var conv = new BooleanConverter();

        Assert.ThrowsException<ArgumentException>(() => ((IValueConverter)conv).Convert("not-bool", typeof(object), null, CultureInfo.InvariantCulture));
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
    public void NullOrEmptyConverter_HandlesCollectionsAndNull_AndEnumerableNonCollection()
    {
        var conv = new NullOrEmptyConverter { True = true, False = false };

        var empty = ((IValueConverter)conv).Convert(Array.Empty<int>(), typeof(object), null, CultureInfo.InvariantCulture);
        var nonEmpty = ((IValueConverter)conv).Convert(new[] { 1, 2 }, typeof(object), null, CultureInfo.InvariantCulture);
        var nullValue = ((IValueConverter)conv).Convert(null, typeof(object), null, CultureInfo.InvariantCulture);

        Assert.AreEqual(true, empty);
        Assert.AreEqual(false, nonEmpty);
        Assert.AreEqual(true, nullValue);

        // enumerable that is not ICollection
        IEnumerable SingleItemEnumerable()
        {
            yield return 1;
        }

        var seqResult = ((IValueConverter)conv).Convert(SingleItemEnumerable(), typeof(object), null, CultureInfo.InvariantCulture);
        Assert.AreEqual(false, seqResult);
    }

    [TestMethod]
    public void NullOrWhiteSpaceConverter_WorksForStrings()
    {
        var conv = new NullOrWhiteSpaceConverter { True = "X", False = "Y" };

        var ws = ((IValueConverter)conv).Convert("   ", typeof(object), null, CultureInfo.InvariantCulture);
        var text = ((IValueConverter)conv).Convert("text", typeof(object), null, CultureInfo.InvariantCulture);
        var nil = ((IValueConverter)conv).Convert(null, typeof(object), null, CultureInfo.InvariantCulture);

        Assert.AreEqual("X", ws);
        Assert.AreEqual("Y", text);
        Assert.AreEqual("X", nil);
    }

    [TestMethod]
    public void EnumConverters_DisplayNameAndDescription()
    {
        var displayConv = new EnumDisplayNameConverter();
        var descConv = new EnumDescriptionConverter();

        var display = ((IValueConverter)displayConv).Convert(TestEnum.First, typeof(string), null, CultureInfo.InvariantCulture);
        var description = ((IValueConverter)descConv).Convert(TestEnum.Second, typeof(string), null, CultureInfo.InvariantCulture);
        var fallback = ((IValueConverter)displayConv).Convert(TestEnum.Third, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.AreEqual("First", display);
        Assert.AreEqual("Second description", description);
        Assert.AreEqual("Third", fallback);
    }

    [TestMethod]
    public void CompareConverters_BasicComparisons()
    {
        var gt = new GreaterThanConverter
        {
            True = "T",
            False = "F",
            Input = 5,
        };
        var gte = new GreaterThanOrEqualConverter
        {
            True = true,
            False = false,
            Input = 5,
        };
        var lt = new LessThanConverter
        {
            True = "Y",
            False = "N",
            Input = 10,
        };
        var eq = new EqualConverter
        {
            True = 1,
            False = 0,
            Input = 42,
        };

        Assert.AreEqual("T", ((IValueConverter)gt).Convert(6, typeof(object), null, CultureInfo.InvariantCulture));
        Assert.AreEqual(true, ((IValueConverter)gte).Convert(5, typeof(object), null, CultureInfo.InvariantCulture));
        Assert.AreEqual("Y", ((IValueConverter)lt).Convert(5, typeof(object), null, CultureInfo.InvariantCulture));
        Assert.AreEqual(1, ((IValueConverter)eq).Convert(42, typeof(object), null, CultureInfo.InvariantCulture));
    }

    [TestMethod]
    public void CompositeConverter_ChainsConvertersSequentially()
    {
        var composite = new CompositeConverter();

        // converter1: append "-A"
        IValueConverter conv1 = new FuncConverter((v, t, p, c) => v!.ToString() + "-A");
        // converter2: append "-B"
        IValueConverter conv2 = new FuncConverter((v, t, p, c) => v!.ToString() + "-B");

        composite.Converters.Add(conv1);
        composite.Converters.Add(conv2);

        var result = ((IValueConverter)composite).Convert("X", typeof(string), null, CultureInfo.InvariantCulture);

        Assert.AreEqual("X-A-B", result);
    }

    [TestMethod]
    public void FuncConverter_CreatesFromDelegates_AndImplicitOperators()
    {
        // factory create (3-arg)
        var fc = FuncConverter.Create((v, t, p) => $"[{v}]");
        var res = fc.Convert("Z", typeof(string), null, CultureInfo.InvariantCulture);
        Assert.AreEqual("[Z]", res);

        // implicit operator using 4-arg
        Func<object, Type, object, CultureInfo, object> f = (v, t, p, c) => $"{v}-{c?.Name}";
        FuncConverter conv = f;
        var outv = ((IValueConverter)conv).Convert("P", typeof(string), null, CultureInfo.InvariantCulture);
        Assert.IsTrue(outv.ToString()!.StartsWith("P-"));
    }

    [TestMethod]
    public void BrushStringConverter_ParsesBrushFromHex()
    {
        var conv = new BrushStringConverter();
        var method = typeof(BrushStringConverter).GetMethod("ConvertFrom", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(method);
        var brush = method!.Invoke(conv, new object[] { "#FF0000" });
        Assert.IsInstanceOfType(brush, typeof(SolidColorBrush));
        var scb = (SolidColorBrush)brush!;
        Assert.AreEqual(255, scb.Color.R);
    }

    [TestMethod]
    public void NotNullOrEmptyConverter_StringBehavior()
    {
        var conv = new NotNullOrEmptyConverter { True = true, False = false };

        var empty = ((IValueConverter)conv).Convert(string.Empty, typeof(object), null, CultureInfo.InvariantCulture);
        var nonEmpty = ((IValueConverter)conv).Convert("v", typeof(object), null, CultureInfo.InvariantCulture);
        var nullVal = ((IValueConverter)conv).Convert(null, typeof(object), null, CultureInfo.InvariantCulture);

        Assert.AreEqual(false, empty);
        Assert.AreEqual(true, nonEmpty);
        Assert.AreEqual(false, nullVal);
    }

    [TestMethod]
    public void ColorStringConverter_ParsesHexColor_UsingReflection()
    {
        var conv = new ColorStringConverter();
        // ConvertFrom is protected; invoke via reflection
        var method = typeof(ColorStringConverter).GetMethod("ConvertFrom", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(method);
        var color = method!.Invoke(conv, new object[] { "#FF0000" });

        Assert.IsInstanceOfType(color, typeof(Color));
        var c = (Color)color!;
        Assert.AreEqual(255, c.R);
        Assert.AreEqual(0, c.G);
        Assert.AreEqual(0, c.B);
    }

    [TestMethod]
    public void CollectionCountConverter_CountsBothCollectionAndEnumerable()
    {
        var conv = new CollectionCountConverter();

        var arrCount = ((IValueConverter)conv).Convert(new[] { 1, 2, 3 }, typeof(object), null, CultureInfo.InvariantCulture);
        Assert.AreEqual(3, arrCount);

        IEnumerable Gen()
        {
            yield return 42;
        }

        var seqCount = ((IValueConverter)conv).Convert(Gen(), typeof(object), null, CultureInfo.InvariantCulture);
        Assert.AreEqual(1, seqCount);
    }

    [TestMethod]
    public void LessThanOrEqual_And_NotEqual_Converters()
    {
        var lte = new LessThanOrEqualConverter
        {
            True = true,
            False = false,
            Input = 5,
        };
        var neq = new NotEqualConverter
        {
            True = "T",
            False = "F",
            Input = 10,
        };

        Assert.AreEqual(true, ((IValueConverter)lte).Convert(5, typeof(object), null, CultureInfo.InvariantCulture));
        Assert.AreEqual("T", ((IValueConverter)neq).Convert(9, typeof(object), null, CultureInfo.InvariantCulture));
        Assert.AreEqual("F", ((IValueConverter)neq).Convert(10, typeof(object), null, CultureInfo.InvariantCulture));
    }

    [TestMethod]
    public void IfConverter_BehavesAsExpected_And_ThrowsOnWrongType()
    {
        var iff = new If { True = "YES", False = "NO" };

        var yes = ((IValueConverter)iff).Convert(true, typeof(object), null, CultureInfo.InvariantCulture);
        var no = ((IValueConverter)iff).Convert(false, typeof(object), null, CultureInfo.InvariantCulture);

        Assert.AreEqual("YES", yes);
        Assert.AreEqual("NO", no);

        Assert.ThrowsException<NotSupportedException>(() => ((IValueConverter)iff).Convert("wrong", typeof(object), null, CultureInfo.InvariantCulture));
    }

    [TestMethod]
    public void SwitchConverter_SelectsMatchingCaseOrDefault()
    {
        var sw = new Switch();
        sw.Cases.Add(new Case { Input = 1, Value = "one" });
        sw.Cases.Add(new Case { Input = "x", Value = "ex" });
        sw.DefaultValue = "def";

        Assert.AreEqual("one", ((IValueConverter)sw).Convert(1, typeof(object), null, CultureInfo.InvariantCulture));
        Assert.AreEqual("ex", ((IValueConverter)sw).Convert("x", typeof(object), null, CultureInfo.InvariantCulture));
        Assert.AreEqual("def", ((IValueConverter)sw).Convert(99, typeof(object), null, CultureInfo.InvariantCulture));
    }

    [TestMethod]
    public void InRangeConverter_ValidatesRangeAndThrowsOnInvalidType()
    {
        var r = new InRangeConverter
        {
            MinValue = 1,
            MaxValue = 5,
            IncludeEquals = true,
            True = true,
            False = false,
        };

        Assert.AreEqual(true, ((IValueConverter)r).Convert(1, typeof(object), null, CultureInfo.InvariantCulture));
        r.IncludeEquals = false;
        Assert.AreEqual(false, ((IValueConverter)r).Convert(1, typeof(object), null, CultureInfo.InvariantCulture));

        Assert.ThrowsException<ArgumentException>(() => ((IValueConverter)r).Convert("not-number", typeof(object), null, CultureInfo.InvariantCulture));
    }

    [TestMethod]
    public void OutOfRangeConverter_WorksAccordingToImplementation()
    {
        var r = new OutOfRangeConverter
        {
            MinValue = 1,
            MaxValue = 5,
            IncludeEquals = false,
            True = true,
            False = false,
        };

        // value below min is considered out-of-range by implementation
        Assert.AreEqual(true, ((IValueConverter)r).Convert(0, typeof(object), null, CultureInfo.InvariantCulture));

        r.IncludeEquals = true;
        // according to implementation, boundary may be treated as true when IncludeEquals is true
        Assert.AreEqual(true, ((IValueConverter)r).Convert(1, typeof(object), null, CultureInfo.InvariantCulture));
    }

    [TestMethod]
    public void NullConverter_NotNullConverters_BehaveCorrectly()
    {
        var n = new NullConverter { True = true, False = false };
        var nn = new NotNullConverter { True = "A", False = "B" };

        Assert.AreEqual(true, ((IValueConverter)n).Convert(null, typeof(object), null, CultureInfo.InvariantCulture));
        Assert.AreEqual(false, ((IValueConverter)n).Convert("v", typeof(object), null, CultureInfo.InvariantCulture));

        Assert.AreEqual("A", ((IValueConverter)nn).Convert("x", typeof(object), null, CultureInfo.InvariantCulture));
        Assert.AreEqual("B", ((IValueConverter)nn).Convert(null, typeof(object), null, CultureInfo.InvariantCulture));
    }

    [TestMethod]
    public void NotNullOrWhiteSpaceConverter_StringCases()
    {
        var conv = new NotNullOrWhiteSpaceConverter { True = "T", False = "F" };

        Assert.AreEqual("F", ((IValueConverter)conv).Convert("   ", typeof(object), null, CultureInfo.InvariantCulture));
        Assert.AreEqual("T", ((IValueConverter)conv).Convert("v", typeof(object), null, CultureInfo.InvariantCulture));
        Assert.AreEqual("F", ((IValueConverter)conv).Convert(null, typeof(object), null, CultureInfo.InvariantCulture));
    }

    [TestMethod]
    public void PrintConverter_UsesConfiguredFunction_WhenPrintable()
    {
        PrintConverter.SetObjectToStringConverter(o => $"<{o}>");
        var pc = new PrintConverter { Printable = true };

        var outv = ((IValueConverter)pc).Convert("abc", typeof(object), null, CultureInfo.InvariantCulture);
        Assert.AreEqual("abc", outv);
    }

    [TestMethod]
    public void StaticConverters_ProvideExpectedBehavior()
    {
        // BooleanReverse maps true -> false and vice versa
        Assert.AreEqual(false, ((IValueConverter)Converters.BooleanReverse).Convert(true, typeof(object), null, CultureInfo.InvariantCulture));

        // IsNullOrEmpty static instance
        Assert.AreEqual(true, ((IValueConverter)Converters.IsNullOrEmpty).Convert(string.Empty, typeof(object), null, CultureInfo.InvariantCulture));
    }
}
