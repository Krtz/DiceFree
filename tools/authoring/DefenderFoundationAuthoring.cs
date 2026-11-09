using System;
using System.IO;
using System.Linq;
using DiceFree.Advancement;
using DiceFree.Combat;
using DiceFree.Persistence;
using DiceFree.Progression;
using DiceFree.Skills;
using DiceFree.UI;
using DiceFree.Visuals;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

public static class DefenderFoundationAuthoring
{
    private const string Folder = "Assets/_DiceFree/Settings/Defender/";
    private const string DevelopmentCatalog = "Assets/_DiceFree/Settings/Development/Priest/Development Priest catalog.asset";
    private const string ParentId = "class.physically-blessed-novice";
    private const string ClassId = DefenderSkillProgression.DefenderClassId;
    private const string TrialId = "quest.trimery.defender-gate-trial";

    public static void Batch()
    {
        Directory.CreateDirectory(Folder);
        AssetDatabase.Refresh();
        var scene = EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/DevelopmentRoom.unity");
        var controller = Object.FindObjectsByType<AdvancementController>(FindObjectsInactive.Include).Single();
        var player = controller.GetComponent<CombatActor>();
        var classes = AssetDatabase.LoadAssetAtPath<ClassCatalog>(DevelopmentCatalog);
        var parent = classes.Resolve(ParentId);
        var defender = Asset<ActorDefinition>("Defender class");
        defender.stableId = ClassId;
        defender.displayName = "Defender";
        defender.baseAttributes = new AttributeValues { vitality = 23, strength = 17, agility = 7, intelligence = 6, spirit = 8 };
        defender.growth = new AttributeValues { vitality = 3.2f, strength = 2.15f, agility = .7f, intelligence = .58f, spirit = .85f };
        defender.baseHp = 10;
        defender.physicalDefense = 9;
        defender.magicalDefense = 6;
        defender.moveSpeed = parent.moveSpeed;
        defender.tuning = parent.tuning;
        var basic = Attack("Defender sword", "attack.defender.sword", 6, 1.04f);
        basic.reach = .9f;
        basic.interval = 1.19f;
        basic.windup = .32f;
        defender.basicAttack = basic;
        var bash = Attack("Defender Shield Bash", "skill.defender.shield-bash", 14, 1.15f);
        var sweep = Attack("Defender Shield Sweep", "skill.defender.shield-sweep", 10, .85f);

        var resolve = Asset<ResourceDefinition>("Resolve");
        resolve.stableId = DefenderSkillCaster.ResolveId;
        resolve.displayName = "Resolve";
        var profile = Asset<ClassResourceProfile>("Defender Resolve");
        profile.stableId = "resource-profile.defender.resolve";
        profile.classId = ClassId;
        profile.resource = resolve;
        profile.baseMaximum = 100;
        profile.baseRegenPerSecond = 0;
        profile.initialFraction = 0;
        profile.loadPolicy = ResourceRetentionPolicy.ResetOnLoad;

        // A reserved gate-trial ID lets the development selector use the same
        // bypass it already uses for quest-gated classes.
        var edge = Asset<AdvancementDefinition>("Trimery Gate advancement");
        edge.stableId = "advance.physically-blessed.defender";
        edge.displayName = "Defender";
        edge.sourceClass = parent;
        edge.targetClass = defender;
        edge.requiredLevel = 30;
        edge.requiredQuestId = TrialId;
        foreach (var asset in new Object[] { defender, resolve, profile, edge }) EditorUtility.SetDirty(asset);

        classes.Configure(classes.Classes.Where(value => value.stableId != ClassId).Append(defender).ToArray());
        EditorUtility.SetDirty(classes);
        controller.Configure(classes, controller.Definitions.Where(value => value != edge).Append(edge).ToArray());
        player.GetComponent<ManifestationPersistence>().ConfigureClassCatalog(classes);
        var resources = player.GetComponent<ActorResourceController>();
        resources.Configure(resources.Profiles.Where(value => value != profile).Append(profile).ToArray());

        var caster = player.GetComponent<DefenderSkillCaster>() ?? player.gameObject.AddComponent<DefenderSkillCaster>();
        caster.Configure(bash, sweep);
        var provider = player.GetComponent<DefenderAbilityProvider>() ?? player.gameObject.AddComponent<DefenderAbilityProvider>();
        provider.Configure(Enumerable.Range(0, DefenderSkillProgression.SkillCount).Select(Icon).ToArray());
        BindVisual(player);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Could not save the Defender test setup.");
        AssetDatabase.SaveAssets();
        Debug.Log("DICEFREE_DEFENDER_FOUNDATION_AUTHOR_OK");
    }

    private static AttackDefinition Attack(string name, string id, float damage, float strength)
    {
        var attack = Asset<AttackDefinition>(name);
        attack.stableId = id;
        attack.displayName = name;
        attack.channel = DamageChannel.Physical;
        attack.requiresAccuracy = true;
        attack.scaling = AttributeScaling.Weighted;
        attack.weights = new AttributeValues { strength = strength };
        attack.baseDamage = damage;
        attack.coefficient = 1;
        EditorUtility.SetDirty(attack);
        return attack;
    }

    private static void BindVisual(CombatActor player)
    {
        var switcher = player.GetComponent<ClassPresentationSwitcher>();
        var source = switcher.Resolve(ParentId);
        var visual = Object.Instantiate(source.gameObject);
        visual.name = "Trimery Defender";
        visual.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        // This class uses the portable rig. The optional Physical replacement
        // would hide the Defender's hand-mounted pieces when it swaps the body.
        foreach (var replacement in visual.GetComponentsInChildren<PhysicalBlessedVisual>(true))
            Object.DestroyImmediate(replacement);
        var transforms = visual.GetComponentsInChildren<Transform>(true);
        var sword = transforms.First(socket => socket.name == "RightHandWeapon");
        var shield = transforms.First(socket => socket.name == "LeftHandOffhand");
        var darkSteel = Material("Blued steel", new Color(.29f, .38f, .44f), .7f, .46f);
        var brass = Material("Gate brass", new Color(.71f, .52f, .25f), .62f, .42f);
        var oak = Material("Trimery blue oak", new Color(.085f, .22f, .28f), .08f, .32f);
        var steel = Material("Honed sword steel", new Color(.67f, .75f, .80f), .82f, .52f);
        var leather = Material("Brown grip leather", new Color(.18f, .10f, .072f), 0, .31f);
        foreach (var renderer in visual.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer.name.StartsWith("T1_Iron", StringComparison.Ordinal)) renderer.sharedMaterial = darkSteel;
            if (renderer.name.StartsWith("T1_Chest", StringComparison.Ordinal)) renderer.sharedMaterial = oak;
        }

        var weapon = Part(sword, "Trimery gate sword", Vector3.zero);
        weapon.localRotation = Quaternion.Euler(90, 0, 0);
        Solid(weapon, "Leather grip", Vector3.zero, new Vector3(.052f,.17f,.045f), leather);
        Solid(weapon, "Brass quillons", new Vector3(0,.10f,0), new Vector3(.24f,.034f,.062f), brass);
        Solid(weapon, "Counterweight", new Vector3(0,-.11f,0), new Vector3(.073f,.049f,.062f), brass);
        var bladeOutline = new[] {new Vector2(-.063f,.12f),new Vector2(.063f,.12f),new Vector2(.043f,.67f),
            new Vector2(0,.78f),new Vector2(-.043f,.67f)};
        Prism(weapon, "Straight sword blade", bladeOutline, .018f, steel, Vector3.zero);
        Solid(weapon, "Dark fuller", new Vector3(0,.376f,.011f), new Vector3(.026f,.46f,.007f), darkSteel);

        var guard = Part(shield, "Trimery gate shield", new Vector3(0,.03f,.071f));
        guard.rotation = visual.transform.rotation * Quaternion.Euler(0, -16, -8);
        var outline = new[] { new Vector2(-.32f,.35f), new Vector2(.32f,.35f), new Vector2(.35f,.14f),
            new Vector2(.29f,-.22f), new Vector2(0,-.49f), new Vector2(-.29f,-.22f), new Vector2(-.35f,.14f) };
        Prism(guard, "Hammered brass rim", outline, .079f, brass, Vector3.zero);
        Prism(guard, "Blued oak field", outline.Select(p => p * .87f).ToArray(), .049f, oak, Vector3.forward * .035f);
        Solid(guard, "Gate crest lintel", new Vector3(0,.115f,.070f), new Vector3(.34f,.071f,.033f), brass);
        foreach (float x in new[] { -.123f, 0, .123f })
            Solid(guard, "Gate crest upright", new Vector3(x,-.052f,.070f), new Vector3(.061f,.25f,.034f), brass);
        foreach (var point in outline)
            Solid(guard, "Rim rivet", new Vector3(point.x*.91f,point.y*.91f,.048f), Vector3.one*.036f, darkSteel, PrimitiveType.Sphere);

        var animator = ClassPresentationSwitcher.FindVisualAnimator(visual.transform);
        if (animator == null) throw new InvalidOperationException("The physical training rig has no Animator.");
        var prefab = PrefabUtility.SaveAsPrefabAsset(visual, Folder + "Trimery Defender presentation.prefab");
        Object.DestroyImmediate(visual);
        var property = new SerializedObject(switcher).FindProperty("bindings");
        int index = Enumerable.Range(0,property.arraySize).Where(i =>
            property.GetArrayElementAtIndex(i).FindPropertyRelative("classId").stringValue == ClassId).DefaultIfEmpty(-1).First();
        if (index < 0)
        {
            index = property.arraySize;
            property.InsertArrayElementAtIndex(index);
        }
        else if (property.GetArrayElementAtIndex(index).FindPropertyRelative("visualRoot").objectReferenceValue is Transform old)
            Object.DestroyImmediate(old.gameObject);
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, player.transform);
        instance.SetActive(false);
        var entry = property.GetArrayElementAtIndex(index);
        entry.FindPropertyRelative("classId").stringValue = ClassId;
        entry.FindPropertyRelative("visualRoot").objectReferenceValue = instance.transform;
        entry.FindPropertyRelative("animator").objectReferenceValue = ClassPresentationSwitcher.FindVisualAnimator(instance.transform);
        property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static Transform Part(Transform parent, string name, Vector3 point)
    {
        var part = new GameObject(name).transform;
        part.SetParent(parent, false);
        part.localPosition = point;
        return part;
    }

    private static void Solid(Transform parent, string name, Vector3 point, Vector3 size, Material material,
        PrimitiveType primitive = PrimitiveType.Cube)
    {
        var solid = GameObject.CreatePrimitive(primitive);
        Object.DestroyImmediate(solid.GetComponent<Collider>());
        solid.name = name;
        solid.transform.SetParent(parent, false);
        solid.transform.localPosition = point;
        solid.transform.localScale = size;
        solid.GetComponent<Renderer>().sharedMaterial = material;
    }

    private static void Prism(Transform parent, string name, Vector2[] outline, float thickness, Material material, Vector3 point)
    {
        var shape = Part(parent, name, point);
        string path = Folder + name + ".asset";
        var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        bool newMesh = mesh == null;
        {
            int count = outline.Length;
            float winding = 0;
            for (int i = 0; i < count; i++)
            {
                Vector2 a = outline[i], b = outline[(i+1)%count];
                winding += a.x*b.y - b.x*a.y;
            }
            if (winding < 0) outline = outline.Reverse().ToArray();
            var points = outline.Select(p => new Vector3(p.x,p.y,thickness*.5f))
                .Concat(outline.Select(p => new Vector3(p.x,p.y,-thickness*.5f))).ToArray();
            var indices = new System.Collections.Generic.List<int>();
            for (int i = 1; i < count - 1; i++)
            {
                indices.AddRange(new[] { 0,i,i+1, count,count+i+1,count+i });
            }
            for (int i = 0; i < count; i++)
            {
                int next = (i + 1) % count;
                indices.AddRange(new[] { i,i+count,next, next,i+count,next+count });
            }
            mesh ??= new Mesh { name = name };
            mesh.Clear();
            mesh.vertices = points;
            mesh.triangles = indices.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            if (newMesh) AssetDatabase.CreateAsset(mesh, path);
            else EditorUtility.SetDirty(mesh);
        }
        shape.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
        shape.gameObject.AddComponent<MeshRenderer>().sharedMaterial = material;
    }

    private static Material Material(string name, Color color, float metallic, float smoothness)
    {
        string path = Folder + name + ".mat";
        var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null) return existing;
        var material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = name, color = color };
        material.SetFloat("_Metallic", metallic);
        material.SetFloat("_Smoothness", smoothness);
        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    private static Texture2D Icon(int index)
    {
        string name = DefenderSkillProgression.Name((DefenderSkill)index);
        string path = Folder + name + " icon.asset";
        var existing = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (existing != null) return existing;
        const int size = 72;
        var icon = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = name };
        var color = index switch
        {
            0 => new Color(.99f,.67f,.33f), 1 => new Color(1f,.48f,.23f),
            2 => new Color(.48f,.78f,.96f), 3 => new Color(.80f,.86f,.91f),
            4 => new Color(.42f,.87f,.76f), _ => new Color(.99f,.83f,.47f)
        };
        Vector2[] shield = { new(-.42f,.47f),new(.42f,.47f),new(.43f,-.06f),new(.23f,-.41f),
            new(0,-.58f),new(-.23f,-.41f),new(-.43f,-.06f),new(-.42f,.47f) };
        Vector2[] strike = { new(-.59f,-.49f),new(.56f,.49f),new(.34f,.47f),new(.56f,.49f),new(.53f,.26f) };
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                var point = new Vector2((x+.5f)*2/size-1,(y+.5f)*2/size-1);
                float Segment(Vector2 a, Vector2 b)
                {
                    var distance = Vector2.Distance(point, a + (b-a) * Mathf.Clamp01(Vector2.Dot(point-a,b-a)/(b-a).sqrMagnitude));
                    return 1 - Mathf.SmoothStep(0,1,Mathf.InverseLerp(.023f,.046f,distance));
                }
                float Polyline(Vector2[] line)
                {
                    float value=0;
                    for (int i=1; i<line.Length; i++) value=Mathf.Max(value,Segment(line[i-1],line[i]));
                    return value;
                }
                float Ring(float radius) => 1 - Mathf.SmoothStep(0,1,Mathf.Abs(point.magnitude-radius)/.034f);
                float shape = index switch
                {
                    0 => Mathf.Max(Polyline(shield.Select(v=>v*.80f+new Vector2(.20f,0)).ToArray()),
                        Segment(new(-.70f,.15f),new(-.21f,.15f))),
                    1 => Mathf.Max(Ring(.53f), Mathf.Max(Segment(new(0,-.42f),new(0,.12f)),
                        Mathf.Max(Segment(new(0,.12f),new(-.31f,.40f)),Segment(new(0,.12f),new(.31f,.40f))))),
                    2 => Mathf.Max(Polyline(shield), Ring(.22f)*.65f),
                    3 => Mathf.Max(Polyline(strike), Ring(.69f)*Mathf.Clamp01(point.x-point.y)),
                    4 => Mathf.Max(Ring(.61f), Mathf.Max(Polyline(shield.Select(v=>v*.63f).ToArray()),
                        Segment(new(-.52f,-.61f),new(.52f,-.61f)))),
                    _ => Mathf.Max(Polyline(shield), Mathf.Max(Segment(new(-.24f,.20f),new(.24f,.20f)),
                        Segment(new(0,.20f),new(0,-.36f))))
                };
                var background = Color.Lerp(new Color(.035f,.049f,.067f), new Color(.12f,.16f,.19f),
                    Mathf.Clamp01(1-point.magnitude*.8f));
                icon.SetPixel(x,y,Color.Lerp(background,color,shape));
            }
        icon.Apply();
        AssetDatabase.CreateAsset(icon,path);
        return icon;
    }

    private static T Asset<T>(string name) where T : ScriptableObject
    {
        string path = Folder + name + ".asset";
        var existing = AssetDatabase.LoadAssetAtPath<T>(path);
        if (existing != null) return existing;
        var value = ScriptableObject.CreateInstance<T>();
        value.name = name;
        AssetDatabase.CreateAsset(value,path);
        return value;
    }
}