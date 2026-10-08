using DiceFree.Combat;
using DiceFree.Items;
using DiceFree.World;
using DiceFree.Input;
using UnityEngine;

namespace DiceFree.UI
{
    [RequireComponent(typeof(Interactor))]
    public sealed class ShopPanel : HudWidget
    {
        private Interactor interactor;
        private string message = "";
        private ItemShop last;
        private void Awake() => interactor = GetComponent<Interactor>();
        public override Rect Bounds => interactor != null && interactor.Active is ItemShop
            ? new Rect(Screen.width / 2 - 250, Screen.height / 2 - 140, 500, 280) : default;
        private void OnGUI()
        {
            if (HudPointerBlocker.ModalOpen || interactor.Active is not ItemShop shop) return;
            if (last != shop) { message = ""; last = shop; }
            var actor = GetComponent<CombatActor>();
            var wallet = GetComponent<GoldWallet>();
            var r = Bounds;
            GUI.Box(r, GUIContent.none);
            GUI.Label(new Rect(r.x + 12, r.y + 10, 476, 25), shop.DisplayName + "    Gold: " + wallet.Gold);
            for (int i = 0; i < shop.Offers.Length; i++)
            {
                var offer = shop.Offers[i];
                if (offer?.item == null) continue;
                float y = r.y + 44 + i * 64;
                GUI.Label(new Rect(r.x + 12, y, 320, 25), new GUIContent(offer.item.displayName, HudTooltip.Item(offer.item)));
                GUI.Label(new Rect(r.x + 12, y + 24, 340, 25), offer.goldPrice + " gold" + (offer.provisionalPrice ? " (provisional)" : ""));
                bool enabled = GUI.enabled;
                GUI.enabled = enabled && wallet.CanSpend(offer.goldPrice) && shop.CanInteract(actor);
                if (GUI.Button(new Rect(r.x + 366, y + 8, 122, 32), "Buy")) shop.Purchase(actor, i, out message);
                GUI.enabled = enabled;
            }
            GUI.Label(new Rect(r.x + 12, r.y + 188, 476, 42), message, new GUIStyle(GUI.skin.label) { wordWrap = true });
            string close = InputBindings.Display(InputBindings.Current.Action("Gameplay/Close interaction"));
            if (GUI.Button(new Rect(r.x + 366, r.y + 238, 122, 30), "Close [" + close + "]")) interactor.Cancel();
            HudTooltip.DrawCurrent();
        }
    }
}
