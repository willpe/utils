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
    using System.Configuration;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    public sealed class ConnectionString
    {
        private const string KeyExpression = @"\w+";
        private const string PropertyExpression = "(?<key>" + KeyExpression + ")=(?<value>(" + ValueExpression + "|" + QuotedValueExpression + "))(;|$|;$)";
        private const string QuotedValueExpression = @"('([^']|\\')+'|""([^""]|\\"")+"")";
        private const string ValueExpression = @"[^;]+";

        private static readonly Regex PropertyRegularExpression = new Regex(PropertyExpression, RegexOptions.ExplicitCapture);

        private readonly Dictionary<string, string> properties;

        private ConnectionString()
        {
            this.properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public string this[string key]
        {
            get
            {
                if (this.properties.ContainsKey(key))
                {
                    return this.properties[key];
                }

                return null;
            }
        }

        public static ConnectionString Parse(string connectionString)
        {
            ConnectionString result;
            if (ConnectionString.TryParse(connectionString, out result))
            {
                return result;
            }

            throw new ArgumentException("The connection string '" + connectionString + "' is not valid.", "connectionString");
        }

        public static bool TryParse(string connectionString, out ConnectionString value)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }

            if (ConfigurationManager.ConnectionStrings[connectionString] != null)
            {
                connectionString = ConfigurationManager.ConnectionStrings[connectionString].ConnectionString;
            }

            var matches = PropertyRegularExpression.Matches(connectionString);
            if (matches.Count == 0)
            {
                value = null;
                return false;
            }

            value = new ConnectionString();
            foreach (var match in matches.OfType<Match>())
            {
                var key = match.Groups["key"].Value;
                var val = match.Groups["value"].Value;
                if (val.StartsWith("'") && val.EndsWith("'") && val.Length > 2)
                {
                    val = val.Substring(1, val.Length - 2).Replace("\\'", "'");
                }

                if (val.StartsWith("\"") && val.EndsWith("\"") && val.Length > 2)
                {
                    val = val.Substring(1, val.Length - 2).Replace("\\\"", "\"");
                }

                value.properties.Add(key, val);
            }

            return true;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            var sortedKeys = this.properties.Keys.Select(k => k.ToLowerInvariant()).OrderBy(k => k);
            foreach (var propertyName in sortedKeys)
            {
                builder.AppendFormat("{0}={1};", propertyName, this.properties[propertyName]);
            }

            return builder.ToString();
        }
    }
}