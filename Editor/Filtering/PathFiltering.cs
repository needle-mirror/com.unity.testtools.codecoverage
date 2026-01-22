using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor.PackageManager;
using UnityEditor.TestTools.CodeCoverage.Analytics;
using UnityEditor.TestTools.CodeCoverage.Utils;

namespace UnityEditor.TestTools.CodeCoverage
{
    internal class PathFiltering
    {
        public const string kCoreAlias = "<core-pkg>";

        public string includedPaths
        {
            get;
            private set;
        }

        public string excludedPaths
        {
            get;
            private set;
        }

        private (Regex filterRegex, bool isIncluded)[] m_PathFilters;
        private bool m_HasPathFilters;
        private bool m_HasIncludedPaths;

        public PathFiltering()
        {
            m_PathFilters = Array.Empty<(Regex filterRegex, bool isIncluded)>();
            m_HasPathFilters = false;
            m_HasIncludedPaths = false;
            includedPaths = string.Empty;
            excludedPaths = string.Empty;
        }

        public void Parse(string pathsToInclude, string pathsToExclude)
        {
            var includePaths = pathsToInclude
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(path => path.Trim())
                .Where(path => path != "-") // Remove `-` entries
                .Distinct()
                .Select(path => (path, true));

            var excludePaths = pathsToExclude
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(path => path.Trim())
                .Where(path => path != "-") // Remove `-` entries
                .Distinct()
                .Select(path => (path, false));

            var pathFiltersList = excludePaths
                .Concat(includePaths)
                .ToList();

            Parse(pathFiltersList);
        }

        public void Parse(List<(String filter, bool isIncluded)> pathFiltersList)
        {
            var includedPathsList = new List<string>();
            var excludedPathsList = new List<string>();

            m_PathFilters = pathFiltersList
                .Where(f => !string.IsNullOrWhiteSpace(f.filter))
                .Distinct()
                .Select(f =>
                {
                    var filter = f.filter.Trim();
                    if (f.isIncluded)
                    {
                        includedPathsList.Add(filter);
                    }
                    else
                    {
                        excludedPathsList.Add(filter);
                    }
                    return (CreateFilterRegex(filter), f.isIncluded);
                })
                .ToArray();

            includedPaths = string.Join(",", includedPathsList);
            excludedPaths = string.Join(",", excludedPathsList);

            m_HasPathFilters = m_PathFilters.Length > 0;
            m_HasIncludedPaths = includedPathsList.Count > 0;

            CoverageAnalytics.instance.CurrentCoverageEvent.numOfIncludedPaths = includedPathsList.Count;
            CoverageAnalytics.instance.CurrentCoverageEvent.numOfExcludedPaths = excludedPathsList.Count;
        }

        public bool IsPathIncluded(string name)
        {
            if (!m_HasPathFilters)
                return true;

            name = name.ToLowerInvariant();
            name = CoverageUtils.NormaliseFolderSeparators(name, true);

            foreach (var (filterRegex, isIncluded) in m_PathFilters)
            {
                if (filterRegex.IsMatch(name))
                {
                    return isIncluded; // Return inclusion/exclusion based on order
                }
            }

            // If there are no filter matches
            if (m_HasIncludedPaths)
            {
                // And there are included paths, exclude by default
                return false;
            }
            else
            {
                // If there are no included paths, include by default
                return true;
            }
        }

        Regex CreateFilterRegex(string filter)
        {
            filter = filter.ToLowerInvariant();

            if (filter.StartsWith("<", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(filter, PathFiltering.kCoreAlias, StringComparison.OrdinalIgnoreCase))
            {
                var builtin = PackageManager.PackageInfo.GetAllRegisteredPackages()
                    .Where(p => p.source == PackageSource.BuiltIn && string.IsNullOrEmpty(p.type))
                    .Select(p =>
                        "^" + CoverageUtils.NormaliseFolderSeparators(p.resolvedPath, true)
                            .ToLowerInvariant()
                            .Replace(".", "\\."))
                    .Append("^$") // ensure that the list is not empty and will not match any file
                    .ToList();
                filter = string.Join("|", builtin);

                return new Regex(filter, RegexOptions.Compiled);
            }

            filter = CoverageUtils.NormaliseFolderSeparators(filter, true);

            return new Regex(CoverageUtils.GlobToRegex(filter), RegexOptions.Compiled);
        }
    }
}
