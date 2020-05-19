using UnityEngine;
using UnityEditor.SettingsManagement;

namespace UnityEditor.TestTools.CodeCoverage
{
    internal class CoveragePreferences : CoveragePreferencesImplementation
    {
        private static CoveragePreferences s_Instance = null;
        private const string k_PreferencesBaseName = "CodeCoverageSettings";
        private const string k_PackageName = "com.unity.testtools.codecoverage";

        public static CoveragePreferences instance
        {
            get
            {
                if (s_Instance == null)
                    s_Instance = new CoveragePreferences();

                return s_Instance;
            }
        }

        protected CoveragePreferences() : base(k_PackageName, k_PreferencesBaseName, Application.dataPath.GetHashCode().ToString("X8"))
        {
        }
    }

    internal class CoveragePreferencesImplementation
    {
        private string m_PreferencesBaseName;
        private string m_ProjectPathHash;
        protected Settings m_Settings;

        public CoveragePreferencesImplementation(string packageName, string preferencesBaseName, string projectPathHash)
        {
            m_ProjectPathHash = projectPathHash;
            m_PreferencesBaseName = preferencesBaseName;
            m_Settings = new Settings(packageName);
        }

        private string GetEditorPrefKey(string key)
        {
            return $"{m_PreferencesBaseName}.{key}.{m_ProjectPathHash}";
        }

        public bool GetBool(string key, bool defaultValue, SettingsScope scope = SettingsScope.Project)
        {
            if (m_Settings.ContainsKey<bool>(key, scope))
            {
                return m_Settings.Get<bool>(key, scope, defaultValue);
            }

            string editorPrefKey = GetEditorPrefKey(key);
            if (EditorPrefs.HasKey(editorPrefKey))
            {
                bool value = EditorPrefs.GetBool(editorPrefKey, defaultValue);
                m_Settings.Set<bool>(key, value, scope);
                m_Settings.Save();
                return value;
            }

            return defaultValue;
        }

        public int GetInt(string key, int defaultValue, SettingsScope scope = SettingsScope.Project)
        {
            if (m_Settings.ContainsKey<int>(key, scope))
            {
                return m_Settings.Get<int>(key, scope, defaultValue);
            }

            string editorPrefKey = GetEditorPrefKey(key);
            if (EditorPrefs.HasKey(editorPrefKey))
            {
                int value = EditorPrefs.GetInt(editorPrefKey, defaultValue);
                m_Settings.Set<int>(key, value, scope);
                m_Settings.Save();
                return value;
            }

            return defaultValue;
        }

        public string GetString(string key, string defaultValue, SettingsScope scope = SettingsScope.Project)
        {
            if (m_Settings.ContainsKey<string>(key, scope))
            {
                return m_Settings.Get<string>(key, scope, defaultValue);
            }

            string editorPrefKey = GetEditorPrefKey(key);
            if (EditorPrefs.HasKey(editorPrefKey))
            {
                string value = EditorPrefs.GetString(editorPrefKey, defaultValue);
                m_Settings.Set<string>(key, value, scope);
                m_Settings.Save();
                return value;
            }

            return defaultValue;
        }

        public void SetBool(string key, bool value, SettingsScope scope = SettingsScope.Project)
        {
            m_Settings.Set<bool>(key, value, scope);
            m_Settings.Save();
        }

        public void SetInt(string key, int value, SettingsScope scope = SettingsScope.Project)
        {
            m_Settings.Set<int>(key, value, scope);
            m_Settings.Save();
        }

        public void SetString(string key, string value, SettingsScope scope = SettingsScope.Project)
        {
            m_Settings.Set<string>(key, value, scope);
            m_Settings.Save();
        }

        public void DeleteBool(string key, SettingsScope scope = SettingsScope.Project)
        {
            m_Settings.DeleteKey<bool>(key, scope);
            m_Settings.Save();
        }

        public void DeleteInt(string key, SettingsScope scope = SettingsScope.Project)
        {
            m_Settings.DeleteKey<int>(key, scope);
            m_Settings.Save();
        }

        public void DeleteString(string key, SettingsScope scope = SettingsScope.Project)
        {
            m_Settings.DeleteKey<string>(key, scope);
            m_Settings.Save();
        }
    }
}
