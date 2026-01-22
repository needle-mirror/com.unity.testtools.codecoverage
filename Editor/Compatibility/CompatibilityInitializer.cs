using UnityEngine;

namespace UnityEditor.TestTools.CodeCoverage
{
    [InitializeOnLoad]
    internal class CompatibilityInitializer
    {
        static CompatibilityInitializer()
        {
            Debug.LogError("[Code Coverage] This version of the Code Coverage package is not compatible with versions of Unity earlier than 2021.3.");
        }
    }
}
