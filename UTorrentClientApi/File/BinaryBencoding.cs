namespace UTorrent.Api.File.Bencoding
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Text;

    public static class BinaryBencoding
    {
        public static IBElement[] Decode(string bencodedValue)
        {
            if (bencodedValue == null)
                throw new ArgumentNullException("bencodedValue");

            using (MemoryStream ms = new MemoryStream())
            {
                StreamWriter sw = new StreamWriter(ms, Encoding.ASCII, 1024);
                sw.Write(bencodedValue);
                sw.Flush();

                ms.Position = 0;

                BinaryReader br = new BinaryReader(ms, Encoding.ASCII);
                return Decode(br);
            }
        }
        public static IBElement[] Decode(Stream input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            if (input.CanRead == false)
                throw new InvalidOperationException("Input stream must be seekable");

            BinaryReader br = new BinaryReader(input, Encoding.ASCII);
            return Decode(br);
        }

        private static IBElement[] Decode(BinaryReader binaryReader)
        {
            if (binaryReader == null)
                throw new ArgumentNullException("binaryReader");

            try
            {
                List<IBElement> rootElements = new List<IBElement>();
                while (binaryReader.PeekChar() >= 0)
                {
                    rootElements.Add(ReadElement(binaryReader));
                }

                return rootElements.ToArray();
            }
            catch (BencodingException) { throw; }
            catch (Exception e) { throw Error(e); }
        }

        private static IBElement ReadElement(BinaryReader binaryReader)
        {
            Contract.Requires(binaryReader != null);

            int b = binaryReader.PeekChar();

            switch (b)
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
                case '9': return ReadString(binaryReader);
                case 'i': return ReadInteger(binaryReader);
                case 'l': return ReadList(binaryReader);
                case 'd': return ReadDictionary(binaryReader);
                default: throw Error();
            }
        }

        private static BDictionary ReadDictionary(BinaryReader binaryReader)
        {
            Contract.Requires(binaryReader != null);

            int i = binaryReader.ReadByte();
            if (i != 'd')
            {
                throw Error();
            }

            BDictionary dict = new BDictionary();

            try
            {
                for (int c = binaryReader.PeekChar(); ; c = binaryReader.PeekChar())
                {
                    if (c == -1) throw Error();
                    if (c == 'e')
                    {
                        binaryReader.ReadByte();
                        break;
                    }
                    BString k = ReadString(binaryReader);
                    IBElement v = ReadElement(binaryReader);
                    dict.Add(k, v);
                }
            }
            catch (BencodingException) { throw; }
            catch (Exception e) { throw Error(e); }

            return dict;
        }

        private static BList ReadList(BinaryReader binaryReader)
        {
            Contract.Requires(binaryReader != null);

            byte i = binaryReader.ReadByte();
            if (i != 'l')
            {
                throw Error();
            }

            BList lst = new BList();

            try
            {
                for (int c = binaryReader.PeekChar(); ; c = binaryReader.PeekChar())
                {
                    if (c == -1) throw Error();
                    if (c == 'e')
                    {
                        binaryReader.ReadByte();
                        break;
                    }
                    lst.Add(ReadElement(binaryReader));
                }
            }
            catch (BencodingException) { throw; }
            catch (Exception e) { throw Error(e); }

            return lst;
        }

        private static BInteger ReadInteger(BinaryReader binaryReader)
        {
            Contract.Requires(binaryReader != null);

            byte i = binaryReader.ReadByte();
            if (i != 'i')
            {
                throw Error();
            }

            List<char> numList = new List<char>();
            while (true)
            {
                var b = binaryReader.ReadByte();
                if (b == 'e')
                {
                    break;
                }

                numList.Add((char)b);
            }

            try
            {
                var integer = Convert.ToInt64(new string(numList.ToArray()), System.Globalization.CultureInfo.InvariantCulture);
                return new BInteger(integer);
            }
            catch (Exception e) { throw Error(e); }
        }

        private static BString ReadString(BinaryReader binaryReader)
        {
            Contract.Requires(binaryReader != null);

            List<char> sizeList = new List<char>();

            while (true)
            {
                var b = binaryReader.ReadByte();
                if (b == ':')
                {
                    break;
                }

                if (b < '0' || b > '9') throw Error();

                sizeList.Add((char)b);
            }

            try
            {
                var length = Convert.ToInt32(new string(sizeList.ToArray()), System.Globalization.CultureInfo.InvariantCulture);

                char[] strArray = new char[length];
                var bytes = binaryReader.ReadBytes(length);

                Array.Copy(bytes, strArray, length);

                string str = new string(strArray);
                return new BString(str);
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
}
