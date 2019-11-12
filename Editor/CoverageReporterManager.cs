using UnityEngine;
using UnityEditor.TestTools.CodeCoverage.OpenCover;

namespace UnityEditor.TestTools.CodeCoverage
{
    internal class CoverageReporterManager
    {
        private CoverageSettings m_CoverageSettings = null;
        private ICoverageReporter m_CoverageReporter = null;
        CoverageReportGenerator m_ReportGenerator = null;

        public CoverageReporterManager(CoverageSettings coverageSettings)
        {
            m_CoverageSettings = coverageSettings;
        }

        public ICoverageReporter CoverageReporter
        {
            get
            {
                if (m_CoverageReporter == null)
                {
                    CreateCoverageReporter();
                }
                return m_CoverageReporter;
            }
        }

        public void CreateCoverageReporter()
        {
            m_CoverageReporter = null;

            string projectPathHash = Application.dataPath.GetHashCode().ToString("X8");
            CoverageFormat coverageFormat = (CoverageFormat)EditorPrefs.GetInt("CodeCoverageSettings.Format." + projectPathHash, 0);

            switch (coverageFormat)
            {
                case CoverageFormat.OpenCover:
                    m_CoverageSettings.resultsFileExtension = "xml";
                    m_CoverageSettings.resultsFolderSuffix = "-opencov";
                    m_CoverageSettings.resultsFileName = CoverageRunData.instance.isRecording ? "RecordingCoverageResults" : "TestCoverageResults";

                    m_CoverageReporter = new OpenCoverReporter();
                    break;
            }

            if (m_CoverageReporter != null)
            {
                m_CoverageReporter.OnInitialise(m_CoverageSettings);
            }
        }

        public void GenerateReport()
        {
            string projectPathHash = Application.dataPath.GetHashCode().ToString("X8");
            bool autoGenerateReport = EditorPrefs.GetBool("CodeCoverageSettings.AutoGenerateReport." + projectPathHash, true);
            bool generateHTMLReport = EditorPrefs.GetBool("CodeCoverageSettings.GenerateHTMLReport." + projectPathHash, true);
            bool generateBadge = EditorPrefs.GetBool("CodeCoverageSettings.GenerateBadge." + projectPathHash, true);
            autoGenerateReport = autoGenerateReport && (generateHTMLReport || generateBadge);

            if (CommandLineManager.instance.runFromCommandLine)
            {
                generateHTMLReport = CommandLineManager.instance.generateHTMLReport;
                generateBadge = CommandLineManager.instance.generateBadgeReport;
                autoGenerateReport = generateHTMLReport || generateBadge;
            }

            if (!autoGenerateReport)
            {
                // Clear ProgressBar left from saving results to file,
                // otherwise continue on the same ProgressBar
                EditorUtility.ClearProgressBar();
                return;
            }

            if (m_ReportGenerator == null)
                m_ReportGenerator = new CoverageReportGenerator();

            if (m_CoverageSettings != null)
                m_ReportGenerator.Generate(m_CoverageSettings);
        }
    }
}
