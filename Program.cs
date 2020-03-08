using System;

namespace Cryptopals
{
    class Program
    {
        static void Main(string[] args)
        {

            string hexString = "49276d206b696c6c696e6720796f757220627261696e206c696b65206120706f69736f6e6f7573206d757368726f6f6d";
            byte[] barray = hexToBytes(hexString);

            string test = "SSdtIGtpbGxpbmcgeW91ciBicmFpbiBsaWtlIGEgcG9pc29ub3VzIG11c2hyb29t";

            Console.WriteLine(test == prettyPrint64(barray));
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
    }
}
