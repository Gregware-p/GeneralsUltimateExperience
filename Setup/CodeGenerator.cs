using System;
using System.Text;

namespace Setup
{
    public static class CodeGenerator
    {
        private static readonly Random _random = new Random();

        /// <summary>
        /// Format : XXXX-XXXXXXX-XXXXXXX-XXXX
        /// Chiffres uniquement
        /// </summary>
        public static string GenerateNumeric()
        {
            var allowedChars = "0123456789";
            var sb = new StringBuilder();

            AppendRandom(sb, allowedChars, 4);
            sb.Append('-');
            AppendRandom(sb, allowedChars, 7);
            sb.Append('-');
            AppendRandom(sb, allowedChars, 7);
            sb.Append('-');
            AppendRandom(sb, allowedChars, 4);

            return sb.ToString();
        }

        /// <summary>
        /// Format : XXXX-XXXX-XXXX-XXXX-XXXX
        /// Chiffres + lettres MAJUSCULES
        /// </summary>
        public static string GenerateAlphaNumericUpper()
        {
            var allowedChars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var sb = new StringBuilder();

            AppendRandom(sb, allowedChars, 4);
            sb.Append('-');
            AppendRandom(sb, allowedChars, 4);
            sb.Append('-');
            AppendRandom(sb, allowedChars, 4);
            sb.Append('-');
            AppendRandom(sb, allowedChars, 4);
            sb.Append('-');
            AppendRandom(sb, allowedChars, 4);

            return sb.ToString();
        }

        private static void AppendRandom(StringBuilder sb, string allowedChars, int length)
        {
            for (int i = 0; i < length; i++)
            {
                sb.Append(allowedChars[_random.Next(allowedChars.Length)]);
            }
        }
    }
}
