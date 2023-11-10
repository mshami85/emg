using System.Text;

namespace Emergency.Classes
{
    public static class ArabicNumberHelper
    {
        /// <summary>
        /// convert arabic (HINDI) number in string to english numbers
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Ar2En(this string input)
        {
            StringBuilder sb = new();
            foreach (var ch in input)
            {
                if (char.IsDigit(ch))
                    sb.Append(char.GetNumericValue(ch));
                else
                    sb.Append(ch);
            }
            return sb.ToString();
        }
    }
}
