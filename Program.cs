﻿using System;
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
            double highScore = 100,
                currentScore;

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

            byte[] testPlainBytes;
            string testPlainText, regexPattern;
            double charCount;

            // Test range of possible single character keys
            for (int i = 0; i < 127; i++)
            {
                currentScore = 0;
                byte[] testKey = { Convert.ToByte(i) };


                testPlainBytes = xorByteArrays(cipherBytes, testKey);
                testPlainText = System.Text.Encoding.UTF8.GetString(testPlainBytes);

                // Find the frequency of most common letters and subtract from their expected frequency
                // The sum of this result is the plain text's score.
                // The lowest score indicates the least deviation from the expected frequency.
                // This works for Set 1/Challenge 3, but if required in the future,
                // we could return a top 10 list rather than a single winner.

                foreach (KeyValuePair<string, double> letter in alphabet)
                {
                    regexPattern = @"[" + letter.Key + letter.Key.ToLower() + "]";

                    charCount = new Regex(regexPattern).Matches(testPlainText).Count;
                    currentScore += Math.Abs(letter.Value - (charCount / testPlainText.Length));
                }

                // Console.WriteLine("Score: " + currentScore + " for string " + testPlainText);

                if (currentScore < highScore)
                {
                    plainText = testPlainText;
                    highScore = currentScore;
                }
            }

            Console.WriteLine("Most likely plaintext based on frequency analysis:");
            Console.WriteLine(plainText);

        }
    }
}