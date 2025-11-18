using NUnit.Framework;
using Moq;

namespace pr2.Tests;

[TestFixture]
public class ClassUnderTestProtectedMethodTests
{
    private Mock<IAffectingClass>? _mockAffectingClass;
    private TestableClassUnderTest? _testableClass;

    private class TestableClassUnderTest : ClassUnderTest
    {
        public TestableClassUnderTest(IAffectingClass pAff) : base(pAff)
        {
        }

        public int CallProtectedMethod(int arg)
        {
            return ProtectedMethod(arg);
        }
    }

    [SetUp]
    public void SetUp()
    {
        _mockAffectingClass = new Mock<IAffectingClass>();
        _testableClass = new TestableClassUnderTest(_mockAffectingClass.Object);
    }

    [Test]
    public void ProtectedMethod_WhenArgIsZero_ThrowsArgumentException()
    {
        int arg = 0;
        _mockAffectingClass!.Setup(x => x.Val).Returns(10);

        var exception = Assert.Throws<ArgumentException>(() => _testableClass!.CallProtectedMethod(arg));
        
        Assert.That(exception, Is.Not.Null);
        Assert.That(exception!.Message, Does.Contain("arg is equal to zero"));
        Assert.That(exception.ParamName, Is.EqualTo("arg"));
    }

    [TestCase(1, 5, 5)]
    [TestCase(-1, 10, 10)]
    [TestCase(100, 50, 50)]
    [TestCase(5, 0, 0)]
    [TestCase(-5, -10, -10)]
    public void ProtectedMethod_WhenArgIsNotZero_ReturnsAffectingClassVal(int arg, int affectingVal, int expectedResult)
    {
        _mockAffectingClass!.Setup(x => x.Val).Returns(affectingVal);

        int result = _testableClass!.CallProtectedMethod(arg);

        Assert.That(result, Is.EqualTo(expectedResult));
        _mockAffectingClass.Verify(x => x.Val, Times.Once);
    }

    [Test]
    public void ProtectedMethod_WhenArgIsZero_DoesNotAccessVal()
    {
        int arg = 0;
        _mockAffectingClass!.Setup(x => x.Val).Returns(10);

        Assert.Throws<ArgumentException>(() => _testableClass!.CallProtectedMethod(arg));

        _mockAffectingClass.Verify(x => x.Val, Times.Never);
    }

    [TestCase(1)]
    [TestCase(-1)]
    [TestCase(100)]
    [TestCase(int.MaxValue)]
    [TestCase(int.MinValue)]
    public void ProtectedMethod_WithNonZeroArg_DoesNotThrowException(int arg)
    {
        _mockAffectingClass!.Setup(x => x.Val).Returns(42);

        Assert.DoesNotThrow(() => _testableClass!.CallProtectedMethod(arg));
    }
}

