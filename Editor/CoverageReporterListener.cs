using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
using UnityEngine.TestTools;

namespace UnityEditor.TestTools.CodeCoverage
{
    internal class CoverageReporterListener : ScriptableObject, ICallbacks
    {
        private CoverageReporterManager m_CoverageReporterManager;

        public CoverageReporterManager CoverageReporterManager
        {
            set
            {
                m_CoverageReporterManager = value;
            }
        }

        public void RunStarted(ITestAdaptor testsToRun)
        {
            if (CoverageRunData.instance.isRunning)
                return;

            CoverageRunData.instance.Start();
            m_CoverageReporterManager.CreateCoverageReporter();
            ICoverageReporter coverageReporter = m_CoverageReporterManager.CoverageReporter;
            if (coverageReporter != null)
                coverageReporter.OnRunStarted(testsToRun);
        }

        public void RunFinished(ITestResultAdaptor testResults)
        {
            if (CoverageRunData.instance.isRecording)
                return;

            CoverageRunData.instance.Stop();

            if (!CoverageRunData.instance.DidTestsRun())
                return;

            ICoverageReporter coverageReporter = m_CoverageReporterManager.CoverageReporter;
            if (coverageReporter != null)
                coverageReporter.OnRunFinished(testResults);

            m_CoverageReporterManager.GenerateReport();
        }

        public void TestStarted(ITestAdaptor test)
        {
            if (CoverageRunData.instance.HasLastIgnoredSuiteID())
                return;

            if (test.RunState == RunState.Ignored)
            {
                if (test.IsSuite)
                    CoverageRunData.instance.SetLastIgnoredSuiteID(test.Id);

                return;
            }

            if (test.IsSuite)
                return;

            CoverageRunData.instance.IncrementTestRunCount();
            ICoverageReporter coverageReporter = m_CoverageReporterManager.CoverageReporter;
            if (coverageReporter != null)
                coverageReporter.OnTestStarted(test);
        }

        public void TestFinished(ITestResultAdaptor result)
        {
            if (result.Test.RunState == RunState.Ignored)
            {
                if (result.Test.IsSuite && string.Equals(CoverageRunData.instance.GetLastIgnoredSuiteID(), result.Test.Id))
                    CoverageRunData.instance.SetLastIgnoredSuiteID(string.Empty);

                return;
            }

            if (CoverageRunData.instance.HasLastIgnoredSuiteID())
                return;

            if (result.Test.IsSuite)
                return;

            ICoverageReporter coverageReporter = m_CoverageReporterManager.CoverageReporter;
            if (coverageReporter != null)
                coverageReporter.OnTestFinished(result);
        }
    }

    [InitializeOnLoad]
    internal class CoverageReporterStarter
    {
        public static CoverageReporterManager CoverageReporterManager;

        static CoverageReporterStarter()
        {
            if (!Coverage.enabled)
                return;

#if CONDITIONAL_IGNORE_SUPPORTED
            ConditionalIgnoreAttribute.AddConditionalIgnoreMapping("IgnoreForCoverage", true);
#endif

            TestRunnerApi api = ScriptableObject.CreateInstance<TestRunnerApi>();
            CoverageReporterListener listener = ScriptableObject.CreateInstance<CoverageReporterListener>();
            api.RegisterCallbacks(listener);

            CoverageSettings coverageSettings = new CoverageSettings()
            {
                resultsPathFromCommandLine = CommandLineManager.instance.coverageResultsPath
            };

            CoverageReporterManager = new CoverageReporterManager(coverageSettings);

            listener.CoverageReporterManager = CoverageReporterManager;

            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;

            // Generate a report if running from the command line,
            // generateHTMLReport or generateBadgeReport is passed to -coverageOptions
            // and -runTests has not been passed to the command line
            if (CommandLineManager.instance.runFromCommandLine &&
                (CommandLineManager.instance.generateHTMLReport || CommandLineManager.instance.generateBadgeReport) &&
                !CommandLineManager.instance.runTests)
            {
                CoverageReporterManager.CoverageReporter.OnInitialise(coverageSettings);
                CoverageReporterManager.GenerateReport();
            }
        }

        static void OnBeforeAssemblyReload()
        {
            if (!CoverageRunData.instance.isRunning)
                return;

            if (!CoverageRunData.instance.DidTestsRun())
                return;

            if (CoverageRunData.instance.isRecording && CoverageRunData.instance.isRecordingPaused)
                return;

            ICoverageReporter coverageReporter = CoverageReporterManager.CoverageReporter;
            if (coverageReporter != null)
                coverageReporter.OnBeforeAssemblyReload();
        }

        static void OnAfterAssemblyReload()
        {
            if (!CoverageRunData.instance.isRunning)
                return;

            CoverageReporterManager.CreateCoverageReporter();
        }
    }
}
