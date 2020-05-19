using System.IO;
using NUnit.Framework;
using UnityEditor.TestTools.CodeCoverage.Utils;
using UnityEngine;

namespace UnityEditor.TestTools.CodeCoverage
{
    internal abstract class CoverageResultWriterBase<T> where T : class
    {
        protected CoverageSettings m_CoverageSettings;

        public T CoverageSession { get; set; }

        protected CoverageResultWriterBase(CoverageSettings coverageSettings)
        {
            m_CoverageSettings = coverageSettings;
        }

        public virtual void WriteCoverageSession()
        {
#if UNITY_2020_1_OR_NEWER
            if (Compilation.CompilationPipeline.codeOptimization == Compilation.CodeOptimization.Release)
            {
                Debug.LogWarning($"[{CoverageSettings.PackageName}] Code Coverage requires Code Optimization to be set to debug mode in order to obtain accurate coverage information. Switch to debug mode in the Editor (bottom right corner, select the Bug icon > Switch to debug mode), using the CompilationPipeline api by setting 'CompilationPipeline.codeOptimization = CodeOptimization.Debug' or by passing '-debugCodeOptimization' to the command line in batchmode.");

                if (!CommandLineManager.instance.runFromCommandLine)
                {
                    if (EditorUtility.DisplayDialog(
                        L10n.Tr("Code Coverage"),
                        L10n.Tr("Code Coverage requires Code Optimization to be set to debug mode in order to obtain accurate coverage information. Do you want to switch to debug mode?\n\nNote that you would need to rerun " + (CoverageRunData.instance.isRecording ? "the Coverage Recording session." : "the tests.")),
                        L10n.Tr("Switch to debug mode"),
                        L10n.Tr("Cancel")))
                    {
                        Compilation.CompilationPipeline.codeOptimization = Compilation.CodeOptimization.Debug;
                        EditorPrefs.SetBool("ScriptDebugInfoEnabled", true);
                    }
                }
            }
#endif
        }

        public void SetupCoveragePaths()
        {
            string folderName = CoverageUtils.GetProjectFolderName(m_CoverageSettings.projectPath);
            string resultsRootDirectoryName = folderName + m_CoverageSettings.resultsFolderSuffix;

            bool isRecording = CoverageRunData.instance.isRecording;

            // We want to save in the 'Recording' subdirectory of the results folder when recording
            string testSuite = isRecording ? "Recording" : TestContext.Parameters.Get("platform");
            string directoryName = Path.Combine(resultsRootDirectoryName, testSuite != null ? testSuite : "");

            m_CoverageSettings.rootFolderPath = CoverageUtils.GetRootFolderPath(m_CoverageSettings);
            m_CoverageSettings.historyFolderPath = CoverageUtils.GetHistoryFolderPath(m_CoverageSettings);

            string filePath = Path.Combine(directoryName, m_CoverageSettings.resultsFileName);
            filePath = Path.Combine(m_CoverageSettings.rootFolderPath, filePath);
            filePath = CoverageUtils.NormaliseFolderSeparators(filePath);
            CoverageUtils.EnsureFolderExists(Path.GetDirectoryName(filePath));

            m_CoverageSettings.resultsRootFolderPath = CoverageUtils.NormaliseFolderSeparators(Path.Combine(m_CoverageSettings.rootFolderPath, resultsRootDirectoryName));
            m_CoverageSettings.resultsFolderPath = CoverageUtils.NormaliseFolderSeparators(Path.Combine(m_CoverageSettings.rootFolderPath, directoryName));
            m_CoverageSettings.resultsFilePath = filePath;
        }

        public void ClearCoverageFolderIfExists()
        {
            CoverageUtils.ClearFolderIfExists(m_CoverageSettings.resultsFolderPath);
        }

        protected string GetNextFullFilePath()
        {
            int nextFileIndex = m_CoverageSettings.hasPersistentRunData ? CoverageRunData.instance.GetNextFileIndex() : 0;
            string fullFilePath = m_CoverageSettings.resultsFilePath + "_" + nextFileIndex.ToString("D4") + "." + m_CoverageSettings.resultsFileExtension;
            return fullFilePath;
        }
    }
}