using UnityEngine.TestTools;

namespace UnityEditor.TestTools.CodeCoverage
{
    /// <summary>
    /// Utility class for the CodeCoverage api.
    /// </summary>
    /// <example>
    /// The following example loads a scene, starts coverage recording, initialises a number of instances of a prefab, then pauses the recording to load another scene, unpauses the recording, initialises a number of instances of a different prefab and finally stops the recording.
    /// <code>
    /// using UnityEngine;
    /// using UnityEditor;
    /// using UnityEditor.TestTools.CodeCoverage;
    /// using UnityEditor.SceneManagement;
    ///
    /// public class CoverageApiTest : MonoBehaviour
    /// {
    ///     [MenuItem("CodeCoverage/Run Recording")]
    ///     static void RunRecording()
    ///     {
    ///         int i;
    ///
    ///         EditorSceneManager.OpenScene("Assets/Scenes/Scene1.unity");
    ///
    ///         CodeCoverage.StartRecording();
    ///
    ///         for (i = 0; i &lt; 1000; ++i)
    ///         {
    ///             Instantiate(Resources.Load("ComplexPrefab1"));
    ///         }
    ///
    ///         CodeCoverage.PauseRecording();
    ///
    ///         EditorSceneManager.OpenScene("Assets/Scenes/Scene2.unity");
    ///
    ///         CodeCoverage.UnpauseRecording();
    ///
    ///         for (i = 0; i &lt; 1000; ++i)
    ///         {
    ///             Instantiate(Resources.Load("ComplexPrefab2"));
    ///         }
    ///
    ///         CodeCoverage.StopRecording();
    ///     }
    /// }
    /// </code>
    /// </example>
    public static class CodeCoverage
    {
        private static CoverageReporterManager s_CoverageReporterManager;

        /// <summary>
        /// Call this to start a new coverage recording session.
        /// </summary>
        public static void StartRecording()
        {
            bool isRunning = CoverageRunData.instance.isRunning;

            if (!isRunning)
            {
                Coverage.ResetAll();

                CoverageRunData.instance.StartRecording();

                if (s_CoverageReporterManager == null)
                    s_CoverageReporterManager = CoverageReporterStarter.CoverageReporterManager;
                s_CoverageReporterManager.CreateCoverageReporter();

                ICoverageReporter coverageReporter = s_CoverageReporterManager.CoverageReporter;
                if (coverageReporter != null)
                    coverageReporter.OnRunStarted(null);
            }
        }

        /// <summary>
        /// Call this to pause the recording on the current coverage recording session.
        /// </summary>
        public static void PauseRecording()
        {
            bool isRunning = CoverageRunData.instance.isRunning;

            if (isRunning)
            {
                if (CoverageRunData.instance.isRecording && !CoverageRunData.instance.isRecordingPaused)
                {
                    if (s_CoverageReporterManager == null)
                        s_CoverageReporterManager = CoverageReporterStarter.CoverageReporterManager;

                    ICoverageReporter coverageReporter = s_CoverageReporterManager.CoverageReporter;
                    if (coverageReporter != null)
                        coverageReporter.OnCoverageRecordingPaused();

                    CoverageRunData.instance.PauseRecording();
                }
            }
        }

        /// <summary>
        /// Call this to continue recording on the current coverage recording session, after having paused the recording.
        /// </summary>
        public static void UnpauseRecording()
        {
            bool isRunning = CoverageRunData.instance.isRunning;

            if (isRunning)
            {
                if (CoverageRunData.instance.isRecording && CoverageRunData.instance.isRecordingPaused)
                {
                    Coverage.ResetAll();

                    CoverageRunData.instance.UnpauseRecording();
                }
            }
        }

        /// <summary>
        /// Call this to end the current coverage recording session.
        /// </summary>
        public static void StopRecording()
        {
            bool isRunning = CoverageRunData.instance.isRunning;

            if (isRunning)
            {
                if (CoverageRunData.instance.isRecording)
                {
                    if (CoverageRunData.instance.isRecordingPaused)
                        Coverage.ResetAll();

                    if (s_CoverageReporterManager == null)
                        s_CoverageReporterManager = CoverageReporterStarter.CoverageReporterManager;

                    ICoverageReporter coverageReporter = s_CoverageReporterManager.CoverageReporter;
                    if (coverageReporter != null)
                        coverageReporter.OnRunFinished(null);

                    CoverageRunData.instance.StopRecording();

                    s_CoverageReporterManager.GenerateReport();
                }
            }
        }
    }
}
