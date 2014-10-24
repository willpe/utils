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
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;

    public static class ConcurrentCRC32
    {
        [ThreadStatic]
        private static HashAlgorithm threadLocalCRC32;

        public static byte[] ComputeHash(byte[] buffer)
        {
            if (ConcurrentCRC32.threadLocalCRC32 == null)
            {
                ConcurrentCRC32.threadLocalCRC32 = CRC32.Create();
            }

            return ConcurrentCRC32.threadLocalCRC32.ComputeHash(buffer);
        }

        public static string ComputeHash(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException("value");
            }

            var buffer = Encoding.UTF8.GetBytes(value);
            var hash = ConcurrentCRC32.ComputeHash(buffer);

            return Base64Encoding.ToBase64String(hash);
        }

        public static string ComputeHash(Stream inputStream)
        {
            if (inputStream == null)
            {
                throw new ArgumentNullException("inputStream");
            }

            if (ConcurrentCRC32.threadLocalCRC32 == null)
            {
                ConcurrentCRC32.threadLocalCRC32 = CRC32.Create();
            }

            var hashBuffer = ConcurrentCRC32.threadLocalCRC32.ComputeHash(inputStream);

            return Base64Encoding.ToBase64String(hashBuffer);
        }

        public static int ComputeHashAsInt32(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException("value");
            }

            var buffer = Encoding.UTF8.GetBytes(value);
            var hash = ConcurrentCRC32.ComputeHash(buffer);

            return BitConverter.ToInt32(hash, 0);
        }
    }
}