using System;

namespace Cryptopals
{
    class Program
    {
        static void Main(string[] args)
        {

            string inputString = "1c0111001f010100061a024b53535009181c",
                keyString = "686974207468652062756c6c277320657965",
                requiredString = "746865206b696420646f6e277420706c6179";

            byte[] inputBytes = hexToBytes(inputString),
                keyBytes = hexToBytes(keyString);

            string output = prettyPrint16(xorByteArrays(inputBytes, keyBytes));

            Console.WriteLine(requiredString == output);
        }

        static byte[] hexToBytes(string hex)
        {

            byte[] barray = new byte[hex.Length / 2];

            int j = 0;

            for (int i = 0; i < hex.Length; i += 2)
            {
                barray[j] = Convert.ToByte(hex.Substring(i, 2), 16);
                j++;
            }

            return barray;
        }

        static string prettyPrint16(byte[] barray)
        {
            string prettyString = "";

            for (int i = 0; i< barray.Length; i++)
            {
                prettyString += Convert.ToString(barray[i], 16);
            }

            return prettyString;
        }

        static string prettyPrint64(byte[] barray) {
            return Convert.ToBase64String(barray);
        }

        static byte[] xorByteArrays(byte[] barray1, byte[] barray2)
        {
            byte[] result = new byte[barray1.Length];

            for (int i=0; i<barray1.Length; i++)
            {
                result[i] = (byte)(barray1[i] ^ barray2[i]);
            }

            return result;
        }
    }
}
