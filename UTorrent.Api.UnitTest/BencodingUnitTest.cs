using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using UTorrent.Api.File.Bencoding;

namespace UTorrent.Api.UnitTest
{
    [TestClass]
    public class BencodingUnitTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestDecodeNullString()
        {
            BencodeDecoder.Decode(bencodedValue: null);
        }
        [TestMethod]
        public void TestDecodeEmptyString()
        {
            IBElement[] result = BencodeDecoder.Decode("");
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Length);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestDecodeNullStream()
        {
            BencodeDecoder.Decode(input: null);
        }

        [TestMethod]
        public void TestDecodeStream()
        {
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                BencodeDecoder.Decode(ms);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(BencodingException))]
        public void TestDecodeInvalidBElement_1()
        {
            BencodeDecoder.Decode("k");
        }

        [TestMethod]
        public void TestDecodeBInteger()
        {
            IBElement[] list = BencodeDecoder.Decode("i45e");
            Assert.IsNotNull(list);
            Assert.AreEqual(1, list.Length);
            Assert.IsNotNull(list[0]);
            Assert.IsInstanceOfType(list[0], typeof(BInteger));
            BInteger integer = (BInteger)list[0];
            Assert.AreEqual(45L, integer.Value);
        }

        [TestMethod]
        public void TestDecodeInvalidBIteger_1()
        {
            Assert.ThrowsException<BencodingException>(() => BencodeDecoder.Decode("i45"));
        }

        [TestMethod]
        public void TestDecodeInvalidBIteger_2()
        {
            Assert.ThrowsException<BencodingException>(() => BencodeDecoder.Decode("45"));
        }

        [TestMethod]
        public void TestDecodeInvalidBIteger_3()
        {
            Assert.ThrowsException<BencodingException>(() => BencodeDecoder.Decode("45e"));
        }

        [TestMethod]
        public void TestDecodeInvalidBIteger_4()
        {
            Assert.ThrowsException<BencodingException>(() => BencodeDecoder.Decode("ie"));
        }

        [TestMethod]
        public void TestDecodeBString()
        {
            IBElement[] list = BencodeDecoder.Decode("28:aBCdefghijklmnopqrstuvwxyz12");
            Assert.IsNotNull(list);
            Assert.AreEqual(1, list.Length);
            Assert.IsNotNull(list[0]);
            Assert.IsInstanceOfType(list[0], typeof(BString));
            BString str = (BString)list[0];
            Assert.AreEqual("aBCdefghijklmnopqrstuvwxyz12", str.Value);
        }

        [TestMethod]
        public void TestDecodeInvalidBString_1()
        {
            Assert.ThrowsException<BencodingException>(() => BencodeDecoder.Decode(":aze"));
        }

        [TestMethod]
        public void TestDecodeInvalidBString_2()
        {
            Assert.ThrowsException<BencodingException>(() => BencodeDecoder.Decode("5:aze"));
        }

        [TestMethod]
        public void TestDecodeInvalidBString_3()
        {
            Assert.ThrowsException<BencodingException>(() => BencodeDecoder.Decode("5:"));
        }

        [TestMethod]
        public void TestBIntegerImplicitOperator()
        {
            BInteger bint = 10;
            Assert.AreEqual(10, bint.Value);
        }

        [TestMethod]
        public void TestBIntegerEquality()
        {
            BInteger bint1 = new BInteger(25);
            BInteger bint2 = new BInteger(25);
            BInteger bint3 = new BInteger(10);
            Assert.IsTrue(bint1 == bint2);
            Assert.IsFalse(bint1 == bint3);
            Assert.IsFalse(null == bint2);
            Assert.IsFalse(bint1 == null);

            Assert.IsFalse(bint1 != bint2);
            Assert.IsTrue(bint1 != bint3);
            Assert.IsTrue(null != bint2);
            Assert.IsTrue(bint1 != null);
        }

        [TestMethod]
        public void TestBIntegerGreaterLowerOperator()
        {
            BInteger bint1 = new BInteger(1);
            BInteger bint2 = new BInteger(2);
            BInteger bint3 = new BInteger(2);
            BInteger bint4 = new BInteger(3);

            Assert.IsTrue(bint1 < bint2);
            Assert.IsTrue(bint2 < bint4);
            Assert.IsFalse(bint4 < bint1);
            Assert.IsFalse(bint2 < bint3);

            Assert.IsFalse(bint1 > bint2);
            Assert.IsFalse(bint2 > bint4);
            Assert.IsTrue(bint4 > bint1);
            Assert.IsFalse(bint2 > bint3);

            Assert.AreEqual(1, bint1.CompareTo(null));
            Assert.AreEqual(0, BInteger.Compare(null, null));
            Assert.AreEqual(1, BInteger.Compare(bint1, null));
            Assert.AreEqual(-1, BInteger.Compare(null, bint1));
        }

        [TestMethod]
        public void TestBInteger_String()
        {
            BInteger bint = 123;
            var tb = bint.ToBencodedString(null);
            Assert.IsNotNull(tb);
            Assert.AreEqual("i123e", tb.ToString());

            Assert.AreEqual("123", bint.ToString());
            Assert.AreEqual("i123e", bint.ToBencodedString());

            BInteger bint2 = 123;
            BInteger bint3 = 12;
            Assert.AreEqual(bint.GetHashCode(), bint2.GetHashCode());
            Assert.AreNotEqual(bint.GetHashCode(), bint3.GetHashCode());
        }

        [TestMethod]
        public void TestDecodeBList()
        {
            IBElement[] result = BencodeDecoder.Decode("li45e28:aBCdefghijklmnopqrstuvwxyz12e");
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Length);
            Assert.IsNotNull(result[0]);
            Assert.IsInstanceOfType(result[0], typeof(BList));
            BList list = (BList)result[0];
            Assert.AreEqual(2, list.Count);

            Assert.IsNotNull(list[0]);
            Assert.IsInstanceOfType(list[0], typeof(BInteger));
            BInteger integer = (BInteger)list[0];
            Assert.AreEqual(45L, integer.Value);

            Assert.IsNotNull(list[1]);
            Assert.IsInstanceOfType(list[1], typeof(BString));
            BString str = (BString)list[1];
            Assert.AreEqual("aBCdefghijklmnopqrstuvwxyz12", str.Value);
        }

        [TestMethod]
        public void TestDecodeBDictionary()
        {
            IBElement[] result = BencodeDecoder.Decode("d5:itemsli45e28:aBCdefghijklmnopqrstuvwxyz12ee");
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Length);
            Assert.IsNotNull(result[0]);
            Assert.IsInstanceOfType(result[0], typeof(BDictionary));
            BDictionary dict = (BDictionary)result[0];
            Assert.AreEqual(1, dict.Count);

            var item = dict.First();
            Assert.IsNotNull(item);

            BString key = item.Key;
            Assert.IsNotNull(key);
            Assert.AreEqual("items", key.Value);

            Assert.IsNotNull(item.Value);
            Assert.IsInstanceOfType(item.Value, typeof(BList));
            BList list = (BList)item.Value;
            Assert.AreEqual(2, list.Count);

            Assert.IsNotNull(list[0]);
            Assert.IsInstanceOfType(list[0], typeof(BInteger));
            BInteger integer = (BInteger)list[0];
            Assert.AreEqual(45L, integer.Value);

            Assert.IsNotNull(list[1]);
            Assert.IsInstanceOfType(list[1], typeof(BString));
            BString str = (BString)list[1];
            Assert.AreEqual("aBCdefghijklmnopqrstuvwxyz12", str.Value);
        }

        [TestMethod]
        public void TestBEncodeComplexString()
        {
            const string initialString = "d5:itemsli45e28:aBCdefghijklmnopqrstuvwxyz12ee";
            IBElement[] result = BencodeDecoder.Decode(initialString);
            Assert.IsNotNull(result);
            string encodedString = string.Join("", result.Select(e => e.ToBencodedString()));
            Assert.AreEqual(initialString, encodedString, true);
        }
    }
}
