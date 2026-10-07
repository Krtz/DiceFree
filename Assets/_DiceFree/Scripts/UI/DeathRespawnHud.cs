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

        public override bool BlocksPointer => player != null && !player.Alive;
        public override Rect Bounds =>
            new(Screen.width * 0.5f - 190f, Screen.height * 0.5f - 78f, 380f, 156f);

        public void Configure(CombatActor actor)
        {
            player = actor;
            respawn = actor == null ? null : actor.GetComponent<RespawnAtAnchor>();
        }

        private void Awake()
        {
            if (player == null) player = GetComponent<CombatActor>();
            if (player != null) Configure(player);

            bindings = InputBindings.Current;
            var action = bindings.Action("Gameplay/Return to anchor");
            returnBinding = InputBindings.Display(action);
        }

        private void OnGUI()
        {
            if (player == null || respawn == null || player.Alive || HudPointerBlocker.ModalOpen) return;

            Rect rect = Bounds;
            var previous = GUI.color;
            GUI.color = new Color(0.025f, 0.02f, 0.02f, 0.94f);
            GUI.Box(rect, GUIContent.none);
            GUI.color = previous;

            var title = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = 22
            };
            GUI.Label(new Rect(rect.x + 12, rect.y + 12, rect.width - 24, 30), "You have fallen", title);

            string destination = string.IsNullOrWhiteSpace(respawn.AnchorName)
                ? "the emergence point"
                : respawn.AnchorName;
            GUI.Label(
                new Rect(rect.x + 16, rect.y + 47, rect.width - 32, 28),
                "Return to " + destination,
                new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter });

            GUI.enabled = respawn.CanReturn;
            string label = respawn.CanReturn
                ? "Return [" + returnBinding + "]"
                : "Return in " + respawn.SecondsUntilReturn.ToString("0.0") + "s";
            if (GUI.Button(new Rect(rect.x + 62, rect.y + 86, rect.width - 124, 42), label))
                respawn.Return();
            GUI.enabled = true;
        }
    }
}
