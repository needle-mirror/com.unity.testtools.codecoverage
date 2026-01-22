namespace UnityEditor.TestTools.CodeCoverage.Analytics
{
#if !UNITY_6000_0_OR_NEWER
    internal enum EventName
    {
        codeCoverage
    }
#endif
    internal enum ActionID
    {
        Other = 0,
        DataOnly = 1,
        ReportOnly = 2,
        DataReport = 3
    }

    internal enum CoverageModeID
    {
        None = 0,
        TestRunner = 1,
        Recording = 2
    }
}
