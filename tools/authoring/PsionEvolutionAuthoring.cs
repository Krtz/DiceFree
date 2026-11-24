using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using DiceFree.Advancement;
using DiceFree.Combat;
using DiceFree.Items;
using DiceFree.Persistence;
using DiceFree.Progression;
using DiceFree.Quests;
using DiceFree.Skills;
using DiceFree.UI;
using DiceFree.World;
using DiceFree.Visuals;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using Object=UnityEngine.Object;

public static class PsionEvolutionAuthoring
{
    private const string Data="Assets/_DiceFree/Settings/PsionEvolutions/";
    private const string Parent="Assets/_DiceFree/Settings/Psion/";
    private static ActorDefinition[] classes;
    private static AdvancementDefinition[] edges;
    private static QuestDefinition[] quests;
    private static ItemDefinition[][] gear;
    private static GameObject[] forms;
    public static void Batch()
    {
        Directory.CreateDirectory(Data);AssetDatabase.Refresh();
        classes=new ActorDefinition[2];edges=new AdvancementDefinition[2];quests=new QuestDefinition[2];gear=new ItemDefinition[2][];forms=new GameObject[2];
        for(int n=0;n<2;n++)AuthorClass(n);
        foreach(var path in new[]{"Assets/_DiceFree/Settings/Progression/Class catalog.asset","Assets/_DiceFree/Settings/Development/Priest/Development Priest catalog.asset"})
        {var catalog=Load<ClassCatalog>(path);catalog.Configure(catalog.Classes.Where(c=>!classes.Contains(c)).Concat(classes).ToArray());EditorUtility.SetDirty(catalog);}
        PsionEvolutionGearAuthoring.Author(gear);AuthorCompatibility();AuthorMenu();AssetDatabase.SaveAssets();
        AuthorScene("Cornberg",true);AuthorScene("DevelopmentRoom",false);AuthorWorldTwoTeacher();
        EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/StartMenu.unity");AssetDatabase.SaveAssets();Debug.Log("DICEFREE_PSION_EVOLUTIONS_AUTHOR_OK");
    }
    private static void AuthorClass(int n)
    {
        bool k=n==0;string name=k?"Psionic Knight":"Mirage Mage",id=k?PsionEvolutionProgression.KnightId:PsionEvolutionProgression.MageId;
        var parent=Load<ActorDefinition>(Parent+"Psion class.asset");
        var attack=Asset<AttackDefinition>(name+" basic");EditorUtility.CopySerialized(parent.basicAttack,attack);attack.name=name+" basic";attack.stableId="attack."+(k?"psionic-knight":"mirage-mage")+".basic";attack.displayName=k?"Projected Mindblade":"Mirage Spark";attack.reach=k?3:9;attack.weights=new AttributeValues{agility=k?.6f:.4f,intelligence=k?.4f:.6f};
        var actor=Asset<ActorDefinition>(name+" class");EditorUtility.CopySerialized(parent,actor);actor.name=name+" class";actor.stableId=id;actor.displayName=name;
        actor.baseAttributes=new AttributeValues{vitality=k?22:19,strength=8,agility=k?30:22,intelligence=k?22:32,spirit=11};
        actor.growth=new AttributeValues{vitality=k?2.5f:2,strength=1,agility=k?5:2.7f,intelligence=k?2.7f:5,spirit=1};
        actor.baseHp=k?30:20;actor.moveSpeed=5;actor.overrideVitalityHp=true;actor.overrideVitalityRegeneration=true;actor.hpPerVitality=15;actor.regenerationPerVitality=.1f;actor.basicAttack=attack;classes[n]=actor;
        var mana=Asset<ClassResourceProfile>(name+" Mana");EditorUtility.CopySerialized(Load<ClassResourceProfile>(Parent+"Psion Mana.asset"),mana);mana.name=name+" Mana";mana.stableId="resource-profile."+id+".mana";mana.classId=id;
        mana.baseMaximum=k?140:160;mana.maximumPerIntelligence=k?4:5.5f;mana.maximumPerSpirit=k?1.5f:2;mana.baseRegenPerSecond=k?3.5f:4;mana.regenPerIntelligence=k?.045f:.07f;mana.regenPerSpirit=k?.035f:.055f;
        var quest=Asset<QuestDefinition>(k?"The Living Blade":"The Mirrored Storm");quest.stableId=k?"quest.psionic-knight.advancement":"quest.mirage-mage.advancement";quest.title=quest.name;quest.requiredClassId="class.psion";quest.requiredLevel=60;
        quest.offer=k?"A blade is only as strong as the will beside it. Enter the Moonlit Clearing alone and survive all ten waves. Begin a fresh trial after accepting my challenge; your reflections may accompany you. Then return to me.":"A reflection must hold its shape inside a storm. Accept my challenge, then enter Fourfold Hollow alone and overcome its ritual. Your images are part of you. Return when the Hollow falls silent. Its ordinary entrance requirements still apply.";
        quest.activeDialogue=k?"Complete a new solo Moonlit Clearing run, then return. Earlier clears do not prove this trial.":"Complete a new solo Fourfold Hollow run, then return. Earlier clears and other manifestations cannot complete this trial.";
        quest.turnInDialogue=k?"You and your reflection have learned to fight as one. Take the path of the Psionic Knight.":"You have found the still point within the storm. Take the path of the Mirage Mage.";
        quest.locationHint=k?"Find the Whispered Blade among Ironpine's quieter trees.":"Seek the Glass Seer in Steamroot Vale.";quest.turnInHint="Return to the hidden teacher who gave you this challenge.";
        quest.stages=new[]{new QuestObjective{kind=ObjectiveKind.SoloDungeon,objectiveId="fresh-solo-clear",contentId=k?"dungeon.ironpine.wolf-survival":"dungeon.fourfold-hollow",count=1,instruction=k?"Complete all ten Moonlit Clearing waves solo after accepting this quest":"Complete Fourfold Hollow solo after accepting this quest"}};
        quests[n]=quest;
        var edge=Asset<AdvancementDefinition>(name+" advancement");edge.stableId="advance.psion."+(k?"psionic-knight":"mirage-mage");edge.displayName=name;edge.sourceClass=parent;edge.targetClass=actor;edge.requiredLevel=60;edge.requiredQuestId=quest.stableId;edges[n]=edge;
        var main=Asset<ItemDefinition>(k?"Whispersteel Mindblade":"Prismbound Grimoire");EditorUtility.CopySerialized(Load<ItemDefinition>(Parent+"Gear/Mirrorsteel Mindblade.asset"),main);main.name=k?"Whispersteel Mindblade":"Prismbound Grimoire";main.displayName=main.name;main.stableId=k?"item.psionic-knight.whispersteel-mindblade":"item.mirage-mage.prismbound-grimoire";main.allowedClassIds=new[]{id};main.itemLevel=1;main.flavor=k?"The edge answers a thought before the hand has moved.":"A hundred mirrored pages hold one impossible spell.";
        main.stats.attributes.agility=k?8:3;main.stats.attributes.intelligence=k?3:8;
        var off=Asset<ItemDefinition>(k?"Ionic Stone of Motion":"Ionic Stone of Reflection");EditorUtility.CopySerialized(Load<ItemDefinition>(Parent+"Gear/Mirrorleaf Codex.asset"),off);off.name=k?"Ionic Stone of Motion":"Ionic Stone of Reflection";off.displayName=off.name;off.stableId=k?"item.ionic-stone.motion":"item.ionic-stone.reflection";off.allowedClassIds=new[]{PsionEvolutionProgression.KnightId,PsionEvolutionProgression.MageId};off.stats.attributes.agility=k?5:1;off.stats.attributes.intelligence=k?1:5;off.itemLevel=1;off.flavor="A silver cage holds a thought that has not yet been spoken.";gear[n]=new[]{main,off};
        forms[n]=AuthorForm(n);
        foreach(var o in new Object[]{actor,attack,mana,quest,edge,main,off})EditorUtility.SetDirty(o);
    }
    private static GameObject AuthorForm(int n)
    {
        string key=n==0?"PsionicKnight":"MirageMage";
        string model=Data+key+"/"+key+".fbx";
        var source=Load<GameObject>(model);
        var importer=(ModelImporter)AssetImporter.GetAtPath(model);importer.animationType=ModelImporterAnimationType.Generic;importer.avatarSetup=ModelImporterAvatarSetup.CreateFromThisModel;importer.SaveAndReimport();source=Load<GameObject>(model);
        var posed=(GameObject)PrefabUtility.InstantiatePrefab(source);
        PrefabUtility.UnpackPrefabInstance(posed,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
        RepairAuxiliaryRoot(posed);
        var bones=posed.GetComponentsInChildren<Transform>(true);
        var leftWrist=bones.First(b=>b.name=="Bip001-L-Hand").localRotation;
        var rightWrist=bones.First(b=>b.name=="Bip001-R-Hand").localRotation;
        var map=new Dictionary<string,string>{{"Hips","Bip001-Pelvis"},{"Spine","Bip001-Spine"},{"Chest","Bip001-Spine1"},{"UpperChest","Bip001-Spine2"},{"Neck","Bip001-Neck"},{"Head","Bip001-Head"}};
        foreach(string side in new[]{"Left","Right"})
        {
            string s=side=="Left"?"L":"R";
            foreach(var pair in new[]{("Shoulder","Clavicle"),("UpperArm","UpperArm"),("LowerArm","Forearm"),("Hand","Hand"),("UpperLeg","Thigh"),("LowerLeg","Calf"),("Foot","Foot"),("Toes","Toe0")})map.Add(side+pair.Item1,"Bip001-"+s+"-"+pair.Item2);
        }
        foreach(string side in new[]{"L","R"})
        {
            Transform Joint(string name)=>bones.First(b=>b.name=="Bip001-"+side+"-"+name);
            var outward=Vector3.right*Mathf.Sign(Joint("UpperArm").position.x-bones.First(b=>b.name=="Bip001-Spine2").position.x);
            void Aim(Transform from,Transform to,Vector3 direction)=>from.rotation=Quaternion.FromToRotation(to.position-from.position,direction)*from.rotation;
            Aim(Joint("UpperArm"),Joint("Forearm"),outward);Aim(Joint("Forearm"),Joint("Hand"),outward);
            Aim(Joint("Thigh"),Joint("Calf"),Vector3.down);Aim(Joint("Calf"),Joint("Foot"),Vector3.down);
        }
        // Humanoid reference pose AND skin binding must describe the same pose.
        // The old avatar described a T-pose while the mesh remained bound in its A-pose.
        // Rebind the visibly posed geometry, retaining its original topology/UVs/weights.
        BakeRetargetPose(posed,key);
        bones=posed.GetComponentsInChildren<Transform>(true);
        var leftHand=bones.First(b=>b.name=="Bip001-L-Hand");var rightHand=bones.First(b=>b.name=="Bip001-R-Hand");
        var leftPalm=PalmLocal(posed,leftHand);var rightPalm=PalmLocal(posed,rightHand);
        var fingerAxis=leftHand.TransformVector(leftPalm).normalized;
        var palmNormal=leftHand.InverseTransformDirection(Vector3.ProjectOnPlane(-posed.transform.forward,fingerAxis).normalized);
        var leftUpper=bones.First(b=>b.name=="Bip001-L-UpperArm").localRotation;var leftLower=bones.First(b=>b.name=="Bip001-L-Forearm").localRotation;
        var rightUpper=bones.First(b=>b.name=="Bip001-R-UpperArm").localRotation;var rightLower=bones.First(b=>b.name=="Bip001-R-Forearm").localRotation;
        var description=new HumanDescription{human=map.Select(p=>new HumanBone{humanName=p.Key,boneName=bones.First(b=>b.name==p.Value).name,limit=new HumanLimit{useDefaultValues=true}}).ToArray(),skeleton=bones.Select(b=>new SkeletonBone{name=b.name,position=b.localPosition,rotation=b.localRotation,scale=b.localScale}).ToArray(),armStretch=.05f,legStretch=.05f,upperArmTwist=.5f,lowerArmTwist=.5f,upperLegTwist=.5f,lowerLegTwist=.5f};
        var avatar=AvatarBuilder.BuildHumanAvatar(posed,description);if(avatar==null||!avatar.isValid||!avatar.isHuman)throw new Exception("Invalid Tier III humanoid: "+key);
        avatar.name=key+" Humanoid";string avatarPath=Data+key+" Humanoid.asset";var stored=AssetDatabase.LoadAssetAtPath<Avatar>(avatarPath);if(stored==null)AssetDatabase.CreateAsset(avatar,avatarPath);else{EditorUtility.CopySerialized(avatar,stored);Object.DestroyImmediate(avatar);avatar=stored;EditorUtility.SetDirty(avatar);}
        var body=posed;body.name=n==0?"Whispersteel Psionic Knight":"Prismweave Mirage Mage";
        var animator=body.GetComponent<Animator>()??body.AddComponent<Animator>();animator.avatar=avatar;animator.runtimeAnimatorController=Load<RuntimeAnimatorController>("Assets/_DiceFree/Resources/PlayerMotion/Magical.controller");animator.applyRootMotion=false;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
        body.AddComponent<PsionEvolutionHandPose>().Configure(leftWrist,rightWrist,leftPalm,rightPalm,palmNormal,leftUpper,leftLower,rightUpper,rightLower);
        foreach(var renderer in body.GetComponentsInChildren<Renderer>(true))
        {
            renderer.sharedMaterials=renderer.sharedMaterials.Select(old=>{
                string safe=new string(old.name.Select(c=>char.IsLetterOrDigit(c)?c:'_').ToArray());string path=Data+key+"/"+safe+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
                if(mat==null){mat=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=old.name};AssetDatabase.CreateAsset(mat,path);}
                string textureName=n==0?(old.name.Contains("Hair")?"_DiffuseTexture":"Isaac__GuildWar_A_diff"):(old.name.Contains("Hair")?"brigitte_hair_diff":old.name.Contains("Weapon")?"weapon_diff":"brigitte_default_diff");
                Texture texture=AssetDatabase.LoadAssetAtPath<Texture2D>(Data+key+"/"+textureName+".png");if(texture==null)
                {
                    string[] candidates=AssetDatabase.FindAssets("t:Texture2D",new[]{Data+key});texture=candidates.Select(AssetDatabase.GUIDToAssetPath).Where(p=>p.IndexOf("diff",StringComparison.OrdinalIgnoreCase)>=0||p.IndexOf("base",StringComparison.OrdinalIgnoreCase)>=0).Select(AssetDatabase.LoadAssetAtPath<Texture2D>).FirstOrDefault();
                }
                if(texture!=null)mat.SetTexture("_BaseMap",texture);mat.SetFloat("_Smoothness",.32f);
                if(old.name.Contains("Hair")){mat.SetFloat("_AlphaClip",1);mat.SetFloat("_Cutoff",.35f);mat.SetFloat("_Cull",0);mat.EnableKeyword("_ALPHATEST_ON");}
                EditorUtility.SetDirty(mat);return mat;
            }).ToArray();
        }
        var result=PrefabUtility.SaveAsPrefabAsset(body,Data+key+" form.prefab");Object.DestroyImmediate(body);return result;
    }
    private static void BakeRetargetPose(GameObject model,string key)
    {
        int index=0;
        foreach(var skin in model.GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            var original=skin.sharedMesh;var baked=new Mesh();skin.BakeMesh(baked,false);
            // BakeMesh has already evaluated the import's unit conversion. Express
            // those points in the unscaled model root, including the mesh rotation.
            var intoRoot=model.transform.worldToLocalMatrix*Matrix4x4.TRS(skin.transform.position,skin.transform.rotation,Vector3.one);
            var mesh=Object.Instantiate(original);mesh.name=key+" coherent T-pose skin "+index;
            mesh.vertices=baked.vertices.Select(intoRoot.MultiplyPoint3x4).ToArray();
            mesh.normals=baked.normals.Select(v=>intoRoot.MultiplyVector(v).normalized).ToArray();
            mesh.tangents=baked.tangents.Select(t=>{var v=intoRoot.MultiplyVector(new Vector3(t.x,t.y,t.z)).normalized;return new Vector4(v.x,v.y,v.z,t.w);}).ToArray();
            skin.transform.SetParent(model.transform,true);skin.transform.localPosition=Vector3.zero;skin.transform.localRotation=Quaternion.identity;skin.transform.localScale=Vector3.one;
            mesh.bindposes=skin.bones.Select(b=>b.worldToLocalMatrix*skin.transform.localToWorldMatrix).ToArray();mesh.RecalculateBounds();
            int slot=index++;string path=Data+key+" retargeted skin "+slot+".asset";var saved=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(saved==null){AssetDatabase.CreateAsset(mesh,path);saved=mesh;}else{EditorUtility.CopySerialized(mesh,saved);Object.DestroyImmediate(mesh);EditorUtility.SetDirty(saved);}
            skin.sharedMesh=saved;skin.localBounds=saved.bounds;skin.updateWhenOffscreen=true;Object.DestroyImmediate(baked);
        }
    }
    private static Vector3 PalmLocal(GameObject model,Transform hand)
    {
        Vector3 sum=Vector3.zero;int count=0;
        foreach(var skin in model.GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            var mesh=skin.sharedMesh;var vertices=mesh.vertices;var weights=mesh.boneWeights;
            bool Belongs(int i)=>skin.bones[i]==hand||skin.bones[i].IsChildOf(hand);
            int weighted=0;float closest=float.PositiveInfinity;
            for(int i=0;i<vertices.Length;i++)
            {
                var w=weights[i];float weight=(Belongs(w.boneIndex0)?w.weight0:0)+(Belongs(w.boneIndex1)?w.weight1:0)+(Belongs(w.boneIndex2)?w.weight2:0)+(Belongs(w.boneIndex3)?w.weight3:0);
                var p=skin.transform.TransformPoint(vertices[i]);
                if(weight>.8f){weighted++;closest=Mathf.Min(closest,Vector3.Distance(p,hand.position));}
                if(weight>.8f&&(p-hand.position).sqrMagnitude<.25f){sum+=p;count++;}
            }
            Debug.Log("PALM_CANDIDATES "+hand.name+" at "+hand.position+" mesh "+mesh.bounds+" weighted "+weighted+" closest "+closest);
        }
        if(count<10)throw new Exception("Cannot locate skinned palm: "+hand.name);
        var local=hand.InverseTransformPoint(sum/count);
        Debug.Log("PSION_HAND_PALM "+model.name+" "+hand.name+" "+local+" from "+count+" vertices");return local;
    }
    private static void RepairAuxiliaryRoot(GameObject model)
    {
        var bones=model.GetComponentsInChildren<Transform>(true);var hips=bones.First(b=>b.name=="Bip001-Pelvis");
        var oldRoot=bones.FirstOrDefault(b=>b.name=="Dummy_Root")??bones.FirstOrDefault(b=>b.name=="Bip001");
        // These source rigs skin cloth/crystal vertices to a legacy non-humanoid root.
        // Make it follow the animated hips while preserving all bind-space positions.
        if(oldRoot!=null&&hips.IsChildOf(oldRoot))
        {var container=oldRoot.parent;hips.SetParent(container,true);oldRoot.SetParent(hips,true);}
    }
    private static void AuthorScene(string name,bool teachers)
    {
        var scene=EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/"+name+".unity");var controller=Object.FindAnyObjectByType<AdvancementController>(FindObjectsInactive.Include);var player=controller.GetComponent<CombatActor>();
        var catalog=Load<ClassCatalog>(name=="Cornberg"?"Assets/_DiceFree/Settings/Progression/Class catalog.asset":"Assets/_DiceFree/Settings/Development/Priest/Development Priest catalog.asset");
        controller.Configure(catalog,controller.Definitions.Where(e=>!edges.Contains(e)).Concat(edges).ToArray());player.GetComponent<ManifestationPersistence>().ConfigureClassCatalog(catalog);
        Add<PsionicKnightProgression>(player);Add<MirageMageProgression>(player);var caster=Add<PsionEvolutionCaster>(player);
        var voidHit=Asset<AttackDefinition>("Empowered Void");EditorUtility.CopySerialized(Load<AttackDefinition>(Parent+"Mental Image rupture.asset"),voidHit);voidHit.name="Empowered Void";voidHit.stableId="skill.psionic-knight.empowered-void";EditorUtility.SetDirty(voidHit);
        caster.Configure(Load<AttackDefinition>(Parent+"Psychic Bolt.asset"),Load<AttackDefinition>(Parent+"Lightning Storm.asset"),Load<AttackDefinition>(Parent+"Mental Image rupture.asset"),voidHit,Load<ElementDefinition>(Parent+"Metal.asset"),Load<ElementDefinition>("Assets/_DiceFree/Settings/Acolyte/Void.asset"),forms[0],forms[1],Load<Material>(Parent+"Mirrorweave/Mirrorweave apparition.mat"));
        Texture2D Icon(string file)=>Load<Texture2D>(Parent+file+" icon.asset");
        Add<PsionicKnightAbilityProvider>(player).Configure(new[]{Icon("Psionic Blades Mastery"),Icon("Mental Image"),PsionEvolutionGearAuthoring.Glyph("Personal Void Shield",0),PsionEvolutionGearAuthoring.Glyph("Empowered Attacks",1),PsionEvolutionGearAuthoring.Glyph("Speed Presence",2)});
        Add<MirageMageAbilityProvider>(player).Configure(new[]{Icon("Psychic Bolt"),Icon("Mental Image"),Icon("Lightning Storm"),Icon("Metal Shield"),PsionEvolutionGearAuthoring.Glyph("Speed Presence",2)});
        var resources=player.GetComponent<ActorResourceController>();var profiles=new[]{Load<ClassResourceProfile>(Data+"Psionic Knight Mana.asset"),Load<ClassResourceProfile>(Data+"Mirage Mage Mana.asset")};resources.Configure(resources.Profiles.Except(profiles).Concat(profiles).ToArray());
        var inventory=player.GetComponent<CarriedInventory>();inventory.Configure(inventory.Definitions.Except(gear.SelectMany(g=>g)).Concat(gear.SelectMany(g=>g)).ToArray());var journal=player.GetComponent<QuestJournal>();journal.Configure(journal.Definitions.Except(quests).Concat(quests).ToArray());
        var switcher=new SerializedObject(player.GetComponent<ClassPresentationSwitcher>());var bindings=switcher.FindProperty("bindings");
        var equipment=player.GetComponent<EquipmentPresentation>();var fitted=equipment.Bindings.Where(b=>!gear.SelectMany(g=>g).Contains(b.definition)).ToList();
        for(int n=0;n<2;n++)
        {
            int ix=Enumerable.Range(0,bindings.arraySize).Where(i=>bindings.GetArrayElementAtIndex(i).FindPropertyRelative("classId").stringValue==classes[n].stableId).DefaultIfEmpty(-1).First();
            if(ix<0){ix=bindings.arraySize;bindings.InsertArrayElementAtIndex(ix);}else if(bindings.GetArrayElementAtIndex(ix).FindPropertyRelative("visualRoot").objectReferenceValue is Transform old)Object.DestroyImmediate(old.gameObject);
            var form=(GameObject)PrefabUtility.InstantiatePrefab(forms[n],player.transform);fitted.AddRange(PsionEvolutionGearAuthoring.Fit(form,n,gear));form.SetActive(false);var entry=bindings.GetArrayElementAtIndex(ix);entry.FindPropertyRelative("classId").stringValue=classes[n].stableId;entry.FindPropertyRelative("visualRoot").objectReferenceValue=form.transform;entry.FindPropertyRelative("animator").objectReferenceValue=form.GetComponent<Animator>();
        }
        switcher.ApplyModifiedPropertiesWithoutUndo();equipment.Configure(fitted.ToArray());if(teachers)Teacher(0,scene,player,new Vector3(119,0,29));
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
    }
    private static void AuthorWorldTwoTeacher()
    {
        var scene=EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/WorldTwoArrival.unity");Teacher(1,scene,null,AshfallReach.Origin+new Vector3(-96,0,-31));EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
    }
    private static void Teacher(int n,UnityEngine.SceneManagement.Scene scene,CombatActor player,Vector3 wanted)
    {
        foreach(var old in Object.FindObjectsByType<PsionEvolutionMentor>(FindObjectsInactive.Include))if(old.gameObject.scene==scene)Object.DestroyImmediate(old.gameObject);
        var nav=Object.FindObjectsByType<WorldNavigation>(FindObjectsInactive.Include).First(v=>v.gameObject.scene==scene);var instance=NavMesh.AddNavMeshData(nav.Data);
        try
        {
            Vector3 p=Vector3.zero;bool found=false;
            for(int i=0;i<60&&!found;i++)
            {
                var candidate=wanted+new Vector3(Mathf.Cos(i)*i*.35f,0,Mathf.Sin(i)*i*.35f);
                if(!Physics.Raycast(candidate+Vector3.up*60,Vector3.down,out var ground,150,1<<8)||!NavMesh.SamplePosition(ground.point,out var hit,2,1))continue;
                if(Physics.CheckCapsule(hit.position+Vector3.up*.4f,hit.position+Vector3.up*1.7f,.5f,1<<9))continue;p=hit.position;found=true;
            }
            if(!found)throw new Exception("No hidden teacher position near "+wanted);
            var root=new GameObject(n==0?"Seren — hidden Knight teacher":"Vaelis — hidden Mirage teacher"){layer=11};root.transform.position=p;
            var form=(GameObject)PrefabUtility.InstantiatePrefab(forms[n],root.transform);form.transform.localRotation=Quaternion.Euler(0,125,0);
            var collider=root.AddComponent<CapsuleCollider>();collider.center=Vector3.up*.9f;collider.height=1.8f;collider.radius=.4f;
            var approach=new GameObject("Conversation approach").transform;approach.SetParent(root.transform,false);if(NavMesh.SamplePosition(p+Vector3.forward*1.3f,out var a,.5f,1))approach.position=a.position;else approach.position=p;
            var teacher=root.AddComponent<PsionEvolutionMentor>();teacher.Configure(n==0,quests[n],edges[n],gear[n]);teacher.ConfigureApproach(approach);
            if(player!=null){root.AddComponent<InteractionPrompt>().Configure(teacher,player);player.GetComponent<InteractionRegistry>()?.Register(teacher);}
            Debug.Log("PSION_TIER3_TEACHER "+teacher.TeacherId+" "+p);
        }finally{instance.Remove();}
    }
    private static void AuthorMenu()
    {
        var menu=Load<MenuPresentationCatalog>("Assets/_DiceFree/Resources/MenuPresentationCatalog.asset");
        menu.items=menu.items.Except(gear.SelectMany(g=>g)).Concat(gear.SelectMany(g=>g)).ToArray();
        var entries=menu.forms.Where(f=>!classes.Any(c=>c.stableId==f.classId)).ToList();
        for(int n=0;n<2;n++)
        {
            var root=new GameObject(classes[n].displayName+" menu presentation");
            try
            {
                root.AddComponent<ActorStats>().Configure(classes[n]);root.AddComponent<CarriedInventory>().Configure(menu.items);root.AddComponent<Equipment>();
                var visual=new GameObject("Class visual").transform;visual.SetParent(root.transform,false);
                var form=(GameObject)PrefabUtility.InstantiatePrefab(forms[n],visual);
                foreach(var skin in form.GetComponentsInChildren<SkinnedMeshRenderer>(true))skin.updateWhenOffscreen=true;
                root.AddComponent<EquipmentPresentation>().Configure(PsionEvolutionGearAuthoring.Fit(form,n,gear));
                var preview=PrefabUtility.SaveAsPrefabAsset(root,Data+classes[n].displayName+" menu.prefab");entries.Add(new MenuPresentationCatalog.Form{classId=classes[n].stableId,tier=3,preview=preview});
            }
            finally{Object.DestroyImmediate(root);}
        }
        menu.forms=entries.ToArray();EditorUtility.SetDirty(menu);
    }
    private static void AuthorCompatibility()
    {
        foreach(var id in AssetDatabase.FindAssets("t:ItemDefinition",new[]{"Assets/_DiceFree"}))
        {
            var item=Load<ItemDefinition>(AssetDatabase.GUIDToAssetPath(id));var allowed=item.allowedClassIds??Array.Empty<string>();if(allowed.Length==0)continue;
            bool parent=allowed.Contains("class.psion");bool agi=allowed.Contains("class.thief")&&item.stats.attributes.agility>0;bool magic=allowed.Contains("class.mage")&&item.stats.attributes.intelligence>0;
            var added=allowed.Concat(parent?new[]{classes[0].stableId,classes[1].stableId}:Array.Empty<string>()).Concat(agi?new[]{classes[0].stableId}:Array.Empty<string>()).Concat(magic?new[]{classes[1].stableId}:Array.Empty<string>()).Distinct().ToArray();
            if(added.Length!=allowed.Length){item.allowedClassIds=added;EditorUtility.SetDirty(item);}
        }
    }
    private static T Add<T>(CombatActor actor) where T:Component=>actor.GetComponent<T>()??actor.gameObject.AddComponent<T>();
    private static T Asset<T>(string name) where T:ScriptableObject {string p=Data+name+".asset";var value=AssetDatabase.LoadAssetAtPath<T>(p);if(value==null){value=ScriptableObject.CreateInstance<T>();value.name=name;AssetDatabase.CreateAsset(value,p);}return value;}
    private static T Load<T>(string path) where T:Object=>AssetDatabase.LoadAssetAtPath<T>(path)??throw new Exception("Missing "+path);
}


