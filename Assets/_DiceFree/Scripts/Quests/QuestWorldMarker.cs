using DiceFree.Combat;
using UnityEngine;

namespace DiceFree.Quests
{
    /// <summary>
    /// Animated world-space, physically modelled punctuation above NPCs:
    /// gold ! for ordinary quests, blue ! for repeatable quests, green ? for turn-ins.
    /// No UI screen-space text and no hit-test/collision.
    /// </summary>
    [DisallowMultipleComponent,RequireComponent(typeof(QuestGiver))]
    public sealed class QuestWorldMarker : MonoBehaviour
    {
        private QuestGiver giver;
        private QuestJournal journal;
        private Transform pivot;
        private GameObject exclamation,repeatExclamation,question;
        private float baseHeight=2.9f;
        private static Material gold,blue,green;
        public bool IsExclamation => exclamation!=null && exclamation.activeSelf;
        public bool IsRepeatExclamation => repeatExclamation!=null && repeatExclamation.activeSelf;
        public static bool IsRepeatAvailable(QuestDefinition quest,bool canAccept) =>
            quest!=null && quest.repeatable && canAccept;
        public bool IsQuestion => question!=null && question.activeSelf;

        private void Awake()
        {
            giver=GetComponent<QuestGiver>();
        }
        private static Material MaterialFor(Color color,string name)
        {
            var shader=Shader.Find("Universal Render Pipeline/Lit") ??
                Shader.Find("Standard");
            var mat=new Material(shader){name=name};
            if(mat.HasProperty("_BaseColor"))mat.SetColor("_BaseColor",color);
            mat.color=color;
            return mat;
        }
        private static Transform Shape(PrimitiveType primitive,Transform parent,
            string name,Vector3 point,Vector3 scale,Material art)
        {
            var go=GameObject.CreatePrimitive(primitive);
            go.name=name;
            go.transform.SetParent(parent,false);
            go.transform.localPosition=point;
            go.transform.localScale=scale;
            foreach(var col in go.GetComponents<Collider>())Destroy(col);
            go.GetComponent<Renderer>().sharedMaterial=art;
            go.layer=2; // Ignore Raycast
            return go.transform;
        }
        private static void Stroke(Transform root,Material art,Vector3 a,Vector3 b,
            float thickness)
        {
            var line=Shape(PrimitiveType.Cylinder,root,"3D glyph stroke",
                (a+b)*.5f,new Vector3(thickness,(b-a).magnitude*.5f,thickness),art);
            line.localRotation=Quaternion.FromToRotation(Vector3.up,b-a);
        }
        private void Start()
        {
            if(gold==null)gold=MaterialFor(new Color(1f,.75f,.08f),
                "Quest available golden 3D marker");
            if(green==null)green=MaterialFor(new Color(.18f,1f,.28f),
                "Quest ready green 3D marker");
            if(blue==null)blue=MaterialFor(new Color(.13f,.54f,1f),
                "Repeatable quest available blue 3D marker");
            foreach(var render in GetComponentsInChildren<Renderer>())
                baseHeight=Mathf.Max(baseHeight,
                    render.bounds.max.y-transform.position.y+.65f);
            pivot=new GameObject("Floating 3D quest marker").transform;
            pivot.SetParent(transform,false);
            pivot.gameObject.layer=2;
            var bang=new GameObject("Available quest 3D !");
            bang.transform.SetParent(pivot,false);
            exclamation=bang;
            Stroke(bang.transform,gold,new Vector3(0,.03f,0),
                new Vector3(0,.60f,0),.09f);
            Shape(PrimitiveType.Sphere,bang.transform,"! dot",
                new Vector3(0,-.20f,0),Vector3.one*.18f,gold);
            repeatExclamation=Instantiate(bang,pivot,false);
            repeatExclamation.name="Repeatable quest 3D blue !";
            foreach(var r in repeatExclamation.GetComponentsInChildren<Renderer>())
                r.sharedMaterial=blue;

            var query=new GameObject("Ready quest 3D ?");
            query.transform.SetParent(pivot,false);
            question=query;
            var points=new[]{
                new Vector3(-.30f,.43f,0),new Vector3(-.20f,.58f,0),
                new Vector3(0,.64f,0),new Vector3(.22f,.54f,0),
                new Vector3(.32f,.38f,0),new Vector3(.24f,.20f,0),
                new Vector3(0,.08f,0),new Vector3(0,-.01f,0)};
            for(int i=1;i<points.Length;i++)
                Stroke(query.transform,green,points[i-1],points[i],.078f);
            Shape(PrimitiveType.Sphere,query.transform,"? dot",
                new Vector3(0,-.24f,0),Vector3.one*.18f,green);
            exclamation.SetActive(false);
            repeatExclamation.SetActive(false);
            question.SetActive(false);
        }
        private void LateUpdate()
        {
            if(pivot==null)return;
            if(journal==null)journal=FindFirstObjectByType<QuestJournal>();
            bool ready=false,available=false,repeatAvailable=false;
            if(journal!=null)
                foreach(var quest in giver.Offers)
                {
                    if(quest==null)continue;
                    var state=journal.GetProgress(quest.stableId);
                    if(state?.status==QuestStatus.ReadyToTurnIn)ready=true;
                    if(journal.CanAccept(quest))
                    {
                        if(IsRepeatAvailable(quest,true))repeatAvailable=true;
                        else available=true;
                    }
                }
            pivot.gameObject.SetActive(ready || available || repeatAvailable);
            if(!ready && !available && !repeatAvailable)return;
            // Turn-ins take precedence, then new story quests, then repeatables.
            question.SetActive(ready);
            exclamation.SetActive(!ready && available);
            repeatExclamation.SetActive(!ready && !available && repeatAvailable);
            pivot.localPosition=Vector3.up*(baseHeight+
                .13f*Mathf.Sin(Time.time*2.5f));
            if(Camera.main!=null)
            {
                Vector3 toward=Camera.main.transform.position-pivot.position;
                toward.y=0;
                if(toward.sqrMagnitude>.01f)
                    pivot.rotation=Quaternion.LookRotation(toward.normalized);
            }
            pivot.Rotate(Vector3.up,10f*Mathf.Sin(Time.time*2f),Space.Self);
        }
    }
}
