using DiceFree.Combat;
using DiceFree.Input;
using DiceFree.Quests;
using DiceFree.World;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DiceFree.UI
{
    public enum PointerRelation { Neutral, Enemy, Friendly, Interactable }

    [DisallowMultipleComponent, RequireComponent(typeof(CombatActor))]
    public sealed class WorldHoverFeedback : MonoBehaviour
    {
        private static readonly Color EnemyColor=new(.98f,.18f,.14f,1);
        private static readonly Color FriendlyColor=new(.18f,.95f,.43f,1);
        private static readonly Color InteractionColor=new(1,.75f,.25f,1);
        private readonly WorldTargetHighlight hover=new();
        private readonly WorldTargetHighlight selected=new();
        private readonly Texture2D[] cursors=new Texture2D[4];
        private CombatActor owner;
        private TargetSelection selection;
        private InspectSelection inspect;
        private SkillTargetingController targeting;
        private InputBindings bindings;
        private Vector2 pointer;
        private bool cursorOwned, focused=true;
        private PointerRelation applied=(PointerRelation)(-1);
        public Component CurrentTarget { get; private set; }
        public PointerRelation CurrentRelation { get; private set; }
        public Color CursorOutlineColor => ColorFor(CurrentRelation);
        // Software mode also supports deterministic player captures without OS cursor injection.
        public bool SoftwareCursor { get; set; }
        public Texture2D CursorTexture => cursors[(int)CurrentRelation];
        private void Awake()
        {
            owner=GetComponent<CombatActor>(); selection=GetComponent<TargetSelection>();
            inspect=GetComponent<InspectSelection>(); targeting=GetComponent<SkillTargetingController>();
            bindings=InputBindings.Current;
            for(int i=0;i<cursors.Length;i++) cursors[i]=CreateCursor(ColorFor((PointerRelation)i));
        }
        private void LateUpdate()
        {
            if(Mouse.current == null || !focused || !owner.Alive || bindings.Suppressed || GetComponent<WorldAtlasHud>()?.Open==true)
            { ClearFeedback(); return; }
            RefreshPointer(Mouse.current.position.ReadValue());
        }
        public void RefreshPointer(Vector2 screenPoint)
        {
            pointer=screenPoint;
            bool blocked=HudPointerBlocker.Covers(screenPoint) || screenPoint.x<0 || screenPoint.y<0 || screenPoint.x>=Screen.width || screenPoint.y>=Screen.height;
            var camera=Camera.main;
            Component target=null;
            if(!blocked)
            {
                if(targeting?.Active==true && targeting.Mode==SkillTargetingMode.Corpse) target=targeting.HoveredActor;
                else target=WorldTargetPicker.Pick(owner,camera,screenPoint).Target;
            }
            CurrentTarget=target; CurrentRelation=Relation(target);
            hover.Show(target,CursorOutlineColor,true,camera);
            Component chosen=selection?.Selected;
            if(chosen == null) chosen=inspect?.Npc;
            if(chosen == target || chosen is CombatActor actor && (!actor.Alive || (actor.Concealed && actor.Faction!=owner.Faction)) ||
               chosen != null && !WorldTargetPicker.IsVisibleTo(owner,chosen)) chosen=null;
            selected.Show(chosen,ColorFor(Relation(chosen))*.8f,false,camera);
            if(targeting?.Active==true) return; // The skill owns its reticle and cursor visibility.
            if(!cursorOwned || applied!=CurrentRelation)
            {
                Cursor.SetCursor(cursors[(int)CurrentRelation],new Vector2(4,4),CursorMode.Auto);
                applied=CurrentRelation; cursorOwned=true;
            }
            Cursor.visible=!SoftwareCursor;
        }
        public PointerRelation Relation(Component target)
        {
            if(target == null) return PointerRelation.Neutral;
            if(target is CombatActor actor) return actor.Faction==owner.Faction?PointerRelation.Friendly:PointerRelation.Enemy;
            if(target is InteractionTarget interaction &&
               (interaction is ContextualNpc || interaction is ConversationTarget || interaction is QuestGiver || interaction is HealerInteraction ||
                interaction.GetComponent<VillageVisualIdle>()!=null ||
                interaction.GetComponentInChildren<Animator>()!=null || interaction.GetComponentInChildren<SkinnedMeshRenderer>()!=null))
                return PointerRelation.Friendly;
            return PointerRelation.Interactable;
        }
        public static Color ColorFor(PointerRelation relation) => relation switch
        {
            PointerRelation.Enemy=>EnemyColor, PointerRelation.Friendly=>FriendlyColor,
            PointerRelation.Interactable=>InteractionColor, _=>new Color(.85f,.9f,.94f,1)
        };
        private void OnGUI()
        {
            if(!cursorOwned || targeting?.Active==true) return;
            var point=new Vector2(pointer.x,Screen.height-pointer.y);
            if(SoftwareCursor) GUI.DrawTexture(new Rect(point.x-4,point.y-4,40,48),CursorTexture);
        }
        private static Texture2D CreateCursor(Color border)
        {
            // Ivory arrow with a dark keyline and a broad coloured outer edge.
            var vertices=new Vector2[] {new(4,4),new(4,33),new(12,26),new(19,41),new(25,38),new(18,24),new(29,24)};
            bool Inside(Vector2 p)
            {
                bool inside=false;
                for(int i=0,j=vertices.Length-1;i<vertices.Length;j=i++)
                    if((vertices[i].y>p.y)!=(vertices[j].y>p.y) && p.x<(vertices[j].x-vertices[i].x)*(p.y-vertices[i].y)/(vertices[j].y-vertices[i].y)+vertices[i].x) inside=!inside;
                return inside;
            }
            float Edge(Vector2 p)
            {
                float distance=float.PositiveInfinity;
                for(int i=0;i<vertices.Length;i++)
                {
                    var a=vertices[i]; var b=vertices[(i+1)%vertices.Length]; var ab=b-a;
                    distance=Mathf.Min(distance,Vector2.Distance(p,a+ab*Mathf.Clamp01(Vector2.Dot(p-a,ab)/ab.sqrMagnitude)));
                }
                return distance;
            }
            var texture=new Texture2D(40,48,TextureFormat.RGBA32,false) {name="World pointer "+border,filterMode=FilterMode.Bilinear,hideFlags=HideFlags.DontSave};
            var pixels=new Color[40*48];
            for(int y=0;y<48;y++) for(int x=0;x<40;x++)
            {
                Color sum=Color.clear;
                for(int sy=0;sy<2;sy++) for(int sx=0;sx<2;sx++)
                {
                    var p=new Vector2(x+(sx+.5f)/2,y+(sy+.5f)/2); float edge=Edge(p); bool inside=Inside(p);
                    Color sample=inside?(edge>1.7f?new Color(.97f,.97f,.9f,1):new Color(.05f,.065f,.08f,1)):
                        edge<2?border:edge<2.8f?new Color(.025f,.03f,.035f,1):Color.clear;
                    sum+=sample*.25f;
                }
                // Cursor hotspots use top-left origin; Texture2D pixels use bottom-left.
                pixels[(47-y)*40+x]=sum;
            }
            texture.SetPixels(pixels); texture.Apply(false,false); return texture;
        }
        private void ClearFeedback()
        {
            CurrentTarget=null; CurrentRelation=PointerRelation.Neutral;
            hover.Clear(); selected.Clear();
            if(cursorOwned) { Cursor.SetCursor(null,Vector2.zero,CursorMode.Auto); cursorOwned=false; applied=(PointerRelation)(-1); }
            if(targeting?.Active!=true) Cursor.visible=true;
        }
        private void OnApplicationFocus(bool value) { focused=value; if(!value) ClearFeedback(); }
        private void OnDisable()=>ClearFeedback();
        private void OnDestroy()
        {
            ClearFeedback(); foreach(var texture in cursors) if(texture!=null) Destroy(texture);
        }
    }
}
