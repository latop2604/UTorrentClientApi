using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace UTorrent.Api.UnitTest
{
    [TestClass]
    public class UrlActionUnitTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestUrlActionWithNullCtorParameter()
        {
            UrlAction.Create(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestUrlActionWithEmptyCtorParameter()
        {
            UrlAction.Create("");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestUrlActionWithWhiteSpaceCtorParameter()
        {
            UrlAction.Create(" ");
        }

        [TestMethod]
        public void TestUrlActionEquality()
        {
            UrlAction action1 = UrlAction.Create("action 1");
            UrlAction action2 = UrlAction.Create("ACTION 1");
            UrlAction action3 = UrlAction.Create("action 3");

            Assert.IsTrue(action1.Equals(action2));
            Assert.IsFalse(action1.Equals(action3));
            Assert.IsFalse(action1.Equals(null));
        }

        [TestMethod]
        public void TestUrlActionCollectionGetEnumerator()
        {
            UrlActionCollection urlActions = new UrlActionCollection(new UrlAction[0]);
            System.Collections.IEnumerable iEnumerable = urlActions;

            foreach (var item in iEnumerable)
            {
            }
        }
    }
}
