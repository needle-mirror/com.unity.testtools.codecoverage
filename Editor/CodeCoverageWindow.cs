using System;
using System.IO;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor.TestTools.CodeCoverage.Utils;
using UnityEditor.TestTools.TestRunner;

namespace UnityEditor.TestTools.CodeCoverage
{
    internal class CodeCoverageWindow : EditorWindow
    {
        private bool m_EnableCodeCoverage;
        private string m_CodeCoveragePath;
        private CoverageFormat m_CodeCoverageFormat;
        private string m_ProjectPathHash;
        private bool m_EnableCyclomaticComplexity;
        private string m_AssembliesToInclude;

        CoverageReportGenerator m_ReportGenerator;
        private bool m_GenerateHTMLReport;
        private bool m_GenerateBadge;
        private bool m_AutoGenerateReport;

        private CoverageSettings m_CoverageSettings;

        private bool m_DoRepaint;
        private bool m_IncludeWarnings;
        private static readonly Vector2 m_WindowMinSizeNormal = new Vector2(430, 210);
        private static readonly Vector2 m_WindowMinSizeWithWarnings = new Vector2(430, 280);

        private bool m_AfterPopupDelayInput = false;
        private bool m_GenerateReport = false;
        private bool m_StopRecording = false;

        public void HandleInputAfterPopup()
        {
            m_AfterPopupDelayInput = true;
        }

        private void Update()
        {
            if (m_AfterPopupDelayInput)
            {
                m_AfterPopupDelayInput = false;
            }

            if (m_GenerateReport)
            {
                m_ReportGenerator.Generate(m_CoverageSettings);
                m_GenerateReport = false;
            }

            if (m_StopRecording)
            {
                CodeCoverage.StopRecording();
                m_StopRecording = false;
            }
        }

        public string AssembliesToInclude
        {
            set
            {
                m_AssembliesToInclude = value.Replace(" ", "");
                EditorPrefs.SetString("CodeCoverageSettings.IncludeAssemblies." + m_ProjectPathHash, m_AssembliesToInclude);
            }
        }

        class Styles
        {
            static bool s_Initialized;

            public static readonly GUIContent OpenPreferencesButton = EditorGUIUtility.TrTextContent("Open Preferences");
            public static readonly GUIContent SwitchToDebugCodeOptimizationButton = EditorGUIUtility.TrTextContent("Switch to debug mode");
            public static readonly GUIContent CodeCoverageResultsLocationLabel = EditorGUIUtility.TrTextContent("Destination", "Click the Browse button to specify the folder where the coverage results and report will be saved to. The default location is the Project's folder.");
            public static readonly GUIContent CoverageOptionsLabel = EditorGUIUtility.TrTextContent("Settings");
            public static readonly GUIContent CodeCoverageFormat = EditorGUIUtility.TrTextContent("Coverage Format", "The Code Coverage format used when saving the results.");
            public static readonly GUIContent CodeCoverageCyclomaticComplexityLabel = EditorGUIUtility.TrTextContent("Cyclomatic Complexity", "Check this to enable the Cyclomatic Complexity calculation for each method.");
            public static readonly GUIContent AssembliesToIncludeLabel = EditorGUIUtility.TrTextContent("Included Assemblies", "Specify the assemblies that will be included in the coverage results. This is a comma separated string. Click the Select button to view and easily select or deselect the assemblies.");
            public static readonly GUIContent AssembliesToIncludeDropdownLabel = EditorGUIUtility.TrTextContent("Select", "Click this to view and easily select or deselect the assemblies that will be included in the coverage results.");
            public static readonly GUIContent BrowseButtonLabel = EditorGUIUtility.TrTextContent("Browse", "Click this to specify the folder where the coverage results and report will be saved to.");
            public static readonly GUIContent GenerateHTMLReportLabel = EditorGUIUtility.TrTextContent("Generate HTML Report", "Check this to include an HTML version of the report when the report is generated.");
            public static readonly GUIContent GenerateBadgeReportLabel = EditorGUIUtility.TrTextContent("Generate Summary Badge", "Check this to include a coverage summary badge when the report is generated.");
            public static readonly GUIContent AutoGenerateReportLabel = EditorGUIUtility.TrTextContent("Auto Generate Report", "Check this to generate the report automatically after the Test Runner has finished running the tests or the Coverage Recording has completed.");
            public static readonly GUIContent GenerateReportButtonLabel = EditorGUIUtility.TrTextContent("Generate from Last", "Generates a Coverage Report from the last set of tests that were run in the Test Runner or from the last Coverage Recording session.");
            public static readonly GUIContent ClearCoverageButtonLabel = EditorGUIUtility.TrTextContent("Clear Data", "Clears the Coverage data from previous test runs for both EditMode and PlayMode tests or from previous Coverage Recording sessions, for the current project.");
            public static readonly GUIContent ClearHistoryButtonLabel = EditorGUIUtility.TrTextContent("Clear History", "Clears the Coverage Report history.");
            public static readonly GUIContent StartRecordingButtonLabel = EditorGUIUtility.TrTextContentWithIcon(" Start Recording", "Record Coverage data.", "Record Off");
            public static readonly GUIContent StopRecordingButtonLabel = EditorGUIUtility.TrTextContentWithIcon(" Stop Recording", "Stop recording Coverage data.", "Record On");

            public static readonly GUIStyle largeButton = "LargeButton";

            public static GUIStyle settings;

            public static void Init()
            {
                if (s_Initialized)
                    return;

                s_Initialized = true;

                settings = new GUIStyle()
                {
                    margin = new RectOffset(8, 4, 18, 4)
                };
            }
        }

        [MenuItem("Window/General/Code Coverage", false, 202)]
        public static void ShowCodeCoverageWindow()
        {
            TestRunnerWindow.ShowPlaymodeTestsRunnerWindowCodeBased();
            CodeCoverageWindow window = GetWindow<CodeCoverageWindow>("Code Coverage", typeof(TestRunnerWindow));
            window.minSize = m_WindowMinSizeNormal;
            window.Show();
        }

        private void InitCodeCoverageWindow()
        {
            m_CoverageSettings = new CoverageSettings()
            {
                resultsPathFromCommandLine = string.Empty
            };

            m_ProjectPathHash = Application.dataPath.GetHashCode().ToString("X8");
            m_CodeCoveragePath = EditorPrefs.GetString("CodeCoverageSettings.Path." + m_ProjectPathHash, string.Empty);
            m_CodeCoverageFormat = (CoverageFormat)EditorPrefs.GetInt("CodeCoverageSettings.Format." + m_ProjectPathHash, 0);
            m_EnableCyclomaticComplexity = EditorPrefs.GetBool("CodeCoverageSettings.EnableCyclomaticComplexity." + m_ProjectPathHash, false);
            m_AssembliesToInclude = EditorPrefs.GetString("CodeCoverageSettings.IncludeAssemblies." + m_ProjectPathHash, AssemblyFiltering.GetUserOnlyAssembliesString());
            m_ReportGenerator = new CoverageReportGenerator();
            m_GenerateHTMLReport = EditorPrefs.GetBool("CodeCoverageSettings.GenerateHTMLReport." + m_ProjectPathHash, true);
            m_GenerateBadge = EditorPrefs.GetBool("CodeCoverageSettings.GenerateBadge." + m_ProjectPathHash, true);
            m_AutoGenerateReport = EditorPrefs.GetBool("CodeCoverageSettings.AutoGenerateReport." + m_ProjectPathHash, true);

            UpdateCoverageSettings();
            RefreshCodeCoverageWindow();

            m_IncludeWarnings = false;
            m_AfterPopupDelayInput = false;
            m_GenerateReport = false;
            m_StopRecording = false;
        }

        private void RefreshCodeCoverageWindow()
        {
            UpdateWindowSize();
            Repaint();
        }

        private void UpdateWindowSize()
        {
            if (m_IncludeWarnings)
            {
                minSize = m_WindowMinSizeWithWarnings;
            }
            else
            {
                minSize = m_WindowMinSizeNormal;
            }
        }

        private void UpdateCoverageSettings()
        {
            if (m_CoverageSettings != null)
            {
                m_CoverageSettings.rootFolderPath = CoverageUtils.GetRootFolderPath(m_CoverageSettings);

                // TODO: Refactor this when more formats are implemented
                if (m_CodeCoverageFormat == CoverageFormat.OpenCover)
                {
                    m_CoverageSettings.resultsFolderSuffix = "-opencov";
                    string folderName = CoverageUtils.GetProjectFolderName(m_CoverageSettings.projectPath);
                    string resultsRootDirectoryName = folderName + m_CoverageSettings.resultsFolderSuffix;
                    m_CoverageSettings.resultsRootFolderPath = CoverageUtils.NormaliseFolderSeparators(Path.Combine(m_CoverageSettings.rootFolderPath, resultsRootDirectoryName));
                }
            }
        }

        public void OnDestroy(){}

        private void OnEnable()
        {
            InitCodeCoverageWindow();
        }

        private void OnFocus()
        {
            RefreshCodeCoverageWindow();
        }

        public void OnGUI()
        {
            Styles.Init();

            EditorGUIUtility.labelWidth = 190f;

            GUILayout.BeginVertical(Styles.settings);

            m_EnableCodeCoverage = EditorPrefs.GetBool("CodeCoverageEnabled", false);

#if UNITY_2019_3_OR_NEWER
            bool hasLatestScriptingRuntime = true;
#else
            bool hasLatestScriptingRuntime = PlayerSettings.scriptingRuntimeVersion == ScriptingRuntimeVersion.Latest;
#endif
            if (!hasLatestScriptingRuntime)
                EditorGUILayout.HelpBox("Code Coverage requires the latest Scripting Runtime Version (.NET 4.x). You can set this in the Player Settings.", MessageType.Warning);

            using (new EditorGUI.DisabledScope(!hasLatestScriptingRuntime))
            {
                if (m_EnableCodeCoverage != Coverage.enabled)
                    EditorGUILayout.HelpBox((m_EnableCodeCoverage ? "Enabling " : "Disabling ") + "Code Coverage will not take effect until Unity is restarted.", MessageType.Warning);
                else if (!m_EnableCodeCoverage)
                {
                    EditorGUILayout.HelpBox("Code Coverage is disabled. To enable Code Coverage, go to Preferences > General, check Enable Code Coverage, and restart Unity.", MessageType.Warning);

                    if (GUILayout.Button(Styles.OpenPreferencesButton))
                        SettingsService.OpenUserPreferences("Preferences/_General");
                }

                using (new EditorGUI.DisabledScope(!m_EnableCodeCoverage || m_EnableCodeCoverage != Coverage.enabled || m_AfterPopupDelayInput))
                {
                    DrawCodeCoverageLocation();
                    DrawCoverageSettings();
                    DrawButtons();
                }
            }

            DrawCodeOptimizationWarning();

            GUILayout.EndVertical();

            if (m_DoRepaint)
            {
                RefreshCodeCoverageWindow();
                m_DoRepaint = false;
            } 
        }

        void DrawCodeOptimizationWarning()
        {
#if UNITY_2020_1_OR_NEWER
            if (Compilation.CompilationPipeline.codeOptimization == Compilation.CodeOptimization.Release)
            {
                GUILayout.Space(5);

                EditorGUILayout.HelpBox("Code Coverage requires Code Optimization to be set to Debug mode.", MessageType.Warning);

                if (!m_IncludeWarnings)
                {
                    m_IncludeWarnings = true;
                    m_DoRepaint = true;
                }

                if (GUILayout.Button(Styles.SwitchToDebugCodeOptimizationButton))
                {
                    Compilation.CompilationPipeline.codeOptimization = Compilation.CodeOptimization.Debug;
                    EditorPrefs.SetBool("ScriptDebugInfoEnabled", true);
                    m_IncludeWarnings = false;
                    m_DoRepaint = true;
                }
            }
            else
            {
                m_IncludeWarnings = false;
            }
#endif
        }

        void DrawCodeCoverageLocation()
        {
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();

            using (new EditorGUI.DisabledScope(CoverageRunData.instance.isRunning))
            {
                Rect textFieldPosition = EditorGUILayout.GetControlRect();
                textFieldPosition = EditorGUI.PrefixLabel(textFieldPosition, Styles.CodeCoverageResultsLocationLabel);
                EditorGUI.SelectableLabel(textFieldPosition, m_CodeCoveragePath, EditorStyles.textField);

                bool autoDetect = !CoverageUtils.IsValidFolder(m_CodeCoveragePath);

                if (autoDetect)
                {
                    SetDefaultCoverageLocation();
                }

                Vector2 buttonSize = EditorStyles.miniButton.CalcSize(Styles.BrowseButtonLabel);
                if (EditorGUILayout.DropdownButton(Styles.BrowseButtonLabel, FocusType.Keyboard, EditorStyles.miniButton, GUILayout.MaxWidth(buttonSize.x)))
                {
                    string candidate = Browse(m_CodeCoveragePath);
                    if (CoverageUtils.IsValidFolder(candidate))
                    {
                        m_CodeCoveragePath = CoverageUtils.NormaliseFolderSeparators(candidate, true);
                        EditorPrefs.SetString("CodeCoverageSettings.Path." + m_ProjectPathHash, m_CodeCoveragePath);

                        UpdateCoverageSettings();

                        GUI.FocusControl("");
                    }
#if UNITY_EDITOR_OSX
                    //After returning from a native dialog on OSX GUILayout gets into a corrupt state, stop rendering UI for this frame.
                    GUIUtility.ExitGUI();
#endif
                }
            }

            GUILayout.EndHorizontal();
        }

        void DrawCoverageSettings()
        {
            GUILayout.Space(5);

            EditorGUILayout.LabelField(Styles.CoverageOptionsLabel, EditorStyles.boldLabel);

            /*
            // Re-enable Coverage Format selection when there are more formats implemented

            EditorGUI.BeginChangeCheck();
            m_CodeCoverageFormat = (CoverageFormat)EditorGUILayout.EnumPopup(Styles.CodeCoverageFormat, m_CodeCoverageFormat);

            if (EditorGUI.EndChangeCheck())
                EditorPrefs.SetInt("CodeCoverageSettings.Format." + m_ProjectPathHash, (int)m_CodeCoverageFormat);

            if (m_CodeCoverageFormat == CoverageFormat.DotCover)
                EditorGUILayout.HelpBox("DotCover format is not supported yet.", MessageType.Warning);
            */

            using (new EditorGUI.DisabledScope(CoverageRunData.instance.isRunning))
            {
                // Draw Included Assemblies
                GUILayout.BeginHorizontal();

                Rect textFieldPosition = EditorGUILayout.GetControlRect();
                textFieldPosition = EditorGUI.PrefixLabel(textFieldPosition, Styles.AssembliesToIncludeLabel);

                EditorGUI.BeginChangeCheck();
                string assembliesToInclude = EditorGUI.TextField(textFieldPosition, m_AssembliesToInclude);

                Vector2 buttonSize = EditorStyles.miniButton.CalcSize(Styles.BrowseButtonLabel);
                if (EditorGUILayout.DropdownButton(Styles.AssembliesToIncludeDropdownLabel, FocusType.Keyboard, EditorStyles.miniButton, GUILayout.MaxWidth(buttonSize.x)))
                {
                    GUI.FocusControl("");
                    PopupWindow.Show(textFieldPosition, new IncludedAssembliesPopupWindow(m_AssembliesToInclude, this) { Width = textFieldPosition.width });
                }

                if (EditorGUI.EndChangeCheck())
                {
                    AssembliesToInclude = assembliesToInclude;
                }

                GUILayout.EndHorizontal();

                // Draw the rest of the settings
                EditorGUI.BeginChangeCheck();
                m_EnableCyclomaticComplexity = EditorGUILayout.Toggle(Styles.CodeCoverageCyclomaticComplexityLabel, m_EnableCyclomaticComplexity);
                if (EditorGUI.EndChangeCheck())
                {
                    EditorPrefs.SetBool("CodeCoverageSettings.EnableCyclomaticComplexity." + m_ProjectPathHash, m_EnableCyclomaticComplexity);
                }

                EditorGUI.BeginChangeCheck();
                m_GenerateHTMLReport = EditorGUILayout.Toggle(Styles.GenerateHTMLReportLabel, m_GenerateHTMLReport);
                if (EditorGUI.EndChangeCheck())
                {
                    EditorPrefs.SetBool("CodeCoverageSettings.GenerateHTMLReport." + m_ProjectPathHash, m_GenerateHTMLReport);
                }

                EditorGUI.BeginChangeCheck();
                m_GenerateBadge = EditorGUILayout.Toggle(Styles.GenerateBadgeReportLabel, m_GenerateBadge);
                if (EditorGUI.EndChangeCheck())
                {
                    EditorPrefs.SetBool("CodeCoverageSettings.GenerateBadge." + m_ProjectPathHash, m_GenerateBadge);
                }

                EditorGUI.BeginChangeCheck();
                m_AutoGenerateReport = EditorGUILayout.Toggle(Styles.AutoGenerateReportLabel, m_AutoGenerateReport);
                if (EditorGUI.EndChangeCheck())
                {
                    EditorPrefs.SetBool("CodeCoverageSettings.AutoGenerateReport." + m_ProjectPathHash, m_AutoGenerateReport);
                }
            }
        }

        void DrawButtons()
        {
            GUILayout.Space(15);

            GUILayout.BeginHorizontal();

            Vector2 buttonSize = EditorStyles.miniButton.CalcSize(Styles.ClearHistoryButtonLabel);
            using (new EditorGUI.DisabledScope(!DoesResultsRootFolderExist() || CoverageRunData.instance.isRunning))
            {
                if (EditorGUILayout.DropdownButton(Styles.ClearCoverageButtonLabel, FocusType.Keyboard, Styles.largeButton, GUILayout.MaxWidth(buttonSize.x)))
                {
                    ClearResultsRootFolderIfExists();
                }
            }

            using (new EditorGUI.DisabledScope(!DoesReportHistoryExist() || CoverageRunData.instance.isRunning))
            {
                if (EditorGUILayout.DropdownButton(Styles.ClearHistoryButtonLabel, FocusType.Keyboard, Styles.largeButton, GUILayout.MaxWidth(buttonSize.x)))
                {
                    ClearReportHistoryFolderIfExists();
                }
            }

            GUILayout.FlexibleSpace();

            buttonSize = EditorStyles.miniButton.CalcSize(Styles.GenerateReportButtonLabel);
            using (new EditorGUI.DisabledScope((!m_GenerateHTMLReport && !m_GenerateBadge) || !DoesResultsRootFolderExist() || CoverageRunData.instance.isRunning || m_GenerateReport))
            {
                if (EditorGUILayout.DropdownButton(Styles.GenerateReportButtonLabel, FocusType.Keyboard, Styles.largeButton, GUILayout.MaxWidth(buttonSize.x)))
                {
                    m_GenerateReport = true;
                }
            }

            // Coverage Recording
            bool isRunning = CoverageRunData.instance.isRunning;
            bool isRecording = CoverageRunData.instance.isRecording;

            using (new EditorGUI.DisabledScope((isRunning && !isRecording) || m_StopRecording))
            {
                if (EditorGUILayout.DropdownButton(isRecording ? Styles.StopRecordingButtonLabel : Styles.StartRecordingButtonLabel, FocusType.Keyboard, Styles.largeButton, GUILayout.MaxWidth(buttonSize.x)))
                {
                    if (isRecording)
                    {
                        m_StopRecording = true;
                    }
                    else
                    {
                        CodeCoverage.StartRecording();
                    }
                }
            }

            GUILayout.EndHorizontal();
        }

        private bool DoesResultsRootFolderExist()
        {
            if (m_CoverageSettings == null)
                return false;

            string resultsRootFolderPath = m_CoverageSettings.resultsRootFolderPath;
            return CoverageUtils.DoesFolderExistAndNotEmpty(resultsRootFolderPath);
        }

        private void ClearResultsRootFolderIfExists()
        {
            if (!EditorUtility.DisplayDialog(L10n.Tr("Clear Data"), L10n.Tr("Are you sure you would like to clear the Coverage data from previous test runs or from previous Coverage Recording sessions? Note that you cannot undo this action."), L10n.Tr("Clear"), L10n.Tr("Cancel")))
                return;

            if (m_CoverageSettings == null)
                return;

            string resultsRootFolderPath = m_CoverageSettings.resultsRootFolderPath;

            CoverageUtils.ClearFolderIfExists(resultsRootFolderPath);
        }

        private bool DoesReportHistoryExist()
        {
            if (m_CoverageSettings == null)
                return false;

            string rootFolderPath = m_CoverageSettings.rootFolderPath;
            string historyFolderPath = Path.Combine(rootFolderPath, CoverageSettings.ReportHistoryFolderName);

            return CoverageUtils.DoesFolderExistAndNotEmpty(historyFolderPath);
        }

        private void ClearReportHistoryFolderIfExists()
        {
            if (!EditorUtility.DisplayDialog(L10n.Tr("Clear History"), L10n.Tr("Are you sure you would like to clear the Coverage Report history? Note that you cannot undo this action."), L10n.Tr("Clear"), L10n.Tr("Cancel")))
                return;

            if (m_CoverageSettings == null)
                return;

            string rootFolderPath = m_CoverageSettings.rootFolderPath;
            string historyFolderPath = Path.Combine(rootFolderPath, CoverageSettings.ReportHistoryFolderName);

            CoverageUtils.ClearFolderIfExists(historyFolderPath);
        }

        void SetDefaultCoverageLocation()
        {
            string projectPath = CoverageUtils.StripAssetsFolderIfExists(Application.dataPath);
            if (CoverageUtils.IsValidFolder(projectPath))
            {
                m_CodeCoveragePath = CoverageUtils.NormaliseFolderSeparators(projectPath, true);
                EditorPrefs.SetString("CodeCoverageSettings.Path." + m_ProjectPathHash, m_CodeCoveragePath);
                UpdateCoverageSettings();
            }
        }

        string Browse(string directory)
        {
            if (string.IsNullOrEmpty(directory))
            {
                string variable = "ProgramFiles";
#if UNITY_EDITOR_OSX
                variable = "HOME";
#endif
                string candidateDirectory = Environment.GetEnvironmentVariable(variable);
                if (CoverageUtils.IsValidFolder(candidateDirectory))
                    directory = candidateDirectory;
            }

            directory = EditorUtility.OpenFolderPanel("Select Code Coverage destination directory", directory, string.Empty);
            if (!CoverageUtils.IsValidFolder(directory))
                return string.Empty;

            return directory;
        }

        bool MatchSearch(string searchContext, string content)
        {
            return content != null && searchContext != null && content.IndexOf(searchContext, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
