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
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Utility class for reading and writing objects to CSV files.
    /// </summary>
    /// <remarks>
    /// Input files are required to have a header row.
    /// </remarks>
    public static class CsvSerializer
    {
        // Matches individual values in a CSV row where those values may be enclosed in "double quotes"
        private static readonly Regex csvRowExpression = new Regex(@"^((""(?<value>[^""]*)""|(?<value>[^""\,]*)),)*(""(?<value>[^""]*)""|(?<value>[^""\,]*))$", RegexOptions.Compiled);

        /// <summary>
        /// Returns the rows in the CSV file deserialized as instances of <typeparamref name="TRow"/>.
        /// </summary>
        /// <typeparam name="TRow">The type of the object which each row can be deserialized into.</typeparam>
        public static IEnumerable<TRow> Deserialize<TRow>(string path) where TRow : new()
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path");
            }

            var rawRows = File.ReadAllLines(path);

            if (rawRows.Length < 2)
            {
                return new TRow[0];
            }

            return Deserialize<TRow>(rawRows);
        }

        /// <summary>
        /// Gets the headers from the CSV file
        /// </summary>
        public static IEnumerable<string> ReadHeaders(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path");
            }

            var rawRows = File.ReadAllLines(path);
            if (rawRows.Length < 1)
            {
                return new string[0];
            }

            return ReadHeaders(rawRows);
        }

        /// <summary>
        /// Gets the data rows from the CSV file
        /// </summary>
        public static IEnumerable<IDictionary<string, string>> ReadRows(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path");
            }

            var rawRows = File.ReadAllLines(path);
            if (rawRows.Length < 2)
            {
                return new IDictionary<string, string>[0];
            }

            return ReadRows(rawRows);
        }

        /// <summary>
        /// Returns the rows in the CSV file deserialized as instances of <typeparamref name="TRow"/>.
        /// </summary>
        /// <typeparam name="TRow">The type of the object which each row can be deserialized into.</typeparam>
        public static void Serialize<TRow>(IEnumerable<TRow> rows, string path)
        {
            using (var file = new StreamWriter(path))
            {
                Serialize(rows, file);
                file.Flush();
            }
        }

        public static void Serialize<TRow>(IEnumerable<TRow> rows, TextWriter writer)
        {
            Serialize(rows, writer, typeof(TRow));
        }

        public static void Serialize(IEnumerable rows, TextWriter writer, Type rowType)
        {
            var columns = PropertyMetadata.GetPropertiesToSerialize(rowType);

            for (var i = 0; i < columns.Length; i++)
            {
                writer.Write(columns[i].Name);

                if (i < columns.Length - 1)
                {
                    writer.Write(",");
                }
            }

            writer.WriteLine();

            foreach (var row in rows)
            {
                for (var i = 0; i < columns.Length; i++)
                {
                    writer.Write(@"""{0}""", columns[i].GetValue(row));

                    if (i < columns.Length - 1)
                    {
                        writer.Write(",");
                    }
                }

                writer.WriteLine();
            }
        }

        private static IEnumerable<TRow> Deserialize<TRow>(string[] fileLines) where TRow : new()
        {
            var headers = ReadHeaders(fileLines);
            var rows = ReadRows(fileLines);
            var propertiesToDeserialize = PropertyMetadata.FindPropertiesToDeserialize<TRow>(headers).ToArray();

            foreach (var row in rows)
            {
                var deserializedRow = new TRow();
                foreach (var property in propertiesToDeserialize)
                {
                    property.SetValue(deserializedRow, row[property.Name]);
                }

                yield return deserializedRow;
            }
        }

        private static string[] ParseCsvRow(string row)
        {
            var rowRegexMatch = csvRowExpression.Match(row);
            var captures = rowRegexMatch.Groups["value"].Captures.OfType<Capture>();

            return captures.Select(c => c.Value).ToArray();
        }

        private static string[] ReadHeaders(IEnumerable<string> fileLines)
        {
            var headerRow = fileLines.First();
            return ParseCsvRow(headerRow).Select(h => h.Trim()).ToArray();
        }

        private static IEnumerable<IDictionary<string, string>> ReadRows(string[] fileLines)
        {
            var headers = ReadHeaders(fileLines);
            var dataRows = fileLines.Skip(1).ToArray();

            foreach (var dataRow in dataRows)
            {
                var row = new Dictionary<string, string>(headers.Length);
                var dataRowColumns = ParseCsvRow(dataRow);
                for (var i = 0; i < dataRowColumns.Length; i++)
                {
                    row.Add(headers[i], dataRowColumns[i]);
                }

                yield return row;
            }
        }

        private sealed class PropertyMetadata
        {
            private readonly MethodInfo formatMethod;
            private readonly string name;
            private readonly MethodInfo parseMethod;
            private readonly PropertyInfo property;

            private PropertyMetadata(string name, PropertyInfo property, MethodInfo parseMethod = null, MethodInfo formatMethod = null)
            {
                this.name = name;
                this.property = property;
                this.parseMethod = parseMethod;
                this.formatMethod = formatMethod;
            }

            public string Name
            {
                get { return this.name; }
            }

            public static IEnumerable<PropertyMetadata> FindPropertiesToDeserialize<TRow>(IReadOnlyCollection<string> headers)
            {
                //// Finds each property on TRow which maps to a named column in the csv file being 
                //// read. Properties can specify a column name to read from which is not equal to the
                //// property's name by specifying the 'Column' attribute:
                ////
                ////    [Column("Sum")]
                ////    public int AmountInBytes { get; set; }
                ////
                //// If the value from the CSV file needs additional parsing/processing, an additional
                //// member - parseMethod - can be specified in the attribute. This method specifies
                //// a static method to parse the content of the column:
                //// 
                ////    [Column("GroupBy", ParseMethod = "DeserializeDataTransferDirectionFromGroupBy")]
                ////    public string DataTransferDirection { get; set; }
                //// 
                ////    private static string DeserializeDataTransferDirectionFromGroupBy(string value)
                ////    {
                ////        return value.Split('|')[0].ToUpperCase();
                ////    }

                var properties = typeof(TRow).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (var property in properties)
                {
                    var columnNameOverride = property.GetCustomAttribute<ColumnAttribute>();
                    if (columnNameOverride != null)
                    {
                        if (headers.Contains(columnNameOverride.Name))
                        {
                            MethodInfo parseMethod = null;
                            if (!string.IsNullOrEmpty(columnNameOverride.ParseMethod))
                            {
                                parseMethod = typeof(TRow).GetMethod(columnNameOverride.ParseMethod, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                            }

                            yield return new PropertyMetadata(columnNameOverride.Name, property, parseMethod: parseMethod);
                        }
                    }
                    else
                    {
                        if (headers.Contains(property.Name))
                        {
                            yield return new PropertyMetadata(property.Name, property);
                        }
                    }
                }
            }

            public static PropertyMetadata[] GetPropertiesToSerialize(Type rowType)
            {
                //// Finds each property on TRow which maps to a named column in the csv file being 
                //// read. Properties can specify a column name to read from which is not equal to the
                //// property's name by specifying the 'Column' attribute:
                ////
                ////    [Column("Sum")]
                ////    public int AmountInBytes { get; set; }
                ////
                //// If the value from the CSV file needs additional parsing/processing, an additional
                //// member - formatMethod - can be specified in the attribute. This method specifies
                //// a static method to format the content of the column:
                //// 
                ////    [Column("GroupBy", FormatMethod = "DeserializeDataTransferDirectionFromGroupBy")]
                ////    public int Quantity { get; set; }
                //// 
                ////    private static string DeserializeDataTransferDirectionFromGroupBy(int value)
                ////    {
                ////        return value + " MB";
                ////    }

                var result = new List<PropertyMetadata>();

                var properties = rowType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (var property in properties)
                {
                    var columnNameOverride = property.GetCustomAttribute<ColumnAttribute>();
                    if (columnNameOverride != null)
                    {
                        MethodInfo formatMethod = null;
                        if (!string.IsNullOrEmpty(columnNameOverride.FormatMethod))
                        {
                            formatMethod = rowType.GetMethod(columnNameOverride.FormatMethod, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                        }

                        result.Add(new PropertyMetadata(columnNameOverride.Name, property, formatMethod: formatMethod));
                    }
                    else
                    {
                        result.Add(new PropertyMetadata(property.Name, property));
                    }
                }

                return result.ToArray();
            }

            public string GetValue(object obj)
            {
                //// Converts the type of 'value' to match the property's type and sets the property
                //// value. Note that TimeSpans receive special treatment, and DateTimes are always
                //// forced to be read as UTC.

                if (obj == null)
                {
                    throw new ArgumentNullException("obj");
                }

                var value = this.property.GetValue(obj, null);

                if (this.formatMethod != null)
                {
                    return (string)this.parseMethod.Invoke(null, new[] { value });
                }

                if (value == null)
                {
                    return string.Empty;
                }

                return value.ToString();
            }

            public void SetValue(object obj, object value)
            {
                //// Converts the type of 'value' to match the property's type and sets the property
                //// value. Note that TimeSpans receive special treatment, and DateTimes are always
                //// forced to be read as UTC.

                var typedValue = this.parseMethod != null
                    ? this.parseMethod.Invoke(null, new[] { value })
                    : this.ParseValue(value);

                this.property.SetValue(obj, typedValue, null);
            }

            public override string ToString()
            {
                return string.Format("{0} ({1})", this.name, this.property.PropertyType.Name);
            }

            private object ParseValue(object value)
            {
                if (this.property.PropertyType == typeof(TimeSpan))
                {
                    return TimeSpan.Parse(value.ToString());
                }

                if (this.property.PropertyType.IsEnum)
                {
                    return Enum.Parse(this.property.PropertyType, value.ToString());
                }

                var result = Convert.ChangeType(value, this.property.PropertyType);
                if (result is DateTime)
                {
                    result = DateTime.SpecifyKind((DateTime)result, DateTimeKind.Utc);
                }

                return result;
            }
        }
    }
}