using System;
using NUnit.Framework;
using Moq;

namespace pr2.Tests;

[TestFixture]
public class ClassUnderTestTests
{
    private Mock<IAffectingClass>? _mockAffectingClass;
    private ClassUnderTest? _classUnderTest;

    [SetUp]
    public void SetUp()
    {
        _mockAffectingClass = new Mock<IAffectingClass>();
        _classUnderTest = new ClassUnderTest(_mockAffectingClass.Object);
    }

    [TestCase(5, 10, 10)]
    [TestCase(10, 5, 10)]
    [TestCase(5, 5, 5)]
    [TestCase(0, 10, 10)]
    [TestCase(10, 0, 10)]
    [TestCase(-5, 10, 10)]
    [TestCase(10, -5, 10)]
    [TestCase(100, 50, 100)]
    [TestCase(1, 1, 1)]
    public void PublicMethod_WithDifferentValues_ReturnsExpectedResult(int arg, int affectingVal, int expectedResult)
    {
        _mockAffectingClass!.Setup(x => x.Val).Returns(affectingVal);
        
        int result = _classUnderTest!.PublicMethod(arg);
        
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(expectedResult));
            _mockAffectingClass.Verify(x => x.Val, Times.AtLeastOnce);
        });
    }

    [TestCase(5, 10, 10, 2)]
    public void PublicMethod_VerifiesValPropertyWasCalled(int testArg, int affectingVal, int expectedResult, int expectedCallCount)
    {
        _mockAffectingClass!.Setup(x => x.Val).Returns(affectingVal);

        int result = _classUnderTest!.PublicMethod(testArg);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(expectedResult));
            _mockAffectingClass.Verify(x => x.Val, Times.Exactly(expectedCallCount));
        });
    }

    [TestCase(100, 50, 100)]
    public void PublicMethod_WhenValLessThanArg_VerifiesValPropertyWasCalledOnce(int testArg, int affectingVal, int expectedResult)
    {
        _mockAffectingClass!.Setup(x => x.Val).Returns(affectingVal);

        int result = _classUnderTest!.PublicMethod(testArg);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(expectedResult));
            _mockAffectingClass.Verify(x => x.Val, Times.Once);
        });
    }

    [TestCase(5, 10)]
    public void PublicMethod_VerifiesValPropertyWasCalledAtLeastOnce(int arg, int affectingVal)
    {
        _mockAffectingClass!.Setup(x => x.Val).Returns(affectingVal);
        _classUnderTest!.PublicMethod(arg);
        _mockAffectingClass.Verify(x => x.Val, Times.AtLeastOnce);
    }
}

