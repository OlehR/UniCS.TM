using System;
using System.Collections.Generic;
using System.Text;

namespace ModelMID
{
    public static class StringExtensions
    {
        public static string Right(this string str, int length)
        {
            return str.Substring(str.Length - length, length);
        }
    }
}
