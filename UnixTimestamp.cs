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

    public static class UnixTimestamp
    {
        private static readonly DateTime UnixEpochStart = new DateTime(1970, 01, 01, 0, 0, 0, 0, DateTimeKind.Utc);

        public static DateTime ToDateTime(uint value)
        {
            return UnixTimestamp.UnixEpochStart.AddSeconds(value);
        }

        public static DateTime ToDateTime(int value)
        {
            return UnixTimestamp.UnixEpochStart.AddSeconds(value);
        }

        public static uint ToUnixTime(this DateTime value)
        {
            if (value.Kind != DateTimeKind.Utc)
            {
                value = value.ToUniversalTime();
            }

            var duration = value.Subtract(UnixTimestamp.UnixEpochStart);

            return (uint)duration.TotalSeconds;
        }
    }
}