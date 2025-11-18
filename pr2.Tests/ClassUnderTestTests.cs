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
        
        Assert.That(result, Is.EqualTo(expectedResult));
    }
}

