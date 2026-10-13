using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DiceFree.Ironpine
{
    /// <summary>
    /// Nonredistributable Unity Asset Store wolf overlay. The freely shareable
    /// Blender wolf remains a fully working visual fallback on other machines.
    /// This component never changes AI, collider, faction or drop rules.
    /// </summary>
    internal static class IronpineRealisticWolfRuntime
    {
        private const string Licensed = "IronpineLicensedWolf/RealisticWolf";
        private static readonly Color EyeRed = new(.65f,.095f,.09f);
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Register()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            ApplyToWorld();
        }
        private static void OnSceneLoaded(Scene scene,LoadSceneMode mode)=>ApplyToWorld();
        private static void ApplyToWorld()
        {
            var source=Resources.Load<GameObject>(Licensed);
            if(source==null)return;
            foreach(var wolf in UnityEngine.Object.FindObjectsByType<IronpineWildlife>())
            {
                if(wolf.Kind==WildlifeKind.Quillrat ||
                   wolf.transform.Find("Realistic furry wolf licensed visual")!=null)continue;
                var presentation=wolf.GetComponent<IronpineCreaturePresentation>();
                if(presentation==null || presentation.ModelAnimator==null)continue;
                var old=presentation.ModelAnimator.gameObject;
                var parent=old.transform.parent;
                var visual=UnityEngine.Object.Instantiate(source,parent,false);
                visual.name="Realistic furry wolf licensed visual";
                visual.transform.localPosition=old.transform.localPosition;
                // Vendor bones confirm head +Z / tail -Z. A 180 yaw here made
                // wolves chase backward and bite with their rears.
                visual.transform.localRotation=Quaternion.identity;
                // The pack's wolf is roughly 0.9m high. Preserve readable
                // gameplay proportions, making the alpha visibly larger.
                float size=wolf.Kind switch
                {
                    WildlifeKind.AlphaWolf=>4.70f, // double the prior Alpha scale
                    WildlifeKind.DarkWolf=>1.74f,
                    _=>1.62f
                };
                visual.transform.localScale=Vector3.one*size;
                var animator=visual.GetComponentInChildren<Animator>(true);
                if(animator==null || animator.runtimeAnimatorController==null)
                {
                    UnityEngine.Object.Destroy(visual);
                    continue; // Never break playable fallback when pack is damaged.
                }
                float coatTint=wolf.Kind switch
                {
                    WildlifeKind.DarkWolf=>.60f,
                    WildlifeKind.AlphaWolf=>.45f,
                    _=>1f
                };
                var urpLit=Shader.Find("Universal Render Pipeline/Lit");
                foreach(var renderer in visual.GetComponentsInChildren<Renderer>(true))
                {
                    var mats=renderer.sharedMaterials;
                    bool changed=false;
                    // Pack is installed in an isolated no-UPM environment with
                    // Standard fallback materials. Upgrade to real URP Lit once
                    // the user's game loads its Universal Render Pipeline.
                    if(urpLit!=null)
                    {
                        for(int j=0;j<mats.Length;j++)
                        {
                            var original=mats[j];
                            if(original==null || original.shader==null ||
                                original.shader.name!="Standard")continue;
                            var converted=new Material(urpLit){name=original.name+" URP"};
                            Texture texture=original.HasProperty("_BaseMap")
                                ? original.GetTexture("_BaseMap") :
                                original.HasProperty("_MainTex") ? original.GetTexture("_MainTex") : null;
                            if(texture!=null)converted.SetTexture("_BaseMap",texture);
                            converted.SetColor("_BaseColor",original.HasProperty("_BaseColor")
                                ? original.GetColor("_BaseColor"):Color.white);
                            if(original.IsKeywordEnabled("_ALPHATEST_ON") ||
                               (original.HasProperty("_AlphaClip") && original.GetFloat("_AlphaClip")>.5f))
                            {
                                converted.SetFloat("_AlphaClip",1f);
                                converted.SetFloat("_Cutoff",.22f);
                                converted.EnableKeyword("_ALPHATEST_ON");
                                converted.renderQueue=2450;
                            }
                            mats[j]=converted;
                            changed=true;
                        }
                    }
                    for(int i=0;i<mats.Length;i++)
                    {
                        var current=mats[i];
                        if(current==null)continue;
                        if(current.name.Contains("Eye",StringComparison.OrdinalIgnoreCase))
                        {
                            if(wolf.Kind!=WildlifeKind.AlphaWolf)continue;
                            var shader=Shader.Find("Universal Render Pipeline/Lit")??Shader.Find("Standard");
                            var eye=new Material(shader){name="Alpha subtle red eyes"};
                            eye.SetColor("_BaseColor",EyeRed);
                            if(eye.HasProperty("_EmissionColor"))
                            {
                                eye.EnableKeyword("_EMISSION");
                                eye.SetColor("_EmissionColor",EyeRed*1.6f);
                            }
                            mats[i]=eye;changed=true;
                        }
                        else if(coatTint<.99f)
                        {
                            var variant=new Material(current);
                            var color=new Color(coatTint,coatTint,coatTint,1f);
                            if(variant.HasProperty("_BaseColor"))
                                variant.SetColor("_BaseColor",color);
                            else if(variant.HasProperty("_Color"))
                                variant.SetColor("_Color",color);
                            mats[i]=variant;changed=true;
                        }
                    }
                    if(changed)renderer.materials=mats;
                }
                if(wolf.Kind==WildlifeKind.AlphaWolf)
                {
                    foreach(var bone in visual.GetComponentsInChildren<Transform>(true)
                        .Where(t=>t.name=="Eye.L"||t.name=="Eye.R"))
                    {
                        var light=new GameObject("Faint red eye ember");
                        light.transform.SetParent(bone,false);
                        light.transform.localPosition=Vector3.zero;
                        var sourceLight=light.AddComponent<Light>();
                        sourceLight.type=LightType.Point;
                        sourceLight.color=new Color(.91f,.12f,.1f);
                        sourceLight.intensity=.13f;
                        sourceLight.range=.31f;
                        sourceLight.shadows=LightShadows.None;
                    }
                }
                // Update both owner scripts before hiding the public fallback rig.
                presentation.Configure(animator);
                wolf.UseVisualModel(visual.transform);
                old.SetActive(false);
                animator.Play("Idle",0,0f);
            }
        }
    }
}
