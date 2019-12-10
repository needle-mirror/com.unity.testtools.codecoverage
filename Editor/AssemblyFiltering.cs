using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor.Compilation;
using UnityEditor.TestTools.CodeCoverage.Utils;
using System.Collections.Generic;

namespace UnityEditor.TestTools.CodeCoverage
{
    internal class AssemblyFiltering
    {
        static public string kDefaultExcludedAssemblies = "system*,mono*,nunit*,microsoft*,mscorlib*,roslyn*";

        public string includedAssemblies
        {
            get;
            private set;
        }

        public string excludedAssemblies
        {
            get;
            private set;
        }

        private Regex[] m_IncludeAssemblies;
        private Regex[] m_ExcludeAssemblies;

        public AssemblyFiltering()
        {
            m_IncludeAssemblies = new Regex[] { };
            m_ExcludeAssemblies = new Regex[] { };
        }

        public void Parse(string includeAssemblies, string excludeAssemblies)
        {
            includedAssemblies = includeAssemblies;
            excludedAssemblies = excludeAssemblies;

            string[] includeAssemblyFilters = includeAssemblies.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string[] excludeAssemblyFilters = excludeAssemblies.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            m_IncludeAssemblies = includeAssemblyFilters
                .Select(f => CreateFilterRegex(f))
                .ToArray();

            m_ExcludeAssemblies = excludeAssemblyFilters
                .Select(f => CreateFilterRegex(f))
                .ToArray();
        }

        public bool IsAssemblyIncluded(string name)
        {
            name = name.ToLowerInvariant();

            if (m_ExcludeAssemblies.Any(f => f.IsMatch(name)))
            {
                return false;
            }
            else
            {
                return m_IncludeAssemblies.Any(f => f.IsMatch(name));
            }
        }

        public static Assembly[] GetAllProjectAssemblies()
        {
            Assembly[] assemblies = CompilationPipeline.GetAssemblies();
            Array.Sort(assemblies, (x, y) => String.Compare(x.name, y.name));
            return assemblies;
        }

        public static string GetAllProjectAssembliesString()
        {
            Assembly[] assemblies = AssemblyFiltering.GetAllProjectAssemblies();

            string assembliesString = "";
            int assembliesLength = assemblies.Length;
            for (int i=0; i<assembliesLength; ++i)
            {
                assembliesString += assemblies[i].name;
                if (i < assembliesLength - 1)
                    assembliesString += ",";
            }

            return assembliesString;
        }

        public static string GetUserOnlyAssembliesString()
        {
            Assembly[] assemblies = AssemblyFiltering.GetAllProjectAssemblies();
            List<string> userAssemblies = new List<string>();

            string assembliesString = "";
            int assembliesLength = assemblies.Length;
            int i;
            for (i = 0; i < assembliesLength; ++i)
            {
                string name = assemblies[i].name;
                string[] sourceFiles = assemblies[i].sourceFiles;

                if (sourceFiles.Length > 0 &&
                    sourceFiles[0].StartsWith("Assets"))
                {
                    userAssemblies.Add(name);
                }
            }

            int userAssembliesLength = userAssemblies.Count;
            for (i = 0; i < userAssembliesLength; ++i)
            {
                assembliesString += userAssemblies[i];
                if (i < userAssembliesLength - 1)
                    assembliesString += ",";
            }

            return assembliesString;
        }

        public static Regex CreateFilterRegex(string filter)
        {
            filter = filter.ToLowerInvariant();

            return new Regex(CoverageUtils.GlobToRegex(filter), RegexOptions.Compiled);
        }
    }
}
