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
        private string message="";
        private ItemShop last;
        private bool selling;
        private Vector2 sellScroll,buyScroll;
        private void Awake()=>interactor=GetComponent<Interactor>();
        public override Rect Bounds=>interactor!=null&&interactor.Active is ItemShop?
            new Rect(Screen.width/2-250,Mathf.Max(10,Screen.height*.12f),500,Mathf.Min(440,Screen.height*.58f)):default;
        private void OnGUI()
        {
            if(HudPointerBlocker.ModalOpen||interactor.Active is not ItemShop shop)return;
            if(last!=shop){message="";last=shop;buyScroll=sellScroll=Vector2.zero;}
            var actor=GetComponent<CombatActor>();var wallet=GetComponent<GoldWallet>();var inventory=GetComponent<CarriedInventory>();var r=Bounds;
            GUI.Box(r,GUIContent.none);GUI.Label(new Rect(r.x+12,r.y+10,260,25),shop.DisplayName);
            if(GUI.Button(new Rect(r.x+280,r.y+10,90,25),"Buy"))selling=false;
            if(GUI.Button(new Rect(r.x+378,r.y+10,100,25),"Sell"))selling=true;
            var area=new Rect(r.x+12,r.y+44,476,r.height-130);
            GUILayout.BeginArea(area);
            if(selling)
            {
                sellScroll=GUILayout.BeginScrollView(sellScroll);
                foreach(var item in inventory.Items)
                {
                    var definition=inventory.Resolve(item.definitionId);if(definition==null)continue;
                    GUILayout.BeginHorizontal();GUILayout.Label(definition.displayName+" - "+shop.ResalePrice(definition)+"g");
                    bool prior=GUI.enabled;GUI.enabled=prior&&shop.CanSell(actor,item.instanceId);
                    if(GUILayout.Button("Sell",GUILayout.Width(80)))shop.Sell(actor,item.instanceId,out message);
                    GUI.enabled=prior;GUILayout.EndHorizontal();
                }
            }
            else
            {
                buyScroll=GUILayout.BeginScrollView(buyScroll);
                for(int i=0;i<shop.Offers.Length;i++)
                {
                    var offer=shop.Offers[i];if(offer?.item==null)continue;
                    GUILayout.BeginHorizontal(GUILayout.Height(58));
                    var iconRect=GUILayoutUtility.GetRect(48,48,GUILayout.Width(48));
                    if(!ItemIconGUI.Draw(iconRect,offer.item))GUI.Label(iconRect,offer.item.slot.ToString().Substring(0,2));
                    GUI.Label(iconRect,new GUIContent("",HudTooltip.Item(offer.item)),GUIStyle.none);
                    GUILayout.BeginVertical();GUILayout.Label(new GUIContent(offer.item.displayName,HudTooltip.Item(offer.item)));
                    GUILayout.Label(offer.goldPrice+" gold"+(offer.provisionalPrice?" (provisional)":""));GUILayout.EndVertical();
                    bool prior=GUI.enabled;GUI.enabled=prior&&wallet.CanSpend(offer.goldPrice)&&shop.CanInteract(actor);
                    if(GUILayout.Button("Buy",GUILayout.Width(88),GUILayout.Height(32)))shop.Purchase(actor,i,out message);
                    GUI.enabled=prior;GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndScrollView();GUILayout.EndArea();
            GUI.Label(new Rect(r.x+12,r.yMax-79,476,40),message,new GUIStyle(GUI.skin.label){wordWrap=true});
            GUI.Label(new Rect(r.x+12,r.yMax-36,300,30),"Gold: "+wallet.Gold);
            string close=InputBindings.Display(InputBindings.Current.Action("Gameplay/Close interaction"));
            if(GUI.Button(new Rect(r.x+366,r.yMax-36,122,30),"Close ["+close+"]"))interactor.Cancel();
            HudTooltip.DrawCurrent();
        }
    }
}
