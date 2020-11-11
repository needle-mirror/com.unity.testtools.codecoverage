using System;
using System.IO;
using System.Text;
using Palmmedia.ReportGenerator.Core;
using Palmmedia.ReportGenerator.Core.Logging;
using UnityEditor.TestTools.CodeCoverage.Utils;
using ILogger = Palmmedia.ReportGenerator.Core.Logging.ILogger;
using Palmmedia.ReportGenerator.Core.CodeAnalysis;
using UnityEditor.TestTools.CodeCoverage.Analytics;

namespace UnityEditor.TestTools.CodeCoverage
{
    internal class CoverageReportGenerator
    {
        public void Generate(CoverageSettings coverageSettings)
        {
            if (coverageSettings == null)
            {
                EditorUtility.ClearProgressBar();
                ResultsLogger.Log(ResultID.Warning_FailedReportNullCoverageSettings);
                return;
            }

            string includeAssemblies = CommandLineManager.instance.runFromCommandLine ?
                CommandLineManager.instance.assemblyFiltering.includedAssemblies :
                CoveragePreferences.instance.GetString("IncludeAssemblies", AssemblyFiltering.GetUserOnlyAssembliesString());

            // If override for include assemblies is set in coverageSettings, use overrideIncludeAssemblies instead
            if (!String.IsNullOrEmpty(coverageSettings.overrideIncludeAssemblies))
                includeAssemblies = coverageSettings.overrideIncludeAssemblies;

            string[] includeAssembliesArray = includeAssemblies.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < includeAssembliesArray.Length; i++)
            {
                includeAssembliesArray[i] = "+" + includeAssembliesArray[i];
            }
            string assemblies = string.Join(",", includeAssembliesArray);
            string[] assemblyFilters = assemblies.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            if (assemblyFilters.Length == 0)
            {
                EditorUtility.ClearProgressBar();
                ResultsLogger.Log(ResultID.Error_FailedReportNoAssemblies);
                return;
            }

            string rootFolderPath = coverageSettings.rootFolderPath;

            if (rootFolderPath == null || CoverageUtils.GetNumberOfFilesInFolder(rootFolderPath, "*.xml", SearchOption.AllDirectories) == 0)
            {
                EditorUtility.ClearProgressBar();
                ResultsLogger.Log(ResultID.Error_FailedReportNoTests);
                return;
            }

            // Only include xml files with the correct filename format
            string sourceXmlPath = CoverageUtils.JoinPaths(rootFolderPath, "**");       
            string testResultsXmlPath = CoverageUtils.JoinPaths(sourceXmlPath, "TestCoverageResults_????.xml");
            string recordingResultsXmlPath = CoverageUtils.JoinPaths(sourceXmlPath, "RecordingCoverageResults_????.xml");

            string[] reportFilePatterns = new string[] { testResultsXmlPath, recordingResultsXmlPath };

            bool includeHistoryInReport = CommandLineManager.instance.runFromCommandLine ?
                CommandLineManager.instance.generateHTMLReportHistory :
                CoveragePreferences.instance.GetBool("IncludeHistoryInReport", true);

            string historyDirectory = includeHistoryInReport ? coverageSettings.historyFolderPath : null;

            string targetDirectory = CoverageUtils.JoinPaths(rootFolderPath, CoverageSettings.ReportFolderName);

            if (Directory.Exists(targetDirectory))
                Directory.Delete(targetDirectory, true);

            string[] sourceDirectories = new string[] { };

            bool generateHTMLReport = CommandLineManager.instance.runFromCommandLine ?
                CommandLineManager.instance.generateHTMLReport :
                CoveragePreferences.instance.GetBool("GenerateHTMLReport", true);

            bool generateBadge = CommandLineManager.instance.runFromCommandLine ?
                CommandLineManager.instance.generateBadgeReport :
                CoveragePreferences.instance.GetBool("GenerateBadge", true);

            string reportTypesString = "xmlSummary,";
            if (generateHTMLReport)
                reportTypesString += "Html,";
            if (generateBadge)
                reportTypesString += "Badges,";

            string[] reportTypes = reportTypesString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string[] plugins = new string[] { };

            bool includeCoverageOptions = CommandLineManager.instance.runFromCommandLine ?
                CommandLineManager.instance.generateAdditionalMetrics :
                CoveragePreferences.instance.GetBool("GenerateAdditionalMetrics", false);

            string[] classFilters = new string[] { };
            string[] fileFilters = new string[] { };
            string verbosityLevel = null;
            string tag = null;

            ReportConfiguration config = new ReportConfiguration(
            reportFilePatterns,
            targetDirectory,
            sourceDirectories,
            historyDirectory,
            reportTypes,
            plugins,
            assemblyFilters,
            classFilters,
            fileFilters,
            verbosityLevel,
            tag);

            DebugFactory loggerFactory = new DebugFactory();
            LoggerFactory.Configure(loggerFactory);

            try
            {
                if (!CommandLineManager.instance.runFromCommandLine)
                    EditorUtility.DisplayProgressBar(ReportGeneratorStyles.ProgressTitle.text, ReportGeneratorStyles.ProgressInfo.text, 0.4f);

                Generator generator = new Generator();
                if (generator.GenerateReport(config, new Settings() { DisableRiskHotspots = !includeCoverageOptions }, new RiskHotspotsAnalysisThresholds()))
                {
                    ResultsLogger.Log(ResultID.Log_ReportSaved, targetDirectory);
                    ResultsLogger.LogSessionItem(loggerFactory.Logger.ToString(), LogVerbosityLevel.Info);

                    // Send Analytics event (Report Only / Data & Report)
                    CoverageAnalytics.instance.SendCoverageEvent(true);

                    if (!CommandLineManager.instance.runFromCommandLine &&
                        coverageSettings.revealReportInFinder)
                    {
                        string indexHtm = CoverageUtils.JoinPaths(targetDirectory, "index.htm");
                        if (File.Exists(indexHtm))
                            EditorUtility.RevealInFinder(indexHtm);
                        else
                            EditorUtility.RevealInFinder(targetDirectory);
                    }
                }
                else
                {
                    ResultsLogger.Log(ResultID.Error_FailedReport);
                    ResultsLogger.LogSessionItem(loggerFactory.Logger.ToString(), LogVerbosityLevel.Error);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }
    }

    class DebugFactory : ILoggerFactory
    {
        public VerbosityLevel VerbosityLevel { get; set; }

        public DebugLogger Logger { get; set; }

        public DebugFactory()
        {
            Logger = new DebugLogger();
        }

        public ILogger GetLogger(Type type)
        {
            return Logger;
        }
    }

    class DebugLogger : ILogger
    {
        public VerbosityLevel VerbosityLevel { get; set; }

        StringBuilder m_StringBuilder = new StringBuilder();

        public void Debug(string message)
        {
            m_StringBuilder.AppendLine(message);
        }

        public void DebugFormat(string format, params object[] args)
        {
            string message = string.Format(format, args);
            m_StringBuilder.AppendLine(message);

            if (!CommandLineManager.instance.runFromCommandLine)
            {
                if (string.Equals(format, " Creating report {0}/{1} (Assembly: {2}, Class: {3})"))
                {
                    if (args.Length >= 2)
                    {
                        if (Int32.TryParse(string.Format("{0}", args[0]), out int currentNum) &&
                            Int32.TryParse(string.Format("{0}", args[1]), out int totalNum) &&
                            currentNum <= totalNum &&
                            currentNum > 0 &&
                            totalNum > 0)
                        {
                            float progress = (1f / totalNum) * currentNum;
                            EditorUtility.DisplayProgressBar(ReportGeneratorStyles.ProgressTitle.text, ReportGeneratorStyles.ProgressInfoCreating.text, progress);
                        }
                    }
                }
            }
        }

        public void Error(string message)
        {
            m_StringBuilder.AppendLine(message);
        }

        public void ErrorFormat(string format, params object[] args)
        {
            string message = string.Format(format, args);
            m_StringBuilder.AppendLine(message);
        }

        public void Info(string message)
        {
            m_StringBuilder.AppendLine(message);
        }

        public void InfoFormat(string format, params object[] args)
        {
            string message = string.Format(format, args);
            m_StringBuilder.AppendLine(message);

            if (!CommandLineManager.instance.runFromCommandLine)
            {
                if (string.Equals(format, "Loading report '{0}' {1}/{2}"))
                {
                    if (args.Length == 3)
                    {
                        if (Int32.TryParse(string.Format("{0}", args[1]), out int currentNum) &&
                            Int32.TryParse(string.Format("{0}", args[2]), out int totalNum) &&
                            currentNum <= totalNum &&
                            currentNum > 0 &&
                            totalNum > 0)
                        {
                            float progress = (1f / (totalNum + 1)) * currentNum;
                            EditorUtility.DisplayProgressBar(ReportGeneratorStyles.ProgressTitle.text, ReportGeneratorStyles.ProgressInfoPreparing.text, progress);
                        }
                    }
                }
            }
        }

        public void Warn(string message)
        {
            m_StringBuilder.AppendLine(message);
        }

        public void WarnFormat(string format, params object[] args)
        {
            string message = string.Format(format, args);
            m_StringBuilder.AppendLine(message);
        }

        public override string ToString()
        {
            return m_StringBuilder.ToString();
        }
    }
}