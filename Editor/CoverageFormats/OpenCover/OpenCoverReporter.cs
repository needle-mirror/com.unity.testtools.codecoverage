using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor.TestTools.TestRunner.Api;
using OpenCover.Framework.Model;
using Module = OpenCover.Framework.Model.Module;
using ModelFile = OpenCover.Framework.Model.File;
using File = System.IO.File;

namespace UnityEditor.TestTools.CodeCoverage.OpenCover
{
    internal class OpenCoverReporter : ICoverageReporter
    {
        class Styles
        {
            public static GUIContent ProgressTitle = EditorGUIUtility.TrTextContent("Code Coverage");
            public static GUIContent ProgressGatheringResults = EditorGUIUtility.TrTextContent("Gathering Coverage results..");
            public static GUIContent ProgressWritingFile = EditorGUIUtility.TrTextContent("Writing Coverage results to file..");
        }

        private bool m_OutputPerTest = false;
        private bool m_OutputCyclomaticComplexity = false;

        private CoverageSettings m_CoverageSettings;
        private AssemblyFiltering m_AssemblyFiltering;
        private OpenCoverResultWriter m_Writer;

        public void OnBeforeAssemblyReload()
        {
            OutputCoverageReport();
        }

        public void OnCoverageRecordingPaused()
        {
            OutputCoverageReport();
        }

        public void OnInitialise(CoverageSettings settings)
        {
            m_CoverageSettings = settings;

            if (!m_OutputPerTest && m_CoverageSettings.resetCoverageData)
            {
                Coverage.ResetAll();
            }

            SetupAssemblyFiltering();
            m_OutputCyclomaticComplexity = ShouldOutputCyclomaticComplexity();

            if (m_Writer == null)
            {
                m_Writer = new OpenCoverResultWriter(m_CoverageSettings);
            }
            m_Writer.SetupCoveragePaths();
        }

        public void OnRunStarted(ITestAdaptor testsToRun)
        {
            if (m_Writer != null)
            {
                m_Writer.ClearCoverageFolderIfExists();
                m_Writer.SetupCoveragePaths();
            }
        }

        public void OnRunFinished(ITestResultAdaptor testResults)
        {
            if (!m_OutputPerTest)
            {
                OutputCoverageReport(testResults, false);
            }
        }

        public void OnTestStarted(ITestAdaptor test)
        {
            if (m_OutputPerTest && m_CoverageSettings.resetCoverageData)
            {
                Coverage.ResetAll();
            }
        }

        public void OnTestFinished(ITestResultAdaptor result)
        {
            if (m_OutputPerTest)
            {
                OutputCoverageReport(result);
            }
        }

        public void OutputCoverageReport(ITestResultAdaptor testResults = null, bool clearProgressBar = true)
        {
            if (!CommandLineManager.instance.runFromCommandLine)
                EditorUtility.DisplayProgressBar(Styles.ProgressTitle.text, Styles.ProgressWritingFile.text, 0.95f);

            CoverageSession coverageSession = GenerateOpenCoverSession();
            if (coverageSession != null && m_Writer != null)
            {
                m_Writer.CoverageSession = coverageSession;
                m_Writer.WriteCoverageSession();
            }
            else
            {
                Debug.LogWarning("[Code Coverage] No coverage results were saved.");
            }

            if (clearProgressBar)
                EditorUtility.ClearProgressBar();
        }

        private bool ShouldProcessAssembly(string assemblyName)
        {
            if (CommandLineManager.instance.runFromCommandLine)
                return CommandLineManager.instance.assemblyFiltering.IsAssemblyIncluded(assemblyName);

            return m_AssemblyFiltering.IsAssemblyIncluded(assemblyName);
        }

        private bool ShouldProcessFile(string filename)
        {
            // PathFiltering is implemented only via the command line.
            // Will assess whether PathFiltering is needed to be set via the UI too (similar to Assembly Filtering).
            if (CommandLineManager.instance.runFromCommandLine)
                return CommandLineManager.instance.pathFiltering.IsPathIncluded(filename);
            else
                return true;
        }

        private bool IsSpecialMethod(MethodBase methodBase)
        {
            return methodBase.IsSpecialName && ((methodBase.Attributes & MethodAttributes.HideBySig) != 0);
        }

        private bool IsConstructor(MethodBase methodBase)
        {
            return IsSpecialMethod(methodBase) && methodBase.MemberType == MemberTypes.Constructor && methodBase.Name == ".ctor";
        }

        private bool IsStaticConstructor(MethodBase methodBase)
        {
            return IsSpecialMethod(methodBase) && methodBase.IsStatic && methodBase.MemberType == MemberTypes.Constructor && methodBase.Name == ".cctor";
        }

        private bool IsPropertySetter(MethodBase methodBase)
        {
            return IsSpecialMethod(methodBase) && methodBase.Name.Contains("set_");
        }

        private bool IsPropertyGetter(MethodBase methodBase)
        {
            return IsSpecialMethod(methodBase) && methodBase.Name.Contains("get_");
        }

        private bool IsOperator(MethodBase methodBase)
        {
            return IsSpecialMethod(methodBase) && methodBase.Name.Contains("op_");
        }

        private string GenerateOperatorName(MethodBase methodBase)
        {
            string operatorName = string.Empty;

            switch (methodBase.Name)
            {
                case "op_Implicit":
                    operatorName = string.Format("implicit operator {0}", GetReturnTypeName(methodBase));
                    break;

                case "op_Explicit":
                    operatorName = string.Format("explicit operator {0}", GetReturnTypeName(methodBase));
                    break;

                case "op_Addition":
                case "op_UnaryPlus":
                    operatorName = "operator+";
                    break;

                case "op_Increment":
                    operatorName = "operator++";
                    break;

                case "op_Subtraction":
                case "op_UnaryNegation":
                    operatorName = "operator-";
                    break;

                case "op_Decrement":
                    operatorName = "operator--";
                    break;

                case "op_Multiply":
                    operatorName = "operator*";
                    break;

                case "op_Division":
                    operatorName = "operator/";
                    break;

                case "op_Modulus":
                    operatorName = "operator%";
                    break;

                case "op_ExclusiveOr":
                    operatorName = "operator^";
                    break;

                case "op_BitwiseAnd":
                    operatorName = "operator&";
                    break;

                case "op_BitwiseOr":
                    operatorName = "operator|";
                    break;

                case "op_LeftShift":
                    operatorName = "operator<<";
                    break;

                case "op_RightShift":
                    operatorName = "operator>>";
                    break;

                case "op_Equality":
                    operatorName = "operator==";
                    break;

                case "op_Inequality":
                    operatorName = "operator!=";
                    break;

                case "op_GreaterThan":
                    operatorName = "operator>";
                    break;

                case "op_LessThan":
                    operatorName = "operator<";
                    break;

                case "op_GreaterThanOrEqual":
                    operatorName = "operator>=";
                    break;

                case "op_LessThanOrEqual":
                    operatorName = "operator<=";
                    break;

                case "op_OnesComplement":
                    operatorName = "operator~";
                    break;

                case "op_LogicalNot":
                    operatorName = "operator!";
                    break;

                case "op_True":
                    operatorName = "operator true";
                    break;

                case "op_False":
                    operatorName = "operator false";
                    break;

                default:
                    operatorName = string.Format("unknown operator {0}", methodBase.Name);
                    break;
            }

            return operatorName;
        }

        private string GetReturnTypeName(MethodBase methodBase)
        {
            string returnTypeName = string.Empty;
            MethodInfo methodInfo = methodBase as MethodInfo;
            if (methodInfo != null)
            {
                returnTypeName = GenerateTypeName(methodInfo.ReturnType);
            }

            return returnTypeName;
        }

        internal string GenerateMethodName(MethodBase methodBase)
        {
            StringBuilder sb = new StringBuilder();

            if (methodBase.IsStatic)
            {
                sb.Append("static ");
            }

            string returnTypeName = GetReturnTypeName(methodBase);
            if (returnTypeName != string.Empty)
            {
                sb.Append(returnTypeName);
                sb.Append(' ');
            }

            StringBuilder methodStringBuilder = new StringBuilder();

            methodStringBuilder.Append(GenerateTypeName(methodBase.DeclaringType));
            methodStringBuilder.Append(".");

            if (IsConstructor(methodBase) || IsStaticConstructor(methodBase))
            {
                methodStringBuilder.Append(GenerateConstructorName(methodBase.DeclaringType));
            }
            else if (IsOperator(methodBase))
            {
                methodStringBuilder.Append(GenerateOperatorName(methodBase));
            }
            else
            {
                methodStringBuilder.Append(methodBase.Name);
            }

            if ( methodStringBuilder.Length > 0)
            {
                int lastDotPos = -1;
                for (int i = methodStringBuilder.Length - 1; i >= 0; i--)
                {
                    if (methodStringBuilder[i] == '.')
                    {
                        lastDotPos = i;
                        break;
                    }
                }

                if (lastDotPos != -1 )
                {
                    methodStringBuilder.Remove(lastDotPos, 1);
                    methodStringBuilder.Insert(lastDotPos, "::");
                }
            }

            sb.Append(methodStringBuilder.ToString());

            if (methodBase.IsGenericMethodDefinition)
            {
                Type[] types = methodBase.GetGenericArguments();
                sb.Append(GenerateGenericTypeList(types));
            }
            sb.Append('(');
            ParameterInfo[] parameterInfos = methodBase.GetParameters();
            for (int i=0; i<parameterInfos.Length; ++i)
            {
                sb.Append(GenerateTypeName(parameterInfos[i].ParameterType));

                if (i != parameterInfos.Length - 1)
                    sb.Append(", ");
            }
            sb.Append(')');

            return sb.ToString();
        }

        private string GenerateTypeName(Type type)
        {
            StringBuilder sb = new StringBuilder();
            if (type != null)
            {
                if (type.IsGenericTypeDefinition || type.IsGenericType)
                {
                    sb.Append(GenerateGenericTypeName(type));
                }
                else if (type.IsGenericParameter)
                {
                    sb.Append(type.Name);
                }
                else
                {
                    sb.Append(type.FullName);
                }

                // Replace + with / so as nested classes appear in the same file
                sb.Replace('+', '/');
            }

            return sb.ToString();
        }

        private string GenerateGenericTypeName(Type type)
        {
            StringBuilder sb = new StringBuilder();
            if (type != null)
            {
                if (type.IsGenericTypeDefinition || type.IsGenericType)
                {
                    // When IsGenericType the FullName includes unnecessary information and thus cannot be used.
                    // Therefore we use the Name instead and add the Namespace at the beginning
                    if (!type.IsGenericTypeDefinition && type.IsGenericType && type.Namespace != string.Empty)
                    {
                        sb.Append(type.Namespace);
                        sb.Append('.');
                    }

                    string[] splitTypes = type.IsGenericTypeDefinition ? type.FullName.Split('+') : type.Name.Split('+');
                    Type[] genericTypeArguments = type.GetGenericArguments();
                    int genericTypeArgumentIndex = 0;

                    int numOfTypes = splitTypes.Length;
                    for (int i = 0; i < numOfTypes; ++i)
                    {
                        string splitType = splitTypes[i];

                        int genericSeparatorIndex = splitType.LastIndexOf('`');
                        if (genericSeparatorIndex != -1)
                        {
                            sb.Append(splitType.Substring(0, genericSeparatorIndex));
                            string argumentIndexStr = splitType.Substring(genericSeparatorIndex+1);

                            int numOfArguments;
                            if (Int32.TryParse(argumentIndexStr, out numOfArguments))
                            {
                                sb.Append("[");
                                for (int j = 0; j < numOfArguments; ++j)
                                {
                                    if (genericTypeArgumentIndex < genericTypeArguments.Length)
                                    {
                                        sb.Append($"{genericTypeArguments[genericTypeArgumentIndex++].Name}");

                                        if (j < numOfArguments - 1)
                                            sb.Append(",");
                                    }
                                }
                                sb.Append("]");
                            }

                            if (i < numOfTypes - 1)
                                sb.Append("/");
                        }
                        else
                        {
                            sb.Append(splitType);
                        }
                    }
                }
            }

            return sb.ToString();
        }

        private string GenerateConstructorName(Type type)
        {
            StringBuilder sb = new StringBuilder();

            string typeName = type.Name;

            int genericSeparatorIndex = typeName.LastIndexOf('`');
            if (genericSeparatorIndex != -1)
            {
                int nestedGenericSeparatorIndex = typeName.LastIndexOf('+');

                sb.Append(typeName.Substring(0, genericSeparatorIndex));
            }
            else
            {
                sb.Append(typeName);
            }

            return sb.ToString();
        }

        private string GenerateGenericTypeList(Type[] genericTypes)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append('[');
            for (int i = 0; i < genericTypes.Length; ++i)
            {
                sb.Append(genericTypes[i].Name);

                if (i != genericTypes.Length - 1)
                    sb.Append(", ");
            }
            sb.Append(']');

            return sb.ToString();
        }

        private bool ShouldOutputCyclomaticComplexity()
        {
            bool shouldOutputCyclomaticComplexity = CommandLineManager.instance.enableCyclomaticComplexity;

            if (!shouldOutputCyclomaticComplexity)
            {
                string projectPathHash = Application.dataPath.GetHashCode().ToString("X8");
                shouldOutputCyclomaticComplexity = EditorPrefs.GetBool("CodeCoverageSettings.EnableCyclomaticComplexity." + projectPathHash, false);
            }
            return shouldOutputCyclomaticComplexity;
        }

        private void SetupAssemblyFiltering()
        {
            m_AssemblyFiltering = new AssemblyFiltering();

            string projectPathHash = Application.dataPath.GetHashCode().ToString("X8");
            string includeAssemblies = EditorPrefs.GetString("CodeCoverageSettings.IncludeAssemblies." + projectPathHash, AssemblyFiltering.GetUserOnlyAssembliesString());

            m_AssemblyFiltering.Parse(includeAssemblies, AssemblyFiltering.kDefaultExcludedAssemblies);
        }

        private CoverageSession GenerateOpenCoverSession()
        {
            CoverageSession coverageSession = null;

            UInt32 fileUID = 0;
            List<Module> moduleList = new List<Module>();

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            float progressInterval = 0.9f / assemblies.Length;
            float currentProgress = 0.0f;

            foreach (Assembly assembly in assemblies)
            {
                if (!CommandLineManager.instance.runFromCommandLine)
                    EditorUtility.DisplayProgressBar(Styles.ProgressTitle.text, Styles.ProgressGatheringResults.text, currentProgress);
                currentProgress += progressInterval;

                string assemblyName = assembly.GetName().Name.ToLower();
                if (!ShouldProcessAssembly(assemblyName))
                {
                    continue;
                }

                List<Class> coveredClasses = new List<Class>();
                List<string> filesNotFound = new List<string>();
                Dictionary<string, UInt32> fileList = new Dictionary<string, UInt32>();
                Type[] assemblyTypes = null;

                try
                {
                    assemblyTypes = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    // This exception can be thrown if some of the types from this assembly can't be loaded. If this
                    // happens, the Types property array contains a Type for all loaded types and null for each
                    // type that couldn't be loaded.
                    assemblyTypes = ex.Types;
                    ShouldProcessAssembly(assemblyName);
                }

                Debug.Assert(assemblyTypes != null);

                foreach (Type type in assemblyTypes)
                {
                    // The type can be null if the ReflectionTypeLoadException has been thrown previously.
                    if (type == null)
                    {
                        continue;
                    }

                    CoveredMethodStats[] classMethodStatsArray = Coverage.GetStatsFor(type);
                    if (classMethodStatsArray.Length > 0)
                    {
                        List<Method> coveredMethods = new List<Method>();

                        foreach (CoveredMethodStats classMethodStats in classMethodStatsArray)
                        {
                            if (classMethodStats.totalSequencePoints > 0)
                            {
                                List<SequencePoint> coveredSequencePoints = new List<SequencePoint>();

                                uint fileId = 0;
                                CoveredSequencePoint[] classMethodSequencePointsArray = Coverage.GetSequencePointsFor(classMethodStats.method);
                                foreach (CoveredSequencePoint classMethodSequencePoint in classMethodSequencePointsArray)
                                {
                                    string filename = classMethodSequencePoint.filename;
                                    if (filesNotFound.Contains(filename) || !ShouldProcessFile(filename))
                                        continue;

                                    if (!fileList.TryGetValue(filename, out fileId))
                                    {
                                        if (!File.Exists(filename))
                                        {
                                            filesNotFound.Add(filename);
                                            continue;
                                        }
                                        else
                                        {
                                            fileId = ++fileUID;
                                            fileList.Add(filename, fileId);
                                        }
                                    }

                                    SequencePoint coveredSequencePoint = new SequencePoint();
                                    coveredSequencePoint.FileId = fileId;
                                    coveredSequencePoint.StartLine = (int)classMethodSequencePoint.line;
                                    coveredSequencePoint.StartColumn = (int)classMethodSequencePoint.column;
                                    coveredSequencePoint.EndLine = (int)classMethodSequencePoint.line;
                                    coveredSequencePoint.EndColumn = (int)classMethodSequencePoint.column;
                                    coveredSequencePoint.VisitCount = (int)classMethodSequencePoint.hitCount;
                                    coveredSequencePoint.Offset = (int)classMethodSequencePoint.ilOffset;
                                    coveredSequencePoints.Add(coveredSequencePoint);
                                }

                                if (coveredSequencePoints.Count > 0)
                                {
                                    Method coveredMethod = new Method();
                                    MethodBase methodBase = classMethodStats.method;
                                    coveredMethod.MetadataToken = methodBase.MetadataToken;
                                    coveredMethod.FullName = GenerateMethodName(methodBase);
                                    coveredMethod.FileRef = new FileRef() { UniqueId = fileId };
                                    coveredMethod.Visited = (classMethodStats.uncoveredSequencePoints != classMethodStats.totalSequencePoints) ? true : false;
                                    coveredMethod.IsConstructor = IsConstructor(methodBase) || IsStaticConstructor(methodBase);
                                    coveredMethod.IsStatic = methodBase.IsStatic;
                                    coveredMethod.IsSetter = IsPropertySetter(methodBase);
                                    coveredMethod.IsGetter = IsPropertyGetter(methodBase);
                                    coveredMethod.SequencePoints = coveredSequencePoints.ToArray();
                                    decimal sequenceCoverage = decimal.Round(100.0m * (decimal)(classMethodStats.totalSequencePoints - classMethodStats.uncoveredSequencePoints) / (decimal)(classMethodStats.totalSequencePoints), 1);
                                    coveredMethod.SequenceCoverage = sequenceCoverage;
                                    coveredMethod.Summary.SequenceCoverage = sequenceCoverage;
                                    if (m_OutputCyclomaticComplexity)
                                    {
                                        coveredMethod.CyclomaticComplexity = methodBase.CalculateCyclomaticComplexity();
                                    }
                                    coveredMethods.Add(coveredMethod);
                                }
                            }
                        }

                        if (coveredMethods.Count > 0)
                        {
                            Class coveredClass = new Class();
                            coveredClass.FullName = GenerateTypeName(type);
                            coveredClass.Methods = coveredMethods.ToArray();
                            coveredClasses.Add(coveredClass);
                        }
                    }
                }

                if (coveredClasses.Count != 0)
                {
                    Module module = new Module();
                    module.ModuleName = assembly.GetName().Name;
                    List<ModelFile> coveredFileList = new List<ModelFile>();
                    foreach (KeyValuePair<string, UInt32> fileEntry in fileList)
                    {
                        ModelFile coveredFile = new ModelFile();
                        coveredFile.FullPath = fileEntry.Key;
                        coveredFile.UniqueId = fileEntry.Value;

                        coveredFileList.Add(coveredFile);
                    }
                    module.Files = coveredFileList.ToArray();
                    module.Classes = coveredClasses.ToArray();
                    moduleList.Add(module);
                }
            }

            if (moduleList.Count > 0)
            {
                coverageSession = new CoverageSession();
                coverageSession.Modules = moduleList.ToArray();
            }

            ProcessGenericMethods(coverageSession);

            return coverageSession;
        }

        private void ProcessGenericMethods(CoverageSession coverageSession)
        {
            CoveredMethodStats[] coveredMethodStats = Coverage.GetStatsForAllCoveredMethods();
            foreach (CoveredMethodStats coveredMethodStat in coveredMethodStats)
            {
                MethodBase method = coveredMethodStat.method;

                Type declaringType = method.DeclaringType;
                string assemblyName = declaringType.Assembly.GetName().Name.ToLower();
                if (!ShouldProcessAssembly(assemblyName))
                {
                    continue;
                }

                if (!(declaringType.IsGenericType || method.IsGenericMethod))
                {
                    continue;
                }

                Module module = Array.Find(coverageSession.Modules, element => element.ModuleName.ToLower() == assemblyName);
                if (module != null)
                {
                    string className = string.Empty;
                    if (declaringType.IsGenericType)
                    {
                        Type genericTypeDefinition = declaringType.GetGenericTypeDefinition();
                        className = GenerateTypeName(genericTypeDefinition);
                    }
                    else if (method.IsGenericMethod)
                    {
                        className = GenerateTypeName(declaringType);
                    }

                    Class klass = Array.Find(module.Classes, element => element.FullName == className);
                    if (klass != null)
                    {
                        Method targetMethod = Array.Find(klass.Methods, element => element.MetadataToken == method.MetadataToken);
                        if (targetMethod != null)
                        {
                            CoveredSequencePoint[] coveredSequencePoints = Coverage.GetSequencePointsFor(method);
                            foreach (CoveredSequencePoint coveredSequencePoint in coveredSequencePoints)
                            {
                                SequencePoint targetSequencePoint = Array.Find(targetMethod.SequencePoints, element => (element.StartLine == coveredSequencePoint.line && element.Offset == coveredSequencePoint.ilOffset));
                                if (targetSequencePoint != null)
                                {
                                    targetSequencePoint.VisitCount += (int)coveredSequencePoint.hitCount;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
