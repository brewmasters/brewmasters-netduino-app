using System;

namespace EmbeddedWebserver.Core.Helpers
{
    public static class StringHelper
    {
        #region Public members

        public static bool IsNullOrEmpty(this string pInput)
        {
            return (pInput == null || pInput == string.Empty);
        }

        public static string Replace(this string pInput, string pPattern, string pReplacement)
        {
            if (pInput.IsNullOrEmpty() || pPattern.IsNullOrEmpty())
            {
                return pInput;
            }
            else
            {
                string retval = "";
                int startIndex = 0;

                int matchIndex = -1;
                while ((matchIndex = pInput.IndexOf(pPattern, startIndex)) >= 0)
                {
                    if (matchIndex > startIndex)
                    {
                        retval += pInput.Substring(startIndex, matchIndex - startIndex);
                    }
                    retval += pReplacement;
                    matchIndex += pPattern.Length;
                    startIndex = matchIndex;
                }
                if (startIndex < pInput.Length)
                {
                    retval += pInput.Substring(startIndex);
                }
                return retval;
            }
        }

        public static string PadRight(this string pInput, int pTotalLength)
        {
            if (pInput.IsNullOrEmpty())
            {
                throw new ArgumentNullException("pInput");
            }
            if (pInput.Length >= pTotalLength)
            {
                return pInput;
            }

            char[] retval = new string(' ', pTotalLength).ToCharArray();
            Array.Copy(pInput.ToCharArray(), retval, pInput.Length);
            return new string(retval);
        }

        public static bool StartsWith(this string pInput, string pPattern)
        {
            if (pInput.IsNullOrEmpty())
            {
                throw new ArgumentNullException("pInput");
            }
            if (pPattern.IsNullOrEmpty())
            {
                throw new ArgumentNullException("pPattern");
            }
            return (pInput.IndexOf(pPattern) == 0);
        }

        #endregion
    }
}
