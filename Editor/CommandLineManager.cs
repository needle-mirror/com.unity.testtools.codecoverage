using System;
using UnityEditor.TestTools.CodeCoverage.CommandLineParser;
using UnityEditor.TestTools.CodeCoverage.Utils;

namespace UnityEditor.TestTools.CodeCoverage
{
    internal class CommandLineManager : CommandLineManagerImplementation
    {
        private static CommandLineManager s_Instance = null;

        public static CommandLineManager instance
        {
            get
            {
                if (s_Instance == null)
                    s_Instance = new CommandLineManager();

                return s_Instance;
            }
        }

        protected CommandLineManager() : base(Environment.GetCommandLineArgs())
        {
        }
    }

    internal class CommandLineManagerImplementation
    {
        public bool runFromCommandLine
        {
            get;
            private set;
        }

        public string coverageResultsPath
        {
            get;
            private set;
        }

        public string coverageHistoryPath
        {
            get;
            private set;
        }

        public bool generateAdditionalMetrics
        {
            get;
            private set;
        }

        public bool generateHTMLReportHistory
        {
            get;
            private set;
        }

        public bool generateHTMLReport
        {
            get;
            private set;
        }

        public bool generateBadgeReport
        {
            get;
            private set;
        }

        public bool assemblyFiltersSpecified
        {
            get;
            private set;
        }

        public bool pathFiltersSpecified
        {
            get;
            private set;
        }

        public AssemblyFiltering assemblyFiltering
        {
            get;
            private set;
        }

        public PathFiltering pathFiltering
        {
            get;
            private set;
        }

        public bool runTests
        {
            get;
            private set;
        }

        private string m_CoverageOptionsArg;
        private string[] m_CoverageOptions;
        private string m_IncludeAssemblies;
        private string m_ExcludeAssemblies;
        private string m_IncludePaths;
        private string m_ExcludePaths;

        public CommandLineManagerImplementation(string[] commandLineArgs)
        {
            runFromCommandLine = false;
            coverageResultsPath = string.Empty;
            coverageHistoryPath = string.Empty;
            generateAdditionalMetrics = false;
            generateHTMLReportHistory = false;
            generateHTMLReport = false;
            generateBadgeReport = false;
            assemblyFiltersSpecified = false;
            pathFiltersSpecified = false;
            assemblyFiltering = new AssemblyFiltering();
            pathFiltering = new PathFiltering();
            runTests = false;

            m_CoverageOptionsArg = string.Empty;
            m_CoverageOptions = new string[] { };
            m_IncludeAssemblies = string.Empty;
            m_ExcludeAssemblies = string.Empty;
            m_IncludePaths = string.Empty;
            m_ExcludePaths = string.Empty;

            CommandLineOptionSet optionSet = new CommandLineOptionSet(
                new CommandLineOption("enableCodeCoverage", () => { runFromCommandLine = true; }),
                new CommandLineOption("coverageResultsPath", filePathArg => { SetCoverageResultsPath(filePathArg); }),
                new CommandLineOption("coverageHistoryPath", filePathArg => { SetCoverageHistoryPath(filePathArg); }),
                new CommandLineOption("coverageOptions", optionsArg => { AddCoverageOptions(optionsArg); }),
                new CommandLineOption("runTests", () => { runTests = true; })
            );
            optionSet.Parse(commandLineArgs);

            ValidateCoverageResultsPath();
            ValidateCoverageHistoryPath();

            ParseCoverageOptions();
        }

        private void SetCoverageResultsPath(string filePathArg)
        {
            if (coverageResultsPath != string.Empty)
            {
                ResultsLogger.Log(ResultID.Warning_MultipleResultsPaths, coverageResultsPath);
            }
            else
            {
                if (filePathArg != null)
                {
                    coverageResultsPath = filePathArg;
                }
            }
        }

        private void ValidateCoverageResultsPath()
        {
            if (!CoverageUtils.EnsureFolderExists(coverageResultsPath))
                coverageResultsPath = string.Empty;
        }

        private void SetCoverageHistoryPath(string filePathArg)
        {
            if (coverageHistoryPath != string.Empty)
            {
                ResultsLogger.Log(ResultID.Warning_MultipleHistoryPaths, coverageHistoryPath);
            }
            else
            {
                if (filePathArg != null)
                {
                    coverageHistoryPath = filePathArg;
                }
            }
        }

        private void ValidateCoverageHistoryPath()
        {
            if (!CoverageUtils.EnsureFolderExists(coverageHistoryPath))
                coverageHistoryPath = string.Empty;
        }

        private void AddCoverageOptions(string coverageOptionsArg)
        {
            if (coverageOptionsArg != null)
            {
                coverageOptionsArg = coverageOptionsArg.Trim('\'');

                if (coverageOptionsArg != string.Empty)
                {
                    if (m_CoverageOptionsArg == string.Empty)
                    {
                        m_CoverageOptionsArg = coverageOptionsArg;
                    }
                    else
                    {
                        m_CoverageOptionsArg += ";";
                        m_CoverageOptionsArg += coverageOptionsArg;
                    }
                }
            }
        }

        private void ParseCoverageOptions()
        {
            m_CoverageOptions = m_CoverageOptionsArg.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string optionArgsStr in m_CoverageOptions)
            {
                if (optionArgsStr.Length == 0)
                    continue;

                string optionName = optionArgsStr;
                string optionArgs = string.Empty;

                int indexOfColon = optionArgsStr.IndexOf(':');
                if (indexOfColon > 0)
                {
                    optionName = optionArgsStr.Substring(0, indexOfColon);
                    optionArgs = optionArgsStr.Substring(indexOfColon+1);
                }        

                switch (optionName.ToUpperInvariant())
                {
                    case "GENERATEADDITIONALMETRICS":
                        generateAdditionalMetrics = true;
                        break;

                    case "GENERATEHTMLREPORTHISTORY":
                        generateHTMLReportHistory = true;
                        break;

                    case "GENERATEHTMLREPORT":
                        generateHTMLReport = true;
                        break;

                    case "GENERATEBADGEREPORT":
                        generateBadgeReport = true;
                        break;

                    case "ASSEMBLYFILTERS":
                        if (optionArgs.Length > 0)
                        {
                            assemblyFiltersSpecified = true;

                            string[] assemblyFilters = optionArgs.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                            for (int i = 0; i < assemblyFilters.Length; ++i)
                            {
                                string filter = assemblyFilters[i];

                                if (filter.StartsWith("+", StringComparison.OrdinalIgnoreCase))
                                {
                                    if (m_IncludeAssemblies.Length > 0)
                                        m_IncludeAssemblies += ",";
                                    m_IncludeAssemblies += filter.Substring(1);
                                }
                                else if (filter.StartsWith("-", StringComparison.OrdinalIgnoreCase))
                                {
                                    if (m_ExcludeAssemblies.Length > 0)
                                        m_ExcludeAssemblies += ",";
                                    m_ExcludeAssemblies += filter.Substring(1);
                                }
                                else
                                {
                                    ResultsLogger.Log(ResultID.Warning_AssemblyFiltersNotPrefixed, filter);
                                }
                            }
                        }
                        break;

                    case "PATHFILTERS":
                        if (optionArgs.Length > 0)
                        {
                            pathFiltersSpecified = true;

                            string[] pathFilters = optionArgs.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                            for (int i = 0; i < pathFilters.Length; ++i)
                            {
                                string filter = pathFilters[i];

                                if (filter.StartsWith("+", StringComparison.OrdinalIgnoreCase))
                                {
                                    if (m_IncludePaths.Length > 0)
                                        m_IncludePaths += ",";
                                    m_IncludePaths += filter.Substring(1);
                                }
                                else if (filter.StartsWith("-", StringComparison.OrdinalIgnoreCase))
                                {
                                    if (m_ExcludePaths.Length > 0)
                                        m_ExcludePaths += ",";
                                    m_ExcludePaths += filter.Substring(1);
                                }
                                else
                                {
                                    ResultsLogger.Log(ResultID.Warning_PathFiltersNotPrefixed, filter);
                                }
                            }
                        }
                        break;
                }
            }

            if (m_IncludeAssemblies.Length == 0)
                m_IncludeAssemblies = AssemblyFiltering.GetUserOnlyAssembliesString();

            if (m_ExcludeAssemblies.Length > 0)
                m_ExcludeAssemblies += ",";

            m_ExcludeAssemblies += AssemblyFiltering.kDefaultExcludedAssemblies;

            assemblyFiltering.Parse(m_IncludeAssemblies, m_ExcludeAssemblies);
            pathFiltering.Parse(m_IncludePaths, m_ExcludePaths);
        }
    }
}
