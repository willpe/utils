// --------------------------------------------------------------------------
//  Copyright (c) 2014 Will Perry, Microsoft
//  
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
//  
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// --------------------------------------------------------------------------

namespace WillPe.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// A set of frequently used extension methods for base types (like string, int, long, etc.)
    /// </summary>
    public static class BaseTypeExtensions
    {
        public static bool AppendIf(this StringBuilder builder, string value, bool condition)
        {
            if (condition)
            {
                builder.Append(value);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Converts the numeric value to its string representation as a file size expressed in bytes,
        /// kilobytes, megabytes, etc.
        /// </summary>
        /// <param name="sizeInBytes">The size (in bytes) to be converted to it's string representation</param>
        /// <returns>A string like '21.4 MB'</returns>
        public static string FormatAsFileSize(this long sizeInBytes)
        {
            var symbols = new[] { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

            var symbol = symbols[0];
            long divisor = 1;
            var decimalPlaces = 0;
            if (sizeInBytes > 0)
            {
                var magnitude = (int)(System.Math.Log(sizeInBytes, 2) / 10);
                if (magnitude < symbols.Length)
                {
                    divisor = (long)System.Math.Pow(1024, magnitude);
                    symbol = symbols[magnitude];
                }
                else
                {
                    divisor = (long)System.Math.Pow(1024, symbols.Length - 1);
                    symbol = symbols[symbols.Length - 1];
                }

                decimalPlaces = magnitude < 2 ? 0 : 1;
            }

            return string.Format("{0:F" + decimalPlaces + "} {1,-2}", (double)sizeInBytes / (double)divisor, symbol);
        }

        /// <summary>
        /// Takes a TitleCased string and inserts spaces between each word.
        /// </summary>
        /// <param name="value">The value to insert spaces into.</param>
        /// <example>'ATitleCasedString' returns 'A Title Cased String'</example>
        public static string InsertSpaces(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            var sb = new StringBuilder();
            foreach (var character in value)
            {
                if (!char.IsLower(character))
                {
                    sb.Append(' ');
                }

                sb.Append(character);
            }

            return sb.ToString().Trim();
        }

        public static T[] Clone<T>(this T[] array)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }

            var copy = new T[array.Length];
            Array.Copy(array, copy, array.Length);
            return copy;
        }

        public static T Random<T>(this IReadOnlyCollection<T> items)
        {
            var index = ConcurrentRandom.Next(0, items.Count);
            return items.ElementAt(index);
        }

        public static void InvokeIfNotNull<T>(this Action<T> action, T arg1)
        {
            if (action != null)
            {
                action(arg1);
            }
        }
    }
}