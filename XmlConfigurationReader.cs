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
    using System.Linq;
    using System.Reflection;
    using System.Xml;

    public abstract class XmlConfigurationReader<T>
    {
        protected delegate bool ParseFunc<TOut>(string value, out TOut result);

        public T Load(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path");
            }

            if (!Path.IsPathRooted(path))
            {
                var homeDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (string.IsNullOrEmpty(homeDirectory))
                {
                    throw new IOException("Cannot determine full path for '" + path + "'");
                }

                path = Path.Combine(homeDirectory, path);
            }

            var xmlDocument = new XmlDocument();
            xmlDocument.Load(path);

            return this.Load(xmlDocument, path);
        }

        public T Parse(string xml)
        {
            if (string.IsNullOrEmpty(xml))
            {
                throw new ArgumentNullException("xml");
            }

            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xml);

            return this.Load(xmlDocument);
        }

        protected abstract T Load(XmlDocument xmlDocument, string sourceFile = null);

        protected TOut TryGetAttributeValue<TOut>(XmlElement element, string name, ParseFunc<TOut> tryParse, TOut defaultValue = default(TOut))
        {
            TOut result;

            var attrValue = element.GetAttribute(name);
            if (!string.IsNullOrEmpty(attrValue) && tryParse(attrValue, out result))
            {
                return result;
            }

            return defaultValue;
        }

        protected bool TryParseDateTimeRangeArray(string value, out DateTimeRange[] result)
        {
            string[] values;
            if (this.TryParseStringArray(value, out values))
            {
                result = new DateTimeRange[values.Length];
                for (var i = 0; i < values.Length; i++)
                {
                    if (!DateTimeRange.TryParse(values[i], out result[i]))
                    {
                        return false;
                    }
                }

                return true;
            }

            result = new DateTimeRange[0];
            return false;
        }

        protected bool TryParseStringArray(string value, out string[] result)
        {
            if (string.IsNullOrEmpty(value))
            {
                result = null;
                return false;
            }

            if (value.Contains(","))
            {
                result = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .ToArray();

                return true;
            }

            result = new[] { value.Trim() };
            return true;
        }
    }
}