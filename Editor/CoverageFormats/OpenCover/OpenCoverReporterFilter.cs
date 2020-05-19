using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.TestTools.CodeCoverage.OpenCover
{
    internal class OpenCoverReporterFilter : ICoverageReporterFilter
    {
        private AssemblyFiltering m_AssemblyFiltering;

        public void SetupFiltering()
        {
            m_AssemblyFiltering = new AssemblyFiltering();

            string includeAssemblies = CoveragePreferences.instance.GetString("IncludeAssemblies", AssemblyFiltering.GetUserOnlyAssembliesString());

            m_AssemblyFiltering.Parse(includeAssemblies, AssemblyFiltering.kDefaultExcludedAssemblies);
        }

        public bool ShouldProcessAssembly(string assemblyName)
        {
            if (CommandLineManager.instance.runFromCommandLine)
                return CommandLineManager.instance.assemblyFiltering.IsAssemblyIncluded(assemblyName);

            return m_AssemblyFiltering.IsAssemblyIncluded(assemblyName);
        }

        public bool ShouldProcessFile(string filename)
        {
            // PathFiltering is implemented only via the command line.
            // Will assess whether PathFiltering is needed to be set via the UI too (similar to Assembly Filtering).
            if (CommandLineManager.instance.runFromCommandLine)
                return CommandLineManager.instance.pathFiltering.IsPathIncluded(filename);
            else
                return true;
        }

        public bool ShouldGenerateAdditionalMetrics()
        {
            bool shouldGenerateAdditionalMetrics = CommandLineManager.instance.generateAdditionalMetrics;

            if (!shouldGenerateAdditionalMetrics)
            {
                shouldGenerateAdditionalMetrics = CoveragePreferences.instance.GetBool("GenerateAdditionalMetrics", false);
            }
            return shouldGenerateAdditionalMetrics;
        }
    }
}
