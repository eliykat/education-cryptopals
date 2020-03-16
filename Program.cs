using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Cryptopals
{
    class Program
    {
        static void Main(string[] args)
        {
            string plainText1 = "Burning 'em, if you ain't quick and nimble" + Environment.NewLine + "I go crazy when I hear a cymbal",
                key = "ICE",
                test1 = "0b3637272a2b2e63622c2e69692a23693a2a3c6324202d623d63343c2a26226324272765272" +
                    "a282b2f20430a652e2c652a3124333a653e2b2027630c692b20283165286326302e27282f";

            byte[] bytes1 = System.Text.Encoding.UTF8.GetBytes(plainText1),
                keyBytes = System.Text.Encoding.UTF8.GetBytes(key);

            string result1 = prettyPrint16(xorByteArrays(bytes1, keyBytes));

            Console.WriteLine("Passed: {0}", test1 == result1);
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
                prettyString += barray[i].ToString("X2").ToLower();
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

        static string frequencyAnalysis(List<string> inputList)
        {
            // Simple frequency analysis by scoring the top 3 most common letters
            // They are assigned a weight that is the integer representation of their relative %
            // as per wikipedia page

            string plainText = "Error - frequency analysis did not return any likely match",
                regexPattern;
            double highScore = 100,
                currentScore,
                charCount;

            // Dictionary representing most common characters in English language
            // and their relative frequency
            Dictionary<string, double> alphabet = new Dictionary<string, double>
            {
                { "e", 0.12702 },
                { "t", 0.09356 },
                { "a", 0.08167 },
                { "o", 0.07507 },
                { "i", 0.06966 },
                { "n", 0.06749 },
                { "s", 0.06327 },
                { "h", 0.06094 }, 
                { "r", 0.05987 },
                { "d", 0.04253 },
                { "l", 0.04025 },
                { "u", 0.02758 }
            };

            // Test range of possible single character keys
            foreach (string input in inputList)
            {
                currentScore = 0;

                // Find the frequency of most common letters and subtract from their expected frequency
                // The sum of this result is the plain text's score.
                // The lowest score indicates the least deviation from the expected frequency.
                // This works for Set 1/Challenge 3, but if required in the future,
                // we could return a top 10 list rather than a single winner.

                foreach (KeyValuePair<string, double> letter in alphabet)
                {
                    regexPattern = @"[" + letter.Key + letter.Key.ToLower() + "]";

                    charCount = new Regex(regexPattern).Matches(input).Count;
                    currentScore += Math.Abs(letter.Value - (charCount / input.Length));
                }

                if (currentScore < highScore)
                {
                    plainText = input;
                    highScore = currentScore;
                }
            }

            return plainText;

        }

        static string bruteForceSingleBitXOR(byte[] cipherBytes)
        {
            // Tries a range of single-bit keys against XOR'd ciphertext
            // and returns the most likely plaintext based on frequency analysis

            List<string> testPlainText = new List<string>();

            // Construct array of test plain texts
            for (int i = 0; i < 127; i++)
            {
                byte[] testKey = { Convert.ToByte(i) };

                testPlainText.Add(System.Text.Encoding.UTF8.GetString(xorByteArrays(cipherBytes, testKey)));
            }

            return frequencyAnalysis(testPlainText);

        }
    }
}