using System;
using System.Reflection;
using HarmonyLib;
using NUnit.Framework;
using Moq;
using Is = NUnit.Framework.Is;

namespace pr2.Tests;

[TestFixture]
public class ClassUnderTestStaticMethodTests
{
    private Mock<IAffectingClass>? _mockAffectingClass;
    private ClassUnderTest? _classUnderTest;
    private Harmony? _harmony;

    [SetUp]
    public void SetUp()
    {
        _mockAffectingClass = new Mock<IAffectingClass>();
        _classUnderTest = new ClassUnderTest(_mockAffectingClass.Object);
        _harmony = new Harmony("StaticMethodShim");
    }

    [TearDown]
    public void TearDown()
    {
        _harmony?.UnpatchAll("StaticMethodShim");
    }

    private static MethodInfo GetStaticDependencyMethod()
    {
        var method = typeof(IAffectingClass).GetMethod(
            nameof(IAffectingClass.StaticDependency),
            BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
        
        Assert.That(method, Is.Not.Null);
        
        return method!;
    }

    [TestCase(2)]
    public void CallStatic_WithDefaultStaticDependency_ReturnsOriginalValue(int expectedResult)
    {
        int result = _classUnderTest!.CallStatic();
        Assert.That(result, Is.EqualTo(expectedResult));
    }

    [TestCase(100)]
    public void CallStatic_WithHarmonyPatch_ReturnsMockedValue(int mockedValue)
    {
        var method = GetStaticDependencyMethod();

        var patchClass = typeof(StaticDependencyPatch);
        _harmony!.Patch(
            original: method,
            prefix: new HarmonyMethod(patchClass.GetMethod(nameof(StaticDependencyPatch.Prefix)))
        );

        StaticDependencyPatch.MockedValue = mockedValue;
        StaticDependencyPatch.UseMockedValue = true;

        int result = _classUnderTest!.CallStatic();

        Assert.That(result, Is.EqualTo(mockedValue));
    }

    [TestCase(2)]
    public void CallStatic_WithHarmonyPatch_Prefix_WhenMockedValueDisabled_ReturnsOriginalValue(int expectedResult)
    {
        var method = GetStaticDependencyMethod();

        var patchClass = typeof(StaticDependencyPatch);
        _harmony!.Patch(
            original: method,
            prefix: new HarmonyMethod(patchClass.GetMethod(nameof(StaticDependencyPatch.Prefix)))
        );

        StaticDependencyPatch.UseMockedValue = false;

        int result = _classUnderTest!.CallStatic();

        Assert.That(result, Is.EqualTo(expectedResult));
    }

    [TestCase(0)]
    [TestCase(10)]
    [TestCase(-5)]
    [TestCase(999)]
    [TestCase(int.MaxValue)]
    public void CallStatic_WithDifferentMockedValues_ReturnsExpectedResult(int mockedValue)
    {
        var method = GetStaticDependencyMethod();

        var patchClass = typeof(StaticDependencyPatch);
        _harmony!.Patch(
            original: method,
            prefix: new HarmonyMethod(patchClass.GetMethod(nameof(StaticDependencyPatch.Prefix)))
        );

        StaticDependencyPatch.MockedValue = mockedValue;
        StaticDependencyPatch.UseMockedValue = true;

        int result = _classUnderTest!.CallStatic();

        Assert.That(result, Is.EqualTo(mockedValue));
    }

    [TestCase(10, 20, 30)]
    public void CallStatic_WithHarmonyPatch_CanUseComplexLogic(int expectedResult1, int expectedResult2, int expectedResult3)
    {
        int callCount = 0;
        
        var method = GetStaticDependencyMethod();

        var patchClass = typeof(StaticDependencyPatch);
        _harmony!.Patch(
            original: method,
            prefix: new HarmonyMethod(patchClass.GetMethod(nameof(StaticDependencyPatch.PrefixWithCounter)))
        );

        StaticDependencyPatch.CallCounter = () => ++callCount;
        StaticDependencyPatch.UseCounter = true;

        int result1 = _classUnderTest!.CallStatic();
        int result2 = _classUnderTest.CallStatic();
        int result3 = _classUnderTest.CallStatic();

        Assert.Multiple(() =>
        {
            Assert.That(result1, Is.EqualTo(expectedResult1));
            Assert.That(result2, Is.EqualTo(expectedResult2));
            Assert.That(result3, Is.EqualTo(expectedResult3));
        });
    }

    [TestCase(2)]
    public void CallStatic_WithHarmonyPatch_PrefixWithCounter_WhenCounterDisabled_ReturnsOriginalValue(int expectedResult)
    {
        var method = GetStaticDependencyMethod();

        var patchClass = typeof(StaticDependencyPatch);
        _harmony!.Patch(
            original: method,
            prefix: new HarmonyMethod(patchClass.GetMethod(nameof(StaticDependencyPatch.PrefixWithCounter)))
        );

        StaticDependencyPatch.UseCounter = false;
        StaticDependencyPatch.CallCounter = null;

        int result = _classUnderTest!.CallStatic();

        Assert.That(result, Is.EqualTo(expectedResult));
    }

    [TestCase("Mocked exception")]
    public void CallStatic_WithHarmonyPatch_CanThrowException(string exceptionMessage)
    {
        var method = GetStaticDependencyMethod();

        var patchClass = typeof(StaticDependencyPatch);
        _harmony!.Patch(
            original: method,
            prefix: new HarmonyMethod(patchClass.GetMethod(nameof(StaticDependencyPatch.PrefixThrowException)))
        );

        StaticDependencyPatch.ThrowException = true;
        StaticDependencyPatch.ExceptionMessage = exceptionMessage;

        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            _classUnderTest!.CallStatic();
        });

        Assert.That(exception.Message, Is.EqualTo(exceptionMessage));
    }

    [TestCase(2)]
    public void CallStatic_WithHarmonyPatch_PrefixThrowException_WhenExceptionDisabled_ReturnsOriginalValue(int expectedResult)
    {
        var method = GetStaticDependencyMethod();

        var patchClass = typeof(StaticDependencyPatch);
        _harmony!.Patch(
            original: method,
            prefix: new HarmonyMethod(patchClass.GetMethod(nameof(StaticDependencyPatch.PrefixThrowException)))
        );

        StaticDependencyPatch.ThrowException = false;

        int result = _classUnderTest!.CallStatic();

        Assert.That(result, Is.EqualTo(expectedResult));
    }

    [TestCase(42, 2)]
    public void CallStatic_WithHarmonyPatch_IsIsolatedFromOtherTests(int mockedValue, int expectedOriginalResult)
    {
        var method = GetStaticDependencyMethod();

        var patchClass = typeof(StaticDependencyPatch);
        _harmony!.Patch(
            original: method,
            prefix: new HarmonyMethod(patchClass.GetMethod(nameof(StaticDependencyPatch.Prefix)))
        );

        StaticDependencyPatch.MockedValue = mockedValue;
        StaticDependencyPatch.UseMockedValue = true;

        int result = _classUnderTest!.CallStatic();

        Assert.That(result, Is.EqualTo(mockedValue));

        _harmony.UnpatchAll("StaticMethodShim");

        int originalResult = _classUnderTest.CallStatic();
        Assert.That(originalResult, Is.EqualTo(expectedOriginalResult));
    }
}

public static class StaticDependencyPatch
{
    public static int MockedValue { get; set; }
    public static bool UseMockedValue { get; set; }
    public static bool ThrowException { get; set; }
    public static string ExceptionMessage { get; set; } = string.Empty;
    public static bool UseCounter { get; set; }
    public static Func<int>? CallCounter { get; set; }

    public static bool Prefix(ref int __result)
    {
        if (UseMockedValue)
        {
            __result = MockedValue;
            return false;
        }
        return true;
    }

    public static bool PrefixWithCounter(ref int __result)
    {
        if (UseCounter && CallCounter != null)
        {
            int count = CallCounter();
            __result = count * 10;
            return false;
        }
        return true;
    }

    public static bool PrefixThrowException(ref int __result)
    {
        if (ThrowException)
        {
            throw new InvalidOperationException(ExceptionMessage);
        }
        return true;
    }
}

