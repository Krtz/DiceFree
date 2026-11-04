using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Rendering;

namespace DiceFree.EditorTools
{
    // Editor shader imports can cache the session URP clone as build data. Its lifetime
    // must end before a subsequent build enumerates assets after Play Mode destroys it.
    [InitializeOnLoad]
    public sealed class GraphicsQualityEditorCleanup : IPreprocessBuildWithReport
    {
        static GraphicsQualityEditorCleanup()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if(state==PlayModeStateChange.EnteredEditMode)CoreBuildData.instance.Dispose();
            };
        }
        // URP's asset enumeration runs at int.MinValue + 100 in the installed package.
        public int callbackOrder => int.MinValue + 50;
        public void OnPreprocessBuild(BuildReport report) => CoreBuildData.instance.Dispose();
    }
}
