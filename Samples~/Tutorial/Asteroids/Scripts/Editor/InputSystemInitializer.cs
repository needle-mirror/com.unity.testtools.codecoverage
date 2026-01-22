using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
internal class InputSystemInitializer
{
    static InputSystemInitializer()
    {
#if ENABLE_LEGACY_INPUT_MANAGER
        Debug.LogWarning("The Code Coverage Tutorial sample uses the new Input System package. Go to Project Settings > Player > Other Settings and set 'Active Input Handling' to 'Input System Package (New)'.");
#endif
    }
}
