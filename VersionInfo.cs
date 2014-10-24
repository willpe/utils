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
    using System.Linq;
    using System.Reflection;

    public class VersionInfo
    {
        private static readonly object SyncRoot = new object();

        private static volatile VersionInfo current;
        private static volatile VersionInfo entryPoint;

        private readonly DateTime date;
        private readonly int major;
        private readonly string revision;
        private readonly string version;

        public VersionInfo(string version, string revision, DateTime date, int major)
        {
            this.version = version;
            this.revision = revision;
            this.date = date;
            this.major = major;
        }

        public static VersionInfo Current
        {
            get
            {
                if (VersionInfo.current == null)
                {
                    lock (VersionInfo.SyncRoot)
                    {
                        if (VersionInfo.current == null)
                        {
                            VersionInfo.current = VersionInfo.ForAssembly(typeof(VersionInfo).Assembly);
                        }
                    }
                }

                return VersionInfo.current;
            }
        }

        public static VersionInfo EntryPoint
        {
            get
            {
                if (VersionInfo.entryPoint == null)
                {
                    lock (VersionInfo.SyncRoot)
                    {
                        if (VersionInfo.entryPoint == null)
                        {
                            VersionInfo.entryPoint = VersionInfo.ForAssembly(Assembly.GetEntryAssembly());
                        }
                    }
                }

                return VersionInfo.entryPoint;
            }
        }

        public string Copyright { get; private set; }

        public DateTime Date
        {
            get { return this.date; }
        }

        public string Description { get; private set; }

        public int Major
        {
            get { return this.major; }
        }

        public string Revision
        {
            get { return this.revision; }
        }

        public string Title { get; private set; }

        public string Version
        {
            get { return this.version; }
        }

        public static VersionInfo For<T>()
        {
            var assembly = typeof(T).Assembly;
            return VersionInfo.ForAssembly(assembly);
        }

        private static VersionInfo ForAssembly(Assembly assembly)
        {
            var version = string.Empty;
            var versionAttribute = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>();
            if (versionAttribute != null)
            {
                version = versionAttribute.Version;
            }

            var revision = string.Empty;
            var date = DateTime.MinValue;
            foreach (var metadataAttribute in assembly.GetCustomAttributes<AssemblyMetadataAttribute>())
            {
                if (string.Equals(metadataAttribute.Key, "Revision", StringComparison.InvariantCultureIgnoreCase))
                {
                    revision = metadataAttribute.Value;
                }

                if (string.Equals(metadataAttribute.Key, "Date", StringComparison.InvariantCultureIgnoreCase))
                {
                    date = DateTime.Parse(metadataAttribute.Value);
                }
            }

            var major = 0;
            if (version.IndexOf('.') > 0)
            {
                var majorVersion = version.Remove(version.IndexOf('.'));
                int.TryParse(majorVersion, out major);
            }

            var result = new VersionInfo(version, revision, date, major);

            var titleAttribute = assembly.GetCustomAttributes<AssemblyTitleAttribute>().FirstOrDefault();
            if (titleAttribute != null)
            {
                result.Title = titleAttribute.Title;
            }

            var copyrightAttribute = assembly.GetCustomAttributes<AssemblyCopyrightAttribute>().FirstOrDefault();
            if (copyrightAttribute != null)
            {
                result.Copyright = copyrightAttribute.Copyright;
            }

            var descriptionAttribute = assembly.GetCustomAttributes<AssemblyDescriptionAttribute>().FirstOrDefault();
            if (descriptionAttribute != null)
            {
                result.Description = descriptionAttribute.Description;
            }

            return result;
        }
    }
}