// #define COVERAGE_ANALYTICS_LOGGING
using System;
using System.Collections.Generic;
using UnityEditor.TestTools.CodeCoverage.Utils;
using UnityEngine;
using UnityEditor.Compilation;
using UnityEngine.Analytics;
#if !UNITY_6000_0_OR_NEWER
using System.Linq;
#endif

namespace UnityEditor.TestTools.CodeCoverage.Analytics
{
    [Serializable]
    internal class Timer
    {
        public void Start()
        {
            startTime = DateTime.Now;
        }

        public long elapsedTimeMs => (long)(DateTime.Now - startTime).TotalMilliseconds;

        [SerializeField]
        private DateTime startTime = DateTime.Now;
    }

    [Serializable]
    internal class CoverageAnalytics : ScriptableSingleton<CoverageAnalytics>
    {
#if !UNITY_6000_0_OR_NEWER
        [SerializeField]
        private bool s_Registered;
#endif
        [SerializeField]
        private List<int> s_ResultsIdsList;

        public CoverageAnalyticsEvent CurrentCoverageEvent;
        public Timer CoverageTimer;

        protected CoverageAnalytics() : base()
        {
            ResetEvents();
        }

        public void ResetEvents()
        {
            CurrentCoverageEvent = new CoverageAnalyticsEvent();
            CoverageTimer = new Timer();
            s_ResultsIdsList = new List<int>();

            CurrentCoverageEvent.actionID = ActionID.Other;
            CurrentCoverageEvent.coverageModeId = CoverageModeID.None;
            CurrentCoverageEvent.numOfIncludedPaths = 0;
            CurrentCoverageEvent.numOfExcludedPaths = 0;
        }

        public void StartTimer()
        {
            CoverageTimer.Start();
        }

        // See ResultsLogger.cs for details
        public void AddResult(ResultID resultId)
        {
            s_ResultsIdsList.Add((int)resultId);
        }

        public void SendCoverageEvent(bool success)
        {
            CurrentCoverageEvent.success = success;
            CurrentCoverageEvent.duration = CoverageTimer.elapsedTimeMs;
            CurrentCoverageEvent.resultIds = s_ResultsIdsList.ToArray();

            bool runFromCommandLine = CommandLineManager.instance.runFromCommandLine;
            bool batchmode = CommandLineManager.instance.batchmode;
            bool useProjectSettings = CommandLineManager.instance.useProjectSettings;

            CurrentCoverageEvent.runFromCommandLine = runFromCommandLine;
            CurrentCoverageEvent.batchmode = batchmode;
            CurrentCoverageEvent.useProjectSettings = useProjectSettings;

            if (batchmode && !useProjectSettings)
            {
                CurrentCoverageEvent.autogenerate = CommandLineManager.instance.generateBadgeReport || CommandLineManager.instance.generateHTMLReport || CommandLineManager.instance.generateAdditionalReports;
                CurrentCoverageEvent.createBadges = CommandLineManager.instance.generateBadgeReport;
                CurrentCoverageEvent.generateHistory = CommandLineManager.instance.generateHTMLReportHistory;
                CurrentCoverageEvent.generateHTMLReport = CommandLineManager.instance.generateHTMLReport;
                CurrentCoverageEvent.generateMetrics = CommandLineManager.instance.generateAdditionalMetrics;
                CurrentCoverageEvent.generateTestReferences = CommandLineManager.instance.generateTestReferences;
                CurrentCoverageEvent.generateAdditionalReports = CommandLineManager.instance.generateAdditionalReports;
                CurrentCoverageEvent.dontClear = CommandLineManager.instance.dontClear;
                CurrentCoverageEvent.useDefaultAssemblyFilters = !CommandLineManager.instance.assemblyFiltersSpecified;
                CurrentCoverageEvent.useDefaultPathFilters = !CommandLineManager.instance.pathFiltersSpecified;
                CurrentCoverageEvent.useDefaultResultsLoc = CommandLineManager.instance.coverageResultsPath.Length == 0;
                CurrentCoverageEvent.useDefaultHistoryLoc = CommandLineManager.instance.coverageHistoryPath.Length == 0;
                CurrentCoverageEvent.usePathReplacePatterns = CommandLineManager.instance.pathReplacingSpecified;
                CurrentCoverageEvent.useSourcePaths = CommandLineManager.instance.sourcePathsSpecified;
                CurrentCoverageEvent.usePathFiltersFromFile = CommandLineManager.instance.pathFiltersFromFileSpecified;
                CurrentCoverageEvent.useAssemblyFiltersFromFile = CommandLineManager.instance.assemblyFiltersFromFileSpecified;
            }
            else
            {
                CurrentCoverageEvent.autogenerate = CommandLineManager.instance.generateBadgeReport || CommandLineManager.instance.generateHTMLReport || CommandLineManager.instance.generateAdditionalReports || CoveragePreferences.instance.GetBool("AutoGenerateReport", true);
                CurrentCoverageEvent.autoOpenReport = CoveragePreferences.instance.GetBool("OpenReportWhenGenerated", true);
                CurrentCoverageEvent.createBadges = CommandLineManager.instance.generateBadgeReport || CoveragePreferences.instance.GetBool("GenerateBadge", true);
                CurrentCoverageEvent.generateHistory = CommandLineManager.instance.generateHTMLReportHistory || CoveragePreferences.instance.GetBool("IncludeHistoryInReport", true);
                CurrentCoverageEvent.generateHTMLReport = CommandLineManager.instance.generateHTMLReport || CoveragePreferences.instance.GetBool("GenerateHTMLReport", true);
                CurrentCoverageEvent.generateMetrics = CommandLineManager.instance.generateAdditionalMetrics || CoveragePreferences.instance.GetBool("GenerateAdditionalMetrics", false);
                CurrentCoverageEvent.generateTestReferences = CommandLineManager.instance.generateTestReferences || CoveragePreferences.instance.GetBool("GenerateTestReferences", false);
                CurrentCoverageEvent.generateAdditionalReports = CommandLineManager.instance.generateAdditionalReports || CoveragePreferences.instance.GetBool("GenerateAdditionalReports", false);
                CurrentCoverageEvent.dontClear = CommandLineManager.instance.dontClear;
                CurrentCoverageEvent.usePathReplacePatterns = CommandLineManager.instance.pathReplacingSpecified;
                CurrentCoverageEvent.useSourcePaths = CommandLineManager.instance.sourcePathsSpecified;
                CurrentCoverageEvent.usePathFiltersFromFile = CommandLineManager.instance.pathFiltersFromFileSpecified;
                CurrentCoverageEvent.useAssemblyFiltersFromFile = CommandLineManager.instance.assemblyFiltersFromFileSpecified;


                CurrentCoverageEvent.useDefaultAssemblyFilters = !CommandLineManager.instance.assemblyFiltersSpecified;
                if (!CommandLineManager.instance.assemblyFiltersSpecified)
                    CurrentCoverageEvent.useDefaultAssemblyFilters = string.Equals(CoveragePreferences.instance.GetString("IncludeAssemblies", AssemblyFiltering.GetUserOnlyAssembliesString()), AssemblyFiltering.GetUserOnlyAssembliesString(), StringComparison.InvariantCultureIgnoreCase);

                CurrentCoverageEvent.useDefaultPathFilters = !CommandLineManager.instance.pathFiltersSpecified;
                if (!CommandLineManager.instance.pathFiltersSpecified)
                    CurrentCoverageEvent.useDefaultPathFilters = string.Equals(CoveragePreferences.instance.GetString("PathsToInclude", string.Empty), string.Empty) && string.Equals(CoveragePreferences.instance.GetString("PathsToExclude", string.Empty), string.Empty);

                CurrentCoverageEvent.useDefaultResultsLoc = CommandLineManager.instance.coverageResultsPath.Length == 0;
                if (CommandLineManager.instance.coverageResultsPath.Length == 0)
                    CurrentCoverageEvent.useDefaultResultsLoc = string.Equals(CoveragePreferences.instance.GetStringForPaths("Path", string.Empty), CoverageUtils.GetProjectPath(), StringComparison.InvariantCultureIgnoreCase);

                CurrentCoverageEvent.useDefaultHistoryLoc = CommandLineManager.instance.coverageHistoryPath.Length == 0;
                if (CommandLineManager.instance.coverageHistoryPath.Length == 0)
                    CurrentCoverageEvent.useDefaultHistoryLoc = string.Equals(CoveragePreferences.instance.GetStringForPaths("HistoryPath", string.Empty), CoverageUtils.GetProjectPath(), StringComparison.InvariantCultureIgnoreCase);
            }

            CurrentCoverageEvent.inDebugMode = CompilationPipeline.codeOptimization == CodeOptimization.Debug;

            if (!runFromCommandLine || (runFromCommandLine && !batchmode && !CommandLineManager.instance.assemblyFiltersSpecified))
            {
                if (CurrentCoverageEvent.actionID == ActionID.ReportOnly)
                {
                    string includeAssemblies = CoveragePreferences.instance.GetString("IncludeAssemblies", AssemblyFiltering.GetUserOnlyAssembliesString());
                    string[] includeAssembliesArray = includeAssemblies.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    CurrentCoverageEvent.includedAssemblies = includeAssembliesArray;
                }
            }

            Send(CurrentCoverageEvent);

            ResetEvents();
        }

        private void Send(CoverageAnalyticsEvent data)
        {
#if UNITY_6000_0_OR_NEWER
            if (!EditorAnalytics.enabled)
#else
            if (!RegisterEvents())
#endif
            {
#if COVERAGE_ANALYTICS_LOGGING
                Console.WriteLine($"[{CoverageSettings.PackageName}] Analytics disabled: event='codeCoverage', time='{DateTime.Now:HH:mm:ss}', payload={EditorJsonUtility.ToJson(data, true)}");
#endif
                return;
            }
            try
            {
#if UNITY_6000_0_OR_NEWER
                Analytic analytic = new Analytic(data);
                var result = EditorAnalytics.SendAnalytic(analytic);
#else
                var result = EditorAnalytics.SendEventWithLimit(EventName.codeCoverage.ToString(), data);
#endif
                if (result == AnalyticsResult.Ok)
                {
#if COVERAGE_ANALYTICS_LOGGING
                    ResultsLogger.LogSessionItem($"Event=codeCoverage, time={DateTime.Now:HH:mm:ss}, payload={EditorJsonUtility.ToJson(data, true)}", LogVerbosityLevel.Info);
#endif
                }
                else
                {
                    ResultsLogger.LogSessionItem($"Failed to send analytics event codeCoverage. Result: {result}", LogVerbosityLevel.Error);
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

#if UNITY_6000_0_OR_NEWER
        [AnalyticInfo(eventName: "codeCoverage", vendorKey: "unity.testtools.codecoverage", maxEventsPerHour: 100, maxNumberOfElements: 1000, version: 2)]
        internal class Analytic : IAnalytic
        {
            public Analytic(CoverageAnalyticsEvent data) { m_Data = data; }
            public bool TryGatherData(out IAnalytic.IData data, out Exception error)
            {
                data = m_Data;
                error = null;
                return true;
            }

            CoverageAnalyticsEvent m_Data;
        }
#else
        public bool RegisterEvents()
        {
            if (!EditorAnalytics.enabled)
            {
                ResultsLogger.LogSessionItem("Editor analytics are disabled", LogVerbosityLevel.Info);
                return false;
            }

            if (s_Registered)
            {
                return true;
            }

            var allNames = Enum.GetNames(typeof(EventName));
            if (allNames.Any(eventName => !RegisterEvent(eventName)))
            {
                return false;
            }

            s_Registered = true;
            return true;
        }

        private bool RegisterEvent(string eventName)
        {
            const string vendorKey = "unity.testtools.codecoverage";
            var result = EditorAnalytics.RegisterEventWithLimit(eventName, 100, 1000, vendorKey);
            switch (result)
            {
                case AnalyticsResult.Ok:
                    {
#if COVERAGE_ANALYTICS_LOGGING
                        ResultsLogger.LogSessionItem($"Registered analytics event: {eventName}", LogVerbosityLevel.Info);
#endif
                        return true;
                    }
                case AnalyticsResult.TooManyRequests:
                    // this is fine - event registration survives domain reload (native)
                    return true;
                default:
                    {
                        ResultsLogger.LogSessionItem($"Failed to register analytics event {eventName}. Result: {result}", LogVerbosityLevel.Error);
                        return false;
                    }
            }
        }
#endif
    }
}
