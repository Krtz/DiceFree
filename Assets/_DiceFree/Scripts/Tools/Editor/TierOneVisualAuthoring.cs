using System;
using System.IO;
using System.Linq;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DiceFree.EditorTools
{
    /// <summary>Tier I layered visual signature, applied to existing humanoid prefabs only.</summary>
    internal static class TierOneVisualAuthoring
    {
        const string Base = "Assets/_DiceFree/Art/Characters";
        const string Physical = Base + "/PhysicallyBlessed/Prefabs/PhysicallyBlessedPresentation.prefab";
        const string Magical = Base + "/MagicallyTouched/Prefabs/MagicallyTouchedPresentation.prefab";
        const string Novice = Base + "/Novice/Prefabs/NovicePresentation.prefab";
        const string Folder = Base + "/TierOneAccents";
        const string Preview = "Assets/_DiceFree/Art/Validation/Previews/TierOneClassIdentities_Unity.png";

        static Material Mat(string name, Color color, float metallic = 0)
        {
            var path = Folder + "/" + name + ".mat";
            var result = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (result == null)
            {
                result = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = name };
                AssetDatabase.CreateAsset(result, path);
            }
            result.SetColor("_BaseColor", color);
            result.SetFloat("_Metallic", metallic);
            result.SetFloat("_Smoothness", metallic > 0 ? .42f : .18f);
            EditorUtility.SetDirty(result);
            return result;
        }

        static GameObject Shape(Transform root, Transform bone, string name, PrimitiveType primitive, Vector3 pos, Vector3 size, Material material)
        {
            var o = GameObject.CreatePrimitive(primitive);
            o.name = "T1_" + name;
            o.transform.position = root.TransformPoint(pos);
            o.transform.rotation = root.rotation;
            o.transform.localScale = Vector3.Scale(root.lossyScale, size);
            o.transform.SetParent(bone, true);
            UnityEngine.Object.DestroyImmediate(o.GetComponent<Collider>());
            o.GetComponent<Renderer>().sharedMaterial = material;
            return o;
        }

        // Sculpted, softly flaring mantle over the hips. Open at the front, with
        // 48 lateral and 16 vertical subdivisions: not a rigid rectangle or low-poly cone.
        static Mesh GetCasterMantle()
        {
            const string path = Folder + "/T1_CasterMantle.asset";
            var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh!=null) return mesh;
            const int slices=48, rows=16;
            var vertices=new Vector3[(slices+1)*(rows+1)];
            var triangles=new int[slices*rows*6];
            int tri=0;
            for(int r=0;r<=rows;r++)
            {
                float t=r/(float)rows;
                for(int i=0;i<=slices;i++)
                {
                    float angle=(-90f + 180f*i/slices)*Mathf.Deg2Rad;
                    float width=Mathf.Lerp(.205f,.285f,t);
                    float depth=Mathf.Lerp(.145f,.202f,t);
                    float ripple=.007f*Mathf.Sin(angle*7+t*14)*t;
                    vertices[r*(slices+1)+i]=new Vector3(
                        Mathf.Sin(angle)*(width+ripple),
                        -.48f*t + .012f*Mathf.Sin(angle*4+t*9)*t,
                        Mathf.Cos(angle)*(depth+ripple) + .022f);
                    if(r==rows||i==slices)continue;
                    int v=r*(slices+1)+i;
                    triangles[tri++]=v;triangles[tri++]=v+slices+1;triangles[tri++]=v+1;
                    triangles[tri++]=v+1;triangles[tri++]=v+slices+1;triangles[tri++]=v+slices+2;
                }
            }
            mesh=new Mesh {name="T1_CasterMantle"};
            mesh.vertices=vertices;mesh.triangles=triangles;
            mesh.RecalculateNormals();mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh,path);
            return mesh;
        }

        static void Accent(string path, bool magic, Material primary, Material trim, Material detail, Material highlight)
        {
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var animator = root.GetComponentInChildren<Animator>(true);
                if (animator == null || !animator.isHuman) throw new InvalidOperationException("Tier I rig not humanoid: " + path);
                foreach (var obj in root.GetComponentsInChildren<Transform>(true).Where(t => t.name.StartsWith("T1_", StringComparison.Ordinal)).ToArray())
                    UnityEngine.Object.DestroyImmediate(obj.gameObject);

                Transform Bone(HumanBodyBones human, string imported)
                {
                    var value = animator.GetBoneTransform(human);
                    return value != null ? value : animator.GetComponentsInChildren<Transform>(true)
                        .FirstOrDefault(t => t.name == imported);
                }
                var chest = Bone(HumanBodyBones.Chest, "Chest");
                var hips = Bone(HumanBodyBones.Hips, "Hips");
                var left = Bone(HumanBodyBones.LeftLowerArm, "LeftForeArm");
                var right = Bone(HumanBodyBones.RightLowerArm, "RightForeArm");
                if (chest == null || hips == null || left == null || right == null)
                    throw new InvalidOperationException("Class signature bone missing: chest=" +(chest!=null)+
                        " hips="+(hips!=null)+" left="+(left!=null)+" right="+(right!=null));

                // Derived Tier 1 prefabs predate the Novice costume upgrade.
                // Copy the current Novice per-submesh materials first; otherwise all skin,
                // hair and accessories may be overridden by the old grey shirt material.
                var novice = AssetDatabase.LoadAssetAtPath<GameObject>(Novice);
                var original = novice.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                foreach (var renderer in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                {
                    var matching = original.FirstOrDefault(r => r.name == renderer.name);
                    if (matching == null) continue;
                    var mats = matching.sharedMaterials.ToArray();
                    if (renderer.name == "Novice_Baseline_TShirt")
                        for (int i = 0; i < mats.Length; i++)
                            if (i == 0 || i == 1) mats[i] = primary;
                    renderer.sharedMaterials = mats;
                }

                var neck = Shape(root.transform, chest, magic ? "AdeptIndigoCollar" : "WarriorShoulderYoke",
                    PrimitiveType.Sphere, new Vector3(0, 1.395f, .018f),
                    magic ? new Vector3(.29f, .085f, .18f) : new Vector3(.285f, .065f, .17f), primary);
                foreach (float side in new[] { -1f, 1f })
                {
                    Shape(root.transform, chest, magic ? "MantleShoulder" : "IronShoulder",
                        PrimitiveType.Sphere, new Vector3(side * .235f, 1.32f, -.005f),
                        magic ? new Vector3(.175f, .075f, .173f) : new Vector3(.168f, .105f, .155f), trim);
                    Shape(root.transform, side < 0 ? left : right,
                        magic ? "CastingCuff" : "WarriorBracer",
                        PrimitiveType.Sphere, new Vector3(side * .410f, 1.115f, -.028f),
                        magic ? new Vector3(.083f, .105f, .084f) : new Vector3(.091f, .115f, .091f), detail);
                    if (!magic)
                        Shape(root.transform, chest, "ShoulderRivet",
                            PrimitiveType.Sphere, new Vector3(side * .27f, 1.372f, -.128f),
                            new Vector3(.022f, .022f, .015f), highlight);
                }

                if (magic)
                {
                    Shape(root.transform, chest, "ArcaneFocus", PrimitiveType.Sphere,
                        new Vector3(0, 1.238f, -.207f), new Vector3(.051f, .071f, .037f), highlight);
                    Shape(root.transform, hips, "Spellbook", PrimitiveType.Cube,
                        new Vector3(-.286f, 1.01f, .003f), new Vector3(.14f, .20f, .059f), primary);
                    Shape(root.transform, hips, "SpellbookClasp", PrimitiveType.Cube,
                        new Vector3(-.286f, 1.01f, -.033f), new Vector3(.059f, .022f, .012f), highlight);
                    var skirt=new GameObject("T1_CasterMantle");
                    skirt.transform.position=root.transform.TransformPoint(new Vector3(0f,1.04f,0f));
                    skirt.transform.rotation=root.transform.rotation;
                    skirt.transform.SetParent(hips,true);
                    skirt.AddComponent<MeshFilter>().sharedMesh=GetCasterMantle();
                    skirt.AddComponent<MeshRenderer>().sharedMaterial=primary;
                }
                else
                {
                    Shape(root.transform, chest, "ChestLeatherPatch", PrimitiveType.Cube,
                        new Vector3(0, 1.228f, -.164f), new Vector3(.25f, .20f, .041f), primary);
                    Shape(root.transform, chest, "BreastplateBadge", PrimitiveType.Sphere,
                        new Vector3(0, 1.228f, -.193f), new Vector3(.065f, .041f, .019f), highlight);
                }

                foreach (var c in root.GetComponentsInChildren<Collider>(true))
                    throw new InvalidOperationException("An art collider appeared: " + c.name);
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        [CliCommand("dicefree.visual.tier1.install", "Give physical and magical first advancements distinct bone-bound silhouettes.")]
        public static object Install()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode first.");
            if (!AssetDatabase.IsValidFolder(Folder))
                AssetDatabase.CreateFolder(Base, "TierOneAccents");
            var physPrimary = Mat("T1_WornOxBlood", new Color(.34f,.115f,.09f));
            var iron = Mat("T1_PolishedSteel", new Color(.40f,.44f,.46f), .35f);
            var leather = Mat("T1_WarriorLeather", new Color(.21f,.13f,.10f));
            var ironEdge = Mat("T1_SteelRivets", new Color(.77f,.68f,.47f), .42f);
            var arcane = Mat("T1_Indigo", new Color(.13f,.12f,.27f));
            var magicTrim = Mat("T1_BlueCloth", new Color(.25f,.25f,.51f));
            var wrists = Mat("T1_WovenSilver", new Color(.50f,.52f,.64f), .12f);
            var crystal = Mat("T1_FocusGem", new Color(.41f,.73f,.92f), .15f);
            Accent(Physical, false, physPrimary, iron, leather, ironEdge);
            Accent(Magical, true, arcane, magicTrim, wrists, crystal);
            AssetDatabase.SaveAssets();
            return new {success=true, physicalSignature="warrior shoulders bracers chest patch", magicalSignature="caster mantle cuffs book focus", extraColliders=0};
        }

        [CliCommand("dicefree.visual.tier1.validate", "Check Tier I prefabs preserve Humanoid rigs and get unique bone-bound class silhouettes.")]
        public static object Validate()
        {
            string[] paths = { Physical, Magical };
            var result = paths.Select(p =>
            {
                var g = AssetDatabase.LoadAssetAtPath<GameObject>(p);
                if (g == null) throw new InvalidOperationException("Missing "+p);
                var animator = g.GetComponentInChildren<Animator>(true);
                var details = g.GetComponentsInChildren<Transform>(true).Count(t=>t.name.StartsWith("T1_",StringComparison.Ordinal));
                if (animator==null || animator.avatar == null || !animator.avatar.isValid || details < 8)
                    throw new InvalidOperationException("Signature or Humanoid invalid: "+p+" count="+details);
                if (g.GetComponentsInChildren<Collider>(true).Length!=0)
                    throw new InvalidOperationException("Art prefab must not contain colliders: "+p);
                return new {path=p, signaturePieces=details};
            }).ToArray();
            return new {success=true, classes=result};
        }

        [CliCommand("dicefree.visual.tier1.capture", "Render a single same-camera lineup of Novice and both tier 1 class prefabs.")]
        public static object Capture()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Edit mode only");
            for (int i=0;i<UnityEngine.SceneManagement.SceneManager.sceneCount;i++)
                if(UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty)throw new InvalidOperationException("Unsaved scene exists");
            var setup = EditorSceneManager.GetSceneManagerSetup();
            Camera camera = null;
            RenderTexture target = null;
            Texture2D bitmap = null;
            RenderTexture previous = RenderTexture.active;
            try
            {
                var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
                var go=new GameObject("Tier I camera");
                camera=go.AddComponent<Camera>();
                camera.orthographic=true;
                camera.orthographicSize=1.58f;
                camera.backgroundColor=new Color(.15f,.19f,.235f);
                camera.clearFlags=CameraClearFlags.SolidColor;
                camera.transform.position=new Vector3(3.7f,2.2f,-9.5f);
                camera.transform.LookAt(new Vector3(0,1,0));
                var sun=new GameObject("Preview lighting").AddComponent<Light>();
                sun.type=LightType.Directional;sun.intensity=1.5f;sun.transform.rotation=Quaternion.Euler(45,-35,0);
                var paths=new[]{Physical, Novice, Magical};
                for (int i=0;i<paths.Length;i++)
                {
                    var model=AssetDatabase.LoadAssetAtPath<GameObject>(paths[i]);
                    var character=(GameObject)PrefabUtility.InstantiatePrefab(model,scene);
                    character.transform.position=new Vector3((i-1)*1.43f,0,0);
                    character.transform.rotation=Quaternion.identity;
                }
                target=new RenderTexture(1440,850,24);
                camera.targetTexture=target;camera.Render();
                RenderTexture.active=target;
                bitmap=new Texture2D(1440,850,TextureFormat.RGB24,false);
                bitmap.ReadPixels(new Rect(0,0,1440,850),0,0);bitmap.Apply();
                File.WriteAllBytes(Path.Combine(Directory.GetParent(Application.dataPath).FullName,Preview),bitmap.EncodeToPNG());
                AssetDatabase.ImportAsset(Preview,ImportAssetOptions.ForceUpdate);
                return new {success=true, file=Preview, order="Physical / Novice / Magical"};
            }
            finally
            {
                if(camera!=null)camera.targetTexture=null;
                RenderTexture.active=previous;
                if(target!=null){target.Release();UnityEngine.Object.DestroyImmediate(target);}
                if(bitmap!=null)UnityEngine.Object.DestroyImmediate(bitmap);
                EditorSceneManager.RestoreSceneManagerSetup(setup);
            }
        }
    }
}
