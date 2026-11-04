using DiceFree.Combat;
using DiceFree.Input;
using DiceFree.World;
using UnityEngine;

namespace DiceFree.UI
{
    [DisallowMultipleComponent]
    public sealed class DeathRespawnHud : HudWidget
    {
        [SerializeField] private CombatActor player;
        [SerializeField] private RespawnAtAnchor respawn;

        private InputBindings bindings;
        private string returnBinding;
        private static DeathRespawnHud current;
        public static bool HasFallenPlayer => current != null && current.isActiveAndEnabled &&
            current.player != null && current.respawn != null && !current.player.Alive;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ClearCurrent() => current = null;
        private void OnDestroy() { if(current==this)current=null; }

        public override bool BlocksPointer => player != null && !player.Alive;
        public override Rect Bounds
        {
            get
            {
                float width=Mathf.Min(420f,Screen.width-24f);
                return new Rect((Screen.width-width)*.5f,Screen.height*.5f-102f,width,204f);
            }
        }

        public void Configure(CombatActor actor)
        {
            player = actor;
            respawn = actor == null ? null : actor.GetComponent<RespawnAtAnchor>();
        }

        private void Awake()
        {
            current=this;
            if (player == null) player = GetComponent<CombatActor>();
            if (player != null) Configure(player);

            bindings = InputBindings.Current;
            var action = bindings.Action("Gameplay/Return to anchor");
            returnBinding = InputBindings.Display(action);
        }

        private void OnGUI()
        {
            if (player == null || respawn == null || HudPointerBlocker.ModalOpen) return;
            int previousDepth=GUI.depth;
            GUI.depth=-180;
            try { DrawReturnScreen(); }
            finally { GUI.depth=previousDepth; }
        }
        private void DrawReturnScreen()
        {
            if(player.Alive)
            {
                if(Time.unscaledTime-respawn.LastAnchorActivatedAt<4f)
                {
                    var toast=new Rect(Screen.width*.5f-190f,42f,380f,32f);
                    var theme=HudLayoutManager.Current?.Theme??HudThemes.Get(HudThemes.CompactStone);
                    HudChrome.DrawPanel(toast,theme);
                    GUI.Label(toast,"Revive point attuned: "+respawn.AnchorName,new GUIStyle(GUI.skin.label)
                    {alignment=TextAnchor.MiddleCenter,normal={textColor=theme.textTint}});
                }
                return;
            }

            Rect rect = Bounds;
            var metrics=HudLayoutManager.Current?.Theme??HudThemes.Get(HudThemes.CompactStone);
            HudChrome.Fill(new Rect(0,0,Screen.width,Screen.height),new Color(.025f,.015f,.025f,.16f));
            HudChrome.DrawPanel(rect,metrics);
            HudChrome.DrawHeader(new Rect(rect.x+6,rect.y+6,rect.width-12,24),metrics,"Resurrection");

            var title = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = 22,
                normal={textColor=metrics.textTint}
            };
            GUI.Label(new Rect(rect.x + 12, rect.y + 40, rect.width - 24, 30), "You have fallen", title);

            string destination = string.IsNullOrWhiteSpace(respawn.AnchorName)
                ? "the emergence point"
                : respawn.AnchorName;
            GUI.Label(
                new Rect(rect.x + 16, rect.y + 76, rect.width - 32, 42),
                "Return to " + destination,
                new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter,wordWrap=true,normal={textColor=metrics.textTint} });

            GUI.Label(new Rect(rect.x+16,rect.y+118,rect.width-32,22),
                respawn.CanReturn?"Your return is ready.":"Return available in "+respawn.SecondsUntilReturn.ToString("0.0")+"s",
                new GUIStyle(GUI.skin.label){alignment=TextAnchor.MiddleCenter,fontSize=11,normal={textColor=metrics.textTint}});

            bool previousEnabled=GUI.enabled;
            GUI.enabled = previousEnabled && respawn.CanReturn;
            string label = "Return [" + returnBinding + "]";
            if (GUI.Button(new Rect(rect.x + 40, rect.y + 150, rect.width - 80, 38), label,
                new GUIStyle(GUI.skin.button){fontSize=14,fontStyle=FontStyle.Bold}))
                respawn.Return();
            GUI.enabled = previousEnabled;
        }
    }
}
