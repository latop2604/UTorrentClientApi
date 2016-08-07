using System;

namespace UTorrent.Api.Tools
{
    public sealed class Base32Helper
    {
        public static byte[] ToBytes(string input)
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentNullException(nameof(input));

            input = input.TrimEnd('='); //remove padding characters
            var byteCount = input.Length*5/8; //this must be TRUNCATED
            var returnArray = new byte[byteCount];

            byte curByte = 0, bitsRemaining = 8;
            var arrayIndex = 0;

            foreach (var c in input)
            {
                var cValue = CharToValue(c);

                var mask = 0;
                if (bitsRemaining > 5)
                {
                    mask = cValue << (bitsRemaining - 5);
                    curByte = (byte) (curByte | mask);
                    bitsRemaining -= 5;
                }
                else
                {
                    mask = cValue >> (5 - bitsRemaining);
                    curByte = (byte) (curByte | mask);
                    returnArray[arrayIndex++] = curByte;
                    curByte = (byte) (cValue << (3 + bitsRemaining));
                    bitsRemaining += 3;
                }
            }

            //if we didn't end with a full byte
            if (arrayIndex != byteCount)
                returnArray[arrayIndex] = curByte;

            return returnArray;
        }

        private static int CharToValue(char c)
        {
            int value = c;

            //65-90 == uppercase letters
            if ((value < 91) && (value > 64))
                return value - 65;
            //50-55 == numbers 2-7
            if ((value < 56) && (value > 49))
                return value - 24;
            //97-122 == lowercase letters
            if ((value < 123) && (value > 96))
                return value - 97;

            throw new ArgumentException("Character is not a Base32 character.", nameof(c));
        }
    }
}