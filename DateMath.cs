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
    using System.Globalization;
    using System.Text.RegularExpressions;

    public static class DateMath
    {
        private const string DateExpr = "(?<DateClause>" + NowExpr + "|" + Iso8601Date + ")(?<RoundingClause>/" + ResolutionExpr + @")?(?<OffsetClause>(?<OffsetSign>[+-])(?<OffsetMagnitude>\d+)(?<OffsetUnits>" + ResolutionExpr + "))?";
        private const string Iso8601Date = @"\d{4}-[01]\d-[0-3]\d(T[0-2]\d:[0-5]\d(:[0-5]\d(.\d{1,3})?)?)?Z?";
        private const string NowExpr = "NOW";
        private const string ResolutionExpr = "(YEAR|MONTH|DAY|HOUR|MINUTE|SECOND)S?";

        private static readonly Regex DateExpression = new Regex("^" + DateExpr + "$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static DateTime Evaluate(string value)
        {
            DateTime result;
            if (DateMath.TryEvaluate(value, out result))
            {
                return result;
            }

            throw new FormatException("Invalid format: '" + value + "'.");
        }

        public static bool TryEvaluate(string value, out DateTime result)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException("value");
            }

            result = default(DateTime);
            var match = DateExpression.Match(value);
            if (!match.Success)
            {
                return false;
            }

            var dateClause = match.Groups["DateClause"].Value;
            if (dateClause.Equals(NowExpr, StringComparison.InvariantCultureIgnoreCase))
            {
                result = DateTime.UtcNow;
            }
            else
            {
                if (!DateTime.TryParse(dateClause, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out result))
                {
                    return false;
                }
            }

            var roundingClause = match.Groups["RoundingClause"];
            if (roundingClause != null && roundingClause.Success)
            {
                var resolution = roundingClause.Value.ToLowerInvariant().TrimStart('/').TrimEnd('s') + "s";
                DateTimeUnits roundTo;
                if (!Enum.TryParse(resolution, true, out roundTo))
                {
                    return false;
                }

                result = result.Floor(roundTo);
            }

            var offsetClause = match.Groups["OffsetClause"];
            if (offsetClause != null && offsetClause.Success)
            {
                var sign = match.Groups["OffsetSign"];
                var magnitudeExpr = match.Groups["OffsetMagnitude"];
                var unitsExpr = match.Groups["OffsetUnits"];

                int magnitude;
                if (!int.TryParse(magnitudeExpr.Value, out magnitude))
                {
                    return false;
                }

                if (sign.Value == "-")
                {
                    magnitude *= -1;
                }

                DateTimeUnits units;
                var unitsString = unitsExpr.Value.ToLowerInvariant().TrimEnd('s') + "s";
                if (!Enum.TryParse(unitsString, true, out units))
                {
                    return false;
                }

                result += new DateTimeRange(magnitude, units);
            }

            return true;
        }

        public static bool TryEvaluate(string value, out DateTime? result)
        {
            DateTime r;
            var success = TryEvaluate(value, out r);

            result = r;
            return success;
        }
    }
}