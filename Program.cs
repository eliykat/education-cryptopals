using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Cryptopals
{
    class Program
    {
        static void Main(string[] args)
        {

            string inputString = "1b37373331363f78151b7f2b783431333d78397828372d363c78373e783a393b3736";

            frequencyAnalysis(hexToBytes(inputString));
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
                result[i] = (byte)(barray1[i] ^ barray2[i % barray2.Length]);
            }

            return result;
        }

        static void frequencyAnalysis(byte[] cipherBytes)
        {
            // Simple frequency analysis by scoring the top 3 most common letters
            // They are assigned a weight that is the integer representation of their relative %
            // as per wikipedia page

            string plainText = "Error";
            int highScore = 0,
                currentScore;

            Dictionary<string, int> alphabet = new Dictionary<string, int>
            {
                { "e", 127 },
                { "t", 94 },
                { "a", 82 }
            };

            byte[] testPlainBytes;
            string testPlainText;

            // Test range of possible single character keys
            for (int i = 0; i < 127; i++)
            {
                currentScore = 0;
                byte[] testKey = { Convert.ToByte(i) };

                testPlainBytes = xorByteArrays(cipherBytes, testKey);
                testPlainText = System.Text.Encoding.UTF8.GetString(testPlainBytes);

                foreach (KeyValuePair<string, int> letter in alphabet)
                {
                    currentScore += new Regex(Regex.Escape(letter.Key)).Matches(testPlainText).Count;
                }

                Console.WriteLine("Score: " + currentScore + " for string " + testPlainText);

                if (currentScore > highScore)
                {
                    plainText = testPlainText;
                    highScore = currentScore;
                    Console.WriteLine("New high score!");
                }
            }

            Console.WriteLine("We have a winner!");
            Console.WriteLine(plainText);

        }
    }
}