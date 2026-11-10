using System;
using System.IO;
using System.Linq;
using DiceFree.Advancement;
using DiceFree.Combat;
using DiceFree.Items;
using DiceFree.Skills;
using DiceFree.UI;
using DiceFree.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

public static class PsionGearAuthoring
{
    private const string Folder = "Assets/_DiceFree/Settings/Psion/Gear/";
    private const string ScenePath = "Assets/_DiceFree/Scenes/DevelopmentRoom.unity";
    private const string BladeName = "Mirrorsteel Mindblade";
    private const string CodexName = "Mirrorleaf Codex";

    public static void Batch()
    {
        Directory.CreateDirectory(Folder);
        AssetDatabase.Refresh();
        var scene = EditorSceneManager.OpenScene(ScenePath);
        var player = Object.FindObjectsByType<AdvancementController>(FindObjectsInactive.Include).Single();
        var rig = player.GetComponent<ClassPresentationSwitcher>().Resolve(PsionSkillProgression.PsionClassId);
        if (rig == null) throw new InvalidOperationException("Author the Psion foundation first.");
        var equipment = player.GetComponent<EquipmentPresentation>();
        var blade = Item(BladeName, "item.psion.mirrorsteel-mindblade", EquipmentSlot.MainHand,
            new EquipmentStats { basicAttackFlatBonus = 5, attributes = new AttributeValues { agility = 2, intelligence = 1 } },
            "A faceted edge that catches light a heartbeat before the strike.");
        var codex = Item(CodexName, "item.psion.mirrorleaf-codex", EquipmentSlot.OffHand,
            new EquipmentStats { magicalDefense = 2, attributes = new AttributeValues { intelligence = 3, spirit = 2 } },
            "Pale leaves hold impressions of thoughts their readers never spoke.");

        var steel = Material("Mirrorsteel", new Color(.62f,.73f,.83f), .78f, .61f);
        var dark = Material("Blackened silver", new Color(.085f,.12f,.20f), .57f, .4f);
        var violet = Material("Charged amethyst", new Color(.53f,.21f,.87f), .37f, .6f, new Color(.25f,.055f,.56f));
        var rim = Material("Brushed electrum", new Color(.73f,.55f,.32f), .64f, .48f);
        var hide = Material("Midnight bookcloth", new Color(.10f,.088f,.19f), .06f, .34f);
        var paper = Material("Silvered vellum", new Color(.84f,.78f,.64f), .04f, .39f);
        var pageEdges = Material("Warm page edges", new Color(.47f,.34f,.24f), .05f, .37f);

        var bladePrefab = CreateBlade(steel, dark, violet, rim, hide);
        var codexPrefab = CreateCodex(hide, rim, paper, pageEdges, violet);
        var mainSocket = Find(rig, "RightHandWeapon");
        var offSocket = Find(rig, "LeftHandOffhand");
        var bladeVisual = Place(bladePrefab, mainSocket, Quaternion.Euler(90,0,0));
        var codexVisual = Place(codexPrefab, offSocket, Quaternion.Euler(-12, 14, -12) * Quaternion.AngleAxis(42, Vector3.up));

        equipment.Configure(equipment.Bindings
            .Where(binding => binding.definition != blade && binding.definition != codex)
            .Concat(new[] {
                Binding(blade, bladeVisual, Find(rig, "T1_ArcaneFocus").gameObject),
                Binding(codex, codexVisual, Find(rig, "T1_Spellbook").gameObject, Find(rig, "T1_SpellbookClasp").gameObject)
            }).ToArray());
        var inventory = new SerializedObject(player.GetComponent<CarriedInventory>());
        var definitions = inventory.FindProperty("definitions");
        foreach (var item in new[] { blade, codex })
        {
            if (Enumerable.Range(0, definitions.arraySize).Any(i => definitions.GetArrayElementAtIndex(i).objectReferenceValue == item))
                continue;
            int index = definitions.arraySize;
            definitions.InsertArrayElementAtIndex(index);
            definitions.GetArrayElementAtIndex(index).objectReferenceValue = item;
        }
        inventory.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(equipment);
        SeparateRevivalStoneFromSpawn();
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Could not save the Psion equipment bindings.");
        AssetDatabase.SaveAssets();
        Debug.Log("DICEFREE_PSION_GEAR_AUTHOR_OK");
    }

    private static void SeparateRevivalStoneFromSpawn()
    {
        var anchor = Object.FindObjectsByType<ResurrectionAnchor>(FindObjectsInactive.Include)
            .Single(value => value.StableId == "anchor.dev-room");
        if (!anchor.TryGetComponent<MeshFilter>(out var source)) return;
        var root = anchor.transform;
        var stone = new GameObject("Revival stone marker") { layer = anchor.gameObject.layer };
        stone.transform.SetParent(root.parent,false);
        stone.transform.SetLocalPositionAndRotation(root.localPosition,root.localRotation);
        stone.transform.localScale = root.localScale;
        foreach (var component in new Component[] { source, anchor.GetComponent<MeshRenderer>(), anchor.GetComponent<BoxCollider>() })
        {
            UnityEditorInternal.ComponentUtility.CopyComponent(component);
            UnityEditorInternal.ComponentUtility.PasteComponentAsNew(stone);
            Object.DestroyImmediate(component);
        }
        root.localPosition += new Vector3(1.35f,-.65f,0);
        root.localScale = Vector3.one;
        stone.transform.SetParent(root,true);
        anchor.Configure(anchor.StableId,stone.transform,anchor.ActivationRadius);
    }

    private static EquipmentPresentation.Binding Binding(ItemDefinition item, GameObject visual, params GameObject[] baseline) =>
        new() { definition = item, slot = item.slot, visuals = new[] { visual }, baselineVisuals = baseline };

    private static Transform Find(Transform rig, string name) =>
        rig.GetComponentsInChildren<Transform>(true).Single(value => value.name == name);

    private static GameObject Place(GameObject prefab, Transform socket, Quaternion rotation)
    {
        var previous = socket.Find(prefab.name);
        if (previous != null) Object.DestroyImmediate(previous.gameObject);
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, socket);
        instance.transform.localRotation = rotation;
        instance.SetActive(false);
        return instance;
    }

    private static GameObject CreateBlade(Material steel, Material dark, Material violet, Material rim, Material wrap)
    {
        var root = new GameObject(BladeName);
        try
        {
            Solid(root.transform, "Bound grip", Vector3.zero, new Vector3(.057f,.14f,.057f), wrap, PrimitiveType.Cylinder);
            for (int i = 0; i < 5; i++)
                Solid(root.transform, "Grip winding", new Vector3(0,-.102f + i*.046f,0),
                    new Vector3(.061f,.006f,.061f), dark, PrimitiveType.Cylinder);
            Solid(root.transform, "Electrum pommel", new Vector3(0,-.154f,0), new Vector3(.076f,.025f,.076f), rim, PrimitiveType.Cylinder);
            Solid(root.transform, "Blade collar", new Vector3(0,.145f,0), new Vector3(.12f,.027f,.095f), dark, PrimitiveType.Cylinder);
            foreach (int side in new[] { -1, 1 })
            {
                var arm = Solid(root.transform, "Swept electrum guard", new Vector3(side*.083f,.157f,0),
                    new Vector3(.13f,.035f,.055f), rim);
                arm.transform.localRotation = Quaternion.Euler(0,0,side*20);
            }
            var section = new[] {
                new Vector3(.050f,.16f,.026f), new Vector3(.098f,.27f,.043f),
                new Vector3(.073f,.57f,.035f), new Vector3(.020f,.76f,.014f), new Vector3(.001f,.82f,.001f)
            };
            var shell = Part(root.transform, "Cut mirrorsteel facets");
            shell.gameObject.AddComponent<MeshFilter>().sharedMesh = Facets(section);
            shell.gameObject.AddComponent<MeshRenderer>().sharedMaterials = new[] { steel, dark };
            foreach (int side in new[] { -1, 1 })
            {
                Line(root.transform, "Amethyst channel", new Vector3(0,.23f,side*.044f),
                    new Vector3(0,.60f,side*.036f), .023f, violet);
                var gem = Solid(root.transform, "Amethyst ricasso jewel", new Vector3(0,.181f,side*.046f),
                    new Vector3(.078f,.094f,.028f), violet);
                gem.transform.localRotation = Quaternion.Euler(0,0,45);
            }
            return PrefabUtility.SaveAsPrefabAsset(root, Folder + BladeName + ".prefab");
        }
        finally { Object.DestroyImmediate(root); }
    }

    private static Mesh Facets(Vector3[] sections)
    {
        string path = Folder + "Mindblade facets.asset";
        var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        bool create = mesh == null;
        mesh ??= new Mesh { name = "Mindblade facets" };
        var vertices = new System.Collections.Generic.List<Vector3>();
        var bright = new System.Collections.Generic.List<int>();
        var shadow = new System.Collections.Generic.List<int>();
        Vector3 Ring(Vector3 s, int i) => i switch
        {
            0 => new Vector3(-s.x,s.y,0), 1 => new Vector3(0,s.y,s.z),
            2 => new Vector3(s.x,s.y,0), _ => new Vector3(0,s.y,-s.z)
        };
        for (int ring = 1; ring < sections.Length; ring++)
            for (int side = 0; side < 4; side++)
            {
                int next = (side + 1) % 4;
                int v = vertices.Count;
                vertices.Add(Ring(sections[ring-1],side));
                vertices.Add(Ring(sections[ring-1],next));
                vertices.Add(Ring(sections[ring],next));
                vertices.Add(Ring(sections[ring],side));
                var faces = side is 0 or 2 ? bright : shadow;
                faces.AddRange(new[] { v,v+1,v+2,v,v+2,v+3 });
            }
        mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.subMeshCount = 2;
        mesh.SetTriangles(bright,0);
        mesh.SetTriangles(shadow,1);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        if (create) AssetDatabase.CreateAsset(mesh,path);
        else EditorUtility.SetDirty(mesh);
        return mesh;
    }

    private static GameObject CreateCodex(Material hide, Material rim, Material paper, Material edge, Material violet)
    {
        var root = new GameObject(CodexName);
        try
        {
            Solid(root.transform, "Bound spine", new Vector3(0,.14f,0),
                new Vector3(.063f,.36f,.08f), hide);
            Solid(root.transform, "Leather palm loop", Vector3.zero, new Vector3(.10f,.043f,.065f), hide);
            foreach (int side in new[] { -1, 1 })
            {
                var leaf = Part(root.transform, side < 0 ? "Left leaf" : "Right leaf");
                leaf.localPosition = new Vector3(0,.15f,0);
                leaf.localRotation = Quaternion.Euler(0,side*-14,0);
                float x = side*.116f;
                Solid(leaf, "Cloth cover", new Vector3(x,0,0), new Vector3(.244f,.384f,.024f), hide);
                Solid(leaf, "Gilded paper block", new Vector3(x,0,.033f), new Vector3(.216f,.347f,.054f), edge);
                Solid(leaf, "Exposed vellum", new Vector3(x,0,.061f), new Vector3(.206f,.333f,.018f), paper);
                for (int line = 0; line < 5; line++)
                {
                    float margin = line is 0 or 4 ? .025f : 0;
                    Solid(leaf, "Silverleaf script", new Vector3(x+side*margin,.095f-line*.044f,.071f),
                        new Vector3(.108f-margin,.008f,.004f), line == 2 ? violet : edge);
                }
                foreach (int end in new[] { -1,1 })
                    Solid(leaf, "Electrum cover corner", new Vector3(side*.219f,end*.169f,.004f),
                        new Vector3(.053f,.056f,.030f), rim);
            }
            var jewel = Solid(root.transform, "Spine seal", new Vector3(0,.142f,.059f),
                new Vector3(.052f,.052f,.030f), violet);
            jewel.transform.localRotation = Quaternion.Euler(0,0,45);
            Line(root.transform, "Amethyst ribbon", new Vector3(.038f,.10f,.071f),
                new Vector3(.049f,-.066f,.081f), .017f, violet);
            return PrefabUtility.SaveAsPrefabAsset(root, Folder + CodexName + ".prefab");
        }
        finally { Object.DestroyImmediate(root); }
    }

    private static Transform Part(Transform parent, string name)
    {
        var transform = new GameObject(name).transform;
        transform.SetParent(parent,false);
        return transform;
    }

    private static GameObject Solid(Transform parent, string name, Vector3 position, Vector3 scale, Material material,
        PrimitiveType primitive = PrimitiveType.Cube)
    {
        var solid = GameObject.CreatePrimitive(primitive);
        Object.DestroyImmediate(solid.GetComponent<Collider>());
        solid.name = name;
        solid.transform.SetParent(parent,false);
        solid.transform.localPosition = position;
        solid.transform.localScale = scale;
        solid.GetComponent<MeshRenderer>().sharedMaterial = material;
        return solid;
    }

    private static void Line(Transform parent, string name, Vector3 from, Vector3 to, float width, Material material)
    {
        var delta = to-from;
        var stroke = Solid(parent,name,(from+to)*.5f,new Vector3(width,delta.magnitude,.009f),material);
        stroke.transform.localRotation = Quaternion.FromToRotation(Vector3.up,delta);
    }

    private static Material Material(string name, Color color, float metal, float smooth, Color emission = default)
    {
        string path = Folder + name + ".mat";
        var value = AssetDatabase.LoadAssetAtPath<Material>(path);
        bool create = value == null;
        value ??= new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = name };
        value.SetColor("_BaseColor",color);
        value.SetFloat("_Metallic",metal);
        value.SetFloat("_Smoothness",smooth);
        if (emission != default)
        {
            value.EnableKeyword("_EMISSION");
            value.SetColor("_EmissionColor",emission);
        }
        if (create) AssetDatabase.CreateAsset(value,path);
        else EditorUtility.SetDirty(value);
        return value;
    }

    private static ItemDefinition Item(string name, string id, EquipmentSlot slot, EquipmentStats stats, string flavor)
    {
        string path = Folder + name + ".asset";
        var value = AssetDatabase.LoadAssetAtPath<ItemDefinition>(path);
        if (value == null)
        {
            value = ScriptableObject.CreateInstance<ItemDefinition>();
            AssetDatabase.CreateAsset(value,path);
        }
        value.name = value.displayName = name;
        value.stableId = id;
        value.sourceId = "dev.psion.training-kit";
        value.slot = slot;
        value.handsRequired = 1;
        value.itemLevel = 1;
        value.allowedClassIds = new[] { PsionSkillProgression.PsionClassId };
        value.stats = stats;
        value.flavor = flavor;
        value.icon = Icon(name,slot == EquipmentSlot.MainHand);
        EditorUtility.SetDirty(value);
        return value;
    }

    private static Sprite Icon(string name, bool blade)
    {
        string path = Folder + name + " icon.asset";
        var existing = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault();
        if (existing != null) return existing;
        const int size = 64;
        var texture = new Texture2D(size,size,TextureFormat.RGBA32,false) { name = name };
        var silver = new Color(.81f,.87f,.95f);
        var purple = new Color(.67f,.38f,.98f);
        var gold = new Color(.86f,.66f,.41f);
        var vellum = new Color(.94f,.83f,.62f);
        var ink = new Color(.37f,.19f,.45f);
        for (int y=0; y<size; y++)
            for (int x=0; x<size; x++)
            {
                var p = new Vector2((x+.5f)*2/size-1,(y+.5f)*2/size-1);
                var background = Color.Lerp(new Color(.023f,.026f,.05f),new Color(.25f,.11f,.36f),Mathf.Clamp01(1-p.magnitude));
                float glow = Mathf.Exp(-6*p.sqrMagnitude);
                var c = Color.Lerp(background,purple,glow*.19f);
                if (blade)
                {
                    float along = (p.x+p.y)*.7071f;
                    float across = (p.x-p.y)*.7071f;
                    float wide = Mathf.Min(.18f,.68f-along);
                    if (along > -.34f && along < .68f && Mathf.Abs(across) < wide)
                        c = Mathf.Abs(across) < .037f ? purple : across < 0 ? silver : Color.Lerp(silver,ink,.51f);
                    if (Mathf.Abs(along+.37f)<.047f && Mathf.Abs(across)<.26f) c=gold;
                    if (along > -.71f && along < -.41f && Mathf.Abs(across) < .071f)
                        c = along < -.63f ? gold : Color.Lerp(ink,silver,.2f+.13f*Mathf.Sin(along*150));
                }
                else
                {
                    float spine = Mathf.Abs(p.x);
                    float leafY = p.y+.19f*spine;
                    if (spine < .72f && Mathf.Abs(leafY)<.52f) c=gold;
                    if (spine < .65f && Mathf.Abs(leafY)<.44f)
                    {
                        c = Color.Lerp(vellum,gold,.33f*spine + .32f*(p.x<0 ? 1 : 0));
                        if (spine<.055f) c=ink;
                        else if (spine<.57f && spine>.12f && Mathf.Abs(leafY)<.31f &&
                            Mathf.Abs(Mathf.Sin(leafY*23f)) < .35f) c=ink;
                    }
                    if (Mathf.Abs(p.x-.12f) < .046f && p.y > -.66f && p.y < -.22f) c=purple;
                }
                texture.SetPixel(x,y,c);
            }
        texture.Apply();
        AssetDatabase.CreateAsset(texture,path);
        var icon = Sprite.Create(texture,new Rect(0,0,size,size),new Vector2(.5f,.5f));
        icon.name = name;
        AssetDatabase.AddObjectToAsset(icon,texture);
        return icon;
    }
}
