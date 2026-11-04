using DiceFree.World;
using UnityEngine;

namespace DiceFree.UI
{
    public static class GraphicsQualityControls
    {
        static readonly string[] Names={"Low","Medium","High","Ultra"};
        public static void Draw()
        {
            GUILayout.Label("Visual quality");
            int previous=(int)GraphicsQualityRuntime.Preset;
            int next=GUILayout.SelectionGrid(previous,Names,4,GUILayout.MinHeight(34));
            if(next!=previous)GraphicsQualityRuntime.Select((VisualQuality)next);
            GUILayout.Label("High is the default. Ultra increases image resolution and shadow detail. Attack warnings remain visible at every setting.");
            GUILayout.Space(12);
        }
    }
}
