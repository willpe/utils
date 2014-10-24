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

    public static class HexEncoding
    {
        public static byte[] FromHexString(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            if (value.Length % 2 == 1)
            {
                throw new ArgumentOutOfRangeException("Value is not a valid hexadecimal string.", "value");
            }

            var result = new byte[value.Length / 2];
            for (var i = 0; i < value.Length; i += 2)
            {
                var upperNyble = FromHex(value[i]);
                var lowerNyble = FromHex(value[i + 1]);

                result[i / 2] = (byte)((upperNyble << 4) | lowerNyble);
            }

            return result;
        }

        public static string ToHexString(byte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            var result = new char[buffer.Length * 2];
            for (var i = 0; i < buffer.Length; i++)
            {
                var upperNyble = buffer[i] >> 4;
                result[2 * i] = upperNyble < 10 ? (char)('0' + upperNyble) : (char)('A' + upperNyble - 10);

                var lowerNyble = buffer[i] & 0x0F;
                result[(2 * i) + 1] = lowerNyble < 10 ? (char)('0' + lowerNyble) : (char)('A' + lowerNyble - 10);
            }

            return new string(result);
        }

        public static string ToHexString(int value)
        {
            var buffer = BitConverter.GetBytes(value);
            return ToHexString(buffer);
        }

        public static int ToInt32(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException("value");
            }

            if (value.Length != 8)
            {
                throw new ArgumentOutOfRangeException("4 bytes (8 characters) were expected, but " + value.Length + " characters were specified.");
            }

            var buffer = FromHexString(value);
            return BitConverter.ToInt32(buffer, 0);
        }

        private static byte FromHex(char value)
        {
            if ('0' <= value && value <= '9')
            {
                return (byte)(value - '0');
            }

            if ('a' <= value && value <= 'f')
            {
                return (byte)(value - 'a' + 10);
            }

            if ('A' <= value && value <= 'F')
            {
                return (byte)(value - 'A' + 10);
            }

            throw new InvalidOperationException("'" + value + "' is not a valid hexadecimal character.");
        }
    }
}