using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiceFree.World;
using DiceFree.Combat;
using DiceFree.Characters;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;
using static DiceFree.EditorTools.BlockoutShapes;

namespace DiceFree.EditorTools
{
    /// <summary>
    /// First playable Trimery settlement: founded on the existing Ironpine
    /// clearing, with three correctly oriented roads and real collision.
    /// Vendor art stays local; tracked geometry remains portable.
    /// </summary>
    public static class TrimeryVillageAuthoring
    {
        public const string RootName="13 - Trimery lumber village";
        public const string ScenePath="Assets/_DiceFree/Scenes/Cornberg.unity";
        public const string AnchorId="anchor.trimery";
        private const string Art="Assets/_DiceFree/Resources/TrimeryLicensed";
        private static Vector3 Ground(float x,float z)=>
            CornbergLandscape.Ground(x,z);
        private static T[] All<T>(Scene scene) where T:Component=>
            scene.GetRootGameObjects()
                .SelectMany(g=>g.GetComponentsInChildren<T>(true)).ToArray();
        private static void Check(bool condition,string explanation)
        {
            if(!condition)throw new InvalidOperationException("TRIMERY: "+explanation);
        }
        private static bool NearRoad(Vector2 point,Vector2 a,Vector2 b,float clearance)
        {
            var diff=b-a;
            float along=Mathf.Clamp01(Vector2.Dot(point-a,diff)/
                Mathf.Max(.001f,diff.sqrMagnitude));
            return Vector2.Distance(point,a+diff*along)<clearance;
        }
        internal static int ClearFootprint(Scene scene)
        {
            int cleared=0;
            foreach(var t in All<Transform>(scene))
            {
                if(!t.gameObject.activeSelf || !t.gameObject.activeInHierarchy)continue;
                bool tree=t.name=="Woodland tree" ||
                    t.name.StartsWith("Ironpine conifer",StringComparison.Ordinal) ||
                    t.name.IndexOf("Trees/Tree_",StringComparison.Ordinal)>=0;
                if(!tree)continue;
                var p=new Vector2(t.position.x,t.position.z);
                bool inTown=p.x>217f && p.x<282f && p.y>148f && p.y<213f;
                bool road=NearRoad(p,new Vector2(223,149),new Vector2(248,154),7f) ||
                    NearRoad(p,new Vector2(220,180),new Vector2(190,180),7f) ||
                    NearRoad(p,new Vector2(248,207),new Vector2(248,239),7f);
                if(!inTown && !road)continue;
                t.gameObject.SetActive(false);
                cleared++;
            }
            return cleared;
        }
        private static Mesh ExtendedGround()
        {
            const int columns=45,rows=49;
            const float x0=194,z0=131,step=2.4f;
            var vertices=new Vector3[(columns+1)*(rows+1)];
            var triangles=new int[columns*rows*6];
            for(int z=0;z<=rows;z++)
            for(int x=0;x<=columns;x++)
                vertices[z*(columns+1)+x]=Ground(x0+x*step,z0+z*step);
            int write=0;
            for(int z=0;z<rows;z++)
            for(int x=0;x<columns;x++)
            {
                int i=z*(columns+1)+x;
                triangles[write++]=i;
                triangles[write++]=i+columns+1;
                triangles[write++]=i+1;
                triangles[write++]=i+1;
                triangles[write++]=i+columns+1;
                triangles[write++]=i+columns+2;
            }
            var mesh=SaveMesh("Trimery continuous playable terrain",vertices,triangles);
            mesh.SetPreBakeCollisionMesh(false,true);
            EditorUtility.SetDirty(mesh);
            return mesh;
        }
        private static void WallLine(Transform walls,
            Vector2 begin,Vector2 end,bool gate,float gatePosition)
        {
            Vector2 direction=(end-begin).normalized;
            float length=Vector2.Distance(begin,end);
            for(float along=0;along<=length;along+=1.35f)
            {
                var point=begin+direction*along;
                if(gate && Mathf.Abs(along-gatePosition)<5f)continue;
                Vector3 y=Ground(point.x,point.y);
                var post=Shape("Trimery pointed timber palisade",PrimitiveType.Cylinder,
                    y+Vector3.up*1.65f,new Vector3(.72f,1.65f,.72f),
                    Timber,walls);
                post.transform.rotation=Quaternion.Euler(0,along*1.3f,0);
                if(along%5.4f<1.35f)
                    Shape("Iron-banded wall brace",PrimitiveType.Cube,
                        y+Vector3.up*1.25f,new Vector3(.8f,.25f,.83f),
                        Dark,walls,false);
            }
        }
        private static void Tower(Transform parent,string name,float x,float z)
        {
            var group=Group(name,parent);
            float y=Ground(x,z).y;
            for(int side=-1;side<=1;side+=2)
            for(int front=-1;front<=1;front+=2)
                Shape("Guard lookout legs",PrimitiveType.Cylinder,
                    new Vector3(x+side*1.05f,y+1.9f,z+front*1.05f),
                    new Vector3(.46f,1.9f,.46f),Timber,group);
            Box("Solid watch platform",new Vector3(x,y+3.85f,z),
                new Vector3(3.1f,.28f,3.1f),Timber,group);
            for(int i=0;i<4;i++)
            {
                float angle=i*Mathf.PI*.5f;
                Box("Watch cover",new Vector3(x+Mathf.Cos(angle)*1.2f,
                    y+4.5f,z+Mathf.Sin(angle)*1.2f),
                    new Vector3(.55f,1.2f,.55f),Timber,group);
            }
            var roof=Shape("Canopied tower roof",PrimitiveType.Cube,
                new Vector3(x,y+5.1f,z),new Vector3(3.7f,.3f,3.7f),
                Roof,group,false);
            roof.transform.rotation=Quaternion.Euler(0,45,0);
        }
        private static void Gate(Transform root,string title,float x,float z,bool horizontal)
        {
            var group=Group(title,root);
            float shift=5.5f;
            Tower(group,"Guard post A",horizontal?x-shift:x,
                horizontal?z:z-shift);
            Tower(group,"Guard post B",horizontal?x+shift:x,
                horizontal?z:z+shift);
            float y=Ground(x,z).y;
            Box("Gate crossbeam",new Vector3(x,y+5,z),
                horizontal?new Vector3(9.4f,.7f,1.1f):
                    new Vector3(1.1f,.7f,9.4f),Timber,group);
            Label(title,new Vector3(x,y+6.2f,z),group);
        }
        private static void DrawPalisade(Transform root)
        {
            var palisade=Group("Believable log palisade with three guarded openings",root);
            // SOUTH entrance, x=248; WEST timber-cart gate z=180;
            // NORTH quilboar trail gate x=248. East is uninterrupted.
            WallLine(palisade,new Vector2(220,153),new Vector2(276,153),true,28f);
            WallLine(palisade,new Vector2(220,207),new Vector2(276,207),true,28f);
            WallLine(palisade,new Vector2(220,153),new Vector2(220,207),true,27f);
            WallLine(palisade,new Vector2(276,153),new Vector2(276,207),false,0);
            Gate(palisade,"SOUTH - to Ironpine and Cornberg",248,153,true);
            Gate(palisade,"WEST - Big City timber trade",220,180,false);
            Gate(palisade,"NORTH - quilboar frontier",248,207,true);
        }
        private static void House(Transform root,string name,
            float x,float z,float width,float depth,int style)
        {
            var group=Group(name,root);
            float y=Ground(x,z).y;
            Box("Stone plinth",new Vector3(x,y+.35f,z),
                new Vector3(width+.45f,.7f,depth+.45f),Stone,group);
            Box("Timber structure",new Vector3(x,y+2.1f,z),
                new Vector3(width,3.4f,depth),style%2==0?Plaster:Timber,group);
            // Decorative real kit components appear if installed locally.
            float edge=width*.5f-.23f;
            foreach(int side in new[]{-1,1})
            {
                Box("Load-bearing timber corner",new Vector3(x+side*edge,y+2,z-depth*.5f),
                    new Vector3(.48f,3.8f,.50f),Timber,group,false);
                Box("Shuttered dark window",new Vector3(x+side*width*.27f,
                    y+2.55f,z-depth*.5f-.03f),
                    new Vector3(1.1f,1.2f,.13f),Dark,group,false);
            }
            Box("Visible front door",new Vector3(x,y+1.28f,z-depth*.5f-.15f),
                new Vector3(1.5f,2.5f,.15f),Timber,group,false);
            var roof=Box("Shingled roof",new Vector3(x,y+4.35f,z),
                new Vector3(width+1.6f,.62f,depth+1.7f),
                Roof,group,false);
            roof.transform.rotation=Quaternion.Euler(0,0,style%2==0?8f:-8f);
            Label(name,new Vector3(x,y+5.7f,z),group);
        }
        private static void LogStack(Transform parent,string title,
            float x,float z,int count,bool solid)
        {
            var group=Group(title,parent);
            float y=Ground(x,z).y;
            for(int row=0;row<2;row++)
            for(int i=0;i<count;i++)
            {
                var log=Shape("Cut timber log",PrimitiveType.Cylinder,
                    new Vector3(x+i*.8f,y+.43f+row*.72f,z),
                    new Vector3(.41f,1.7f,.41f),Timber,group,solid);
                log.transform.rotation=Quaternion.Euler(0,0,90);
                if(i==0)
                    Shape("Pale exposed timber end",PrimitiveType.Cylinder,
                        new Vector3(x+i*.8f,y+.43f+row*.72f,z-1.7f),
                        new Vector3(.405f,.055f,.405f),Plaster,group,false)
                        .transform.rotation=Quaternion.Euler(90,0,0);
            }
        }
        private static void LicensedDecoration(Transform parent,string name,
            string asset,float x,float z,Vector3 scale)
        {
            var obj=Group(name,parent);
            obj.position=Ground(x,z);
            var fallback=Group("FallbackVisual",obj);
            Box("Wooden display counter",obj.position+Vector3.up*.72f,
                new Vector3(2.2f,1.3f,1.3f),Timber,fallback,false);
            var slot=obj.gameObject.AddComponent<TrimeryArtSlot>();
            slot.Configure(asset,fallback,scale,Vector3.zero);
        }
        private static void Sawmill(Transform parent)
        {
            var mill=Group("Working sawmill and lumber yard",parent);
            float x=231,z=190,y=Ground(x,z).y;
            Box("Sawmill raised wood platform",new Vector3(x,y+.32f,z),
                new Vector3(11,.64f,9),Timber,mill);
            for(int sx=-1;sx<=1;sx+=2)
            for(int sz=-1;sz<=1;sz+=2)
                Shape("Sawmill timber stanchion",PrimitiveType.Cylinder,
                    new Vector3(x+sx*5,y+2.45f,z+sz*3.8f),
                    new Vector3(.43f,2.45f,.43f),Timber,mill);
            Box("Sawmill sheltered roof",new Vector3(x,y+5,z),
                new Vector3(11.8f,.38f,9.8f),Roof,mill,false);
            Box("Saw table",new Vector3(x,y+1.3f,z),
                new Vector3(5.3f,.55f,1.35f),Timber,mill);
            for(int i=0;i<3;i++)
                LogStack(mill,"Lumber stock "+i,
                    228+i*3.1f,195.5f,3,true);
            Label("Trimery lumberworks",new Vector3(x,y+6.1f,z),mill);
        }
        private static void WestRoadDamage(Transform parent)
        {
            var broken=Group("Broken western trade road (no Roadwarden yet)",parent);
            for(int i=0;i<12;i++)
            {
                float x=199.5f+i*.55f;
                float z=176.5f+(i%4)*1.6f;
                float y=Ground(x,z).y;
                var rock=Box("Roadwarden smashed paving - solid",
                    new Vector3(x,y+.5f,z),
                    new Vector3(1.35f,1.1f,1.85f),Stone,broken);
                rock.transform.rotation=Quaternion.Euler(
                    (i%3-1)*12,i*23,(i%5-2)*14);
            }
            for(int n=0;n<4;n++)
                Box("Splintered lumber cart wreckage",
                    Ground(204+n*1.2f,181f)+Vector3.up*.8f,
                    new Vector3(.7f,1.5f,3.6f),Timber,broken);
            Label("THE WESTERN HIGHWAY IS DESTROYED",
                Ground(206,181)+Vector3.up*4.3f,broken);
        }
        private static int CloneServices(Scene scene,Transform root)
        {
            int copied=0;
            foreach(var specification in new[]{
                (source:"NPCs/NPC_Merchant - 3D",name:"Trimery lumber trader",
                    x:257f,z:183f),
                (source:"NPCs/NPC_Banker - 3D",name:"Trimery banker",
                    x:237f,z:177f)})
            {
                var original=All<Transform>(scene)
                    .FirstOrDefault(t=>t.name==specification.source);
                if(original==null)continue;
                var clone=Object.Instantiate(original.gameObject,root);
                clone.name=specification.name;
                clone.transform.position=Ground(specification.x,specification.z);
                clone.transform.rotation=Quaternion.Euler(0,180,0);
                foreach(var interactive in clone.GetComponentsInChildren<InteractionTarget>(true))
                    interactive.ConfigureName(specification.name);
                copied++;
            }
            return copied;
        }
        private static void ReviveStone(Transform parent)
        {
            var stone=Group("Trimery Dice revive stone",parent);
            float x=248,z=179,y=Ground(x,z).y;
            Box("Stone plinth",new Vector3(x,y+.24f,z),
                new Vector3(4.5f,.5f,4.5f),Stone,stone);
            var die=Box("Trimery D6 revive beacon",
                new Vector3(x,y+1.7f,z),new Vector3(2.5f,2.5f,2.5f),
                Glow,stone);
            die.transform.rotation=Quaternion.Euler(25,30,15);
            // Anchor at the accessible floor, not the D6 center.
            var anchor=Group("Trimery resurrection anchor",stone);
            // Land beside the D6's physical footprint on an actual
            // walkable NavMesh polygon; Return Home must not target its body.
            anchor.position=Ground(x+2f,z+2.8f);
            anchor.gameObject.AddComponent<ResurrectionAnchor>().Configure(AnchorId,die.transform,4.5f);
            var trigger=Group("Return of the Runner - village arrival",stone);
            trigger.position=Ground(248,161);
            trigger.gameObject.AddComponent<ReachArea>().Configure(
                CornbergDepartureSetup.DestinationId,10f);
        }
        [CliCommand("dicefree.trimery.install",
            "Author playable Trimery terrain, guarded palisade, three roads, lumberworks, shop and revive stone.")]
        public static object Install()
        {
            Check(!EditorApplication.isPlayingOrWillChangePlaymode,
                "Stop Play Mode before authoring.");
            for(int i=0;i<SceneManager.sceneCount;i++)
                Check(!SceneManager.GetSceneAt(i).isDirty,
                    "Save modified scenes before installing Trimery.");
            var previous=EditorSceneManager.GetSceneManagerSetup();
            try
            {
                var scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
                Check(!scene.GetRootGameObjects().Any(g=>g.name==RootName),
                    "Trimery is already installed. Use validate, not Install.");
                Initialize();
                var world=All<Transform>(scene).First(t=>
                    t.name=="Cornberg - World 1 starter pocket");
                var root=Group(RootName,world);
                var terrain=Group("Trimery navigable meadow and woodland edge",root);
                MeshObject("Continuous solid valley meadow",
                    ExtendedGround(),Grass,terrain,true);
                var roads=Group("SOUTH, WEST and NORTH roads",root);
                CornbergLandscape.Trail("Trimery southern caravan approach",
                    new[]{new Vector2(223,149),new Vector2(248,153),
                        new Vector2(248,180)},6f,roads);
                CornbergLandscape.Trail("Trimery western lumber-cart highway",
                    new[]{new Vector2(248,180),new Vector2(220,180),
                        new Vector2(195,180)},7.5f,roads);
                CornbergLandscape.Trail("Trimery northern quilboar route",
                    new[]{new Vector2(248,180),new Vector2(248,207),
                        new Vector2(248,243)},5f,roads);
                int treesRemoved=ClearFootprint(scene);
                OctoberPlaytestRepairs.RepairOuterNavigationEdges(All<Transform>(scene));
                DrawPalisade(root);
                var buildings=Group("Buildings - safe inner village",root);
                foreach(var house in new[]{
                    (name:"Timber merchant hall",x:260f,z:188f,w:8f,d:7f,s:0),
                    (name:"Trimery bank",x:235f,z:171f,w:7f,d:7f,s:1),
                    (name:"Forester's inn",x:260f,z:171f,w:8f,d:7f,s:0),
                    (name:"Forester's cabin",x:264f,z:197f,w:6f,d:7f,s:1),
                    (name:"Millworkers' bunkhouse",x:235f,z:200f,w:7f,d:6f,s:0),
                    (name:"Lumber guild office",x:228f,z:169f,w:6f,d:6f,s:1),
                    (name:"Carpenter workshop",x:270f,z:180f,w:6f,d:7f,s:0)})
                    House(buildings,house.name,house.x,house.z,
                        house.w,house.d,house.s);
                Sawmill(root);
                var market=Group("Portable pack-first goods and market props",root);
                LicensedDecoration(market,"Goods market stand A",
                    "MarketStand",258,181,Vector3.one*1.35f);
                LicensedDecoration(market,"Timber exchange stand",
                    "MarketStand2",254,195,Vector3.one*1.35f);
                for(int i=0;i<4;i++)
                    LicensedDecoration(market,"Cargo stacked box "+i,
                        "Crate",229+i*3f,185,Vector3.one*.85f);
                LogStack(market,"West-bound timber for stalled convoy",225,187,4,true);
                WestRoadDamage(root);
                ReviveStone(root);
                int services=CloneServices(scene,root);
                EditorSceneManager.MarkSceneDirty(scene);
                Check(EditorSceneManager.SaveScene(scene),"Scene save failed.");
                AssetDatabase.SaveAssets();
                // NavMesh rebuilt separately after installation, so visible
                // scene and tracked source meshes remain even if bake fails.
                return new{success=true,marker="TRIMERY_VILLAGE_INSTALLED",
                    roads=3,gates=3,buildings=7,
                    sawmill=true,anchorId=AnchorId,
                    treesCleared=treesRemoved,services};
            }
            finally{if(previous.Length>0)
                EditorSceneManager.RestoreSceneManagerSetup(previous);}
        }
        [CliCommand("dicefree.trimery.rebake",
            "Expand and rebake the shared World 1 navigation to include Trimery.")]
        public static object RebakeNav()
        {
            Check(!EditorApplication.isPlayingOrWillChangePlaymode,
                "Stop Play Mode before rebaking.");
            var prior=EditorSceneManager.GetSceneManagerSetup();
            try
            {
                var scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
                Check(scene.GetRootGameObjects().Any(g=>g.name==
                    "Cornberg - World 1 starter pocket"),
                    "World 1 terrain is not open.");
                Check(All<Transform>(scene).Any(t=>t.name==RootName),
                    "Trimery must be installed first.");
                CornbergSceneBuilder.Rebake();
                return new{success=true,marker="TRIMERY_NAVMESH_REBAKED"};
            }
            finally{if(prior.Length>0)
                EditorSceneManager.RestoreSceneManagerSetup(prior);}
        }

        [CliCommand("dicefree.trimery.licensed.install",
            "Prepare optional local Asset Store prefabs for portable resource-backed town art.")]
        public static object InstallLicensed()
        {
            if(!Directory.Exists(Art))Directory.CreateDirectory(Art);
            int prepared=0;
            var specs=new[]{
                (name:"MarketStand",src:"Assets/Low-Poly Medieval Market/Prefabs/stand_01.prefab"),
                (name:"MarketStand2",src:"Assets/Low-Poly Medieval Market/Prefabs/stand_02.prefab"),
                (name:"Crate",src:"Assets/Low-Poly Medieval Market/Prefabs/wooden_box.prefab"),
                (name:"TimberSupport",src:"Assets/Medieval Village Building Pack/Prefabs/Wood_Pillar_Thick.prefab")
            };
            foreach(var spec in specs)
            {
                var old=AssetDatabase.LoadAssetAtPath<GameObject>(spec.src);
                if(old==null)continue;
                string dest=Art+"/"+spec.name+".prefab";
                if(AssetDatabase.LoadAssetAtPath<GameObject>(dest)!=null)
                {prepared++;continue;}
                var instance=(GameObject)PrefabUtility.InstantiatePrefab(old);
                try
                {
                    PrefabUtility.SaveAsPrefabAsset(instance,dest);
                    prepared++;
                }
                finally{Object.DestroyImmediate(instance);}
            }
            AssetDatabase.SaveAssets();
            return new{success=true,prepared,total=specs.Length,licensedLocalOnly=true};
        }
    }
}
