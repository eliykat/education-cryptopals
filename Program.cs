using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Collections;

namespace Cryptopals
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                using (StreamReader sr = new StreamReader("/Users/tom/Projects/Cryptopals/6.txt"))
                {

                    string ciphertext = sr.ReadToEnd(),
                        plaintext;

                    byte[] cipherbytes = new byte[ciphertext.Length],
                        key;

                    cipherbytes = Encoding.UTF8.GetBytes(ciphertext);

                    key = breakVigenere(cipherbytes);
                    plaintext = Encoding.UTF8.GetString(xorByteArrays(cipherbytes, key));


                    Console.WriteLine("Key is {0}", Encoding.UTF8.GetString(key));
                    Console.WriteLine("Plaintext is {0}", plaintext);

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

            int guess = 0;
            double highScore = 100;

            // Try keysizes from 4 - 40
            for (int keysize = 4; keysize < 41; keysize++)
            {

                byte[] block1 = new byte[keysize],
                    block2 = new byte[keysize],
                    block3 = new byte[keysize],
                    block4 = new byte[keysize];

                Array.Copy(ciphertext, block1, keysize);
                Array.Copy(ciphertext, keysize, block2, 0, keysize);
                Array.Copy(ciphertext, keysize * 2, block3, 0, keysize);
                Array.Copy(ciphertext, keysize * 3, block4, 0, keysize);

                double distance = (double)(hemmingDistance(block1, block2) + hemmingDistance(block3, block4)) / 2;
                double normalised = distance / keysize;

                // This is for debugging if required
                //Console.WriteLine("Keysize {0}: Distance between {1} and {2} is {3}; distance between {4} and {5} is {6}; average is {7} and normalised is {8}",
                //    keysize,
                //    Encoding.UTF8.GetString(block1),
                //    Encoding.UTF8.GetString(block2),
                //    hemmingDistance(block1, block2),
                //    Encoding.UTF8.GetString(block3),
                //    Encoding.UTF8.GetString(block4),
                //    hemmingDistance(block3, block4),
                //    distance,
                //    normalised);

                //Console.WriteLine("For comparison: without averaging, based on first two blocks, it would be {0}",
                //    (double)hemmingDistance(block1, block2) / keysize);

                if (normalised < highScore)
                {
                    guess = keysize;
                    highScore = normalised;
                }
            }

            Console.WriteLine("Best guess at keylength: {0}", guess);

            return guess;
        }

        static int hemmingDistance(byte[] barray1, byte[] barray2)
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
            for (int i = 32; i < 127; i++)
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