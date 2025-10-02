using System;
using System.Collections.Generic;
using System.Text;
using Utils;

namespace Model
{
    public class StaticModel
    {

        static Encoding encoding = Encoding.GetEncoding(1251);

        public static int PrefToInt(string pP)
        {
            if (pP == null || pP.Length != 3) return 0;

            if (pP[2] == '-') return StringToInt(pP[..2]);
            return 0;
        }
        static int CodeCharToByte(byte pChar)
        {
            if (pChar >= 48 && pChar <= 57)
                return pChar - 48;
            if (pChar >= 192 && pChar <= 223)
                return pChar - 192 + 10;
            return 0;
        }

        static int StringToInt(string pS)
        {
            byte[] data = Encoding.GetEncoding(1251).GetBytes(pS);
            return CodeCharToByte(data[0]) * 100 + CodeCharToByte(data[1]);
        }

        static byte DecodeBytetoChar(int pP)
        {
            if (pP >= 0 & pP < 10) return (byte)(pP + 48);
            if (pP >= 10 & pP < 45) return (byte)(pP + 192 - 10);
            return 48;
        }
        public static string InttoPref(int pP)
        {
            if (pP == 0) return "00-";

            byte[] r = [DecodeBytetoChar(pP / 100), DecodeBytetoChar(pP % 100)];
            return $"{encoding.GetString(r)}-";
        }
        /*static int CharToInt(char pChar)
        {
            byte[] data = Encoding.GetEncoding(1251).GetBytes(pChar.ToString());
            byte Char = data[0];//Encoding.UTF16.GetBytes(pChar.ToString())[0];            
            if (Char >= 48 && Char <= 57)
                return Char - 48;
            if (Char >= 192 && Char <= 223)
                return Char - 192 + 10;
            return 0;
        } 
        
         static char IntToChar(int pP)
        {
            byte[] a = new byte[1];
            a[0]=  Convert.ToByte( pP+192 - 10) ;
            var r= encoding.GetString(a);
            if (pP >= 0 & pP < 10) return Convert.ToChar(pP + 48);
            if (pP >= 10 & pP < 45) return Convert.ToChar(pP + 192 - 10);
            return ' ';
        }*/


        const long size8 = 100000000;
        const long size6 = 100000;
        public static long Str13ToLong(string s)
        {
            try
            {
                return (s.Length == 11) ? PrefToInt(s.Substring(0, 3)) * size8 + Convert.ToInt64(s.Substring(3, 8)) : Convert.ToInt64(s);
            }
            catch (Exception e)
            {
                var el = e.Message;
            }
            return 0;
        }
        public static string LongToStr13(long l)
        {
            var a = $"{InttoPref(Convert.ToInt32(l / size8))}";
            var b=$"{(l % size8):D8}";
            return $"{InttoPref(Convert.ToInt32(l / size8))}{(l % size8):D8}";

        }
        public static int Str11ToInt(string s) => (int)((s.Length == 9) ? PrefToInt(s.Substring(0, 3)) * size6 + Convert.ToInt64(s.Substring(3, 6)) : Convert.ToInt64(s));
        public static string IntToStr11(int i) => $"{InttoPref(Convert.ToInt32(i / size6))}{(i % size6):D6}";

        public static long CodeToLong(string s)
        {
            long Res = 0;
            if (string.IsNullOrEmpty(s)) return Res;
            Res = s.ToLong();
            if (Res != 0) return Res;
            try
            {
                return Str13ToLong(s);
            }
            catch (Exception e)
            {
                var el = e.Message;
            }
            return 0;
        }

        public static int CodeToInt(string s)
        {
            int Res = 0;
            if (string.IsNullOrEmpty(s)) return Res;
            Res = s.ToInt();
            if (Res != 0) return Res;
            try
            {
                return Str11ToInt(s);
            }
            catch (Exception e)
            {
                var el = e.Message;
            }
            return 0;
        }
    }
}
