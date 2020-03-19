using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Collections;
using System.Linq;

namespace Cryptopals
{
    class Program
    {
        static void Main(string[] args)
        {

            // Sanity test
            Console.WriteLine("Wokka wokka!! is {0}", hammingDistance(
                Encoding.UTF8.GetBytes("this is a test"),
                Encoding.UTF8.GetBytes("wokka wokka!!!")));

            try
            {
                using (StreamReader sr = new StreamReader("/Users/tom/Projects/Cryptopals/6.txt"))
                {

                    string ciphertext = sr.ReadToEnd(),
                        plaintext;

                    byte[] cipherbytes = base64ToBytes(ciphertext),
                        key;

                    key = breakVigenere(cipherbytes);

                    plaintext = Encoding.UTF8.GetString(xorByteArrays(cipherbytes, key));

                    Console.WriteLine("Key is: {0}", Encoding.UTF8.GetString(key));
                    Console.WriteLine("Plaintext is: {0}", plaintext);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error - file could not be read:");
                Console.WriteLine(e.Message);
            }
        }

        static byte[] breakVigenere(byte[] ciphertext)
        {
            int keysize = breakVigenereKeysize(ciphertext);
            byte[] key = new byte[keysize];

            int blocksInFile = ciphertext.Length / keysize;

            // transposedBytes is a 2D byte array.
            // Each row refers to one character in the key.
            // Each column refers to a block of length keysize in the ciphertext (of which there are blocksInFile).
            // The effect is to collate all chars that align with each char of the key in each row.
            byte[][] transposedBytes = new byte[keysize][];

            for (int i = 0; i<transposedBytes.Length; ++i)
            {
                transposedBytes[i] = new byte[blocksInFile];
            }

            for (int blockIndex = 0; blockIndex<blocksInFile; blockIndex++)
            {
                byte[] block = new byte[keysize];
                Array.Copy(ciphertext, keysize * blockIndex, block, 0, keysize);

                for (int item = 0; item<keysize; item++)
                {
                    transposedBytes[item][blockIndex] = block[item];
                }
            }

            // Debug only
            //Console.WriteLine("Transposed chars:");
            //for (int i = 0; i<keysize; ++i)
            //{
            //    Console.WriteLine(i + ": " + Encoding.UTF8.GetString(transposedBytes[i]));
            //}

            for (int i = 0; i<keysize; ++i)
            {
                KeyValuePair<byte[], string> bruteForced = bruteForceSingleBitXOR(transposedBytes[i]);

                key[i] = bruteForced.Key[0];
            }

            return key;
        }

        static int breakVigenereKeysize(byte[] ciphertext)
        {

            Dictionary<int, double> likelyKeys = new Dictionary<int, double>();
            KeyValuePair<int, double> result = new KeyValuePair<int, double>(0, 100);

            // Try keysizes from 4 - 40
            for (int keysize = 4; keysize < 41; keysize++)
            {

                int blocksInText = ciphertext.Length / keysize;
                double[] hammings = new double[blocksInText];

                // Calculate hamming distance between every pair of blocks in the file
                // Then get the overall average.
                // More data = more accuracy.
                for (int index = 0; index<blocksInText-1; index += 2)
                {
                    byte[] block1 = new byte[keysize],
                        block2 = new byte[keysize];

                    Array.Copy(ciphertext, index * keysize, block1, 0, keysize);
                    Array.Copy(ciphertext, (index + 1) * keysize, block2, 0, keysize);
                    hammings[index] = hammingDistance(block1, block2) / keysize;
                }

                double averageHamming = Queryable.Average(hammings.AsQueryable());

                likelyKeys.Add(keysize, averageHamming);

            }

            // For debugging - gives an ordered list of keys
            var ordered = likelyKeys.OrderBy(x => x.Value);

            foreach (KeyValuePair<int, double> item in likelyKeys)
            {
                if (item.Value < result.Value) result = item;
            }

            Console.WriteLine("Guessed keysize: {0}", result);
            return result.Key;
        }

        static int hammingDistance(byte[] barray1, byte[] barray2)
        {
            byte[] xorResult = xorByteArrays(barray1, barray2);

            BitArray xorResultBits = new BitArray(xorResult);

            int count = 0;

            foreach (bool bit in xorResultBits)
            {
                if (bit) count++;
            }

            return count;
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

        static byte[] base64ToBytes(string base64)
        {
            return Convert.FromBase64String(base64);
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
                { "u", 0.02758 },
                { "w", 0.0256 },
                { "m", 0.02406 },
                { "f", 0.02228 },
                { "c", 0.02202 },
                { "g", 0.02015 },
                { "y", 0.01994 },
                { "p", 0.01929 },
                { "b", 0.01492 },
                { "k", 0.01292 },
                { "v", 0.00978 },
                { "j", 0.00153 },
                { "x", 0.0015 },
                { "q", 0.00095 },
                { "z", 0.00077 }
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

        static KeyValuePair<byte[], string> bruteForceSingleBitXOR(byte[] cipherBytes)
        {
            // Tries a range of single-bit keys against XOR'd ciphertext
            // and returns a key-value pair of key:plaintext based on frequency analysis

            List<string> testPlainTextList = new List<string>();

            Dictionary<string, byte[]> testPlainTextWithKeys = new Dictionary<string, byte[]>();

            string plainText;
            byte[] key;

            // Constructs:
            // 1. A list of test plain texts using every possible key - for the frequencyAnalysis function
            // 2. A dictionary of the plain texts associated with their keys - so that we can identify the key
            for (int i = 0; i < 127; i++)
            {
                byte[] testKey = { Convert.ToByte(i) };
                string testPlainText = System.Text.Encoding.UTF8.GetString(xorByteArrays(cipherBytes, testKey));

                testPlainTextList.Add(testPlainText);
                testPlainTextWithKeys.Add(testPlainText, testKey);

            }

            plainText = frequencyAnalysis(testPlainTextList);

            testPlainTextWithKeys.TryGetValue(plainText, out key);

            return new KeyValuePair<byte[], string>(key, plainText);
        }
    }
}