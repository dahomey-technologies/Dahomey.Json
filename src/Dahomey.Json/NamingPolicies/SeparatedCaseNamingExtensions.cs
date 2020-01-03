using System;

namespace Dahomey.Json.NamingPolicies
{
    internal static class SeparatedCaseNamingExtensions
    {
        internal static string ToSeparatedCase(this string name, char separator)
        {
            if (string.IsNullOrEmpty(name))
                return string.Empty;

            int countOfCapitalLettersFromFirstIndex = 0;
            ReadOnlySpan<char> nameSpan = name.AsSpan();
            bool isFirstLetterCapital = char.IsUpper(nameSpan[0]);
            
            foreach (char nameChar in nameSpan.Slice(1))
                if (char.IsUpper(nameChar))
                    countOfCapitalLettersFromFirstIndex++;

            if (countOfCapitalLettersFromFirstIndex == 0)
            {
                if (!isFirstLetterCapital)
                    return name;

#if NETCOREAPP3_0 || NETCOREAPP3_1
                return string.Create(nameSpan.Length, name,
                    (Span<char> destination, string source) =>
                    {
                        source.AsSpan().CopyTo(destination);
                        destination[0] = char.ToLowerInvariant(destination[0]);
                    });
#else
                Span<char> spanWithoutCapitalLetterAtZeroIndex = stackalloc char[nameSpan.Length];
                nameSpan.Slice(1).CopyTo(spanWithoutCapitalLetterAtZeroIndex.Slice(1));
                spanWithoutCapitalLetterAtZeroIndex[0] = char.ToLowerInvariant(nameSpan[0]);
                return spanWithoutCapitalLetterAtZeroIndex.ToString();
#endif
            }

#if NETCOREAPP3_0 || NETCOREAPP3_1
            return string.Create(nameSpan.Length + countOfCapitalLettersFromFirstIndex, separator,
                (Span<char> destination, char separatorChar) =>
                {
                    ConvertToSeparatedCase(destination, name.AsSpan(), separatorChar);
                });
#else
            Span<char> destination = stackalloc char[nameSpan.Length + countOfCapitalLettersFromFirstIndex];
            ConvertToSeparatedCase(destination, nameSpan, separator);
            return destination.ToString();
#endif
        }
        
        private static void ConvertToSeparatedCase(Span<char> destination, ReadOnlySpan<char> nameSpan, char separator)
        {
            int position = 0;

            foreach (char nameChar in nameSpan)
            {
                if (char.IsUpper(nameChar))
                {
                    if (position > 0)
                        destination[position++] = separator;
                    destination[position++] = char.ToLowerInvariant(nameChar);
                }
                else
                {
                    destination[position++] = nameChar;
                }
            }
        }
    }
}
