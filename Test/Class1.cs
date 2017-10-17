using ClassLibrary2;
using NUnit.Framework;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    [TestFixture]
    public class TestClass
    {
        
        [Profiler.Profiler(2L, @"C:\src\opencover\main\bin\Debug\OpenCover.Console.exe", @"C:\src\opencover2\ClassLibrary1\packages\NUnit.ConsoleRunner.3.7.0\tools\nunit3-console.exe")]
        [Test]
        public void IsCorrect()
        {
            var x = Class2.M2();

            //Assert.AreEqual(3, x);
        }
        
    }
}
