using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using RadixTree;

namespace NodeTest
{
    [TestFixture]
    public class StringTest
    {
        [Test]
        public void Should_find_the_common_beginning()
        {
            Assert.That("HelloWorld".CommonBeginningWith("HelloGold"), Is.EqualTo("Hello"));
        }

        [Test]
        public void There_is_no_common_beginning()
        {
            Assert.That("HelloWorld".CommonBeginningWith("Superman"), Is.EqualTo(string.Empty));
        }
    }
}