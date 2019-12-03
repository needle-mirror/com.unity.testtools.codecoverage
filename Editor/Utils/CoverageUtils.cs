using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UnityEditor.TestTools.CodeCoverage.Utils
{
    internal static class CoverageUtils
    {
        public static string NormaliseFolderSeparators(string folderPath, bool stripTrailingSlash = false)
        {
            if (folderPath != null)
            {
                folderPath = folderPath.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
                if (stripTrailingSlash)
                {
                    folderPath = folderPath.TrimEnd(Path.DirectorySeparatorChar);
                }
            }

            return folderPath;
        }

        public static bool EnsureFolderExists(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath))
                return false;

            if (!Directory.Exists(folderPath))
            {
                try
                {
                    Directory.CreateDirectory(folderPath);
                }
                catch (Exception)
                {
                    return false;
                }
            }

            return true;
        }

        public static string GetProjectFolderName(string projectPath)
        {
            if (projectPath == null)
                return null;

            string[] projectPathArray = CoverageUtils.NormaliseFolderSeparators(projectPath).Split(Path.DirectorySeparatorChar);
            string folderName = projectPathArray[projectPathArray.Length - 2];

            char[] invalidChars = Path.GetInvalidPathChars();
            StringBuilder folderNameStringBuilder = new StringBuilder();
            foreach (char c in folderName)
            {
                if (invalidChars.Contains(c))
                {
                    folderNameStringBuilder.Append('_');
                }
                else
                {
                    folderNameStringBuilder.Append(c);
                }
            }

            return folderNameStringBuilder.ToString();
        }

        public static string StripAssetsFolderIfExists(string folderPath)
        {
            if (folderPath != null)
            {
                string toTrim = "Assets";
                if (folderPath.EndsWith(toTrim))
                {
                    int startIndex = toTrim.Length;
                    return folderPath.Substring(0, folderPath.Length - startIndex);
                }
            }

            return folderPath;
        }

        public static string GetRootFolderPath(CoverageSettings coverageSettings)
        {
            string rootFolderPath = string.Empty;
            string coverageFolderPath = string.Empty;

            if (coverageSettings.resultsPathFromCommandLine.Length > 0)
            {
                coverageFolderPath = coverageSettings.resultsPathFromCommandLine;
                CoverageUtils.EnsureFolderExists(coverageFolderPath);
            }
            else
            {
                string projectPathHash = coverageSettings.projectPath.GetHashCode().ToString("X8");
                coverageFolderPath = EditorPrefs.GetString("CodeCoverageSettings.Path." + projectPathHash, string.Empty);
            }

            string projectPath = CoverageUtils.StripAssetsFolderIfExists(coverageSettings.projectPath);
            projectPath = CoverageUtils.NormaliseFolderSeparators(projectPath, true);

            if (CoverageUtils.IsValidFolder(coverageFolderPath))
            {
                coverageFolderPath = CoverageUtils.NormaliseFolderSeparators(coverageFolderPath, true);

                // Add 'CodeCoverage' directory if coverageFolderPath is projectPath
                if (string.Equals(coverageFolderPath, projectPath, StringComparison.InvariantCultureIgnoreCase))
                    rootFolderPath = Path.Combine(coverageFolderPath, coverageSettings.rootFolderName);
                // else user coverageFolderPath as the root folder
                else
                    rootFolderPath = coverageFolderPath;
            }
            else
            {
                // Add 'CodeCoverage' directory to projectPath if coverageFolderPath is not valid
                rootFolderPath = Path.Combine(projectPath, coverageSettings.rootFolderName);
            }
            return rootFolderPath;
        }

        public static int GetNumberOfXMLFilesInFolder(string folderPath)
        {
            if (folderPath == null)
                return 0;

            string[] files = Directory.GetFiles(folderPath, "*.xml", SearchOption.AllDirectories);
            int numFiles = files.Length;
            return numFiles;
        }

        public static void ClearFolderIfExists(string folderPath)
        {
            if (folderPath != null)
            {
                if (Directory.Exists(folderPath))
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(folderPath);
                    foreach (FileInfo file in dirInfo.GetFiles())
                    {
                        try
                        {
                            file.Delete();
                        }
                        catch (Exception)
                        {
                            Debug.LogWarning($"[{CoverageSettings.PackageName}] Failed to delete file: {file.FullName}");
                        }
                    }
                    foreach (DirectoryInfo dir in dirInfo.GetDirectories())
                    {
                        try
                        {
                            dir.Delete(true);
                        }
                        catch (Exception)
                        {
                            Debug.LogWarning($"[{CoverageSettings.PackageName}] Failed to delete directory: {dir.FullName}");
                        }
                    }
                }
            } 
        }

        public static bool DoesFolderExistAndNotEmpty(string folderPath)
        {
            if (folderPath != null)
            {
                if (Directory.Exists(folderPath))
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(folderPath);
                    return dirInfo.GetFiles().Length > 0 || dirInfo.GetDirectories().Length > 0;
                }
            }

            return false;
        }

        public static bool IsValidFolder(string folderPath)
        {
            return folderPath != null && !string.IsNullOrEmpty(folderPath) && Directory.Exists(folderPath);
        }

        private static HashSet<char> regexSpecialChars = new HashSet<char>(new[] { '[', '\\', '^', '$', '.', '|', '?', '*', '+', '(', ')' });

        public static string GlobToRegex(string glob)
        {
            var regex = new StringBuilder();
            var characterClass = false;
            regex.Append("^");
            foreach (var c in glob)
            {
                if (characterClass)
                {
                    if (c == ']')
                    {
                        characterClass = false;
                    }
                    regex.Append(c);
                    continue;
                }
                switch (c)
                {
                    case '*':
                        regex.Append(".*");
                        break;
                    case '?':
                        regex.Append(".");
                        break;
                    case '[':
                        characterClass = true;
                        regex.Append(c);
                        break;
                    default:
                        if (regexSpecialChars.Contains(c))
                        {
                            regex.Append('\\');
                        }
                        regex.Append(c);
                        break;
                }
            }
            regex.Append("$");
            return regex.ToString();
        }
    }
}
