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

    public static class ConcurrentRandom
    {
        private static readonly Random SeedGenerator = new Random();

        [ThreadStatic]
        private static Random random;

        public static int Next(int minValue, int maxValue)
        {
            if (ConcurrentRandom.random == null)
            {
                int seed;
                lock (ConcurrentRandom.SeedGenerator)
                {
                    seed = ConcurrentRandom.SeedGenerator.Next();
                }

                ConcurrentRandom.random = new Random(seed);
            }

            return ConcurrentRandom.random.Next(minValue, maxValue);
        }

        public static double NextDouble()
        {
            if (ConcurrentRandom.random == null)
            {
                int seed;
                lock (ConcurrentRandom.SeedGenerator)
                {
                    seed = ConcurrentRandom.SeedGenerator.Next();
                }

                ConcurrentRandom.random = new Random(seed);
            }

            return ConcurrentRandom.random.NextDouble();
        }
    }
}