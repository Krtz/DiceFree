using System;
using System.Linq;
using DiceFree.World;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;
using static DiceFree.EditorTools.BlockoutShapes;

namespace DiceFree.EditorTools
{
    public static class TrimeryVillageEnhancements
    {
        private const string ScenePath=TrimeryVillageAuthoring.ScenePath;
        private const string Name="Trimery v1 gate watch and real lumber traffic";
        private static T[] All<T>(Scene scene) where T:Component =>
            scene.GetRootGameObjects()
                .SelectMany(g=>g.GetComponentsInChildren<T>(true)).ToArray();
        [CliCommand("dicefree.trimery.fix-anchor",
            "Place Trimery's revive landing anchor on verified walkable ground beside the physical D6.")]
        public static object FixAnchor()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode first");
            var previous=EditorSceneManager.GetSceneManagerSetup();
            try
            {
                var scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
                var anchor=All<ResurrectionAnchor>(scene).Single(x=>
                    x.StableId==TrimeryVillageAuthoring.AnchorId);
                anchor.transform.position=CornbergLandscape.Ground(250f,181.8f);
                EditorUtility.SetDirty(anchor.transform);
                EditorSceneManager.MarkSceneDirty(scene);
                if(!EditorSceneManager.SaveScene(scene))
                    throw new InvalidOperationException("Cannot save Trimery anchor");
                return new{success=true,marker="TRIMERY_ANCHOR_ON_NAVMESH",
                    position=anchor.transform.position.ToString()};
            }
            finally{if(previous.Length>0)
                EditorSceneManager.RestoreSceneManagerSetup(previous);}
        }
        [CliCommand("dicefree.trimery.polish",
            "Place six existing animated guard models, work carts and locally licensed timber architectural details.")]
        public static object Install()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode first");
            for(int i=0;i<SceneManager.sceneCount;i++)
                if(SceneManager.GetSceneAt(i).isDirty)
                    throw new InvalidOperationException("Save pending scene changes.");
            var setup=EditorSceneManager.GetSceneManagerSetup();
            try
            {
                var scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
                var village=All<Transform>(scene).FirstOrDefault(t=>
                    t.name==TrimeryVillageAuthoring.RootName);
                if(village==null)throw new InvalidOperationException("Install Trimery first");
                if(village.Find(Name)!=null)
                    return new{success=true,alreadyInstalled=true};
                var root=Group(Name,village);
                int guards=0;
                var template=All<Transform>(scene).FirstOrDefault(
                    t=>t.name=="NPCs/NPC_Guard - 3D");
                if(template==null)
                    throw new InvalidOperationException("No existing guard model for staffed gates");
                foreach(var pair in new[]{
                    (gate:"South",x:239f,z:160f),
                    (gate:"South",x:258f,z:160f),
                    (gate:"West",x:226f,z:171f),
                    (gate:"West",x:226f,z:190f),
                    (gate:"North",x:239f,z:201f),
                    (gate:"North",x:258f,z:201f)})
                {
                    var npc=Object.Instantiate(template.gameObject,root);
                    npc.name="Trimery "+pair.gate+" gate watch "+guards;
                    npc.transform.position=CornbergLandscape.Ground(pair.x,pair.z);
                    npc.transform.rotation=Quaternion.Euler(0,
                        pair.gate=="South"?180:
                        pair.gate=="West"?270:0,0);
                    foreach(var target in npc.GetComponentsInChildren<InteractionTarget>(true))
                        target.ConfigureName("Trimery "+pair.gate+" Gate Guard");
                    guards++;
                }
                int detail=0;
                var buildings=All<Transform>(scene).FirstOrDefault(
                    t=>t.name=="Buildings - safe inner village" &&
                    t.IsChildOf(village));
                if(buildings!=null)
                foreach(Transform house in buildings)
                {
                    var baseWall=house.Find("Timber structure");
                    if(baseWall==null)continue;
                    var center=baseWall.position;
                    var size=baseWall.localScale;
                    for(int side=-1;side<=1;side+=2)
                    {
                        var trim=Group("Asset-pack timber support "+side,house);
                        trim.position=new Vector3(
                            center.x+side*(size.x*.5f-.30f),
                            center.y-.9f,
                            center.z-size.z*.5f-.16f);
                        var art=trim.gameObject.AddComponent<TrimeryArtSlot>();
                        art.Configure("TimberSupport",null,
                            new Vector3(1.0f,1.35f,1.0f),Vector3.zero);
                        detail++;
                    }
                }
                int carts=0;
                var cart=AssetDatabase.LoadAssetAtPath<GameObject>(
                    "Assets/_DiceFree/Art/World/VisualBatch01/Models/cart_02.fbx");
                if(cart!=null)
                    foreach(var place in new[]{
                        (x:231f,z:185f,yaw:65f),
                        (x:210f,z:181.5f,yaw:95f)})
                    {
                        var prop=(GameObject)PrefabUtility.InstantiatePrefab(cart,scene);
                        prop.name="Trimery loaded timber caravan cart "+carts;
                        prop.transform.SetParent(root,true);
                        prop.transform.position=CornbergLandscape.Ground(place.x,place.z);
                        prop.transform.rotation=Quaternion.Euler(0,place.yaw,0);
                        prop.transform.localScale=Vector3.one*1.4f;
                        foreach(var collider in prop.GetComponentsInChildren<Collider>(true))
                            Object.DestroyImmediate(collider);
                        carts++;
                    }
                EditorSceneManager.MarkSceneDirty(scene);
                if(!EditorSceneManager.SaveScene(scene))
                    throw new InvalidOperationException("Trimery scene save failed");
                AssetDatabase.SaveAssets();
                return new{success=true,marker="TRIMERY_GUARDS_AND_ART_READY",
                    guardModels=guards,timberSupports=detail,caravans=carts};
            }
            finally{if(setup.Length>0)EditorSceneManager.RestoreSceneManagerSetup(setup);}
        }
    }
}
