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
    using System.Text.RegularExpressions;

    public struct DateTimeRange : IFormatProvider, ICustomFormatter, IComparable<DateTimeRange>
    {
        public static readonly DateTimeRange None = new DateTimeRange();
        private static readonly Regex generalFormatExpression = new Regex(@"^(?<magnitude>([+-]?)\d+(\.\d+)?)\s*(?<units>[a-zA-Z]+)$");
        private static readonly Regex shortFormatExpression = new Regex(@"^(?<magnitude>([+-]?)\d+(\.\d+)?)(?<units>[smhdwny])$");

        private readonly double magnitude;
        private readonly DateTimeUnits units;

        public DateTimeRange(double magnitude, DateTimeUnits units)
            : this()
        {
            this.magnitude = magnitude;
            this.units = units;
        }

        public double Magnitude
        {
            get { return this.magnitude; }
        }

        public DateTimeUnits Units
        {
            get { return this.units; }
        }

        public static DateTimeRange Parse(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException("value");
            }

            value = value.ToLowerInvariant();

            var match = shortFormatExpression.Match(value);
            if (match.Success)
            {
                return ParseShort(match);
            }

            match = generalFormatExpression.Match(value);
            if (match.Success)
            {
                return ParseGeneral(match);
            }

            throw new FormatException("The value '" + value + "' does not represent a date time range.");
        }

        public static bool TryParse(string value, out DateTimeRange result)
        {
            if (string.IsNullOrEmpty(value))
            {
                result = default(DateTimeRange);
                return false;
            }

            value = value.ToLowerInvariant();

            var match = shortFormatExpression.Match(value);
            if (match.Success)
            {
                result = ParseShort(match);
                return true;
            }

            match = generalFormatExpression.Match(value);
            if (match.Success)
            {
                result = ParseGeneral(match);
                return true;
            }

            result = default(DateTimeRange);
            return false;
        }

        public static DateTime operator +(DateTime value, DateTimeRange range)
        {
            switch (range.Units)
            {
                case DateTimeUnits.Seconds:
                    return value.AddSeconds(range.magnitude);
                case DateTimeUnits.Minutes:
                    return value.AddMinutes(range.magnitude);
                case DateTimeUnits.Hours:
                    return value.AddHours(range.magnitude);
                case DateTimeUnits.Days:
                    return value.AddDays(range.magnitude);
                case DateTimeUnits.Weeks:
                    return value.AddDays(7 * range.magnitude);
                case DateTimeUnits.Months:
                    return value.AddMonths((int)range.magnitude);
                case DateTimeUnits.Years:
                    return value.AddYears((int)range.magnitude);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static implicit operator TimeSpan(DateTimeRange value)
        {
            switch (value.Units)
            {
                case DateTimeUnits.Seconds:
                    return TimeSpan.FromSeconds(value.magnitude);
                case DateTimeUnits.Minutes:
                    return TimeSpan.FromMinutes(value.magnitude);
                case DateTimeUnits.Hours:
                    return TimeSpan.FromHours(value.magnitude);
                case DateTimeUnits.Days:
                    return TimeSpan.FromDays(value.magnitude);
                case DateTimeUnits.Weeks:
                    return TimeSpan.FromDays(7 * value.magnitude);
                case DateTimeUnits.Months:
                case DateTimeUnits.Years:
                    throw new NotSupportedException("Cannot convert a time range specified in '" + value.units + "' to a TimeSpan");
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static implicit operator DateTimeRange(string value)
        {
            return DateTimeRange.Parse(value);
        }

        public static DateTime operator -(DateTime value, DateTimeRange range)
        {
            return value + (-range);
        }

        public static DateTimeRange operator -(DateTimeRange value)
        {
            return new DateTimeRange(-value.magnitude, value.units);
        }

        public int CompareTo(DateTimeRange other)
        {
            var resolutionDiff = (int)this.units - (int)other.units;
            if (resolutionDiff != 0)
            {
                return resolutionDiff;
            }

            return (int)((this.magnitude - other.magnitude) * 1000);
        }

        public override bool Equals(object obj)
        {
            if (obj is DateTimeRange)
            {
                var other = (DateTimeRange)obj;
                return this.units.Equals(other.units) && this.magnitude.Equals(other.magnitude);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return (int)((int)this.units ^ BitConverter.DoubleToInt64Bits(this.magnitude));
        }

        public override string ToString()
        {
            return this.magnitude + " " + this.units;
        }

        public string ToString(string format)
        {
            return ((ICustomFormatter)this).Format(format, this, this);
        }

        string ICustomFormatter.Format(string format, object arg, IFormatProvider formatProvider)
        {
            var value = (DateTimeRange)arg;

            // Handle null or empty format string, string with precision specifier. 
            var fmt = string.Empty;

            // Extract first character of format string (precision specifiers 
            // are not supported). 
            if (!string.IsNullOrEmpty(format))
            {
                fmt = format.Length > 1 ? format.Substring(0, 1) : format;
            }

            switch (fmt.ToUpperInvariant())
            {
                case "S":
                {
                    string resolutionChar;
                    switch (value.units)
                    {
                        case DateTimeUnits.Seconds:
                        case DateTimeUnits.Minutes:
                        case DateTimeUnits.Hours:
                        case DateTimeUnits.Days:
                        case DateTimeUnits.Weeks:
                        case DateTimeUnits.Years:
                            resolutionChar = value.units.ToString().Remove(1).ToLowerInvariant();
                            break;
                        case DateTimeUnits.Months:
                            resolutionChar = "n";
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    return value.Magnitude + resolutionChar;
                }
                default:
                    return value.ToString();
            }
        }

        object IFormatProvider.GetFormat(Type formatType)
        {
            return this;
        }

        private static DateTimeRange ParseGeneral(Match match)
        {
            var quantityStr = match.Groups["magnitude"].Value;
            var resolutionStr = match.Groups["units"].Value.ToLowerInvariant().TrimEnd('s') + 's';

            DateTimeUnits units;
            if (!Enum.TryParse(resolutionStr, true, out units))
            {
                throw new FormatException("'" + resolutionStr + "' is not a valid units. specify one of [Seconds, Minutes, Hours, Days, Months, Years].");
            }

            var quantity = double.Parse(quantityStr);
            return new DateTimeRange(quantity, units);
        }

        private static DateTimeRange ParseShort(Match match)
        {
            var quantityStr = match.Groups["magnitude"].Value;
            var resolutionStr = match.Groups["units"].Value;

            DateTimeUnits units;
            switch (resolutionStr.ToLowerInvariant())
            {
                case "s":
                    units = DateTimeUnits.Seconds;
                    break;

                case "m":
                    units = DateTimeUnits.Minutes;
                    break;

                case "h":
                    units = DateTimeUnits.Hours;
                    break;

                case "d":
                    units = DateTimeUnits.Days;
                    break;

                case "w":
                    units = DateTimeUnits.Weeks;
                    break;

                case "n":
                    units = DateTimeUnits.Months;
                    break;

                case "y":
                    units = DateTimeUnits.Years;
                    break;

                default:
                    throw new FormatException("'" + resolutionStr + "' is not a valid units. specify one of [Seconds, Minutes, Hours, Days, Months, Years].");
            }

            var quantity = double.Parse(quantityStr);
            return new DateTimeRange(quantity, units);
        }
    }
}