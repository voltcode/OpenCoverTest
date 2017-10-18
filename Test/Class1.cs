using ClassLibrary2;
using NUnit.Framework;
using Profiler;

namespace Test
{
    [TestFixture]
    public class TestClass
    {        
        [TotalMethodCallsAssertion(maximum:2L, OpenCoverConsolePath=@"C:\src\opencover\main\bin\Debug\OpenCover.Console.exe", NUnit3ConsolePath = @"C:\src\opencover2\ClassLibrary1\packages\NUnit.ConsoleRunner.3.7.0\tools\nunit3-console.exe")]
        public void IsCorrect()
        {
            var x = Class2.M2();

            Assert.AreEqual(3, x);
        }

        [TotalMethodCallsAssertion(maximum: 7L)]
        public void IsCorrect2()
        {
            var x = Class2.M2();

            Assert.AreEqual(3, x);
        }

        [TotalMethodCallsAssertion(maximum: 6L, OpenCoverConsolePath = @"C:\src\opencover\main\bin\Debug\OpenCover.Console.exe", NUnit3ConsolePath = @"C:\src\opencover2\ClassLibrary1\packages\NUnit.ConsoleRunner.3.7.0\tools\nunit3-console.exe")]
        public void IsCorrect3()
        {
            var x = Class2.M2();

            Assert.AreEqual(3, x);
        }
    }
}
