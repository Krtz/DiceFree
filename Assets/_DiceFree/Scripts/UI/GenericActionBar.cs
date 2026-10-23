using UnityEngine;
using UnityEngine.InputSystem;

namespace DiceFree.UI
{
    public sealed class GenericActionBar : CustomizableHudWidget
    {
        [SerializeField] private AbilityBarSource source;
        private string hoveredSkillTooltip;
        private IQueuedOrders queuedOrders;
        private Vector2 tooltipPointer;

        public override string LayoutId => "action-bar";
        public override string DisplayName => "Action Bar";
        public override Rect DefaultNormalizedBounds => new(0.535f, 0.785f, 0.405f, 0.195f);
        public override Vector2 MinimumPixelSize => new(440, 135);

        private void Awake()
        {
            if (source == null) source = GetComponent<AbilityBarSource>();
            foreach(var component in GetComponents<MonoBehaviour>())
                if(component is IQueuedOrders queue){queuedOrders=queue;break;}
        }

        private void Update()
        {
            if (source == null || HudPointerBlocker.ModalOpen ||
                (HudLayoutManager.Current != null && HudLayoutManager.Current.EditMode))
                return;

            for (int i = 0; i <= AbilityBarSource.UltimateSlot; i++)
                if (source.WasPressedThisFrame(i))
                    source.Activate(i);
        }

        private void OnGUI()
        {
            if (source == null || HudPointerBlocker.ModalOpen) return;

            hoveredSkillTooltip=null;
            tooltipPointer=Event.current.mousePosition;
            if(Mouse.current!=null)
            {
                Vector2 device=Mouse.current.position.ReadValue();
                Vector2 ui=new Vector2(device.x,Screen.height-device.y);
                // Mouse Input System can update without an IMGUI mouse event.
                if(ui.x>=0&&ui.x<=Screen.width&&ui.y>=0&&ui.y<=Screen.height)
                    tooltipPointer=ui;
            }
            if(queuedOrders==null)
                foreach(var component in GetComponents<MonoBehaviour>())
                    if(component is IQueuedOrders queue){queuedOrders=queue;break;}
            Rect panel = Bounds;
            DrawPanel(panel);
            if(source.HasPurchasableUpgrade)
            {
                Rect label=new(panel.x+12f,panel.y-23f,panel.width-24f,21f);
                HudChrome.Fill(label,new Color(.10f,.23f,.15f,.96f));
                GUI.Label(label,"+"+source.UnspentPoints+
                    " skill point"+(source.UnspentPoints==1?"":"s")+
                    " available — use + on a skill",
                    new GUIStyle(GUI.skin.label)
                    {
                        fontStyle=FontStyle.Bold,alignment=TextAnchor.MiddleCenter,
                        fontSize=11,normal={textColor=new Color(.85f,1f,.72f)}
                    });
            }
            if(queuedOrders!=null && queuedOrders.PendingOrders>0)
                GUI.Label(new Rect(panel.x+12,source.HasPurchasableUpgrade?panel.y-45:panel.y-21,panel.width-20,20),
                    "Queued orders: "+queuedOrders.PendingOrders+
                    "   (Shift+click to append; Stop to clear)");
            if (source.PageCount > 1)
            {
                var y = panel.y + 2f;
                if (GUI.Button(new Rect(panel.xMax-155f,y,30f,22f),"<")) source.SetPage((source.SkillPage+source.PageCount-1)%source.PageCount);
                GUI.Label(new Rect(panel.xMax-121f,y,82f,22f),"Skills "+(source.SkillPage+1)+"/"+source.PageCount);
                if (GUI.Button(new Rect(panel.xMax-34f,y,30f,22f),">")) source.SetPage((source.SkillPage+1)%source.PageCount);
            }
            Rect inner = Inner(panel);
            if(source.PageCount>1){inner.y+=24f;inner.height-=24f;}
            float gap = Theme.gap;
            float cellWidth = (inner.width - gap * 5) / 6f;
            float cellHeight = (inner.height - gap) / 2f;

            for (int row = 0; row < 2; row++)
            for (int column = 0; column < 6; column++)
            {
                int index = row * 6 + column;
                var rect = new Rect(
                    inner.x + column * (cellWidth + gap),
                    inner.y + row * (cellHeight + gap),
                    cellWidth,
                    cellHeight);

                DrawAbility(rect, index);
            }
            if(!(HudLayoutManager.Current?.EditMode ?? false))
            {
                if(!string.IsNullOrWhiteSpace(hoveredSkillTooltip))
                    HudTooltip.DrawAt(hoveredSkillTooltip,tooltipPointer);
            }
        }

        private void DrawAbility(Rect rect, int slot)
        {
            var view = source.View(slot);
            if (!view.Exists)
            {
                DrawEmpty(rect, slot);
                return;
            }

            bool interactable = !view.passive && view.learned &&
                                view.cooldown <= 0.001f &&
                                !(HudLayoutManager.Current?.EditMode ?? false);
            bool hovered = rect.Contains(tooltipPointer) ||
                rect.Contains(Event.current.mousePosition);
            if(hovered)
                hoveredSkillTooltip=view.tooltip;
            HudChrome.DrawSlot(rect, Theme, view.learned, hovered);

            // Draw the actual ability art before the interaction layer, not a
            // text-only button. Icons are tied to skill definitions in the
            // source rather than slot number (so class changes remain correct).
            Rect imageRect = new(rect.x + 3f, rect.y + 3f,
                Mathf.Max(1f, rect.width - 6f), Mathf.Max(1f, rect.height - 6f));
            if (view.icon != null)
            {
                Color tint=GUI.color;
                GUI.color=view.learned ? Color.white :
                    new Color(.43f,.45f,.48f,.79f);
                GUI.DrawTexture(imageRect, view.icon, ScaleMode.StretchToFill, true);
                GUI.color=tint;
            }
            else
                GUI.Label(rect, view.name,HudChrome.SlotTextStyle(Theme,true));

            // Hotkeys/ranks are compact corner overlays so they don't cover
            // the ability artwork, unlike the old two-line skill names.
            if (!string.IsNullOrEmpty(view.key))
            {
                float w=Mathf.Min(rect.width-4f,
                    Mathf.Max(19f, 9f+view.key.Length*8f));
                Rect badge=new(rect.x+2,rect.y+2,w,19);
                HudChrome.Fill(badge,new Color(.025f,.029f,.045f,.90f));
                GUI.Label(badge,view.key,new GUIStyle(GUI.skin.label)
                {
                    fontStyle=FontStyle.Bold,
                    alignment=TextAnchor.MiddleCenter,
                    fontSize=11,
                    normal={textColor=Color.white}
                });
            }

            if (slot<=AbilityBarSource.UltimateSlot)
            {
                Rect rankBadge=new(rect.xMax-37,rect.yMax-19,35,17);
                HudChrome.Fill(rankBadge,new Color(.015f,.021f,.035f,.84f));
                GUI.Label(rankBadge,view.rank+"/"+view.maxRank,
                    new GUIStyle(GUI.skin.label)
                    {
                        fontStyle=FontStyle.Bold,
                        alignment=TextAnchor.MiddleCenter,
                        fontSize=10,
                        normal={textColor=Color.white}
                    });
            }

            // Keep the clickable hit area independent from the art and from
            // the skill's cooldown mask.
            bool previous = GUI.enabled;
            GUI.enabled = interactable;
            // The upgrade hotspot must NOT overlap the regular action
            // button: IMGUI claims mouse-down for the first matching button.
            // Reserve the top-right 28x27 area for '+' when it is available.
            if(view.canUpgrade)
            {
                Rect actionBody=new(rect.x,rect.y,
                    Mathf.Max(0f,rect.width-28f),rect.height);
                Rect actionCorner=new(rect.xMax-28f,rect.y+27f,
                    28f,Mathf.Max(0f,rect.height-27f));
                if(GUI.Button(actionBody,new GUIContent("",view.tooltip),GUIStyle.none) ||
                   GUI.Button(actionCorner,new GUIContent("",view.tooltip),GUIStyle.none))
                    source.Activate(slot);
            }
            else if(GUI.Button(rect,new GUIContent("",view.tooltip),GUIStyle.none))
                source.Activate(slot);
            GUI.enabled = previous;

            if (slot==11 && source.HomeCasting)
            {
                HudChrome.Fill(rect,new Color(.12f,.27f,.46f,.40f));
                GUI.Label(rect,source.HomeCastRemaining.ToString("0.0")+"s",
                    new GUIStyle(GUI.skin.label)
                    {
                        alignment=TextAnchor.MiddleCenter,
                        fontStyle=FontStyle.Bold,
                        fontSize=14,
                        normal={textColor=Color.white}
                    });
            }
            if (view.cooldown > 0)
            {
                var old = GUI.color;
                GUI.color = new Color(0.04f, 0.04f, 0.04f, 0.72f);
                GUI.DrawTexture(rect, Texture2D.whiteTexture);
                GUI.color = old;
                GUI.Label(
                    rect,
                    view.cooldown.ToString("0.0"),
                    new GUIStyle(GUI.skin.label)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontStyle = FontStyle.Bold,
                        fontSize = Mathf.RoundToInt(Mathf.Clamp(rect.height * 0.25f, 11, 20))
                    });
            }
            // Passives always remain visible/readable, but are not clickable
            // actions. Their benefits still use the current source-of-truth
            // rank curves in the same tooltips as active skills.
            if (view.passive)
            {
                // A compact passive marker leaves the top-right '+' clear
                // even at the HUD's minimum scale and size.
                float badgeWidth=view.canUpgrade?Mathf.Min(27f,rect.width-31f):
                    Mathf.Min(46f,rect.width-4f);
                Rect passiveBadge=new(rect.x+2f,rect.y+2f,
                    Mathf.Max(12f,badgeWidth),17f);
                HudChrome.Fill(passiveBadge,new Color(.06f,.10f,.22f,.9f));
                GUI.Label(passiveBadge,view.canUpgrade?"PASS":"PASSIVE",
                    new GUIStyle(GUI.skin.label)
                    {
                        alignment=TextAnchor.MiddleCenter,
                        fontSize=8,fontStyle=FontStyle.Bold,
                        normal={textColor=new Color(.73f,.85f,1f)}
                    });
            }
            // This comes AFTER the main transparent slot button: IMGUI
            // processes controls in drawing order, so + gets its own hit area
            // and never fires the ability below when clicked.
            if(view.canUpgrade && !(HudLayoutManager.Current?.EditMode ?? false))
            {
                Rect plus=new(rect.xMax-27f,rect.y+2f,25f,23f);
                bool over=plus.Contains(tooltipPointer) ||
                    plus.Contains(Event.current.mousePosition);
                string details=source.UpgradeTooltip(slot);
                if(over)hoveredSkillTooltip=details;
                HudChrome.Fill(plus,over?new Color(.28f,.58f,.16f,.98f):
                    new Color(.17f,.43f,.13f,.98f));
                if(GUI.Button(plus,new GUIContent("+",details),
                    new GUIStyle(GUI.skin.button)
                    {
                        fontStyle=FontStyle.Bold,
                        fontSize=18,
                        alignment=TextAnchor.MiddleCenter,
                        normal={textColor=Color.white}
                    }))
                    source.Upgrade(slot);
            }
            // Tooltips work even when the button is on cooldown or unlearned.
            GUI.Label(rect, new GUIContent("", view.tooltip), GUIStyle.none);
        }

        private void DrawEmpty(Rect rect, int index)
        {
            bool hovered = rect.Contains(Event.current.mousePosition);
            HudChrome.DrawSlot(rect, Theme, false, hovered);
            // Future class/utility slots are deliberately left blank.
            // They cannot be mistaken for an equipped action.
        }
    }
}
