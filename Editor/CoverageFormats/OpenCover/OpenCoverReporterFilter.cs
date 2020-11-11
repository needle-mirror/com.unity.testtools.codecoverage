namespace UnityEditor.TestTools.CodeCoverage.OpenCover
{
    internal class OpenCoverReporterFilter : ICoverageReporterFilter
    {
        private AssemblyFiltering m_AssemblyFiltering;
        private PathFiltering m_PathFiltering;

        public void SetupFiltering()
        {
            if (CommandLineManager.instance.runFromCommandLine)
                return;

            m_AssemblyFiltering = new AssemblyFiltering();

            string includeAssemblies = CoveragePreferences.instance.GetString("IncludeAssemblies", AssemblyFiltering.GetUserOnlyAssembliesString());
            m_AssemblyFiltering.Parse(includeAssemblies, AssemblyFiltering.kDefaultExcludedAssemblies);

            m_PathFiltering = new PathFiltering();

            string pathsToInclude = CoveragePreferences.instance.GetStringForPaths("PathsToInclude", string.Empty);
            string pathsToExclude = CoveragePreferences.instance.GetStringForPaths("PathsToExclude", string.Empty);

            m_PathFiltering.Parse(pathsToInclude, pathsToExclude);
        }

        public bool ShouldProcessAssembly(string assemblyName)
        {
            if (CommandLineManager.instance.runFromCommandLine)
                return CommandLineManager.instance.assemblyFiltering.IsAssemblyIncluded(assemblyName);
            else
                return m_AssemblyFiltering.IsAssemblyIncluded(assemblyName);
        }

        public bool ShouldProcessFile(string filename)
        {
            if (CommandLineManager.instance.runFromCommandLine)
                return CommandLineManager.instance.pathFiltering.IsPathIncluded(filename);
            else
                return m_PathFiltering.IsPathIncluded(filename);
        }

        public bool ShouldGenerateAdditionalMetrics()
        {
            if (CommandLineManager.instance.runFromCommandLine)
                return CommandLineManager.instance.generateAdditionalMetrics;
            else
                return CoveragePreferences.instance.GetBool("GenerateAdditionalMetrics", false);
        }
    }
}
