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
    using System.Security.Cryptography;
    using System.Text;

    public static class ConcurrentMD5
    {
        [ThreadStatic]
        private static MD5 hashAlgorithm;

        public static byte[] ComputeHash(byte[] buffer)
        {
            return GetThreadLocalMD5().ComputeHash(buffer);
        }

        public static string ComputeHash(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException("value");
            }

            var buffer = Encoding.UTF8.GetBytes(value);
            var hash = ConcurrentMD5.ComputeHash(buffer);

            return Base64Encoding.ToBase64String(hash);
        }

        public static Guid ComputeHashAsGuid(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException("value");
            }

            var buffer = Encoding.UTF8.GetBytes(value);
            var hash = ConcurrentMD5.ComputeHash(buffer);

            return new Guid(hash);
        }

        private static MD5 GetThreadLocalMD5()
        {
            if (hashAlgorithm == null)
            {
                hashAlgorithm = MD5.Create();
            }

            return hashAlgorithm;
        }
    }
}