using System;
using System.IO;
using DiceFree.AI;
using DiceFree.Combat;
using DiceFree.UI;
using DiceFree.Verdant;
using DiceFree.World;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;

public static partial class ReedscaleLookoutAuthoring
{
    private static ActorDefinition Species(string role,string id,CombatTuning tuning,float hp,float damage,
        float reach,float windup,float speed,int xp)
    {
        bool ranged=id=="ridgebow-sentry";
        var attack=Asset<AttackDefinition>(role+" attack");
        attack.stableId="attack.reedscale."+id;
        attack.displayName=ranged?"Ridgebow arrow":"Spear thrust";
        attack.channel=DamageChannel.Physical;attack.requiresAccuracy=true;
        attack.baseDamage=damage;attack.coefficient=0;attack.reach=reach;attack.windup=windup;
        attack.interval=ranged?2.4f:1.85f;
        var unit=Asset<ActorDefinition>("Reedscale "+role);
        unit.stableId="enemy.reedscale."+id;
        unit.displayName="Reedscale "+role;
        unit.familyId="reedscale.lizardman";
        unit.tags=new[]{"lizardman","reedscale","lookout"};
        unit.tuning=tuning;
        unit.baseAttributes=new AttributeValues {strength=14,agility=18,vitality=8};
        unit.growth=default;unit.baseHp=hp;
        unit.overrideVitalityHp=unit.overrideVitalityRegeneration=true;
        unit.hpPerVitality=unit.regenerationPerVitality=0;
        unit.physicalDefense=ranged?12:20;unit.magicalDefense=ranged?15:9;
        unit.moveSpeed=speed;unit.experienceReward=xp;unit.basicAttack=attack;
        EditorUtility.SetDirty(attack);EditorUtility.SetDirty(unit);
        return unit;
    }

    private static GameObject Spawn(Transform parent,ActorDefinition definition,GameObject model,int level,
        Vector2 at,RespawnDefinition respawn,bool bow)
    {
        var root=Group(parent,definition.displayName+" (Lv "+level+")",Ground(at));
        root.localRotation=Quaternion.Euler(0,bow?225:180,0);
        root.gameObject.layer=10;
        var obj=root.gameObject;
        var agent=obj.AddComponent<NavMeshAgent>();agent.enabled=false;agent.height=2.25f;agent.areaMask=1;
        var capsule=obj.AddComponent<CapsuleCollider>();capsule.radius=.46f;capsule.height=2.25f;capsule.center=Vector3.up*1.125f;
        obj.AddComponent<ActorStats>().Configure(definition,level);
        obj.AddComponent<CombatActor>().Configure(1,.46f);
        obj.AddComponent<CombatStatusController>();
        obj.AddComponent<BasicAttack>();
        obj.AddComponent<DefeatReporter>();
        obj.AddComponent<OverworldRespawn>().Configure(respawn);
        obj.AddComponent<AggroBehaviour>().Configure(bow?10.5f:7.5f,bow?19:17);
        var rig=((GameObject)PrefabUtility.InstantiatePrefab(model,root)).transform;
        var arm=rig.Find(bow?"Left arm":"Right arm");
        var grip=arm.Find("Weapon grip");
        obj.AddComponent<ReedscaleMotion>().Configure(rig.Find("Left leg"),rig.Find("Right leg"),
            rig.Find("Left arm"),rig.Find("Right arm"),rig.Find("Tail"),grip,grip.Find("Arrow nock"),bow);
        obj.AddComponent<ActorFeedback>().Configure(rig);
        return obj;
    }

    private static GameObject ModelPrefab(bool bow)
    {
        var body=Group(null,bow?"Reedscale sentry model":"Reedscale spear guard model",Vector3.zero);
        Part(body,"Plated pelvis",PrimitiveType.Sphere,P(0,1.06f,0),P(.62f,.46f,.47f),leather);
        Part(body,"Reptilian torso",PrimitiveType.Capsule,P(0,1.42f,.035f),P(.65f,.46f,.48f),scale)
            .localRotation=Quaternion.Euler(13,0,0);
        for(int i=0;i<4;i++)
            Part(body,"Throat and belly scutes",PrimitiveType.Sphere,P(0,1.17f+i*.142f,.258f),P(.45f-i*.03f,.16f,.093f),belly);
        Part(body,"Indigo chest sash",PrimitiveType.Cube,P(0,1.51f,.315f),P(.135f,.68f,.054f),teal)
            .localRotation=Quaternion.Euler(0,0,-33);
        Part(body,"Bronze belt",PrimitiveType.Cube,P(0,1.11f,.252f),P(.58f,.13f,.084f),bronze);
        Part(body,"Sash tab",PrimitiveType.Cube,P(.12f,.91f,.217f),P(.25f,.33f,.085f),teal);
        Part(body,"Neck scales",PrimitiveType.Capsule,P(0,1.78f,.16f),P(.32f,.23f,.33f),scale);
        Part(body,"Reptile brow",PrimitiveType.Sphere,P(0,2.03f,.25f),P(.54f,.40f,.57f),scale);
        Part(body,"Long angular snout",PrimitiveType.Cube,P(0,1.96f,.54f),P(.37f,.24f,.46f),scale)
            .localRotation=Quaternion.Euler(-9,0,0);
        Part(body,"Pale lower jaw",PrimitiveType.Cube,P(0,1.856f,.54f),P(.35f,.095f,.47f),belly);
        Part(body,"Mouth seam",PrimitiveType.Cube,P(0,1.895f,.55f),P(.383f,.032f,.465f),leather);
        for(int i=0;i<4;i++)
            Part(body,"Crest spine",PrimitiveType.Sphere,P(0,2.21f-i*.035f,.23f-i*.10f),P(.15f,.37f-i*.042f,.23f),teal)
                .localRotation=Quaternion.Euler(-27,0,0);

        for(int side=-1;side<=1;side+=2)
        {
            Part(body,"Amber eye",PrimitiveType.Sphere,P(side*.242f,2.065f,.374f),P(.104f,.095f,.105f),eye);
            Part(body,"Slit pupil",PrimitiveType.Sphere,P(side*.274f,2.065f,.393f),P(.025f,.069f,.037f),leather);
            Part(body,"Snout nostril",PrimitiveType.Sphere,P(side*.130f,2.069f,.730f),P(.055f,.018f,.051f),leather);
            Part(body,"Angled brow ridge",PrimitiveType.Sphere,P(side*.22f,2.131f,.347f),P(.18f,.071f,.26f),belly)
                .localRotation=Quaternion.Euler(-9,0,side*14);
            for(int i=0;i<3;i++)
                Part(body,"Exposed teeth",PrimitiveType.Sphere,P(side*.173f,1.889f,.501f+i*.074f),P(.044f,.075f,.052f),ivory);
            Limbs(body,side,bow);
        }

        var tail=Group(body,"Tail",P(0,1.05f,-.21f));
        var points=new[]{Vector3.zero,P(0,-.19f,-.39f),P(.08f,-.37f,-.80f),P(.29f,-.45f,-1.13f),P(.48f,-.43f,-1.35f)};
        for(int i=0;i<points.Length-1;i++)
            Beam(tail,"Tapered tail segment",points[i],points[i+1],.16f-i*.029f,scale);
        var path=Art+"/"+body.name+".prefab";
        var prefab=PrefabUtility.SaveAsPrefabAsset(body.gameObject,path);
        Object.DestroyImmediate(body.gameObject);
        if(prefab==null)throw new IOException("Could not save "+path);
        return prefab;
    }

    private static void Limbs(Transform body,int side,bool bow)
    {
        string prefix=side<0?"Left":"Right";
        var leg=Group(body,prefix+" leg",P(side*.22f,1.01f,0));
        Beam(leg,"Digitigrade thigh",Vector3.zero,P(side*.085f,-.43f,.16f),.16f,scale);
        Beam(leg,"Bent hock",P(side*.085f,-.42f,.16f),P(side*.09f,-.78f,-.10f),.109f,scale);
        Beam(leg,"Ankle",P(side*.09f,-.78f,-.10f),P(side*.1f,-.95f,.12f),.09f,belly);
        Part(leg,"Three-toed foot",PrimitiveType.Sphere,P(side*.1f,-.96f,.19f),P(.26f,.13f,.41f),scale);
        for(int i=0;i<3;i++)
            Part(leg,"Foot claw",PrimitiveType.Sphere,P(side*.10f+(i-1)*.073f,-.98f,.374f),P(.052f,.061f,.15f),ivory);
        var arm=Group(body,prefix+" arm",P(side*.35f,1.64f,.075f));
        Part(arm,"Bronze shoulder scale",PrimitiveType.Sphere,Vector3.zero,P(.34f,.31f,.40f),bronze);
        Beam(arm,"Upper arm",Vector3.zero,P(side*.15f,-.38f,.13f),.107f,scale);
        Beam(arm,"Leather forearm guard",P(side*.15f,-.38f,.13f),P(side*.09f,-.47f,.50f),.091f,leather);
        Part(arm,"Clawed hand",PrimitiveType.Sphere,P(side*.085f,-.47f,.54f),P(.18f,.18f,.18f),belly);
        bool armed=(bow&&side<0)||(!bow&&side>0);
        if(armed)Weapon(Group(arm,"Weapon grip",P(side*.075f,-.46f,.55f)),bow);
        if(!bow&&side<0)
        {
            Part(arm,"Ribbed hide shield",PrimitiveType.Sphere,P(-.16f,-.41f,.54f),P(.58f,.75f,.16f),leather)
                .localRotation=Quaternion.Euler(10,-23,-12);
            Part(arm,"Shield boss",PrimitiveType.Sphere,P(-.16f,-.40f,.65f),P(.19f,.23f,.10f),bronze);
        }
    }

    private static void Weapon(Transform grip,bool bow)
    {
        if(bow)
        {
            var rim=new[]{P(0,-.77f,.15f),P(0,-.45f,.35f),P(0,0,.41f),P(0,.45f,.35f),P(0,.77f,.15f)};
            for(int i=0;i<rim.Length-1;i++)Beam(grip,"Laminated bow limb",rim[i],rim[i+1],.035f,wood);
            Beam(grip,"Taut bowstring",rim[0],rim[^1],.012f,ivory);
            Group(grip,"Arrow nock",P(0,.12f,.34f));
        }
        else
        {
            var top=P(0,1.38f,.39f);
            Beam(grip,"Larch spear shaft",P(0,-.94f,-.24f),top,.048f,bark);
            Part(grip,"Bronze spear point",PrimitiveType.Sphere,top+P(0,.15f,.05f),P(.18f,.54f,.105f),bronze)
                .localRotation=Quaternion.Euler(15,0,0);
            Part(grip,"Spear grip",PrimitiveType.Cylinder,Vector3.zero,P(.133f,.22f,.133f),leather);
        }
    }
}
