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
    using System.Text;

    public static class Base64Encoding
    {
        public static byte[] FromBase64StringToBuffer(string value)
        {
            // Convert from Base64URL (RFC 4648) to Base64 (standard)
            value = value.Replace('-', '+').Replace('_', '/');

            if (value.Length % 4 > 1)
            {
                value += new string('=', 4 - (value.Length % 4));
            }

            return Convert.FromBase64String(value);
        }

        public static string FromBase64StringToEncoding(string value, Encoding encoding)
        {
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }

            var buffer = Base64Encoding.FromBase64StringToBuffer(value);
            return encoding.GetString(buffer);
        }

        public static string FromBase64StringToUTF8(string value)
        {
            return Base64Encoding.FromBase64StringToEncoding(value, Encoding.UTF8);
        }

        public static string ToBase64String(string value, bool omitPaddingCharacters = true)
        {
            return Base64Encoding.ToBase64String(value, Encoding.UTF8, omitPaddingCharacters);
        }

        public static string ToBase64String(string value, Encoding encoding, bool omitPaddingCharacters = true)
        {
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }

            var valueBuffer = encoding.GetBytes(value);
            return Base64Encoding.ToBase64String(valueBuffer, omitPaddingCharacters);
        }

        public static string ToBase64String(byte[] value, bool omitPaddingCharacters = true)
        {
            var result = Convert.ToBase64String(value);

            if (omitPaddingCharacters)
            {
                result = result.TrimEnd('=');
            }

            // Convert from Base64 (standard) to Base64URL (RFC 4648)
            result = result.Replace('+', '-').Replace('/', '_');

            return result;
        }
    }
}