using System;
using UnityEngine;

namespace DiceFree.Combat
{
    // References owned local packs; the authoring recipe is tracked separately.
    public sealed class AssetPackVfxLibrary:ScriptableObject
    {
        [Serializable]public sealed class Entry{public string key;public GameObject prefab;public float unitRadius=1;public float prewarmSeconds;}
        public Entry[] entries=Array.Empty<Entry>();
        public Entry Find(string key)=>Array.Find(entries,e=>e.key==key&&e.prefab!=null);
    }
    public static class AssetPackVfx
    {
        private static AssetPackVfxLibrary library;
        private static AssetPackVfxLibrary Library=>library!=null?library:library=Resources.Load<AssetPackVfxLibrary>("AssetLibraryLicensed/VisualEffects");
        public static bool TryAura(Transform parent,GroundAuraStyle style,float radius,out GameObject effect)
        {
            effect=null;var entry=Library?.Find("aura."+style);if(entry==null)return false;
            effect=UnityEngine.Object.Instantiate(entry.prefab,parent,false);effect.name="Korean pattern aura - "+style;
            effect.transform.localScale*=radius/Mathf.Max(.1f,entry.unitRadius);
            foreach(var t in effect.GetComponentsInChildren<Transform>(true))t.gameObject.layer=30;
            foreach(var ps in effect.GetComponentsInChildren<ParticleSystem>(true)){var main=ps.main;main.loop=true;main.stopAction=ParticleSystemStopAction.None;main.scalingMode=ParticleSystemScalingMode.Hierarchy;main.simulationSpace=ParticleSystemSimulationSpace.Local;}
            effect.SetActive(false);return true;
        }
        public static bool TryPlay(string key,Vector3 point,float radius=.7f,float seconds=1.1f,Transform follow=null)=>Spawn(key,point,radius,seconds,follow)!=null;
        public static GameObject Spawn(string key,Vector3 point,float radius=.7f,float seconds=1.1f,Transform follow=null,bool loop=false)
        {
            var entry=Library?.Find(key);if(entry==null)return null;
            var root=UnityEngine.Object.Instantiate(entry.prefab,point,entry.prefab.transform.rotation);
            root.name="Asset VFX - "+key;if(follow!=null)root.transform.SetParent(follow,true);
            root.transform.localScale*=Mathf.Max(.1f,radius)/Mathf.Max(.1f,entry.unitRadius);
            foreach(var t in root.GetComponentsInChildren<Transform>(true))t.gameObject.layer=2;
            foreach(var ps in root.GetComponentsInChildren<ParticleSystem>(true)){var main=ps.main;main.loop=loop;main.stopAction=ParticleSystemStopAction.None;main.scalingMode=ParticleSystemScalingMode.Hierarchy;ps.Simulate(entry.prewarmSeconds,false,true,true);ps.Play(false);}
            UnityEngine.Object.Destroy(root,Mathf.Clamp(seconds,.1f,30));return root;
        }
        public static string Theme(string classId,string skill)
        {
            string name=(skill??"").Replace("-","").Replace(" ","").ToLowerInvariant();
            if(name.Contains("heal")||name.Contains("mend")||name.Contains("lightburst")||name.Contains("holy")||name.Contains("growth")||name.Contains("revive")||name.Contains("grace"))return "holy";
            if(name.Contains("ice")||name.Contains("frost"))return "ice";
            if(name.Contains("ember")||name.Contains("burn")||name.Contains("fire")||name.Contains("gunpowder")||name.Contains("flame")||name.Contains("ignite")||name.Contains("bomb")||name.Contains("explod"))return "fire";
            if(name.Contains("lightning")||name.Contains("storm"))return "lightning";
            if(name.Contains("radiation")||name.Contains("halflife")||name.Contains("poison")||name.Contains("venom"))return "poison";
            if(name.Contains("sand")||name.Contains("earth"))return "earth";
            if(name.Contains("shield")||name.Contains("guard")||name.Contains("barrier")||name.Contains("aegis")||name.Contains("quickening")||name.Contains("haste")||name.Contains("step")||name.Contains("cry"))return "buff";
            if(classId=="class.acolyte"||name.Contains("skeleton")||name.Contains("smoke")||name.Contains("stealth")||name.Contains("sneak")||name.Contains("curse")||name.Contains("siphon")||name.Contains("shadow"))return "dark";
            if(name.Contains("psychic")||name.Contains("psionic")||name.Contains("mental")||name.Contains("mindblade")||name.Contains("mirage")||name.Contains("mirrored"))return "arcane";
            if(classId=="class.fire-mage")return "fire";
            if(classId=="class.priest")return "holy";
            if(classId=="class.psion"||classId=="class.psionic-knight"||classId=="class.mirage-mage"||classId=="class.mage"||classId=="class.magically-touched-novice")return "arcane";
            return "physical";
        }
        public static void Cast(CombatActor actor,string name)
        {
            if(!Application.isPlaying||actor==null||!actor.Alive||actor.Concealed)return;
            TryPlay("cast."+Theme(actor.Stats.Definition?.stableId,name),actor.transform.position+Vector3.up*.08f,.65f,.85f);
        }
        public static bool Impact(string name,Vector3 point,float radius=.7f)=>TryPlay("impact."+Theme("",name),point,Mathf.Clamp(radius,.35f,3),1.3f);
    }
}