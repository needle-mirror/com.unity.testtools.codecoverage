using System;
using System.IO;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor.TestTools.CodeCoverage.Utils;
using UnityEditor.TestTools.TestRunner;
using System.Text.RegularExpressions;

namespace UnityEditor.TestTools.CodeCoverage
{
    internal class CodeCoverageWindow : EditorWindow
    {
        private bool m_EnableCodeCoverage;
        private bool m_HasLatestScriptingRuntime;
        private string m_CodeCoveragePath;
        private string m_CodeCoverageHistoryPath;
        private CoverageFormat m_CodeCoverageFormat;
        private bool m_IncludeHistoryInReport;
        private string m_AssembliesToInclude;

        CoverageReportGenerator m_ReportGenerator;
        private bool m_GenerateHTMLReport;
        private bool m_GenerateBadge;
        private bool m_GenerateAdditionalMetrics;
        private bool m_AutoGenerateReport;

        private CoverageSettings m_CoverageSettings;

        private bool m_DoRepaint;
        private bool m_IncludeWarnings;
        private bool m_IncludeWarningsLast;
        private static readonly Vector2 m_WindowMinSizeNormal = new Vector2(430, 250);
        private static readonly Vector2 m_WindowMinSizeWithWarnings = new Vector2(430, 320);

        private bool m_GenerateReport = false;
        private bool m_StopRecording = false;
              
        private readonly string kLatestScriptingRuntimeMessage = L10n.Tr("Code Coverage requires the latest Scripting Runtime Version (.NET 4.x). You can set this in the Player Settings.");
        private readonly string kCodeCoverageDisabledMessage = L10n.Tr("Code Coverage is disabled. To enable Code Coverage, go to Preferences > General, check Enable Code Coverage, and restart Unity.");
        private readonly string kCodeCoverageDisabledNoRestartMessage = L10n.Tr("Code Coverage is disabled. To enable Code Coverage, go to Preferences > General and check Enable Code Coverage.");
        private readonly string kEnablingCodeCoverageMessage = L10n.Tr("Enabling Code Coverage will not take effect until Unity is restarted.");
        private readonly string kDisablingCodeCoverageMessage = L10n.Tr("Disabling Code Coverage will not take effect until Unity is restarted.");
        private readonly string kCodeOptimizationMessage = L10n.Tr("Code Coverage requires Code Optimization to be set to debug mode in order to obtain accurate coverage information.");
        private readonly string kSelectCoverageDirectoryMessage = L10n.Tr("Select the Code Coverage directory");
        private readonly string kSelectCoverageHistoryDirectoryMessage = L10n.Tr("Select the Coverage Report history directory");
        private readonly string kClearDataMessage = L10n.Tr("Are you sure you would like to clear the Coverage data from previous test runs or from previous Coverage Recording sessions? Note that you cannot undo this action.");
        private readonly string kClearHistoryMessage = L10n.Tr("Are you sure you would like to clear the coverage report history? Note that you cannot undo this action.");

        private void Update()
        {
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
                m_AssembliesToInclude = Regex.Replace(value, @"\s+", String.Empty);
                CoveragePreferences.instance.SetString("IncludeAssemblies", m_AssembliesToInclude);
            }
        }

        class Styles
        {
            static bool s_Initialized;

            public static readonly GUIContent OpenPreferencesButton = EditorGUIUtility.TrTextContent("Open Preferences");
            public static readonly GUIContent SwitchToDebugCodeOptimizationButton = EditorGUIUtility.TrTextContent("Switch to debug mode");
            public static readonly GUIContent CodeCoverageResultsLocationLabel = EditorGUIUtility.TrTextContent("Results Location", "Click the Browse button to specify the folder where the coverage results and report will be saved to. The default location is the Project's folder.");
            public static readonly GUIContent CodeCoverageHistoryLocationLabel = EditorGUIUtility.TrTextContent("History Location", "Click the Browse button to specify the folder where the coverage report history will be saved to. The default location is the Project's folder.");
            public static readonly GUIContent CoverageOptionsLabel = EditorGUIUtility.TrTextContent("Settings");
            public static readonly GUIContent CodeCoverageFormat = EditorGUIUtility.TrTextContent("Coverage Format", "The Code Coverage format used when saving the results.");
            public static readonly GUIContent GenerateAdditionalMetricsLabel = EditorGUIUtility.TrTextContent("Generate Additional Metrics", "Check this to generate and include additional metrics in the HTML report. These currently include Cyclomatic Complexity and Crap Score calculations for each method.");
            public static readonly GUIContent CoverageHistoryLabel = EditorGUIUtility.TrTextContent("Generate History", "Check this to generate and include the coverage history in the HTML report.");
            public static readonly GUIContent AssembliesToIncludeLabel = EditorGUIUtility.TrTextContent("Included Assemblies", "Specify the assemblies that will be included in the coverage results. This is a comma separated string. Click the Select button to view and easily select or deselect the assemblies.");
            public static readonly GUIContent AssembliesToIncludeDropdownLabel = EditorGUIUtility.TrTextContent("Select", "Click this to view and easily select or deselect the assemblies that will be included in the coverage results.");
            public static readonly GUIContent BrowseButtonLabel = EditorGUIUtility.TrTextContent("Browse", "Click this to specify the folder where the coverage results and report will be saved to.");
            public static readonly GUIContent GenerateHTMLReportLabel = EditorGUIUtility.TrTextContent("Generate HTML Report", "Check this to generate an HTML version of the report.");
            public static readonly GUIContent GenerateBadgeReportLabel = EditorGUIUtility.TrTextContent("Generate Summary Badges", "Check this to generate coverage summary badges in SVG and PNG format.");
            public static readonly GUIContent AutoGenerateReportLabel = EditorGUIUtility.TrTextContent("Auto Generate Report", "Check this to generate the report automatically after the Test Runner has finished running the tests or the Coverage Recording session has completed.");
            public static readonly GUIContent GenerateReportButtonLabel = EditorGUIUtility.TrTextContent("Generate from Last", "Generates a coverage report from the last set of tests that were run in the Test Runner or from the last Coverage Recording session.");
            public static readonly GUIContent ClearCoverageButtonLabel = EditorGUIUtility.TrTextContent("Clear Data", "Clears the Coverage data from previous test runs for both EditMode and PlayMode tests or from previous Coverage Recording sessions, for the current project.");
            public static readonly GUIContent ClearHistoryButtonLabel = EditorGUIUtility.TrTextContent("Clear History", "Clears the coverage report history.");
            public static readonly GUIContent StartRecordingButtonLabel = EditorGUIUtility.TrTextContentWithIcon(" Start Recording", "Record coverage data.", "Record Off");
            public static readonly GUIContent StopRecordingButtonLabel = EditorGUIUtility.TrTextContentWithIcon(" Stop Recording", "Stop recording coverage data.", "Record On");

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
                resultsPathFromCommandLine = string.Empty,
                historyPathFromCommandLine = string.Empty
            };

            m_CodeCoveragePath = CoveragePreferences.instance.GetString("Path", string.Empty);
            m_CodeCoverageHistoryPath = CoveragePreferences.instance.GetString("HistoryPath", string.Empty);
            m_CodeCoverageFormat = (CoverageFormat)CoveragePreferences.instance.GetInt("Format", 0);
            m_GenerateAdditionalMetrics = CoveragePreferences.instance.GetBool("GenerateAdditionalMetrics", false);
            m_IncludeHistoryInReport = CoveragePreferences.instance.GetBool("IncludeHistoryInReport", true);
            m_AssembliesToInclude = CoveragePreferences.instance.GetString("IncludeAssemblies", AssemblyFiltering.GetUserOnlyAssembliesString());
            m_ReportGenerator = new CoverageReportGenerator();
            m_GenerateHTMLReport = CoveragePreferences.instance.GetBool("GenerateHTMLReport", true);
            m_GenerateBadge = CoveragePreferences.instance.GetBool("GenerateBadge", true);
            m_AutoGenerateReport = CoveragePreferences.instance.GetBool("AutoGenerateReport", true);

            UpdateCoverageSettings();
            RefreshCodeCoverageWindow();

            m_IncludeWarnings = false;
            m_IncludeWarningsLast = false;
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
                m_CoverageSettings.historyFolderPath = CoverageUtils.GetHistoryFolderPath(m_CoverageSettings);

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

            ResetIncludeWarnings();

            CheckScriptingRuntimeVersion();
            CheckCoverageEnabled();
            CheckCodeOptimization();

            using (new EditorGUI.DisabledScope(!m_EnableCodeCoverage || 
                                                m_EnableCodeCoverage != Coverage.enabled || 
                                                !m_HasLatestScriptingRuntime))
            {
                DrawCodeCoverageLocation();
                DrawCodeCoverageHistoryLocation();
                DrawCoverageSettings();
                DrawButtons();
            }

            GUILayout.EndVertical();

            HandleWarningsRepaint(); 
        }

        void ResetIncludeWarnings()
        {
            m_IncludeWarningsLast = m_IncludeWarnings;
            m_IncludeWarnings = false;
        }

        void HandleWarningsRepaint()
        {
            if (m_IncludeWarnings != m_IncludeWarningsLast)
            {
                m_DoRepaint = true;
            }

            if (m_DoRepaint)
            {
                RefreshCodeCoverageWindow();
                m_DoRepaint = false;
            }
        }

        void CheckScriptingRuntimeVersion()
        {
#if UNITY_2019_3_OR_NEWER
            m_HasLatestScriptingRuntime = true;
#else
            m_HasLatestScriptingRuntime = PlayerSettings.scriptingRuntimeVersion == ScriptingRuntimeVersion.Latest;
#endif

            if (!m_HasLatestScriptingRuntime)
            {
                EditorGUILayout.HelpBox(kLatestScriptingRuntimeMessage, MessageType.Warning);
                GUILayout.Space(5);

                m_IncludeWarnings = true;
            }
        }

        void CheckCoverageEnabled()
        {
            m_EnableCodeCoverage = EditorPrefs.GetBool("CodeCoverageEnabled", false);

#if UNITY_2020_2_OR_NEWER
            if (!m_EnableCodeCoverage)
            {
                EditorGUILayout.HelpBox(kCodeCoverageDisabledNoRestartMessage, MessageType.Warning);

                if (GUILayout.Button(Styles.OpenPreferencesButton))
                    SettingsService.OpenUserPreferences("Preferences/_General");

                GUILayout.Space(5);

                m_IncludeWarnings = true;
            }
#else
            if (m_EnableCodeCoverage != Coverage.enabled)
            {
                if (m_EnableCodeCoverage)
                    EditorGUILayout.HelpBox(kEnablingCodeCoverageMessage, MessageType.Warning);
                else
                    EditorGUILayout.HelpBox(kDisablingCodeCoverageMessage, MessageType.Warning);

                GUILayout.Space(5);

                m_IncludeWarnings = true;
            }
            else if (!m_EnableCodeCoverage)
            {
                EditorGUILayout.HelpBox(kCodeCoverageDisabledMessage, MessageType.Warning);

                if (GUILayout.Button(Styles.OpenPreferencesButton))
                    SettingsService.OpenUserPreferences("Preferences/_General");

                GUILayout.Space(5);

                m_IncludeWarnings = true;
            }
#endif
        }

        void CheckCodeOptimization()
        {
#if UNITY_2020_1_OR_NEWER
            if (Compilation.CompilationPipeline.codeOptimization == Compilation.CodeOptimization.Release)
            {
                EditorGUILayout.HelpBox(kCodeOptimizationMessage, MessageType.Warning);
                m_IncludeWarnings = true;

                if (GUILayout.Button(Styles.SwitchToDebugCodeOptimizationButton))
                {
                    Compilation.CompilationPipeline.codeOptimization = Compilation.CodeOptimization.Debug;
                    EditorPrefs.SetBool("ScriptDebugInfoEnabled", true);
                    m_IncludeWarnings = false;
                    m_DoRepaint = true;
                }

                GUILayout.Space(5);
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
                    string candidate = Browse(m_CodeCoveragePath, kSelectCoverageDirectoryMessage);
                    if (CoverageUtils.IsValidFolder(candidate))
                    {
                        m_CodeCoveragePath = CoverageUtils.NormaliseFolderSeparators(candidate, true);
                        CoveragePreferences.instance.SetString("Path", m_CodeCoveragePath);

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

        void DrawCodeCoverageHistoryLocation()
        {
            GUILayout.BeginHorizontal();

            using (new EditorGUI.DisabledScope(CoverageRunData.instance.isRunning))
            {
                Rect textFieldPosition = EditorGUILayout.GetControlRect();
                textFieldPosition = EditorGUI.PrefixLabel(textFieldPosition, Styles.CodeCoverageHistoryLocationLabel);
                EditorGUI.SelectableLabel(textFieldPosition, m_CodeCoverageHistoryPath, EditorStyles.textField);

                bool autoDetect = !CoverageUtils.IsValidFolder(m_CodeCoverageHistoryPath);

                if (autoDetect)
                {
                    SetDefaultCoverageHistoryLocation();
                }

                Vector2 buttonSize = EditorStyles.miniButton.CalcSize(Styles.BrowseButtonLabel);
                if (EditorGUILayout.DropdownButton(Styles.BrowseButtonLabel, FocusType.Keyboard, EditorStyles.miniButton, GUILayout.MaxWidth(buttonSize.x)))
                {
                    string candidate = Browse(m_CodeCoverageHistoryPath, kSelectCoverageHistoryDirectoryMessage);
                    if (CoverageUtils.IsValidFolder(candidate))
                    {
                        m_CodeCoverageHistoryPath = CoverageUtils.NormaliseFolderSeparators(candidate, true);
                        CoveragePreferences.instance.SetString("HistoryPath", m_CodeCoverageHistoryPath);

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
                CoveragePreferences.instance.SetInt("Format", (int)m_CodeCoverageFormat);

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
                m_GenerateHTMLReport = EditorGUILayout.Toggle(Styles.GenerateHTMLReportLabel, m_GenerateHTMLReport, GUILayout.ExpandWidth(false));
                if (EditorGUI.EndChangeCheck())
                {
                    CoveragePreferences.instance.SetBool("GenerateHTMLReport", m_GenerateHTMLReport);
                }

                EditorGUIUtility.labelWidth = 170f;

                GUILayout.BeginHorizontal();
                GUILayout.Space(23);

                EditorGUI.BeginChangeCheck();

                using (new EditorGUI.DisabledScope(!m_GenerateHTMLReport))
                {
                    m_IncludeHistoryInReport = EditorGUILayout.Toggle(Styles.CoverageHistoryLabel, m_IncludeHistoryInReport, GUILayout.ExpandWidth(false));
                    if (EditorGUI.EndChangeCheck())
                    {
                        CoveragePreferences.instance.SetBool("IncludeHistoryInReport", m_IncludeHistoryInReport);
                    }
                }

                GUILayout.EndHorizontal();

                EditorGUIUtility.labelWidth = 190f;

                EditorGUI.BeginChangeCheck();
                m_GenerateBadge = EditorGUILayout.Toggle(Styles.GenerateBadgeReportLabel, m_GenerateBadge, GUILayout.ExpandWidth(false));
                if (EditorGUI.EndChangeCheck())
                {
                    CoveragePreferences.instance.SetBool("GenerateBadge", m_GenerateBadge);
                }

                EditorGUI.BeginChangeCheck();
                m_GenerateAdditionalMetrics = EditorGUILayout.Toggle(Styles.GenerateAdditionalMetricsLabel, m_GenerateAdditionalMetrics, GUILayout.ExpandWidth(false));
                if (EditorGUI.EndChangeCheck())
                {
                    CoveragePreferences.instance.SetBool("GenerateAdditionalMetrics", m_GenerateAdditionalMetrics);
                }

                EditorGUI.BeginChangeCheck();
                m_AutoGenerateReport = EditorGUILayout.Toggle(Styles.AutoGenerateReportLabel, m_AutoGenerateReport, GUILayout.ExpandWidth(false));
                if (EditorGUI.EndChangeCheck())
                {
                    CoveragePreferences.instance.SetBool("AutoGenerateReport", m_AutoGenerateReport);
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
            if (!EditorUtility.DisplayDialog(L10n.Tr("Clear Data"), kClearDataMessage, L10n.Tr("Clear"), L10n.Tr("Cancel")))
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

            string historyFolderPath = m_CoverageSettings.historyFolderPath;

            return CoverageUtils.DoesFolderExistAndNotEmpty(historyFolderPath);
        }

        private void ClearReportHistoryFolderIfExists()
        {
            if (!EditorUtility.DisplayDialog(L10n.Tr("Clear History"), kClearHistoryMessage, L10n.Tr("Clear"), L10n.Tr("Cancel")))
                return;

            if (m_CoverageSettings == null)
                return;

            string historyFolderPath = m_CoverageSettings.historyFolderPath;

            CoverageUtils.ClearFolderIfExists(historyFolderPath);
        }

        void SetDefaultCoverageLocation()
        {
            string projectPath = CoverageUtils.StripAssetsFolderIfExists(Application.dataPath);
            if (CoverageUtils.IsValidFolder(projectPath))
            {
                m_CodeCoveragePath = CoverageUtils.NormaliseFolderSeparators(projectPath, true);
                CoveragePreferences.instance.SetString("Path", m_CodeCoveragePath);
                UpdateCoverageSettings();
            }
        }

        void SetDefaultCoverageHistoryLocation()
        {
            string projectPath = CoverageUtils.StripAssetsFolderIfExists(Application.dataPath);
            if (CoverageUtils.IsValidFolder(projectPath))
            {
                m_CodeCoverageHistoryPath = CoverageUtils.NormaliseFolderSeparators(projectPath, true);
                CoveragePreferences.instance.SetString("HistoryPath", m_CodeCoverageHistoryPath);
                UpdateCoverageSettings();
            }
        }

        string Browse(string directory, string title)
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

            directory = EditorUtility.OpenFolderPanel(title, directory, string.Empty);
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
