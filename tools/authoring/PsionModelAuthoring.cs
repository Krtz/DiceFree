using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiceFree.Advancement;
using DiceFree.Items;
using DiceFree.Skills;
using DiceFree.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

public static class PsionModelAuthoring
{
    private const string Folder = "Assets/_DiceFree/Settings/Psion/Mirrorweave/";
    private const string GearFolder = "Assets/_DiceFree/Settings/Psion/Gear/";
    private const string ScenePath = "Assets/_DiceFree/Scenes/DevelopmentRoom.unity";
    private const string FormName = "Mirrorweave Psion";
    private const string ClassId = PsionSkillProgression.PsionClassId;

    private static Transform Joint(GameObject model,string name) =>
        model.GetComponentsInChildren<Transform>(true).Single(bone => bone.name == name);

    private static void Aim(GameObject model,string from,string to,Vector3 direction)
    {
        var bone = Joint(model,from);
        var child = Joint(model,to);
        bone.rotation = Quaternion.FromToRotation(child.position-bone.position,direction) * bone.rotation;
    }

    private static Dictionary<string,string> Mapping()
    {
        var human = new Dictionary<string,string>();
        foreach (string name in new[] { "Hips","Spine","Chest","UpperChest","Neck","Head" }) human.Add(name,name);
        foreach (var side in new[] { (Name:"Left",Suffix:".L"), (Name:"Right",Suffix:".R") })
        {
            foreach (string name in new[] { "Shoulder","UpperArm","LowerArm","Hand","UpperLeg","LowerLeg","Foot","Toes" })
                human.Add(side.Name+name,name+side.Suffix);
            foreach (string digit in new[] {"Middle","Thumb"})
            {
                human.Add(side.Name+" "+digit+" Proximal",digit+".01"+side.Suffix);
                human.Add(side.Name+" "+digit+" Intermediate",digit+".02"+side.Suffix);
            }
        }
        return human;
    }

    private static HumanDescription Describe(GameObject posed)
    {
        foreach (string side in new[] { ".L",".R" })
        {
            var chest = Joint(posed,"Chest");
            var upperArm = Joint(posed,"UpperArm"+side);
            var outward = Vector3.ProjectOnPlane(upperArm.position-chest.position,Vector3.up).normalized;
            Aim(posed,"UpperArm"+side,"LowerArm"+side,outward);
            Aim(posed,"LowerArm"+side,"Hand"+side,outward);
            Aim(posed,"Hand"+side,"Middle.01"+side,outward);
            Aim(posed,"Middle.01"+side,"Middle.02"+side,outward);
            Aim(posed,"UpperLeg"+side,"LowerLeg"+side,Vector3.down);
            Aim(posed,"LowerLeg"+side,"Foot"+side,Vector3.down);
        }
        return new HumanDescription
        {
            human = Mapping().Select(pair => new HumanBone {
                humanName=pair.Key,boneName=pair.Value,limit=new HumanLimit {useDefaultValues=true}
            }).ToArray(),
            skeleton = posed.GetComponentsInChildren<Transform>(true).Select(bone => new SkeletonBone {
                name=bone.name,position=bone.localPosition,rotation=bone.localRotation,scale=bone.localScale
            }).ToArray(),
            armStretch=.05f,legStretch=.05f,upperArmTwist=.5f,lowerArmTwist=.5f,
            upperLegTwist=.5f,lowerLegTwist=.5f,feetSpacing=0,hasTranslationDoF=false
        };
    }

    public static void PrepareTextures()
    {
        string source = Path.Combine(Environment.GetEnvironmentVariable("DICEFREE_PSION_ART"),"Psion-Mirrorweave-ORM.png");
        var image = new Texture2D(2,2,TextureFormat.RGBA32,false,true);
        if (!ImageConversion.LoadImage(image,File.ReadAllBytes(source))) throw new InvalidOperationException("Invalid Psion ORM.");
        var pixels = image.GetPixels32();
        for (int i=0;i<pixels.Length;i++)
        {
            var input = pixels[i];
            pixels[i] = new Color32(input.b,0,0,(byte)(255-input.g));
        }
        image.SetPixels32(pixels);
        image.Apply();
        File.WriteAllBytes(Folder+"Mirrorweave metal-smoothness.png",image.EncodeToPNG());
        Object.DestroyImmediate(image);
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
    }

    public static void Batch()
    {
        if (!File.Exists(Folder+"Mirrorweave metal-smoothness.png")) PrepareTextures();
        string metalPath = Folder+"Mirrorweave metal-smoothness.png";
        var importer = (TextureImporter)AssetImporter.GetAtPath(metalPath);
        if (importer.sRGBTexture)
        {
            importer.sRGBTexture = false;
            importer.SaveAndReimport();
        }
        var model = Load<GameObject>(Folder+"Mirrorweave-Psion.fbx");
        var posed = (GameObject)PrefabUtility.InstantiatePrefab(model);
        posed.name = model.name;
        var avatar = AvatarBuilder.BuildHumanAvatar(posed,Describe(posed));
        Object.DestroyImmediate(posed);
        if (avatar == null || !avatar.isValid || !avatar.isHuman) throw new InvalidOperationException("Invalid Psion avatar.");
        avatar.name = "Mirrorweave Humanoid";
        string avatarPath = Folder+avatar.name+".asset";
        var existingAvatar = AssetDatabase.LoadAssetAtPath<Avatar>(avatarPath);
        if (existingAvatar == null) AssetDatabase.CreateAsset(avatar,avatarPath);
        else
        {
            EditorUtility.CopySerialized(avatar,existingAvatar);
            Object.DestroyImmediate(avatar);
            avatar = existingAvatar;
            EditorUtility.SetDirty(avatar);
        }

        var cloth = Material("Mirrorweave fabric",Shader.Find("Universal Render Pipeline/Lit"));
        cloth.SetTexture("_BaseMap",Load<Texture2D>(Folder+"Psion-Mirrorweave-BaseColor.png"));
        cloth.SetTexture("_MetallicGlossMap",Load<Texture2D>(metalPath));
        cloth.SetFloat("_Smoothness",1);
        cloth.EnableKeyword("_METALLICSPECGLOSSMAP");
        var ghost = Material("Mirrorweave apparition",Shader.Find("Universal Render Pipeline/Lit"));
        EditorUtility.CopySerialized(Load<Material>("Assets/_DiceFree/Resources/EarlySkillVfx/SoftRing.mat"),ghost);
        ghost.name = "Mirrorweave apparition";
        ghost.SetColor("_BaseColor",new Color(.55f,.39f,1,.62f));
        ghost.SetColor("_EmissionColor",new Color(.34f,.19f,.91f));
        ghost.EnableKeyword("_EMISSION");
        EditorUtility.SetDirty(cloth);
        EditorUtility.SetDirty(ghost);

        var body = (GameObject)PrefabUtility.InstantiatePrefab(model);
        body.name = FormName;
        var animator = body.GetComponent<Animator>();
        if (animator == null) animator = body.AddComponent<Animator>();
        animator.avatar = avatar;
        animator.runtimeAnimatorController = Load<RuntimeAnimatorController>("Assets/_DiceFree/Resources/PlayerMotion/Magical.controller");
        animator.applyRootMotion = false;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        var skin = body.GetComponentInChildren<SkinnedMeshRenderer>();
        skin.sharedMaterial = cloth;
        var prefab = PrefabUtility.SaveAsPrefabAsset(body,Folder+FormName+".prefab");
        Object.DestroyImmediate(body);

        var scene = EditorSceneManager.OpenScene(ScenePath);
        var actor = Object.FindObjectsByType<AdvancementController>(FindObjectsInactive.Include).Single();
        var switcher = actor.GetComponent<ClassPresentationSwitcher>();
        var switcherData = new SerializedObject(switcher);
        var bindings = switcherData.FindProperty("bindings");
        var entry = Enumerable.Range(0,bindings.arraySize).Select(bindings.GetArrayElementAtIndex)
            .Single(value => value.FindPropertyRelative("classId").stringValue == ClassId);
        var old = (Transform)entry.FindPropertyRelative("visualRoot").objectReferenceValue;
        var equipment = actor.GetComponent<EquipmentPresentation>();
        var blade = Load<ItemDefinition>(GearFolder+"Mirrorsteel Mindblade.asset");
        var book = Load<ItemDefinition>(GearFolder+"Mirrorleaf Codex.asset");
        foreach (var binding in equipment.Bindings.Where(value=>value.definition==blade||value.definition==book))
            foreach (var visual in binding.visuals)
                if (visual != null) Object.DestroyImmediate(visual);

        if (old != null && old.name == FormName) Object.DestroyImmediate(old.gameObject);
        var visualRoot = (GameObject)PrefabUtility.InstantiatePrefab(prefab,actor.transform);
        var gearBindings = FitGear(visualRoot,blade,book);
        visualRoot.SetActive(false);
        entry.FindPropertyRelative("visualRoot").objectReferenceValue = visualRoot.transform;
        entry.FindPropertyRelative("animator").objectReferenceValue = visualRoot.GetComponent<Animator>();
        switcherData.ApplyModifiedPropertiesWithoutUndo();

        equipment.Configure(equipment.Bindings.Where(value=>value.definition!=blade&&value.definition!=book)
            .Concat(gearBindings).ToArray());
        EditorUtility.SetDirty(equipment);
        var caster = new SerializedObject(actor.GetComponent<PsionSkillCaster>());
        caster.FindProperty("imageVisual").objectReferenceValue = prefab;
        caster.FindProperty("imageMaterial").objectReferenceValue = ghost;
        caster.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Could not save the Mirrorweave form.");
        AssetDatabase.SaveAssets();
        Debug.Log("DICEFREE_MIRRORWEAVE_AUTHOR_OK");
    }

    internal static EquipmentPresentation.Binding[] FitGear(GameObject root,ItemDefinition blade,ItemDefinition book)
    {
        var bladePose = Quaternion.FromToRotation(Vector3.up,new Vector3(.38f,.72f,.56f));
        var bookPose = Quaternion.LookRotation(new Vector3(-.10f,.92f,.38f),new Vector3(0,.38f,-.92f));
        var bladeVisual = Mount(root,blade,"Grip.R",bladePose);
        var bookVisual = Mount(root,book,"Grip.L",bookPose);
        return new[] { Binding(blade,bladeVisual), Binding(book,bookVisual) };
    }

    private static GameObject Mount(GameObject root,ItemDefinition item,string socket,Quaternion rotation)
    {
        var visual = (GameObject)PrefabUtility.InstantiatePrefab(
            Load<GameObject>(GearFolder+item.name+".prefab"),Joint(root,socket));
        visual.transform.localPosition = Vector3.zero;
        visual.transform.rotation = root.transform.rotation*rotation;
        visual.SetActive(false);
        return visual;
    }

    private static EquipmentPresentation.Binding Binding(ItemDefinition item,GameObject visual) =>
        new() { definition=item, slot=item.slot, visuals=new[]{visual} };

    private static Material Material(string name,Shader shader)
    {
        string path = Folder+name+".mat";
        var value = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (value != null) return value;
        value = new Material(shader) { name=name };
        AssetDatabase.CreateAsset(value,path);
        return value;
    }

    private static T Load<T>(string path) where T:Object =>
        AssetDatabase.LoadAssetAtPath<T>(path) ?? throw new InvalidOperationException("Missing "+path);
}
