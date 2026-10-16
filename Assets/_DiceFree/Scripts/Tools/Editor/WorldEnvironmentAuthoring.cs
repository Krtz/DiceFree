using System;
using System.IO;
using System.Linq;
using DiceFree.Core;
using DiceFree.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Unity.Pipeline.Commands;
using Object = UnityEngine.Object;
namespace DiceFree.EditorTools
{
    public static class WorldEnvironmentAuthoring
    {
        const string Source = "Assets/_DiceFree/LocalArt/TreesLicensed/Source";
        const string Output = "Assets/_DiceFree/Resources/TreesLicensed";
        public static void BuildTrees()
        {
            Directory.CreateDirectory(Output); AssetDatabase.Refresh();
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(Source + "/Model/M_Oak01.fbx");
            if (model == null) throw new InvalidOperationException("Verified local oak source required for licensed prefab authoring");
            string normalPath = Source + "/Texture/Oak/T_Oak01_N.png";
            var importer = (TextureImporter)AssetImporter.GetAtPath(normalPath);
            if (importer.textureType != TextureImporterType.NormalMap) { importer.textureType=TextureImporterType.NormalMap; importer.SaveAndReimport(); }
            for (int i=1;i<=4;i++)
            {
                string path = Output + "/Oak" + i + ".mat";
                var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat == null) { mat = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(mat,path); }
                mat.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Source + "/Texture/Oak/T_Oak0" + i + "_D.png"));
                mat.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath));
                mat.SetTexture("_OcclusionMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Source + "/Texture/Oak/T_Oak01_O.png"));
                mat.SetColor("_BaseColor",Color.white); mat.SetFloat("_BumpScale",.65f);
                mat.SetFloat("_Smoothness",.08f); mat.SetFloat("_AlphaClip",1); mat.SetFloat("_Cutoff",.45f);
                mat.SetFloat("_Cull",0); mat.EnableKeyword("_ALPHATEST_ON"); mat.EnableKeyword("_NORMALMAP");
                mat.EnableKeyword("_OCCLUSIONMAP"); mat.SetOverrideTag("RenderType","TransparentCutout");
                mat.renderQueue=(int)RenderQueue.AlphaTest; mat.enableInstancing=true; EditorUtility.SetDirty(mat);
                var root=new GameObject("Oak"+i);
                try {
                    var visual=Object.Instantiate(model,root.transform,false);
                    var renderers=visual.GetComponentsInChildren<Renderer>();
                    var bounds=renderers[0].bounds;
                    foreach(var r in renderers) bounds.Encapsulate(r.bounds);
                    float scale=1/Mathf.Max(.01f,bounds.size.y);
                    visual.transform.localScale=Vector3.one*scale;
                    visual.transform.localPosition=new Vector3(-bounds.center.x,-bounds.min.y,-bounds.center.z)*scale;
                    var lods=new LOD[3];
                    for(int j=0;j<3;j++){
                        var renderer=renderers.Single(r=>r.name.EndsWith("LOD"+j));
                        renderer.name="Canopy oak LOD"+j;
                        renderer.sharedMaterial=mat; renderer.shadowCastingMode=ShadowCastingMode.TwoSided; renderer.receiveShadows=true;
                        lods[j]=new LOD(j==0?.30f:j==1?.12f:.025f,new[]{renderer});
                    }
                    var group=root.AddComponent<LODGroup>();group.SetLODs(lods);group.RecalculateBounds();
                    PrefabUtility.SaveAsPrefabAsset(root,Output+"/Oak"+i+".prefab");
                } finally {Object.DestroyImmediate(root);}
            }
            AssetDatabase.SaveAssets();
        }
        public static int InstallScene(Scene scene)
        {
            var all=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).ToArray();
            var targets=all.Where(t=>t.gameObject.activeInHierarchy &&
                ((t.name.Contains("Tree_")&&t.name.EndsWith(" - 3D") &&
                  !t.name.Contains("Spruce")&&!t.name.Contains("Pine")&&!t.name.Contains("Cypress")) ||
                 t.name=="Woodland tree")).ToArray();
            int index=0;
            foreach(var tree in targets)
            {
                var existing=tree.GetComponent<DetailedTreeVisual>();
                var renderers=existing!=null ? existing.Originals : tree.GetComponentsInChildren<Renderer>(true);
                if(renderers==null||renderers.Length==0)continue;
                var bounds=renderers[0].bounds;
                foreach(var r in renderers)bounds.Encapsulate(r.bounds);
                var bottom=new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);
                // Briarback primitives use world coordinates beneath an origin-based group.
                if(tree.name!="Woodland tree") bottom=new Vector3(tree.position.x,bounds.min.y,tree.position.z);
                if(tree.name=="Woodland tree" && tree.position.sqrMagnitude<.01f){
                    var children=tree.Cast<Transform>().ToArray();
                    foreach(var child in children)child.SetParent(null,true);
                    tree.position=bottom;
                    foreach(var child in children)child.SetParent(tree,true);
                }
                var adapter=existing??tree.gameObject.AddComponent<DetailedTreeVisual>();
                int palette=Mathf.Abs(Mathf.RoundToInt(bottom.x*17+bottom.z*31))%4;
                // Keep the first two summer palettes common; autumn colors are occasional accents.
                if(palette>1 && index%7!=0)palette=index%2;
                adapter.Configure(palette,renderers,tree.InverseTransformPoint(bottom),
                    Mathf.Clamp(bounds.size.y,4,14)/Mathf.Max(.001f,Mathf.Abs(tree.lossyScale.y)));
                if(tree.GetComponent<TreeCameraOccluder>()==null)tree.gameObject.AddComponent<TreeCameraOccluder>();
                EditorUtility.SetDirty(adapter);index++;
            }
            var sun=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Light>(true))
                .FirstOrDefault(l=>l.type==LightType.Directional&&l.enabled);
            if(sun!=null){
                var cycle=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<WorldDayNightCycle>(true)).FirstOrDefault();
                if(cycle==null){cycle=sun.gameObject.AddComponent<WorldDayNightCycle>();}
                sun.shadows=LightShadows.Soft;sun.shadowStrength=.85f;sun.shadowBias=.04f;sun.shadowNormalBias=.25f;
                cycle.Configure(sun);EditorUtility.SetDirty(cycle);EditorUtility.SetDirty(sun);
            }
            // The original distant city tree occupied the newly playable stockade.
            // Move only its visual tree parts; keep distant terrain dressing in place.
            var trunk=all.FirstOrDefault(t=>t.name=="Ancient trunk");
            if(trunk!=null){
                float shift=560-trunk.position.z;
                foreach(var part in all.Where(t=>t.name=="Ancient trunk"||t.name=="Ancient bough"||t.name=="Vast crown"))
                    part.position+=Vector3.forward*shift;
            }
            EditorSceneManager.MarkSceneDirty(scene);
            return index;
        }
        [CliCommand("dicefree.environment.install","Build locally licensed textured trees and install the outdoor day/night cycle without changing collision.")]
        public static object Install()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Edit Mode only");
            var setup=EditorSceneManager.GetSceneManagerSetup();int trees=0,scenes=0;
            try{
                BuildTrees();
                foreach(string path in EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path))
                {
                    if(!path.EndsWith("/Cornberg.unity")&&!path.EndsWith("/DevelopmentRoom.unity")&&!path.EndsWith("/World2TransitionTest.unity"))continue;
                    var scene=EditorSceneManager.OpenScene(path);trees+=InstallScene(scene);EditorSceneManager.SaveScene(scene);scenes++;
                }
                foreach(var pipeline in AssetDatabase.FindAssets("t:UniversalRenderPipelineAsset").Select(g=>AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(AssetDatabase.GUIDToAssetPath(g)))){
                    if(!AssetDatabase.GetAssetPath(pipeline).StartsWith("Assets/_DiceFree/"))continue;
                    pipeline.shadowDistance=80;EditorUtility.SetDirty(pipeline);
                }
                AssetDatabase.SaveAssets();
                return new{marker="DICEFREE_ENVIRONMENT_INSTALLED",trees,scenes,cycleMinutes=48,shadowDistance=80};
            }finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
        }
    }
}
