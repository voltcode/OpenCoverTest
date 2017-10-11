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
        [Test]
        public void IsCorrect()
        {
            var x = Class2.M2();

            Assert.AreEqual(3, x);
        }
        
    }
}
