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
    using System.Text.RegularExpressions;

    public class DateTimeWindow
    {
        private static readonly Regex StringFormatExpression = new Regex(@"^\[(?<start>[^ ]+) TO (?<end>[^\]]+)\]$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private readonly DateTime? end;
        private readonly DateTime? start;

        public DateTimeWindow(DateTime? start, DateTimeRange range)
            : this(start, start.HasValue ? (start.Value + range) : (DateTime?)null)
        {
        }

        public DateTimeWindow(DateTime? start, DateTime? end)
        {
            this.start = start;
            this.end = end;
        }

        /// <summary>
        /// Determines how to operate in cases where the start and end times do not align with supplied bucket granularities exactly
        /// </summary>
        public enum EdgeMode
        {
            /// <summary>
            /// If the start and end times are not aligned with buckets add extra buckets to fully cover the requested window
            /// </summary>
            Grow,

            /// <summary>
            /// Shrink the window if the start and end times are not aligned with buckets and only return buckets which are wholly within the requested range
            /// </summary>
            Shrink
        }

        public DateTime? End
        {
            get { return this.end; }
        }

        public DateTime? Start
        {
            get { return this.start; }
        }

        public static DateTimeWindow Find(DateTime value, DateTimeRange range)
        {
            // TODO: this implementation is naive
            var rangeStr = range.ToString("S");
            switch (rangeStr)
            {
                case "1m":
                case "5m":
                case "15m":
                {
                    var timestamp = value.ToUnixTime();
                    var bucketTimestamp = timestamp - (timestamp % (60 * (int)range.Magnitude));

                    return new DateTimeWindow(UnixTimestamp.ToDateTime((int)bucketTimestamp), range);
                }

                case "1h":
                case "4h":
                case "8h":
                {
                    var timestamp = value.ToUnixTime();
                    var bucketTimestamp = timestamp - (timestamp % (60 * 60 * (int)range.Magnitude));

                    return new DateTimeWindow(UnixTimestamp.ToDateTime((int)bucketTimestamp), range);
                }

                case "1d":
                {
                    var timestamp = value.ToUnixTime();
                    var bucketTimestamp = timestamp - (timestamp % (60 * 60 * 24));

                    return new DateTimeWindow(UnixTimestamp.ToDateTime((int)bucketTimestamp), range);
                }

                case "1w":
                {
                    var weekStart = value.AddDays(-(int)value.DayOfWeek);
                    var weekStartTimestamp = weekStart.ToUnixTime();
                    var bucketTimestamp = weekStartTimestamp - (weekStartTimestamp % (60 * 60 * 24));

                    return new DateTimeWindow(UnixTimestamp.ToDateTime((int)bucketTimestamp), range);
                }

                case "1n":
                {
                    var monthStart = value.AddDays(1 - value.Day);
                    var monthStartTimestamp = monthStart.ToUnixTime();
                    var bucketTimestamp = monthStartTimestamp - (monthStartTimestamp % (60 * 60 * 24));

                    return new DateTimeWindow(UnixTimestamp.ToDateTime((int)bucketTimestamp), range);
                }

                case "1y":
                {
                    var yearStart = value.AddDays(1 - value.DayOfYear);
                    var yearStartTimestamp = yearStart.ToUnixTime();
                    var bucketTimestamp = yearStartTimestamp - (yearStartTimestamp % (60 * 60 * 24));

                    return new DateTimeWindow(UnixTimestamp.ToDateTime((int)bucketTimestamp), range);
                }

                default:
                    throw new ArgumentOutOfRangeException("range", "Unsupported range, '" + range + "'. Valid values are: 1m, 5m, 15m, 1h, 4h, 8h, 1d, 1w, 1n, 1y");
            }
        }

        public static IReadOnlyCollection<DateTimeWindow> GetWindowsBetween(DateTime start, DateTime end, IReadOnlyCollection<DateTimeRange> granularities, EdgeMode edges = EdgeMode.Shrink)
        {
            var result = new List<DateTimeWindow>();
            if (start >= end || granularities.Count == 0)
            {
                return result;
            }

            var ranges = granularities.OrderByDescending(g => g).ToArray();
            AppendWindowsBetween(start, end, ranges, 0, result);

            // In 'Grow' mode, ensure the returned list of buckets **at least** covers the start and end times
            if (edges == EdgeMode.Grow)
            {
                var smallestRange = ranges.Last();
                if (result.Count == 0 || result[0].start > start)
                {
                    // No buckets fit, or start bucket starts too late.
                    // Add the smallest bucket starting before the start time
                    var startWindow = DateTimeWindow.Find(start, smallestRange);
                    result.Insert(0, startWindow);
                }

                var lastWindow = result.Last();
                if (lastWindow.end < end)
                {
                    // End bucket finishes too early.
                    // Add the smallest bucket starting before the end time
                    var finalWindow = DateTimeWindow.Find(end, smallestRange);
                    result.Add(finalWindow);
                }
            }

            return result.AsReadOnly();
        }

        public static DateTimeWindow Parse(string value)
        {
            DateTimeWindow result;
            if (DateTimeWindow.TryParse(value, out result))
            {
                return result;
            }

            throw new FormatException("Invalid window format: '" + value + "'.");
        }

        public static bool TryParse(string value, out DateTimeWindow result)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException("value");
            }

            var match = StringFormatExpression.Match(value);
            if (!match.Success)
            {
                result = null;
                return false;
            }

            var startExpr = match.Groups["start"].Value;
            DateTime? start = null;
            if (startExpr != "*" && !DateMath.TryEvaluate(startExpr, out start))
            {
                result = null;
                return false;
            }

            var endExpr = match.Groups["end"].Value;
            DateTime? end = null;
            if (endExpr != "*" && !DateMath.TryEvaluate(endExpr, out end))
            {
                result = null;
                return false;
            }

            result = new DateTimeWindow(start, end);
            return true;
        }

        public static DateTimeWindow operator +(DateTimeWindow value, DateTimeRange range)
        {
            return new DateTimeWindow(value.start + range, value.end + range);
        }

        public static DateTimeWindow operator -(DateTimeWindow value, DateTimeRange range)
        {
            return new DateTimeWindow(value.start - range, value.end - range);
        }

        public override bool Equals(object obj)
        {
            var other = obj as DateTimeWindow;
            if (other == null)
            {
                return false;
            }

            return this.end.Equals(other.end) && this.start.Equals(other.start);
        }

        public override int GetHashCode()
        {
            return this.end.GetHashCode() ^ this.start.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("[{0} TO {1}]", this.start.HasValue ? this.start.Value.ToString("s") : "*", this.end.HasValue ? this.end.Value.ToString("s") : "*");
        }

        private static void AppendWindowsBetween(DateTime start, DateTime end, DateTimeRange[] ranges, int rangeIndex, ICollection<DateTimeWindow> result)
        {
            if (start >= end || rangeIndex >= ranges.Length)
            {
                return;
            }

            var currentRange = ranges[rangeIndex];

            var window = DateTimeWindow.Find(start, currentRange);
            if (window.Start < start)
            {
                if (window.Start + currentRange <= end)
                {
                    window += currentRange;

                    // Append buckets with a smaller range that are required before this one
                    AppendWindowsBetween(start, window.Start.Value, ranges, rangeIndex + 1, result);
                }
                else
                {
                    // This bucket is too large to fit in the interval, try with a smaller range
                    AppendWindowsBetween(start, end, ranges, rangeIndex + 1, result);
                    return;
                }
            }

            // Append buckets of this size until no more fit
            var windowEnd = window.Start + currentRange;
            while (windowEnd <= end)
            {
                result.Add(window);

                window += currentRange;
                windowEnd = windowEnd + currentRange;
            }

            if (window.Start < end)
            {
                // Append smaller buckets to fill the gap between bucket.Timestamp and end
                AppendWindowsBetween(window.Start.Value, end, ranges, rangeIndex + 1, result);
            }
        }
    }
}