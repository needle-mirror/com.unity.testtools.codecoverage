namespace UnityEditor.TestTools.CodeCoverage
{
    internal class CoverageSettings
    {
        public const string ReportFolderName = "Report";
        public const string ReportHistoryFolderName = "Report-history";
        public const string PackageName = "Code Coverage";

        public string rootFolderPath;
        public string rootFolderName = "CodeCoverage";
        public string resultsPathFromCommandLine;
        public string resultsFileName = "TestCoverageResults";
        public string resultsFileExtension = "xml";
        public string resultsFolderSuffix = "-opencov";
        public string resultsFolderPath;
        public string resultsRootFolderPath;
        public string resultsFilePath;
        public string historyPathFromCommandLine;
        public string historyFolderPath;
        public string overrideIncludeAssemblies;
        public bool overrideGenerateHTMLReport = false;
        public bool hasPersistentRunData = true;
        public bool resetCoverageData = true;
        public bool revealReportInFinder = true;
    }
}
