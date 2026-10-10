using System;
using DiceFree.World;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEngine;

namespace DiceFree.EditorTools
{
    internal static class WorldFogVisualAuthoring
    {
        private const string ShaderPath="Assets/_DiceFree/Art/WorldFogOverlay.shader";
        private const string MaterialPath=
            "Assets/_DiceFree/Resources/WorldFogOverlay.mat";

        [CliCommand("dicefree.fog.visuals.prepare",
            "Create serialized URP fog overlay material for builds and verify shader support.")]
        public static object Prepare()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Edit Mode required");
            AssetDatabase.ImportAsset(ShaderPath,ImportAssetOptions.ForceUpdate);
            var shader=AssetDatabase.LoadAssetAtPath<Shader>(ShaderPath);
            if(shader==null || !shader.isSupported)
                throw new InvalidOperationException("Custom fog shader failed URP import");
            var material=AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if(material==null)
            {
                material=new Material(shader) { name="WorldFogOverlay" };
                AssetDatabase.CreateAsset(material,MaterialPath);
            }
            else material.shader=shader;
            material.SetColor("_FogTint",new Color(.035f,.055f,.085f,1f));
            material.renderQueue=3100;
            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();
            return new{success=true,shader=shader.name,shaderSupported=shader.isSupported,
                material=MaterialPath,renderQueue=material.renderQueue};
        }

        [CliCommand("dicefree.fog.visuals.travel-test",
            "Verify that moving the player darkens previously visible explored terrain and reveals the new area.")]
        public static object TravelTest()
        {
            if(!EditorApplication.isPlaying)
                throw new InvalidOperationException("Play Mode required");
            var fog=UnityEngine.Object.FindFirstObjectByType<WorldFogOfWar>();
            var player=UnityEngine.Object.FindFirstObjectByType<
                DiceFree.Characters.TraversalInput>();
            if(fog==null || player==null)
                throw new InvalidOperationException("Fog or player not found");
            var helper=new GameObject("Temporary fog travel test");
            try
            {
                var start=player.transform.position;
                helper.transform.position=start;
                fog.Configure(helper.transform,Camera.main,
                    new Vector2(-70,-60),new Vector2(275,190),24f);
                fog.RefreshVisionNow();
                byte atStart=fog.SmoothedOpacityAt(start);
                helper.transform.position=start+Vector3.right*60f;
                fog.RefreshVisionNow();
                byte oldArea=fog.SmoothedOpacityAt(start);
                byte newArea=fog.SmoothedOpacityAt(helper.transform.position);
                if(atStart>30 || oldArea<120 || oldArea>190 || newArea>30 ||
                   !fog.IsExplored(start) || fog.IsVisible(start))
                    throw new InvalidOperationException(
                        "Fog traversal failed: "+atStart+" -> "+oldArea+
                        ", new "+newArea);
                return new{success=true,initialAlpha=atStart,
                    exploredAfterTravelAlpha=oldArea,newAreaAlpha=newArea,
                    oldAreaRemembered=true};
            }
            finally
            {
                fog.Configure(player.transform,Camera.main,
                    new Vector2(-70,-60),new Vector2(275,190),24f);
                UnityEngine.Object.DestroyImmediate(helper);
            }
        }

        [CliCommand("dicefree.fog.visuals.benchmark",
            "Measure visibility and smooth texture refresh cost in Play Mode.")]
        public static object Benchmark()
        {
            if(!EditorApplication.isPlaying)
                throw new InvalidOperationException("Play Mode required");
            var fog=UnityEngine.Object.FindFirstObjectByType<WorldFogOfWar>();
            if(fog==null || !fog.UsesWorldOverlay)
                throw new InvalidOperationException("Fog overlay not active");
            var stopwatch=System.Diagnostics.Stopwatch.StartNew();
            for(int i=0;i<4;i++) fog.RefreshVisionNow();
            stopwatch.Stop();
            return new{success=true,averageRefreshMs=stopwatch.Elapsed.TotalMilliseconds/4.0,
                texturePixels=fog.FogTextureWidth*fog.FogTextureHeight,
                losOccluders=fog.BlockingObjectsCount};
        }

        [CliCommand("dicefree.fog.visuals.runtime-validate",
            "Validate fog uses a smooth world-space mesh, not the old screen GUI projection.")]
        public static object RuntimeValidate()
        {
            if(!EditorApplication.isPlaying)
                throw new InvalidOperationException("Play Mode required");
            var fog=UnityEngine.Object.FindFirstObjectByType<WorldFogOfWar>();
            if(fog==null || !fog.UsesWorldOverlay)
                throw new InvalidOperationException("World-space fog plane missing");
            if(fog.FogTextureWidth<512 || fog.FogTextureHeight<384)
                throw new InvalidOperationException("Fog still uses low-res screen texture");
            var plane=fog.GetComponentInChildren<MeshRenderer>();
            if(plane==null || plane.sharedMaterial==null ||
               plane.sharedMaterial.shader.name!="DiceFree/WorldFogOverlay")
                throw new InvalidOperationException("World fog shader not applied");
            if(plane.GetComponent<Collider>()!=null || plane.gameObject.layer!=2)
                throw new InvalidOperationException("Fog blocks gameplay raycasts");
            return new{success=true,worldSpace=true,
                width=fog.FogTextureWidth,height=fog.FogTextureHeight,
                shader=plane.sharedMaterial.shader.name,groundOnly=true,
                colliders=0,blockers=fog.BlockingObjectsCount};
        }
    }
}
