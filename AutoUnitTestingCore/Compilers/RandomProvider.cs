using System;
using System.Linq;

namespace AutoUnitTestingCore.Compilers
{
    public class RandomProvider
    {
        public static readonly Random Rand = new Random();

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-=/+*!@#$%^&()|\\ \"";
            return RandomStringFrom(length, chars);
        }

        public static string RandomStringFrom(int length, string chars)
        {
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[Rand.Next(s.Length)]).ToArray());
        }
    }
}