/*****
  * Encoding usage:
  *
  * new BDictionary()
  * {
  * {"Some Key", "Some Value"},
  * {"Another Key", 42}
  * }.ToBencodedString();
  *
  * Decoding usage:
  *
  * BencodeDecoder.Decode("d8:Some Key10:Some Value13:Another Valuei42ee");
  *
  * Feel free to use it.
  * More info about Bencoding at http://wiki.theory.org/BitTorrentSpecification#bencoding
  *
  * Originally posted at http://snipplr.com/view/37790/ by SuprDewd.
  * */

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;
using System.Text;

namespace UTorrent.Api.File.Bencoding
{
    /// <summary>
    /// A class used for decoding Bencoding.
    /// </summary>
    public static class BencodeDecoder
    {
        /// <summary>
        /// Decodes the stream
        /// </summary>
        /// <param name="input">The bencoded stream.</param>
        /// <returns>An array of root elements.</returns>
        public static IBElement[] Decode(System.IO.Stream input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            if (input.CanRead == false)
                throw new InvalidOperationException("Input stream must be seekable");

            var sr = new System.IO.StreamReader(input, Encoding.ASCII);
            var filestr = sr.ReadToEnd();
            IBElement[] belements = Decode(filestr);
            return belements;
        }

        /// <summary>
        /// Decodes the string.
        /// </summary>
        /// <param name="bencodedValue">The bencoded string.</param>
        /// <returns>An array of root elements.</returns>
        public static IBElement[] Decode(string bencodedValue)
        {
            if (bencodedValue == null)
                throw new ArgumentNullException("bencodedValue");

            int index = 0;
            try
            {
                List<IBElement> rootElements = new List<IBElement>();
                while (bencodedValue.Length > index)
                {
                    rootElements.Add(ReadElement(ref bencodedValue, ref index));
                }

                return rootElements.ToArray();
            }
            catch (BencodingException) { throw; }
            catch (Exception e) { throw Error(e); }
        }

        private static IBElement ReadElement(ref string bencodedString, ref int index)
        {
            Contract.Requires(bencodedString != null);
            Contract.Requires(0 <= index);
            Contract.Requires(index < bencodedString.Length);
            Contract.Ensures(index >= Contract.OldValue(index));
            Contract.Ensures(index <= bencodedString.Length);


            switch (bencodedString[index])
            {
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9': return ReadString(ref bencodedString, ref index);
                case 'i': return ReadInteger(ref bencodedString, ref index);
                case 'l': return ReadList(ref bencodedString, ref index);
                case 'd': return ReadDictionary(ref bencodedString, ref index);
                default: throw Error();
            }
        }

        private static BDictionary ReadDictionary(ref string bencodedString, ref int index)
        {
            Contract.Requires(bencodedString != null);
            Contract.Requires(0 <= index);
            Contract.Requires((index + 1) < bencodedString.Length);
            Contract.Ensures(index >= Contract.OldValue(index));

            index++;
            BDictionary dict = new BDictionary();

            try
            {
                int tmpIndex = index;
                while (bencodedString.Length > tmpIndex && bencodedString[tmpIndex] != 'e')
                {
                    BString k = ReadString(ref bencodedString, ref tmpIndex);
                    if (bencodedString.Length <= tmpIndex)
                    {
                        throw new BencodingException("Invalid dictionary");
                    }
                    IBElement v = ReadElement(ref bencodedString, ref tmpIndex);
                    dict.Add(k, v);
                }
                index = tmpIndex;
            }
            catch (BencodingException) { throw; }
            catch (Exception e) { throw Error(e); }

            index++;
            return dict;
        }

        private static BList ReadList(ref string bencodedString, ref int index)
        {
            Contract.Requires(bencodedString != null);
            Contract.Requires(0 <= index);
            Contract.Requires((index + 1) < bencodedString.Length);
            Contract.Ensures(index >= Contract.OldValue(index));

            index++;
            BList lst = new BList();

            try
            {
                int tmpIndex = index;
                while (bencodedString.Length > tmpIndex && bencodedString[tmpIndex] != 'e')
                {
                    lst.Add(ReadElement(ref bencodedString, ref tmpIndex));
                }
                index = tmpIndex;
            }
            catch (BencodingException) { throw; }
            catch (Exception e) { throw Error(e); }

            index++;
            return lst;
        }

        private static BInteger ReadInteger(ref string bencodedString, ref int index)
        {
            Contract.Requires(bencodedString != null);
            Contract.Requires((index + 1) >= 0);
            Contract.Requires((index + 1) <= bencodedString.Length);
            Contract.Ensures(index >= Contract.OldValue(index));
            Contract.Ensures(index <= bencodedString.Length);

            index++;

            int end = bencodedString.IndexOf("e", index, StringComparison.OrdinalIgnoreCase);
            if (end == -1) throw Error();

            long integer;

            try
            {
                integer = Convert.ToInt64(bencodedString.Substring(index, end - index), System.Globalization.CultureInfo.InvariantCulture);
                index = end + 1;
            }
            catch (Exception e) { throw Error(e); }

            return new BInteger(integer);
        }

        private static BString ReadString(ref string bencodedString, ref int index)
        {
            Contract.Requires(bencodedString != null);
            Contract.Requires(index >= 0);
            Contract.Requires(index <= bencodedString.Length);
            Contract.Ensures(index >= Contract.OldValue(index));
            Contract.Ensures(index <= bencodedString.Length);

            int length, colon;

            try
            {
                colon = bencodedString.IndexOf(":", index, StringComparison.OrdinalIgnoreCase);
                if (colon == -1) throw Error();
                length = Convert.ToInt32(bencodedString.Substring(index, colon - index), System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (Exception e) { throw Error(e); }

            index = colon + 1;
            int tmpIndex = index;
            index += length;

            try
            {
                return new BString(bencodedString.Substring(tmpIndex, length));
            }
            catch (Exception e) { throw Error(e); }
        }

        private static Exception Error(Exception e)
        {
            return new BencodingException("Bencoded string invalid.", e);
        }

        private static Exception Error()
        {
            return new BencodingException("Bencoded string invalid.");
        }
    }

    /// <summary>
    /// An interface for bencoded elements.
    /// </summary>
    public interface IBElement
    {
        /// <summary>
        /// Generates the bencoded equivalent of the element.
        /// </summary>
        /// <returns>The bencoded equivalent of the element.</returns>
        string ToBencodedString();

        /// <summary>
        /// Generates the bencoded equivalent of the element.
        /// </summary>
        /// <param name="builder">The StringBuilder to append to.</param>
        /// <returns>The bencoded equivalent of the element.</returns>
        StringBuilder ToBencodedString(StringBuilder builder);
    }

    /// <summary>
    /// A bencode integer.
    /// </summary>
    public class BInteger : IBElement, IComparable<BInteger>
    {
        /// <summary>
        /// Allows you to set an integer to a BInteger.
        /// </summary>
        /// <param name="value">The integer.</param>
        /// <returns>The BInteger.</returns>
        public static implicit operator BInteger(int value)
        {
            return new BInteger(value);
        }

        /// <summary>
        /// The value of the bencoded integer.
        /// </summary>
        public long Value { get; set; }

        /// <summary>
        /// The main constructor.
        /// </summary>
        /// <param name="value">The value of the bencoded integer.</param>
        public BInteger(long value)
        {
            Value = value;
        }

        /// <summary>
        /// Generates the bencoded equivalent of the integer.
        /// </summary>
        /// <returns>The bencoded equivalent of the integer.</returns>
        public string ToBencodedString()
        {
            return ToBencodedString(new StringBuilder()).ToString();
        }

        /// <summary>
        /// Generates the bencoded equivalent of the integer.
        /// </summary>
        /// <returns>The bencoded equivalent of the integer.</returns>
        public StringBuilder ToBencodedString(StringBuilder builder)
        {
            Contract.Ensures(Contract.Result<StringBuilder>() != null);
            if (builder == null) builder = new StringBuilder("i");
            else builder.Append("i");
            return builder.Append(Value.ToString(System.Globalization.CultureInfo.InvariantCulture)).Append("e");
        }

        /// <see cref="Object.GetHashCode()"/>
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        /// <summary>
        /// Int32.Equals(object)
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            BInteger objInt = obj as BInteger;
            return objInt != null && Value.Equals(objInt.Value);
        }

        /// <see cref="Object.ToString()"/>
        public override string ToString()
        {
            return Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        public static int Compare(BInteger left, BInteger right)
        {
            if (ReferenceEquals(left, right))
            {
                return 0;
            }
            if (ReferenceEquals(left, null))
            {
                return -1;
            }
            return left.CompareTo(right);
        }


        /// <see cref="IComparable.CompareTo(object)"/>
        public int CompareTo(BInteger other)
        {
            if (other == null)
            {
                return 1;
            }

            return Value.CompareTo(other.Value);
        }

        public static bool operator ==(BInteger left, BInteger right)
        {
            if (ReferenceEquals(left, null))
            {
                return ReferenceEquals(right, null);
            }
            return left.Equals(right);
        }
        public static bool operator !=(BInteger left, BInteger right)
        {
            return !(left == right);
        }
        public static bool operator <(BInteger left, BInteger right)
        {
            return (Compare(left, right) < 0);
        }
        public static bool operator >(BInteger left, BInteger right)
        {
            return (Compare(left, right) > 0);
        }

    }

    /// <summary>
    /// A bencode string.
    /// </summary>
    public class BString : IBElement, IComparable<BString>
    {
        /// <summary>
        /// Allows you to set a string to a BString.
        /// </summary>
        /// <param name="value">The string.</param>
        /// <returns>The BString.</returns>
        public static implicit operator BString(string value)
        {
            return new BString(value);
        }

        /// <summary>
        /// The value of the bencoded integer.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// The main constructor.
        /// </summary>
        /// <param name="value"></param>
        public BString(string value)
        {
            Value = value;
        }

        /// <summary>
        /// Generates the bencoded equivalent of the string.
        /// </summary>
        /// <returns>The bencoded equivalent of the string.</returns>
        public string ToBencodedString()
        {
            return ToBencodedString(new StringBuilder()).ToString();
        }

        /// <summary>
        /// Generates the bencoded equivalent of the string.
        /// </summary>
        /// <param name="builder">The StringBuilder to append to.</param>
        /// <returns>The bencoded equivalent of the string.</returns>
        public StringBuilder ToBencodedString(StringBuilder builder)
        {
            Contract.Ensures(Contract.Result<StringBuilder>() != null);
            if (builder == null) builder = new StringBuilder(Value.Length);
            else builder.Append(Value.Length);
            return builder.Append(":").Append(Value);
        }

        /// <see cref="Object.GetHashCode()"/>
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        /// <summary>
        /// String.Equals(object)
        /// </summary>
        public override bool Equals(object obj)
        {
            BString other = obj as BString;
            if (ReferenceEquals(other, null))
            {
                return false;
            }
            return CompareTo(other) == 0;
        }

        /// <see cref="Object.ToString()"/>
        public override string ToString()
        {
            return Value;
        }

        public static int Compare(BString left, BString right)
        {
            if (ReferenceEquals(left, right))
            {
                return 0;
            }
            if (ReferenceEquals(left, null))
            {
                return -1;
            }
            return left.CompareTo(right);
        }

        /// <see cref="IComparable.CompareTo(Object)"/>
        public int CompareTo(BString other)
        {
            if (other == null)
            {
                return String.Compare(Value, null, StringComparison.Ordinal);
            }

            return String.Compare(Value, other.Value, StringComparison.Ordinal);
        }

        public static bool operator ==(BString left, BString right)
        {
            if (ReferenceEquals(left, null))
            {
                return ReferenceEquals(right, null);
            }
            return left.Equals(right);
        }

        public static bool operator !=(BString left, BString right)
        {
            return !(left == right);
        }

        public static bool operator <(BString left, BString right)
        {
            return (Compare(left, right) < 0);
        }

        public static bool operator >(BString left, BString right)
        {
            return (Compare(left, right) > 0);
        }

    }

    /// <summary>
    /// A bencode list.
    /// </summary>
    public class BList : List<IBElement>, IBElement
    {
        /// <summary>
        /// Generates the bencoded equivalent of the list.
        /// </summary>
        /// <returns>The bencoded equivalent of the list.</returns>
        public string ToBencodedString()
        {
            return ToBencodedString(new StringBuilder()).ToString();
        }

        /// <summary>
        /// Generates the bencoded equivalent of the list.
        /// </summary>
        /// <param name="builder">The StringBuilder to append to.</param>
        /// <returns>The bencoded equivalent of the list.</returns>
        public StringBuilder ToBencodedString(StringBuilder builder)
        {
            Contract.Ensures(Contract.Result<StringBuilder>() != null);
            if (builder == null) builder = new StringBuilder("l");
            else builder.Append("l");

            foreach (IBElement element in this)
            {
                if (element != null)
                {
                    element.ToBencodedString(builder);
                }
            }

            return builder.Append("e");
        }

        /// <summary>
        /// Adds the specified value to the list.
        /// </summary>
        /// <param name="value">The specified value.</param>
        public void Add(string value)
        {
            Add(new BString(value));
        }

        /// <summary>
        /// Adds the specified value to the list.
        /// </summary>
        /// <param name="value">The specified value.</param>
        public void Add(int value)
        {
            Add(new BInteger(value));
        }
    }

    /// <summary>
    /// A bencode dictionary.
    /// </summary>
    public class BDictionary : SortedDictionary<BString, IBElement>, IBElement
    {
        /// <summary>
        /// Generates the bencoded equivalent of the dictionary.
        /// </summary>
        /// <returns>The bencoded equivalent of the dictionary.</returns>
        public string ToBencodedString()
        {
            return ToBencodedString(new StringBuilder()).ToString();
        }

        /// <summary>
        /// Generates the bencoded equivalent of the dictionary.
        /// </summary>
        /// <param name="builder">The StringBuilder to append to.</param>
        /// <returns>The bencoded equivalent of the dictionary.</returns>
        public StringBuilder ToBencodedString(StringBuilder builder)
        {
            Contract.Ensures(Contract.Result<StringBuilder>() != null);

            if (builder == null) builder = new StringBuilder("d");
            else builder.Append("d");

            foreach (var item in this)
            {
                if (item.Key == null || item.Value == null)
                    throw new InvalidOperationException("Inconcistant BDictionary");

                item.Key.ToBencodedString(builder);
                item.Value.ToBencodedString(builder);
            }

            return builder.Append("e");
        }

        /// <summary>
        /// Adds the specified key-value pair to the dictionary.
        /// </summary>
        /// <param name="key">The specified key.</param>
        /// <param name="value">The specified value.</param>
        public void Add(string key, IBElement value)
        {
            Add(new BString(key), value);
        }

        /// <summary>
        /// Adds the specified key-value pair to the dictionary.
        /// </summary>
        /// <param name="key">The specified key.</param>
        /// <param name="value">The specified value.</param>
        public void Add(string key, string value)
        {
            Add(new BString(key), new BString(value));
        }

        /// <summary>
        /// Adds the specified key-value pair to the dictionary.
        /// </summary>
        /// <param name="key">The specified key.</param>
        /// <param name="value">The specified value.</param>
        public void Add(string key, int value)
        {
            Add(new BString(key), new BInteger(value));
        }

        /// <summary>
        /// Gets or sets the value assosiated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get or set.</param>
        /// <returns>The value assosiated with the specified key.</returns>
        public IBElement this[string key]
        {
            get
            {
                return this[new BString(key)];
            }
            set
            {
                this[new BString(key)] = value;
            }
        }
    }

    /// <summary>
    /// A bencoding exception.
    /// </summary>
#if !PORTABLE
    [Serializable]
#endif
    public class BencodingException : FormatException
    {
        /// <summary>
        /// Creates a new BencodingException.
        /// </summary>
        public BencodingException() { }

        /// <summary>
        /// Creates a new BencodingException.
        /// </summary>
        /// <param name="message">The message.</param>
        public BencodingException(string message) : base(message) { }

        /// <summary>
        /// Creates a new BencodingException.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner exception.</param>
        public BencodingException(string message, Exception inner) : base(message, inner) { }

#if !PORTABLE
        protected BencodingException(SerializationInfo info, StreamingContext context) : base(info, context) { }
#endif
    }
}