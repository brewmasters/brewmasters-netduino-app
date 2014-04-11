using System;
using Microsoft.SPOT;

namespace Brewmasters
{
    public static class StringHelper
    {
        public static string GetTextBetween(string str, string a, string b)
        {
            if (str == null || str == String.Empty) { return String.Empty; }

            int aIdx = str.IndexOf(a);
            if (aIdx == -1) { return String.Empty; }
            int strt = aIdx + a.Length;

            int bIdx = str.IndexOf(b, strt);
            if (bIdx == -1) { return String.Empty; }
            int stop = bIdx;

            return str.Substring(strt, stop - strt).Trim();
        }
    }

}