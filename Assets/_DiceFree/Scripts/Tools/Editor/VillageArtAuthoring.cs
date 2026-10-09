using System;
using System.Collections.Generic;
using System.Linq;
using DiceFree.Items;
using DiceFree.UI;
using DiceFree.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Pipeline.Commands;

namespace DiceFree.EditorTools
{
    internal static class VillageArtAuthoring
    {
        internal const string Root = "Assets/_DiceFree/Art/Equipment/SlimeDungeon";
        internal static readonly string[] Ids = { "item.slime.slime-orb", "item.slime.slime-shield", "item.slime.slimy-farmers-gloves", "item.slime.slimy-tophat" };
        internal static readonly string[] Names = { "SlimeOrb", "SlimeShield", "SlimyFarmersGloves", "SlimyTophat" };
        internal static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
        internal static ItemDefinition[] Definitions() => Ids.Select(id => AssetDatabase.FindAssets("t:ItemDefinition").Select(g => AssetDatabase.LoadAssetAtPath<ItemDefinition>(AssetDatabase.GUIDToAssetPath(g))).Single(d => d.stableId == id)).ToArray();
        internal static IEnumerable<T> All<T>(Scene scene) where T : Component => scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<T>(true));
        internal static void EditMode()
        {
            Require(!EditorApplication.isPlayingOrWillChangePlaymode, "Requires stopped Edit Mode.");
            for (int i = 0; i < SceneManager.sceneCount; i++) Require(!SceneManager.GetSceneAt(i).isDirty, "Save existing scene changes before authoring.");
        }
        [CliCommand("dicefree.village-art.install", "Add handmade dungeon equipment and safe generic NPC life to existing scenes, idempotently.")]
        public static object Install()
        {
            EditMode();
            var definitions = Definitions();
            var snapshot = definitions.Select(JsonUtility.ToJson).ToArray();
            Folder(Root); Folder(Root + "/Meshes"); Folder(Root + "/Materials");
            var prefabs = Enumerable.Range(0, 4).Select(CreateArt).ToArray();
            var setup = EditorSceneManager.GetSceneManagerSetup();
            int heroes = 0, forms = 0, idle = 0, wander = 0;
            try
            {
                foreach (string name in new[] { "Cornberg", "SlimeDungeon", "DungeonRewardRoom", "NoviceMountainTrial" })
                {
                    string path = "Assets/_DiceFree/Scenes/" + name + ".unity";
                    var scene = SceneManager.GetSceneByPath(path);
                    bool opened = !scene.isLoaded;
                    if (opened) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                    bool changed = false;
                    foreach (var presentation in All<EquipmentPresentation>(scene))
                    {
                        var animators = presentation.GetComponentsInChildren<Animator>(true).Where(a => a.isHuman).ToArray();
                        Require(animators.Length > 0, "Equipment hero has no supported humanoid rig: " + presentation.name);
                        heroes++; forms += animators.Length;
                        var inv = presentation.GetComponent<CarriedInventory>();
                        var catalog = inv.Definitions.Concat(definitions).Distinct().ToArray();
                        if (catalog.Length != inv.Definitions.Count) { inv.Configure(catalog); EditorUtility.SetDirty(inv); changed = true; }
                        for (int i = 0; i < definitions.Length; i++)
                        {
                            var existing = presentation.Bindings.SingleOrDefault(b => b.definition == definitions[i]);
                            if (existing != null)
                            {
                                if (i == 0) foreach (var visual in existing.visuals) changed |= AlignOrb(visual);
                                continue;
                            }
                            var visuals = new List<GameObject>(); var baseline = new List<GameObject>();
                            foreach (var animator in animators)
                            {
                                if (i == 2)
                                {
                                    var pair = (GameObject)PrefabUtility.InstantiatePrefab(prefabs[i], scene);
                                    PrefabUtility.UnpackPrefabInstance(pair, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                                    foreach (bool left in new[] { true, false })
                                    {
                                        var glove = pair.transform.Find(left ? "LeftGlove" : "RightGlove").gameObject;
                                        var bone = animator.GetBoneTransform(left ? HumanBodyBones.LeftHand : HumanBodyBones.RightHand);
                                        var forearm = animator.GetBoneTransform(left ? HumanBodyBones.LeftLowerArm : HumanBodyBones.RightLowerArm);
                                        var socket = Socket(bone, left ? "SlimeLeftGloveSocket" : "SlimeRightGloveSocket");
                                        socket.rotation = Quaternion.LookRotation((bone.position - forearm.position).normalized, presentation.transform.forward);
                                        glove.transform.SetParent(socket, false); glove.transform.localPosition = Vector3.zero; glove.SetActive(false); visuals.Add(glove);
                                    }
                                    UnityEngine.Object.DestroyImmediate(pair);
                                }
                                else
                                {
                                    bool head = i == 3;
                                    var bone = animator.GetBoneTransform(head ? HumanBodyBones.Head : HumanBodyBones.LeftHand);
                                    var socket = Socket(bone, head ? "SlimeHeadSocket" : "SlimeOffHandSocket");
                                    // Neutral authoring pose, parented to animated bones; existing weapon sockets untouched.
                                    socket.rotation = presentation.transform.rotation;
                                    if (head) socket.position = bone.position + presentation.transform.up * .30f * animator.transform.lossyScale.y;
                                    var visual = (GameObject)PrefabUtility.InstantiatePrefab(prefabs[i], scene);
                                    visual.transform.SetParent(socket, false);
                                    if (!head) visual.transform.localPosition = i == 0 ? new Vector3(0, .10f, .13f) : new Vector3(0, 0, .10f);
                                    if (i == 0) AlignOrb(visual);
                                    visual.SetActive(false); visuals.Add(visual);
                                    // Only explicit baseline head art, never an entire body renderer or rig.
                                    if (head) baseline.AddRange(animator.GetComponentsInChildren<Transform>(true).Where(t => t.name == "Hat" || t.name == "BaselineHat").Select(t => t.gameObject));
                                }
                            }
                            presentation.Configure(presentation.Bindings.Concat(new[] { new EquipmentPresentation.Binding { definition = definitions[i], slot = definitions[i].slot, visuals = visuals.ToArray(), baselineVisuals = baseline.ToArray() } }).ToArray());
                            EditorUtility.SetDirty(presentation); changed = true;
                        }
                    }
                    if (name == "Cornberg")
                    {
                        foreach (var npc in All<Transform>(scene).Where(t => t.name.StartsWith("NPCs/NPC_", StringComparison.Ordinal) && t.name.EndsWith(" - 3D", StringComparison.Ordinal)).ToArray())
                        {
                            Require(npc.GetComponentInChildren<Animator>(true) == null && npc.GetComponent<DiceFree.Combat.CombatActor>() == null, "Generic NPC selection unexpectedly includes a rig/combat actor.");
                            int seed = StableSeed(npc.name + npc.position.ToString("F3"));
                            if (npc.GetComponent<VillageVisualIdle>() == null)
                            {
                                var children = npc.Cast<Transform>().ToArray();
                                Require(children.Length > 0 && npc.GetComponent<Renderer>() == null, "NPC needs child-only art: " + npc.name);
                                // Unity forbids reparenting imported prefab children. Unpack this scene instance
                                // only; imported FBX and every existing material/interaction remain unchanged.
                                if (PrefabUtility.IsPartOfPrefabInstance(npc.gameObject)) PrefabUtility.UnpackPrefabInstance(npc.gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                                var pivot = new GameObject("VillageIdleVisual").transform; pivot.SetParent(npc, false);
                                foreach (var child in children) child.SetParent(pivot, false);
                                npc.gameObject.AddComponent<VillageVisualIdle>().Configure(pivot, seed); changed = true;
                            }
                            idle++;
                            bool walking = npc.name.Contains("NPC_Villager") || npc.name.Contains("NPC_Farmer") || npc.name.Contains("NPC_Herbalist");
                            if (walking && npc.GetComponent<InteractionTarget>() == null)
                            {
                                if (npc.GetComponent<VillageHomeWander>() == null) { npc.gameObject.AddComponent<VillageHomeWander>().Configure(seed); changed = true; }
                                wander++;
                            }
                        }
                    }
                    if (changed) { EditorSceneManager.MarkSceneDirty(scene); Require(EditorSceneManager.SaveScene(scene), "Scene save failed: " + path); }
                    if (opened) EditorSceneManager.CloseScene(scene, true);
                }
                Require(definitions.Select(JsonUtility.ToJson).SequenceEqual(snapshot), "Authoring changed gameplay definitions.");
                AssetDatabase.SaveAssets();
            }
            finally { EditorSceneManager.RestoreSceneManagerSetup(setup); }
            return new { success = true, marker = "VILLAGE_ART_INSTALLED", heroes, forms, idle, wander };
        }
        static int StableSeed(string text) { unchecked { int result = 17; foreach (char c in text) result = result * 31 + c; return result; } }
        static bool AlignOrb(GameObject visual)
        {
            var socket = visual.transform.parent; var hand = socket.parent;
            var animator = hand.GetComponentInParent<Animator>(true);
            Require(animator != null && animator.isHuman, "Orb socket lost humanoid rig.");
            var forearm = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            var offset = (hand.position - forearm.position).normalized * .30f + animator.transform.forward * .12f + Vector3.up * .06f;
            var position = socket.InverseTransformVector(offset);
            if ((visual.transform.localPosition - position).sqrMagnitude < .0000001f) return false;
            visual.transform.localPosition = position; return true;
        }
        static Transform Socket(Transform bone, string name)
        {
            Require(bone != null, "Required humanoid bone absent.");
            var socket = bone.Find(name); if (socket != null) return socket;
            socket = new GameObject(name).transform; socket.SetParent(bone, false); return socket;
        }
        static void Folder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/'); Folder(parent); AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(path));
        }
        static Material Mat(string name, Color color, float metal = 0, float smooth = .3f, bool gel = false)
        {
            string path = Root + "/Materials/" + name + ".mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path); if (m != null) return m;
            var shader = Shader.Find("Universal Render Pipeline/Lit"); Require(shader != null, "URP Lit shader unavailable.");
            m = new Material(shader) { name = name }; m.SetColor("_BaseColor", color); m.SetFloat("_Metallic", metal); m.SetFloat("_Smoothness", smooth);
            if (gel) { m.SetFloat("_Surface", 1); m.SetFloat("_SrcBlend", 5); m.SetFloat("_DstBlend", 10); m.SetFloat("_ZWrite", 0); m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT"); m.SetOverrideTag("RenderType", "Transparent"); m.renderQueue = 3000; }
            AssetDatabase.CreateAsset(m, path); return m;
        }
        // Authored radial profiles produce closed faceted meshes; no Unity primitive meshes/colliders.
        static Mesh Lathe(string name, Vector2[] profile, int sides = 12)
        {
            string path = Root + "/Meshes/" + name + ".asset";
            var old = AssetDatabase.LoadAssetAtPath<Mesh>(path); if (old != null) return old;
            var vertices = new List<Vector3>(); var triangles = new List<int>();
            for (int j = 0; j < profile.Length; j++) for (int i = 0; i < sides; i++) { float a = i * Mathf.PI * 2 / sides; vertices.Add(new Vector3(Mathf.Cos(a) * profile[j].x, profile[j].y, Mathf.Sin(a) * profile[j].x)); }
            for (int j = 0; j < profile.Length - 1; j++) for (int i = 0; i < sides; i++)
            { int a = j * sides + i, b = j * sides + (i + 1) % sides, c = a + sides, d = b + sides; triangles.AddRange(new[] { a, c, b, b, c, d }); }
            return SaveMesh(name, vertices, triangles);
        }
        static Mesh SaveMesh(string name, List<Vector3> vertices, List<int> triangles)
        {
            // Split faces for the deliberate hand-cut low-poly shading.
            var v = triangles.Select(i => vertices[i]).ToArray();
            var mesh = new Mesh { name = name, vertices = v, triangles = Enumerable.Range(0, v.Length).ToArray() }; mesh.RecalculateNormals(); mesh.RecalculateBounds(); AssetDatabase.CreateAsset(mesh, Root + "/Meshes/" + name + ".asset"); return mesh;
        }
        static Mesh ShieldMesh()
        {
            var old = AssetDatabase.LoadAssetAtPath<Mesh>(Root + "/Meshes/KiteShield.asset"); if (old != null) return old;
            var outline = new[] { new Vector2(-.28f,.32f),new Vector2(0,.40f),new Vector2(.28f,.32f),new Vector2(.30f,.02f),new Vector2(.20f,-.23f),new Vector2(0,-.43f),new Vector2(-.20f,-.23f),new Vector2(-.30f,.02f) };
            var v = new List<Vector3>(); foreach (float z in new[] { -.055f, .055f }) foreach (var p in outline) v.Add(new Vector3(p.x, p.y, z));
            v.Add(new Vector3(0, 0, -.085f)); v.Add(new Vector3(0, 0, .09f)); var t = new List<int>();
            for (int i = 0; i < 8; i++) { int n = (i + 1) % 8; t.AddRange(new[] {16,n,i,17,i+8,n+8,i,n,n+8,i,n+8,i+8}); }
            t.Reverse(); return SaveMesh("KiteShield", v, t);
        }
        static GameObject Part(GameObject root, string name, Mesh mesh, Material mat, Vector3 position, Vector3 scale, Quaternion? rotation = null)
        {
            var p = new GameObject(name); p.transform.SetParent(root.transform, false); p.transform.localPosition = position; p.transform.localScale = scale; p.transform.localRotation = rotation ?? Quaternion.identity;
            p.AddComponent<MeshFilter>().sharedMesh = mesh; p.AddComponent<MeshRenderer>().sharedMaterial = mat; p.layer = 2; return p;
        }
        static Vector2 P(float r, float y) => new Vector2(r, y);
        static GameObject CreateArt(int index)
        {
            string path = Root + "/" + Names[index] + ".prefab";
            var old = AssetDatabase.LoadAssetAtPath<GameObject>(path); if (old != null) return old;
            var gel = Mat("Emerald translucent gel", new Color(.16f,.85f,.24f,.44f), 0, .86f, true);
            var core = Mat("Suspended green core", new Color(.035f,.42f,.065f), 0, .68f);
            var magic = Mat("Azure magic", new Color(.08f,.75f,.88f), .2f, .65f);
            var iron = Mat("Blessed iron rim", new Color(.24f,.30f,.29f), .7f, .38f);
            var wood = Mat("Warm oak", new Color(.38f,.20f,.065f));
            var leather = Mat("Farmer ochre leather", new Color(.53f,.32f,.12f));
            var cloth = Mat("Emerald black felt", new Color(.024f,.045f,.031f));
            var ball = Lathe("FacetedGel", new[]{P(0,-1),P(.58f,-.81f),P(.91f,-.42f),P(1,0),P(.91f,.42f),P(.58f,.81f),P(0,1)},14);
            var root = new GameObject(Names[index]);
            try
            {
                if (index == 0)
                {
                    Part(root,"Translucent gel shell",ball,gel,Vector3.zero,Vector3.one*.23f);
                    Part(root,"Suspended slime core",ball,core,new Vector3(0,-.025f,0),new Vector3(.105f,.13f,.105f));
                    for(int i=0;i<4;i++) Part(root,"Internal azure bubble "+i,ball,magic,new Vector3(Mathf.Cos(i*2.4f)*.12f,.055f+i*.018f,Mathf.Sin(i*2.4f)*.11f),Vector3.one*.023f);
                    var ring=Lathe("ArcaneCradle",new[]{P(0,-.03f),P(.15f,-.03f),P(.22f,0),P(.19f,.045f),P(.14f,.045f),P(0,.01f)},12);
                    Part(root,"Magical azure cradle",ring,magic,new Vector3(0,-.20f,0),Vector3.one);
                    for(int i=0;i<2;i++) Part(root,"Core eye "+i,ball,magic,new Vector3(i==0?-.038f:.038f,.015f,.091f),new Vector3(.018f,.027f,.014f));
                }
                else if (index == 1)
                {
                    var shield=ShieldMesh();
                    Part(root,"Substantial iron shield rim",shield,iron,Vector3.zero,Vector3.one);
                    Part(root,"Oak shield face",shield,wood,new Vector3(0,0,.027f),new Vector3(.88f,.89f,1));
                    Part(root,"Translucent slime coating",shield,gel,new Vector3(0,0,.079f),new Vector3(.77f,.78f,.55f));
                    Part(root,"Slime boss medallion",ball,core,new Vector3(0,.035f,.13f),new Vector3(.13f,.16f,.055f));
                    for(int i=0;i<2;i++) Part(root,"Medallion eye "+i,ball,magic,new Vector3(i==0?-.044f:.044f,.05f,.179f),new Vector3(.024f,.03f,.011f));
                    for(int i=0;i<6;i++) Part(root,"Iron rivet "+i,ball,iron,new Vector3(Mathf.Sin(i*Mathf.PI/3)*.245f,Mathf.Cos(i*Mathf.PI/3)*.29f,.092f),Vector3.one*.026f);
                    Part(root,"Rear leather grip",Lathe("ShieldGrip",new[]{P(0,-.09f),P(.035f,-.09f),P(.035f,.09f),P(0,.09f)},8),leather,new Vector3(0,0,-.12f),Vector3.one);
                }
                else if (index == 2)
                {
                    foreach(bool left in new[]{true,false})
                    {
                        var glove=new GameObject(left?"LeftGlove":"RightGlove"); glove.transform.SetParent(root.transform,false); glove.transform.localPosition=new Vector3(left?-.16f:.16f,0,0);
                        Part(glove,"Oversized leather palm",ball,leather,new Vector3(0,0,.09f),new Vector3(.088f,.046f,.105f));
                        var cuff=Lathe("GloveCuff",new[]{P(0,-.027f),P(.077f,-.027f),P(.085f,.026f),P(.068f,.04f),P(0,.04f)},10);
                        Part(glove,"Practical canvas cuff",cuff,wood,Vector3.zero,new Vector3(1,1,.68f),Quaternion.Euler(90,0,0));
                        Part(glove,"Shiny slime cuff",cuff,gel,new Vector3(0,0,.011f),new Vector3(1.02f,.25f,.70f),Quaternion.Euler(90,0,0));
                        for(int f=0;f<4;f++)
                        {
                            var pos=new Vector3((f-1.5f)*.04f,0,.18f+(f==1||f==2?.014f:0));
                            Part(glove,"Leather finger "+f,ball,leather,pos,new Vector3(.025f,.034f,.053f));
                            Part(glove,"Slimy fingertip "+f,ball,gel,pos+new Vector3(0,0,.032f),new Vector3(.026f,.035f,.023f));
                        }
                        Part(glove,"Practical thumb",ball,leather,new Vector3(left?.087f:-.087f,-.01f,.08f),new Vector3(.037f,.036f,.064f),Quaternion.Euler(0,left?35:-35,0));
                    }
                }
                else
                {
                    Part(root,"Wide solid brim",Lathe("TophatBrim",new[]{P(0,0),P(.33f,0),P(.35f,.025f),P(.31f,.052f),P(0,.052f)},16),cloth,Vector3.zero,new Vector3(1,1,.87f));
                    Part(root,"Tall flared felt crown",Lathe("TophatCrown",new[]{P(0,.025f),P(.19f,.025f),P(.18f,.13f),P(.205f,.36f),P(.23f,.49f),P(.20f,.52f),P(0,.52f)},14),cloth,Vector3.zero,new Vector3(1,1,.88f));
                    Part(root,"Emerald slimy hatband",Lathe("TophatBand",new[]{P(0,.066f),P(.196f,.066f),P(.19f,.13f),P(0,.13f)},14),gel,Vector3.zero,new Vector3(1,1,.9f));
                    for(int i=0;i<5;i++) {float a=i*1.8f;Part(root,"Hanging slime droplet "+i,ball,gel,new Vector3(Mathf.Cos(a)*.197f,.065f-i%2*.023f,Mathf.Sin(a)*.174f),new Vector3(.022f,.036f+i%2*.02f,.022f));}
                    Part(root,"Azure hatband clasp",ball,magic,new Vector3(0,.10f,.179f),new Vector3(.043f,.031f,.012f));
                }
                return PrefabUtility.SaveAsPrefabAsset(root,path);
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }
        [CliCommand("dicefree.village-art.validate", "Validate handmade mesh assets, bone attachments, unique bindings and safe generic NPC selection.")]
        public static object Validate()
        {
            EditMode(); var definitions=Definitions();
            var scene=SceneManager.GetSceneByPath(CornbergEconomyAuthoring.ScenePath); Require(scene.isLoaded,"Open existing Cornberg for validation.");
            int meshParts=0;
            for(int i=0;i<4;i++)
            {
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/"+Names[i]+".prefab"); Require(prefab!=null,"Missing art prefab "+Names[i]);
                Require(prefab.GetComponentsInChildren<Collider>(true).Length==0,"Art has gameplay collider.");
                var filters=prefab.GetComponentsInChildren<MeshFilter>(true); Require(filters.Length>=4,"Distinct multipart art missing.");
                foreach(var f in filters) {Require(AssetDatabase.GetAssetPath(f.sharedMesh).StartsWith(Root+"/Meshes/"),"Placeholder mesh.");Require(f.sharedMesh.vertexCount>12 && f.sharedMesh.bounds.size.sqrMagnitude>0,"Invalid 3D mesh.");}
                foreach(var r in prefab.GetComponentsInChildren<Renderer>(true)) Require(r.sharedMaterial.shader.name=="Universal Render Pipeline/Lit","Not URP art.");
                meshParts+=filters.Length;
            }
            var presentation=All<EquipmentPresentation>(scene).Single();
            foreach(var d in definitions)
            {
                var b=presentation.Bindings.Single(v=>v.definition==d); Require(b.slot==d.slot,"Wrong definition slot.");
                Require(b.visuals.Length==(d.slot==EquipmentSlot.Hands?6:3),"Missing authored class form visuals.");
                foreach(var v in b.visuals) {Require(v!=null&&!v.activeSelf,"Item visual must start unequipped.");Require(v.transform.parent.name.StartsWith("Slime"),"Missing reliable bone socket.");Require(v.GetComponentsInChildren<Collider>(true).Length==0,"Binding has collider.");}
            }
            Require(presentation.Bindings.Any(b=>b.definition.stableId=="item.bronze-dagger")&&presentation.Bindings.Any(b=>b.slot==EquipmentSlot.Legs)&&presentation.Bindings.Count(b=>b.slot==EquipmentSlot.Hands)==2,"Older equipment bindings lost.");
            var idles=All<VillageVisualIdle>(scene).ToArray(); var walkers=All<VillageHomeWander>(scene).ToArray();
            Require(idles.Length==9 && walkers.Length==4,"Generic NPC coverage mismatch.");
            foreach(var idle in idles) Require(idle.Visual.parent==idle.transform&&idle.Visual.name=="VillageIdleVisual"&&idle.GetComponentInChildren<Animator>(true)==null,"Idle contaminates rig/root.");
            foreach(var w in walkers) Require(w.GetComponent<InteractionTarget>()==null && (w.name.Contains("NPC_Villager")||w.name.Contains("NPC_Farmer")||w.name.Contains("NPC_Herbalist")),"Service or named actor wanders.");
            var merchant=All<ItemShop>(scene).Single(); Require(merchant.GetComponent<VillageVisualIdle>()!=null&&merchant.GetComponent<VillageHomeWander>()==null,"Merchant safety failed.");
            return new {success=true,marker="VILLAGE_ART_VALIDATED",prefabs=4,meshParts,bindings=4,forms=3,idle=idles.Length,wander=walkers.Length};
        }
    }
}
