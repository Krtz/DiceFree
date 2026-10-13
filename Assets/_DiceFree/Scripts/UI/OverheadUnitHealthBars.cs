using System.Collections.Generic;
using DiceFree.Combat;
using DiceFree.Input;
using DiceFree.World;
using UnityEngine;

namespace DiceFree.UI
{
    /// <summary>
    /// Displays accurate live HP for every visible living combat unit.
    /// Player preferences independently determine self/ally/enemy visibility.
    /// Drawn as lightweight world-projected UI; never owns a clickable region.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class OverheadUnitHealthBars : MonoBehaviour
    {
        private struct HeightEntry
        {
            public float height;
            public float until;
        }
        private static readonly Dictionary<CombatActor,HeightEntry> heights=new();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ClearHeights()=>heights.Clear();

        private CombatActor player;
        private WorldFogOfWar fog;
        private GameplayPreferences preferences;
        public static bool ShouldShow(OverheadHealthVisibility visibility,
            float hp,float maximum) =>
            visibility==OverheadHealthVisibility.Always ||
            (visibility==OverheadHealthVisibility.WhenHurt &&
             maximum>0 && hp<maximum-.01f);

        public static float OverheadHeight(CombatActor actor)
        {
            if(actor==null)return 2f;
            CombatActor key=actor;
            if(heights.TryGetValue(key,out var value) &&
               value.until>Time.unscaledTime)return value.height;
            float height=1.55f;
            var capsule=actor.GetComponent<CapsuleCollider>();
            if(capsule!=null)height=Mathf.Max(height,
                capsule.center.y+capsule.height*.5f);
            foreach(var renderer in actor.GetComponentsInChildren<Renderer>())
            {
                if(renderer is LineRenderer || renderer is ParticleSystemRenderer ||
                    renderer.name.Contains("stun") ||
                    renderer.name.Contains("Stun"))continue;
                height=Mathf.Max(height,renderer.bounds.max.y-actor.transform.position.y);
            }
            height=Mathf.Clamp(height+.18f,1.35f,8f);
            heights[key]=new HeightEntry{height=height,until=Time.unscaledTime+1.5f};
            return height;
        }
        private void Awake()=>preferences=GameplayPreferences.Current;
        private void FindPlayer()
        {
            if(player!=null)return;
            var returnHome=FindFirstObjectByType<ReturnHomeAbility>();
            if(returnHome!=null)player=returnHome.GetComponent<CombatActor>();
        }
        public OverheadHealthVisibility ModeFor(CombatActor actor)
        {
            if(preferences==null)preferences=GameplayPreferences.Current;
            FindPlayer();
            if(actor==player)return preferences.SelfHealthBar;
            if(player!=null && actor!=null && actor.Faction!=player.Faction)
                return preferences.EnemyHealthBar;
            return preferences.AllyHealthBar;
        }
        private void OnGUI()
        {
            if(HudPointerBlocker.ModalOpen ||
                (HudLayoutManager.Current?.EditMode??false))return;
            var camera=Camera.main;
            if(camera==null)return;
            FindPlayer();
            if(player==null)return;
            if(fog==null)fog=FindFirstObjectByType<WorldFogOfWar>();
            GUI.depth=6;
            foreach(var unit in CombatActor.All)
            {
                if(unit==null || !unit.Alive || !unit.gameObject.activeInHierarchy)
                    continue;
                var health=unit.Health;
                if(health==null)continue;
                bool showHp=ShouldShow(ModeFor(unit),health.Current,health.Maximum);
                IChannelingAbility channel=null;
                foreach(var behaviour in unit.GetComponents<MonoBehaviour>())
                    if(behaviour is IChannelingAbility spell && spell.IsChanneling)
                    {channel=spell;break;}
                if(!showHp && channel==null)continue;
                if(fog!=null && !fog.IsVisible(unit.transform.position))continue;
                Vector3 target=unit.transform.position+
                    Vector3.up*OverheadHeight(unit);
                var point=camera.WorldToScreenPoint(target);
                if(point.z<=.01f || point.z>75f ||
                   point.x<0f || point.x>Screen.width ||
                   point.y<0f || point.y>Screen.height)continue;
                float width=Mathf.Clamp(105f-point.z*.45f+unit.Radius*10f,
                    62f,135f);
                Rect frame=new(point.x-width*.5f,Screen.height-point.y+8f,
                    width,15f);
                if(showHp)
                {
                float ratio=Mathf.Clamp01(health.Current/Mathf.Max(1f,health.Maximum));
                Color tint=unit==player?new Color(.20f,.80f,.85f,.97f):
                    unit.Faction==player.Faction?
                    new Color(.29f,.87f,.39f,.97f):
                    new Color(.88f,.25f,.23f,.97f);
                HudChrome.Fill(frame,new Color(.018f,.020f,.027f,.94f));
                HudChrome.Fill(new Rect(frame.x+2,frame.y+2,
                    Mathf.Max(0f,(frame.width-4)*ratio),frame.height-4),tint);
                HudChrome.Border(frame,1f,new Color(.9f,.9f,.9f,.72f));
                GUI.Label(frame,
                    Mathf.CeilToInt(health.Current)+"/"+
                    Mathf.CeilToInt(health.Maximum),
                    new GUIStyle(GUI.skin.label)
                    {
                        alignment=TextAnchor.MiddleCenter,
                        fontSize=9,
                        fontStyle=FontStyle.Bold,
                        normal={textColor=Color.white}
                    });
                }
                // Every channelled unit has a visible, independent cast gauge,
                // even if the owner disabled that unit's overhead HP display.
                if(channel!=null)
                {
                    float completion=Mathf.Clamp01(1f-channel.ChannelRemaining/
                        Mathf.Max(.01f,channel.ChannelDuration));
                    var castRect=new Rect(frame.x,frame.y+(showHp?18f:0f),
                        frame.width,18f);
                    HudChrome.Fill(castRect,new Color(.025f,.045f,.10f,.95f));
                    HudChrome.Fill(new Rect(castRect.x+2,castRect.y+2,
                        Mathf.Max(0f,(castRect.width-4f)*completion),
                        castRect.height-4f),new Color(.24f,.55f,.98f,.93f));
                    HudChrome.Border(castRect,1f,new Color(.65f,.79f,1f,.90f));
                    GUI.Label(castRect,channel.ChannelLabel+" "+
                        channel.ChannelRemaining.ToString("0.0")+"s",
                        new GUIStyle(GUI.skin.label)
                        {
                            alignment=TextAnchor.MiddleCenter,
                            fontSize=9,
                            fontStyle=FontStyle.Bold,
                            normal={textColor=Color.white}
                        });
                }
            }
        }
    }
}
