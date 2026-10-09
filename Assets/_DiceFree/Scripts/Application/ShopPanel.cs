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
        private bool selling;private Vector2 sellScroll;
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
            GUI.Label(new Rect(r.x + 12, r.y + 10, 260, 25), shop.DisplayName);
            GUI.Label(new Rect(r.x + 12, r.y + 238, 300, 30), "Gold: " + wallet.Gold);
            if(GUI.Button(new Rect(r.x+280,r.y+10,90,25),"Buy"))selling=false;
            if(GUI.Button(new Rect(r.x+378,r.y+10,100,25),"Sell"))selling=true;
            if(selling)
            {
                GUILayout.BeginArea(new Rect(r.x+12,r.y+44,476,140));sellScroll=GUILayout.BeginScrollView(sellScroll);
                foreach(var item in GetComponent<CarriedInventory>().Items)
                {
                    var definition=GetComponent<CarriedInventory>().Resolve(item.definitionId);if(definition==null)continue;
                    GUILayout.BeginHorizontal();GUILayout.Label(definition.displayName+" - "+shop.ResalePrice(definition)+"g");
                    bool prior=GUI.enabled;GUI.enabled=prior&&shop.CanSell(actor,item.instanceId);if(GUILayout.Button("Sell",GUILayout.Width(80)))shop.Sell(actor,item.instanceId,out message);GUI.enabled=prior;GUILayout.EndHorizontal();
                }
                GUILayout.EndScrollView();GUILayout.EndArea();
            }
            else for (int i = 0; i < shop.Offers.Length; i++)
            {
                var offer = shop.Offers[i];
                if (offer?.item == null) continue;
                float y = r.y + 44 + i * 64;
                var iconRect = new Rect(r.x + 12, y, 48, 48);
                if (!ItemIconGUI.Draw(iconRect, offer.item)) GUI.Label(iconRect, offer.item.displayName.Substring(0, Mathf.Min(3, offer.item.displayName.Length)));
                GUI.Label(iconRect, new GUIContent("", HudTooltip.Item(offer.item)), GUIStyle.none);
                GUI.Label(new Rect(r.x + 66, y, 290, 25), new GUIContent(offer.item.displayName, HudTooltip.Item(offer.item)));
                GUI.Label(new Rect(r.x + 66, y + 24, 290, 25), offer.goldPrice + " gold" + (offer.provisionalPrice ? " (provisional)" : ""));
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
