using DiceFree.Combat;
using DiceFree.World;
using UnityEngine;

namespace DiceFree.UI
{
    /// <summary>
    /// Inspect the last left-clicked world target in a Warcraft-style lower
    /// left window, with a true 3D preview rendered by a dedicated camera.
    /// NPCs have names/roles; combat units expose real stats and HP.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TargetInspectionHud : CustomizableHudWidget
    {
        private InspectSelection selected;
        private Transform stage;
        private Camera stageCamera;
        private RenderTexture texture;
        private GameObject preview;
        private Transform lastTarget;
        private const int StudioLayer=31;
        public override string LayoutId=>"target-inspection";
        public override string DisplayName=>"Target Inspector";
        public override Rect DefaultNormalizedBounds=>
            new Rect(.006f,.434f,.255f,.268f);
        public override Vector2 MinimumPixelSize=>new(305,173);
        public override bool BlocksPointer =>
            (HudLayoutManager.Current?.EditMode??false) ||
            selected!=null && selected.SelectedTransform!=null;
        private void Awake()
        {
            texture=new RenderTexture(320,320,16,RenderTextureFormat.ARGB32)
            {
                name="DiceFree selected unit 3D portrait",
                antiAliasing=2
            };
            texture.Create();
            stage=new GameObject("Target inspection 3D studio").transform;
            stage.SetParent(transform,false);
            stage.localPosition=new Vector3(0,-2200f,0);
            var camObj=new GameObject("Target inspection portrait camera");
            camObj.transform.SetParent(stage,false);
            stageCamera=camObj.AddComponent<Camera>();
            stageCamera.cullingMask=1<<StudioLayer;
            stageCamera.clearFlags=CameraClearFlags.SolidColor;
            stageCamera.backgroundColor=new Color(.025f,.029f,.037f,1f);
            stageCamera.fieldOfView=30f;
            stageCamera.nearClipPlane=.05f;
            stageCamera.farClipPlane=40f;
            stageCamera.targetTexture=texture;
            stageCamera.enabled=true;
            var lightObj=new GameObject("Target inspection light");
            lightObj.transform.SetParent(stage,false);
            lightObj.transform.localEulerAngles=new Vector3(34,130,0);
            var light=lightObj.AddComponent<Light>();
            light.type=LightType.Directional;
            light.intensity=1.6f;
            light.cullingMask=1<<StudioLayer;
        }
        private void OnDestroy()
        {
            if(texture!=null){texture.Release();Destroy(texture);}
            if(stage!=null)Destroy(stage.gameObject);
        }
        private void Acquire()
        {
            if(selected==null)selected=FindAnyObjectByType<InspectSelection>();
        }
        private static void SetLayer(Transform node)
        {
            node.gameObject.layer=StudioLayer;
            foreach(Transform child in node)SetLayer(child);
        }
        private Transform Visual(Transform t)
        {
            if(t==null)return null;
            var actor=t.GetComponent<CombatActor>();
            if(actor!=null)
            {
                foreach(var component in actor.GetComponents<MonoBehaviour>())
                    if(component is IInspectableVisual source && source.DisplayModel!=null)
                        return source.DisplayModel;
                var feedback=actor.GetComponent<ActorFeedback>();
                if(feedback!=null && feedback.Visual!=null)return feedback.Visual;
                foreach(Transform child in t)
                    if(child.GetComponentInChildren<Renderer>(true)!=null)
                        return child;
            }
            return t;
        }
        private void Rebuild(Transform target)
        {
            if(preview!=null){Destroy(preview);preview=null;}
            lastTarget=target;
            if(target==null)return;
            var original=Visual(target);
            if(original==null)return;
            preview=Instantiate(original.gameObject,stage);
            preview.name="Selected 3D unit preview";
            preview.transform.localPosition=Vector3.zero;
            preview.transform.localRotation=Quaternion.identity;
            preview.transform.localScale=original.localScale;
            SetLayer(preview.transform);
            foreach(var collider in preview.GetComponentsInChildren<Collider>(true))
                collider.enabled=false;
            foreach(var component in preview.GetComponentsInChildren<MonoBehaviour>(true))
                component.enabled=false;
            var anim=preview.GetComponentInChildren<Animator>(true);
            if(anim!=null)
            {
                anim.enabled=true;
                anim.applyRootMotion=false;
                anim.cullingMode=AnimatorCullingMode.AlwaysAnimate;
            }
            UnityEngine.Bounds bounds=new UnityEngine.Bounds(stage.position+Vector3.up,Vector3.one);
            bool found=false;
            foreach(var renderer in preview.GetComponentsInChildren<Renderer>(true))
            {
                if(!renderer.enabled)continue;
                if(!found){bounds=renderer.bounds;found=true;}
                else bounds.Encapsulate(renderer.bounds);
            }
            Vector3 center=found?bounds.center:stage.position+Vector3.up;
            float radius=found?bounds.extents.magnitude:1.1f;
            float distance=Mathf.Clamp(radius*3.2f,2.6f,14f);
            stageCamera.transform.position=center+new Vector3(0,radius*.12f,distance);
            stageCamera.transform.LookAt(center);
        }
        private void Update()
        {
            Acquire();
            var target=selected?.SelectedTransform;
            if(target!=lastTarget)Rebuild(target);
        }
        private void OnGUI()
        {
            if(HudPointerBlocker.ModalOpen)return;
            Acquire();
            var unit=selected?.SelectedTransform;
            bool editing=HudLayoutManager.Current?.EditMode??false;
            if(unit==null && !editing)return;
            Rect panel=Bounds;
            DrawPanel(panel);
            Rect inner=Inner(panel);
            var actor=selected?.Actor;
            var npc=selected?.Npc;
            string title=actor!=null && actor.Stats.Definition!=null?
                actor.DisplayName+"  Lv "+actor.Stats.Level:
                npc!=null && !string.IsNullOrWhiteSpace(npc.DisplayName)?
                    npc.DisplayName:
                    unit!=null?unit.name:"Select a creature or NPC";
            GUI.Label(new Rect(inner.x+3,inner.y,inner.width-6,23),title,
                new GUIStyle(GUI.skin.label)
                {
                    fontStyle=FontStyle.Bold,fontSize=13,
                    normal={textColor=Theme.textTint}
                });
            Rect body=new(inner.x,inner.y+24,inner.width,
                Mathf.Max(12,inner.height-24));
            Rect renderRect=new(body.x,body.y,body.width*.43f,body.height);
            HudChrome.DrawSlot(renderRect,Theme,true,false);
            if(preview!=null && texture!=null)
                GUI.DrawTexture(new Rect(renderRect.x+2,renderRect.y+2,
                    renderRect.width-4,renderRect.height-4),
                    texture,ScaleMode.ScaleToFit);
            Rect stats=new(body.x+body.width*.46f,body.y+1,
                body.width*.54f-2,body.height-2);
            if(actor!=null)
            {
                var st=actor.Stats;
                string damage="No attack";
                var attack=actor.GetComponent<BasicAttack>()?.Definition;
                if(attack!=null)
                {
                    Vector2 values=actor.GetComponent<BasicAttack>().DamageRange;
                    damage=$"{values.x:0} - {values.y:0}";
                }
                GUI.Label(stats,"HP: "+actor.Health.Current.ToString("0")+
                    " / "+actor.Health.Maximum.ToString("0")+
                    "\nAttack: "+damage+
                    "\nP-DEF: "+st.Defense(DamageChannel.Physical).ToString("0.#")+
                    "\nM-DEF: "+st.Defense(DamageChannel.Magical).ToString("0.#")+
                    "\nSpeed: "+st.MoveSpeed.ToString("0.#"),
                    new GUIStyle(GUI.skin.label)
                    {
                        normal={textColor=Theme.textTint},
                        fontSize=11,wordWrap=true
                    });
            }
            else
                GUI.Label(stats,npc!=null?
                    "NPC / Interactable\nLeft-click to inspect\nRight-click to interact":
                    "Choose a target",
                    new GUIStyle(GUI.skin.label)
                    {
                        normal={textColor=Theme.textTint},
                        wordWrap=true,fontSize=11
                    });
        }
    }
}
