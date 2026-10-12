using System;
using System.Linq;
using System.Reflection;
using DiceFree.Combat;
using DiceFree.Characters;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

namespace DiceFree.EditorTools
{
    [InitializeOnLoad]
    public static class StunFootprintPlaymodeProbe
    {
        private const string Session="DiceFree.StunFootprint.Probe";
        private static int frames;
        static StunFootprintPlaymodeProbe()=>EditorApplication.playModeStateChanged+=OnState;
        public static void Run()
        {
            if(!System.Environment.GetCommandLineArgs().Contains("-diceFreeSaveRoot"))
                throw new InvalidOperationException("Use -diceFreeSaveRoot for isolated playtest saves.");
            EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/Cornberg.unity");
            SessionState.SetBool(Session,true);
            EditorApplication.EnterPlaymode();
        }
        private static void OnState(PlayModeStateChange state)
        {
            if(state!=PlayModeStateChange.EnteredPlayMode ||
               !SessionState.GetBool(Session,false))return;
            frames=0;EditorApplication.update+=Tick;
        }
        private static void Check(bool ok,string reason)
        {
            if(!ok)throw new Exception("STUN_COLLISION_TEST: "+reason);
        }
        private static void Tick()
        {
            try
            {
                if(!EditorApplication.isPlaying || ++frames<60)return;
                var input=UnityEngine.Object.FindFirstObjectByType<TraversalInput>();
                Check(input!=null,"player missing");
                var player=input.GetComponent<CombatActor>();
                var actorList=UnityEngine.Object.FindObjectsByType<CombatActor>(FindObjectsSortMode.None);
                var animals=UnityEngine.Object.FindObjectsByType<DiceFree.Ironpine.IronpineWildlife>(FindObjectsSortMode.None);
                Check(player!=null && player.Alive && animals.Length==22,"actors or wildlife missing");
                Check(player.Radius>=.67f,"player circular footprint not increased");
                int checkedActors=0;
                foreach(var a in actorList)
                {
                    if(!a.Alive)continue;
                    var agent=a.GetComponent<NavMeshAgent>();
                    var capsule=a.GetComponent<CapsuleCollider>();
                    Check(agent!=null && capsule!=null &&
                        Mathf.Abs(a.Radius-agent.radius)<.002f &&
                        Mathf.Abs(a.Radius-capsule.radius)<.002f,
                        "agent/collider/attack footprint disagree: "+a.name);
                    Check(a.GetComponent<ActorFootprintSeparation>()!=null,
                        "missing non-overlap resolver: "+a.name);
                    checkedActors++;
                }
                var wolf=animals.FirstOrDefault(a=>a.Kind==DiceFree.Ironpine.WildlifeKind.GreyWolf)?.Actor;
                var alpha=animals.FirstOrDefault(a=>a.Kind==DiceFree.Ironpine.WildlifeKind.AlphaWolf)?.Actor;
                var rat=animals.FirstOrDefault(a=>a.Kind==DiceFree.Ironpine.WildlifeKind.Quillrat)?.Actor;
                Check(wolf!=null && wolf.Radius>=.72f &&
                    rat!=null && rat.Radius>=.54f &&
                    alpha!=null && alpha.Radius>=1.04f,
                    "enemy collision circles have incorrect sizes");
                var effect=wolf.Effects;
                Check(effect!=null && effect.TryApplyStun(2f,true),
                    "stun not applied to wolf");
                var visual=wolf.GetComponent<StunSpiralVisual>();
                Check(visual!=null&&visual.Visible,
                    "stun visual not spawned immediately");
                Check(visual.transform.Find("DiceFree stun blue swirls (VFX only)")!=null,
                    "blue swirling overhead visual hierarchy missing");
                foreach(var c in visual.transform.Find("DiceFree stun blue swirls (VFX only)").GetComponentsInChildren<Collider>())
                    Check(!c.enabled,"stun VFX has an active physical hitbox");
                effect.ClearTransient();
                Check(!visual.Visible,"stun visual persists after clearing status");
                effect.TryApplyStun(1f,true);
                Check(visual.Visible,"stun VFX does not return when restunned");
                effect.ClearTransient();

                // A deliberate character teleport onto a wolf should not
                // leave the character stuck inside the wolf's unit circle
                // once its own movement resolution executes.
                var motor=player.Motor;
                var otherPosition=wolf.transform.position;
                Vector3 near=otherPosition+new Vector3(.22f,0,.22f);
                Check(motor.Teleport(near),"unable to find test NavMesh near the wolf");
                var separation=player.GetComponent<ActorFootprintSeparation>();
                var method=typeof(ActorFootprintSeparation).GetMethod("LateUpdate",
                    BindingFlags.Instance|BindingFlags.NonPublic);
                Check(method!=null,"footprint runtime loop not found");
                method.Invoke(separation,null);
                float separation2D=Vector3.ProjectOnPlane(player.transform.position-wolf.transform.position,
                    Vector3.up).magnitude;
                Check(separation2D>=(player.Radius+wolf.Radius)*.90f,
                    "player still overlapping wolf after separation: "+separation2D);
                Check(separation.Corrections>0,"no corrective non-overlap was executed");
                player.SetGhosting(true);
                Check(player.Ghosting,"ghosting mode cannot be enabled");
                player.SetGhosting(false);
                Check(!player.Ghosting,"ghosting mode cannot be disabled");
                Debug.Log("DICEFREE_UNIT_COLLISION_AND_STUN_OK: "+checkedActors+
                    " actors, enlarged footprints, no-penetration, ghost exception and blue timed stun VFX");
                Finish(0);
            }
            catch(Exception ex){Debug.LogException(ex);Finish(1);}
        }
        private static void Finish(int code)
        {
            EditorApplication.update-=Tick;
            SessionState.SetBool(Session,false);
            EditorApplication.Exit(code);
        }
    }
}
