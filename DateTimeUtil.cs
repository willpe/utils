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

    public static class DateTimeUtil
    {
        /// <summary>
        /// Rounds the specified date time down to the specified units.
        /// </summary>
        /// <example>
        /// (2014-02-12 10:33:24 AM, DateTimeUnits.Minute) => 2014-02-12 10:33:00 AM
        /// </example>
        public static DateTime Floor(this DateTime value, DateTimeUnits units)
        {
            switch (units)
            {
                case DateTimeUnits.Seconds:
                    return new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second, value.Kind);

                case DateTimeUnits.Minutes:
                    return new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute, 0, value.Kind);

                case DateTimeUnits.Hours:
                    return new DateTime(value.Year, value.Month, value.Day, value.Hour, 0, 0, value.Kind);

                case DateTimeUnits.Days:
                    return new DateTime(value.Year, value.Month, value.Day, 0, 0, 0, value.Kind);

                case DateTimeUnits.Weeks:
                    return new DateTime(value.Year, value.Month, value.Day, 0, 0, 0, value.Kind).AddDays(1 - (int)value.DayOfWeek);

                case DateTimeUnits.Months:
                    return new DateTime(value.Year, value.Month, 1, 0, 0, 0, value.Kind);

                case DateTimeUnits.Years:
                    return new DateTime(value.Year, 1, 1, 0, 0, 0, value.Kind);

                default:
                    throw new ArgumentOutOfRangeException("units");
            }
        }

        public static IEnumerable<DateTime> GetDaysBetween(DateTime from, DateTime to)
        {
            var toFloor = to.Floor(DateTimeUnits.Days).AddDays(1);

            for (var value = from.Floor(DateTimeUnits.Days).AddDays(1); value <= toFloor; value = value.AddDays(1))
            {
                if (value == toFloor)
                {
                    yield return to;
                }

                yield return value;
            }
        }

        /// <summary>
        /// Returns a list of the floors of the minutes from Floor(from) [exclusive] to Floor(to) [inclusive]
        /// </summary>
        /// <example>
        /// (2014-02-12 10:33:24 AM, 2014-02-12 10:37:24 AM) => [ 2014-02-12 10:33:00 AM, 2014-02-12 10:34:00 AM, 2014-02-12 10:35:00 AM, 2014-02-12 10:36:00 AM ]
        /// </example>
        public static IEnumerable<DateTime> GetMinutesBetween(DateTime from, DateTime to)
        {
            var toFloor = to.Floor(DateTimeUnits.Minutes);
            for (var value = from.Floor(DateTimeUnits.Minutes).AddMinutes(1); value <= toFloor; value = value.AddMinutes(1))
            {
                yield return value;
            }
        }

        public static DateTime? NormalizeToUtc(this DateTime? value)
        {
            if (!value.HasValue)
            {
                return null;
            }

            return value.Value.NormalizeToUtc();
        }

        public static DateTime NormalizeToUtc(this DateTime value)
        {
            return value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(value, DateTimeKind.Utc)
                : value.ToUniversalTime();
        }
    }
}