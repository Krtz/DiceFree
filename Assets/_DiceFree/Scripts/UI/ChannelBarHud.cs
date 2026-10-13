using DiceFree.Combat;
using UnityEngine;

namespace DiceFree.UI
{
    /// <summary>
    /// Theme-aware, movable channel progress bar. Reads ANY IChannelingAbility
    /// on the controlled character, not a hardcoded Return Home timer.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ChannelBarHud : CustomizableHudWidget
    {
        private CombatActor player;
        public override string LayoutId => "channel-bar";
        public override string DisplayName => "Channel Bar";
        public override Rect DefaultNormalizedBounds =>
            new Rect(.355f,.712f,.29f,.058f);
        public override Vector2 MinimumPixelSize => new(230,50);
        public override bool BlocksPointer =>
            (HudLayoutManager.Current?.EditMode??false) || TryActive(out _);

        private void AcquirePlayer()
        {
            if(player!=null)return;
            var home=FindFirstObjectByType<ReturnHomeAbility>();
            if(home!=null)player=home.GetComponent<CombatActor>();
        }
        public bool TryActive(out IChannelingAbility spell)
        {
            spell=null;
            AcquirePlayer();
            if(player==null || !player.Alive)return false;
            foreach(var behaviour in player.GetComponents<MonoBehaviour>())
                if(behaviour is IChannelingAbility channel && channel.IsChanneling)
                {
                    spell=channel;
                    return true;
                }
            return false;
        }
        private void OnGUI()
        {
            if(HudPointerBlocker.ModalOpen)return;
            bool editing=HudLayoutManager.Current?.EditMode??false;
            if(!TryActive(out var spell) && !editing)return;
            Rect rect=Bounds;
            DrawPanel(rect);
            Rect interior=Inner(rect,1);
            float remaining=spell==null?0f:Mathf.Max(0f,spell.ChannelRemaining);
            float total=spell==null?1f:Mathf.Max(.01f,spell.ChannelDuration);
            string name=spell?.ChannelLabel??"Channelled spell";
            float progress=spell==null?.4f:Mathf.Clamp01(1f-remaining/total);
            var title=new Rect(interior.x,interior.y,interior.width,20);
            GUI.Label(title,name,new GUIStyle(GUI.skin.label)
            {
                alignment=TextAnchor.MiddleLeft,
                fontStyle=FontStyle.Bold,
                fontSize=12,
                normal={textColor=Theme.textTint}
            });
            if(spell!=null)
                GUI.Label(title,remaining.ToString("0.0")+"s",
                    new GUIStyle(GUI.skin.label)
                    {
                        alignment=TextAnchor.MiddleRight,
                        fontSize=12,
                        normal={textColor=Theme.textTint}
                    });
            Rect gauge=new(interior.x,interior.y+21,
                interior.width,Mathf.Max(9f,interior.height-23));
            HudChrome.Fill(gauge,new Color(.035f,.06f,.12f,.94f));
            HudChrome.Fill(new Rect(gauge.x+2f,gauge.y+2f,
                    Mathf.Max(0f,(gauge.width-4f)*progress),
                    Mathf.Max(0f,gauge.height-4f)),
                new Color(.23f,.61f,1f,.97f));
            HudChrome.Border(gauge,1f,Theme.accentTint);
        }
    }
}
