﻿namespace UnityEditor.TestTools.CodeCoverage.OpenCover
{
    internal class OpenCoverReporterFilter : ICoverageReporterFilter
    {
        private AssemblyFiltering m_AssemblyFiltering;
        private PathFiltering m_PathFiltering;

        public void SetupFiltering()
        {
            if (!CommandLineManager.instance.runFromCommandLine || !CommandLineManager.instance.assemblyFiltersSpecified)
            {
                m_AssemblyFiltering = new AssemblyFiltering();

                string includeAssemblies = CoveragePreferences.instance.GetString("IncludeAssemblies", AssemblyFiltering.GetUserOnlyAssembliesString());
                m_AssemblyFiltering.Parse(includeAssemblies, AssemblyFiltering.kDefaultExcludedAssemblies);
            }

            if (!CommandLineManager.instance.runFromCommandLine || !CommandLineManager.instance.pathFiltersSpecified)
            {
                m_PathFiltering = new PathFiltering();

                string pathsToInclude = CoveragePreferences.instance.GetStringForPaths("PathsToInclude", string.Empty);
                string pathsToExclude = CoveragePreferences.instance.GetStringForPaths("PathsToExclude", string.Empty);

                m_PathFiltering.Parse(pathsToInclude, pathsToExclude);
            }
        }

        public bool ShouldProcessAssembly(string assemblyName)
        {
            if (CommandLineManager.instance.batchmode && !CommandLineManager.instance.useProjectSettings)
                return CommandLineManager.instance.assemblyFiltering.IsAssemblyIncluded(assemblyName);
            else
                return CommandLineManager.instance.assemblyFiltersSpecified ?
                    CommandLineManager.instance.assemblyFiltering.IsAssemblyIncluded(assemblyName) :
                    m_AssemblyFiltering.IsAssemblyIncluded(assemblyName);
        }

        public bool ShouldProcessFile(string filename)
        {
            if (CommandLineManager.instance.batchmode && !CommandLineManager.instance.useProjectSettings)
                return CommandLineManager.instance.pathFiltering.IsPathIncluded(filename);
            else
                return CommandLineManager.instance.pathFiltersSpecified ?
                    CommandLineManager.instance.pathFiltering.IsPathIncluded(filename) :
                    m_PathFiltering.IsPathIncluded(filename);
        }

        public bool ShouldGenerateAdditionalMetrics()
        {
            if (CommandLineManager.instance.batchmode && !CommandLineManager.instance.useProjectSettings)
                return CommandLineManager.instance.generateAdditionalMetrics;
            else
                return CommandLineManager.instance.generateAdditionalMetrics || CoveragePreferences.instance.GetBool("GenerateAdditionalMetrics", false);
        }

        public bool ShouldGenerateTestReferences()
        {
            if (CommandLineManager.instance.batchmode && !CommandLineManager.instance.useProjectSettings)
                return CommandLineManager.instance.generateTestReferences;
            else
                return CommandLineManager.instance.generateTestReferences || CoveragePreferences.instance.GetBool("GenerateTestReferences", false);
        }
    }
}
