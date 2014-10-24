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
    using System.Collections.Generic;
    using System.Linq;

    public static class EnumerableExtensions
    {
        public static bool AddIfNotExists<TKey, TValue>(this IDictionary<TKey, TValue> values, TKey key, TValue value)
        {
            if (values.ContainsKey(key))
            {
                return false;
            }

            values.Add(key, value);
            return true;
        }

        public static bool AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> values, TKey key, TValue value)
        {
            if (values.ContainsKey(key))
            {
                values[key] = value;
                return false;
            }

            values.Add(key, value);
            return true;
        }

        public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> values)
        {
            foreach (var value in values)
            {
                foreach (var element in value)
                {
                    yield return element;
                }
            }
        }

        public static IDictionary<T, IEnumerable<T>> Invert<T>(this IDictionary<T, T[]> dict)
        {
            var table = dict.SelectMany(x => x.Value, (dictEntry, entryElement) => new
            {
                Entity1 = dictEntry.Key,
                Entity2 = entryElement
            });

            return table.GroupBy(x => x.Entity2, x => x.Entity1, (entity2, entity1) => new
            {
                entity1,
                entity2
            }).ToDictionary(x => x.entity2, x => x.entity1);
        }
    }
}