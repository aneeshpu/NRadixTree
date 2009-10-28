namespace RadixTree
{
    public static class StringX
    {
        public static string CommonBeginningWith(this string someString, string otherString)
        {
            string commonStart = string.Empty;
            foreach (char character in someString)
            {
                if (!otherString.StartsWith(commonStart + character)) break;
                commonStart += character;
            }
            return commonStart;
        }
    }
}