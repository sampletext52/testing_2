using NUnit.Framework;
using Moq;

namespace pr2.Tests;

[TestFixture]
public class ClassUnderTest2Tests
{
    [TestCase(1)]
    public void CallAffectingMethod_WithAffectingClass_ReturnsExpectedResult(int expectedResult)
    {
        var affectingClass = new AffectingClass();
        var classUnderTest2 = new ClassUnderTest2(affectingClass);

        int result = classUnderTest2.CallAffectingMethod();

        Assert.That(result, Is.EqualTo(expectedResult));
    }

    [TestCase(1)]
    [TestCase(1)]
    [TestCase(1)]
    public void CallAffectingMethod_WithMultipleInstances_ReturnsConsistentResult(int expectedResult)
    {
        var affectingClass1 = new AffectingClass();
        var affectingClass2 = new AffectingClass();
        var affectingClass3 = new AffectingClass();
        
        var classUnderTest2_1 = new ClassUnderTest2(affectingClass1);
        var classUnderTest2_2 = new ClassUnderTest2(affectingClass2);
        var classUnderTest2_3 = new ClassUnderTest2(affectingClass3);

        int result1 = classUnderTest2_1.CallAffectingMethod();
        int result2 = classUnderTest2_2.CallAffectingMethod();
        int result3 = classUnderTest2_3.CallAffectingMethod();

        Assert.Multiple(() =>
        {
            Assert.That(result1, Is.EqualTo(expectedResult));
            Assert.That(result2, Is.EqualTo(expectedResult));
            Assert.That(result3, Is.EqualTo(expectedResult));
        });
    }

    [TestCase(1)]
    public void CallAffectingMethod_WithMockAffectingClass_ReturnsExpectedResult(int expectedResult)
    {
        var mockAffectingClass = new Mock<AffectingClass>();
        mockAffectingClass.Setup(x => x.Method()).Returns(expectedResult);
        
        var classUnderTest2 = new ClassUnderTest2(mockAffectingClass.Object);

        int result = classUnderTest2.CallAffectingMethod();

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(expectedResult));
            mockAffectingClass.Verify(x => x.Method(), Times.Once);
        });
    }
}

