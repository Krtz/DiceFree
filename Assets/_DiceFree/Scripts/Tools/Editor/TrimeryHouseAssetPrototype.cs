using System;
using System.Linq;
using DiceFree.World;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;

namespace DiceFree.EditorTools
{
    /// <summary>
    /// Local-only art trial for one existing Trimery building. Its authored
    /// renderers remain the public-checkout fallback and its colliders never move.
    /// </summary>
    public static class TrimeryHouseAssetPrototype
    {
        const string MeshPath="Assets/Medieval Village Building Pack/Meshes/All.obj";
        const string ResourcePath="Assets/_DiceFree/Resources/TrimeryLicensed/TimberMerchantHall.prefab";
        const string ResourceName="TimberMerchantHall";
        const string TargetName="Timber merchant hall";
        static readonly string[] FallbackNames={
            "Stone plinth","Timber structure","Load-bearing timber corner",
            "Shuttered dark window","Visible front door","Shingled roof"};

        [CliCommand("dicefree.trimery.house-prototype.install",
            "Replace only the Timber merchant hall visuals with the local medieval timber kit; preserve its collision and public fallback.")]
        public static object Install()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Stop Play Mode before installing the house art prototype.");
            for(int i=0;i<SceneManager.sceneCount;i++)
                if(SceneManager.GetSceneAt(i).isDirty)
                    throw new InvalidOperationException("Save dirty scenes before installing the house art prototype.");

            var previous=EditorSceneManager.GetSceneManagerSetup();
            try
            {
                BuildLocalPrefab();
                var scene=EditorSceneManager.OpenScene(
                    TrimeryVillageAuthoring.ScenePath,OpenSceneMode.Single);
                var house=Object.FindObjectsByType<Transform>(FindObjectsSortMode.None)
                    .SingleOrDefault(t=>t.name==TargetName &&
                        t.parent!=null && t.parent.name=="Buildings - safe inner village");
                if(house==null)
                    throw new InvalidOperationException("Expected exactly one installed Trimery Timber merchant hall.");
                var fallback=house.GetComponentsInChildren<Renderer>(true)
                    .Where(r=>FallbackNames.Contains(r.name)).ToArray();
                if(fallback.Length<7)
                    throw new InvalidOperationException("Expected the original authored house fallback renderers; found "+fallback.Length+".");
                var colliders=house.GetComponentsInChildren<Collider>(true)
                    .Where(c=>c.enabled && !c.isTrigger).ToArray();
                if(colliders.Length<2)
                    throw new InvalidOperationException("The house collision footprint is incomplete.");

                var blockout=house.Find("Timber structure");
                if(blockout==null)
                    throw new InvalidOperationException("The merchant hall footprint anchor is missing.");
                var ground=CornbergLandscape.Ground(blockout.position.x,blockout.position.z);
                var assetAnchor=house.Find("Asset visual anchor - Timber merchant hall");
                if(assetAnchor==null)
                {
                    var anchorObject=new GameObject("Asset visual anchor - Timber merchant hall");
                    Undo.RegisterCreatedObjectUndo(anchorObject,"Add merchant hall art anchor");
                    assetAnchor=anchorObject.transform;
                    assetAnchor.SetParent(house,false);
                }
                assetAnchor.position=ground;
                var rootSlot=house.GetComponent<TrimeryArtSlot>();
                if(rootSlot!=null)Undo.DestroyObjectImmediate(rootSlot);
                var slot=assetAnchor.GetComponent<TrimeryArtSlot>();
                if(slot==null)slot=Undo.AddComponent<TrimeryArtSlot>(assetAnchor.gameObject);
                slot.ConfigureReplacement(ResourceName,fallback,Vector3.one,Vector3.zero);
                EditorUtility.SetDirty(slot);
                EditorUtility.SetDirty(assetAnchor);
                EditorSceneManager.MarkSceneDirty(scene);
                if(!EditorSceneManager.SaveScene(scene))
                    throw new InvalidOperationException("Could not save the targeted house scene change.");
                AssetDatabase.SaveAssets();
                return new{success=true,marker="TRIMERY_HOUSE_ASSET_PROTOTYPE_INSTALLED",
                    target=TargetName,localOnlyResource=ResourcePath,
                    authoredFallbackRenderers=fallback.Length,
                    gameplayCollidersPreserved=colliders.Length,
                    otherBuildingsChanged=0};
            }
            finally{if(previous.Length>0)EditorSceneManager.RestoreSceneManagerSetup(previous);}
        }

        [CliCommand("dicefree.trimery.house-prototype.validate",
            "Check the one-house optional art adapter, source prefab and unchanged collision footprint.")]
        public static object Validate()
        {
            var previous=EditorSceneManager.GetSceneManagerSetup();
            try
            {
                var scene=EditorSceneManager.OpenScene(
                    TrimeryVillageAuthoring.ScenePath,OpenSceneMode.Single);
                var house=Object.FindObjectsByType<Transform>(FindObjectsSortMode.None)
                    .SingleOrDefault(t=>t.name==TargetName &&
                        t.parent!=null && t.parent.name=="Buildings - safe inner village");
                if(house==null)throw new InvalidOperationException("Trimery merchant hall missing.");
                var slot=house.GetComponentsInChildren<TrimeryArtSlot>(true)
                    .SingleOrDefault(x=>x.Resource==ResourceName);
                if(slot==null || slot.Resource!=ResourceName || slot.FallbackRendererCount<7)
                    throw new InvalidOperationException("Local art slot/fallback renderer wiring is incomplete.");
                var blockout=house.Find("Timber structure");
                var ground=CornbergLandscape.Ground(blockout.position.x,blockout.position.z);
                if(Vector3.Distance(slot.transform.position,ground)>.1f)
                    throw new InvalidOperationException("Asset anchor is not at the merchant hall ground coordinate.");
                var prefab=Resources.Load<GameObject>("TrimeryLicensed/"+ResourceName);
                if(prefab==null)throw new InvalidOperationException("Optional house resource cannot be loaded.");
                var renderers=prefab.GetComponentsInChildren<Renderer>(true);
                if(renderers.Length<30)
                    throw new InvalidOperationException("Modular prototype is missing its walls, roof or framing.");
                var bounds=renderers[0].bounds;
                foreach(var renderer in renderers)bounds.Encapsulate(renderer.bounds);
                if(bounds.min.y<0 || bounds.size.x>9 || bounds.size.z>9 || bounds.max.y>5.5f)
                    throw new InvalidOperationException("The art shell does not fit the existing house scale: "+bounds);
                var colliders=house.GetComponentsInChildren<Collider>(true)
                    .Where(c=>c.enabled && !c.isTrigger).ToArray();
                if(colliders.Length<2 || colliders.Any(c=>!c.transform.IsChildOf(house)))
                    throw new InvalidOperationException("House footprint collision is absent or reparented.");
                return new{success=true,marker="TRIMERY_HOUSE_ASSET_PROTOTYPE_VALID",
                    source="locally installed Medieval Village Building Pack",
                    visualParts=prefab.GetComponentsInChildren<Renderer>(true).Length,
                    localBounds=bounds.ToString(),
                    fallbackRenderers=slot.FallbackRendererCount,
                    collidersPreserved=colliders.Length,otherBuildingsChanged=0};
            }
            finally{if(previous.Length>0)EditorSceneManager.RestoreSceneManagerSetup(previous);}
        }

        static void BuildLocalPrefab()
        {
            var imported=AssetDatabase.LoadAssetAtPath<GameObject>(MeshPath);
            if(imported==null)
                throw new InvalidOperationException("Locally installed medieval building mesh library is missing.");
            var parts=imported.GetComponentsInChildren<Transform>(true)
                .Where(t=>t.GetComponent<MeshFilter>()!=null && t.GetComponent<Renderer>()!=null)
                .ToDictionary(t=>t.name,StringComparer.Ordinal);
            var timber=AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/Medieval Village Building Pack/Materials/WoodPieces.mat");
            if(timber==null)
                throw new InvalidOperationException("The building kit's credited timber material is missing.");
            GameObject root=null;
            try
            {
                root=new GameObject(ResourceName);
                root.transform.position=Vector3.zero;

                void Piece(string mesh,Vector3 position,Vector3 scale,Quaternion rotation)
                {
                    if(!parts.TryGetValue(mesh,out var source))
                        throw new InvalidOperationException("Building kit mesh missing: "+mesh);
                    var piece=Object.Instantiate(source.gameObject,root.transform,false);
                    piece.name=mesh;
                    piece.transform.localPosition=position;
                    piece.transform.localRotation=rotation;
                    piece.transform.localScale=scale;
                    piece.GetComponent<Renderer>().sharedMaterial=timber;
                }

                // A 8 x 7 m hall, matching the blockout footprint. Walls keep a
                // two-metre entrance; the player-facing door is asset geometry.
                const float wallY=.25f;
                Piece("Wall_Wood_2x3",new Vector3(-2.5f,wallY,-3.5f),
                    new Vector3(1,1,1.5f),Quaternion.Euler(0,90,0));
                Piece("Wall_Wood_2x3",new Vector3(2.5f,wallY,-3.5f),
                    new Vector3(1,1,1.5f),Quaternion.Euler(0,90,0));
                for(int i=0;i<4;i++)
                    Piece("Wall_Wood_2x3",new Vector3(-3+2*i,wallY,3.5f),
                        Vector3.one,Quaternion.Euler(0,90,0));
                foreach(float x in new[]{-2.5f,2.5f})
                    foreach(float z in new[]{-1.75f,1.75f})
                        Piece("Wall_Wood_2x3",new Vector3(x,wallY,z),
                            new Vector3(1,1,1.75f),Quaternion.identity);

                Piece("Door_Frame_Wood",new Vector3(0,wallY,-3.5f),
                    new Vector3(1,1,1),Quaternion.Euler(0,90,0));
                // The vendor door pivot sits ~0.9 m above its model base.
                Piece("Door_5",new Vector3(.5f,wallY+.91f,-3.5f),
                    new Vector3(1,1,1),Quaternion.Euler(0,90,0));
                foreach(float x in new[]{-2.5f,2.5f})
                {
                    Piece("Wood_Pole_Tall",new Vector3(x,1.78f,-3.4f),
                        Vector3.one,Quaternion.identity);
                    Piece("Wood_Pole_Tall",new Vector3(x,1.78f,3.4f),
                        Vector3.one,Quaternion.identity);
                }
                foreach(float x in new[]{-2.5f,2.5f})
                    Piece("Wall_Window_Wood_2x2_3",
                        new Vector3(x,.25f,-1.75f),new Vector3(1,1.5f,1.75f),
                        Quaternion.identity);

                // Medium thatch modules form two pitched eaves in 2 x 1 m
                // panels; seams overlap slightly so no blockout slab is needed.
                for(int side=-1;side<=1;side+=2)
                for(int row=0;row<4;row++)
                for(int across=0;across<4;across++)
                {
                    float z=side*row;
                    float y=4.62f-row*.40f;
                    Piece("Roof_Thatch_Med_2x1",
                        new Vector3(-3f+across*2f,y,z),Vector3.one,
                        Quaternion.Euler(0,side<0?270:90,0));
                }

                System.IO.Directory.CreateDirectory(
                    "Assets/_DiceFree/Resources/TrimeryLicensed");
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                PrefabUtility.SaveAsPrefabAsset(root,ResourcePath);
                AssetDatabase.ImportAsset(ResourcePath,ImportAssetOptions.ForceSynchronousImport);
            }
            finally{if(root!=null)Object.DestroyImmediate(root);}
        }
    }
}
