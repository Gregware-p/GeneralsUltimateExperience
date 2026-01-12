using System;
using System.Text;

namespace GeneralsUltimateExperience
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
            return Generate("0123456789");
        }

        /// <summary>
        /// Format : XXXX-XXXXXXX-XXXXXXX-XXXX
        /// Chiffres + lettres MAJUSCULES
        /// </summary>
        public static string GenerateAlphaNumericUpper()
        {
            return Generate("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ");
        }

        private static string Generate(string allowedChars)
        {
            var sb = new StringBuilder(26);

            AppendRandom(sb, allowedChars, 4);
            sb.Append('-');
            AppendRandom(sb, allowedChars, 7);
            sb.Append('-');
            AppendRandom(sb, allowedChars, 7);
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
