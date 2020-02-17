using System;
using System.IO;
using System.Text;
using Palmmedia.ReportGenerator.Core;
using Palmmedia.ReportGenerator.Core.Logging;
using UnityEngine;
using UnityEditor.TestTools.CodeCoverage.Utils;
using ILogger = Palmmedia.ReportGenerator.Core.Logging.ILogger;
using Palmmedia.ReportGenerator.Core.CodeAnalysis;

namespace UnityEditor.TestTools.CodeCoverage
{
    internal class CoverageReportGenerator
    {
        public void Generate(CoverageSettings coverageSettings)
        {
            if (coverageSettings == null)
            {
                EditorUtility.ClearProgressBar();
                return;
            }

            string projectPathHash = Application.dataPath.GetHashCode().ToString("X8");

            string includeAssemblies = CommandLineManager.instance.runFromCommandLine ?
                CommandLineManager.instance.assemblyFiltering.includedAssemblies :
                EditorPrefs.GetString("CodeCoverageSettings.IncludeAssemblies." + projectPathHash, AssemblyFiltering.GetUserOnlyAssembliesString());

            string[] includeAssembliesArray = includeAssemblies.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < includeAssembliesArray.Length; i++)
            {
                includeAssembliesArray[i] = "+" + includeAssembliesArray[i];
            }
            string assemblies = string.Join(",", includeAssembliesArray);
            string[] assemblyFilters = assemblies.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            if (assemblyFilters.Length == 0)
            {
                Debug.LogError($"[{CoverageSettings.PackageName}] Failed to generate Code Coverage Report. Make sure you have included at least one assembly before generating a report.");
                EditorUtility.ClearProgressBar();
                return;
            }

            string rootFolderPath = coverageSettings.rootFolderPath;

            if (CoverageUtils.GetNumberOfXMLFilesInFolder(rootFolderPath) == 0)
            {
                Debug.LogError($"[{CoverageSettings.PackageName}] Failed to generate Code Coverage Report. Make sure you have run one or more tests before generating a report.");
                EditorUtility.ClearProgressBar();
                return;
            }

            string sourceXmlPath = Path.Combine(rootFolderPath, "**");

            // Only include xml files with the correct filename format
            string testResultsXmlPath = Path.Combine(sourceXmlPath, "TestCoverageResults_????.xml");
            string recordingResultsXmlPath = Path.Combine(sourceXmlPath, "RecordingCoverageResults_????.xml");
            string historyXmlPath = Path.Combine(sourceXmlPath, "????-??-??_??-??-??_CoverageHistory.xml");

            string[] reportFilePatterns = new string[] { testResultsXmlPath, recordingResultsXmlPath, historyXmlPath };

            string targetDirectory = Path.Combine(rootFolderPath, CoverageSettings.ReportFolderName);

            if (Directory.Exists(targetDirectory))
                Directory.Delete(targetDirectory, true);

            string[] sourceDirectories = new string[] { };

            string historyDirectory = Path.Combine(rootFolderPath, CoverageSettings.ReportHistoryFolderName);

            bool generateHTMLReport = CommandLineManager.instance.runFromCommandLine ?
                CommandLineManager.instance.generateHTMLReport :
                EditorPrefs.GetBool("CodeCoverageSettings.GenerateHTMLReport." + projectPathHash, true);

            bool generateBadge = CommandLineManager.instance.runFromCommandLine ?
                CommandLineManager.instance.generateBadgeReport :
                EditorPrefs.GetBool("CodeCoverageSettings.GenerateBadge." + projectPathHash, true);

            string reportTypesString = "xmlSummary,";
            if (generateHTMLReport)
                reportTypesString += "Html,";
            if (generateBadge)
                reportTypesString += "Badges,";

            string[] reportTypes = reportTypesString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string[] plugins = new string[] { };

            bool includeCoverageOptions = CommandLineManager.instance.runFromCommandLine ?
                CommandLineManager.instance.enableCyclomaticComplexity :
                EditorPrefs.GetBool("CodeCoverageSettings.EnableCyclomaticComplexity." + projectPathHash, false);

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
                    Debug.Log($"[{CoverageSettings.PackageName}] Code Coverage Report was generated in {targetDirectory}\n{loggerFactory.Logger.ToString()}");
                    if (!CommandLineManager.instance.runFromCommandLine)
                    {
                        string indexHtm = Path.Combine(targetDirectory, "index.htm");
                        if (File.Exists(indexHtm))
                            EditorUtility.RevealInFinder(indexHtm);
                        else
                            EditorUtility.RevealInFinder(targetDirectory);
                    }
                }
                else
                {
                    Debug.LogError($"[{CoverageSettings.PackageName}] Failed to generate Code Coverage Report.\n{loggerFactory.Logger.ToString()}");
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