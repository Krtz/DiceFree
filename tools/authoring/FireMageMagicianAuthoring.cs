using System;
using System.Linq;
using DiceFree.UI;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;

public static class FireMageMagicianAuthoring
{
    public const string Art="Assets/_DiceFree/Art/Characters/FireMageMagician/";
    private static T Load<T>(string path)where T:Object=>AssetDatabase.LoadAssetAtPath<T>(path)??throw new InvalidOperationException("Missing "+path);
    public static GameObject CreateForm()
    {
        const string modelPath=Art+"EmberMagician.fbx";
        var model=Load<GameObject>(modelPath);var importer=(ModelImporter)AssetImporter.GetAtPath(modelPath);
        var names=new[]{"Hips","Spine","Chest","Neck","Head","LeftShoulder","LeftUpperArm","LeftLowerArm","LeftHand","LeftUpperLeg","LeftLowerLeg","LeftFoot","LeftToes","RightShoulder","RightUpperArm","RightLowerArm","RightHand","RightUpperLeg","RightLowerLeg","RightFoot","RightToes"};
        importer.animationType=ModelImporterAnimationType.Human;importer.avatarSetup=ModelImporterAvatarSetup.CreateFromThisModel;
        importer.humanDescription=new HumanDescription{
            human=names.Select(n=>new HumanBone{humanName=n,boneName=n,limit=new HumanLimit{useDefaultValues=true}}).ToArray(),
            skeleton=model.GetComponentsInChildren<Transform>(true).Select(t=>new SkeletonBone{name=t.name,position=t.localPosition,rotation=t.localRotation,scale=t.localScale}).ToArray(),
            upperArmTwist=.5f,lowerArmTwist=.5f,upperLegTwist=.5f,lowerLegTwist=.5f,armStretch=.05f,legStretch=.05f,feetSpacing=0
        };
        importer.importAnimation=false;importer.importCameras=false;importer.importLights=false;importer.preserveHierarchy=true;importer.isReadable=true;
        importer.importNormals=ModelImporterNormals.Import;importer.importTangents=ModelImporterTangents.CalculateMikk;
        string materialPath=Art+"Magician painted atlas.mat";var material=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if(material==null){material=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(material,materialPath);}
        material.SetColor("_BaseColor",Color.white);material.SetTexture("_BaseMap",Load<Texture2D>(Art+"Magician_Albedo.png"));material.SetFloat("_Smoothness",.12f);EditorUtility.SetDirty(material);
        importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material),"Magician painted atlas"),material);importer.SaveAndReimport();
        var staffImporter=(ModelImporter)AssetImporter.GetAtPath(Art+"EmberMagicianStaff.fbx");staffImporter.animationType=ModelImporterAnimationType.None;staffImporter.importAnimation=false;staffImporter.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material),"Magician painted atlas"),material);staffImporter.SaveAndReimport();
        var avatar=AssetDatabase.LoadAllAssetsAtPath(modelPath).OfType<Avatar>().Single();if(!avatar.isValid||!avatar.isHuman)throw new InvalidOperationException("Magician humanoid avatar is invalid");
        var controller=Load<RuntimeAnimatorController>("Assets/_DiceFree/Resources/PlayerMotion/Magical.controller");
        if(controller.animationClips.Any(c=>!c.humanMotion))throw new InvalidOperationException("Magical controller has an incompatible generic clip");
        var root=new GameObject("Ember Magician Fire Mage");
        try
        {
            model=(GameObject)PrefabUtility.InstantiatePrefab(Load<GameObject>(modelPath),root.transform);var animator=model.GetComponent<Animator>()??model.AddComponent<Animator>();
            animator.avatar=avatar;animator.runtimeAnimatorController=controller;animator.applyRootMotion=false;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
            foreach(var skin in model.GetComponentsInChildren<SkinnedMeshRenderer>(true))skin.updateWhenOffscreen=true;
            var grip=new GameObject("Original magician staff grip").transform;grip.SetParent(model.transform,false);
            PrefabUtility.InstantiatePrefab(Load<GameObject>(Art+"EmberMagicianStaff.fbx"),grip);
            model.AddComponent<FireMageStaffPresentation>().Configure(grip);
            foreach(var t in root.GetComponentsInChildren<Transform>(true))t.gameObject.layer=2;
            return PrefabUtility.SaveAsPrefabAsset(root,FireMageAuthoring.Data+"Fire Mage form.prefab");
        }
        finally{Object.DestroyImmediate(root);}
    }
}
